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
    public GameObject tutorialUI; 
    private bool _isTutorial = false;

    void Start()
    {
        SetNickName();
    }

    public void OnExitButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click); 
        UIManager.Inst.SetQuitGamePopup();
    }

    public void OnTutorialButtonClicked()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click); 
        if (_isTutorial) {
            _isTutorial = !_isTutorial;
            tutorialUI.SetActive(false);
        } else {
            _isTutorial = !_isTutorial;
            tutorialUI.SetActive(true);
        }
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
