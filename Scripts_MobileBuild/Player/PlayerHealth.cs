using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using static GameConfig;

// PlayerHealth는 LivingEntity 클래스를 상속합니다.
public class PlayerHealth : LivingEntity, IPunObservable
{
    public Slider healthSlider; // 플레이어 체력 슬라이더
    public Slider chargeSlider; // 플레이어 차지 슬라이더

    public TextMeshProUGUI healthText; // 플레이어 체력 텍스트
    public GameObject playerDistinct; // 플레이어 피아식별 UI
    public Image healthSliderColor; // 플레이어 체력 슬라이더
    public float defense = 0; // 플레이어 방어력
    public float deathcount = 0;

    public TextMeshProUGUI nicknameText; // 플레이어 닉네임
    public int recentDamage; // 가장 최근 데미지를 준 플레이어 ID
    public int killCount; // 플레이어 킬 수

    private PlayerHand _playerHand;
    private PlayerMovement _playerMovement;
    private Animator _playerAnimator;
    private PlayerInput _playerInput;

    public PhotonView pv;

    private Transform _child;
    private SkinnedMeshRenderer _renderer;
    private Color _originalColor;
    private Color _currentEffectColor;
    private Color _sliderColor;
    private Vector2 _sliderOrigin;

    // 플레이어 피격시 전환할 색상
    private static readonly Color _healthRed = new Color(1.0f, 0.4235f, 0.4549f);
    private static readonly Color _healthGreen = new Color(0.1647f, 0.8824f, 0.6157f);
    private static readonly Color _healthBlack = new Color(0.5f, 0.5f, 0.5f);
    private static readonly Color _healthPurple = new Color(0.9f, 0.5f, 1.0f);

    // 독 데미지 관련 변수
    private float _poisonTimeRemain = 0f; // 독 데미지 남은시간
    //private bool _isTakingPoison = false; // 현재 독을 받고 있는지 여부
    private float _damageInterval = 0.4f; // n초에 한번씩 데미지(시간 설정 변수)
    private float _poisonDamage = 1f; // 독 데미지 설정 변수
    private Coroutine _poisonCoroutine;

    //상태이상, 출혈
    public bool isBleeding = false;

    private void Start()
    {
        nicknameText.text = pv.Owner.NickName;
        Debug.Log("플레이어 닉네임: " + pv.Owner.NickName);
        _sliderColor = healthSliderColor.color;

    }

    private void Awake()
    {
        _playerAnimator = GetComponent<Animator>();

        // 플레이어 캐릭터가 사망한 경우, 플레이어 캐릭터가 공격할 수 없도록 조작에 맞춰 동작하는 컴포넌트들을 비활성화
        _playerHand = GetComponent<PlayerHand>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerInput = GetComponent<PlayerInput>();

        pv = GetComponent<PhotonView>();

        _child = transform.Find("Chibi_Cat");
        _renderer = _child.GetComponent<SkinnedMeshRenderer>();
        _originalColor = _renderer.material.GetColor("_SpecularColor");
        _sliderOrigin = healthSlider.GetComponent<RectTransform>().anchoredPosition;
    }

    private void Update()
    {

        if (_playerInput.playerInstantDeath || health <= 0f)
        // if (_playerInput.playerInstantDeath)
        {
            deathcount++;
            // 오버라이드된 OnDamege와 Die에서 음악 실행이 되지 않아 playerHealth에서 Health가 0이되는 로직 추가
            // 여기서 피가 0이 되면 죽는 로직을 실행하도록 하니까 잘 된다. 왜 그런건지는 이해가 잘 안됨
            if (deathcount == 1)
            {
                Die();
            }
        }

        if(pv.IsMine && GameManager.Inst.GetPlayer().transform.position.y < -100)
        {
            Die();
        }
    }
    protected override void OnEnable()
    {
        // LivingEntity의 onEnable() 실행
        base.OnEnable();

        // 체력 슬라이더 활성화
        healthSlider.gameObject.SetActive(true);

        // 체력 UI 설정
        healthUIUpdate(startingHealth);

        // 플레이어 리스폰이 생긴다면 여기에 코드를 작성하면 됨
    }

    // 피격 시 실행하게 될 OnDamage 메서드
    public override void OnDamage(float damage)
    {

        if (!dead)
        {
            // 사망하지 않은 경우에만 피격 효과음 재생이 여기에
            AudioManager.Inst.PlaySfx(AudioManager.Sfx.PlayerDamaged);
        }

        // damage가 defense보다 작다면 damage 0으로 처리
        damage = damage < defense ? 0 : damage - defense;

        // LivingEntity의 OnDamage() 실행
        base.OnDamage(damage);

        // 체력 UI 설정
        healthUIUpdate(health);

        // 모든 클라이언트에서 Shake() 실행
        pv.RPC("StartShake", RpcTarget.All);
    }

