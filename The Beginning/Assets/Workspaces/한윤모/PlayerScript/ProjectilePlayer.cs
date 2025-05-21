using System;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ProjectilePlayer : MonoBehaviour, IPoolable
{
    private Animator animator;
    private Player player;
    float MoveSpeed;
    public AnimationCurve anicurv;
    AnimatorStateInfo stateInfo;
    SpriteRenderer spriteRenderer;
    float anitimer = 0;
    int state = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();   
    }

    public Action ReturnAction { get; set; }

    private void OnEnable()
    {
        if(player  == null) player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    private void Update()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        MoveSpeed = anicurv.Evaluate(stateInfo.normalizedTime);
        transform.position += Vector3.right * state * MoveSpeed * Time.deltaTime;
        if (IsAnimationEnd())
        {
            gameObject.SetActive(false);
        }
    }
    public void Init(bool isFilp = false)
    {
        spriteRenderer.flipX = isFilp;
        state = isFilp ? -1 : 1;
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(player.Damage, gameObject);
        }

    }
}
