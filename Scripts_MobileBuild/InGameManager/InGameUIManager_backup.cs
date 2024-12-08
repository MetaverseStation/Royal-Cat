//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using System;
//using UnityEngine.UI;
//using Photon.Pun.UtilityScripts;
//using Photon.Pun;

//public class InGameUIManager : MonoBehaviour
//{
//    public static InGameUIManager instance;
//    public TextMeshProUGUI countdownDisplay;
//    private PlayerInput _playerInput;
//    private PlayerHealth _playerHealth;
//    public TextMeshProUGUI timerText;
//    public GameObject countdownBackground;
//    public Button exitButton;
//    public GameObject gameResult;
//    public GameObject winnerPosition;
//    private GameObject _characterPrefab;
//    public GameObject pausePanel;
//    public Button resumeButton;
//    public GameObject timerPanel;
//    public GameObject itemPanel;
//    public GameObject tutorialPanel;
//    public GameObject chattingBox;

//    private float _elapsedTime = 0f;
//    private float _minute = 0f;
//    private float _second = 0f;
//    private bool _isGameStarted = false;
//    private bool _isGameFinished = false;
//    public KeyCode toggleKey = KeyCode.Escape;
//    private bool _isVisible;

//    void Start()
//    {
//        // 싱글턴 패턴 설정
//        // 
//        if (instance == null)
//        {
//            instance = this;
//        }
//        else
//        {
//            Destroy(this);
//        }

//        GameObject inputObject = GameObject.FindWithTag("Player");
//        _playerInput = inputObject.GetComponent<PlayerInput>();
//        _playerHealth = inputObject.GetComponent<PlayerHealth>();

//        _playerInput.enabled = false;
//        SetCountdown();
//        _isVisible = pausePanel.activeSelf;
//        resumeButton.onClick.AddListener(HidePanel);
//    }

//    void Update()
//    {

//        if (!_isGameStarted)
//        {
//            return;
//        }

//        PushESC();
//        PushF1();

//        _elapsedTime += Time.deltaTime;

//        if (_elapsedTime >= 1f)
//        {
//            _elapsedTime = 0f;
//            _second += 1f;
//            if (_second >= 60)
//            {
//                _second = 0f;
//                _minute += 1f;
//            }
//            timerText.text = _minute.ToString("00") + " : " + _second.ToString("00");
//        }

//        if (!_playerHealth && !_isGameFinished)
//        {
//            exitButton.gameObject.SetActive(true);
//            _isGameFinished = true;
//        }

//    }

//    // 로비로 나가기    
//    public void ExitGame()
//    {
//        UIManager.Inst.SetExitPopup();
//    }

//    // 게임 종료
//    public void QuitGame()
//    {
//        UIManager.Inst.SetQuitGamePopup();
//    }


//    public void HidePanel()
//    {
//        _isVisible = !_isVisible;
//        timerPanel.SetActive(!_isVisible);
//        itemPanel.SetActive(!_isVisible);
//        pausePanel.SetActive(_isVisible);
//    }

//    public void PushESC()
//    {
//        if (Input.GetKeyDown(toggleKey))
//        {
//            _isVisible = !_isVisible;
//            timerPanel.SetActive(!_isVisible);
//            itemPanel.SetActive(!_isVisible);
//            chattingBox.SetActive(!_isVisible);
//            pausePanel.SetActive(_isVisible);
//        }
//    }

//    public void PushF1()
//    {
//        if (Input.GetKey(KeyCode.F1) && !_isVisible)
//        {
//            timerPanel.SetActive(false);
//            itemPanel.SetActive(false);
//            chattingBox.SetActive(false);
//            tutorialPanel.SetActive(true);
//        }
//        if (Input.GetKeyUp(KeyCode.F1) && !_isVisible)
//        {
//            timerPanel.SetActive(true);
//            itemPanel.SetActive(true);
//            chattingBox.SetActive(true);
//            tutorialPanel.SetActive(false);
//        }
//    }

//    public void SetCountdown()
//    {
//        StartCoroutine(CountdownStart());
//    }

//    IEnumerator CountdownStart()
//    {
//        countdownBackground.SetActive(true);
//        countdownDisplay.gameObject.SetActive(true);
//        for (int i = 3; i > 0; i--)
//        {
//            countdownDisplay.text = i.ToString();
//            yield return new WaitForSeconds(1f);
//        }
//        countdownDisplay.text = "Game Start";
//        yield return new WaitForSeconds(1f);
//        countdownDisplay.gameObject.SetActive(false);
//        countdownBackground.SetActive(false);
//        _playerInput.enabled = true;
//        _isGameStarted = true;

//    }

//    public void GameResult()
//    {
//        AudioManager.instance.PlayBgm(AudioManager.Bgm.Win);
//        gameResult.gameObject.SetActive(true);
//    }

//    private void SetLoadingCharacter()
//    {
//        int idx = UnityEngine.Random.Range(0, 9);
//        _characterPrefab = Resources.Load<GameObject>("Prefabs/Loading/Prefab/Characters/Chibi_Cat_0" + idx);

//        Instantiate(_characterPrefab, winnerPosition.transform);
//        _characterPrefab.transform.localScale = new Vector3(100f, 100f, 100f);

//        // Camera.main.GetComponent<CameraFollow>().SetTarget(_characterPrefab.transform);

//    }

//}
