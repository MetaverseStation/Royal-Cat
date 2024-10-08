using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
public class MagneticField2 : MonoBehaviour
{
    public static MagneticField2 Inst;

    private GameObject[] _players;

    //자기장 줄어드는 간격
    private float _circleShirinkSpeed = 0.5f;
    private float _damageAmount = 5f; // 자기장 데미지 양
    private float _damageInterval = 1f; // 데미지를 주는 간격
    private float _damageTimer = 0f;

    private Vector3 _minScale = new Vector3(0.5f, 0.1f, 0.5f);
    private float _distance;

    private bool _active = true;

    //세이프 존
    Transform safeZone;
    private float _radius;

    //라인렌더러
    //private LineRenderer _lr;
    //private int numSegments = 100;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        safeZone = transform.Find("SafeZone");

        //플레이어 리스트
        _players = GameObject.FindGameObjectsWithTag("Player");

        //라인렌더러 세팅
        //_lr = GetComponent<LineRenderer>();
        //_lr.positionCount = numSegments + 1;

    }

    // Update is called once per frame
    void Update()
    {
        //Draw();
        if (_active)
        {
            //자기장 크기 감소            
            _distance = Vector3.Distance(transform.localScale, _minScale);

            _radius = transform.localScale.x * 0.85f;
            Debug.Log("반지름은" + _radius);
            Debug.Log(transform.position.ToString());
            if (_distance > 0.001f)
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, _minScale, _circleShirinkSpeed * Time.deltaTime);
            }
            else
            {
                transform.localScale = _minScale;
            }

            //데미지 계산
            _damageTimer += Time.deltaTime;

            if (_damageTimer >= _damageInterval)
            {
                DamagePlayersOutsideField();
                _damageTimer = 0f;
            }
        }

    }

    private void DamagePlayersOutsideField()
    {
        if (_players != null)
        {
            foreach (GameObject player in _players)
            {
                if (!player.activeSelf)
                    continue;

                float playerDistance = Vector3.Distance(player.transform.position, transform.position);
                Debug.Log("사거리" + playerDistance);

                if (playerDistance > _radius)
                {
                    //데미지
                    PlayerHealth ph = player.GetComponent<PlayerHealth>();
                    PhotonView pv = player.GetComponent<PhotonView>();
                    if (ph != null && pv.IsMine)
                    {
                        Debug.Log("데미지");
                        ph.OnDamage(_damageAmount);
                        pv.RPC("SyncDamage", RpcTarget.All, ph.pv.ViewID, ph.health);
                    }
                }
            }
        }
    }

    public void EnableField(bool enable)
    {
        _active = enable;
    }

    [PunRPC]
    public void SyncDamage(int playerViewID, float updatedHealth)
    {
        // 모든 클라이언트에서 HP를 동기화
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.healthUIUpdate(updatedHealth);
            }
        }
    }

    //private void Draw()
    //{
    //    // 원의 각도를 계산하기 위해 2 * PI 를 numSegments로 나눔
    //    float angleStep = 2 * Mathf.PI / numSegments;

    //    // LineRenderer에 좌표 설정
    //    for (int i = 0; i <= numSegments; i++)
    //    {
    //        float currentAngle = i * angleStep;

    //        // 원의 X, Y 좌표 계산
    //        float x = Mathf.Cos(currentAngle) * _radius;
    //        float z = Mathf.Sin(currentAngle) * _radius;

    //        // LineRenderer의 좌표 설정 (Y축은 원점이므로 z 대신 y를 사용할 수도 있음)
    //        _lr.SetPosition(i, new Vector3(x, 0, z));
    //    }
    //}
}
