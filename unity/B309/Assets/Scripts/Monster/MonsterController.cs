using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class MonsterController : LivingEntity, IPunObservable
{
    // Itembox ���� �ʵ�
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    public float _maxHealth = 10000f;

    // ���� ����
    public WeightedRandomList<Transform> lootTable;

    protected override void OnEnable()
    {
        base.OnEnable();
        health = _maxHealth;
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
        // 죽은 뒤에도 체력바 흔드는 함수 호출하지 않게 막기
        if (!dead)
        {
            healthText.text = $"{health} / {_maxHealth}";
            photonView.RPC("StartShake", RpcTarget.All);
            photonView.RPC("SliderSync", RpcTarget.All, health);
        }
    }

    [PunRPC]
    private void SliderSync(float updatedHealth)
    {
        healthSlider.value = updatedHealth;
    }

    public override void Die()
    {
        AudioManager.Inst.PlaySfx(AudioManager.Sfx.MonsterDead);
        base.Die();
        if (photonView.IsMine)
        {
            photonView.RPC("MonsterAfterDeath", RpcTarget.All);
        }
    }

    [PunRPC]
    private void MonsterAfterDeath()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            Transform item = lootTable.GetRandom();
            if (item != null)
            {
                string prefabPath = "Prefabs/SpawnedItem/" + item.gameObject.name;
                GameObject spawnedItem = PhotonNetwork.Instantiate(prefabPath, transform.position, Quaternion.identity);
            }
        }
    }

    [PunRPC]
    private void StartShake()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        float t = 2f;
        float shakePower = 0.05f;
        Vector3 origin = transform.position;

        while (t > 0f)
        {
            t -= 0.05f;
            transform.position = origin + (Vector3)UnityEngine.Random.insideUnitCircle * shakePower * t;
            yield return null;
        }
        transform.position = origin;
    }

    // IPunObservable�� �޼��� ���� - HP ����ȭ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)  // �����͸� ������ ��
        {
            stream.SendNext(health);
        }
        else  // �����͸� �޴� ��
        {
            health = (float)stream.ReceiveNext();
        }
    }
}
