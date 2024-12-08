using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 던지는 투사체 프리펩에 포함될 스크립트
public class SlowTurtle : MonoBehaviour
{    
    private float _turtleSlowPercent = 0.6f;
    private float _slowTime = 3f;

    private void OnCollisionEnter(Collision collision)
    {                
        if (collision.gameObject.CompareTag("Player"))
        {          
            PhotonView pv = collision.transform.GetComponent<PhotonView>();

            if(pv.IsMine)
            {
                if (Random.value <= _turtleSlowPercent)
                {
                    PlayerMovement movement = collision.gameObject.GetComponent<PlayerMovement>();                    
                    movement.SlowAttack(_slowTime);
                    pv.RPC("SetTurtleSlow", RpcTarget.All, pv.ViewID, _slowTime);
                }                
            }
            
        }
    }



}
