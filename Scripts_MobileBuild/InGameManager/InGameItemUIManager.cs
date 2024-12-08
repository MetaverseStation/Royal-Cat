//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using Photon.Pun;



//public class InGameItemUIManager : MonoBehaviour
//{
//    public Image skillItemUI;
//    public Image weaponItemUI;
//    private PhotonView _pv;

//    void Start()
//    {
//        _pv = GetComponent<PhotonView>();

//    }

//    // 투사체 아이템 UI 변경
//    public void SetWeaponItemUI(WeaponType weaponType)
//    {
//        if (_pv.IsMine)
//        {
//            Debug.Log("투사체 아이템 획득");
//            Debug.Log("pv Id : " + _pv.ViewID);

//            string path = "Prefabs/UI/Item Img/" + weaponType;
//            Sprite newSprite = Resources.Load<Sprite>(path); // 경로: Resources/Prefabs/UI/ItemImg/Skill_arc
//            weaponItemUI.sprite = newSprite;
//        }
//    }

//    // 스킬 아이템 UI 변경
//    public void SetSkillItemUI(SkillType skillType)
//    {
//        if (_pv.IsMine)
//        {
//            Debug.Log("스킬 아이템 획득");
//            Debug.Log("pv Id : " + _pv.ViewID);

//            string path = "Prefabs/UI/Item Img/" + skillType;
//            Sprite newSprite = Resources.Load<Sprite>(path); // 경로: Resources/Prefabs/UI/ItemImg/Skill_arc
//            skillItemUI.sprite = newSprite;
//        }
//    }
//}
