using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPingPong : MonoBehaviour
{
    private float _moveHeight = 0.4f;
    private float _moveSpeed = 0.3f;
    private float _elapsedTime = 0f;

    private Vector3 _startPosition;
    
    void Start()
    {
        _startPosition = transform.position;
    }

    void Update()
    {
        _elapsedTime += Time.deltaTime;
        // PingPong을 이용한 y축 점프 효과
        // PingPong(지속 시간, 왕복할 범위)
        // 속도를 조절하기 위해선 시간 * 속도를 해줘야함
        float yOffset = Mathf.PingPong(_elapsedTime * _moveSpeed, _moveHeight);
        
        // 새 위치 설정 (제자리에서 위아래로만 이동)
        transform.position = new Vector3(_startPosition.x, _startPosition.y + yOffset, _startPosition.z);
    }
}