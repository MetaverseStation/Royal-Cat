using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon.StructWrapping;
using EpicToonFX;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //싱글톤
    public static PhotonManager Inst { get; private set; }

    public event Action<Dictionary<string, RoomInfo>> OnRoomListUpdated;

    //public TypedLobby sqlLobby = new TypedLobby("MySqlLobby", LobbyType.SqlLobby);
    private Dictionary<string, RoomInfo> _roomDict = new Dictionary<string, RoomInfo>();    

    //서버 지연시간
    private const float _serverWaitingTime = 3f;

    void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Disconnect();
        //}
    }

    public bool GetConnect()
    {
        return PhotonNetwork.IsConnected;
    }
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            //방장이 씬 로딩 시 나머지 사람들은 자동으로 싱크
            PhotonNetwork.AutomaticallySyncScene = true;

            //PhotonNetwork.CleanupPlayerObjects = true;
            PhotonNetwork.GameVersion = GameConfig.GameVersion;

            // 포톤 연결 서버를 kr로 고정하여 한국 서버에만 연결되도록 설정
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";

            //Debug.Log(PhotonNetwork.SendRate); // 포톤서버와 통신 횟수 (30)
            PhotonNetwork.ConnectUsingSettings(); // 서버 접속            
        }
        else
        {
            Debug.Log("이미 연결되어있습니다.");
        }
    }
    public override void OnConnectedToMaster()
    {
        //Debug.Log("Connected To Master!");
        UIManager.Inst.ConnctionFailedPopup(false);
        UIManager.Inst.SetPopup(false);

        if(SceneManager.GetActiveScene().name != GameConfig.titleScene)
        {
            JoinLobby();
        }        
    }
    public void Disconnect() => PhotonNetwork.Disconnect();
        

    public override void OnDisconnected(DisconnectCause cause)
    {        
        if (GameManager.Inst != null)
        {
            SystemManager.Inst.LogoutPlayer();
            GameManager.Inst.ResetPlayerSettings();
            //StartCoroutine(CheckTimeOutConnect());
        }
    }

    private IEnumerator CheckTimeOutConnect()
    {
        yield return new WaitForSeconds(_serverWaitingTime);

        //3초 기다려도 연결 안되면 재접속 메시지 띄우기
        if (SceneManager.GetActiveScene().name != "Title" && UIManager.Inst != null && !PhotonNetwork.IsConnected)
        {            
            UIManager.Inst.ConnctionFailedPopup(true);
        }
    }

    public void ReConnectTry()
    {
        if(Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }

        StartCoroutine(ReConnect());
    }
    private IEnumerator ReConnect()
    {
        Disconnect();

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected &&
        PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Disconnected);
        PhotonNetwork.Reconnect();        
        Debug.Log("재연결됨");

        yield return new WaitForSeconds(3.0f);

        //3초가 지나도 연결 실패시 팝업을 계속 띄움
        if (!PhotonNetwork.IsConnected)
        {
            UIManager.Inst.ConnctionFailedPopup(true);
        }
    }

    //로비관련
    public void JoinLobby()
    {
        if (PhotonNetwork.IsConnected)
        {            
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {        
        StartCoroutine(SceneChanger.Inst.LoadSceneAsync(GameConfig.lobbyScene));
    }

    //방 이름, 인원 수, 비공개 여부
    public void CreateRoom(string roomName = "", int maxPlayers = 6, bool isPrivate = false)
    {
        //Debug.Log(roomName + " " + maxPlayers + " " + isPrivate + "로 방 생성 요청2");

        SceneChanger.Inst.SetLoadingScene();

        if (PhotonNetwork.InLobby)
        {
            RoomOptions ro = new RoomOptions();
            ro.MaxPlayers = maxPlayers;
            ro.IsOpen = true;
            ro.IsVisible = true;
            //ro.CleanupCacheOnLeave = true; //방 삭제 자동화
            ro.EmptyRoomTtl = 0;

            //게임방 시작여부 설정
            ExitGames.Client.Photon.Hashtable roomProp = new ExitGames.Client.Photon.Hashtable();

            //방 초기 인덱스            
            roomProp["mapIdx"] = UnityEngine.Random.Range(0, GameConfig.mapCount);
            roomProp["isPrivate"] = isPrivate;
            ro.CustomRoomProperties = roomProp;

            ro.CustomRoomPropertiesForLobby = new string[] { "isPrivate"};

            //방 코드 생성
            string roomCode = GameConfig.GenerateRoomCode();

            //랜덤 방이름 설정, [방제목+#+방 코드] 형식으로 이름지으며
            //방 이름 지을때 #은 못쓰게 했음
            //* 플레이어는 로비에서 방 제목만 볼 수 있다.
            //PhotonNetwork.CreateRoom("Don't Click" + UnityEngine.Random.Range(1000, 9999), ro);
            PhotonNetwork.CreateRoom(roomName + "#" + roomCode, ro);

        }
    }

    public override void OnCreatedRoom()
    {
        StartCoroutine(SceneChanger.Inst.LoadSceneAsync(GameConfig.roomScene));
    }

    public void JoinRoom(string roomName)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InLobby)
        {
            //방 이름이 규칙과 안맞으면 입장 불가
            if (!roomName.Contains('#'))
            {
                return;
            }
            RoomInfo room = _roomDict[roomName];
            if (room != null)
            {
                //게임중인 방이면 입장 불가
                if (!room.IsOpen || !room.IsVisible)
                {
                    return;
                }
            }
            PhotonNetwork.JoinRoom(roomName);
        }

        SceneChanger.Inst.SetLoadingScene();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        //StartCoroutine(SceneChanger.Inst.LoadSceneAsync("Room"));
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("입장 실패, 로비로 돌아감");
        JoinLobby();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        UIManager.Inst.SetPopupCustum("접속할 방이 없습니다.", true, null, null, "확인", null);
        JoinLobby();
    }

    public void LeaveRoom()
    {
        Debug.Log("방나감 버튼누름");

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("나감");
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnLeftRoom()
    {        
        base.OnLeftRoom();
        GameManager.Inst.ResetPlayerSettings();
        Debug.Log("방 나갔음");
        if (SceneChanger.Inst != null)
        {
            SceneChanger.Inst.SetLoadingScene();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> updatedRoomList)
    {
        //Debug.Log("룸 리스트 업데이트");
        //Debug.Log(updatedRoomList.Count);

        //로비에 접속 시, 새로운 룸이 만들어질 시, 룸이 삭제될 시, 룸의 isOpen의 값이 바뀔 시        

        // 방 리스트 초기화        
        foreach (var room in updatedRoomList)
        {
            //방이 삭제될 시
            if (room.RemovedFromList == true || room.PlayerCount == 0)
            {
                Debug.Log("룸 삭제됨");
                RoomInfo roomItem = null;
                if (_roomDict.TryGetValue(room.Name, out roomItem))
                {
                    _roomDict.Remove(room.Name);
                }
            }
            else
            {
                //방이 처음 생성될 시
                if (_roomDict.ContainsKey(room.Name) == false)
                {
                    Debug.Log("룸 처음 생성됨");
                    _roomDict.Add(room.Name, room);
                }
                //룸 정보가 변경된 경우
                else
                {
                    Debug.Log("룸 정보 변경됨");
                    _roomDict[room.Name] = room;
                }
            }
        }

        //업데이트 됐다고 구독한 스크립트에 이벤트 호출        
        OnRoomListUpdated?.Invoke(_roomDict);
    }

    public void LoadSceneToPhoton(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public Dictionary<string, RoomInfo> GetRoomList()
    {
        return _roomDict;
    }

    public void ClearRoomList()
    {
        _roomDict.Clear();
    }
    

    //닉네임은 고유값이고 중간에 바꾸면 게임에 영향을 끼치니 왠만하면 쓰지 맙시다
    public void SetNickName(string name) => PhotonNetwork.NickName = name;

    //플레이어의 고유정보를 서버에 등록하는 함수
    //value가 T타입이라 디폴트 값 지정 순서가 안맞아서 Key, Value 순이 아닌 Value, Key 순이다.
    public void SetPlayerCustomProperty<T>(T value, string name = null, Player player = null)
    {

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("PhotonNetwork에 연결되지 않아 return합니다.");
            return;
        }

        Player p = player;
        if (p == null)
        {
            p = PhotonNetwork.LocalPlayer;
        }

        ExitGames.Client.Photon.Hashtable prop = new Hashtable
        {
            {name, value }
        };
        p.SetCustomProperties(prop);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {    
        //플레이어 탈주 목록 추가
        if(GameManager.Inst.isGaming)
        {
            GameManager.Inst.AddDodgeList(otherPlayer);
            InGameUIManager.Inst.SendInGameChatSystemMessage("["+otherPlayer.NickName+"]님이 게임을 나갔습니다.");
        }

        // 나간 플레이어가 소유한 모든 PhotonView를 찾아서 삭제
        foreach (var view in FindObjectsOfType<PhotonView>())
        {
            if (view.Owner == otherPlayer)  // 해당 플레이어가 소유한 오브젝트인지 확인
            {
                PhotonNetwork.Destroy(view.gameObject);  // 해당 오브젝트 삭제
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(GameConfig.isInGameLoaded))
        {
            //로딩을 체크하고 모든 플레이어가 로드가 됐을 시 게임을 시작한다.

            var playerList = PhotonNetwork.CurrentRoom.Players;

            int cnt = playerList.Count;
            foreach (Player p in playerList.Values)
            {
                //로딩이 안 된 경우도 있으니 한번 더 체크
                if(p.CustomProperties.ContainsKey(GameConfig.isInGameLoaded))
                {
                    bool isGameLoaded = (bool)p.CustomProperties[GameConfig.isInGameLoaded];

                    if (isGameLoaded)
                    {
                        cnt--;
                    }
                }else
                {
                    break;
                }                
            }
            if (cnt == 0)
            {
                Debug.Log("모든 플레이어 로딩 완료");
                GameManager.Inst.SetPlayer();
            }
        }
        else if (changedProps.ContainsKey(GameConfig.isDead)) //플레이어 사망 시
        {
            //로컬 이벤트로 리스트에
            Debug.Log(targetPlayer.NickName + "사망");
            GameManager.Inst.AddDeadList(targetPlayer);
        }
    }

    //플레이어 프로퍼티 초기화
    public void ResetPlayerProperty()
    {
        Player player = PhotonNetwork.LocalPlayer;

        ExitGames.Client.Photon.Hashtable playerProp = player.CustomProperties;

        List<string> removeKeyList = new List<string>();
        foreach (string key in playerProp.Keys)
        {
          removeKeyList.Add(key);
        }
        string[] removeKeyArr = removeKeyList.ToArray();

        PhotonNetwork.RemovePlayerCustomProperties(removeKeyArr);
    }

    //플레이어의 색상코드값을 불러온다. 인자를 생략할 시 로컬 플레이어의 컬러코드값을 가져온다.
    public string GetPlayerThemeColorCode(Player player = null)
    {
        string colorCode = null;

        Player p = player;

        if (p == null)
        {
            p = PhotonNetwork.LocalPlayer;
        }

        if(p.CustomProperties.ContainsKey(GameConfig.playerColor))
        {
            colorCode = (string)p.CustomProperties[GameConfig.playerColor];
        }

        //색이 없을 시 검은색으로 설정
        if(colorCode == null)
        {
            colorCode = "#000000";
        }

        Debug.Log(colorCode);

        return colorCode;
    }
}
