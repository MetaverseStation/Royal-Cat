using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyfishEffect : IHitEffect
{
    private float blindPercent = 0.3f;
    private float confuseTime = 1.5f;

    public void Execute(GameObject target) 
    {
        PhotonView pv = target.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            if (Random.value <= blindPercent)
            {
                PlayerMovement playerMovement = pv.GetComponent<PlayerMovement>();
                playerMovement.ApplyJellyfishEffect(confuseTime);

                pv.RPC("ShowJellyfishEffect", RpcTarget.AllBuffered, pv.ViewId, confuseTime);
            }
        }
    }
}