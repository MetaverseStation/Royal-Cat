using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MagneticField : MonoBehaviour
{
    private static MagneticField _instance;
    private Transform _circleTransform;
    private Transform _topTransform;
    private Transform _bottomTransform;
    private Transform _leftTransform;
    private Transform _rightTransform;

    private float _circleShirinkSpeed;
    private Vector3 _circlePosition;
    private Vector3 _circleSize;
    private Vector3 _targetCircleSize;    

    private float _damageAmount = 5f; // 자기장 데미지 양
    private float _damageInterval = 1f; // 데미지를 주는 간격
    private float _damageTimer = 0f;
    
    [Header("particle")]
    public Material particleMaterial;     // Particle System에 적용된 Material

    // Start is called before the first frame update
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }


        _circleShirinkSpeed = 0.5f;   

        _circleTransform = transform.Find("Circle");

        SetCircleSize(new Vector3(0,0), new Vector3(90,90));

        _targetCircleSize = new Vector3(0,0);

    }   

    private void Update(){
        _damageTimer += Time.deltaTime;
    if (_damageTimer >= _damageInterval)
    {
        DamagePlayersOutsideField();
        _damageTimer = 0f;
    }
        Vector3 sizeChangeVector = (_targetCircleSize - _circleSize).normalized;
        Vector3 newCircleSize = _circleSize + sizeChangeVector*Time.deltaTime*_circleShirinkSpeed;
        SetCircleSize(_circlePosition, newCircleSize);

        particleMaterial.SetFloat("_MaskRadius", newCircleSize.x*0.5f);
    }

    private void SetCircleSize(Vector3 position, Vector3 size){
        _circlePosition = position;
        _circleSize = size;
    }

    private bool IsOutSideCircle(Vector3 position){
        return Vector3.Distance(position,_circlePosition) > _circleSize.x*.5f;
    }

    public static bool IsOutSideCircle_Static(Vector3 position){
        return _instance.IsOutSideCircle(position);
    }

    public void DamagePlayersOutsideField()
{


    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
    foreach (GameObject player in players)
    {
        if (IsOutSideCircle(player.transform.position))
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            PhotonView targetPhotonView = player.GetComponent<PhotonView>();
            if (playerHealth != null)
            {
                if(targetPhotonView.IsMine){
                    Debug.Log("자기장 데미지");
                    playerHealth.OnDamage(_damageAmount);
                    targetPhotonView.RPC("SyncDamage", RpcTarget.All, playerHealth.pv.ViewID, playerHealth.health);
                }
            }
        }
    }
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
}
