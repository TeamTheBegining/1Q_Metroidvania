using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static PlayerAnimation;

//�ش� ��ũ��Ʈ�� ���ٸ� �߰�
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimation))]

public class Player : MonoBehaviour, IDamageable
{


    [SerializeField] float moveSpeed = 4.5f;
    [SerializeField] float lmoveSpeed = 2f;
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float ljumpPower = 2f;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] bool isLadder = false;
    [SerializeField] float jumpDisableGroundCheckTime = 0.1f; // ���� �� �� �ð���ŭ �� üũ ����
    [SerializeField] float jumpTimer = 0f;
    [SerializeField] float parryTimer = 0f;
    [SerializeField] float parryDelay = 0.5f;
    [SerializeField] float currentHp = 3f;
    [SerializeField] float maxHp = 3f;
    //private float ladderInputHoldTime = 0;
    //[SerializeField] private float ladderEnterDelay = 0.2f;

    private PlayerInput input;
    private PlayerAnimation animatorCtrl;
    private Rigidbody2D rb;
    private Transform groundCheckTransform;
    private LayerMask groundLayer;
    private SpriteRenderer spriternderer;
    private Collider2D attackcoll;
    bool isGround = false;
    bool isparrying = false;

    [SerializeField] private PlayerState currentState;
    public PlayerState CurrentState
    {
        get => currentState;
        set => currentState = value;
    }
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Landing,
        Attack,
        Skill1,
        Skill2,
        Skill3,
        Parrying,
        ParryingCounterAttack,  //�и� - �ݰ�
        ParryingReflect,        //�и� - �ݻ�
        ParryingKnockback,      //�и� - �и�
        Ladder,
        Dodging,
        Dash
    }

    public PlayerInput Input { get => input; }
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxHp; set => maxHp = value; }
    public bool IsDead => currentHp <= 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        animatorCtrl = GetComponent<PlayerAnimation>();
        spriternderer = gameObject.GetComponent<SpriteRenderer>();
        groundCheckTransform = transform.GetChild(0).transform;
        attackcoll = transform.GetChild(1).GetComponent<Collider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
        maxHp = 3;
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        ParryingDelayCheck();
        isGround = CheckIsGround();
        switch (currentState)
        {
            case PlayerState.Idle:
                PlayerIdleUpdate();
                break;
            case PlayerState.Move:
                PlayerMoveUpdate();
                break;
            case PlayerState.Jump:
                PlayerJumpUpdate();
                break;
            case PlayerState.Landing:
                PlayerLandingUpdate();
                break;
            case PlayerState.Attack:
                PlayerAttackUpdate();
                break;
            case PlayerState.Skill1:
                //PlayerSkillk1();
                break;
            case PlayerState.Skill2:
                //PlayerSkillk2();
                break;
            case PlayerState.Skill3:
                //PlayerSkillk3();
                break;
            case PlayerState.Parrying:
                //PlayerParryingUpdate();
                break;
            case PlayerState.ParryingCounterAttack:
                //PlayerParryingCounterAttackUpdate();
                break;
            case PlayerState.ParryingReflect:
                //PlayerParryingReflectUpdate();
                break;
            case PlayerState.ParryingKnockback:
                //PlayerParryingKnockbackUpdate();
                break;
            case PlayerState.Ladder:
                PlayerLadderUpdate();
                break;
            case PlayerState.Dodging:
                //PlayerDodging();
                break;
            case PlayerState.Dash:
                //PlayerDash();
                break;
        }
    }

    private void ParryingDelayCheck()
    {
        if (isparrying)
            parryTimer += Time.deltaTime;
        if (parryTimer > parryDelay)
        {
            isparrying = false;
            parryTimer = 0;
        }
    }

    private void PlayerParryingUpdate()
    {
        if(currentState == PlayerState.Parrying)
            currentState = PlayerState.Idle;
    }

    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    private void PlayerIdleUpdate()
    {
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;

        if (input.IsJump && isGround)
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
            input.IsJump = false;
        }

        if(input.IsParrying && !isparrying)
        {
            currentState = PlayerState.Parrying;
            input.IsParrying = false;   // �и� �Է� ����
            isparrying = true;          // �и��� Ȯ��
        }

        if (input.IsAttack)
        {
            currentState = PlayerState.Attack;
            input.IsAttack = false;
            rb.linearVelocity = Vector2.zero;
        }

    }
    private void PlayerMoveUpdate()
    {
        if (input.InputVec.x != 0)
        {
            rb.linearVelocity = new Vector2(input.InputVec.x * moveSpeed, rb.linearVelocity.y);
            if (transform.localScale.x > 0 && input.InputVec.x < 0)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.position += new Vector3(spriternderer.size.x / 2, 0, 0);
            }
            if (transform.localScale.x < 0 && input.InputVec.x > 0)
            {
                transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                transform.position -= new Vector3(spriternderer.size.x / 2, 0, 0);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            currentState = PlayerState.Idle;
        }

        if (input.IsJump && isGround)
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
            input.IsJump = false;
        }

        if (input.IsParrying && !isparrying)
        {
            currentState = PlayerState.Parrying;
            input.IsParrying = false;   // �и� �Է� ����
            isparrying = true;          // �и��� Ȯ��
            rb.linearVelocity = Vector2.zero;
        }

        if (input.IsAttack)
        {
            currentState = PlayerState.Attack;
            input.IsAttack = false;
            rb.linearVelocity = Vector2.zero;
        }

    }

    private void PlayerLadderUpdate()
    {
        // ��ٸ� �̵�
        if (input.InputVec.y > 0)
        {
            transform.position += Vector3.up * lmoveSpeed * Time.deltaTime;
            animatorCtrl.AniSpeed = 1f;
        }
        else if (input.InputVec.y < 0)
        {
            transform.position += Vector3.down * lmoveSpeed * Time.deltaTime;
            animatorCtrl.AniSpeed = 1f;
        }
        else
        {
            animatorCtrl.AniSpeed = 0f;
        }


        // ��ٸ� ����
        if (input.IsJump && !(input.InputVec.y < 0))
        {
            isLadder = false;
            input.IsJump = false;
            rb.gravityScale = 1;
            animatorCtrl.AniSpeed = 1f;

            rb.AddForce(Vector2.up * ljumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
        }
    }
    private void PlayerJumpUpdate()
    {
        jumpTimer += Time.deltaTime;
        if (input.InputVec.x != 0)
        {
            rb.linearVelocity = new Vector2(input.InputVec.x * moveSpeed, rb.linearVelocity.y);
            if (transform.localScale.x > 0 && input.InputVec.x < 0)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.position += new Vector3(spriternderer.size.x / 2, 0, 0);
            }
            if (transform.localScale.x < 0 && input.InputVec.x > 0)
            {
                transform.localScale = new Vector3(Math.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                transform.position -= new Vector3(spriternderer.size.x / 2, 0, 0);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (jumpTimer > jumpDisableGroundCheckTime&&isGround)
        {
            currentState = PlayerState.Landing;
            jumpTimer = 0;
        }
    }
    private void PlayerLandingUpdate()
    {
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
    }
    private void PlayerAttackUpdate()
    {


    }
    private void LandingFinish()
    {
        if(currentState == PlayerState.Landing) 
            currentState = PlayerState.Idle;
    }
    private void ParryingCheck()
    {
        isparrying = false;
    }
    private void ParryingFinish()
    {
        currentState = PlayerState.Idle;
    }
    private void AttackCollider()
    {
        attackcoll.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = PlayerState.Idle;
        attackcoll.enabled = false;
    }




    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Ladder" && input.InputVec.y!=0 && !isLadder)
        {
            rb.gravityScale = 0f;
            currentState = PlayerState.Ladder;
            transform.position = new Vector3(collision.transform.position.x, transform.position.y, transform.position.z);
            isLadder = true;
            rb.linearVelocity = Vector2.zero; // �ӵ� ����
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ladder")
        {
            rb.gravityScale = 1;
            if(CheckIsGround())
                currentState = PlayerState.Idle;
            else
                currentState = PlayerState.Jump;
            isLadder = false;
            animatorCtrl.AniSpeed = 1f;
        }
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (groundCheckTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
#endif
    }

    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;
        PlayerHit();
    }

    private void PlayerHit()
    {
        // ��Ʈ �� �� ������ �Լ� ����
    }

    private void PlayerDead()
    {
        // ������ ������ �Լ� ����
    }
}
