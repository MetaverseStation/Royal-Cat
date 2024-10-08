using System;
using UnityEngine;
using Photon.Pun;

// 생명체로서 동작할 게임 오브젝트들입니다. 우리 게임의 경우 플레이어와 중립 몬스터에 해당
// 체력과 데미지 받아들이기, 사망 가능, 사망 이벤트를 제공 // IDamageable 인터페이스를 상속합니다.
public class LivingEntity : MonoBehaviourPun, IDamageable
{
    public float startingHealth = 100f; // 시작 체력
    public float health { get; set; } // 현재 체력
    public bool dead { get; protected set; } // 사망 상태
    public event Action onDeath; // 사망 시 발동할 이벤트

    // 생명체가 활성화될 때 상태를 리셋
    // virtual의 의미 : LivingEntity를 상속받는 클래스에서 해당 메서드를 수정할 수 있음
    protected virtual void OnEnable()
    {
        // 사망하지 않은 상태로 시작
        dead = false;
        // 체력을 시작 체력으로 초기화
        health = startingHealth;
    }

    // 데미지 입는 기능
    public virtual void OnDamage(float damage)
    {
        // 데미지만큼 체력 감소
        health = health > damage ? health - damage : 0;
        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    // 체력을 회복하는 기능
    public virtual void RestoreHealth(float newHealth)
    {
        if (dead)
        {
            // 이미 사망한 경우 체력을 회복할 수 없음
            return;
        }

        // 체력 추가
        health += newHealth;
    }

    // 사망 처리
    public virtual void Die()
    {
        // onDeath 이벤트에 등록된 메서드가 있다면 실행
        if (onDeath != null)
        {
            onDeath();
        }

        // 사망 상태를 true로 변경
        dead = true;
    }
}
