using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class PlayerRPC : MonoBehaviour, IPunObservable
{
    private const string _playerHandSocketPath = "root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/HandSphere";

    [PunRPC]
    void SwitchFishWeapon(int fishView, int playerView)
    {
        if (GameManager.Inst._fishInHand != null)
        {

            //기존의 무기 프리팹을 제거하고 새로운 프리팹으로 교체
            GameObject fishObj = PhotonView.Find(fishView).gameObject;
            GameObject playerObj = PhotonView.Find(playerView).gameObject;

            Transform handTransform = playerObj.transform.Find(_playerHandSocketPath);

            if (handTransform != null)
            {
                fishObj.transform.SetParent(handTransform);


                if (fishObj.GetPhotonView().IsMine)
                {
                    GameManager.Inst._fishInHand = fishObj;
                }
            }
        }
    }

    [PunRPC]
    public void SetPlayerColorUI(int playerViewID, string playerColor)
    {
        GameObject player = PhotonView.Find(playerViewID).gameObject;
        Color color;
        ColorUtility.TryParseHtmlString("#" + playerColor, out color);

        //체력바
        player.transform.Find("Chibi_Cat/Canvas/HealthSlider/Fill").GetComponent<Image>().color = color;

        //네임택
        player.transform.Find("Chibi_Cat/Canvas/NicknameText").GetComponent<TextMeshProUGUI>().color = color;

        //발판 투명도 100
        color.a = 100f / 255f;
        player.transform.Find("Chibi_Cat/Canvas/PlayerDistinct").GetComponent<Image>().color = color;
    }


    [PunRPC]
    public void ItemEffect(int playerViewID, string itemName)
    {
        BuffHUD.Inst.CreateBuffText(playerViewID, itemName);
    }

    [PunRPC]
    public void SetOctopusBlind()
    {
        BuffHUD.Inst.SetOctopusBlind(true);
    }

    [PunRPC]
    public void SetJellyfishConfuse(int playerViewID, float duration)
    {
        BuffHUD.Inst.SetJellyfishConfuseEffect(playerViewID, duration);
    }

    [PunRPC]
    public void SetTurtleSlow(int playerViewID, float duration)
    {
        BuffHUD.Inst.SetTurtleSlow(playerViewID, duration);
    }

    [PunRPC]
    public void SetCrabBleeding(int playerViewID, float duration)
    {
        BuffHUD.Inst.SetCrabBleeding(playerViewID, duration);
    }


    [PunRPC]
    private void ApplyKnockBack(int playerViewID)
    {
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            Rigidbody rb = targetView.GetComponent<Rigidbody>();

            if (rb != null)
            {
                //isKnockBack = true;
                    
                // 투사체의 진행 방향을 사용하여 넉백 방향 설정 (transform.forward로 변경)
                Vector3 knockbackDirection = transform.forward.normalized;

                // Y축으로 충분히 뜨도록 Vector3.up 값 조정
                Vector3 knockbackForceVector = knockbackDirection * 2f + Vector3.up * 2f;  // Y축 값을 증가

                // 넉백 힘 적용 (ForceMode.Impulse 사용)
                targetView.GetComponent<Animator>().SetTrigger("KnockBack");
                rb.AddForce(knockbackForceVector, ForceMode.VelocityChange);
            }
        }

        //isKnockBack = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }
}
