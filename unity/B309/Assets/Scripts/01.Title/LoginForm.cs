using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System.Collections;
using System.Threading.Tasks;


public class LoginForm : MonoBehaviour
{
    public GameObject loginFormPrefab;

    private TMP_InputField _inputID;
    private TMP_InputField _inputPW;
    private TextMeshProUGUI _infoText;
    private Button _loginButton;
    private Button _signUpButton;

    // 로그인 관련 변수, 객체
    [System.Serializable]
    public class LoginRequest
    {
        public string userName;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string nickname;
        public string token;
    }

    void Start()
    {
        Init();
    }

    private void Update()
    {
        // Tab 키가 눌렸을 때 처리
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 선택된 UI 요소를 가져옴
            Selectable current = EventSystem.current.currentSelectedGameObject?.GetComponent<Selectable>();

            // 현재 선택된 요소가 있다면, 다음 UI 요소로 포커스 이동
            if (current != null)
            {
                Selectable next = current.FindSelectableOnDown();  // 다음 UI 요소
                if (next != null)
                {
                    // 다음 UI 요소로 포커스 이동
                    next.Select();
                }
            }
        }

        //로그인 엔터키
        if (Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter)
            && loginFormPrefab.activeSelf)
        {
            Debug.Log("엔터");
            OnClickLogin();
        }
    }

    private void Init()
    {
        if (loginFormPrefab != null)
        {
            //아이디 인풋필드
            Transform temp = loginFormPrefab.transform.Find("Popup/InputField_Id");
            if (temp != null)
            {
                _inputID = temp.GetComponent<TMP_InputField>();
            }

            //비밀번호 인풋 필드
            temp = loginFormPrefab.transform.Find("Popup/InputField_Password");
            if (temp != null)
            {
                _inputPW = temp.GetComponent<TMP_InputField>();
            }

            //알림메시지
            temp = loginFormPrefab.transform.Find("Popup/ValidationText");
            if (temp != null)
            {
                _infoText = temp.GetComponent<TextMeshProUGUI>();
                _infoText.text = "";
            }

            //버튼 초기화
            temp = loginFormPrefab.transform.Find("Popup/Button_Sign_in");
            if (temp != null)
            {
                _loginButton = temp.GetComponent<Button>();
                _loginButton.GetComponent<Button>().onClick.AddListener(OnClickLogin);
            }

            temp = loginFormPrefab.transform.Find("Popup/Signup_Button");
            if (temp != null)
            {
                _signUpButton = temp.GetComponent<Button>();
                _signUpButton.GetComponent<Button>().onClick.AddListener(OnClickSignUp);
            }
        }
    }

    private async void OnClickLogin()
    {
        //JoinLobby();

        string id = _inputID.text;
        string pw = _inputPW.text;

        // 예시: 간단한 유효성 검사
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            _infoText.text = "ID or PW is Empty";
        }
        else
        {
            bool isDuplicate = await IsDuplicateLogin(id);
            
            // 로그인 중복 체크
            if (isDuplicate)
            {
                // 이미 로그인 중 입니다.
                _infoText.text = "Already logined account";
            }
            else
            {
                // 로그인 요청
                StartCoroutine(Login(id, pw));
            }
        }
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
    }

    // 중복 로그인 확인
    private async Task<bool> IsDuplicateLogin(string userId)
    {
        using (UnityWebRequest request = new UnityWebRequest("http://j11b309.p.ssafy.io/api/member/login/" + userId, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                return false;
            }
            else
            {
                return true; // 요청 실패 시 중복 로그인으로 간주
            }
        }
    }

    private IEnumerator Login(string id, string pw)
    {
        // 로그인 요청 데이터
        LoginRequest loginRequest = new LoginRequest
        {
            userName = id,
            password = pw
        };

        // JSON 데이터로 변환
        string json = JsonUtility.ToJson(loginRequest);

        UnityWebRequest request = new UnityWebRequest("http://j11b309.p.ssafy.io/api/member/login", "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;

            if (jsonResponse.Contains("인증되지 않은 사용자입니다."))
            {
                Debug.Log("로그인 실패");
                _infoText.text = "failed login";
            }
            else
            {
                // 로그인 성공 시, JSON을 LoginResponse로 변환
                LoginResponse response = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                // 토큰 및 사용자 정보 저장
                User.Nickname = response.nickname;
                User.UserName = id;
                // PlayerPrefs.SetString("UserName", id);
                // PlayerPrefs.SetString("Nickname", response.nickname);
                // PlayerPrefs.Save(); // 변경사항 저장

                // 로비 이동
                _infoText.text = "";
                JoinLobby();
            }
        }
        else
        {
            Debug.LogError($"Error: {request.error}");

            _infoText.text = "wrong id or password";
        }
    }


    private void OnClickSignUp()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        Application.OpenURL(GameConfig.WebsiteURL);
    }

    public void JoinLobby()
    {
        string nickname = _infoText.text;

        if (string.IsNullOrEmpty(nickname))
        {
            // // 로그인 시 저장된 닉네임 할당
            // if (PlayerPrefs.HasKey("Nickname"))
            // {
            //     nickname = PlayerPrefs.GetString("Nickname");
            // }
            if (User.Nickname != null) {
                nickname = User.Nickname;
            }
            else
            {
                // 게스트 로그인 용 닉네임 할당
                nickname = "User" + UnityEngine.Random.Range(100, 999);
            }
        }

        GameConfig.UserNickName = nickname;
        PhotonManager.Inst.SetNickName(GameConfig.UserNickName);

        //포톤 로비로 입장
        PhotonManager.Inst.JoinLobby();
    }

}
