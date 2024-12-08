using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 데미지를 입을 수 있는 타입들이 공통적으로 가져야 하는 인터페이스 : 우리 게임의 경우 플레이어와 몬스터, 상자 정도
public interface IDamageable
{
    // 데미지를 입을 수 있는 타입들은 IDamageable을 상속하고 OnDamage 메서드를 반드시 구현해야 합니다.
    // == 데미지를 입혔는지 판정할 때는 충돌한 오브젝트에 OnDamage 메서드가 있는지만 확인하면 됩니다.
    void OnDamage(float damage);
}
