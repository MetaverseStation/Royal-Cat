using UnityEngine;
using Photon.Pun;
using System.Collections;

public class Squid : MonoBehaviour
{
    //private Rigidbody _squidRigidbody;
    //private PhotonView _pv;
    private float blindPercent = 0.3f;
    //private PlayerBlind _pb;

    private void Awake()
    {
        //_squidRigidbody = GetComponent<Rigidbody>();
        // 투사체의 포톤 뷰를 가져오기
        //_pv = GetComponent<PhotonView>();
        //_pb = GameManager.Inst.GetPlayer().GetComponent<PlayerBlind>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 투사체와 플레이어의 충돌지점 중 맨 처음 충돌한 지점
        //ContactPoint contact = collision.contacts[0];
        if (collision.transform.CompareTag("Player"))
        {
            PhotonView targetPhotonView = collision.transform.GetComponent<PhotonView>();
            if (targetPhotonView != null && targetPhotonView.IsMine)
            {
                Debug.Log("사람이 오징어에 맞았다");

                if (Random.value <= blindPercent)
                {
                    PlayerHealth ph = targetPhotonView.GetComponent<PlayerHealth>();
                    //ph.ApplyVisualEffect("squid", 0f);
                    targetPhotonView.RPC("SetOctopusBlind", targetPhotonView.Owner);
                }

                //_pv.RPC("interruptSight", RpcTarget.MasterClient, targetPhotonView.ViewID);
                // 투사체가 닿았을 때 데미지 입히는 로직이 필요한지?
            }
        }
    }
    //[PunRPC]
    //private void interruptSight(int playerViewID)
    //{
    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        PhotonView targetView = PhotonView.Find(playerViewID);
    //        if (targetView != null)
    //        {
    //            Debug.Log("확률 돌리기");
    //            // 30% 확률로 시야 가리기 효과 적용
    //            if (Random.value <= 0.3f)
    //            {
    //                Debug.Log("오징어 당첨");
    //                _pv.RPC("ApplySquidEffect", targetView.Owner, playerViewID);
    //            }

    //        }
    //    }
    //}

    //[PunRPC]
    //private void ApplySquidEffect(int playerViewID)
    //{
    //    // PlayerBlind 시야를 가리는 효과를 적용
    //    PhotonView targetView = PhotonView.Find(playerViewID);        
    //    if (targetView != null)
    //    {
    //        PlayerBlind playerBlind = targetView.GetComponent<PlayerBlind>();
    //        if (playerBlind != null)
    //        {
    //            // 해당 플레이어의 움직임을 제한하는 효과 적용
    //            playerBlind.ShowSquidEffect();
    //        }
    //        else
    //        {
    //            Debug.LogError("PlayerMovement 컴포넌트를 찾을 수 없습니다.");
    //        }
    //    }
    //}
}
