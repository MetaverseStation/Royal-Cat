using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager Inst { get; private set; }

    //기본 폰트
    public TMP_FontAsset defaultFont;

    //프리팹 UI
    public GameObject popupUIPanelPrefab;
    public GameObject navBarPrefab;
    public GameObject gameSettingPrefab;    

    public GameObject tutorialPrefab;

    void Awake()
    {        
        Screen.SetResolution(2550, 1440, true);

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

    private void Start()
    {
        Screen.SetResolution(2550, 1440, true);
        //InitFont();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {

            EnableSettingPopup(true);
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            EnableTutorialPopup(false);
            EnableSettingPopup(false);
        }
    }

    public void SetPopupCustum(string message, bool isOneButton,
        UnityAction confirmAction, UnityAction cancelAction,
        string okButtonText, string noButtonText)
    {
        NotificationUI script = null;

        if (popupUIPanelPrefab != null)
        {
           script  = popupUIPanelPrefab.GetComponent<NotificationUI>();
        }

        if (script != null)
        {
            script.Init(message, isOneButton, confirmAction, cancelAction, okButtonText, noButtonText);
        }
    }


    public void SetNavBar(bool enable)
    {
        navBarPrefab.SetActive(enable);
    }
    
    public void SetExitPopup() {
        SetPopupCustum("로비로 돌아가시겠어요?", false, PhotonManager.Inst.LeaveRoom, null, "네..", "아니요!");
    }


    //나가기 여부 팝업창
    public void SetQuitGamePopup()
    {
        SetPopupCustum("정말로 게임을 종료하시겠어요?", false, SystemManager.Inst.QuitGame, null, "네..", "아니요!");
    }

    //연결 실패 팝업 창
    public void ConnctionFailedPopup(bool enable)
    {            
        if (enable)
        {
            SetPopupCustum("네트워크 연결이 원활하지 않습니다.", false, SceneChanger.Inst.GoToTitleScene, SystemManager.Inst.QuitGame, "타이틀로", "게임종료");
        }
    }

    public void SetInformationPopup(string text)
    {
        SetPopupCustum(text, true, null, null, "확인", null);
    }

    public void SetUnknownError()
    {
        SetPopupCustum("알 수 없는 오류", true, SystemManager.Inst.QuitGame, SystemManager.Inst.QuitGame, "게임종료", null);
    }

    public void SetPopup(bool enable)
    {
        popupUIPanelPrefab.SetActive(enable);
    }

    //튜토리얼 팝업
    public void EnableTutorialPopup(bool enable)
    {
        tutorialPrefab.SetActive(enable);
    }

    //세팅 팝업
    public void EnableSettingPopup(bool enable)
    {
        gameSettingPrefab.SetActive(enable);
    }
    
    public void InitFont()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allTexts)
        {
            text.font = defaultFont;
        }
    }
}
