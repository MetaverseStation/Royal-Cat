using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Lobster : MonoBehaviour
{
    private PhotonView _pv;
    private bool isKnockBack = false;

    private void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_pv.IsMine)
        {
            // 플레이어와 닿았을 때만 넉백 처리
            if (collision.gameObject.CompareTag("Player"))
            {
                PhotonView targetPhotonView = collision.transform.GetComponent<PhotonView>();

                if (targetPhotonView != null)
                {
                    // 넉백을 모든 클라이언트에 버퍼링하도록 전송
                    _pv.RPC("ApplyKnockBack", RpcTarget.All, targetPhotonView.ViewID);
                }
            }
            else
            {
                // Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    private void ApplyKnockBack(int playerViewID)
    {
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            Rigidbody rb = targetView.GetComponent<Rigidbody>();

            if (rb != null)
            {
                isKnockBack = true;
                    
                // 투사체의 진행 방향을 사용하여 넉백 방향 설정 (transform.forward로 변경)
                Vector3 knockbackDirection = transform.forward.normalized;

                // Y축으로 충분히 뜨도록 Vector3.up 값 조정
                Vector3 knockbackForceVector = knockbackDirection * 2f + Vector3.up * 2f;  // Y축 값을 증가

                // 넉백 힘 적용 (ForceMode.Impulse 사용)
                targetView.GetComponent<Animator>().SetTrigger("KnockBack");
                rb.AddForce(knockbackForceVector, ForceMode.VelocityChange);
            }
        }

        isKnockBack = false;
    }
}
