using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInput : MonoBehaviour
{

    private PlayerHealth _playerHealth;

    public float moveVertical { get; private set; }
    public float moveHorizontal { get; private set; }
    public bool chargeFood { get; private set; }
    public bool throwFood { get; private set; }
    public bool playerInstantDeath { get; private set; }

    public bool dodgeDown;
    private PhotonView _pv; // ���� ����ȭ

    void Start()
    {
        _pv = GetComponent<PhotonView>();
        _playerHealth = GetComponent<PlayerHealth>();

        if (!_pv.IsMine)
        {
            GetComponent<PhotonAnimatorView>().enabled = true;
        }

    }

    void Update()
    {
        if (_pv.IsMine)
        {
            // ���� ���� ���¿����� ����� �Է� �������� �ʴ� ����

            // �Է� ���� ����
            moveVertical = Input.GetAxis("Vertical");
            moveHorizontal = Input.GetAxis("Horizontal");

            if (_playerHealth && !_playerHealth.dead)
            {
                dodgeDown = Input.GetButtonDown("Dodge");
            }

            chargeFood = Input.GetMouseButton(0);
            throwFood = Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.Q);            
            //playerInstantDeath = Input.GetButtonDown("InstantDeath");

        }
    }

}
