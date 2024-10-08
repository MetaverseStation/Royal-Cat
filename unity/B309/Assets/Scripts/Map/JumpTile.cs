using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpTile : MonoBehaviour
{
    public GameObject targetPosition;  // 목표 위치
    private float _moveSpeed = 1f;       // 이동 속도
    private float _waitTime = 0.7f;     // 대기 시간 0.7초
    private float _shakeAmount = 0.1f;   // 타일 흔들림의 정도
    private float _launchForce = 10f;    // y축으로 발사하는 힘

    private bool _isJumping = false;
    private bool _isPlayerOnCube = false;
    private float _jumpProgress = 0f;
    private float _timer = 0f;          // 3초 대기 타이머

    private GameObject _player;         // 충돌한 플레이어 객체
    private Vector3 _startPosition;
    private Vector3 _originalTilePosition;  // 타일의 원래 위치

    void Start()
    {
        _originalTilePosition = transform.position;  // 타일의 원래 위치 저장
    }

    void Update()
    {
        if (_isPlayerOnCube)
        {
            Debug.Log("Player is on the cube.");
        }
        // 플레이어가 큐브 위에 있을 때 타일 흔들림 효과
        if (_isPlayerOnCube && !_isJumping)
        {
            ShakeTile();
            _timer += Time.deltaTime;  // 시간이 경과함
        }

        // 3초가 지났으면 점프를 시작
        if (_isPlayerOnCube && _timer >= _waitTime && !_isJumping)
        {
            _isJumping = true;  // 점프 상태로 전환
            _timer = 0f;        // 타이머 초기화
        }

        // 점프 로직
        if (_isJumping)
        {
            JumpTowardsTarget();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("123");
        // 플레이어가 큐브에 올라왔을 때만 활성화
        if (collision.gameObject.CompareTag("Player"))  // 플레이어가 태그 "Player"로 설정된 경우
        {
            _isPlayerOnCube = true;
            _player = collision.gameObject;  // 충돌한 플레이어 객체를 저장
            _startPosition = _player.transform.position;  // 플레이어의 시작 위치 저장
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log("321");
        // 플레이어가 큐브에서 떨어지면 타이머를 리셋
        if (collision.gameObject == _player)
        {
            _isPlayerOnCube = false;
            _timer = 0f;  // 타이머 초기화
            transform.position = _originalTilePosition;  // 타일 위치 초기화
        }
    }


    // 타일 흔들림 효과
    private void ShakeTile()
    {
        float shakeX = Random.Range(-_shakeAmount, _shakeAmount);  // X축 흔들림
        float shakeZ = Random.Range(-_shakeAmount, _shakeAmount);  // Z축 흔들림
        transform.position = _originalTilePosition + new Vector3(shakeX, 0, shakeZ);  // 타일 흔들림 적용
    }

    // 플레이어를 목표 위치로 점프시킴
    private void JumpTowardsTarget()
    {
        Vector3 targetPos = targetPosition.transform.position;

        // 점프 진행률 계산
        _jumpProgress += Time.deltaTime * _moveSpeed;

        if (_jumpProgress < 1f)
        {
            // Slerp로 부드럽게 이동
            Vector3 newPos = Vector3.Lerp(_startPosition, targetPos, _jumpProgress);

            // 발사 시 y축으로 강하게 올라감
            float yForce = Mathf.Sin(_jumpProgress * Mathf.PI) * _launchForce;

            // 플레이어의 위치 변경 (y축으로 힘 있게 올라감)
            _player.transform.position = new Vector3(newPos.x, newPos.y + yForce, newPos.z);
        }
        else
        {
            // 점프 완료
            _isJumping = false;
            _jumpProgress = 0f;  // 초기화
            transform.position = _originalTilePosition;  // 타일 위치 복구
        }
    }
}
