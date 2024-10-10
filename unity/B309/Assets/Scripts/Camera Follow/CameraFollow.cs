using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public Vector3 offset;

    public LayerMask cameraCollision;
    public float rayDistance = 5f;

    Vector3 newPosition = new Vector3();
    void LateUpdate()
    {
        if (target != null)
        {
            // offset을 포함한 카메라 위치
            Vector3 cameraPosition = target.position + offset;
            // y축은 이동 없으니 고정
            newPosition.y =  cameraPosition.y;

            // 충돌이 없으면 정상적으로 카메라를 따라감
            transform.position = cameraPosition;
            
        }
    }


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

}
