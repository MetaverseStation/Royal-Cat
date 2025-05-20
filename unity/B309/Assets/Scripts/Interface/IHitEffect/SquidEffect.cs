using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquidEffect : IHitEffect
{
    private float blindPercent = 0.3f;

    public void Execute(GameObject target)
    {
        PhotonView pv = target.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            if (Random.value <= blindPercent)
            {
                BuffHUD.Inst.SetOctopusBlind(true);
            }
        }
    }
}