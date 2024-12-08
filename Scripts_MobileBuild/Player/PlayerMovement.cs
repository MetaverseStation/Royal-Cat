using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI; // Image 사용을 위해 추가
using System.Collections.Generic; // HashSet 사용을 위해 추가
using TMPro;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    [Header("Speed")]
    public float originalSpeed = 5f;
    public float moveSpeed;
    public float rotationSpeed = 10f;
    private float SpeedUpCount = 0;

    [Header("Components")]
    private Renderer[] _playerRenderers;
    private PlayerInput _playerInput; // 플레이어 입력 알려주는 컴포넌트
    private Rigidbody _playerRigidbody;// 플레이어 캐릭터의 리지드 바디
    private Animator _playerAnimator; // 플레이어 캐릭터의 애니메이터
    private PhotonView _pv; // ???? ?????
    private PlayerHand _playerHand;
    //private GameObject _turtleOnHead;

    [Header("Status")]
    public bool _isDodge; // Check Player is Dodging now
    public bool _isReverse = false; // Check Player is Reverse now
    private bool _isBorder; // 플레이어 벽 충돌 플래그
    public bool isSlow = false; // 거북이 적용여부


    [Header("Movement")]
    private Vector3 _moveDirection;
    private Vector3 _dodgeMovement;
    private float _dodgeDistance = 3.5f; // 회피 시 이동할 거리
    private float _dodgeDuration = 0.5f; // 회피 동작의 지속 시간

    public GameObject playerTag;

    //네트워크 통신
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;
    private float _moveMagnitude; //이동속도

    // 부쉬 내 플레이어 관리
    public HashSet<int> _playersInBush = new HashSet<int>(); // 필드 내에 있는 플레이어들 관리

    void Awake()
    {
        // 사용할 컴포넌트들의 참조 가져오기
        _playerRenderers = GetComponentsInChildren<Renderer>();
        _playerInput = GetComponent<PlayerInput>();
        _playerRigidbody = GetComponent<Rigidbody>();
        _playerRigidbody.mass = 99999999f;
        _playerAnimator = GetComponent<Animator>();
        _pv = GetComponent<PhotonView>();

        // 추가부분
        _playerHand = FindObjectOfType<PlayerHand>();
        //_turtleOnHead = transform.Find("HeadPosition").GetChild(0).gameObject;

        if (!_pv.IsMine)
        {
            GetComponent<PhotonAnimatorView>().enabled = true;
        }

        moveSpeed = originalSpeed;
    }

    private void FixedUpdate()
    {
        if (_pv.IsMine)
        {
            // FixedUpdate는 물리 갱신 주기에 맞춰서 실행됨
            StopToWall();
            Move();
        }
        else
        {
            // 네트워크로 수신한 위치로 부드럽게 이동
            _playerRigidbody.MovePosition(Vector3.Lerp(_playerRigidbody.position, _networkPosition, Time.fixedDeltaTime * moveSpeed));

            // 네트워크로 수신한 회전으로 부드럽게 회전
            _playerRigidbody.MoveRotation(Quaternion.Lerp(_playerRigidbody.rotation, _networkRotation, Time.fixedDeltaTime * rotationSpeed));
        }
    }

    private void Update()
    {
        if (_pv.IsMine)
        {
            // 애니메이션에 이동 속도 전달
            _playerAnimator.SetFloat("Move", _moveMagnitude);

            // Dodge();
        }
    }

    private void Move()
    {
        // Debug.Log(_playerInput.moveHorizontal);
        // Debug.Log(_playerInput.moveVertical);
        // 이동 방향 벡터 생성
        _moveDirection = new Vector3(_playerInput.moveHorizontal, 0.0f, _playerInput.moveVertical);

        // 이동 방향 벡터 정규화
        if (_moveDirection.magnitude > 1)
        {
            _moveDirection.Normalize();
        }

        Vector3 movement;

        // 반대 이동에 걸린 경우 moveDirection 반전
        // 회피와 구르기보다 먼저 설정해준다
        if (_isReverse)
        {
            _moveDirection = -_moveDirection;
        }

        if (_isDodge)
        {
            // 회피 중인 경우 일정한 속도로 이동
            float dodgeSpeed = _dodgeDistance / _dodgeDuration;
            movement = _dodgeMovement * dodgeSpeed * Time.fixedDeltaTime;
        }
        else if (_isBorder)
        {
            movement = _moveDirection * 0.1f;
        }
        else
        {
            movement = _moveDirection * moveSpeed * Time.fixedDeltaTime;
        }

        _moveMagnitude = movement.magnitude;

        // 회피 중이 아닐 때만 회전 처리
        if (_moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            Quaternion smoothedRotation = Quaternion.Lerp(_playerRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            _playerRigidbody.MoveRotation(smoothedRotation);
        }

        if (!_isBorder && !_isDodge && _moveDirection != Vector3.zero)
        {
            if (!AudioManager.Inst.sfxPlayer[AudioManager.Inst.sfxChannelIndex].isPlaying)
            {
                AudioManager.Inst.PlaySfx(AudioManager.Sfx.FootStep);
            }
        }

        // 리지드 바디를 이용해 위치 변경
        _playerRigidbody.MovePosition(_playerRigidbody.position + movement);
    }
    public void Dodge()
    {
        Debug.Log("닷지 호출됨");
        if (_moveDirection != Vector3.zero && !_isDodge && !isSlow)
        {
            Debug.Log("닷지 조건 걸러짐");
            //
            _dodgeMovement = _moveDirection.normalized;
            _playerAnimator.SetTrigger("doDodge");
            _isDodge = true;

            _playerHand.chargeTime = 0;
            _playerHand.chargeSlider.gameObject.SetActive(false);
            _playerHand.ResetLine();

            Invoke("DodgeOut", _dodgeDuration);

            if (_isBorder)
            {
                // _isDodge = false; // 충돌하면 회피 중단
                _moveDirection = Vector3.zero;
            }
        }
    }

    // Dodge Cooltime
    private void DodgeOut()
    {
        _isDodge = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 자신의 위치와 속도를 네트워크에 전송
            stream.SendNext(_playerRigidbody.position);
            stream.SendNext(_playerRigidbody.rotation);
        }
        else
        {
            // 네트워크로부터 상대의 위치와 속도를 수신하여 동기화
            //_playerRigidbody.position = (Vector3)stream.ReceiveNext();
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();

        }
    }


    [PunRPC]
    public void SetVisibility(int targetViewID, bool isVisible)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView != null)
        {
            Renderer[] renderers = targetView.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = isVisible;
            }
        }
    }

    [PunRPC]
    public void SetVisibility(bool isVisible)
    {
        if (!_pv.IsMine) // 로컬 플레이어가 아닌 경우에만 가시성 변경
        {
            _playerRenderers = GetComponentsInChildren<Renderer>();
            if (_playerRenderers != null)
            {
                foreach (var renderer in _playerRenderers)
                {
                    renderer.enabled = isVisible;
                }
            }
            else
            {
                Debug.LogError("SetVisibility 호출 시 Renderer가 null입니다.");
            }

            // Image 처리
            Image[] images = GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                Color color = image.color;
                color.a = isVisible ? 1.0f : 0.0f; // 가시성에 따라 투명도 설정
                image.color = color;
            }

            // 플레이어 태그 활성화
            playerTag.SetActive(isVisible);
        }

    }


    private void StopToWall()
    {
        RaycastHit hit;
        _isBorder = Physics.Raycast(_playerRigidbody.position + Vector3.up * 0.5f, _moveDirection, out hit, 1f, LayerMask.GetMask("Wall"));
        Debug.DrawRay(_playerRigidbody.position + Vector3.up * 0.5f, _moveDirection * 5f, Color.green);

    }


    public void ApplyJellyFishEffect(float confuseTime)
    {
        StartCoroutine(ReverseMovementCoroutine(confuseTime));
    }

    IEnumerator ReverseMovementCoroutine(float confuseTime)
    {
        // 3초간 반대 방향으로 이동하게 설정 -> 69번 줄 참고

        _isReverse = true;
        yield return new WaitForSeconds(confuseTime);
        _isReverse = false;

    }

    public void SlowAttack(float slowTime)
    {
        // 이미 머리에 거북이 쓰고 있으면 코루틴 호출 없이 리턴
        if (isSlow)
        {
            return;
        }

        StartCoroutine(TurtleAttack(slowTime));
    }

    private IEnumerator TurtleAttack(float slowTime)
    {
        isSlow = true;
        moveSpeed *= 0.7f;

        yield return new WaitForSeconds(slowTime);

        moveSpeed = originalSpeed;
        isSlow = false;
    }

    public void SpeedUp()
    {
        if (SpeedUpCount < 5)
        {
            SpeedUpCount++;
            moveSpeed += 0.5f;
        }
    }

    [PunRPC]
    private void SetVisibilityToPlayer(int targetViewID, bool isVisible)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);
        if (targetView != null)
        {
            // RPC를 받은 플레이어의 시점에서 target을 안보이게 설정
            targetView.gameObject.GetComponent<PlayerMovement>().SetVisibility(isVisible);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_pv.IsMine)
        {
            if (other.CompareTag("Buff") || other.CompareTag("Skill") || other.CompareTag("Weapon"))
            {
                AudioManager.Inst.PlaySfx(AudioManager.Sfx.CollectItem);
            }
        }
    }
}
