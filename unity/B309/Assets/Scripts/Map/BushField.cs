using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Collections;

public class BushField : MonoBehaviourPunCallbacks
{
    // 사람
    private PhotonView _localPlayerPhotonView;
    private PhotonView _photonView;
    [SerializeField] private float _visibilityRange = 2f;

    // 부쉬
    private float _transparentAlpha = 0.1f;
    private Dictionary<Bush, float> _transparentBushes = new Dictionary<Bush, float>(); // 투명해진 부쉬들을 추적
    [SerializeField] private bool _isInBush;
    private Coroutine _exitCoroutine;

    void Start()
    {    
        // BushField가 Bush 군집의 크기에 맞게 설정하는 코드
        _photonView = GetComponent<PhotonView>();
        // LocalPlayer의 포톤뷰를 가져오는 코드. 이걸 활용해서 주변 부쉬 투명화 및 플레이어 찾기를 할 예정
        Invoke("InitializeLocalPlayerPhotonView", 2f);
    }

    void Update()
    {
         if(_isInBush){
            UpdatePlayerVisibility();
            UpdateBushTransparency();
        }
    }
    
    void InitializeLocalPlayerPhotonView() 
    {
        GameObject localPlayerObject = GameObject.FindGameObjectWithTag("Player");

        if (localPlayerObject != null)
            _localPlayerPhotonView = localPlayerObject.GetComponent<PhotonView>();
        else
            Debug.LogWarning("Local player object with tag 'Player' not found.");
    }

void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Player"))
    {
        PhotonView photonView = other.GetComponent<PhotonView>();
        int viewID = photonView.ViewID;
        GameManager.Inst._playersInBush.Add(viewID);
        if (photonView != null && photonView.IsMine)
        {
                
            // 가시성 업데이트 시작
            _isInBush = true;
            SetVisibility(photonView, false);
        }
    }
}


void OnTriggerExit(Collider other)
{
    if (other.CompareTag("Player"))
    {
        PhotonView photonView = other.GetComponent<PhotonView>();
        int viewID = photonView.ViewID;
        GameManager.Inst._playersInBush.Remove(viewID);
        if (photonView != null && photonView.IsMine)
        {
          
                Debug.Log("OnTriggerExit 호출: "+photonView.ViewID);
                // Null 체크 추가

                // _isInBush = false;
                // // 부쉬 필드 내 모든 Bush 오브젝트의 투명도를 초기화
                // ResetAllBushesTransparency();
                // // 가시필드를 나간 플레이어의 가시성을 다시 원래대로 설정
                // SetVisibility(photonView, true);
                // // 부쉬 필드에 남아있는 플레이어의 가시성을 다시 안보이도록 설정
                
                // // 왜 0.3초부터는 안돼.....
                // Invoke("HidePlayersInBush",0.4f);
                 if (_exitCoroutine != null)
                {
                    StopCoroutine(_exitCoroutine);
                }
                
                // 새 코루틴 시작
                _exitCoroutine = StartCoroutine(ExitBushCoroutine(photonView));
        }
    }
}
    private IEnumerator ExitBushCoroutine(PhotonView photonView)
    {
        
        _isInBush = false;
        // 한 프레임 대기
        // 코루틴을 활용해버려 ㅠ
        yield return new WaitForSeconds(0.2f);
        
        // 부쉬 필드 내 모든 Bush 오브젝트의 투명도를 초기화
        ResetAllBushesTransparency();
        
        // 가시필드를 나간 플레이어의 가시성을 다시 원래대로 설정
        SetVisibility(photonView, true);
        
        // 부쉬 필드에 남아있는 플레이어의 가시성을 다시 안보이도록 설정
        yield return new WaitForSeconds(0.2f);
        HidePlayersInBush();
    }
private void ResetAllBushesTransparency()
{
    Bush[] bushes = GetComponentsInChildren<Bush>();

    foreach (Bush bush in bushes)
    {
        bush.ResetBushTransparency();
    }
    _transparentBushes = new Dictionary<Bush, float>();
}

