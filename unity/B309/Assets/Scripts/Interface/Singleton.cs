using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

//*포톤은 사용 불가
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _inst;
    private static object _lock = new object();
    private static bool _appQuit = false;

    public static T Inst
    {
        get
        {
            //인스턴스가 없으면 해당 씬에서 오브젝트를 찾는다.
            //그래도 없다면 하나 만든다.

            //앱을 종료할 때 싱글톤 오브젝트를 다시 만드려는 현상을 방지
            if (_appQuit)
            {
                return null;
            }

            lock (_lock)
            {
                if (_inst == null)
                {
                    _inst = FindObjectOfType<T>();

                    if (_inst == null)
                    {
                        GameObject singletonObj = new GameObject();
                        _inst = singletonObj.AddComponent<T>();
                        singletonObj.name = typeof(T).ToString() + "(SingleTon)";

                        DontDestroyOnLoad(singletonObj);
                    }
                }
                return _inst;
            }
        }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (_inst == null)
        {
            _inst = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        StartCoroutine(Logout());
    }

    private IEnumerator Logout()
    {
        // 로그인 상태 갱신 요청
        UnityWebRequest request = new UnityWebRequest("http://j11b309.p.ssafy.io/api/member/logout/" + User.UserName, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("로그아웃 성공");
            _appQuit = true;
        }
        else
        {
            Debug.Log("로그아웃 실패");
        }
    }

    protected virtual void OnDestroy()
    {
        _appQuit = true;
    }

    //protected virtual void OnDestroy()
    //{
    //    if (_inst == this)
    //    {
    //        Destroy(gameObject);
    //        _inst = null; // 인스턴스가 파괴될 때 _inst 값을 null로 설정
    //    }
    //}
}
