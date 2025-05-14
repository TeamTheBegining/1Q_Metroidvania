using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static PlayerAnimation;

//해당 스크립트가 없다면 추가
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
    [SerializeField] float jumpDisableGroundCheckTime = 0.1f; // 점프 후 이 시간만큼 땅 체크 무시
    [SerializeField] float jumpTimer = 0f;
    [SerializeField] float parryTimer = 0f;
    [SerializeField] float parryDelay = 0.5f;
    [SerializeField] float maxHp = 10f;
    [SerializeField] float maxMp = 25f;
    [SerializeField] float damage = 2f;
    public float currentHp = 10f;
    public float currentMp = 0f;
    //private float ladderInputHoldTime = 0;
    //[SerializeField] private float ladderEnterDelay = 0.2f;

    private PlayerInput input;
    private PlayerAnimation animatorCtrl;
    private Rigidbody2D rb;
    private Transform groundCheckTransform;
    private LayerMask groundLayer;
    private SpriteRenderer spriternderer;
    private Collider2D attackcoll;
    private Collider2D attackcoll2;
    private Collider2D attackcoll3;
    bool isGround = false;
    bool isparrying = false;
    bool isparrysuccess = false;
    bool attack2able = false;
    bool attack3able = false;

    public float Damage
    {
        get => damage;
        set => damage = value;
    }

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
        Attack1,
        Attack2,
        Attack3,
        Skill1,
        Skill2,
        Skill3,
        Parrying,
        ParryingCounterAttack,  //패링 - 반격
        ParryingReflect,        //패링 - 반사
        ParryingKnockback,      //패링 - 밀림
        Ladder,
        Dodging,
        Dash,
        Hit,
        Dead
    }

    public PlayerInput Input { get => input; }
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxHp; set => maxHp = value; }
    public float CurrentMp { get => currentMp; set => currentMp = value; }
    public float MaxMp { get => maxMp; set => maxMp = value; }
    public bool IsDead => currentHp <= 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        animatorCtrl = GetComponent<PlayerAnimation>();
        spriternderer = gameObject.GetComponent<SpriteRenderer>();
        groundCheckTransform = transform.GetChild(0).transform;
        attackcoll = transform.GetChild(1).GetComponent<Collider2D>();
        attackcoll2 = transform.GetChild(2).GetComponent<Collider2D>();
        attackcoll3 = transform.GetChild(3).GetComponent<Collider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
        currentHp = 10;
        currentMp = 0;
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
            case PlayerState.Attack1:
                PlayerAttack1Update();
                break;
            case PlayerState.Attack2:
                PlayerAttack2Update();
                break;
            case PlayerState.Attack3:
                //PlayerAttack2Update();
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


    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    #region Update 모음
    private void PlayerIdleUpdate()
    {
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
        Jumpable();
        Attackable();
        Parryable();
    }
    private void PlayerMoveUpdate()
    {
        Movable();
        Jumpable();
        Parryable();
        Attackable();
    }

    private void PlayerLadderUpdate()
    {
        // 사다리 이동
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


        // 사다리 점프
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
        if (jumpTimer > jumpDisableGroundCheckTime && isGround)
        {
            currentState = PlayerState.Landing;
            jumpTimer = 0;
        }

        Movable();
        Attackable();
    }
    private void PlayerLandingUpdate()
    {
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
    }
    private void PlayerAttack1Update()
    {
        if(attack2able&& input.IsAttack)
        {
            attack2able = false;
            input.IsAttack = false;
            attackcoll.enabled = false;
            attackcoll2.enabled = true;
            currentState = PlayerState.Attack2;
        }
    }
    private void PlayerAttack2Update()
    {
        if(attack3able&& input.IsAttack)
        {
            attack3able = false;
            input.IsAttack = false;
            attackcoll2.enabled = false;
            attackcoll3.enabled = true;
            currentState = PlayerState.Attack3;
        }
    }

    #endregion

    #region 움직임 가능 함수
    void Movable()
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
            if(currentState == PlayerState.Move) 
                currentState = PlayerState.Idle;
        }
    }
    void Attackable()
    {
        if (input.IsAttack)
        {
            input.IsAttack = false;
            currentState = PlayerState.Attack1;
            attackcoll.enabled = true;
            if (currentState != PlayerState.Jump) 
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }
    void Parryable()
    {
        if (input.IsParrying && !isparrying)
        {
            currentState = PlayerState.Parrying;
            rb.linearVelocity = Vector2.zero;
            input.IsParrying = false;   // 패링 입력 종료
            isparrying = true;          // 패링중 확인
            isparrysuccess = true;      // 패링성공 확인 변수
        }
    }
    void Jumpable()
    {
        if (input.IsJump && isGround)
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
            input.IsJump = false;
        }
    }
    #endregion

    #region 이벤트 함수
    private void LandingFinish()
    {
        if(currentState == PlayerState.Landing) 
            currentState = PlayerState.Idle;
    }
    private void ParryingCheck()
    {
        isparrysuccess = false;
    }
    private void ParryingFinish()
    {
        currentState = PlayerState.Idle;
    }
    private void AttackCollider()
    {
        //attackcoll.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = PlayerState.Idle;
        attackcoll.enabled = false;
        attack2able = false;
    }
    private void Attack2Finish()
    {
        currentState = PlayerState.Idle;
        attackcoll2.enabled = false;
        attack3able = false;
    }
    private void Attack3Finish()
    {
        attackcoll3.enabled = false;
        currentState = PlayerState.Idle;
    }

    private void Attack2Check()
    {
        attack2able = true;
    }

    private void Attack３Check()
    {
        attack3able = true;
    }

    #endregion

    #region 트리거
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Ladder" && input.InputVec.y!=0 && !isLadder)
        {
            rb.gravityScale = 0f;
            currentState = PlayerState.Ladder;
            transform.position = new Vector3(collision.transform.position.x, transform.position.y, transform.position.z);
            isLadder = true;
            rb.linearVelocity = Vector2.zero; // 속도 제거
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
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
    #endregion

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
        if (!isparrysuccess) CurrentHp -= damage;
        PlayerHit();
    }
    private void PlayerHit()
    {
        currentState = PlayerState.Hit;
    }

    private void PlayerDead()
    {
        if(IsDead)
            currentState = PlayerState.Dead;
    }

}
