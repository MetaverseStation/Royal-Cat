using UnityEngine;

public class CanvasWithWorldSpace : MonoBehaviour
{
    public Camera followCamera; // UI가 바라 볼 카메라

    private void LateUpdate()
    {
        transform.LookAt(transform.position + followCamera.transform.forward, Vector3.up);
    }
}
