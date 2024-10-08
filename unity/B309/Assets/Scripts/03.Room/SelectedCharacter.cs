using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Linq;



public class SelectedCharacter : MonoBehaviourPunCallbacks
{
    public string faceTexturePath = "Materials/Cat_Face/T_Chibi_Emo_";
    public string skinMaterialPath = "Materials/Cat_Skin/M_Chibi_Cat_";
    public string AnimationName = "Animation_";

    public GameObject LeftButton;
    public GameObject RightButton;

    private Renderer _catRenderer;
    private Animator _animator;
    private Material[] _materials;
    private RoomManager _roomManager;

    private int _maxFace = 27; // 27개
    private int _maxSkin = 10; // 10개
    private int _maxAnimation = 10; // 10개

    public int skinIndex;
    public int faceIndex;

    void Start()
    {
        _catRenderer = GetComponent<Renderer>();
        _materials = _catRenderer.materials; // 0: 스킨, 1: 얼굴
        _animator = transform.parent.GetComponent<Animator>();

        skinIndex = 1;
        faceIndex = 1;

        _roomManager = GameObject.Find("Room").GetComponent<RoomManager>();
    }

    public void Prev()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        if (skinIndex - 1 < 1)
        {
            return;
        }

        if (!RightButton.activeSelf)
        {
            RightButton.SetActive(true);
        }

        ChangeSkin(--skinIndex);

        if (skinIndex == 1)
        {
            LeftButton.SetActive(false);
        }
    }

    public void Next()
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
        if (skinIndex + 1 > _maxSkin)
        {
            return;
        }

        if (!LeftButton.activeSelf)
        {
            LeftButton.SetActive(true);
        }

        ChangeSkin(++skinIndex);

        if (skinIndex == _maxSkin)
        {
            RightButton.SetActive(false);
        }
    }

    public void ChangeSkin(int newSkinIndex)
    {
        Material newMaterial = Resources.Load<Material>(skinMaterialPath + newSkinIndex);
        faceIndex = UnityEngine.Random.Range(1, _maxFace);
        Texture newTexture = Resources.Load<Texture>(faceTexturePath + faceIndex);

        _materials[0] = newMaterial;
        _materials[1].SetTexture("_MainTex", newTexture);

        _catRenderer.materials = _materials;
        _animator.Play(AnimationName + UnityEngine.Random.Range(1, _maxAnimation));
    }
    public void ChangeSkin(int newSkinIndex, int newFaceIndex)
    {
        if (newSkinIndex == 1)
        {
            LeftButton.SetActive(false);
        }
        else if (newSkinIndex == _maxSkin)
        {
            RightButton.SetActive(false);
        }

        skinIndex = newSkinIndex;

        Material newMaterial = Resources.Load<Material>(skinMaterialPath + newSkinIndex);
        Texture newTexture = Resources.Load<Texture>(faceTexturePath + newFaceIndex);

        _materials[0] = newMaterial;
        _materials[1].SetTexture("_MainTex", newTexture);

        _catRenderer.materials = _materials;
        _animator.Play(AnimationName + UnityEngine.Random.Range(1, _maxAnimation));
    }

    public void SelectSkin()
    {
        if (_roomManager.isSelectedSkin[skinIndex])
        {
            UIManager.Inst.SetPopupCustum("이미 선택된 스킨입니다.", true, null, null, "확인", null);
            return;
        }

        // 스킨이 변경되면 플레이어 설정 값에 반영
        Debug.Log(skinIndex + "번으로 스킨 설정");
        PhotonManager.Inst.SetPlayerCustomProperty<int>(skinIndex, "skinIndex");
        PhotonManager.Inst.SetPlayerCustomProperty<int>(faceIndex, "faceIndex");
        Debug.Log("selectedSkin의 faceIndex" + faceIndex);
    }
}
