using System.Collections;
using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;

//public class FishInHand : MonoBehaviour
//{
//    public GameObject fish;
//    public Transform fishPosition;
//    //HandSphere

//    private void Awake()
//    {                
//        UpdateFish("Weapon#DefaultFish");
//    }

//    // 받는 인자(fishName)에 따라 프리펩 주소를 바꿔줌
//    public void UpdateFish(string fishName)
//    {
//        Debug.Log("생선 생성");
//        // Destroy the existing fish
//        //if (fish == null)
//        //{
//            //PhotonNetwork.Destroy(fish);
//            string prefabPath = "FishInHand/" + fishName;
//            fish = PhotonNetwork.Instantiate(prefabPath, fishPosition.position, fishPosition.rotation);
//            fish.transform.SetParent(fishPosition);
//        //}        
//    }
//}
