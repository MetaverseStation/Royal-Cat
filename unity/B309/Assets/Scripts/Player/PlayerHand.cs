using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;

public class PlayerHand : MonoBehaviour
{
    private readonly string resourcePath = "Throwable Food/";

    public Transform throwPosition; // 투사체 발사 시작 위치
    public Slider chargeSlider;

    private Animator _playerAnimator; // 
    private Rigidbody _playerRigidbody; // ?��???? ��?????? ????????
    private PlayerInput _playerInput;
    private PlayerMovement _playerMovement;
    private PhotonView _pv; //해당 플레이어의 photonView

    // 공격 관련 계수
    public float damage = 10f;
    private const float _distance = 10f; //공격 사거리
    public float attackCoolTime; // 공격 쿨타임
    private float _currentAttackCoolTime; // 현재 공격 쿨타임

    // 공격력 아이템 관련 변수
    private int _damageCount = 0; // 현재 먹은 공격력 아이템 갯수
    private int _maxDamageCount = 5; // 공격력 아이템 최대 증가량

    // 차징 관련 변수
    public float chargeTime = 0f; // 차징 시간
    public float maxChargeTime = 1f; // �최대 차징 시간

    // 포물선 발사 관련 변수
    public Vector3 desPosition; // 목표 지점
    private LineRenderer _lineRenderer; // 라인 렌더러 참조 변수
    public GameObject circlePrefab;
    public GameObject circleInstance;
    public bool isParabolicAttack;
    public float firingAngle;
    public float gravity ;
    public LayerMask wallLayer;
    public LayerMask groundLayer;

    // 스킬 아이템 변경을 위한 변수
    public SkillType currentSkillType = SkillType.Normal;

    // 공격 사거리 제한 관련 변수
    public float limitedAttackRange = 7.5f;

    // 레이캐스트 조정
    Plane rayPlane;

    //[Header("Weapon")]
    //public string fishName = "DefaultFish";
    private void Start()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _playerAnimator = GetComponent<Animator>();
        _pv = GetComponent<PhotonView>();
        _playerMovement = GetComponent<PlayerMovement>();
        if (!_pv.IsMine)
        {
            GetComponent<PhotonAnimatorView>().enabled = true;
        }

        attackCoolTime = 0.4f;
        _currentAttackCoolTime = attackCoolTime;

        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        circleInstance = Instantiate(circlePrefab);
        circleInstance.SetActive(false);
        isParabolicAttack = false;

        // 스킬 타입 초기화
        currentSkillType = SkillType.Normal;