    [PunRPC]
    private void StartShake()
    {
        StartCoroutine(Shake());
    }

    // 체력 회복 버프 아이템을 먹었을 때 실행하게 될 RestoreHealth 메서드
    public override void RestoreHealth(float newHealth)
    {
        // LivingEntity의 RestoreHealth() 실행
        base.RestoreHealth(newHealth);

        // 갱신된 체력으로 체력 UI 갱신
        healthUIUpdate(health);
    }

    // 플레이어 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die() 실행
        base.Die();

        string killerName = PhotonView.Find(recentDamage)?.Owner.NickName ?? "Unknown"; // 마지막 공격자 이름
        string victimName = pv.Owner.NickName; // 사망한 플레이어 이름        

        if (pv.IsMine)
        {
            AudioManager.Inst.PlaySfx(AudioManager.Sfx.PlayerDead);
            
           
            //먹물 제거
            if (BuffHUD.Inst != null)
            {
                BuffHUD.Inst.SetOctopusBlind(false);
            }            

            //플레이어 사망처리
            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(GameConfig.isDead))
            {
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isDead);
            }
        }

        AudioManager.Inst.PlaySfx(AudioManager.Sfx.PlayerDead);        

        // 모든 클라이언트에서 킬 로그를 출력하도록 RPC 호출        
        pv.RPC("ShowKillLog", RpcTarget.All, killerName, victimName);

        // 모든 클라이언트에서 Die 처리
        pv.RPC("StartDie", RpcTarget.All);
    }

    [PunRPC]
    private void StartDie()
    {
        StartCoroutine(AfterDie());
    }

    // 킬로그
    [PunRPC]
    public void ShowKillLog(string killerName, string victimName)
    {
        string killMessage ="["+killerName + "]님이 [" + victimName+"]를 처치!";

        
        //pv.RPC("SendKillLog", pv.Owner, killerName, victimName, killMessage);
    }

    //[PunRPC]
    //public void SendKillLog(string killerName, string victimName, string killMessage)
    //{
    //    InGameUIManager.Inst.ShowKillLog(killerName, victimName, killMessage); // UIManager를 통해 킬 로그 출력
    //}


    // 플레이어 체력 슬라이더 및 텍스트 업데이트
    public void healthUIUpdate(float changedHealth)
    {
        if (healthSlider.value > changedHealth)
        {
            Debug.Log("피격");
            if (pv.IsMine)
            {
                Debug.Log("소리 재생");
                AudioManager.Inst.PlaySfx(AudioManager.Sfx.PlayerDamaged);
            }
        }

        if (healthSlider.maxValue < changedHealth)
        {
            healthSlider.maxValue = changedHealth;
        }

        healthSlider.value = changedHealth;
        healthText.text = changedHealth.ToString();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 현재 HP를 전송 (이 클라이언트가 '마스터' 또는 로컬 플레이어일 때)
            stream.SendNext(health);
        }
        else
        {
            // 다른 클라이언트에서 받은 HP 정보를 갱신
            health = (float)stream.ReceiveNext();
            healthUIUpdate(health); // UI 업데이트
        }
    }

    IEnumerator Shake()
    {
        float t = 2.5f;
        float shakePower = 0.08f;

        healthSliderColor.color = _healthRed;



        _renderer.material.SetColor("_BaseColor", _healthRed);

        while (t > 0f)
        {
            t -= 0.05f;
            healthSlider.GetComponent<RectTransform>().anchoredPosition = _sliderOrigin + UnityEngine.Random.insideUnitCircle * shakePower * t;
            yield return null;
        }

        _renderer.material.SetColor("_BaseColor", _originalColor);

        healthSlider.GetComponent<RectTransform>().anchoredPosition = _sliderOrigin;

        // Color color;
        // ColorUtility.TryParseHtmlString("#" + PhotonManager.Inst.GetPlayerThemeColorCode(), out color);

        // healthSliderColor.color = _healthGreen;
        healthSliderColor.color = _sliderColor;
    }

    // 새로운 플레이어가 접속했을 때 현재 상태를 요청
    [PunRPC]
    public void RequestHealthStatus()
    {
        pv = PhotonView.Get(this);
        pv.RPC("SyncHealthStatus", RpcTarget.Others, health);
    }

    // 상태 동기화
    [PunRPC]
    public void SyncHealthStatus(float syncedHealth)
    {
        health = syncedHealth;
        healthUIUpdate(health); // UI 업데이트
    }

    private void OnPlayerJoinedRoom(Player newPlayer)
    {
        // 새로운 플레이어가 들어왔을 때 상태 요청
        if (PhotonNetwork.IsMasterClient)
        {
            pv = PhotonView.Get(this);
            pv.RPC("RequestHealthStatus", newPlayer);
        }
    }

    IEnumerator AfterDie()
    {
        // 체력 슬라이더 비활성화
        healthSlider.gameObject.SetActive(false);

        // 피아식별 UI 비활성화
        playerDistinct.SetActive(false);

        // 플레이어 공격 컴포넌트 비활성화
        _playerHand.enabled = false;

        // 차지 슬라이더 비활성화
        chargeSlider.gameObject.SetActive(false);

        // 플레이어 이동 컴포넌트 비활성화
        _playerMovement.enabled = false;

        // 사망 애니메이션 재생
        _playerAnimator.SetTrigger("Die");

        // 플레이어가 자신이 죽은 것을 인지할 수 있도록 대기
        yield return new WaitForSeconds(2f);

        gameObject.SetActive(false);
        //Destroy(gameObject);

        PhotonView pv = PhotonView.Get(this);

        if (pv.IsMine)
        {
            //플레이어 UI 비활성화
            if (InGameUIManager.Inst != null)
            {
                InGameUIManager.Inst.SetGameOverPanel(true);
            }
            
            //GameObject ghost = Instantiate((GameObject)Resources.Load("GhostCharacter"), transform.position, transform.rotation);
            GameObject ghost = PhotonNetwork.Instantiate("GhostCharacter", transform.position, transform.rotation, 0);
            Camera.main.GetComponent<CameraFollow>().SetTarget(ghost.transform);
            if (!ghost.GetComponent<PlayerMovement>().enabled)
            {
                ghost.GetComponent<PlayerMovement>().enabled = true;
            }

            SetRendererVisibility(ghost, true);
        }
    }

    // 렌더러 가시성 설정
    private void SetRendererVisibility(GameObject player, bool isVisible)
    {
        Renderer[] renderers = player.GetComponentsInChildren<Renderer>();

        // 다른 클라이언트에서는 렌더러를 비활성화하여 보이지 않게 설정
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = isVisible;
            Debug.Log($"Renderer for {renderer.gameObject.name} set to {isVisible}");
        }
    }

    // 독 데미지
    public void DamageOverTime(int totalDamage, float duration)
    {
        if (_poisonCoroutine != null)
        {
            StopCoroutine(_poisonCoroutine); // 이전 코루틴 정지
        }
        //_isTakingPoison = true;
        _poisonTimeRemain = duration;
        _poisonCoroutine = StartCoroutine(ApplyPoisonDamage());
    }
    private IEnumerator ApplyPoisonDamage()
    {
        while (_poisonTimeRemain > 0 && health > 0)
        {
            health -= _poisonDamage; // 데미지 적용
            healthUIUpdate(health); // UI 업데이트
            if (health <= 0)
            {
                //_isTakingPoison = false;
                _poisonTimeRemain = 0;
                Die();
                yield break;
            }
            _poisonTimeRemain -= _damageInterval; // 남은 시간 감소
            yield return new WaitForSeconds(_damageInterval); // 지정된 시간만큼 대기
        }

        //_isTakingPoison = false;
        _poisonTimeRemain = 0;
    }

    // 오징어
    public void ApplyVisualEffect(string effectType, float duration)
    {
        pv.RPC("StartVisualEffect", RpcTarget.All, effectType, duration); // effectType 변수 전달
    }

    [PunRPC]
    private void StartVisualEffect(string effectType, float duration)
    {
        if (effectType == "squid")
        {
            _currentEffectColor = _healthBlack; // 오징어 피격 시 검정색
        }
        else if (effectType == "jellyFish")
        {
            _currentEffectColor = _healthPurple; // 해파리 피격 시 보라색
        }

        InvokeRepeating("VisualEffect", 0f, 1f); // 1초 간격으로 SquidEffect 호출
        Invoke("StopVisualEffect", duration); // duration 후에 효과 종료
    }

    private void VisualEffect()
    {
        StartCoroutine(ChangeColorTemporarily());
    }

    private IEnumerator ChangeColorTemporarily()
    {
        // 선택된 색으로 변경
        _renderer.material.SetColor("_BaseColor", _currentEffectColor);
        yield return new WaitForSeconds(0.1f); // 0.1초 기다림
                                               // 원래 색으로 복원
        _renderer.material.SetColor("_BaseColor", _originalColor);
    }

    private void StopVisualEffect()
    {
        CancelInvoke("VisualEffect"); // SquidEffect 반복 호출 중지
    }




    [PunRPC]
    public void SyncDamage(int playerViewID, float updatedHealth)
    {
        // 모든 클라이언트에서 HP를 동기화
        PhotonView targetView = PhotonView.Find(playerViewID);
        if (targetView != null)
        {
            PlayerHealth playerHealth = targetView.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.healthUIUpdate(updatedHealth);
            }
        }
    }
}
