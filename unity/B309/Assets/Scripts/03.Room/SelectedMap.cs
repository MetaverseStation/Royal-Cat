using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;

public class SelectedMap : MonoBehaviourPunCallbacks
{

    [Header("image option")]
    public Image currentMapImage;
    public Sprite[] mapImages;
    public int mapIndex;

    private RoomManager _roomManager;
    private Player _myPlayer;


    // Start is called before the first frame update
    void Start()
    {
        //currentMapImage = GetComponent<Image>();
        currentMapImage.sprite = mapImages[mapIndex];

        _myPlayer = PhotonNetwork.LocalPlayer;
        _roomManager = GameObject.Find("Room").GetComponent<RoomManager>();
    }

    public void NextMap()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);

        if (!(_myPlayer.IsMasterClient))
        {
            UIManager.Inst.SetPopupCustum("방장만 맵 변경이 가능합니다.", true, null, null, "확인", null);
            return;
        }

        // 맨 마지막 map일 경우 처음으로 돌아가기
        if (currentMapImage.sprite == mapImages[mapImages.Length - 1])
        {
            currentMapImage.sprite = mapImages[0];
            mapIndex = 0;
        }
        else
        {
            // mapIndex에 따라 맵 이미지 설정
            currentMapImage.sprite = mapImages[++mapIndex];
        }

        SelectMap();
    }

    public void PreviousMap()
    {
        if (!(_myPlayer.IsMasterClient))
        {
            UIManager.Inst.SetPopupCustum("방장만 맵 변경이 가능합니다.", true, null, null, "확인", null);
            return;
        }

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        if (currentMapImage.sprite == mapImages[0])
        {
            currentMapImage.sprite = mapImages[mapImages.Length - 1];
            mapIndex = mapImages.Length - 1;
        }
        else
        {
            currentMapImage.sprite = mapImages[--mapIndex];
        }

        SelectMap();
    }

    public void SelectMap()
    {
        if (_myPlayer.IsMasterClient)
        {
            if (_roomManager._mapIndex == mapIndex)
            {
                return;
            }

            //Debug.Log("맵 설정 완료");
            _roomManager.SelectMap(mapIndex);

        }
        else
        {
            //UIManager.Inst.SetPopupCustum("방장만 맵 변경이 가능합니다.", true, null, null, "확인", null);
        }
    }

    //맵 변동이 일어날 시 인덱스 변경
    public void UpdateMap(int mapIdx)
    {
        currentMapImage.sprite = mapImages[mapIdx];
        mapIndex = mapIdx;
    }
}
