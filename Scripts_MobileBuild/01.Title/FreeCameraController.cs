using UnityEngine;

public class FreeCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;  // 마우스 회전 감도
    public float moveSpeed = 5f;           // 카메라 이동 속도

    private float pitch = 0f;              // 수직 회전값 (X축 기준)
    private float yaw = 0f;                // 수평 회전값 (Y축 기준)

    void Start()
    {
        // 초기 카메라 회전 설정 (-X 방향을 보도록 Y축 회전 설정)
        yaw = -90f;  // Y축 기준 90도 회전하면 -X 방향을 보게 됨

        // 카메라의 초기 회전 적용
        transform.eulerAngles = new Vector3(pitch, 0f, 0f);
        // transform.rotation = Quaternion.LookRotation(Vector3.forward);

        // 마우스 커서 고정 및 숨기기
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 1. 마우스 입력으로 카메라 회전 제어
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;                     // 수평 회전 누적
        pitch -= mouseY;                   // 수직 회전 누적
        pitch = Mathf.Clamp(pitch, -90f, 90f);  // 위/아래 각도 제한

        // 카메라 회전 적용
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // 2. 방향키로 카메라 이동 제어
        float horizontal = Input.GetAxis("Horizontal");  // A(-1) <-> D(1)
        float vertical = Input.GetAxis("Vertical");      // W(1) <-> S(-1)

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // 3. Q/E 키로 상하 이동
        if (Input.GetKey(KeyCode.Q))
            transform.position += transform.up * moveSpeed * Time.deltaTime;  // 위로 이동
        if (Input.GetKey(KeyCode.E))
            transform.position -= transform.up * moveSpeed * Time.deltaTime;  // 아래로 이동
    }
}
