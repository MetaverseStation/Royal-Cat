//BuffHUD.cs로 이사했습니다

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Photon.Pun;
//using UnityEngine.UI;

//public class PlayerBlind : MonoBehaviour
//{
//    [Header("Squid Effect")]
//    // 플레이어 내부의 Canvas인 먹물을 통해 시야를 가린다-> 플레이어 프리팹 참조
//    public GameObject squidEffect;
//    private float _squidEffectDuration = 3f;
//    private float _squidTimer = 0f;
//    private bool _isSquidEffectActive = false;
//    private PlayerHealth _playerHealth; 

//    private void Awake()
//    {
//        _playerHealth = GetComponent<PlayerHealth>();
//    }

//    // Start is called before the first frame update
//    // Squid 클래스에서 호출되는 함수(66번 줄 참고)
//    public void ShowSquidEffect()
//    {
//        // 이미 맞은 경우는 무시
//        if (_isSquidEffectActive)
//        {
//            return;
//        }
//        // 2초간 squidEffect 활성화
//        StartCoroutine(ShowSquidEffectCoroutine());
//    }

//    IEnumerator ShowSquidEffectCoroutine()
//    {
//        // 맞은 판정 시작
//        _isSquidEffectActive = true;
//        _playerHealth.ApplyVisualEffect("squid");
//        // 화면 활성화
//        squidEffect.SetActive(true);
//        _squidTimer = 0f; // 타이머 초기화

//        while (_squidTimer < _squidEffectDuration)
//    {
//            // 타이머 진행 // 보자
//            _squidTimer += Time.deltaTime;

//            // 타이머 진행에 따라 투명도가 점점 흐려지게 설정
//            float alpha = Mathf.Lerp(1f, 0f, _squidTimer / _squidEffectDuration); // 1에서 0으로 변화
//            squidEffect.GetComponent<Image>().color = new Color(0f, 0f, 0f, alpha);

//            yield return null;
//        }

//        // 효과 종료 후 화면 비활성화
//        squidEffect.SetActive(false);
//        _isSquidEffectActive = false;
//    }

//    public void DisableSquidEffect()
//    {
//        squidEffect.SetActive(false);
//        _isSquidEffectActive = false;
//    }
//}
