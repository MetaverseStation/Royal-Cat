using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowTurtleEffect : IHitEffect
{
    private float _turtleSlowPercent = 0.6f;
    private float _slowTime = 3f;

    public void Execute(GameObject target)
    {
        PhotonView pv = target.GetComponent<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            if (Random.value <= _turtleSlowPercent)
            {
                PlayerMovement movement = target.GetComponent<PlayerMovement>();                    
                movement.SlowAttack(_slowTime);
                pv.RPC("SetTurtleSlow", RpcTarget.All, pv.ViewID, _slowTime);
            }                
        }
            
        
    }

}