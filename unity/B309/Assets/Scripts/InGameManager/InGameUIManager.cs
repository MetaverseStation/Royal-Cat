using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameUIManager : MonoBehaviour
{
    //싱글톤    

    //#1. 카운트 다운 및 게임 시작 및 종료를 알리는 패널
    public GameObject messagePanel;
    private TextMeshProUGUI _messageText;
    public GameObject finishPanel;

    //#2. 게임에 필요한 UI를 관리하는 패널(킬로그, 생존자 수, 아이템, 채팅, 타이머, 자기장)
    public GameObject mainPanel;
    public GameObject killLogPanel;
    public GameObject magneticFieldPanel;

    private TextMeshProUGUI _survivorText; //생존로그
    private TextMeshProUGUI _killLogByKill; //킬로그 Kill
    private TextMeshProUGUI _killLogByKilled; //킬로그 Killed

    private InGameChat _inGameChat; //채팅

    private TextMeshProUGUI _timerText; //타이머

    //타이머 관련 변수
    private bool _isTimer = false;
    private float _elapsedTime = 0f;
    private float _minute = 0f;
    private float _second = 0f;
    private float _time = 0f;

    //로딩 기다리는 시간
    private float _waitingLoadingTime = 10f;

    //자기장 관련 (초단위로 입력)
    private float _magneticFieldTime = 90f; //1분30초뒤에 자기장 생성
    private bool _isMagnetic = false;

    //아이템
    private Image _skillItemIcon;
    private Image _weaponItemIcon;

    //#3. Pause 패널
    public GameObject pausePanel;

    //#4. 튜토리얼 패널
    public GameObject tutorialPanel;

    //#5. 게임오버 패널
    public GameObject gameOverPanel;
    private TextMeshProUGUI _rankText;

    //#6. 게임 결과 패널
    public GameObject gameResultPanel;
    //private GameObject _playerListObj;
    public GameObject winnerCharacter;
    private List<GameObject> _playerListObj = new List<GameObject>();
    public Camera UICamera;

    //public GameObject gameResult;
    //public GameObject winnerPosition;
    //private GameObject _characterPrefab;

    //#7. 로딩 패널
    public GameObject loadingPanel;

    public static InGameUIManager Inst { get; private set; }

    //이 인게임 UI 매니저는 씬이 전환되면 파괴되야 하기 때문에 싱글톤으로만 선언하되 DontDestroy를 할당하지 않음
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject); // 인스턴스가 이미 존재하면 중복 생성 방지
        }
    }

    //기본세팅이라 안읽어봐도됨
    void Init()
    {
        //메시지 패널
        _messageText = messagePanel.transform.Find("MessageText").GetComponent<TextMeshProUGUI>();
        _messageText.text = "Loading..";
        //인게임 
        _survivorText = mainPanel.transform.Find("SurvivorPanel/SurvivorText").GetComponent<TextMeshProUGUI>();
        _killLogByKill = mainPanel.transform.Find("KillLogPanel/Kill").GetComponent<TextMeshProUGUI>();
        _killLogByKill.text = "";
        _killLogByKilled = mainPanel.transform.Find("KillLogPanel/Killed").GetComponent<TextMeshProUGUI>();
        _killLogByKilled.text = "";
        _inGameChat = mainPanel.transform.Find("ChattingBoxIngame").GetComponent<InGameChat>();
        _timerText = mainPanel.transform.Find("InGameTimer/Text_Timer").GetComponent<TextMeshProUGUI>();

        //아이템
        _skillItemIcon = mainPanel.transform.Find("Item/SkillItem/SkillItemIcon").GetComponent<Image>();
        _weaponItemIcon = mainPanel.transform.Find("Item/WeaponItem/WeaponItemIcon").GetComponent<Image>();

        //Pause 버튼 세팅
        Button resumeButton = pausePanel.transform.Find("Popup/Button_Group/Button_Resume").GetComponent<Button>();
        resumeButton.onClick.AddListener(resumeButtonClicked);
        Button settingButton = pausePanel.transform.Find("Popup/Button_Group/Button_Setting").GetComponent<Button>();
        settingButton.onClick.AddListener(settingButtonClicked);
        Button exitButton = pausePanel.transform.Find("Popup/Button_Group/Button_Exit").GetComponent<Button>();
        exitButton.onClick.AddListener(exitRoomButtonClicked);
        Button quitButton = pausePanel.transform.Find("Popup/Button_Group/Button_Quit").GetComponent<Button>();
        quitButton.onClick.AddListener(quitGameButtonClicked);

        //게임오버 패널 설정
        _rankText = gameOverPanel.transform.Find("RankText").GetComponent<TextMeshProUGUI>();
        Button quitButton2 = gameOverPanel.transform.Find("ExitButton").GetComponent<Button>();
        quitButton2.onClick.AddListener(exitRoomButtonClicked);

        //게임 결과 패널 설정
        Transform tr = gameResultPanel.transform.Find("ResultBackground/PlayersList");
        foreach (Transform child in tr)
        {
            _playerListObj.Add(child.gameObject);
        }

        Button exitButton2 = gameResultPanel.transform.Find("ExitButton").GetComponent<Button>();
        exitButton2.onClick.AddListener(OnClickResultExitButton);

        //초기화
        messagePanel.SetActive(false);
        mainPanel.SetActive(false);
        pausePanel.SetActive(false);
        tutorialPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameResultPanel.SetActive(false);
        loadingPanel.SetActive(true);
        magneticFieldPanel.SetActive(false);
        killLogPanel.SetActive(false);
    }

    void Start()
    {
        Init();
        _time = 0f;
        _isMagnetic = false;
        //플레이어의 움직임을 제한
        GameManager.Inst.EnableControlPlayer(false);

        //오랫동안 시작되지 않으면 에러띄움
        StartCoroutine(CheckTimeOver());
    }

    //
    IEnumerator CheckTimeOver()
    {
        yield return new WaitForSeconds(_waitingLoadingTime);
        
        if (!GameManager.Inst.isGaming)
        {
            UIManager.Inst.ConnctionFailedPopup(true);
        }
    }

    public void StartCountdown()
    {
        GameManager.Inst.isGaming = true;
        StartCoroutine(CountdownStart());
    }

    void Update()
    {
        UpdateTimer();

        //Pause창 활성화
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pausePanel.SetActive(!pausePanel.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }

    IEnumerator CountdownStart()
    {
        loadingPanel.SetActive(false);
        messagePanel.SetActive(true);

        for (int i = 3; i > 0; i--)
        {
            _messageText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        _messageText.text = "Game Start!";

        yield return new WaitForSeconds(1f);
        ActiveGame();
    }

    //플레이어 움직일 수 있음
    public void ActiveGame()
    {
        //초기화
        GameManager.Inst.EnableControlPlayer(true);
        messagePanel.SetActive(false);
        loadingPanel.SetActive(false);
        mainPanel.SetActive(true);
        EnableTimer(true);
        SendInGameChatSystemMessage("게임 시작!");
    }


    //버튼 동작 관련
    void resumeButtonClicked()
    {
        pausePanel.SetActive(false);
    }

    void settingButtonClicked()
    {        
        UIManager.Inst.EnableSettingPopup(true);        
    }

    //로비로 돌아가기
    void exitRoomButtonClicked()
    {
        UIManager.Inst.SetExitPopup();
    }

    //게임 종료하기
    void quitGameButtonClicked()
    {
        UIManager.Inst.SetQuitGamePopup();
    }

    void EnableTimer(bool enable)
    {
        _isTimer = enable;
    }

    void UpdateTimer()
    {
        if (_isTimer)
        {
            _elapsedTime += Time.deltaTime;
            _time += Time.deltaTime;
            if (_elapsedTime >= 1f)
            {
                _elapsedTime = 0f;
                _second += 1f;
                if (_second >= 60)
                {
                    _second = 0f;
                    _minute += 1f;
                }
                _timerText.text = _minute.ToString("00") + " : " + _second.ToString("00");

                if (!_isMagnetic && (_time > _magneticFieldTime))
                {
                    _isMagnetic = true;
                    GameManager.Inst.ActivateMagneticField();
                    StartCoroutine(ShowMagneticFieldPanel(3.0f));
                }
            }
        }
    }

    private IEnumerator ShowMagneticFieldPanel(float delay) {
        magneticFieldPanel.SetActive(true);
        yield return new WaitForSeconds(delay);
        magneticFieldPanel.SetActive(false);
    }
    //private void SetLoadingCharacter()
    //{
    //    int idx = UnityEngine.Random.Range(0, 9);
    //    _characterPrefab = Resources.Load<GameObject>("Prefabs/Loading/Prefab/Characters/Chibi_Cat_0" + idx);

    //    Instantiate(_characterPrefab, winnerPosition.transform);
    //    _characterPrefab.transform.localScale = new Vector3(100f, 100f, 100f);

    //    // Camera.main.GetComponent<CameraFollow>().SetTarget(_characterPrefab.transform);

    //}

    public void SetWeaponItemUI(WeaponType weaponType)
    {

        Debug.Log("투사체 아이템 획득");

        string path = "UI/Item Img/" + weaponType;
        Sprite newSprite = Resources.Load<Sprite>(path); // 경로: Resources/Prefabs/UI/ItemImg/Skill_arc
        _weaponItemIcon.sprite = newSprite;

    }

    // 스킬 아이템 UI 변경
    public void SetSkillItemUI(SkillType skillType)
    {

        Debug.Log("스킬 아이템 획득");

        string path = "UI/Item Img/" + skillType;
        Sprite newSprite = Resources.Load<Sprite>(path); // 경로: Resources/Prefabs/UI/ItemImg/Skill_arc
        _skillItemIcon.sprite = newSprite;
    }

    public void ShowKillLog(string kill, string killed, string message)
    {
        killLogPanel.SetActive(true);
        _killLogByKill.text = kill;  // 킬 로그 표시
        _killLogByKilled.text = killed;
        SendInGameChatSystemMessage(message);
        StartCoroutine(ClearKillLogAfterDelay(3f));  // 3초 후에 로그 지우기
    }

    IEnumerator ClearKillLogAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);  // 3초 기다림
        _killLogByKill.text = "";  // 킬 로그 지우기
        _killLogByKilled.text = "";  // 킬 로그 지우기
        killLogPanel.SetActive(false);
    }

    public void SetGameOverPanel(bool enable)
    {
        gameOverPanel.SetActive(enable);
        
        //어뷰징 방지를 위한 채팅 비활성화
        _inGameChat.gameObject.SetActive(!enable);

        Transform itemPanal = mainPanel.gameObject.transform.Find("Item");        
        itemPanal.gameObject.SetActive(!enable);        
    }


    public IEnumerator GameSetEvent(string[] rankList, int skinIndex, int faceIndex)
    {
        // _messageText.text = "FINISH!";
        // messagePanel.SetActive(true);
        finishPanel.SetActive(true);

        //초기화
        GameManager.Inst.EnableControlPlayer(false);
        EnableTimer(false);

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.5f);
        
        GameManager.Inst.DisableMagneticField();
        
        messagePanel.SetActive(false);
        mainPanel.SetActive(false);
        finishPanel.SetActive(false);
        
        //게임 결과창 세팅
        gameOverPanel.SetActive(false);
        SetGameResultScreen(rankList, skinIndex, faceIndex);
        //      
        gameResultPanel.SetActive(true);
    }

    public void SetSurvivorText(int num)
    {
        _survivorText.text = "" + num;
    }

    public void SendInGameChatSystemMessage(string text)
    {
        _inGameChat.SendSystemMessage(text);
    }

    //결과 받은걸 토대로 결과창을 갱신한다.
    private void SetGameResultScreen(string[] rankList, int skinIndex, int faceIndex)
    {
        StartCoroutine(CameraAfterGameOver());

        Transform cameraTransform = UICamera.transform;

        GameObject winner = Instantiate(winnerCharacter, cameraTransform);
        winner.transform.position += new Vector3(-1.8f, -1f, 3.65f);
        winner.transform.LookAt(cameraTransform);
        winner.transform.rotation *= Quaternion.Euler(18, 0, 0);

        Animator winnerAnimator = winner.GetComponent<Animator>();
        winnerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        winnerAnimator.Play("Victory");

        // 우승자 스킨 입히기
        Renderer winnerRenderer = winner.transform.GetChild(0).GetComponent<Renderer>();
        Material[] materials = winnerRenderer.materials;

        Material newMaterial = Resources.Load<Material>("Materials/Cat_Skin/M_Chibi_Cat_" + skinIndex);
        Texture newTexture = Resources.Load<Texture>("Materials/Cat_Face/T_Chibi_Emo_" + faceIndex);

        materials[0] = newMaterial;
        materials[1].SetTexture("_MainTex", newTexture);

        winnerRenderer.materials = materials;

        GameObject obj = null;
        for (int i = 0; i < rankList.Length; i++)
        {
            obj = _playerListObj[i];

            obj.SetActive(true);

            //왕관 달아줌
            if (i == 0)
            {
                obj.transform.Find("Icon").gameObject.SetActive(true);
                obj.transform.Find("Text_Ranking_Num").gameObject.SetActive(false);
            }
            else
            {
                obj.transform.Find("Icon").gameObject.SetActive(false);
                obj.transform.Find("Text_Ranking_Num").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                obj.transform.Find("Text_Ranking_Num").gameObject.SetActive(true);
            }

            //탈주 여부
            string[] parseName = GameConfig.ParseString(rankList[i]);
            if (parseName != null)
            {
                obj.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>().text = parseName[0];
                obj.transform.Find("Text_Ranking_Num").GetComponent<TextMeshProUGUI>().text = "탈주";
                obj.transform.Find("Text_Ranking_Num").GetComponent<TextMeshProUGUI>().color = Color.red;
            }
            else
            {
                obj.transform.Find("Text_UserName").gameObject.SetActive(true);
                obj.transform.Find("Text_UserName").GetComponent<TextMeshProUGUI>().text = rankList[i];
            }
        }
    }

    IEnumerator CameraAfterGameOver()
    {
        GameObject playerCamera = Camera.main.gameObject;

        playerCamera.SetActive(false);

        UICamera.gameObject.SetActive(true);

        yield return null;
    }

    private void OnClickResultExitButton()
    {
        Time.timeScale = 1f;
        PhotonManager.Inst.LeaveRoom();
    }
}