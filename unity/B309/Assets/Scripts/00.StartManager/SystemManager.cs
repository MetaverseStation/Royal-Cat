using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SystemManager : MonoBehaviour {

    public static SystemManager Inst;
    
    const string _logoutAPI = "http://j11b309.p.ssafy.io/api/member/logout/";
    private bool _isLogout = false;

    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    private void Awake()
    {
        if(Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private static bool WantsToQuit()
    {
        if (!Inst._isLogout)
        {           
            Inst.StartCoroutine(Inst.LogoutAndQuit());
            return false;
        }
        return true;
    }

    private IEnumerator LogoutAndQuit()
    {
        yield return Logout();
        _isLogout = true;

        Application.Quit();
    }

    private IEnumerator Logout()
    {
        float timeout = 3.0f;
        bool requestOK = false;

        // 로그인 상태 갱신 요청
        UnityWebRequest request = new UnityWebRequest(_logoutAPI+User.UserName, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var asyncOperation = request.SendWebRequest();

        float startTime = Time.time;

        while (!asyncOperation.isDone && (Time.time - startTime) < timeout) 
        {
            yield return null;
        }

        if(asyncOperation.isDone)
        {
            requestOK = true;
        }
        
        if (requestOK && request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("로그아웃 성공");           
        }
        else
        {
            Debug.Log("로그아웃 실패");
        }
    }

    protected virtual void OnApplicationQuit()
    {
        if (!_isLogout)
        {
            StartCoroutine(LogoutAndQuit());
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
