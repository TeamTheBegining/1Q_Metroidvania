using System;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static PlayerAnimation;
using static Unity.Burst.Intrinsics.X86.Sse4_2;

//해당 스크립트가 없다면 추가
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimation))]

public class Player : MonoBehaviour, IDamageable
{


    [SerializeField] float moveSpeed = 4.5f;
    [SerializeField] float lmoveSpeed = 2f;
    [SerializeField] float jumpMoveSpeed = 0.5f;
    [SerializeField] float slideSpeed = 3f;


    [SerializeField] float jumpPower = 5f;
    [SerializeField] float ljumpPower = 4f;
    [SerializeField] float groundCheckRadius = 0.2f;


    [SerializeField] float maxHp = 10f;
    [SerializeField] float maxMp = 25f;
    public float currentHp = 10f;
    public float currentMp = 0f;


    [SerializeField] float damage = 2f;
    [SerializeField] float flyPower = 3f;


    float jumpTimer = 0f;
    float parryDelayTimer = 0f;
    float slidingTimer = 0f;
    float slidingDelayTimer = 0f;
    float attackDelayTimer = 0f;
    float climbDir = 0f;

    [SerializeField] float jumpDisableGroundCheckTime = 0.1f; // 점프 후 이 시간만큼 땅 체크 무시
    [SerializeField] float parryDelayTime = 0.5f;
    [SerializeField] float slidingTime = 0.1f;
    [SerializeField] float slidingDelayTime = 1f;
    [SerializeField] float attackDelayTime = 1f;

    int sidx = 0;//슬라이딩 인덱스
    //private float ladderInputHoldTime = 0;
    //[SerializeField] private float ladderEnterDelay = 0.2f;

    private PlayerInput input;
    private PlayerAnimation animatorCtrl;
    private Rigidbody2D rb;
    private Transform groundCheckTransform;
    private LayerMask groundLayer;
    private SpriteRenderer spriternderer;
    private Collider2D attackColl;
    private Collider2D attackColl2;
    private Collider2D attackColl3;
    private Collider2D slidingColl;
    private Collider2D playerColl;
    bool isGround = false;
    bool isparrying = false;
    bool issliding = false;
    bool isparrysuccess = false; 
    bool isLadder = false;
    bool ishit = false;
    bool isattack = false;
    bool attack2able = false;
    bool attack3able = false;
    bool isWallClimbable = false;

