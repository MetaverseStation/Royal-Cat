using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.Rendering.Universal;

public class MonsterAttack : MonoBehaviourPun
{
    public float attackDamage = 10f;
    public Vector3 monsterSpawnPoint = new Vector3(-3, 1, 2);
    public float attackDistance = 7f;

    private GameObject[] _playerList;
    private List<GameObject> _targetList;
    private GameObject _targetPlayer;

    private NavMeshAgent _navMeshAgent;
    private Animator _monsterAnimator;
    private MonsterController _monsterController;

    private NavMeshHit _hit;

    private bool _canAttack = true;
    public float attackCooldown = 3.0f;

    public GameObject rangeAttackUI;

    [SerializeField] private GameObject _stonePrefab; // 돌 프리팹
    [SerializeField] private GameObject[] _dropLocations; // 여러 개의 떨어뜨릴 위치
    [SerializeField] private float _dropHeight = 8.0f; // 하늘에서 떨어지는 높이

    private void Start()
    {
        _targetList = new List<GameObject>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _monsterAnimator = GetComponent<Animator>();
        _monsterController = GetComponent<MonsterController>();

        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("FindPlayer", 3f);
        }
    }

    private void FindPlayer()
    {
        _playerList = GameObject.FindGameObjectsWithTag("Player");
        StartCoroutine(AttackLoop());
    }

    // 공격 루프를 관리하는 코루틴
    private IEnumerator AttackLoop()
    {
        while (!_monsterController.dead)
        {
            // 1. 타겟을 찾는다
            FindTarget();


            if (_targetPlayer != null)
            {
                Debug.Log("타겟 위치 : " + _targetPlayer.transform.position);

                // 2. 타겟을 쫓아간다
                yield return StartCoroutine(ChaseTarget());

                // 3. 타겟에게 공격한다
                if (_canAttack)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PerformAttack();
                        yield return StartCoroutine(AttackCooldown());  // 공격 후 쿨타임
                    }
                }

            }
            else
            {
                //Debug.Log("타겟 사망");
                _navMeshAgent.isStopped = false;
                _navMeshAgent.SetDestination(monsterSpawnPoint);
                _monsterAnimator.SetBool("hasTarget", true);
            }

            if (_navMeshAgent.velocity.sqrMagnitude == 0 && _monsterAnimator.GetBool("hasTarget"))
            {
                _monsterAnimator.SetBool("hasTarget", false);
            }

            yield return new WaitForSeconds(1f);  // 반복 루프 대기 시간
        }
    }

    private void FindTarget()
    {
        if (_playerList == null)
        {
            return;
        }

        _targetList.Clear();
        _targetPlayer = null;

        foreach (GameObject player in _playerList)
        {
            if (player)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

                if (playerHealth != null && !playerHealth.dead)
                {
                    if (NavMesh.SamplePosition(player.transform.position, out _hit, 1f, NavMesh.AllAreas))
                    {
                        _targetList.Add(player);
                    }
                }
            }
        }

        if (_targetList.Count > 0)
        {
            _targetPlayer = _targetList[Random.Range(0, _targetList.Count)];
        }
        else
        {
            // 타겟이 없으면 스폰 포인트로 돌아감
            _navMeshAgent.SetDestination(monsterSpawnPoint);
            _monsterAnimator.SetBool("hasTarget", true);
        }
    }

    private IEnumerator ChaseTarget()
    {
        if (_targetPlayer != null)
        {

            _navMeshAgent.isStopped = false;
            _monsterAnimator.SetBool("hasTarget", true);

            // 타겟에게 도달할 때까지 이동
            while (Vector3.Distance(transform.position, _targetPlayer.transform.position) > 3f)
            {
                if (!NavMesh.SamplePosition(_targetPlayer.transform.position, out _hit, 1f, NavMesh.AllAreas)) {
                    break;
                }

                _navMeshAgent.SetDestination(_targetPlayer.transform.position);
                // 몬스터의 위치를 다른 클라이언트에 동기화
                photonView.RPC("SyncMonsterPosition", RpcTarget.Others, _navMeshAgent.destination);
                photonView.RPC("SyncMonsterAnimation", RpcTarget.Others, true);
                yield return null;  // 다음 프레임까지 대기
            }

            // 타겟에 도달하면 멈춘다
            _navMeshAgent.isStopped = true;
            _monsterAnimator.SetBool("hasTarget", false);
            transform.LookAt(new Vector3(_targetPlayer.transform.position.x, transform.position.y, _targetPlayer.transform.position.z));
        }
        // else
        // {
        //     // 타겟이 없으면 스폰 포인트로 돌아감
        //     _monsterAnimator.SetBool("hasTarget", true);
        //     _navMeshAgent.SetDestination(monsterSpawnPoint);
        // }
    }

    private void PerformAttack()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int randomInt = Random.Range(1, 4);
            photonView.RPC("SyncAttackType", RpcTarget.All, randomInt);
        }
    }

    [PunRPC]
    private void SyncAttackType(int attackType)
    {
        switch (attackType)
        {
            case 1:
                CloseAttack();
                break;
            case 2:
                RangeAttack();
                break;
            case 3:
                StoneAttack();
                break;
        }
    }


    private void CloseAttack()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerHealth playerHealth = _targetPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.MonsterCloseAttack);
                _monsterAnimator.SetTrigger("CloseAttack");
                photonView.RPC("SyncCloseAttack", RpcTarget.Others, _targetPlayer.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    [PunRPC]
    private void SyncCloseAttack(int targetViewID)
    {
        GameObject target = PhotonView.Find(targetViewID).gameObject;
        _monsterAnimator.SetTrigger("CloseAttack");
    }


    private void RangeAttack()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.MonsterRangeShot);
            StartCoroutine(DisplayAttackRange());
            photonView.RPC("SyncRangeAttack", RpcTarget.Others);
        }
    }

    [PunRPC]
    private void SyncRangeAttack()
    {
        StartCoroutine(DisplayAttackRange());
    }


    private IEnumerator DisplayAttackRange()
    {
        if (_targetPlayer != null)
        {
            float runningTime = 0.5f;
            photonView.RPC("SyncRangeAttackUI", RpcTarget.All, true); // 모든 클라이언트에게 범위공격 UI 표시
            rangeAttackUI.transform.localScale = new Vector3(0f, 0f, 0f);

            while (runningTime > 0.0f)
            {
                runningTime -= Time.deltaTime;
                Vector3 newScale = rangeAttackUI.transform.localScale += new Vector3(10f * Time.deltaTime, 10f * Time.deltaTime, 10f * Time.deltaTime);
                rangeAttackUI.transform.localScale = newScale;
                photonView.RPC("SyncRangeAttackScale", RpcTarget.Others, newScale);
                yield return null;
            }

            float dotValue = Mathf.Cos(Mathf.Deg2Rad * 15f);
            Vector3 direction = _targetPlayer.transform.position - transform.position;
            if (direction.magnitude < 5f && Vector3.Dot(direction.normalized, transform.forward) > dotValue)
            {
                _monsterAnimator.SetTrigger("RangeAttack");
                yield return new WaitForSeconds(1f);
                _targetPlayer.GetComponent<PlayerHealth>().OnDamage(attackDamage);
                photonView.RPC("AttackSync", RpcTarget.All, _targetPlayer.GetComponent<PlayerHealth>().photonView.ViewID, attackDamage);
            }
            else
            {
                _monsterAnimator.SetTrigger("RangeAttack");
                yield return new WaitForSeconds(1f);
            }

            photonView.RPC("SyncRangeAttackUI", RpcTarget.All, false);
        }
    }

    [PunRPC]
    private void SyncRangeAttackUI(bool isActive)
    {
        rangeAttackUI.SetActive(isActive);
        if (isActive)
        {
            rangeAttackUI.transform.localScale = new Vector3(0f, 0f, 0f);
        }
    }

    private void StoneAttack()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("StoneAttack");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.MonsterStoneShot);
            photonView.RPC("SyncStoneAttack", RpcTarget.All);
        }
    }

    [PunRPC]
    private void SyncStoneAttack()
    {
        foreach (GameObject location in _dropLocations)
        {
            // 돌이 떨어지는 위치 미리 표시
            StartCoroutine(ShowStoneDrop(location));

            // 각 위치에서 dropHeight만큼 위에서 돌을 생성
            Vector3 dropPosition = new Vector3(location.transform.position.x, location.transform.position.y + _dropHeight, location.transform.position.z);
            GameObject stone = Instantiate(_stonePrefab, dropPosition, Quaternion.identity);
        }
    }


    private IEnumerator ShowStoneDrop(GameObject location)
    {
        location.SetActive(true);
        location.transform.localScale = new Vector3(2f, 2f, 2f);

        float runningTime = 1.2f;

        while (runningTime > 0.0f)
        {
            runningTime -= Time.deltaTime;

            // 매 프레임마다 사이즈 줄이기
            location.transform.localScale -= new Vector3(0.5f * Time.deltaTime, 0.5f * Time.deltaTime, 0.5f * Time.deltaTime);

            // 한 프레임 대기
            yield return null;
        }

        location.SetActive(false);
    }

    private IEnumerator AttackCooldown()
    {
        _canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    [PunRPC]
    private void SyncMonsterPosition(Vector3 position)
    {
        _navMeshAgent.SetDestination(position);
    }

    [PunRPC]
    private void SyncMonsterAnimation(bool hasTarget)
    {
        _monsterAnimator.SetBool("hasTarget", hasTarget);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            // 충돌한 플레이어에 대한 정보를 마스터 클라이언트로 전달
            PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();
            if (playerHealth != null && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("AttackSync", RpcTarget.All, playerHealth.photonView.ViewID, attackDamage);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            if (playerHealth != null && PhotonNetwork.IsMasterClient)
            {
                // 플레이어에게 데미지 입힘
                photonView.RPC("AttackSync", RpcTarget.All, playerHealth.photonView.ViewID, attackDamage);
            }
        }
    }

    [PunRPC]
    private void AttackSync(int targetPlayerID, float damage)
    {
        PhotonView targetPhotonView = PhotonView.Find(targetPlayerID);
        if (targetPhotonView != null)
        {
            PlayerHealth playerHealth = targetPhotonView.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDamage(damage);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 마스터 클라이언트는 몬스터의 위치를 전송
            stream.SendNext(_navMeshAgent.transform.position);
        }
        else
        {
            // 클라이언트는 마스터로부터 위치를 수신하여 동기화
            Vector3 syncedPosition = (Vector3)stream.ReceiveNext();
            _navMeshAgent.Warp(syncedPosition);
        }
    }
}