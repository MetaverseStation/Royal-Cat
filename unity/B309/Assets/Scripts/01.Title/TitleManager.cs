using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;

public class TitleManager : MonoBehaviourPunCallbacks
{
    private const string _startWord = "Press to Start";

    public TextMeshProUGUI infoText;
    public GameObject loginForm;

    public Button startButton;
    public Button exitButton;

    public GameObject loginFormScript;
    public GameObject logo;

    public Material titleSkybox;

    void Start()
    {
        InitToTitle();

        RenderSettings.skybox = titleSkybox;
        DynamicGI.UpdateEnvironment();
    }

    IEnumerator StartConnection()
    {
        infoText.GetComponent<TextMeshProUGUI>().text = "Connection to server...";

        PhotonManager.Inst.Connect();

        yield return new WaitUntil(() => PhotonManager.Inst.GetConnect());

        infoText.GetComponent<TextMeshProUGUI>().text = "Connected to Server!";
    }

    public override void OnConnectedToMaster()
    {
        infoText.text = "Connect Success";
        base.OnConnectedToMaster();

        logo.SetActive(false);
        startButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(false);
        ShowLoginForm();

    }

    //연결 실패
    public override void OnDisconnected(DisconnectCause cause) => ShowConnectionFailed();

    void ShowLoginForm()
    {
        infoText.gameObject.SetActive(false);
        loginForm.SetActive(true);
    }

    //네트워크 끊길 시 글로벌 팝업 보여줌
    void ShowConnectionFailed()
    {
        loginForm.SetActive(false);
        infoText.text = "Connection Failed. Please check your network.";
    }

    void InitToTitle()
    {
        infoText.text = _startWord;
        loginForm.SetActive(false);

        //버튼 리스너 등록
        if (startButton != null)
        {
            startButton.GetComponent<Button>().onClick.AddListener(StartButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.GetComponent<Button>().onClick.AddListener(ExitButtonClicked);
        }
    }
    private void StartButtonClicked()
    {
        if (loginForm.activeSelf == false && !PhotonNetwork.IsConnected)
        {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
            StartCoroutine(StartConnection());
        }
    }
    private void ExitButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        Application.Quit();
    }
}