    //벽타기 체크 변수
    private WallSensor m_wallSensor1;
    private WallSensor m_wallSensor2;

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
        Parrying,               //패링 - 패링중
        ParrySuccess,           //패링 - 성공 애니메이션 + 적 경직
        ParryCounterAttack,     //패링 - 반격
        ParryReflect,           //패링 - 반사
        ParryKnockback,         //패링 - 밀림
        Ladder,
        Sliding,
        Climbing,
        Dash,
        Hit,
        Dead,
        Crouch
    }

    PlayerFlipState curflip= PlayerFlipState.Right;//현재 플립 상태
    PlayerFlipState preflip= PlayerFlipState.Right;//이전 플립 상태
    private bool isDead;
    [SerializeField]private float spawnDelay = 0.2f;

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
        attackColl = transform.GetChild(1).GetComponent<Collider2D>();
        attackColl2 = transform.GetChild(2).GetComponent<Collider2D>();
        attackColl3 = transform.GetChild(3).GetComponent<Collider2D>();
        slidingColl = transform.GetChild(4).GetComponent<Collider2D>();
        m_wallSensor1 = transform.GetChild(5).GetComponent<WallSensor>();
        m_wallSensor2 = transform.GetChild(6).GetComponent<WallSensor>();
        playerColl = transform.GetComponent<Collider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        wallCheck();
        DeadCheck();
        DelayCheck();
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
            case PlayerState.Climbing:
                PlayerClimbingUpdate();
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

    private void DelayCheck()
    {
        if (isparrying)
            parryDelayTimer += Time.deltaTime;
        if (parryDelayTimer > parryDelayTime)
        {
            isparrying = false;
            parryDelayTimer = 0;
        }
        if (issliding)
            slidingDelayTimer += Time.deltaTime;
        if (slidingDelayTimer > slidingDelayTime)
        {
            issliding = false;
            slidingDelayTimer = 0;
        }
        if (isattack)
            attackDelayTimer += Time.deltaTime;
        if (attackDelayTimer > attackDelayTime)
        {
            isattack = false;
            attackDelayTimer = 0;
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
    void wallCheck()
    {
        if (m_wallSensor1.State() && m_wallSensor2.State())
        {
            isWallClimbable = true;
            climbDir = (float)curflip;
        }
        else
            isWallClimbable = false;
    }


    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    #region Update 모음
    private void PlayerIdleUpdate()
    {
        //혹시 모를 초기화
        rb.linearVelocity = Vector2.zero;
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
        Jumpable();
        Attackable();
        Parryable();
        Ｃrouchable(); 
        Slidingable();
    }
    private void PlayerMoveUpdate()
    {
        if (!isGround)
            currentState = PlayerState.Jump;
        Movable();
        Jumpable();
        Parryable();
        Attackable();
        Ｃrouchable();
        Slidingable();
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
        Climbingable();
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
            attackColl.enabled = false;
            attackColl2.enabled = true;
            currentState = PlayerState.Attack2;
        }
    }
    private void PlayerAttack2Update()
    {
        if(attack3able&& input.IsAttack)
        {
            attack3able = false;
            input.IsAttack = false;
            attackColl2.enabled = false;
            attackColl3.enabled = true;
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
        slidingTimer += Time.deltaTime;
        rb.linearVelocity = new Vector2((float)curflip * slideSpeed, rb.linearVelocity.y);
        if (slidingTimer > slidingTime)
        {
            sidx = Mathf.Min(sidx++, 2);
            PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, transform.position).Init(sidx, (int)curflip == -1 ? true : false);
            slidingTimer = 0;
        }
    }
    private void PlayerClimbingUpdate()
    {

        //슬라이딩키와 동일
        if (input.IsSliding)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = 0.1f;
        }

        if(input.IsJump)
        {
            curflip = (int)curflip == -1 ? PlayerFlipState.Right : PlayerFlipState.Left;
            rb.gravityScale = 1f;
            rb.AddForce(new Vector2((int)curflip,1) * ljumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
        }

        if (isGround || !isWallClimbable || climbDir != input.InputVec.x)
        {
            if (isGround)
            {
                rb.gravityScale = 1f;
                currentState = PlayerState.Idle;
            }
            else
            {
                rb.gravityScale = 1f;
                currentState = PlayerState.Jump;
            }
        }
    }

    #endregion

    #region 움직임 가능 함수
    void Movable()
    {
        if (input.InputVec.x != 0)
        {
            float jumpmove = currentState == PlayerState.Jump ? jumpMoveSpeed : 1f;//점프중에는 좌우로 이동 느리게
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
        if (input.IsAttack && !isattack)
        {
            if (currentState != PlayerState.Jump)
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            input.IsAttack = false;
            currentState = PlayerState.Attack1;
            attackColl.enabled = true;
            isattack = true;
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
    void Ｃrouchable()
    {
        if (input.InputVec.y<0 && isGround)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = PlayerState.Crouch;
        }
    }
    void Slidingable()
    {
        if (input.IsSliding&& isGround && !issliding)
        {
            rb.linearVelocity = Vector2.zero;//슬라이딩 Velocity 적용을 위한 초기화
            currentState = PlayerState.Sliding;
            Input.IsSliding = false;
            issliding = true;
            slidingTimer = 0;
            sidx = 1;
            slidingDelayTimer = 0;
            PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, transform.position).Init(0, (int)curflip == -1 ? true : false);
            playerColl.enabled = false;
            slidingColl.enabled = true;
            gameObject.layer = LayerMask.NameToLayer("Invincibility");
        }
    }
    void Climbingable()
    {
        if (input.InputVec.x!=0 && isWallClimbable && currentState == PlayerState.Jump)//감지 되었을때
        {
            currentState = PlayerState.Climbing;
            rb.linearVelocity = Vector2.zero;//초기화
            rb.gravityScale = 0.1f;
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
        isparrysuccess = false;
    }
    private void SlidingFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        isparrysuccess = false;
        playerColl.enabled = true;
        slidingColl.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
    private void AttackCollider()
    {
        //attackColl.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl.enabled = false;
        attack2able = false;
    }
    private void Attack2Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl2.enabled = false;
        attack3able = false;
    }
    private void Attack3Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl3.enabled = false;
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
        if (collision.tag == "Ladder"&&!issliding)
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

            rb.linearVelocity = Vector2.zero;//초기화 후 진행
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