private void SetVisibility(PhotonView playerView, bool isVisible)
{
    if (playerView != null)
    {
        Debug.Log("SetVisibility 호출: "+playerView.ViewID + " " + isVisible);
        // 현재 클라이언트를 제외하고 다른 클라이언트에게만 RPC 호출
        playerView.RPC("SetVisibility", RpcTarget.OthersBuffered, isVisible);
    }
}


    private void UpdateBushTransparency()
    {
        if (_localPlayerPhotonView == null) return;

        // 플레이어 주변 3f 내에 있는 부쉬 탐색
        Collider[] hitColliders = Physics.OverlapSphere(_localPlayerPhotonView.transform.position, _visibilityRange, LayerMask.GetMask("Bush"));

        HashSet<Bush> currentlyNearbyBushes = new HashSet<Bush>();

        foreach (Collider hitCollider in hitColliders)
        {
            Bush bush = hitCollider.GetComponent<Bush>();
            if (bush != null)
            {
                currentlyNearbyBushes.Add(bush); // 현재 가까이 있는 부쉬들 저장
                
                // 투명성이 이미 설정된 부쉬인지 확인
                if (!_transparentBushes.ContainsKey(bush))
                {
                    bush.SetBushTransparency(_transparentAlpha);
                    _transparentBushes[bush] = _transparentAlpha; // 새로 투명하게 만든 부쉬 추적
                }
            }
        }

        // 현재 가까이 있지 않은 투명 부쉬들은 투명성을 복원
        List<Bush> bushesToRestore = new List<Bush>();
        foreach (var bush in _transparentBushes.Keys)
        {
            if (!currentlyNearbyBushes.Contains(bush))
            {
                bush.ResetBushTransparency();
                bushesToRestore.Add(bush); // 나중에 투명성 목록에서 제거하기 위해 임시 저장
            }
        }

        // 투명성 복원된 부쉬를 목록에서 제거
        foreach (var bush in bushesToRestore)
        {
            _transparentBushes.Remove(bush);
        }
    }

    private void UpdatePlayerVisibility()
    {
        if (_localPlayerPhotonView == null)
            return;

        if(_localPlayerPhotonView.IsMine){

        foreach (int viewID in GameManager.Inst._playersInBush)
        {
            PhotonView otherPlayerView = PhotonView.Find(viewID);

            if (otherPlayerView != null && otherPlayerView != _localPlayerPhotonView)
            {
                // 특정 거리보다 가까운 플레이어가 있을 경우 서로에게 보이게 SetVisibilityToPlayer 메서드를 실행한다. -> PlayerMovement.cs 286번 코드
                float distance = Vector3.Distance(_localPlayerPhotonView.transform.position, otherPlayerView.transform.position);
                bool shouldSee = distance <= _visibilityRange;

                // 로컬 플레이어의 가시성 업데이트
                otherPlayerView.gameObject.GetComponent<PlayerMovement>().SetVisibility(shouldSee);

                // 다른 플레이어에게 로컬 플레이어의 가시성 업데이트 요청
                Debug.Log("UpdatePlayerVisibility 호출: "+otherPlayerView.ViewID +  "에게 "+ _localPlayerPhotonView.ViewID + "가 " + shouldSee);
                _localPlayerPhotonView.RPC("SetVisibilityToPlayer", otherPlayerView.Owner, _localPlayerPhotonView.ViewID, shouldSee);
            }
        }
    }
        }

    private void HidePlayersInBush(){
        if (_localPlayerPhotonView == null)
            return;

        // 부쉬 안의 플레이어들이 로컬 플레이어를 못보도록 처리
        if(_localPlayerPhotonView.IsMine){
            foreach (int viewID in GameManager.Inst._playersInBush)
            {
                PhotonView otherPlayerView = PhotonView.Find(viewID);
                if (otherPlayerView != null && otherPlayerView != _localPlayerPhotonView)
                {
                    Debug.Log("HidePlayersInBush 호출: "+otherPlayerView.ViewID + " " + false);
                    _localPlayerPhotonView.RPC("SetVisibilityToPlayer", _localPlayerPhotonView.Owner,otherPlayerView.ViewID, false);
                }
            }

        }
    }
}
