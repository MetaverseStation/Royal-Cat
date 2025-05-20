using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabEffect : IHitEffect
{
    //꽃게의 출혈은 2초동안 총 5가 감소
    private int totalDamage = 5;
    private float bleedingTime = 2f;

    public void Execute(GameObject target)
    {

        PhotonView pv = target.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.DamageOverTime(totalDamage, bleedingTime);
                pv.RPC("SetCrabBleeding", RpcTarget.All, pv.ViewID, bleedingTime);
            }
        }
    }
}