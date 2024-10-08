using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Stone : MonoBehaviourPun
{
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트가 플레이어인지 확인합니다.
        if (collision.gameObject.CompareTag("Player"))
        {
            // 플레이어의 체력을 깎습니다.
            PlayerHealth playerHealth = collision.transform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                photonView.RPC("SyncDamage", RpcTarget.All, collision.gameObject.GetComponent<PhotonView>().ViewID, 10);
            }

        }
        // 돌을 없애거나 비활성화합니다.
        Destroy(gameObject); // 또는 gameObject.SetActive(false);
    }

    [PunRPC]
    private void SyncDamage(int playerID, int damage)
    {
        PhotonView targetPlayer = PhotonView.Find(playerID);
        if (targetPlayer != null)
        {
            PlayerHealth playerHealth = targetPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDamage(damage);  // 모든 클라이언트에서 플레이어의 체력을 감소
            }
        }
    }
}
