//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using System.Linq;
using ExitGames.Client.Photon;
using System.Runtime.CompilerServices;
using UnityEngine.Analytics;
using Photon.Pun.Demo.Cockpit;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public GameObject playerListPanel;

    public GameObject playerCardPrefab;
    //public List<GameObject> playerCardList = new List<GameObject>();

    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI roomCodeText;

    //내 화면에 보이는 레디 버튼
    public Button readyButton;
    public Button exitButton;

    private GameObject _playerReadyPanel;
    private Player _myPlayer;
    private bool _myPlayerReady = false;

    private RoomChat _roomChat;

    public GameObject[] _characterList;
    public GameObject _characters;
    private SelectedCharacter _selectedCharacter;
    public bool[] isSelectedSkin;
    private List<int> _selectedSkinList = new List<int>(); // 선택된 스킨 리스트
    private bool _isCharacterInit;
    private int _maxFace = 27; // 27개

    private int _roomLimit;
    public int _mapIndex;

    //현재 방
    private Room room;

    [Header("SelectMap")]
    public string[] mapScenes;
    public SelectedMap selectedMap;

    void Start()
    {
        _myPlayer = PhotonNetwork.LocalPlayer;

        Init();

        //방 생성 때 설정한 최대 인원 수 배치            
        _roomLimit = PhotonNetwork.CurrentRoom.MaxPlayers;
        _mapIndex = 0;

        //해당 클라이언트의 플레이어의 레디 관련 프로퍼티 생성        
        PhotonManager.Inst.SetPlayerCustomProperty<bool>(_myPlayer.IsMasterClient, GameConfig.isReady);

        // 스킨 오브젝트 가져오기
        //_characters = GameObject.Find("CharacterList");
        //_characterList = new GameObject[_roomLimit];
        _selectedCharacter = GameObject.Find("CatSkin").GetComponent<SelectedCharacter>();
        //for (int i = 0; i < _roomLimit; i++)
        //{
        //    _characterList[i] = _characters.transform.GetChild(i).gameObject;
        //}

        //플레이어가 마스터여부에 따라 버튼 네임 변경
        SetButtonType();

        //플레이어 리스트 갱신
        UpdatePlayerList();

        _roomChat = FindObjectOfType<RoomChat>();

        //맵 한번 세팅
        room = PhotonNetwork.CurrentRoom;

        _mapIndex = (int)room.CustomProperties["mapIdx"];

        selectedMap.UpdateMap(_mapIndex);
    }

    private void Update()
    {
        //#########디버그용
        //if (Input.GetKeyDown(KeyCode.F8))
        //{
        //    if (_myPlayer.IsMasterClient)
        //    {
        //        GameStart();

        //        //1명만 플레이 시 예외로 결과창 안뜨게 함
        //        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        //        {
        //            GameManager.Inst.SetPlayerSurviveCount(-1);
        //        }
        //    }
        //    else
        //        UIManager.Inst.SetInformationPopup("방장이 레디하세요");
        //}
        //################

        if (Input.GetKeyDown(KeyCode.F5))
        {
            StartButtonClicked();
        }

        if (!_roomChat.GetInputActive())
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _selectedCharacter.Prev();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _selectedCharacter.Next();
            }
        }
        InitSkin();
    }

    private void Init()
    {
        string[] parsedRoomName = GameConfig.ParseString(PhotonNetwork.CurrentRoom.Name);

        if (roomNameText != null && parsedRoomName != null)
        {
            roomNameText.text = parsedRoomName[0];
        }

        if (roomCodeText != null && parsedRoomName != null)
        {
            roomCodeText.text = parsedRoomName[1];
        }

        if (readyButton != null)
        {
            readyButton.GetComponent<Button>().onClick.AddListener(StartButtonClicked);
        }
        if (exitButton != null)
        {
            exitButton.GetComponent<Button>().onClick.AddListener(ExitButtonClicked);
        }
    }

    private void UpdatePlayerList()
    {
        //Debug.Log("플레이어리스트 업데이트");
        //포톤 내 플레이어 목록
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null)
        {
            //플레이어를 입장한 순서대로 정렬
            var sortedPlayers = PhotonNetwork.CurrentRoom.Players.Values.OrderBy(p => p.ActorNumber).ToList();

            //시스템 최대 인원
            int maxPlayers = GameConfig.MaxPlayersInRoom;

            //기존 목록 삭제
            //foreach (Transform child in playerListPanel.transform)
            //{
            //    child.gameObject.SetActive(false);
            //}

            // 캐릭터 리스트 초기화
            foreach (GameObject n in _characterList)
            {
                n.SetActive(false);
            }

            CheckSkin();

            //플레이어 카드 세팅
            for (int i = 0; i < maxPlayers; i++)
            {
                //플레이어 카드 프리팹 생성
                GameObject playerCard = playerListPanel.transform.GetChild(i).gameObject;
                CreatePlayerCard(playerCard, i, i >= _roomLimit);

                if (i < sortedPlayers.Count)
                {
                    Player player = sortedPlayers[i];

                    //플레이어 네임 세팅
                    Transform playerNameText = playerCard.transform.Find("PlayerPanel/NameText");
                    playerNameText.GetComponent<TextMeshProUGUI>().text = player.NickName;

                    //배경에 컬러 부여
                    GameConfig.PlayerColor color = (GameConfig.PlayerColor)i;
                    Transform backgroundColor = playerCard.transform.Find("PlayerPanel");
                    backgroundColor.GetComponent<Image>().color = GameConfig.GetPlayerColor(color);

                    // 플레이어 스킨 세팅
                    _characterList[i].SetActive(true);

                    Renderer catRenderer = _characterList[i].transform.GetChild(0).GetComponent<Renderer>();
                    Material[] materials = catRenderer.materials;

                    if (player.CustomProperties["skinIndex"] != null)
                    {

                        int skinIndex = (int)player.CustomProperties["skinIndex"];
                        //Debug.Log(i + "번 캐릭터의 스킨이" + skinIndex + "로 설정됨");
                        materials[0] = Resources.Load<Material>(_selectedCharacter.skinMaterialPath + skinIndex);
                    }

                    if (player.CustomProperties["faceIndex"] != null)
                    {
                        int faceIndex = (int)player.CustomProperties["faceIndex"];
                        //Debug.Log("업데이트 되는 faceIndex" + faceIndex);
                        Texture newTexture = Resources.Load<Texture>(_selectedCharacter.faceTexturePath + faceIndex);
                        materials[1].SetTexture("_MainTex", newTexture);
                    }

                    catRenderer.materials = materials;

                    //플레이어 마스터, 레디 여부 세팅
                    //TODO: 플레이어 레디 여부에 따라 프로퍼티 정보 갱신
                    Transform playerReadyPanel = playerCard.transform.Find("ReadyPanel");
                    Transform playerReadyText = playerCard.transform.Find("ReadyPanel/ReadyText");

                    //해당 플레이어가 자신이라면 레디의 오브젝트를 갖는다. 레디 온 오프 기능을 하기 위해
                    if (player == _myPlayer)
                    {
                        _playerReadyPanel = playerReadyPanel.gameObject;

                        //또한 자신을 식별하기 위한 아웃라인을 가진다
                        Transform playerOutLine = playerCard.transform.Find("OutLinePanel");
                        playerOutLine.gameObject.SetActive(true);

                        //플레이어의 테마색 부여
                        Color c = GameConfig.GetPlayerColor(color);
                        string colorCode = ColorUtility.ToHtmlStringRGB(c);
                        PhotonManager.Inst.SetPlayerCustomProperty<string>(colorCode, GameConfig.playerColor);
                    }

                    //해당 플레이어의 레디 여부를 가져온다.
                    bool isReady = false;
                    if (player.CustomProperties[GameConfig.isReady] != null)
                    {
                        isReady = (bool)player.CustomProperties[GameConfig.isReady];
                    }

                    //레디 여부를 활성화한다.
                    SetReadyUI(playerReadyPanel.gameObject, isReady);

                    if (player.IsMasterClient)
                    {
                        //Debug.Log(player.ActorNumber +"num"+ player.NickName + " is a master");                                                
                        playerReadyPanel.gameObject.SetActive(true);
                        playerReadyText.GetComponent<TextMeshProUGUI>().text = "MASTER";
                    }
                    else
                    {
                        playerReadyText.GetComponent<TextMeshProUGUI>().text = "READY";
                    }

                    //플레이어 본인에 해당한다면 해당 레디패널에 권한 부여
                }
                else
                {
                    Transform playerNameText = playerCard.transform.Find("PlayerPanel/NameText");
                    playerNameText.GetComponent<TextMeshProUGUI>().text = "";
                }
            }
        }
    }

    public void ExitButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        PhotonManager.Inst.ResetPlayerProperty();

        //PhotonManager.Inst.SetPlayerCustomProperty<bool>(false, GameConfig.isReady);
        //PhotonManager.Inst.SetPlayerCustomProperty<int>(0, "skinIndex");
        PhotonManager.Inst.LeaveRoom();
    }
    public override void OnJoinedRoom()
    {
        //본인의 정보 저장
        Debug.Log("방 입장!" + _myPlayer.IsMasterClient);

        _selectedSkinList = new List<int>();
        _isCharacterInit = false;

        CheckSkin();
        Debug.Log("onJoinedRoom에서 호출됨");
        InitSkin();
    }

    public void StartButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom && _myPlayer.IsMasterClient)
        {
            if (CheckGameStart())
            {
                Debug.Log("게임시작, 여기에 로딩 페이지 구현");
                GameStart();
            }
        }
        else
        {
            //플레이어 레디
            _myPlayerReady = !_myPlayerReady;
            SetReadyUI(_playerReadyPanel, _myPlayerReady);
            PhotonManager.Inst.SetPlayerCustomProperty<bool>(_myPlayerReady, GameConfig.isReady);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 새로운 플레이어가 방에 들어왔을 때 플레이어 목록 업데이트
        Debug.Log("새로운 플레이어 입장");
        UpdatePlayerList();

        _roomChat.SetMessageLocal("[" + newPlayer.NickName + "] 님이 입장했습니다.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // 플레이어가 방을 떠났을 때 플레이어 목록 업데이트 이거 작동하는지 모르겠음

        UpdatePlayerList();

        _roomChat.SetMessageLocal("[" + otherPlayer.NickName + "] 님이 퇴장했습니다.");
    }

    //플레이어 누군가의 프로퍼티가 바뀌었을때 업데이트
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Debug.Log("업데이트 함수 호출됨");
        if (changedProps.ContainsKey(GameConfig.isReady))
        {
            bool isReady = (bool)changedProps[GameConfig.isReady];
            //Debug.Log(targetPlayer.NickName + "이 ready건드림");
            //Dictionary<int, Player> p = PhotonNetwork.CurrentRoom.Players;
            UpdatePlayerList();
        }

        // 스킨 설정 값 업데이트
        if (changedProps.ContainsKey("skinIndex"))
        {
            //Debug.Log("스킨 설정값이 업데이트됨");
            int skinIndex = (int)changedProps["skinIndex"];
            UpdatePlayerList();
        }

        // 얼굴 설정 값 업데이트
        if (changedProps.ContainsKey("faceIndex"))
        {
            int faceIndex = (int)changedProps["faceIndex"];
            UpdatePlayerList();
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {        
        if (propertiesThatChanged.ContainsKey("mapIdx"))
        {            
            int idx = (int)propertiesThatChanged["mapIdx"];
            if (!_myPlayer.IsMasterClient)
            {
                SelectMap(idx);
            }
            string[] mapName = mapScenes[idx].Split('_');
            _roomChat.SetMessageLocal("[" + mapName[1] + "]" + "맵으로 변경되었습니다. ");
        }
    }

    //플레이어가 방장이 됐을때
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log(newMasterClient.NickName + "이 방장됨");
        if (_myPlayer == newMasterClient)
        {
            PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isReady);
            SetButtonType();

            SelectMap((int)room.CustomProperties["mapIdx"]);
        }
        _roomChat.SetMessageLocal("[" + newMasterClient.NickName + "] 님이 방장이 되었습니다..");
    }

    private GameObject CreatePlayerCard(GameObject playerItem, int avatarIdx, bool isClosed)
    {        
        GameObject playerPanel = playerItem.transform.Find("PlayerPanel").gameObject;
        GameObject closedPanel = playerItem.transform.Find("ClosedPanel").gameObject;
        
        if (isClosed == false)
        {
            playerPanel.SetActive(true);
            closedPanel.SetActive(false);
        }
        //닫힌 패널
        else
        {
            playerPanel.SetActive(false);
            closedPanel.SetActive(true);
            //_characterList[avatarIdx].SetActive(false);
        }
        return playerItem;
    }

    private void SetReadyUI(GameObject playerReadyPanel, bool enable)
    {
        //playerCard.transform.Find("PlayerPanel/ReadyPanel").gameObject.SetActive(enable);
        playerReadyPanel.SetActive(enable);
    }

    //플레이어의 레디 여부 체크
    private bool CheckGameStart()
    {
        int roomLimit = PhotonNetwork.CurrentRoom.MaxPlayers;
        int readyCount = 0;
        var playerList = PhotonNetwork.CurrentRoom.Players;
        int playerListCount = playerList.Count;

        if (playerListCount < roomLimit)
        {
            UIManager.Inst.SetInformationPopup("모든 플레이어가 입장해야합니다");
            return false;
        }

        foreach (Player p in playerList.Values)
        {
            bool isReady = false;

            if (p.CustomProperties[GameConfig.isReady] != null)
            {
                isReady = (bool)p.CustomProperties[GameConfig.isReady];
            }
            if (isReady)
            {
                readyCount++;
            }
        }

        if (readyCount < roomLimit)
        {
            UIManager.Inst.SetInformationPopup("모든 플레이어가 준비해야합니다");
            return false;
        }

        return true;
    }
    private void GameStart()
    {
        //게임이 시작된 방은 로비 목록에 보이지 않음

        PlayerShuffleIdx();

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // 인덱스에 맞는 Scene을 로드
        if (_mapIndex >= 0 && _mapIndex < mapScenes.Length)
        {
            StartCoroutine(SceneChanger.Inst.LoadSceneAsync(GameConfig.loadingScene));
            PhotonManager.Inst.LoadSceneToPhoton(mapScenes[_mapIndex]);  // 선택된 맵으로 씬 이동
        }
        else
        {
            Debug.LogError("잘못된 맵 인덱스입니다.");
        }
    }

    //랜덤 위치 지정해주기 위한값
    private void PlayerShuffleIdx()
    {
        List<int> shuffleList = GameConfig.GetShuffledList(1, GameConfig.MaxPlayersInRoom);
        var player = PhotonNetwork.CurrentRoom.Players;

        int i = 0;
        foreach (Player p in player.Values)
        {
            PhotonManager.Inst.SetPlayerCustomProperty<int>(shuffleList[i++], GameConfig.playerIdx, p);
            Debug.Log("할당된 번호" + shuffleList[i - 1]);
        }
    }

    private void SetButtonType()
    {
        TextMeshProUGUI text = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (_myPlayer.IsMasterClient)
        {
            text.text = "START!";
        }
        else
        {
            text.text = "READY";
        }
    }

    public void CheckSkin(List<Player> playerList = null)
    {
        //Debug.Log("checkskin 호출됨" + playerList);
        if (playerList == null)
        {
            playerList = PhotonNetwork.CurrentRoom.Players.Values.ToList();
        }

        //Debug.Log("isSelectedSkin 초기화함");
        isSelectedSkin = new bool[11];

        foreach (Player player in playerList)
        {
            if (player.CustomProperties.ContainsKey("skinIndex") && player.CustomProperties["skinIndex"] != null)
            {
                int skinIndex = (int)player.CustomProperties["skinIndex"];
                isSelectedSkin[skinIndex] = true;
                _selectedSkinList.Add(skinIndex);
            }
        }
    }

    public void InitSkin()
    {
        if (_isCharacterInit)
        {
            return;
        }

        CheckSkin();

        for (int i = 1; i <= 10; i++)
        {
            if (!isSelectedSkin[i])
            {
                //Debug.Log(i + "번으로 스킨 설정");


                // 초기 표정 설정
                int faceIndex = UnityEngine.Random.Range(1, _maxFace);

                PhotonManager.Inst.SetPlayerCustomProperty<int>(i, "skinIndex");
                PhotonManager.Inst.SetPlayerCustomProperty<int>(faceIndex, "faceIndex");

                //Debug.Log(i + "로 스킨 인덱스 초기화");
                _selectedCharacter.ChangeSkin(i, faceIndex);

                _isCharacterInit = true;
                break;
            }
        }
    }

    public void SelectMap(int newMapIndex)
    {
        if (newMapIndex >= 0 && newMapIndex < mapScenes.Length)
        {
            _mapIndex = newMapIndex;

            if (_myPlayer.IsMasterClient)
            {
                Hashtable propUpdate = new Hashtable();
                propUpdate["mapIdx"] = _mapIndex;

                room.SetCustomProperties(propUpdate);
            }            
            selectedMap.UpdateMap(_mapIndex);
        }
        else
        {
            Debug.LogError("잘못된 맵 인덱스입니다.");
        }

    }
}
