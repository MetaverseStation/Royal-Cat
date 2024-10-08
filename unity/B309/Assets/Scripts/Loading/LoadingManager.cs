using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public GameObject characterParent;
    public RuntimeAnimatorController baseController;

    public Slider progressBar;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI lodingPercentText;

    private GameObject characterPrefab;
    private Animator animator;

    private float _percentage = 0f;

    private float _waitTime = 0f;
    private bool _isWaiting = false;
    void Start()
    {
        SetLoadingCharacter();
        StartCoroutine(UpdateLoadingText());
    }

    void Update()
    {
        progressBar.value = _percentage;
        lodingPercentText.text = $"{(int)_percentage}";

        //서버 연결 타임아웃 시 재연결 팝업
        _waitTime += Time.deltaTime;
        if (_waitTime >= GameConfig.ServerWaitingTime && _isWaiting == false)
        {
            _isWaiting = true;
            UIManager.Inst.ConnctionFailedPopup(true);
        }
    }

    private void SetLoadingCharacter()
    {
        int idx = Random.Range(0, 9);
        characterPrefab = Resources.Load<GameObject>("Prefabs/Loading/Prefab/Characters/Chibi_Cat_0" + idx);

        //애니메이션      
        animator = characterPrefab.GetComponent<Animator>();

        if (animator != null && baseController != null)
        {
            animator.runtimeAnimatorController = baseController;

            animator.Play("Run", 0, 0f);
        }

        Instantiate(characterPrefab, characterParent.transform);
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            loadingText.text = "Loading.";
            yield return new WaitForSeconds(0.3f);
            loadingText.text = "Loading..";
            yield return new WaitForSeconds(0.3f);
            loadingText.text = "Loading...";
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void SetPercent(float percent)
    {
        _percentage = percent;
    }
}
