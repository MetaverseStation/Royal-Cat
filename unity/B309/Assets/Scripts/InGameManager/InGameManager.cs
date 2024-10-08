//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;
//using Photon.Realtime;

//public class InGameManager : MonoBehaviourPunCallbacks
//{
//    public static InGameManager Inst { get; private set; }

//    public GameObject myPlayer;

//    void Awake()
//    {
//        //싱글톤 선언
//        if (Inst == null)
//        {
//            Inst = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//        }
//    }

//    // Start is called before the first frame update
//    void Start()
//    {      
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }

//    public void InitFishWeapon(string fishName )
//    {
//        string prefabPath = "FishInHand/" + fishName;
//        fish = PhotonNetwork.Instantiate(prefabPath, fishPosition.position, fishPosition.rotation);
//        fish.transform.SetParent(fishPosition);
//    }

//    public void SwitchFishWeapon(GameObject obj)
//    {

//    }

    
//}
