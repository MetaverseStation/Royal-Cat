using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

//SceneManager는 유니티에서 사용하는 이름이기 때문에 SceneChanger로 명명
public class SceneChanger : Singleton<SceneChanger>
{
    //public static SceneChanger Inst;

    private LoadingManager _loadingManager = null;
    //씬 로딩 완료 콜백함수
    public event Action<string> OnSceneLoadComplete;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //private void OnDestroy()
    //{
    //    SceneManager.sceneLoaded -= OnSceneLoaded;
    //}

    public IEnumerator LoadSceneAsync(string sceneName)
    {
        // 비동기적으로 씬 로드 시작        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 로딩이 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            // 로딩 진행률을 표시 (0 to 1)
            //float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            //Debug.Log($"로딩 중... {progress * 100}%");

            //로딩이 너무 빨라..
            if (_loadingManager != null)
            {
                //Debug.Log($"로딩 중... {asyncLoad.progress * 100}%");
                _loadingManager.SetPercent(asyncLoad.progress * 100);
            }

            if (asyncLoad.progress >= 0.9f)
            {
                //Debug.Log("로딩 완료!");
                //로딩 완료시 콜백이벤트 호출                                       
                OnSceneLoadComplete?.Invoke(sceneName);

                asyncLoad.allowSceneActivation = true;
                _loadingManager = null;

                yield return null;

            }
            yield return null;  // 다음 프레임까지 대기            
        }
    }

    //씬 로드가 "완전히" 완료된 후 수행하는 작업
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log("씬로딩 완료" + scene.name);

        //UIManager.Inst.InitFont();

        switch (scene.name)
        {
            case "Title":
                UIManager.Inst.SetNavBar(false);
                break;
            case "Lobby":
                UIManager.Inst.SetNavBar(true);
                break;
            case "Room":
                PhotonManager.Inst.ClearRoomList();
                UIManager.Inst.SetNavBar(true);                
                break;
            case "InGame":
                PhotonManager.Inst.ClearRoomList();
                AudioManager.instance.PlayBgm(AudioManager.Bgm.InGame);
                UIManager.Inst.SetNavBar(false);
                //GameManager.Inst.SetPlayer();
                //각 플레이어가 로딩을 끝냈다는 신호를 보냄
                //PhotonManager.Inst.SetInGameLoadComplete();
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isInGameLoaded);
                break;
            case "Loading":
                UIManager.Inst.SetNavBar(false);
                _loadingManager = GameObject.Find("LoadingManager").GetComponent<LoadingManager>();
                break;
            case "InGame_Jungle":
                PhotonManager.Inst.ClearRoomList();
                AudioManager.instance.PlayBgm(AudioManager.Bgm.InGame);
                UIManager.Inst.SetNavBar(false);
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isInGameLoaded);
                break;
            case "InGame_Test":
                PhotonManager.Inst.ClearRoomList();
                AudioManager.instance.PlayBgm(AudioManager.Bgm.InGame);
                UIManager.Inst.SetNavBar(false);
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isInGameLoaded);
                break;
            case "InGame_Snow":
                PhotonManager.Inst.ClearRoomList();
                AudioManager.instance.PlayBgm(AudioManager.Bgm.InGame);
                UIManager.Inst.SetNavBar(false);
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isInGameLoaded);
                break;
            case "InGame_Desert":
                PhotonManager.Inst.ClearRoomList();
                AudioManager.instance.PlayBgm(AudioManager.Bgm.InGame);
                UIManager.Inst.SetNavBar(false);
                PhotonManager.Inst.SetPlayerCustomProperty<bool>(true, GameConfig.isInGameLoaded);
                break;
            default:
                Debug.Log("씬 이름 찾을 수 없음");
                break;

        }
    }

    public void SetLoadingScene()
    {
        SceneManager.LoadScene(GameConfig.loadingScene);
    }
}
