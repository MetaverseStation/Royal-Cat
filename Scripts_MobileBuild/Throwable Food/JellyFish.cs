using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class JellyFish : MonoBehaviour
{
    // private Rigidbody _jellyFishRigidbody;
    //private PhotonView _pv;
    //private PlayerMovement _playerMovement;
    
    private float _jellyfishDebuffPercent = 0.3f;
    private float _confuseTime = 1.5f;

    private void Awake()
    {
        //_jellyFishRigidbody = GetComponent<Rigidbody>();
        // 투사체의 포톤 뷰를 가져오기
        //_pv = GetComponent<PhotonView>();
    }
    private void OnCollisionEnter(Collision collision)
    {                
        if (collision.transform.CompareTag("Player"))
        {
            PhotonView pv = collision.transform.GetComponent<PhotonView>();

            if (pv.IsMine)
            {
                //_pv.RPC("interruptMovement", RpcTarget.MasterClient, targetPhotonView.ViewID);                
                if (Random.value <= _jellyfishDebuffPercent)
                {
                    PlayerMovement playerMovement = pv.GetComponent<PlayerMovement>();
                    PlayerHealth playerHealth = pv.GetComponent<PlayerHealth>();

                    playerMovement.ApplyJellyFishEffect(_confuseTime);
                    playerHealth.ApplyVisualEffect("jellyFish", _confuseTime);

                    pv.RPC("SetJellyfishConfuse", RpcTarget.AllBuffered, pv.ViewID, _confuseTime);
                }
            }
        }
    }

    //[PunRPC]
    //private void interruptMovement(int playerViewID)
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonView targetView = PhotonView.Find(playerViewID);
    //        if (targetView != null)
    //        {
    //            // 30% 확률로 시야 가리기 효과 적용
    //            if (Random.value <= 0.3f)
    //            {
    //                _pv.RPC("ApplyJellyFishEffect", targetView.Owner, playerViewID);
    //            }

    //        }
    //    }
    //}

    //  ______ _       _     _   _               _   _   _ 
    // |  ____(_)     | |   | | (_)             | | | | | |
    // |  __| | |/ _` | '_ \| __| | '_ \ / _` | | | | | | |
    // | |    | | (_| | | | | |_| | | | | (_| | |_| |_| |_|
    // |_|    |_|\__, |_| |_|\__|_|_| |_|\__, | (_) (_) (_)
    //           __/ |                   __/ |            
    //          |___/                   |___/             

    //[PunRPC]
    //private void ApplyJellyFishEffect(int playerViewID)
    //{
    //    PhotonView targetView = PhotonView.Find(playerViewID);
    //    if (targetView != null)
    //    {
    //        PlayerMovement playerMovement = targetView.GetComponent<PlayerMovement>();
    //        PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
    //        if (playerMovement != null)
    //        {
    //            // 해당 플레이어의 움직임을 제한하는 효과 적용
    //            playerMovement.ApplyJellyFishEffect();
    //            playerHealth.ApplyVisualEffect("jellyFish", _confuseTime);

    //            targetView.RPC("SetJellyfishConfuse", RpcTarget.All, targetView.ViewID, _confuseTime);
    //        }
    //        else
    //        {
    //            Debug.LogError("PlayerMovement 컴포넌트를 찾을 수 없습니다.");
    //        }
    //    }
    //}

}
