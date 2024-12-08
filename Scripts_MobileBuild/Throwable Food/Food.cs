using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Threading;
using System.Diagnostics.Contracts;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Food : MonoBehaviourPunCallbacks, IPunObservable 
{
    [Header("Effect")]
    private GameObject throwableFoodCollisionEffect;
    public GameObject straightFoodCollisionEffect;
    public GameObject parabolicFoodCollisionEffect;
    private Rigidbody _foodRigidbody;


    //private float _minSpeed = 5f;
    //private float _maxSpeed = 5f;
    private float _minSpeed = 15f;
    private float _maxSpeed = 40f;
    private float _damage;
            
    private float _maxDistance;
    private float _movedDistance;

    public PlayerHand playerHand { get; set; }

    private Vector3 _originalScale;

    //생명주기
    private float _destroyTime = 2f;
    private float _curTime = 0;

    // 포물선 발사 관련 변수
    [Header("Parabolic Skill")]
    public float parabolicExlplsionRange = 2f;
    private float _firingAngle;
    private float _gravity;
    private Vector3 _desPos;
    private Vector3 _throwPos;

    private ConstantForce _constantForce;

    private bool isParabolicAttack;    

    //플레이어 콜라이더 리스트
    private Dictionary<int, Collider> _prevPlayerCollider;

    private void Awake()
    {
        _prevPlayerCollider = new Dictionary<int, Collider>();
        _constantForce = GetComponent<ConstantForce>();
        _foodRigidbody = GetComponent<Rigidbody>();
        _originalScale = transform.localScale;
    }

    private void Update()
    {
        DestroyOverDistance();
        DestroyAfterTime();
    }



    [PunRPC]
    public void Initialize(int viewID, Quaternion rotation, float damage, float distance)
    {
        _curTime = 0;

        GameObject playerView = PhotonView.Find(viewID).gameObject;
        PlayerHand playerHand = playerView.GetComponent<PlayerHand>();

        //크기 초기화
        transform.localScale = _originalScale;

        //사거리 초기화
        _throwPos = playerHand.throwPosition.position;
        _maxDistance = distance;
        _movedDistance = 0f;

        //리지드바디 초기화
        _foodRigidbody = GetComponent<Rigidbody>();
        _foodRigidbody.velocity = Vector3.zero;
        _foodRigidbody.angularVelocity = Vector3.zero;
        _foodRigidbody.ResetInertiaTensor();

        isParabolicAttack = playerHand.isParabolicAttack;
        //Debug.Log("food : " + isParabolicAttack);
        _firingAngle = playerHand.firingAngle;
        _gravity = playerHand.gravity;
        _damage = damage;

        // 차징 및 발사 관련
        float chargePercent = Mathf.Clamp01(playerHand.chargeTime / playerHand.maxChargeTime);
        float foodSpeed = Mathf.Lerp(_minSpeed, _maxSpeed, chargePercent);

        if (isParabolicAttack)
        {
            throwableFoodCollisionEffect = parabolicFoodCollisionEffect;
            _constantForce.enabled = true;
            // 포물선 공격 시
            _desPos = playerHand.desPosition;
            transform.localScale *= 1.3f;
            LaunchProjectile();
            
        }
        else
        {
             throwableFoodCollisionEffect = straightFoodCollisionEffect;
            _constantForce.enabled = false;
            _foodRigidbody.useGravity = false;
            _foodRigidbody.velocity = rotation * Vector3.forward * foodSpeed;
        }

        // 투사체를 던진 플레이어의 콜라이더와 충돌을 무시
        Collider c = playerHand.GetComponent<Collider>();

        if(!_prevPlayerCollider.ContainsKey(viewID))
        {
            _prevPlayerCollider.Add(viewID, c);
        }
        IgnoreCollision(c, GetComponent<Collider>());

        ResetChargeSlider(playerHand);

    }

    private void OnCollisionEnter(Collision collision)
    {
        // 충돌 감지
        ContactPoint contact = collision.contacts[0];
        if (photonView.IsMine)
        {
            //Debug.Log("투사체 ID: " + photonView.ViewID);
            //Debug.Log("투사체를 던진 플레이어 ID: " + photonView.OwnerActorNr);

            HandleCollision(contact.point, contact.normal, photonView.ViewID);
            

            //투사체 제거            
            photonView.RPC("DestroyFoodRPC", RpcTarget.All);
        }
        GameObject collisionEffect = Instantiate(throwableFoodCollisionEffect, contact.point, Quaternion.LookRotation(contact.normal));
        Destroy(collisionEffect, 1f);
    }

    void PlayerHit(GameObject hitPlayer)
    {
        //DebugManager.Inst.SetInfoText("Player Hit");
    }

    void MonsterHit(GameObject hitMonster)
    {
        //DebugManager.Inst.SetInfoText("몬스터 맞음");
    }

    private void HandleCollision(Vector3 collisionPoint, Vector3 collisionNormal, int foodId)
    {
        //GameObject collisionEffect = Instantiate(throwableFoodCollisionEffect, collisionPoint, Quaternion.LookRotation(collisionNormal));
        //Destroy(collisionEffect, 1f);

        Collider[] hitColliders;
        if (isParabolicAttack)
        {
            hitColliders = Physics.OverlapSphere(collisionPoint, parabolicExlplsionRange);
        }
        else
        {
            hitColliders = Physics.OverlapSphere(collisionPoint, 0.1f);
        }

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Player"))
            {
                Debug.Log("닿는다");
                PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
                Debug.Log("플레이어 ID: " + playerHealth.pv.ViewID);
                if (playerHealth.pv.ViewID / 1000 == foodId / 1000)
                {
                    continue;
                }
                if (playerHealth != null)
                {
                    int damageId = (foodId / 1000) * 1000 + 1;
                    photonView.RPC("UpdateRecentDamage", RpcTarget.All, playerHealth.pv.ViewID, damageId);
                    photonView.RPC("RequestDamage", RpcTarget.MasterClient, playerHealth.pv.ViewID, _damage);
                }
            }
            else if (collider.CompareTag("Monster"))
            {
                MonsterController monster = collider.GetComponent<MonsterController>();
                Debug.Log("몬스터 ID: " + monster.photonView.ViewID);
                if (monster != null)
                {
                    photonView.RPC("MonsterDamage", RpcTarget.MasterClient, monster.photonView.ViewID, _damage);
                }
            }
        }
    }


    [PunRPC]
    private void UpdateRecentDamage(int playerViewID, int damageId)
    {
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.recentDamage = damageId;
            }
        }
    }

    [PunRPC]
    private void MonsterDamage(int monsterID, float damage)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView targetView = PhotonView.Find(monsterID);
            if (targetView != null)
            {
                MonsterController monster = targetView.GetComponent<MonsterController>();
                if (monster != null)
                {
                    monster.OnDamage(damage);
                    photonView.RPC("SyncMonsterHealth", RpcTarget.All, monster.photonView.ViewID, monster.health);
                }
            }
        }
    }

    [PunRPC]
    private void SyncMonsterHealth(int monsterID, float updatedHealth)
    {
        PhotonView targetView = PhotonView.Find(monsterID);
        if (targetView != null)
        {
            MonsterController monster = targetView.GetComponent<MonsterController>();
            if (monster != null)
            {
                // 다른 클라이언트들이 몬스터 체력 업데이트
                monster.health = updatedHealth;
                monster.healthSlider.value = updatedHealth;
                monster.healthText.text = $"{updatedHealth} / {monster._maxHealth}";
            }
        }
    }

    [PunRPC]
    private void RequestDamage(int playerViewID, float damage)
    {
        // 마스터 클라이언트가 플레이어 피해 처리
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView targetView = PhotonView.Find(playerViewID);
            if (targetView != null)
            {
                PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.OnDamage(damage);
                    Debug.Log("동기화 할거임. 지금 ID는 " + playerViewID);
                    photonView.RPC("SyncDamage", RpcTarget.All, playerViewID, playerHealth.health);

                    
                }
            }
        }
    }

    [PunRPC]
    private void SyncDamage(int playerViewID, float updatedHealth)
    {
        Debug.Log("SyncDamage 들어옴. 동기화할 ID는 " + playerViewID + ", 바뀔 HP값은 " + updatedHealth);
        // 모든 클라이언트에서 HP를 동기화
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.health = updatedHealth;
                playerHealth.healthUIUpdate(updatedHealth);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_foodRigidbody.velocity);
        }
        else
        {
            _foodRigidbody.velocity = (Vector3)stream.ReceiveNext();
        }
    }

    //public IEnumerator SimulateProjectile()
    //{
    //    // 시작점과 목표점 사이의 거리 계산
    //    float target_Distance = Vector3.Distance(_throwPos, _desPos);

    //    // 초기 속도 계산
    //    float projectile_Velocity = target_Distance / (Mathf.Sin(2 * _firingAngle * Mathf.Deg2Rad) / _gravity);

    //    // XZ 평면에서의 속도 계산
    //    float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(_firingAngle * Mathf.Deg2Rad);
    //    float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(_firingAngle * Mathf.Deg2Rad);

    //    // 비행 시간 계산
    //    float flightDuration = target_Distance / Vx;

    //    // 발사 방향 설정
    //    transform.rotation = Quaternion.LookRotation(_desPos - _throwPos);

    //    // 비행 시간 동안 이동
    //    float elapse_time = 0;
    //    while (elapse_time < flightDuration)
    //    {
    //        // 오브젝트를 이동
    //        transform.Translate(0, (Vy - (_gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
    //        // Debug.Log("transform : " + transform.position);

    //        // 시간 갱신
    //        elapse_time += Time.deltaTime;

    //        // 오브젝트가 목표점에 도달했는지 확인
    //        if (Vector3.Distance(transform.position, _desPos) < 0.1f) // 목표점에 근접하면
    //        {
    //            // 목표 지점에 도달했으므로 코루틴 종료
    //            break;
    //        }

    //        yield return null;
    //    }
    //}

    //private IEnumerator DestroyAfterTime(float time)
    //{
    //    yield return new WaitForSeconds(time);
    //    //if (_pv.IsMine)
    //    {
    //        if (gameObject.activeSelf)
    //        {
    //            GameManager.Inst.DestroyObj(gameObject);
    //        }            
    //    }
    //}

    private void DestroyAfterTime()
    {
        _curTime += Time.deltaTime;
        if(_curTime >= _destroyTime)
        {
            if (gameObject.activeSelf)
            {
                DestroyFood();
                //_pv.RPC("DestroyObj", RpcTarget.All);
            }
        }
    }
    //일정 사거리가 되면 제거
    private void DestroyOverDistance()
    {
        _movedDistance = Vector3.Distance(_throwPos, transform.position);
        if (_movedDistance >= _maxDistance)
        {
            GameObject collisionEffect = Instantiate(throwableFoodCollisionEffect, transform.position, Quaternion.identity);
            Destroy(collisionEffect, 1f);
            DestroyFood();
        }
    }

    private void DestroyFood()
    {
        gameObject.SetActive(false);
        GameManager.Inst.DestroyObj(gameObject);
    }

    [PunRPC]
    private void DestroyFoodRPC()
    {
        gameObject.SetActive(false);
        GameManager.Inst.DestroyObj(gameObject);
    }

    private void IgnoreCollision(Collider playerCollider, Collider foodCollider)
    {
        //오브젝트를 재활용할 때 무시하는 콜라이더를 기억하고 있어서 충돌이 안되는 현상이 있음
        foreach(Collider c in _prevPlayerCollider.Values)
        {
            Physics.IgnoreCollision(foodCollider, c, false);
        }

        Physics.IgnoreCollision(foodCollider, playerCollider);
    }

    private void ResetChargeSlider(PlayerHand playerHand)
    {
        playerHand.chargeSlider.value = 0;
        playerHand.chargeSlider.gameObject.SetActive(false);
    }

    private void LaunchProjectile()
    {
        
        // 중력 가속도 조정 (기본 중력보다 작게 설정)
        // Physics.gravity = new Vector3(0, -19.6f, 0);  // 기본값 9.8의 절반으로 설정


        _foodRigidbody.useGravity = true;

        Vector3 direction = _desPos - _throwPos;
        float distance = direction.magnitude;
        
        //포물선은 사거리 영향 최대한 안받게 조정
        _maxDistance = 1000f;
        Vector3 directionXZ = new Vector3(direction.x, 0, direction.z);
        float distanceXZ = directionXZ.magnitude;

        float angleRad = _firingAngle * Mathf.Deg2Rad;                

        float velocity =  Mathf.Sqrt(distance * (_gravity) / Mathf.Sin(2 * angleRad));

        Debug.Log("발사각 Rad : "+angleRad);
        Debug.Log("발사각 velocity : "+velocity);


        float Vy = velocity * Mathf.Sin(angleRad);
        float Vxz = velocity * Mathf.Cos(angleRad);

        Vector3 launchVelocity = directionXZ.normalized * Vxz + Vector3.up * Vy;

        _foodRigidbody.AddForce(launchVelocity, ForceMode.VelocityChange);
    }
}