        // raycast 충돌 지점이 될 평면을 생성
        rayPlane = new Plane(Vector3.up, new Vector3(0, 1, 0)); // y = 1 위치에 1 높이의 평면 생성
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            if (!_playerMovement._isDodge)
            {

                if (_playerInput.throwFood && _currentAttackCoolTime >= attackCoolTime)
                {
                    // 투사체 발사
                    ThrowFood();

                    _pv.RPC("SyncChargeFin", RpcTarget.All);
                }

                _currentAttackCoolTime += Time.deltaTime;

                if (_playerInput.chargeFood && _currentAttackCoolTime >= attackCoolTime)
                {
                    // 차징 모션
                    _pv.RPC("SyncCharge", RpcTarget.All);

                    // // 마우스 포인터 위치 계산
                    SetTargetPosition();

                    // 차징
                    if (currentSkillType == SkillType.Normal || currentSkillType == SkillType.ShotGun)
                    {
                        Charge();
                    }
                    else
                    {
                        ShowTrajectory();
                    }
                }
            }
            else
            {
                _pv.RPC("SyncChargeFin", RpcTarget.All);
            }

        }
    }

    [PunRPC]
    private void SyncCharge()
    {
        _playerAnimator.SetBool("doCharge", true);
    }

    [PunRPC]
    private void SyncChargeFin()
    {
        _playerAnimator.SetBool("doCharge", false);
    }

    public void ResetLine()
    {
        _lineRenderer.enabled = false;
        circleInstance.SetActive(false);
    }

    // 투사체의 궤적을 계산하고 라인 렌더러에 표시
    private void ShowTrajectory()
    {
        // 중앙 지점 x,z값 지정
        Vector3 center = (throwPosition.position + desPosition) * 0.5f;
        // 센터 값 지정 / -값 지정 후 RelCenter 설정하며 위쪽 포물선으로 되게 함
        center.y -= 1.0f;
        // 플레이어의 시선 처리
        Quaternion targetRotation = Quaternion.LookRotation(center - throwPosition.position);

        // 곡사를 그리기 위해 도착 지점과 발사 지점에서 중앙지점으로 거리 재기
        Vector3 RelCenter = throwPosition.transform.position - center;
        Vector3 aimRelCenter = desPosition - center;

        // 0.147초는 뭔가요?
        _lineRenderer.positionCount = (int)(1 / 0.0417f); // positionCount 설정

        for (float index = 0.0f, interval = -0.0417f; interval < 1.0f;)
        {
            Vector3 theArc = Vector3.Slerp(RelCenter, aimRelCenter, interval += 0.0417f);
            _lineRenderer.SetPosition((int)index++, theArc + center);
        }

        // 목표 지점 원 표시
        circleInstance.SetActive(true);
        Vector3 tempDes = desPosition;
        tempDes.y += 0.3f;
        circleInstance.transform.position = tempDes;

        // 궤적 표시
        _lineRenderer.enabled = true;
    }

    private void SetTargetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hitInfo;
        RaycastHit hit;

        if (currentSkillType == SkillType.Parabola && Physics.Raycast(ray, out hit))
        {
            desPosition = hit.point;
        }
        else
        {
            float newDistance;
            if (rayPlane.Raycast(ray, out newDistance))
            {
                Vector3 targetPoint = ray.GetPoint(newDistance);
                desPosition = targetPoint;
            }
        }

        float distance = Vector3.Distance(throwPosition.position, desPosition);
        if (distance > limitedAttackRange)
        {
            // throwPosition에서 tempPoint 방향으로 제한 사거리만큼 떨어진 지점 계산
            Vector3 direction = (desPosition - throwPosition.position).normalized;
            desPosition = throwPosition.position + direction * limitedAttackRange;
        }

        desPosition.y = transform.position.y;
        //Debug.Log(desPosition);
        transform.LookAt(new Vector3(desPosition.x, transform.position.y, desPosition.z));
    }

    public void ThrowFood()
    {
        // 궤적 제거
        _lineRenderer.enabled = false;
        circleInstance.SetActive(false);

        if (_currentAttackCoolTime >= attackCoolTime)
        {
            switch (currentSkillType)
            {
                case SkillType.Normal:
                    ThrowNormal();
                    break;
                case SkillType.Parabola:
                    ThrowParabola();
                    break;
                case SkillType.ShotGun:
                    ThrowShotGun();
                    break;
            }

            // **투사체 발사 후에 chargeTime 초기화**
            if (currentSkillType == SkillType.Normal || currentSkillType == SkillType.ShotGun)
            {
                chargeTime = 0;
                chargeSlider.gameObject.SetActive(false);
            }

            ThrowAnimation();
            _currentAttackCoolTime = 0;
        }

    }

    private void ThrowAnimation()
    {

        float animationSpeed = 1 / attackCoolTime;
        animationSpeed = animationSpeed <= 1f ? 1f : animationSpeed;

        if (_pv.IsMine)
        {
            _pv.RPC("SyncThrowAnimation", RpcTarget.All, animationSpeed);
        }
    }

    [PunRPC]
    private void SyncThrowAnimation(float animationSpeed)
    {
        _playerAnimator.SetFloat("AttackSpeed", animationSpeed);
        _playerAnimator.Play("Throw", -1, 0f);

        _playerAnimator.SetBool("doCharge", false);

        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Attack);
    }


    public void Charge()
    {
        chargeSlider.gameObject.SetActive(true);
        if (chargeTime < maxChargeTime)
        {
            chargeTime += Time.deltaTime;
            chargeSlider.value = chargeTime /maxChargeTime;
            //Debug.Log("charge : " + chargeTime);
        }
        else
        {
            chargeSlider.value = 1f;
        }
    }
    public void IncreaseDamage(float multiplier)
    {
        if (_damageCount > _maxDamageCount)
        {
            return;
        }
        damage += multiplier;
        _damageCount++;
    }

    private void ThrowNormal()
    {
        isParabolicAttack = false;
        // 투사체 생성 및 발사        
        GameObject food = CreateFoodObj(throwPosition.position, throwPosition.rotation, damage, _distance);
    }

    private void ThrowParabola()
    {
        isParabolicAttack = true;
        // 투사체 생성 및 발사
        Debug.Log(isParabolicAttack);

        GameObject food = CreateFoodObj(throwPosition.position, throwPosition.rotation, damage, _distance);
        // 포물선 공격 설정        
    }

    private void ThrowShotGun()
    {
        isParabolicAttack = false;
        Debug.Log(isParabolicAttack);

        // 멀티샷 각도 설정 (부채꼴)
        float[] angles = { -5f, 0f, 5f };

        foreach (float angle in angles)
        {
            // 플레이어의 전방 방향을 기준으로 회전 계산
            Quaternion rotation = Quaternion.Euler(0, angle, 0) * transform.rotation;
            // 투사체 생성 및 발사

            //샷건은 탄당 데미지의 60퍼센트만 적용
            float dmg = (int)((damage / 100) * 60f);

            GameObject food = CreateFoodObj(throwPosition.position, rotation, dmg, _distance / 2f);
        }
    }

    private GameObject CreateFoodObj(Vector3 pos, Quaternion rot, float dmg, float dist)
    {
        string weaponName = GameManager.Inst.GetWeaponName();
        GameObject food = PhotonNetwork.Instantiate(resourcePath + weaponName, pos, rot);
        food.GetComponent<Food>().playerHand = GetComponent<PlayerHand>();

        //Debug.Log("투사체 생성됨: " + food.name + " ID: " + food.GetPhotonView().ViewID);

        PhotonView pv = food.GetPhotonView();        
   
        pv.RPC("Initialize", RpcTarget.All, GameManager.Inst.GetPlayerPV().ViewID, rot, dmg, dist);
        return food;
    }
}
