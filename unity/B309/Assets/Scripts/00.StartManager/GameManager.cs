using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameManager : Singleton<GameManager>
{
    private GameObject _localPlayer;
    private PhotonView _playerPV;
    private IPunPrefabPool _objPool;
    public HashSet<int> _playersInBush = new HashSet<int>();

    private PhotonView _gameManagerPV;

    private string _skinMaterialPath = "Materials/Cat_Skin/M_Chibi_Cat_";
    public string _faceTexturePath = "Materials/Cat_Face/T_Chibi_Emo_";
    //private string _MagneticObjectPath = "Prefabs/Map/MagneticField";
    private string _MagneticObjectPath = "Prefabs/Map/MagneticField2";


    //플레이어 핸드
    private const string _playerHandSocketPath = "root/pelvis/spine_01/spine_02/spine_03/clavicle_r/upperarm_r/lowerarm_r/hand_r/HandSphere";
    public GameObject _fishInHand; //플레이어가 손에 쥔 무기
    //private Transform _fishPosition; //플레이어 무기를 쥐는 손의 좌표   

    //인게임 관련
    private int _playerRank = 0;

    private Dictionary<string, Player> _playerDic = new Dictionary<string, Player>();
    //private HashSet<string> _playerSet = new HashSet<string>(); //게임 내 플레이어 목록
    private List<string> _playerDeadList = new List<string>(); //리타이어한 플레이어 순으로 올라온다.
    //탈주한 플레이어 순으로 올라온다.
    private List<string> _playerDodgeList = new List<string>();

    private GameObject _magneticField;

    //게임 관련
    public bool isGaming = false;

    private void Start()
    {
        PhotonNetwork.PrefabPool = new FoodObjectPool();
        _objPool = PhotonNetwork.PrefabPool;        
    }

    private void Update()
    {
           
    }

    //플레이어 세팅    
    public void SetPlayer()
    {
        //인게임에 사용될 게임매니저 RPC        
        GameObject gameManagerPV = PhotonNetwork.Instantiate("GameManagerRPC", Vector3.zero, Quaternion.identity);
        //_gameManagerPV = gameManagerPV.GetPhotonView();
        Instantiate(Resources.Load("Prefabs/StatusManager"));

        //인게임에 사용할 플레이어 닉네임 세팅
        var players = PhotonNetwork.CurrentRoom.Players;
        foreach (Player p in players.Values)
        {
            _playerDic.Add(p.NickName, p);
        }

        Transform[] points = GameObject.Find("CreatePlayerGroup").GetComponentsInChildren<Transform>();
        // 로컬 플레이어 생성 및 PhotonView 연결

        int idx = UnityEngine.Random.Range(1, points.Length);
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(GameConfig.playerIdx))
        {
            idx = (int)PhotonNetwork.LocalPlayer.CustomProperties[GameConfig.playerIdx];
        }

        _localPlayer = PhotonNetwork.Instantiate("PlayerCharacter", points[idx].position, points[idx].rotation, 0);
        EnableControlPlayer(false);

        _playerPV = _localPlayer.GetPhotonView();

        // 플레이어 스킨 설정
        Renderer catRenderer = _localPlayer.transform.GetChild(0).GetComponent<Renderer>();
        Material[] materials = catRenderer.materials;
        int skinIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["skinIndex"];
        materials[0] = Resources.Load<Material>(_skinMaterialPath + skinIndex);
        // 플레이어 표정 설정
        int faceIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["faceIndex"];
        Texture newTexture = Resources.Load<Texture>(_faceTexturePath + faceIndex);
        materials[1].SetTexture("_MainTex", newTexture);
        catRenderer.materials = materials;

        //플레이어 색상 설정
        _playerPV.RPC("SetPlayerColorUI", RpcTarget.All, _playerPV.ViewID, PhotonManager.Inst.GetPlayerThemeColorCode());

        //플레이어 무기 설정
        string prefabName = "Weapon#DefaultFish";
        CreateFishWeapon(prefabName);

        // 카메라 생성 및 로컬 플레이어에 할당
        GameObject camera = Instantiate(Resources.Load<GameObject>("Camera"));  // Camera Prefab을 Resources에서 로드하여 생성
        camera.GetComponent<CameraFollow>().SetTarget(_localPlayer.transform);
        if (_playerPV.IsMine)
        {
            _localPlayer.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        //플레이어 수 배치

        int survivorCount = PhotonNetwork.CurrentRoom.MaxPlayers;

        SetPlayerSurviveCount(survivorCount);

        if (InGameUIManager.Inst != null)
        {
            //Debug.Log("인게임 UI 매니저 활성화");
            InGameUIManager.Inst.StartCountdown();
            InGameUIManager.Inst.SetSurvivorText(survivorCount);
        }

        Invoke("ChangeSkin", 2f);
    }

    public void DestroyObj(GameObject obj)
    {
        _objPool.Destroy(obj);
    }

    public void CreateFishWeapon(string prefabName)
    {
        if (_fishInHand != null && _playerPV.IsMine)
        {
            PhotonNetwork.Destroy(_fishInHand);
        }

        string prefabPath = "FishInHand/" + prefabName;

        Transform handTransform = _localPlayer.transform.Find(_playerHandSocketPath);
        _fishInHand = PhotonNetwork.Instantiate(prefabPath, handTransform.position, handTransform.rotation);

        PhotonView pv = _fishInHand.GetPhotonView();

        _playerPV.RPC("SwitchFishWeapon", RpcTarget.All, pv.ViewID, _playerPV.ViewID);
    }



    public GameObject GetPlayer()
    {
        return _localPlayer;
    }

    public string GetWeaponName()
    {
        string[] name = GameConfig.ParseString(_fishInHand.name);
        return name[1];
    }

    public void EnableControlPlayer(bool enable)
    {
        if (_localPlayer != null)
        {
            _localPlayer.GetComponent<PlayerInput>().enabled = enable;
        }
    }

    public void DestroyIngameUIManager()
    {
        Destroy(InGameUIManager.Inst);
    }

    public PhotonView GetPlayerPV()
    {
        return _playerPV;
    }


    public void ClearObjPool()
    {
        //_objPool.ClearObjPool();
        ((FoodObjectPool)PhotonNetwork.PrefabPool).ClearObjPool();
    }

    public void ActivateMagneticField()
    {
        // 자기장 생성, 실행되자마자 축소됨
        if (PhotonNetwork.IsMasterClient)
        {
            if (InGameUIManager.Inst != null)
            {
                InGameUIManager.Inst.SendInGameChatSystemMessage("경고! 자기장이 생성되었습니다!");
            }
            _magneticField = PhotonNetwork.Instantiate(_MagneticObjectPath, new Vector3(0f, 1f, 0f), Quaternion.identity);
        }
    }

    public void DisableMagneticField()
    {
        if (_magneticField != null)
        {
            _magneticField.SetActive(false);
        }
    }

    private void ChangeSkin()
    {
        // 다른 플레이어들 스킨 설정
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            Player player = players[i];
            Debug.Log("현재 플레이어 : " + player);
            foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player"))
            {
                Debug.Log("플레이어의 게임 오브젝트 출력" + playerObject);
                Debug.Log(playerObject.GetComponent<PhotonView>().IsMine);
                if (playerObject.GetComponent<PhotonView>().Owner == player)
                {
                    // 플레이어 스킨 설정
                    Renderer catRenderer = playerObject.transform.GetChild(0).GetComponent<Renderer>();
                    Material[] materials = catRenderer.materials;
                    int skinIndex = (int)player.CustomProperties["skinIndex"];
                    materials[0] = Resources.Load<Material>(_skinMaterialPath + skinIndex);
                    // 플레이어 표정 설정
                    int faceIndex = (int)player.CustomProperties["faceIndex"];
                    Texture newTexture = Resources.Load<Texture>(_faceTexturePath + faceIndex);
                    materials[1].SetTexture("_MainTex", newTexture);
                    catRenderer.materials = materials;
                }
            }
        }
    }

    public void SetPlayerSurviveCount(int num)
    {
        _playerRank = num;
    }

    public void GetPlayerSurviveCount(int num)
    {
        _playerRank = num;
    }
    public void decresePlayerSurviveCount()
    {
        _playerRank--;

        if (InGameUIManager.Inst != null)
        {
            InGameUIManager.Inst.SetSurvivorText(_playerRank);
        }

        Debug.Log("플레이어[ " + _playerRank + " ]명 남음");
        //DebugManager.Inst.SetInfoText("player[ " + _playerRank + " ] live!");
        if (_playerRank == 1)
        {            
            if (PhotonNetwork.LocalPlayer.NickName == PhotonNetwork.MasterClient.NickName)
            {
                //마스터 클라이언트 기준으로 순위를 매긴다.
                List<string> rankList = new List<string>();

                //1등부터 배치

                //스킨코드값
                int skinIndex = 0; // material 깨짐 방지 위해 1을 디폴트로 설정
                int faceIndex = 0;

                if (_playerDic.Count == 1)
                {
                    foreach (Player player in _playerDic.Values)
                    {
                        rankList.Add(player.NickName);
                        skinIndex = (int)player.CustomProperties["skinIndex"];
                        faceIndex = (int)player.CustomProperties["faceIndex"];
                    }
                }
                else
                {                    
                    Debug.Log("1등이 여러명입니다..?");
                }

                //탈락자 배치(역순)                
                for (int i = _playerDeadList.Count - 1; i >= 0; i--)
                {
                    rankList.Add(_playerDeadList[i]);
                }

                //탈주자 배치
                foreach (string pName in _playerDodgeList)
                {
                    rankList.Add(pName + "#Dodge");
                }

                string[] list = rankList.ToArray();

                Debug.Log(skinIndex);
                Debug.Log(faceIndex);

                //방장 플레이어가 죽으면 플레이어 게임 오브젝트가 비활성화 되어 _playerPV 포톤 뷰를 불러오지 못함
                //따라서 게임매니저 RPC를 만듦,
                //또한 RPC는 static이면 안되기 때문에 불가피하게 싱글톤 생성은 포기했다.
                _gameManagerPV.RPC("ActiveGameSetUI", RpcTarget.All, list, skinIndex, faceIndex);
            }
        }
    }

    public void ResetPlayerSettings()
    {
        //Debug.Log("플레이어 모든 세팅 초기화");
        _playerRank = 0;
        _playerDic.Clear();
        _playerDeadList.Clear();
        _playerDodgeList.Clear();
        ClearObjPool();
        PhotonManager.Inst.ResetPlayerProperty();
        _gameManagerPV = null;
        isGaming = false;
    }

    //죽은 순서로 배치되므로 순위를 매길 때 반대로 정렬해서 넣는다.
    public void AddDeadList(Player player)
    {
        Debug.Log("사망!!!");
        foreach (string s in _playerDeadList)
        {
            if (s == player.NickName)
                return;
        }


        _playerDeadList.Add(player.NickName);
        _playerDic.Remove(player.NickName);
        decresePlayerSurviveCount();
    }

    //탈주한 순서대로 추가되므로 랭크 배치 후 순서대로 넣는다.
    public void AddDodgeList(Player player)
    {
        //누군가 onPlayerLeftRoom을 했을 시 탈주리스트에 추가
        //죽었을 시 프로퍼티로 isDead 활성화 
        //죽고 탈주했을 때는 죽은걸로 간주
        if (player.CustomProperties.ContainsKey(GameConfig.isDead))
        {
            return;
        }

        //누군가 킬을 땄을 시 랭크 순위 변경
        _playerDodgeList.Add(player.NickName);
        _playerDic.Remove(player.NickName);
        decresePlayerSurviveCount();
    }

    public void SetGameManagerRPC(PhotonView pv)
    {
        _gameManagerPV = pv;
    }

    public PhotonView GetGameManagerRPC()
    {
        return _gameManagerPV;
    }
}
