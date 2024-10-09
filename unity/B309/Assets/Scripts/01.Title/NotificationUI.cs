using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.UI.CanvasScaler;
using System;

public class NotificationUI : MonoBehaviour
{
    public GameObject popupPanel;
    public Button confirmButton;
    public Button cancelButton;
    public Button closeButton;

    public TextMeshProUGUI messageText;

    private UnityAction _onConfirmAction;
    private UnityAction _onCancelAction;

    private Vector2 _confirmButtonPos;

    private void Awake()
    {
        //Debug.Log(_confirmButtonPos.x + " " + _confirmButtonPos.y);

        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        closeButton.onClick.AddListener(OnCancelButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        //단축키 등록. 기본적으로 Enter는 Confirm, ESC는 Cancel을 발동한다.
        if (popupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnConfirmButtonClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelButtonClicked();
            }
        }
    }

    public void OnConfirmButtonClicked()
    {
        if (_onConfirmAction != null)
        {
            _onConfirmAction?.Invoke();
            AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click);
        }
        IsVisiblePopup(false);
    }

    public void OnCancelButtonClicked()
    {
        if (_onCancelAction != null)
        {
            _onCancelAction?.Invoke();
            AudioManager.Inst.PlaySfx(AudioManager.Sfx.Click); 
        }
        IsVisiblePopup(false);
    }

    private void IsVisiblePopup(bool enable)
    {
        popupPanel.SetActive(enable);
    }

    //넣을 메시지, 버튼 1~2 여부, 확인버튼 이벤트 함수, 캔슬 버튼 이벤트 함수
    public void Init(string message = null, bool isOneButton = false,
        UnityAction confirmAction = null, UnityAction cancelAction = null,
        string confirmStr = null, string cancelStr = null)
    {
        //버튼 배치
        messageText.text = message;
        _onConfirmAction = confirmAction;
        _onCancelAction = cancelAction;

        RectTransform rectTransform = confirmButton.GetComponent<RectTransform>();
        cancelButton.transform.gameObject.SetActive(!isOneButton);

        if (confirmStr == null || confirmStr == "")
        {
            confirmButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "OK";
        }
        else
        {
            confirmButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = confirmStr;
        }

        if (cancelStr == null || cancelStr == "")
        {
            cancelButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = "NO";
        }
        else
        {
            cancelButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = cancelStr;
        }

        IsVisiblePopup(true);
    }
}
