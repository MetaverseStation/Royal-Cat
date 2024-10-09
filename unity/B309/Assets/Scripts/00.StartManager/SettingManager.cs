using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SettingManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Toggle muteToggle;
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullScreenToggle;
    public GameObject settingsPanel;

    // Start is called before the first frame update
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
        muteToggle.onValueChanged.AddListener(ToggleMute);
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        fullScreenToggle.onValueChanged.AddListener(ToggleFullscreen);
    }
    
    void Update()
    {
        if(settingsPanel.activeSelf){

            if (Input.GetKeyDown(KeyCode.F2))
            {

                UIManager.Inst.SetSettingPopup();
            }
        }
    }

    public void OnExitButtonClicked()
    {

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        UIManager.Inst.SetSettingPopup();
    }

    public void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void ToggleMute(bool isMuted)
    {
        AudioListener.volume = isMuted ? 0f : volumeSlider.value;
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void ChangeResolution(int index)
    {
        if (index == 0)
            Screen.SetResolution(1920, 1080, Screen.fullScreen);
        else if (index == 1)
            Screen.SetResolution(2550, 1440, Screen.fullScreen);
    }

    private void IsVisiblePopup(bool enable)
    {
        settingsPanel.SetActive(enable);
    }
}
