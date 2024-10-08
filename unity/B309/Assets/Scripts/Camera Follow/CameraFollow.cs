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


            // 카메라로부터 투명벽까지 raycast
            if (!Physics.Raycast(cameraPosition, Vector3.right * cameraPosition.x, rayDistance, cameraCollision))
            {
                // ray가 투명벽에 닿지 않으면 카메라가 이동하고, ray가 맞으면 그 방향으로 카메라가 더 이상 이동하지 않는다.
                newPosition.x = cameraPosition.x;
            } 

            // // 똑같이 z축의 방향으로 ray를 쏘면 된다.
            Vector3 ZrayStartPosition = cameraPosition + new Vector3(0, 0, 10f);
            
            if(!Physics.Raycast(ZrayStartPosition, Vector3.forward * cameraPosition.z, rayDistance, cameraCollision))
            {
                newPosition.z = cameraPosition.z;
            }  

            // 충돌이 없으면 정상적으로 카메라를 따라감
            transform.position = newPosition;
            
        }
    }


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

}
