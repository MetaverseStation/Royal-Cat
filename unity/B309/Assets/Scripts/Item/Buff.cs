using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public enum BuffType
{
    Health,
    Attack,
    Defence,
    Speed,
}

public class Buff : MonoBehaviour
{
    public BuffType buffType;
    private float _damageAmount = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PhotonView pv = other.gameObject.GetComponent<PhotonView>();

            if(pv.IsMine)
            {
                string itemName = ApplyEffect(other.gameObject);

                pv.RPC("ItemEffect", RpcTarget.All, pv.ViewID, itemName);
            }            
            Destroy(gameObject);  // 아이템 먹으면 삭제
            // AudioManager.instance.PlaySfx(AudioManager.Sfx.CollectItem);
        }
    }

    private string ApplyEffect(GameObject player)
    {
        string itemName = null;

        switch (buffType)
        {
            // 각 아이템 담당 분들이 수정해주세용~~
            case BuffType.Health:
                // 플레이어 채력 회복 코드
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                health.RestoreHealth(30);
                itemName = "Health";
                break;

            case BuffType.Attack:
                // 플레이어 공격력 상승 코드
                PlayerHand hand = player.GetComponent<PlayerHand>();
                if(hand != null)
                {
                    hand.IncreaseDamage(_damageAmount);
                }
                itemName = "Attack";
                break;

            case BuffType.Defence:
                // 플레이어 방어력 상승 코드
                PlayerHealth health2 = player.GetComponent<PlayerHealth>();
                if (health2.defense < 5)
                {
                    health2.defense += 1;
                }
                itemName = "Defence";
                break;

            case BuffType.Speed:
                // 플레이어 이동 속도 상승 코드
                PlayerMovement movement = player.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.SpeedUp();
                    // 닷지할 때 거리도 늘리나? 일단 이건 나중에 생각
                }
                itemName = "Speed";
                break;
        }

        return itemName;
    }
}