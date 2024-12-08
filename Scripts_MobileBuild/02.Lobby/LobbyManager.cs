using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;
//using System.Collections;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    //방 목록 갱신 관련
    public Transform contentTransform;
    public GameObject roomItemPrefab;

    //방 생성 팝업 관련
    public GameObject createRoomPopupPrefab;
    private TMP_InputField _roomNameText;
    private TMP_Dropdown _maxPlayerDropdown;
    private Toggle _isPrivateToggle;
    private TextMeshProUGUI _InvalidMessageText;

    //방 찾기 팝업 관련
    public GameObject findRoomPopupPrefab;
    private TMP_InputField _roomCodeText;
    private GameObject _findRoomMessegeText;

    //방의 개수 표기
    public TextMeshProUGUI roomCountText;
    public TextMeshProUGUI playerCountText;

    //버튼
    public Button createRoomButton;
    public Button findRoomButton;
    public Button tutorialButton;
    public Button randomMatchingButton;

    // 튜토리얼 관련
    public GameObject tutorialPanel;

    //랜덤매칭 필터
    Hashtable randomFilter = new Hashtable()
    {
        {"isPrivate", false }
    };

    void Start()
    {
        //포톤 네트워크에서 방이 갱신됐다고 호출되면 실행되는 콜백함수
        PhotonManager.Inst.OnRoomListUpdated += UpdateScrollView;

        //씬이 바뀌고 최초 한 번 리스트를 갱신한다.        
        UpdateScrollView(PhotonManager.Inst.GetRoomList());

        //팝업 세팅
        InitPopup();

        if(!GameConfig.isShownTutorial)
        {
            UIManager.Inst.EnableTutorialPopup(true);
            GameConfig.isShownTutorial = true;
        }        
    }
    private void OnDestroy()
    {
        PhotonManager.Inst.OnRoomListUpdated -= UpdateScrollView;
    }

    private void Update()
    {
        //UpdateRoomList(0);
        UpdatePlayerList();

        //ESC 키를 누르면 방 생성, 방 찾기 팝업이 열려있다면 닫는다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelButtonClick();
            AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        }

        //엔터키를 누르면 방 생성, 방 찾기 팝업이 열려있다면 확인버튼을 누른다.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (createRoomPopupPrefab.activeSelf)
            {
                AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);  
                CreateRoomPopupClick();
            }
            else if (findRoomPopupPrefab.activeSelf)
            {
                AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);  
                FindRoomPopupClick();
            }
        }
    }
    private void UpdatePlayerList()
    {
        playerCountText.text = $"로비 인원: {PhotonNetwork.CountOfPlayersOnMaster}";
    }
    private void UpdateRoomList(int count)
    {
        roomCountText.text = $"Rooms : {PhotonNetwork.CountOfRooms}";
    }

    //팝업 관련 컴포넌트 초기화
    private void InitPopup()
    {
        if (createRoomButton != null)
        {
            createRoomButton.GetComponent<Button>().onClick.AddListener(CreateRoomClick);
        }

        if(findRoomButton != null)
        {
            findRoomButton.GetComponent<Button>().onClick.AddListener(FindRoomClick);
        }

        if (tutorialButton != null) {
            tutorialButton.GetComponent<Button>().onClick.AddListener(ShowTutorial);
        }

        if (randomMatchingButton != null)
        {
            randomMatchingButton.GetComponent<Button>().onClick.AddListener(RandomMatchButtonClick);
        }

        //방 생성 팝업 초기화
        if (createRoomPopupPrefab != null)
        {
            createRoomPopupPrefab.SetActive(false);
            //방이름
            Transform temp = createRoomPopupPrefab.transform.Find("Popup/RoomTitle_Container");
            if (temp != null)
            {
                _roomNameText = temp.GetComponent<TMP_InputField>();
                //Debug.Log(temp.GetComponent<TMP_InputField>().text);
            }

            temp = createRoomPopupPrefab.transform.Find("Popup/LimitInput_Container/Dropdown");
            if (temp != null)
            {
                _maxPlayerDropdown = temp.GetComponent<TMP_Dropdown>();
                //Debug.Log(_maxPlayerDropdown.value);
            }

            temp = createRoomPopupPrefab.transform.Find("Popup/Toggle_Check");

            if (temp != null)
            {
                _isPrivateToggle = temp.GetComponent<Toggle>();
                _isPrivateToggle.isOn = false;
            }

            temp = createRoomPopupPrefab.transform.Find("Popup/InvalidMessegeText");

            if (temp != null)
            {
                _InvalidMessageText = temp.GetComponent<TextMeshProUGUI>();
                _InvalidMessageText.gameObject.SetActive(false);
            }

            //버튼 초기화
            temp = createRoomPopupPrefab.transform.Find("Popup/Button_SignIn");
            if (temp != null)
            {
                Button createButton = temp.GetComponent<Button>();
                createButton.onClick.AddListener(CreateRoomPopupClick);
            }
            temp = createRoomPopupPrefab.transform.Find("Popup/Button_Cancel");
            if (temp != null)
            {
                Button cancelButton = temp.GetComponent<Button>();
                cancelButton.onClick.AddListener(CancelButtonClick);
            }
        }

        //방 찾기 팝업 초기화
        if (findRoomPopupPrefab != null)
        {
            findRoomPopupPrefab.SetActive(false);
            Transform temp = findRoomPopupPrefab.transform.Find("Popup/InputField_Container");
            _roomCodeText = temp.GetComponent<TMP_InputField>();

            temp = findRoomPopupPrefab.transform.Find("Popup/ValidCodeText");
            if (temp != null)
            {
                _findRoomMessegeText = temp.gameObject;
                _findRoomMessegeText.SetActive(false);
            }
            //버튼 초기화
            temp = findRoomPopupPrefab.transform.Find("Popup/Button_Find");
            if (temp != null)
            {
                Button createButton = temp.GetComponent<Button>();
                createButton.onClick.AddListener(FindRoomPopupClick);
            }
            temp = findRoomPopupPrefab.transform.Find("Popup/Button_Cancel");
            if (temp != null)
            {
                Button cancelButton = temp.GetComponent<Button>();
                cancelButton.onClick.AddListener(CancelButtonClick);
            }
        }
    }

    //방 리스트의 레코드 객체를 생성
    private GameObject CreateRoomItem(RoomInfo room, int idx)
    {
        GameObject roomItem = Instantiate(roomItemPrefab, contentTransform);
        
        //인덱스 등록
        roomItem.transform.Find("Panel/RoomIdxText").GetComponent<TextMeshProUGUI>().text = "" + idx;
        //roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + idx;
        
        //방 제목 "방 이름#방 코드" 라는 이름을 파싱한다.
        string[] parsedRoomName = GameConfig.ParseString(room.Name);

        //파싱이 안되면 규칙에 어긋난 방으로 생성하지 않는다.
        if (parsedRoomName == null)
        {
            return null;
        }
            
        //방 이름
        roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = parsedRoomName[0];
        
        //카운트
        roomItem.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + " / " + room.MaxPlayers;

        //비공개 여부
        bool isPrivate = false;
        if(room.CustomProperties.ContainsKey("isPrivate"))
        {
            isPrivate = (bool)room.CustomProperties["isPrivate"];
        }

        //자물쇠 아이콘 활성화 여부
        roomItem.transform.GetChild(2).GetChild(0).gameObject.SetActive(isPrivate);

        //RoomItem의 스크립트 내에 변수를 세팅해준다.
        roomItem.GetComponent<RoomItemButton>().roomFullName = room.Name;
        roomItem.GetComponent<RoomItemButton>().roomName = parsedRoomName[0];
        roomItem.GetComponent<RoomItemButton>().roomCode = parsedRoomName[1];
        roomItem.GetComponent<RoomItemButton>().currentRoomCount = room.PlayerCount;
        roomItem.GetComponent<RoomItemButton>().limitedRoomCount = room.MaxPlayers;
        roomItem.GetComponent<RoomItemButton>().isPrivate = isPrivate;

        return roomItem;
    }

    private void UpdateScrollView(Dictionary<string, RoomInfo> updateRoomList)
    {
        //Debug.Log("방 리스트" + updateRoomList.Count);
        //Dictionary<string, RoomInfo> roomList = PhotonManager.Inst.GetRoomList();        

       

        //기존 목록 삭제
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        int index = 1;
        foreach (var room in updateRoomList)
        {
            if (room.Value != null)
            {
                //인원이 0이 아닌곳만 보여주기, [방제목#코드] 방만 보이기
                if (room.Value.PlayerCount != 0 || room.Value.Name.Contains('#'))
                {
                    CreateRoomItem(room.Value, index);
                    index++;
                }
            }
        }
        roomCountText.text = $"대기방 : {index-1}";
    }

    //팝업이 비활성화될 때마다 세팅을 초기화해준다.
    private void ResetPopup()
    {
        //방 만들기
        _roomNameText.text = "";
        _maxPlayerDropdown.value = 0;
        _isPrivateToggle.isOn = false;
        _InvalidMessageText.gameObject.SetActive(false);

        //방찾기
        _roomCodeText.text = "";
        _findRoomMessegeText.SetActive(false);
    } 

    //방 이름 지을때 검증
    private bool CheckRoomName(string roomName)
    {
        if (roomName.Length > GameConfig.maxRoomNameLength)
        {
            _InvalidMessageText.gameObject.SetActive(true);
            _InvalidMessageText.text = "* 방 이름은 20자 이내어야 합니다.";
            return false;
        }

        else if (roomName.Contains('#'))
        {
            _InvalidMessageText.gameObject.SetActive(true);
            _InvalidMessageText.text = "* 방 이름에 '#'은 사용할 수 없습니다.";
            return false;
        }

        return true;
    }

    //방 생성 버튼 클릭
    private void CreateRoomClick()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        ResetPopup();
        createRoomPopupPrefab.SetActive(true);

        //랜덤 방이름을 지어준다. 
        _roomNameText.text = GameConfig.SetRandomRoomName();
    }

    //  #####################버튼 이벤트######################################
    //방 생성 팝업 버튼 클릭
    private void CreateRoomPopupClick()
    {
        if (!CheckRoomName(_roomNameText.text))
        {
            return;
        }
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        //초기값으로 설정        
        int maxPlayer = int.Parse(_maxPlayerDropdown.options[_maxPlayerDropdown.value].text);
        //토글 false상태        
        //방 이름, 인원 수, 비공개 여부
        //Debug.Log(_roomNameText.text + " " + maxPlayer+" "+_isPrivateToggle+"로 방 생성 요청");
        PhotonManager.Inst.CreateRoom(_roomNameText.text, maxPlayer, _isPrivateToggle.isOn);
    }

    //방 찾기 버튼 클릭
    public void FindRoomClick()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        ResetPopup();
        findRoomPopupPrefab.SetActive(true);
    }

    //방 찾기 팝업 버튼 클릭
    private void FindRoomPopupClick()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        //방 코드 검증
        string roomFullName = CheckRoomCode(_roomCodeText.text);
        if (roomFullName != null)
        {
            PhotonManager.Inst.JoinRoom(roomFullName);
        }
    }

    //찾을 시 방 이름을 반환
    private string CheckRoomCode(string code)
    {
        var roomList = PhotonManager.Inst.GetRoomList();
        foreach(RoomInfo r in roomList.Values)
        {
            string[] parsedRoomName = GameConfig.ParseString(r.Name);
            Debug.Log(parsedRoomName[1]+"<-- 방 코드 | 내 코드-->"+code);
            if (parsedRoomName[1] == code)
            {
                return r.Name;
            }
        }

        _findRoomMessegeText.SetActive(true);
        return null;
    }

    //랜덤 매칭
    private void JoinRandomRoom()
    {       
        PhotonNetwork.JoinRandomRoom(randomFilter, 0, MatchmakingMode.RandomMatching, TypedLobby.Default, null, null);
        SceneChanger.Inst.SetLoadingScene();
    }

    private void CancelButtonClick()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        createRoomPopupPrefab.SetActive(false);
        findRoomPopupPrefab.SetActive(false);
        tutorialPanel.SetActive(false);
    }

    private void ShowTutorial() {
        tutorialPanel.SetActive(true);
    }

    private void RandomMatchButtonClick()
    {
        JoinRandomRoom();
    }
}
