using System;
using UnityEngine;

public class TestDummy : MonoBehaviour, IDamageable
{
    private float currentHp = 0f;
    public float CurrentHp 
    { 
        get => currentHp; 
        set
        {
            currentHp = Mathf.Clamp(value, 0f, maxHp);

            if(currentHp <= 0f)
            {
                // 사망
                Debug.Log($"Dummy dead");
                 
                isDead = true;
            }
        }
    }

    private float maxHp = 10f;
    public float MaxHp 
    { 
        get => maxHp;
        set => maxHp = value; 
    }

    private bool isDead = false;
    public bool IsDead { get => isDead; }

    private void Awake()
    {
        CurrentHp = MaxHp;
        isDead = false; 
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        Debug.Log($"Dummy take damage {damage}");
        CurrentHp -= damage;
    }

    public void ResetHp()
    {
        CurrentHp = maxHp;
        isDead = false; 
    }
}
