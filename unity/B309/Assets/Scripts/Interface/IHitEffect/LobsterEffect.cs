using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobsterEffect : IHitEffect
{
    public void Execute(GameObject target)
    {
        PhotonView pv = target.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            pv.RPC("ApplyKnockBack", RpcTarget.All, pv.ViewID); // RPC는 MonoBehaviour 쪽에 정의돼 있어야 함
        }
    }
}