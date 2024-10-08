using System;
using UnityEngine;
using Photon.Pun;
public enum WeaponType
{
    KnockBack,
    Slow,
    Mukmul,
    Poison,
    ReverseMove,

}

public class Weapon : MonoBehaviour
{
    public WeaponType weaponType;
    public GameObject uiManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.gameObject.GetComponent<PhotonView>();

            if (pv.IsMine)
            {
                string itemName = ApplyEffect(other.gameObject);

                pv.RPC("ItemEffect", RpcTarget.All, pv.ViewID, itemName);

                if (InGameUIManager.Inst != null)
                {
                    InGameUIManager.Inst.SetWeaponItemUI(weaponType);
                }
            }
            Destroy(gameObject);  // 아이템 먹으면 삭제                        
        }
    }

    // ApplyEffect 시 Weapon의 fish를 바꾸면 전체 플레이어 오브젝트의 물고기가 변경됨
    // 플레이어 오브젝트에 fishName을 만들어서 Weapon에서 개별 플레이어의 fishName을 변경 > PlayerHand.cs 47번,189번 줄 참고
    private string ApplyEffect(GameObject player)
    {
        string itemName = "DefaultFish";

        switch (weaponType)
        {
            case WeaponType.KnockBack:
                // 플레이어 능력치 상승 코드
                GameManager.Inst.CreateFishWeapon("Weapon#Lobster");
                itemName = "Lobster";
                break;

            case WeaponType.Mukmul:
                // 현재 코드는 아이템을 먹으면 전체 플레이어가 바뀜
                // 아이템을 먹은 플레이어만 바뀌게 수정 필요
                GameManager.Inst.CreateFishWeapon("Weapon#Octopus");
                itemName = "Octopus";
                break;

            case WeaponType.ReverseMove:
                GameManager.Inst.CreateFishWeapon("Weapon#Jellyfish");
                itemName = "Jellyfish";
                break;

            case WeaponType.Poison:
                // 스킬 변경 코드
                GameManager.Inst.CreateFishWeapon("Weapon#Crab");
                itemName = "Crab";
                break;

            case WeaponType.Slow:
                GameManager.Inst.CreateFishWeapon("Weapon#SlowTurtle");
                itemName = "SlowTurtle";
                break;
        }

        return itemName;
    }
}