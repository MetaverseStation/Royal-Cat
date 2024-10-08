using System.Threading;
using UnityEngine;

public class RoomItemButton : MonoBehaviour
{
    private LobbyManager _lobbyManager;

    public string roomFullName;
    public string roomName;
    public string roomCode;
    public int currentRoomCount;
    public int limitedRoomCount;
    public bool isPrivate;

    private void Awake()
    {
        _lobbyManager = FindObjectOfType<LobbyManager>();
    }
    public void OnButtonPressed()
    {
        //플레이어가 0이라 제거될 예정인 방은 입장 불가
        if (currentRoomCount == 0)
        {
            return;
        }
        //꽉찬 방 입장 불가
        if (currentRoomCount == limitedRoomCount)
        {
            if (_lobbyManager != null)
            {
                UIManager.Inst.SetInformationPopup("방이 꽉 찼습니다.");
            }
            return;
        }

        //비공개 방일 시 입장코드 팝업 뜸
        if (isPrivate)
        {
            if (_lobbyManager != null)
            {
                _lobbyManager.FindRoomClick();
            }
            else
            {
                Debug.Log("로비 매니저를 찾을 수 없음");
                UIManager.Inst.SetUnknownError();
            }
            return;
        }

        PhotonManager.Inst.JoinRoom(roomFullName);
    }
}
