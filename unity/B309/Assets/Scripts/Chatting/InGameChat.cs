using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class InGameChat : MonoBehaviourPunCallbacks
{    
    public TMP_InputField inputChat;
    
    public RectTransform chatPanelRect;

    public GameObject chatLogPanel;
    public TextMeshProUGUI chatLog;
    public ScrollRect scrollRect;
    private const byte _chatEventCode = 2;

    //인게임 채팅변수    
    public GameObject currentChat;
    public TextMeshProUGUI currentText;

    //채팅창 크기 조절 변수
    private float _originHeight;
    private const float _divValue = 3f;

    private bool _isChat = false;

    private string _myPlayerColor;

    void Start()
    {
        inputChat.interactable = false;
        _originHeight = chatPanelRect.sizeDelta.y;
        SetChatHeight((int)(_originHeight / _divValue));
        chatLogPanel.SetActive(false);
        currentText.text = "";
        currentChat.SetActive(true);        
    }
    
    void Update()
    {
        //(toggle) 엔터키를 누르면 인풋 활성화 + 창 최대화
        //다시 엔터 누르면 줄어듬      
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if(!string.IsNullOrEmpty(inputChat.text))
            {
                SendMessageInGame(inputChat.text);
                _isChat = false;                
            }
            else
            {
                _isChat = !_isChat;                
            }
            ChatToggle(_isChat);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            ChatToggle(false);
        }
    }

    private void SendMessageInGame(string msg)
    {
        //TODO: 플레이어 별 테마색 입히기
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

    //알림을 주고 싶을 때 사용하는 함수
    /*
     * ex) 자기장이 형성됩니다! , A 플레이어가 B를 처치했습니다.
     */ 
    public void SendSystemMessage(string msg)
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            string coloredMsg = $"<b><color=#8C8C8C>[Info] {msg}</color></b>";

            object content = coloredMsg;

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };
            PhotonNetwork.RaiseEvent(_chatEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }      
    }

    public void OnClickButton()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        SendMessageInGame(inputChat.text);
    }

    private void ChatToggle(bool enable)
    {
        //창을 키우고 인풋을 활성화
        if (enable)
        {
            SetChatHeight((int)_originHeight);
            inputChat.interactable = true;
            inputChat.ActivateInputField();
            chatLogPanel.SetActive(true);
            currentChat.SetActive(false);
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
        else
        {
            //메시지를 보내고 창을 닫기
            SendMessageInGame(inputChat.text);
            SetChatHeight((int)(_originHeight / _divValue));
            chatLogPanel.SetActive(false);
            currentChat.SetActive(true);
            inputChat.interactable = false;
            inputChat.DeactivateInputField();
        }
    }

    private void SetChatHeight(int height)
    {
        Vector2 size = chatPanelRect.sizeDelta;
        size.y = height;
        chatPanelRect.sizeDelta = size;
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

            currentText.text = receivedMessage;

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
