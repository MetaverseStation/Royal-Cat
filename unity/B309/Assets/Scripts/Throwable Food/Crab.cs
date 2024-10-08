using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Crab : MonoBehaviour
{
    //꽃게의 출혈은 2초동안 총 5가 감소
    private int totalDamage = 5;
    private float bleedingTime = 2f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PhotonView pv = collision.transform.GetComponent<PhotonView>();
            if (pv.IsMine)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.DamageOverTime(totalDamage, bleedingTime);
                    pv.RPC("SetCrabBleeding", RpcTarget.All, pv.ViewID, bleedingTime);
                }
            }
        }
    }
}