using UnityEngine;
using Photon.Pun;

public enum SkillType
{
    // 스트레이트샷(기본)
    Normal,
    // 포물선
    Parabola,
    // 멀티샷
    ShotGun,
}

public class Skill : MonoBehaviour
{
    public SkillType skillType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.gameObject.GetPhotonView();

            if (pv.IsMine)
            {
                if (InGameUIManager.Inst != null)
                {
                    InGameUIManager.Inst.SetSkillItemUI(skillType);
                }
                ApplyEffect(other.gameObject);
                string itemName = checkSkillTypeToString(skillType);
                pv.RPC("ItemEffect", RpcTarget.All, pv.ViewID, itemName);
            }
            Destroy(gameObject);  // 아이템 먹으면 삭제
        }
    }

    private void ApplyEffect(GameObject player)
    {
        PlayerHand playerHand = player.GetComponent<PlayerHand>();
        if (playerHand != null)
        {
            playerHand.currentSkillType = skillType;
        }
    }

    public string checkSkillTypeToString(SkillType skillType)
    {
        string res = "Normal";
        switch(skillType)
        {
            case SkillType.Normal:
                break;
            case SkillType.Parabola:
                res = "Parabola";
                break;
            case SkillType.ShotGun:
                res = "ShotGun";
                break;
        }

        return res;
    }
}

