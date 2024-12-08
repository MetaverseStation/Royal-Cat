using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Inst { get; private set; }

    // 배경음
    [Header("#BGM")]
    public AudioClip[] bgmClips;
    public AudioSource[] bgmPlayer;
    public int bgmChannels = 0;
    public int bgmChannelIndex;
    public float bgmVolume = 0.5f;

    // 효과음
    [Header("#SFX")]
    public AudioClip[] sfxClips;
    public float sfxVolume = 0.7f;
    // 다양한 플레이어들의 효과음을 저장할 수 있는 채널시스템
    public int sfxChannels = 0;
    public AudioSource[] sfxPlayer;
    // 현재 채널의 정보를 알기 위한 변수
    public int sfxChannelIndex;

    // 1:1 대응하는 열거형 데이터 선언
    public enum Bgm { Title, Win, InGame }
    public enum Sfx { Attack, Click = 4, FootStep, CollectItem, PlayerDamaged, PlayerDead, MonsterStoneShot, MonsterRangeShot, MonsterCloseAttack, MonsterDead}


    private void Awake()
    {
        //싱글톤 선언
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Init();
        PlayBgm(Bgm.Title);
    }

    // Awake에서 싱글톤 인스턴스 설정
    //private void Awake()
    //{
    //    if (instance == null)
    //    {
    //        instance = this;
    //        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않도록 설정
    //    }
    //    else
    //    {
    //        Destroy(gameObject); // 다른 인스턴스가 있으면 파괴
    //    }
    //}

    void Init()
    {
        if (bgmClips != null && bgmClips.Length > 0)
        {
            bgmChannels = bgmClips.Length;
        }
        // 배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        // 배경음을 담당하는 자식 오브젝트 생성
        bgmObject.transform.parent = transform;
        // bgmPlayer 초기화
        bgmPlayer = new AudioSource[bgmChannels];

        for (int index = 0; index < bgmPlayer.Length; index++)
        {
            //Debug.Log("bgm 초기화 완료");
            bgmPlayer[index] = bgmObject.AddComponent<AudioSource>();
            // 게임 켜자마자 bgm이 흘러나오는 것을 방지
            bgmPlayer[index].playOnAwake = false;
            // 게임 내에서 bgm은 계속 반복되므로 loop를 true로 설정
            bgmPlayer[index].loop = true;
            bgmPlayer[index].volume = bgmVolume;
        }

        if (sfxClips != null && sfxClips.Length > 0)
        {
            sfxChannels = sfxClips.Length;
        }
        // 효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        // 채널값을 사용하여 오디오 소스 배열 초기화
        sfxPlayer = new AudioSource[sfxChannels];

        for (int index = 0; index < sfxPlayer.Length; index++)
        {
            sfxPlayer[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayer[index].playOnAwake = false;
            sfxPlayer[index].volume = sfxVolume;
        }

    }

    public void PlayBgm(Bgm bgm)
    {
        for (int index = 0; index < bgmPlayer.Length; index++)
        {
            int loopIndex = (index + bgmChannelIndex) % bgmPlayer.Length;

            if (bgmPlayer[loopIndex].isPlaying)
            {
                bgmPlayer[loopIndex].Stop();
                continue;
            }
            else {
                int randomIndex = 0;
                if (bgm == Bgm.InGame)
                {
                    randomIndex = Random.Range(0,3);
                }

                bgmChannelIndex = loopIndex;
                bgmPlayer[loopIndex].clip = bgmClips[(int)bgm + randomIndex];
                bgmPlayer[loopIndex].Play();
                break; 
            }
        }
    }

    public void PlaySfx(Sfx sfx)
    {   
        for (int index = 0; index < sfxPlayer.Length; index++)
        {
            int loopIndex = (index + sfxChannelIndex) % sfxPlayer.Length;

            if (sfxPlayer[loopIndex].isPlaying)
            {
                continue;
            }

            int randomIndex = 0;
            if (sfx == Sfx.Attack)
            {
                randomIndex = Random.Range(0,4);
            }

            sfxChannelIndex = loopIndex;
            sfxPlayer[loopIndex].clip = sfxClips[(int)sfx + randomIndex];
            sfxPlayer[loopIndex].Play();
            break; 
        }
    }
}
