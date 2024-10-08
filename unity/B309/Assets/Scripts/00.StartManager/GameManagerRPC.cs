using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Newtonsoft.Json.Bson;
using UnityEngine.Rendering;

public class GameManagerRPC : MonoBehaviourPunCallbacks
{

    private void Start()
    {
        if (photonView.IsMine)
            GameManager.Inst.SetGameManagerRPC(photonView);
    }
    
    [PunRPC]
    public void ActiveGameSetUI(string[] playerList, int skinIndex, int faceIndex)
    {

        if (InGameUIManager.Inst != null)
        {
            StartCoroutine(InGameUIManager.Inst.GameSetEvent(playerList, skinIndex, faceIndex));
        }
    }
}
