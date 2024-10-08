using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreateRoomPopup : MonoBehaviour
{
    public TMP_Dropdown maxPlayerDropdown;
    private List<string> _maxPlayerList = new List<string> { "2", "3", "4", "5", "6" };

    void Start()
    {
        maxPlayerDropdown.ClearOptions();
        maxPlayerDropdown.AddOptions(_maxPlayerList);

        maxPlayerDropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(maxPlayerDropdown); });
    }

    void OnDropdownValueChanged(TMP_Dropdown index)
    {
        Debug.Log(" : " + index.options[index.value].text);
    }
}
