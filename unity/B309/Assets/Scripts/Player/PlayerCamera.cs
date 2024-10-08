using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerCamera : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    private GameObject _camera;

    //public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    //{
    //    PhotonView photonView = GetComponent<PhotonView>();

    //    // 카메라 생성 및 로컬 플레이어에 할당        
    //    if (photonView.IsMine)
    //    {
    //        _camera = Instantiate(Resources.Load<GameObject>("Camera"));  // Camera Prefab을 Resources에서 로드하여 생성
    //        _camera.GetComponent<CameraFollow>().SetTarget(transform);

    //        InGameUIManager.instance.Init();
    //    }
    //}
}