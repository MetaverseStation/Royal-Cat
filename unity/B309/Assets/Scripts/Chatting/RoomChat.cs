using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomChat : MonoBehaviourPunCallbacks
{
    public TMP_InputField inputChat;
    public TextMeshProUGUI chatLog;

    public ScrollRect scrollRect;    

    private const byte _chatEventCode = 1;

    private string _myPlayerColor;

    public Button sendButton;

    void Start()
    {
        sendButton.onClick.AddListener(SendMessageInRoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {                        
            inputChat.ActivateInputField();                                        
            SendMessageInRoom();                        
        }
    }

    public void SendMessageInRoom()
    {
        string msg = inputChat.text;

        //string randomColor = $"#{Random.Range(0x100000, 0xFFFFFF):X6}";        
        _myPlayerColor = PhotonManager.Inst.GetPlayerThemeColorCode();                
        if (!string.IsNullOrEmpty(msg))
        {
            string coloredMsg = $"<b><color=#{_myPlayerColor}>{GameConfig.UserNickName}:</color></b> {msg}";
            object content = coloredMsg;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
            PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);
            
            inputChat.text = "";                        
        }
    }

    public void SetInfoMessage(string msg)
    {
        msg = string.Format($"<color=#00ff00>{msg}</color>");
        object content = msg;
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);        
    }

    // 이벤트 수신 처리
    public override void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == _chatEventCode)
        {
            string receivedMessage = (string)photonEvent.CustomData;
            chatLog.text += receivedMessage + "\n";

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void SetMessageLocal(string msg)
    {
        chatLog.text += msg + "\n";

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public bool GetInputActive()
    {
        return inputChat.interactable;
    }

    //룸매니저에 있음
    //public void OnPlayerEnteredRoom(Player newPlayer)
    //{
    //    base.OnPlayerEnteredRoom(newPlayer);
        
    //    string msg = string.Format("<color=#00ff00>[{0}]님이 입장하셨습니다.</color>", newPlayer.NickName);
    //    SendMessageInRoom(msg);
    //}

    //public override void OnPlayerLeftRoom(Player otherPlayer)
    //{        
    //    string msg = string.Format("<color=#00ff00>[{0}]님이 퇴장하셨습니다.</color>", otherPlayer.NickName);
    //    SendMessageInRoom(msg);
    //}
}
