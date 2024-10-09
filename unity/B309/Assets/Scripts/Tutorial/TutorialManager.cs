using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // 버튼 관련
    public Button arrawR;
    public Button arrawL;
    public Button cancel;

    // 패널 관련
    public GameObject itemPanel;
    public GameObject controlPanel;
    

    // Start is called before the first frame update
    void Start()
    {
        InitButton();
    }
    private void Update() {

        //ESC 키를 누르면 팝업을 닫는다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelButtonClick();
        }
    }

    private void CancelButtonClick()
    {
        gameObject.SetActive(false);
    }

    private void InitButton() {
        if (arrawR != null) {
            arrawR.GetComponent<Button>().onClick.AddListener(MoveToItem);
        }

        if (arrawL != null) {
            arrawL.GetComponent<Button>().onClick.AddListener(MoveToControl);
        }

        if (cancel != null) {
            cancel.GetComponent<Button>().onClick.AddListener(CancelTutorial);
        }
    }

    private void CancelTutorial()
    {
        gameObject.SetActive(false);   
    }

    private void MoveToControl()
    {
        itemPanel.SetActive(false);
        controlPanel.SetActive(true);
    }

    private void MoveToItem()
    {
        controlPanel.SetActive(false);
        itemPanel.SetActive(true);
    }
}
