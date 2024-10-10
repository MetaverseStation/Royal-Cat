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
    private bool _isFullScreen = true;

    // Start is called before the first frame update

    void Start()
    {
        volumeSlider.onValueChanged.AddListener(SetVolume);
        muteToggle.onValueChanged.AddListener(ToggleMute);
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        fullScreenToggle.onValueChanged.AddListener(ToggleFullscreen);
    }

    public void OnExitButtonClicked()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        settingsPanel.SetActive(false);
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
        _isFullScreen = isFullscreen;
        Screen.fullScreen = _isFullScreen;
    }

    public void ChangeResolution(int index)
    {
        if (index == 0)
        {
            Screen.SetResolution(2550, 1440, _isFullScreen);
        }
        else if (index == 1)
        {
            Screen.SetResolution(1920, 1080, _isFullScreen);
        }
    }
}
