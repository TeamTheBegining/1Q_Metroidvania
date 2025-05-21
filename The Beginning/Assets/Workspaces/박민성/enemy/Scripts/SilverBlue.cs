using System;
using UnityEngine;

public class SilverBlue : MonoBehaviour, IDamageable
{
    GameObject      player;
    Animator        animator;
    Rigidbody2D     rb;
    BoxCollider2D   boxCollider2D;
    SpriteRenderer  spriteRenderer;
    GameObject      attackAHitboxObject;
    BoxCollider2D   attackAHitboxCollider;
    EnemyHitbox     attackAEnemyHitbox;

    float distance;
    float randomdir;
    bool  leftdir;
    
    [SerializeField] float findDistance     = 3.0f;
    [SerializeField] float damage           = 0.5f;
    [SerializeField] float attackRange      = 0.75f;
    [SerializeField] float attackdelay      = 1.0f;
    [SerializeField] float attackTime       = 1.0f;
    [SerializeField] float moveChangetime   = 3.0f;
    [SerializeField] float idleChangetime   = 1.0f;
    [SerializeField] float damagedtime      = 0.2f;

    float moveChangetimer;
    float idleChangetimer;
    float attackTimer;
    float damagedtimer;

    float movespeed         = 1.0f;
    float chasingSpeed      = 2.0f;
    private float currentHp = 5.0f;
    private float maxHp     = 5.0f;

    bool isPlayerFind = false;
    bool isDamaged    = false;


    public enum SilverBlueState
    {
        Idle,
        Move,
        Attack,
        Freeze,     // Ìå®ÎßÅ Î∞õÏïòÏùÑ Îïå
        Death
    }

    public SilverBlueState currentState = SilverBlueState.Idle;
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxHp; set => maxHp = value; }
    public float Damage
    {
        get => damage;
        set => damage = value;
    }
    public Action OnDead { get; set; }
    public bool IsDead { get => currentState == SilverBlueState.Death; set { if (value) currentState = SilverBlueState.Death; } }

    public void TakeDamage(float damage, GameObject attackObject)
    {
        isDamaged = true;
        currentHp -= damage;

        if (CurrentHp <= 0)
        {
            IsDead = true;
            currentState = SilverBlueState.Death;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            OnDead?.Invoke();
        }
    }

    public void SilverBlueDamaged()
    {
        if (isDamaged)
        {
            spriteRenderer.color = new Color(1.0f, 0, 0, 1.0f);

            damagedtimer += Time.deltaTime;
            if (damagedtimer > damagedtime)
            {
                spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                damagedtimer = 0;
                isDamaged = false;
            }
            
        }
    }

    private void Awake()
    {
        animator        = GetComponent<Animator>();
        rb              = GetComponent<Rigidbody2D>();
        boxCollider2D   = GetComponent<BoxCollider2D>();
        spriteRenderer  = GetComponent<SpriteRenderer>();
        attackAHitboxObject = GetComponent<GameObject>();
        attackAHitboxCollider = GetComponent<BoxCollider2D>();
        attackAEnemyHitbox = GetComponent<EnemyHitbox>();
        moveChangetimer = 0;
        randomdir       = 1.0f;
        leftdir         = false;        
    }

    private void OnEnable()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
    }

    private void FixedUpdate()
    {
        SilverBlueDamaged();

        distance = Vector2.Distance(transform.position, player.transform.position);
        leftdir = player.transform.position.x < transform.position.x ? true : false;

        switch (currentState)
        {
            case SilverBlueState.Idle:
                SilverBlueIdleUpdate();
                animator.Play("Idle");
                break;
            case SilverBlueState.Move:
                SilverBlueMoveUpdate();
                animator.Play("move");
                break;
            case SilverBlueState.Attack:
                SilverBlueAttackUpdate();
                animator.Play("attack");
                break;
            case SilverBlueState.Freeze:
                //SilverBlueFreezeUpdate();
                break;
            case SilverBlueState.Death:
                animator.Play("death");
                break;
        }
    }

    private void SilverBlueIdleUpdate()
    {
        if (player != null)
        {
            if (distance < findDistance)
            {
                isPlayerFind = true;
                currentState = SilverBlueState.Move;
            }
            else
            {
                moveChangetimer += Time.deltaTime;
                if (moveChangetimer > moveChangetime)
                {
                    moveChangetimer = 0;
                    randomdir *= -1.0f;
                    currentState = SilverBlueState.Move;
                }
            }
        }
    }

    private void SilverBlueMoveUpdate()
    {
        if (isPlayerFind)
        {
            if (distance > attackRange)
            {
                int dir = leftdir ? -1 : 1;
                transform.position += Vector3.right * dir * chasingSpeed * Time.deltaTime;
            }
            else
            {
                currentState = SilverBlueState.Attack;
            }
        }
        else
        {
            // ÏïÑÎ¨¥ Î∞©Ìñ•ÏúºÎ°úÎùºÎèÑ Ïù¥Îèô
            transform.position += Vector3.right * randomdir * movespeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, randomdir < 0 ? 180f : 0f, 0f);
            
            idleChangetimer += Time.deltaTime;
            if (idleChangetimer > idleChangetime)
            {
                idleChangetimer = 0;
                currentState = SilverBlueState.Idle;
            }
        }
    }

    private void SilverBlueAttackUpdate()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackTime)
        {
            attackTimer = 0;

            // «√∑π¿ÃæÓ∞° æ∆¡˜ π¸¿ß ≥ªø° ¿÷¥¬¡ˆ »Æ¿Œ
            if (distance <= attackRange)
            {
                // ∞¯∞› ∑Œ¡˜ (øπ: µ•πÃ¡ˆ ¡÷±‚)
                IDamageable playerDamageable = player.GetComponent<IDamageable>();
                playerDamageable?.TakeDamage(damage, gameObject);
            }

            // ∞¯∞› »ƒ Idle »§¿∫ Move∑Œ ∫π±Õ
            currentState = SilverBlueState.Idle;
        }
    }
    public void EnableAttackHitbox()
    {
        // Check Base IsDead flag
        if (IsDead) return; // IsDead º”º∫ ªÁøÎ // Do not enable hitbox if dead

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            // Set damage value on the EnemyHitbox component
            // Assumes EnemyHitbox.cs has a public float attackDamage; variable.
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = damage; // Set damage from A_Attacker's value
            }

            attackAHitboxCollider.enabled = true; // Enable collider

        }
    }

}
