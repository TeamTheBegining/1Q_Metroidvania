using System;
using UnityEngine;

public class TestDummy : MonoBehaviour, IDamageable
{
    public float CurrentHp { get; set; }
    public float MaxHp { get; set; }
    public Action OnHit { get; set; }
    public Action OnDead { get; set; }

    public void TakeDamage(float damage)
    {
        Debug.Log($"Dummy take damage {damage}");
    }
}
