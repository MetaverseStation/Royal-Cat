using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugManager : Singleton<DebugManager>
{
    public TextMeshProUGUI sceneStatusText;
    public TextMeshProUGUI connectStatusText;
    public TextMeshProUGUI networkStatusText;
    public TextMeshProUGUI pingStatusText;
    public TextMeshProUGUI fpsStatusText;
    public TextMeshProUGUI memoryStatusText;
    public TextMeshProUGUI infoText;

    private static float deltaTime = 0.0f;

    public bool testMode { get; set; } = false;

    //protected override void Awake()
    //{
    //    base.Awake();
    //    Application.targetFrameRate = GameConfig.FPS;
    //}

    private void Start()
    {
        Application.targetFrameRate = GameConfig.FPS;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateConnectionStatus();
        UpdatePingStatus();
        UpdateSceneNameStatus();
        UpdateFPSStatus();
        UpdateMemoryStatus();
    }

    private void UpdateConnectionStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            connectStatusText.text = "Connected";
            if (PhotonNetwork.InRoom)
            {
                networkStatusText.text = "In Room";
            }
            else if (PhotonNetwork.InLobby)
            {
                networkStatusText.text = "In Lobby";
            }
            else
            {
                networkStatusText.text = "None";
            }
        }
        else
        {
            connectStatusText.text = "Disconnected";
            networkStatusText.text = "None";
        }
    }

    private void UpdatePingStatus()
    {
        if (PhotonNetwork.IsConnected)
        {
            pingStatusText.text = $"Ping: {PhotonNetwork.GetPing()} ms";
        }
        else
        {
            pingStatusText.text = "Ping: N/A";
        }
    }

    private void UpdateSceneNameStatus()
    {
        sceneStatusText.text = $"Scene: {SceneManager.GetActiveScene().name}";
    }

    private void UpdateFPSStatus()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        fpsStatusText.text = $"FPS : {fps}";
    }

    private void UpdateMemoryStatus()
    {
        long memory = System.GC.GetTotalMemory(false) / (1024 * 1024);
        memoryStatusText.text = $"Memory: {memory}MB";
    }

    public void SetInfoText(string text)
    {
        infoText.text = text;
    }
}
