using System;
using Unity.IO.LowLevel.Unsafe;
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
    [SerializeField] float jumpMoveSpeed = 0.5f;
    [SerializeField] float slideSpeed = 3f;


    [SerializeField] float jumpPower = 5f;
    [SerializeField] float ljumpPower = 2f;
    [SerializeField] float groundCheckRadius = 0.2f;


    [SerializeField] float maxHp = 10f;
    [SerializeField] float maxMp = 25f;
    public float currentHp = 10f;
    public float currentMp = 0f;


    [SerializeField] float damage = 2f;
    [SerializeField] float flyPower = 3f;


    float jumpTimer = 0f;
    float parryTimer = 0f;
    [SerializeField] float jumpDisableGroundCheckTime = 0.1f; // ���� �� �� �ð���ŭ �� üũ ����
    [SerializeField] float parryDelayTime = 0.5f;

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
    bool isLadder = false;
    bool ishit = false;
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
        Parrying,               //�и� - �и���
        ParrySuccess,           //�и� - ���� �ִϸ��̼� + �� ����
        ParryCounterAttack,     //�и� - �ݰ�
        ParryReflect,           //�и� - �ݻ�
        ParryKnockback,         //�и� - �и�
        Ladder,
        Sliding,
        Dash,
        Hit,
        Dead,
        Crouch
    }

    PlayerFlipState curflip= PlayerFlipState.Right;//���� �ø� ����
    PlayerFlipState preflip= PlayerFlipState.Right;//���� �ø� ����
    private bool isDead;

    enum PlayerFlipState
    {
        Left = -1,
        Right = 1
    }

    public PlayerInput Input { get => input; }
    public float CurrentHp 
    { 
        get => currentHp; 
        set
        {
            currentHp = value;

            if(!IsDead&&currentHp<=0)
            {
                PlayerDead();
            }
        }
    }
    public float MaxHp { get => maxHp; set => maxHp = value; }
    public float CurrentMp { get => currentMp; set => currentMp = value; }
    public float MaxMp { get => maxMp; set => maxMp = value; }
    public bool IsDead => isDead;
    public Action OnDead { get; set; }

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
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        DeadCheck();
        ParryingDelayCheck();
        FlipCheck();
        EnergyOverCheck();
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
            case PlayerState.ParrySuccess:
                //ParrySuccessUpdate();
                break;
            case PlayerState.ParryCounterAttack:
                //PlayerParryingCounterAttackUpdate();
                break;
            case PlayerState.ParryReflect:
                //PlayerParryingReflectUpdate();
                break;
            case PlayerState.ParryKnockback:
                //PlayerParryingKnockbackUpdate();
                break;
            case PlayerState.Ladder:
                PlayerLadderUpdate();
                break;
            case PlayerState.Sliding:
                PlayerSlidingUpdate();
                break;
            case PlayerState.Dash:
                //PlayerDash();
                break;
            case PlayerState.Hit:
                PlayerHitUpdate();
                break;
            case PlayerState.Dead:
                //PlayerDash();
                break;
            case PlayerState.Crouch:
                PlayerCrouchUpdate();
                break;
        }
    }

    private void DeadCheck()
    {
        if (IsDead)
        {
            PlayerDead();
        }
    }

    private void EnergyOverCheck()
    {
        currentHp = currentHp > MaxHp ? MaxHp : currentHp;
        currentMp = currentMp > MaxMp ? MaxHp : currentMp;
    }

    private void ParryingDelayCheck()
    {
        if (isparrying)
            parryTimer += Time.deltaTime;
        if (parryTimer > parryDelayTime)
        {
            isparrying = false;
            parryTimer = 0;
        }
    }

    void FlipCheck()
    {
        if (curflip != preflip)
        {
            transform.eulerAngles = curflip == PlayerFlipState.Right ? Vector3.zero : new Vector3(0, -180, 0);
            preflip = curflip;
        }
    }


    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    #region Update ����
    private void PlayerIdleUpdate()
    {
        rb.linearVelocity = Vector2.zero;
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
        Jumpable();
        Attackable();
        Parryable();
        ��rouchable(); 
        Slidingable();
    }
    private void PlayerMoveUpdate()
    {
        Movable();
        Jumpable();
        Parryable();
        Attackable();
        ��rouchable();
        Slidingable();
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


    private void PlayerHitUpdate()
    {
        if(isGround&&!ishit)
        {
            currentState = PlayerState.Idle;
            rb.linearVelocity = Vector2.zero;
        }
    }



    private void PlayerCrouchUpdate()
    {
        if(input.InputVec.y >=0)
        {
            currentState = PlayerState.Idle;
        }
    }



    private void PlayerSlidingUpdate()
    {
        rb.linearVelocity = new Vector2((float)curflip * slideSpeed, rb.linearVelocity.y);
    }

    #endregion

    #region ������ ���� �Լ�
    void Movable()
    {
        if (input.InputVec.x != 0)
        {
            float jumpmove = currentState == PlayerState.Jump ? jumpMoveSpeed : 1f;//�����߿��� �¿�� �̵� ������
            rb.linearVelocity = new Vector2(input.InputVec.x * jumpmove * moveSpeed, rb.linearVelocity.y);
            curflip = input.InputVec.x  < 0? PlayerFlipState.Left :PlayerFlipState.Right;
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
            if (currentState != PlayerState.Jump)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            input.IsAttack = false;
            currentState = PlayerState.Attack1;
            attackcoll.enabled = true;
        }
    }
    void Parryable()
    {
        if (input.IsParrying && !isparrying)
        {
            currentState = PlayerState.Parrying;
            rb.linearVelocity = Vector2.zero;
            input.IsParrying = false;   // �и� �Է� ����
            isparrying = true;          // �и��� Ȯ��
            isparrysuccess = true;      // �и����� Ȯ�� ����
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
    void ��rouchable()
    {
        if (input.InputVec.y<0 && isGround)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = PlayerState.Crouch;
        }
    }
    void Slidingable()
    {
        if (input.IsSliding&& isGround)
        {
            rb.linearVelocity = Vector2.zero;//�����̵� Velocity ������ ���� �ʱ�ȭ
            currentState = PlayerState.Sliding;
            Input.IsSliding = false;
        }
    }
    #endregion

    #region �̺�Ʈ �Լ�
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
        isparrysuccess = false;
    }
    private void SlidingFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        isparrysuccess = false;
    }
    private void AttackCollider()
    {
        //attackcoll.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackcoll.enabled = false;
        attack2able = false;
    }
    private void Attack2Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackcoll2.enabled = false;
        attack3able = false;
    }
    private void Attack3Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackcoll3.enabled = false;
    }

    private void Attack2Check()
    {
        attack2able = true;
    }

    private void Attack��Check()
    {
        attack3able = true;
    }

    #endregion

    #region Ʈ����
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

    public void TakeDamage(float damage, GameObject enemy)
    {
        if (IsDead) return;

        if (!isparrysuccess)
        {
            CurrentHp -= damage;
            currentMp += 5;
            currentState = PlayerState.Hit;
            ishit = true;

            rb.linearVelocity = Vector2.zero;//�ʱ�ȭ �� ����
            float direction;
            if (enemy.transform.position.x - transform.position.x > 0)
            {
                direction =  -1f;
                curflip = PlayerFlipState.Right;
            }
            else
            {
                direction = 1f;
                curflip = PlayerFlipState.Left;
            }

            rb.AddForce(new Vector2(0.5f * direction, 1) * flyPower, ForceMode2D.Impulse);
        }
        else
        {
            currentMp += 10;
            currentState = PlayerState.ParrySuccess;
            enemy.GetComponent<A_AttackerController>().Stun();
        }
        

    }

    private void PlayerDead()
    {
        OnDead?.Invoke();
        currentState = PlayerState.Dead;
        isDead =true;
        rb.linearVelocity = Vector2.zero;
    }

    public void PlayerHitFinish()
    {
        ishit = false;
    }

}
