using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FindRoom : MonoBehaviour
{
    public TMP_InputField inputField; // InputField 연결

    void Start()
    {
        // 입력할 때마다 대문자로 변환하는 이벤트 리스너 추가
        inputField.onValueChanged.AddListener(HandleInputChange);
    }

    private void HandleInputChange(string input)
    {
        // 입력된 값을 대문자로 변환
        inputField.text = input.ToUpper();
    }
}
