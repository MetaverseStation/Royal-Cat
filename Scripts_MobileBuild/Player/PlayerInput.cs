using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{

    private PlayerHealth _playerHealth;
    private PlayerMovement _playerMovement;
    private PlayerHand _playerHand;

    public float moveVertical { get; private set; }
    public float moveHorizontal { get; private set; }
    public bool chargeFood { get; private set; }
    public bool throwFood { get; private set; }
    public bool playerInstantDeath { get; private set; }

    public JoyStick joyStick;
    public Button DodgeButton;

    public bool dodgeDown;
    private PhotonView _pv; // ���� ����ȭ

    void Start()
    {
        _pv = GetComponent<PhotonView>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerHand = GetComponent<PlayerHand>();

        if (!_pv.IsMine)
        {
            GetComponent<PhotonAnimatorView>().enabled = true;
        }

        joyStick = GameObject.Find("Joystick").GetComponent<JoyStick>();
        Debug.Log("조이스틱" + joyStick);
        DodgeButton = GameObject.Find("DodgeButton").GetComponent<Button>();
        Debug.Log("닷지버튼" + DodgeButton);
        DodgeButton.onClick.AddListener(_playerMovement.Dodge);

    }

    void Update()
    {
        if (_pv.IsMine)
        {
            // ���� ���� ���¿����� ����� �Է� �������� �ʴ� ����

            // �Է� ���� ����
            // moveVertical = Input.GetAxis("Vertical");
            // moveHorizontal = Input.GetAxis("Horizontal");

            if (throwFood)
            {
                throwFood = false;
            }

            if (Input.touchCount > 0)
            {
                Debug.Log("터치카운트 0");
                bool isMoving = moveHorizontal != 0 || moveVertical != 0;

                if (!joyStick.isMoving() && !_playerMovement._isDodge)
                {
                    Debug.Log(isMoving);
                    Touch touch = Input.GetTouch(0);
                    _playerHand.attackTouch = touch;
                    chargeFood = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;

                    if (!isMoving && touch.phase == TouchPhase.Ended)
                    {
                        Debug.Log("throwFood true");
                        throwFood = true;
                    }
                }
                moveVertical = joyStick.Vertical();
                moveHorizontal = joyStick.Horizontal();

            }

            if (Input.touchCount > 1)
            {

                Debug.Log("터치카운트 1");
                if (!_playerMovement._isDodge)
                {
                    Touch touch = Input.GetTouch(1);
                    _playerHand.attackTouch = touch;
                    chargeFood = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;

                    if (touch.phase == TouchPhase.Ended)
                    {
                        Debug.Log("throwFood true");
                        throwFood = true;
                    }
                }
                else
                {
                    chargeFood = false;
                }
            }

            if (_playerHealth && !_playerHealth.dead)
            {
                dodgeDown = Input.GetButtonDown("Dodge");
            }

            // if (!isMoving)
            // {
            //     chargeFood = Input.GetMouseButton(0);
            //     throwFood = Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.Q);
            //     //playerInstantDeath = Input.GetButtonDown("InstantDeath");
            // }

            // if (Input.touchCount > 0)
            // {
            //     Touch touch = Input.GetTouch(0);
            // }
            // // 터치가 시작된 순간일 경우 (한 번 터치할 때마다 공격)
            // if (touch.phase == TouchPhase.Began)
            // {
            //     Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.nearClipPlane));
            //     Attack(touchPosition);
            // }

        }
    }

}
