using System;
using UnityEngine;

public class EffectObject : MonoBehaviour, IPoolable
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public Action ReturnAction { get; set; }

    private void Update()
    {
        if (IsAnimationEnd())
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        ReturnAction?.Invoke();
    }

    public void OnDespawn()
    {
    }

    public void OnSpawn()
    {
    }

    private bool IsAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}
