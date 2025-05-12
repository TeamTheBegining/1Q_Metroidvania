using System;
using UnityEngine;

public interface IDamageable
{
    /// <summary>
    /// 피격 가능한 오브젝트의 현재 체력
    /// </summary>
    public float CurrentHp { get; set; }

    /// <summary>
    /// 피격 가능한 오브젝트의 최대 체력
    /// </summary>
    public float MaxHp { get; set; }

    /// <summary>
    /// 피격 가능한 오브젝트가 피격 시 실행되는 함수
    /// </summary>
    public Action OnHit { get; set; }

    /// <summary>
    /// 피격 가능한 오브젝트가 사망 시 실행되는 함수
    /// </summary>
    public Action OnDead { get; set; }

    public void TakeDamage(float damage);
}
