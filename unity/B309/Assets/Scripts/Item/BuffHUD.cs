using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BuffHUD : MonoBehaviour
{
    public static BuffHUD Inst { get; private set; }

    private const string playerHeadPosPath = "HeadPosition";
    private const string itemHUDPrefabPath = "Effects/Item/ItemHUD";
    private const string auraPrefabPath = "Effects/Item/MagicBuffYellow";
    private const string confusePrefabPath = "Effects/StunnedCirclingStars";
    private const string turtlePrefabPath = "Effects/SlowTurtleOnHead";
    private const string crabBleedPrefabPath = "Effects/BloodDripping";

    private Transform _targetPlayer;
    private PlayerHealth _playerHealth;
    private PlayerMovement _playerMovement;
    private PhotonView _pv;

    //버프창
    public GameObject buffUI;
    private TextMeshProUGUI _itemName;
    private Image _buffIcon;

    //아이템 획득 시 아우라
    public GameObject auraParticle;

    //아이템 획득 UI 계수
    private float riseHeight = 2.1f;
    private float riseSpeed = 8f;
    private float fadeDuration = 0.5f;

    //해파리
    //public GameObject jellyfishStunPrefab;
    public bool isConfusing = false;

    //거북이
    //public GameObject turtleHatPrefab;

    //꽃게
    //public GameObject crabBloodPrefab;

    //문어
    public GameObject octopusBlind;
    public List<Image> octopusInkList;
    private float _octopusEffectDuration = 3f;
    private float _octopusTimer = 0f;

    //###아이콘, 컬러 담을 자료구조
    private Dictionary<string, Sprite> _buffIconDic;
    private Dictionary<string, Color> _buffColorDic;

    public Sprite hpIcon; //초록색,
    public Sprite dmgIcon;
    public Sprite shieldIcon;
    public Sprite speedIcon;

    //컬러
    Color redColor = Color.red; //공격력 증가 시
    Color blueColor = Color.blue; // 방어도 증가시
    Color greenColor = Color.green;// 체력 회복시
    Color skyColor = new Color(135f / 255f, 206f / 255f, 235f / 255f); //이동속도 증가 시
    Color yellowColor = Color.yellow; //기타 아이템 획득 시

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _itemName = buffUI.transform.Find("Panel/BuffText").GetComponent<TextMeshProUGUI>();
        _buffIcon = buffUI.transform.Find("Panel/Image").GetComponent<Image>();


        _buffIconDic = new Dictionary<string, Sprite>();
        _buffColorDic = new Dictionary<string, Color>();

        _buffIconDic.Add("Health", hpIcon);
        _buffColorDic.Add("Health", greenColor);

        _buffIconDic.Add("Attack", dmgIcon);
        _buffColorDic.Add("Attack", redColor);

        _buffIconDic.Add("Defence", shieldIcon);
        _buffColorDic.Add("Defence", blueColor);

        _buffIconDic.Add("Speed", speedIcon);
        _buffColorDic.Add("Speed", skyColor);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            CreateBuffText(0, "Attack");
        }
    }

    //PlayerRPC에서 호출한 함수
    public void CreateBuffText(int playerViewID, string buffName)
    {
        _pv = PhotonView.Find(playerViewID);
        _targetPlayer = _pv.transform;


        //_targetPlayer = GameManager.Inst.GetPlayer().transform;

        Color particleColor = setTextColor(buffName);

        //GameObject itemObject = PhotonNetwork.Instantiate(itemHUDPrefabPath, _targetPlayer.position, Quaternion.identity);
        //GameObject auraObject = PhotonNetwork.Instantiate(auraPrefabPath, _targetPlayer.position, Quaternion.identity);
        GameObject itemObject = Instantiate(Resources.Load<GameObject>(itemHUDPrefabPath), _targetPlayer.position, Quaternion.identity);
        GameObject auraObject = Instantiate(Resources.Load<GameObject>(auraPrefabPath), _targetPlayer.position, Quaternion.identity);

        ParticleSystem ps = auraObject.transform.Find("CenterGlow").GetComponent<ParticleSystem>();

        var mainModule = ps.main;
        mainModule.startColor = particleColor;

        StartCoroutine(UpdateUI(itemObject.transform, auraObject.transform));

    }

    //아이템 획득 메시지 코루틴
    private IEnumerator UpdateUI(Transform tr, Transform aura)
    {
        Vector3 startPos = _targetPlayer.position;
        Vector3 endPos = startPos + new Vector3(0, riseHeight, 0);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float newY = Mathf.Lerp(startPos.y, endPos.y, elapsedTime / fadeDuration);

            Vector3 newPos = new Vector3(_targetPlayer.position.x, newY, _targetPlayer.position.z);
            aura.position = _targetPlayer.position;
            tr.position = newPos;

            elapsedTime += Time.deltaTime * riseSpeed;
            yield return null;
        }

        //endPos로 도착하게 설정

        Vector3 finalPos = new Vector3(_targetPlayer.position.x, endPos.y, _targetPlayer.position.z);
        tr.position = endPos;

        elapsedTime = 0f;
        float additionalTime = 0.5f;
        //float additionalTime = 5f;
        while (elapsedTime < additionalTime)
        {
            // X, Z 축은 계속해서 _targetPlayer 위치를 따라감, Y축은 고정
            tr.position = new Vector3(_targetPlayer.position.x, endPos.y, _targetPlayer.position.z);
            aura.position = _targetPlayer.position; // aura도 플레이어 위치에 고정

            // 1초 경과를 계산
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Destroy(aura.gameObject);
        Destroy(tr.gameObject);
    }

    //해파리 디버프
    public void SetJellyfishConfuseEffect(int playerViewID, float duration)
    {
        _targetPlayer = PhotonView.Find(playerViewID).transform;        
        _playerMovement = _targetPlayer.gameObject.GetComponent<PlayerMovement>();
        //이미 혼란상태이면 리턴
        //if (_playerMovement._isReverse)
        //{
        //    return;
        //}        
        Transform playerHead = _targetPlayer.transform.Find("HeadPosition");

        //해파리 오브젝트 생성
        GameObject jellyfish = Instantiate(Resources.Load<GameObject>(confusePrefabPath));

        jellyfish.transform.position += playerHead.position;        
        
        jellyfish.transform.SetParent(playerHead, true);
        StartCoroutine(WaitingTime(jellyfish, duration));

    }

    private IEnumerator WaitingTime(GameObject obj, float duration)
    {
        yield return new WaitForSeconds(duration);

        Destroy(obj);
    }

    //꽃게 출혈
    public void SetCrabBleeding(int playerViewID, float duration)
    {
        _targetPlayer = PhotonView.Find(playerViewID).transform;
        _playerHealth = _targetPlayer.gameObject.GetComponent<PlayerHealth>();

        //if (_playerHealth.isBleeding)
        //{
        //    return;
        //}

        Transform playerHead = _targetPlayer.transform.Find("HeadPosition");

        //꽃게 오브젝트
        GameObject bloodEffect = Instantiate(Resources.Load<GameObject>(crabBleedPrefabPath));

        bloodEffect.transform.position += playerHead.position;

        bloodEffect.transform.SetParent(playerHead, true);
        StartCoroutine(WaitingTime(bloodEffect, duration));
    }

    //거북이 슬로우
    public void SetTurtleSlow(int playerViewID, float duration)
    {
        _targetPlayer = PhotonView.Find(playerViewID).transform;
        _playerMovement = _targetPlayer.gameObject.GetComponent<PlayerMovement>();

        Transform playerHead = _targetPlayer.transform.Find("HeadPosition");

        GameObject turtleHat = Instantiate(Resources.Load<GameObject>(turtlePrefabPath));

        turtleHat.transform.position += playerHead.position;

        turtleHat.transform.SetParent(playerHead, true);
        StartCoroutine(WaitingTime(turtleHat, duration));
    }

    //문어 먹물
    public void SetOctopusBlind(bool enable)
    {
        //이미 켜진 상태라면 무시
        if (enable && octopusBlind.activeSelf)
        {
            return;
        }

        octopusBlind.SetActive(enable);

        if (enable)
        {
            StartCoroutine(ShowOctopusEffectCoroutine());
        }
    }

    IEnumerator ShowOctopusEffectCoroutine()
    {
        _octopusTimer = 0f;

        while (_octopusTimer < _octopusEffectDuration)
        {
            _octopusTimer += Time.deltaTime;

            // 타이머 진행에 따라 투명도가 점점 흐려지게 설정
            float alpha = Mathf.Lerp(1f, 0f, _octopusTimer / _octopusEffectDuration); // 1에서 0으로 변화
            octopusBlind.GetComponent<Image>().color = new Color(0f, 0f, 0f, alpha);
            foreach (var item in octopusInkList)
            {
                item.color = new Color(0f, 0f, 0f, alpha);
            }

            yield return null;
        }

        SetOctopusBlind(false);
    }

    private Color setTextColor(string itemName)
    {
        Color color = Color.white;

        switch (itemName)
        {
            case "Health":
                _itemName.text = "HP UP!";
                color = _buffColorDic[itemName];
                _itemName.color = color;
                _buffIcon.sprite = _buffIconDic[itemName];
                _buffIcon.color = color;
                _buffIcon.gameObject.SetActive(true);
                break;
            case "Attack":
                _itemName.text = "DMG UP!";
                color = _buffColorDic[itemName];
                _itemName.color = color;
                _buffIcon.sprite = _buffIconDic[itemName];
                _buffIcon.color = color;
                _buffIcon.gameObject.SetActive(true);
                break;
            case "Defence":
                _itemName.text = "SHIELD UP!";
                color = _buffColorDic[itemName];
                _itemName.color = color;
                _buffIcon.sprite = _buffIconDic[itemName];
                _buffIcon.color = color;
                _buffIcon.gameObject.SetActive(true);
                break;
            case "Speed":
                _itemName.text = "SPEED UP!";
                color = _buffColorDic[itemName];
                _itemName.color = color;
                _buffIcon.sprite = _buffIconDic[itemName];
                _buffIcon.color = color;
                _buffIcon.gameObject.SetActive(true);
                break;
            case "Lobster":
                _itemName.text = "랍스터 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "Octopus":
                _itemName.text = "문어 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "Jellyfish":
                _itemName.text = "해파리 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "Crab":
                _itemName.text = "꽃게 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "SlowTurtle":
                _itemName.text = "거북이 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "Parabola":
                _itemName.text = "곡사포 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
            case "ShotGun":
                _itemName.text = "멀티샷 획득!";
                color = yellowColor;
                _itemName.color = color;
                _buffIcon.gameObject.SetActive(false);
                break;
        }
        return color;
    }
}
