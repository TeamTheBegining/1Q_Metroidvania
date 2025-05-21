using System;
using UnityEngine;

public class SilverBlue : MonoBehaviour, IDamageable
{
    GameObject      player;
    Animator        animator;
    Rigidbody2D     rb;
    BoxCollider2D   boxCollider2D;
    SpriteRenderer  spriteRenderer;
    BoxCollider2D   hitBoxCollider;

    float distance;
    float randomdir;
    bool  leftdir;
    
    [SerializeField] float findDistance     = 3.0f;
    [SerializeField] float damage           = 0.5f;
    [SerializeField] float attackRange      = 0.75f;
    [SerializeField] float attackdelaytime      = 1.0f;
    [SerializeField] float moveChangetime   = 3.0f;
    [SerializeField] float idleChangetime   = 1.0f;
    [SerializeField] float damagedtime      = 0.2f;
    [SerializeField] float freezetime      = 0.5f;

    float moveChangetimer  = 0.0f;
    float idleChangetimer  = 0.0f;
    float attackdelaytimer = 0.0f;
    float damagedtimer     = 0.0f;
    float freezetimer      = 0.0f;

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
        Freeze,     // 패링 받았을 때
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
        hitBoxCollider  = transform.Find("HitBox").GetComponent<BoxCollider2D>();
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
        AttackDelayCheck();
        ChasingCheck();

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
                animator.Play("attack");
                break;
            case SilverBlueState.Freeze:
                SilverBlueFreezeUpdate();
                animator.Play("freeze");
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
            if (isPlayerFind)
            {
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
            int dir = leftdir ? -1 : 1;
            transform.rotation = Quaternion.Euler(0f, dir < 0 ? 180f : 0f, 0f);

            if (distance > attackRange)
            {
                transform.position += Vector3.right * dir * chasingSpeed * Time.deltaTime;
            }
            else
            {
                currentState = SilverBlueState.Attack;
            }
        }
        else
        {
            // 아무 방향으로라도 이동
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

    private void AttackDelayCheck()
    {
        attackdelaytimer += Time.deltaTime;
        if (attackdelaytimer >= attackdelaytime)
        {
            attackdelaytimer = 0;
        }
    }

    private void HitBoxEnable()
    {
        hitBoxCollider.enabled = true;
    }

    private void HitBoxDisable()
    {
        hitBoxCollider.enabled = false;
        currentState = SilverBlueState.Idle;
    }

    private void ChasingCheck()
    {
        if (distance < findDistance)
        {
            isPlayerFind = true;
        }
        else
        {
            isPlayerFind = false;
        }
    }

    public void ParryingSuccess()
    {
        currentState = SilverBlueState.Freeze;
    }

    private void SilverBlueFreezeUpdate()
    {
        freezetimer += Time.deltaTime;

        if (freezetimer > freezetime)
        {
            currentState = SilverBlueState.Idle;
        }
        
    }

}
