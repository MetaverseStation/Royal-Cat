    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using Photon.Pun;

    public class ItemboxController : MonoBehaviourPun
    {
        [Header("Item Box Settings")]
        public GameObject itembox;     
        public Slider hpBarSlider;  
        public WeightedRandomList<Transform> lootTable;  

        [Header("Health Settings")]
        [SerializeField] private float _maxHp = 3f;  
        private float _currentHp;
        private bool _isDestroyed = false; 

        [Header("Shake Settings")]
        [SerializeField] private float _shakeMagnitude = 0.25f; 
        private float _shakeDuration; 
        private Vector3 _originalPosition;  

        private void Start()
        {
            InitializeItemBox();
        }

        private void Update()
        {
            UpdateHpBar();       
            HandleShakeEffect(); 
        }

        // 아이템 박스의 초기 설정
        private void InitializeItemBox()
        {
            _currentHp = _maxHp;                      
            _originalPosition = itembox.transform.position;
        }

        // 체력 바 업데이트 (Smooth하게 체력이 줄어들게 처리)
        private void UpdateHpBar()
        {
            if (hpBarSlider != null)
            {
                hpBarSlider.value = Mathf.Lerp(hpBarSlider.value, _currentHp / _maxHp, Time.deltaTime * 5f);
            }
        }

        // 흔들림 효과 처리
        private void HandleShakeEffect()
        {
            if (_shakeDuration > 0)
            {
                itembox.transform.position = _originalPosition + Random.insideUnitSphere * _shakeMagnitude;
                _shakeDuration -= Time.deltaTime;
            }
            else
            {
                itembox.transform.position = _originalPosition; 
            }
        }

        // 충돌 시 처리 (throwingObject만 충돌 처리)
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag("throwingObject") && _currentHp > 0)
            {
                photonView.RPC("StartShake", RpcTarget.All); 
                ApplyDamage();
            }
        }

        private void ApplyDamage()
        {
            _currentHp -= 1f;

            // 모든 클라이언트에서 체력 동기화
            photonView.RPC("SyncHp", RpcTarget.All, _currentHp);

            if (_currentHp <= 0)
            {
                photonView.RPC("HandleItemBoxDestruction", RpcTarget.MasterClient);
            }
        }

        // 흔들림 효과 RPC (모든 클라이언트에서 실행)
        [PunRPC]
        private void StartShake()
        {
            _shakeDuration = 0.2f;
        }

        [PunRPC]
        // 상자 파괴 및 아이템 생성 처리
        private void HandleItemBoxDestruction()
        {
            if (PhotonNetwork.IsMasterClient && !_isDestroyed)
            {
                _isDestroyed = true;
                // 상자를 파괴
                PhotonNetwork.Destroy(gameObject);

                // 아이템을 랜덤으로 생성
                Transform item = lootTable.GetRandom();
                if (item != null)
                {
                    string prefabPath = "Prefabs/SpawnedItem/" + item.gameObject.name;
                    Vector3 itemSpawnPos = _originalPosition + Vector3.up * 0.5f;
                    PhotonNetwork.Instantiate(prefabPath, itemSpawnPos, Quaternion.identity);  // 네트워크 상에서 아이템 생성
                }
            }
        }

        // 체력 동기화 RPC (모든 클라이언트에서 실행)
        [PunRPC]
        private void SyncHp(float newHp)
        {
            _currentHp = newHp;
        }
    }
