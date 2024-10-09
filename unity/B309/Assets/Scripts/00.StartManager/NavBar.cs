using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//로비에서 매니저가 생성함
public class NavBar : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public Button tutorialButton;
    public Button ExitButton;
    public Button SettingButton;
    public GameObject tutorialUI;

    void Start()
    {
        SetNickName();
        InitButton();
    }

    private void Update()
    {
        //ESC 키를 누르면 팝업을 닫는다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelButtonClick();
        }
    }

    private void CancelButtonClick()
    {
        tutorialUI.SetActive(false);
    }

    private void InitButton()
    {
        if (tutorialButton != null)
        {
            tutorialButton.GetComponent<Button>().onClick.AddListener(OnTutorialButtonClicked);
        }

        if (SettingButton != null)
        {
            SettingButton.GetComponent<Button>().onClick.AddListener(OnSettingButtonClicked);
        }

        if (ExitButton != null)
        {
            ExitButton.GetComponent<Button>().onClick.AddListener(OnExitButtonClicked);
        }
    }

    public void OnExitButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click); 
        UIManager.Inst.SetQuitGamePopup();
    }

    public void OnTutorialButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click); 
        tutorialUI.SetActive(true);
    }

    public void OnSettingButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click); 
        UIManager.Inst.SetSettingPopup();
    }

    private void OnEnable()
    {
        SetNickName();
    }

    private void SetNickName()
    {
        string playerName = GameConfig.UserNickName;
        if (playerName == null || playerName == "")
        {
            playerName = "[Unknown]";
        }
        playerNameText.text = playerName;
    }
}
