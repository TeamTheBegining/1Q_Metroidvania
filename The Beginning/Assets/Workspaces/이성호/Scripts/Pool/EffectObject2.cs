using System;
using UnityEditor;
using UnityEngine;

public class EffectObject2 : MonoBehaviour, IPoolable
{
    private Animator animator;
    private Player player;
    AnimatorStateInfo stateInfo;
    SpriteRenderer spriteRenderer;
    int state = 1;
    [SerializeField] float posXmax = 4f;
    [SerializeField] float MoveSpeed = 25;
    Vector3 maxPos = Vector3.zero;
    Vector3 minPos = Vector3.zero;
    Vector3 lastPos = Vector3.zero;
    bool iscol = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        lastPos = Vector3.zero;
        maxPos = transform.position + new Vector3(posXmax, 0, 0);
        minPos = transform.position - new Vector3(posXmax, 0, 0);
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        iscol = false;
    }

    public Action ReturnAction { get; set; }

    private void FixedUpdate()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if(!iscol)
        {
            switch (state)
            { 
                case 1:
                    if (maxPos.x > transform.position.x)
                        transform.position += Vector3.right * MoveSpeed * Time.deltaTime;
                    break;
                case -1:
                    if (minPos.x < transform.position.x)
                        transform.position += Vector3.left * MoveSpeed * Time.deltaTime;
                    break;
            }

        }

        if (IsAnimationEnd())
        {
            gameObject.SetActive(false);
            iscol = false;
            player.CurrentState = Player.PlayerState.Idle;
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
            if (!iscol)
            {
                transform.position = new Vector3(collision.transform.position.x, transform.position.y, transform.position.z);
                iscol = true;
            }
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(player.Damage, gameObject);
        }
    }

    private void ColliderEnable()
    {
        gameObject.GetComponent<Collider2D>().enabled = true;
    }

    private void ColliderDisable()
    {
        gameObject.GetComponent<Collider2D>().enabled = false;
    }
}
