using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Chat;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.EventSystems;

public class LobbyChat : MonoBehaviour, IChatClientListener
{
    public TMP_InputField inputChat;
    public TextMeshProUGUI chatLog;
    public Button sendButton;

    private ChatClient _chatClient;

    private string _userNickname = null;
    private string _lobbyChannel = "LobbyChannel";
    private string _version = null;

    public ScrollRect scrollRect;

    void Start()
    {
        _userNickname = GameConfig.UserNickName;
        _version = GameConfig.GameVersion;

        //채팅서버 연결
        //포톤의 realtime 서버 내에 채팅 서버를 추가로 연결한다.
        _chatClient = new ChatClient(this);
        _chatClient.Connect(
            PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat,
            _version, 
            new Photon.Chat.AuthenticationValues(_userNickname));     

        chatLog.text = "";

        //버튼 연결
        sendButton.onClick.AddListener(SendMessageToLobby);
    }

    void Update()
    {
        if(_chatClient != null)
        {
            _chatClient.Service();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                inputChat.ActivateInputField();
                SendMessageToLobby();               
            }
        }        
    }


    private void ScrollToBottom()
    {        
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
        
    }

    public void SendMessageToLobby()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        string msg = inputChat.text;
        if (!string.IsNullOrEmpty(msg))
        {            
            _chatClient.PublishMessage(_lobbyChannel, msg);

            inputChat.text = "";            

            ScrollToBottom();
        }        
    }

    //메시지를 받을 때
    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            //Debug.Log(senders[i] + ": " + messages[i].ToString() + "\n");

            string msg = $"<b>{senders[i]}:</b> {messages[i].ToString()}\n";
            //senders[i] + ": " + messages[i].ToString() + "\n";            
            chatLog.text += msg;
        }
        ScrollToBottom();
    }

    public void DebugReturn(DebugLevel level, string message){}
    public void OnChatStateChange(ChatState state){}

    public void OnConnected()
    {
        //Debug.Log("Connected to Photon Chat");
        _chatClient.Subscribe("LobbyChannel");       
    }

    public void OnDisconnected()
    {
        Debug.Log("채팅 연결 실패");
        chatLog.text += "[채팅 서버에 접속할 수 없습니다. (최대 20명)]\n";
    }
    public void OnPrivateMessage(string sender, object message, string channelName){}
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message){}
    public void OnSubscribed(string[] channels, bool[] results)
    {
        chatLog.text += "[로비에 접속하였습니다.]\n";
    }
    public void OnUnsubscribed(string[] channels){}
    public void OnUserSubscribed(string channel, string user){}
    public void OnUserUnsubscribed(string channel, string user){}
}
