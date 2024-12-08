using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleCameraMove : MonoBehaviour
{

    public float moveSpeed = 5f;
    public float rotationSpeed = 1f;

    void Update()
    {
        if (transform.position.x < 36f)
        {
            transform.Rotate(0f, -rotationSpeed * Time.deltaTime, 0f, Space.World);
            return;
        }



        // 로컬 좌표계 기준으로 앞으로 이동
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }

}
