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
    [Header("스피드 조절")]
    [Tooltip("플레이어 이동 속도")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float baseMoveSpeed = 4f;
    [Tooltip("플레이어 사다리 이동 속도")]
    [SerializeField] float lmoveSpeed = 2f;
    [Tooltip("플레이어 점프중 이동 속도")]
    [SerializeField] float jumpMoveSpeed = 0.7f;
    [Tooltip("플레이어 슬라이딩 이동 속도")]
    [SerializeField] float slideSpeed = 5f;
    [Tooltip("플레이어 공격중 이동 속도")]
    [SerializeField] float attackMoveSpeed = 0.5f;
    [Space(2)]
    [Header("AddForce 파워 조절")]
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float baseJumpPower = 5f;
    [Tooltip("사다리에서 점프하는 힘")]
    [SerializeField] float ljumpPower = 4f;
    [Tooltip("벽타기 중 점프하는 힘")]
    [SerializeField] float cjumpPower = 4f;
    [Tooltip("피격시 날아가는 힘")]
    [SerializeField] float flyPower = 3f;
    [Space(2)]
    [Header("HP,MP 수치 조정")]
    [SerializeField] float maxHp = 10f;
    [SerializeField] float maxMp = 25f;
    [SerializeField] float currentHp = 10f;
    [SerializeField] float currentMp = 0f;
    [Space(2)]
    [Header("플레이어 공격력")]
    [SerializeField] float baseDamage = 2f;
    [SerializeField] float damage = 2f;

    float groundCheckRadius = 0.21f;//바닥 체크 거리
    float jumpTimer = 0f;
    float parryDelayTimer = 0f;
    float slidingTimer = 0f;
    float slidingDelayTimer = 0f;
    float attackDelayTimer = 0f;
    float moveDelayTimer = 0f;
    float grabDelayTimer = 0f;
    float ladderDelayTimer = 0f;
    float parryCounterTimer = 0;
    float healingTimer = 0;

    [Space(2)]
    [Header("딜레이 시간")]
    [Tooltip("점프 후 시간만큼 땅 체크 무시")]
    [SerializeField] float jumpDisableGroundCheckTime = 0.1f;
    [SerializeField] float moveDelayTime = 0.2f;
    [SerializeField] float grabDelayTime = 0.2f;
    [SerializeField] float ladderDelayTime = 0.2f;
    [SerializeField] float attackDelayTime = 0f;
    [SerializeField] float slidingDelayTime = 1f;
    [SerializeField] float parryDelayTime = 0.8f;
    [SerializeField] float spawnDelayTime = 0.1f;
    [SerializeField] float parryCountTime = 0.3f;

    [Space(2)]
    [Header("현재 상태 - 애니메이션 비교 확인")]
    [SerializeField] private PlayerState currentState;

    int sidx = 0;//슬라이딩 인덱스
    int curParryCount;//패링 카운트
    public int CurParryCount
    {
        get { return curParryCount; } 
    }
    int preParryCount;//패링 카운트
    int healingCount = 0;
    int jumpCount = 0;
    float climbDir = 0f;
    private PlayerInput input;
    private PlayerAnimation animatorCtrl;
    private Rigidbody2D rb;
    private Transform groundCheckTransform;
    private LayerMask groundLayer;
    private SpriteRenderer spriternderer;
    private Collider2D attackColl;
    private Collider2D attackColl2;
    private Collider2D attackColl3;
    private Collider2D skillColl1;
    private Collider2D slidingColl;
    private Collider2D playerColl;
    private Collider2D parryingCounterColl;
    private Bounds grabBounds;
    bool isGround = false;
    bool isParrying = false;
    bool isSliding = false;
    bool isParryAble = false; 
    bool isParrySuccess = false; 
    bool isLadder = false;
    bool isHit = false;
    bool isAttack = false;
    bool isSkill = false;
    bool isMoveDelay = false;
    bool isGrapDelay = false;
    bool isLadderDelay = false;
    bool isWallClimbable = false;
    bool isHealing = false;
    bool attack2Able = false;
    bool attack3Able = false;
    //bool isInteraction = false;

    //벽타기 체크 변수
    private WallSensor m_wallSensor1;
    private WallSensor m_wallSensor2;
    private GrabSensor m_grabSensor;
    private Collider2D m_grabColl;
    private Transform m_grabTransform;
    private Vector3 m_grabPos;
    Vector3 playerleftdown;
    Interactable interactable;
    public float Damage
    {
        get => damage;
        set => damage = value;
    }

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
        Parrying,               //패링 - 패링중
        ParrySuccess,           //패링 - 성공 애니메이션 + 적 경직
        ParryCounterAttack,     //패링 - 반격
        ParryReflect,           //패링 - 반사
        ParryKnockback,         //패링 - 밀림
        Ladder,
        Sliding,
        Climbing,
        Grab,
        GrabSuccess,
        Dash,//사용 X
        Hit,
        Dead,
        Crouch
    }

    PlayerDirectionState curDir = PlayerDirectionState.Right;//현재 방향
    private bool isDead;

    enum PlayerDirectionState
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
        skillColl1 = transform.GetChild(4).GetComponent<Collider2D>();
        slidingColl = transform.GetChild(5).GetComponent<Collider2D>();
        m_wallSensor1 = transform.GetChild(6).GetComponent<WallSensor>();
        m_wallSensor2 = transform.GetChild(7).GetComponent<WallSensor>();
        m_grabSensor = transform.GetChild(8).GetComponent<GrabSensor>();
        m_grabColl = transform.GetChild(9).GetComponent<Collider2D>();
        m_grabTransform = transform.GetChild(9).GetComponent<Transform>();
        playerColl = transform.GetComponent<Collider2D>();
        parryingCounterColl = transform.GetChild(10).GetComponent<Collider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
        curParryCount = 0;
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        //테스트용
        if (input.IsParrying&&input.InputVec.y<0)
        {
            isParryAble = true;
            TakeDamage(95,gameObject);
            input.IsParrying = false;
        }
        if (IsDead) return;
        CheckList();
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
                //PlayerAttack3Update();
                break;
            case PlayerState.Skill1:
                //PlayerSkillk1();
                break;
            case PlayerState.Skill2:
                //PlayerSkillk2();
                break;
            case PlayerState.Parrying:
                //PlayerParryingUpdate();
                break;
            case PlayerState.ParrySuccess:
                ParrySuccessUpdate();
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
            case PlayerState.Grab:
                PlayerGrabUpdate();
                break;
            case PlayerState.GrabSuccess:
                //PlayerGrabSuccessUpdate();
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

    private void CheckList()
    {
        //FlipCheck();
        //ParryingCounterCheck();
        JumpCheck();
        WallCheck();
        ParryCountCheck();
        DelayCheck();
        EnergyOverCheck();
        isGround = CheckIsGround();
    }

    private void ParryCountCheck()
    {
        if(curParryCount != preParryCount)
        {
            switch (curParryCount)
            {
                case 0:
                    damage = baseDamage;
                    jumpPower = baseJumpPower;
                    moveSpeed = baseMoveSpeed;
                    break;
                case 1:
                    damage = baseDamage * 1.25f;
                    break;
                case 2:
                    damage = baseDamage * 1.8f;
                    break;
                case 3:
                    damage = baseDamage * 2.4f;
                    break;
                case 4:
                    //RandomBuff(); �ϴ� ����
                    break;
            }
            preParryCount = curParryCount;
        }
    }


    private void RandomBuff()
    {
        //패링 카운트에서 가중치가 생긴다면 수정해야함
        switch(UnityEngine.Random.Range(0, 4))
        {
            case 0:
                isHealing = true;
                print("0 발동");
                break;
            case 1:
                jumpPower = baseJumpPower * 1.5f;//수정 필요
                print("1 발동");
                break;
            case 2:
                moveSpeed = baseMoveSpeed * 1.2f;
                print("2 발동");
                break;
            case 3:
                print("3 발동");
                break;
            default:
                break;
        }
    }

    private void FlipCheck()
    {
        if (currentState == PlayerState.Parrying|| currentState == PlayerState.Sliding || currentState == PlayerState.Hit || isMoveDelay || currentState == PlayerState.Grab) return;
        if ((int)curDir != Mathf.RoundToInt(input.InputVec.x)&&input.InputVec.x!=0) Flip();
    }

    private void EnergyOverCheck()
    {
        currentHp = currentHp > MaxHp ? MaxHp : currentHp;
        currentMp = currentMp > MaxMp ? MaxHp : currentMp;
    }

    private void DelayCheck()
    {
        if (isParrying)
            parryDelayTimer += Time.deltaTime;

        if (parryDelayTimer > parryDelayTime)
        {
            isParrying = false;
            parryDelayTimer = 0;
        }
        if (isSliding)
            slidingDelayTimer += Time.deltaTime;

        if (slidingDelayTimer > slidingDelayTime)
        {
            isSliding = false;
            slidingDelayTimer = 0;
        }
        if (isMoveDelay)
            moveDelayTimer += Time.deltaTime;

        if (moveDelayTimer > moveDelayTime)
        {
            isMoveDelay = false;
            moveDelayTimer = 0;
        }

        if (isGrapDelay)
            grabDelayTimer += Time.deltaTime;

        if (grabDelayTimer > grabDelayTime)
        {
            isGrapDelay = false;
            grabDelayTimer = 0;
        }

        if (isLadderDelay)
            ladderDelayTimer += Time.deltaTime;

        if (ladderDelayTimer > ladderDelayTime)
        {
            isLadderDelay = false;
            ladderDelayTimer = 0;
        }

        if (isAttack)
            attackDelayTimer += Time.deltaTime;

        if (attackDelayTimer > attackDelayTime)
        {
            isAttack = false;
            attackDelayTimer = 0;
        }

        if (isHealing)
            healingTimer += Time.deltaTime;
        if(healingTimer > 1)
        {
            currentHp += maxHp * 0.05f;
            healingTimer = 0;
            healingCount++;
        }
        if (healingCount > 7)
        {
            isHealing = false;
            healingTimer = 0;
            healingCount = 0;
        }
    }

    void Flip()
    {
        curDir = (int)curDir == 1 ? PlayerDirectionState.Left : PlayerDirectionState.Right;
        transform.eulerAngles = (int)curDir == 1 ? Vector3.zero : new Vector3(0, -180, 0);
    }
    /*private void ParryingCounterCheck()
    {
        if (isParrySuccess)
        {

            parryCounterTimer += Time.deltaTime;
            //추후 방향 및 거리 추가해야함
            if (parryCounterTimer < parryCountTime)
            {
                if (input.IsAttack)
                {
                    currentState = PlayerState.ParryCounterAttack;
                    isParrySuccess = false;
                    parryCounterTimer = 0;
                }
            }
            else
            {
                isParrySuccess = false;
                parryCounterTimer = 0;
            }
        }
    }*/
    void WallCheck()
    {
        if (m_wallSensor1.State() && m_wallSensor2.State())
        {
            isWallClimbable = true;
            climbDir = (float)curDir;
        }
        else
            isWallClimbable = false;
    }
    void JumpCheck()
    {
        if(currentState!=PlayerState.Jump)jumpCount = 0;
    }


    private bool CheckIsGround()
    {
       // return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
        bool onGround = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

        // 점프하거나 낙하 중일 때는 착지로 처리하지 않음
        if (onGround && rb.linearVelocity.y <= 0.1f)
        {
            return true;
        }
        return false;
    }



    #region Update 모음
    private void PlayerIdleUpdate()
    {
        //혹시 모를 초기화
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
        JumpAble();
        AttackAble();
        ParryAble();
        ＣrouchAble(); 
        SlidingAble();
        SkillAble();
    }

    private void PlayerMoveUpdate()
    {
        if (!isGround)
            currentState = PlayerState.Jump;
        MoveAble();
        JumpAble();
        ParryAble();
        AttackAble();
        ＣrouchAble();
        SlidingAble();
        SkillAble();
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

        if (isLadderDelay) return;
        // 사다리 점프
        if (input.IsJump && !(input.InputVec.y < 0))
        {
            isLadderDelay = true;
            isLadder = false;
            input.IsJump = false;
            rb.gravityScale = 1;
            animatorCtrl.AniSpeed = 1f;
            gameObject.layer = LayerMask.NameToLayer("Player");
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
        if (jumpCount == 0) jumpCount = 1; // 점프 상태일 경우 기본 1로 정의
        //if (jumpCount < 2) DoubleJumpAble();
        MoveAble(jumpMoveSpeed);
        AttackAble();
        ClimbingAble();
        GrabAble();
    }

    private void PlayerLandingUpdate()
    {
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
    }
    private void PlayerAttack1Update()
    {
        if (isGround) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (attack2Able&& input.IsAttack)
        {
            attack2Able = false;
            input.IsAttack = false;
            isAttack = true;
            attackColl.enabled = false;
            currentState = PlayerState.Attack2;
            if ((int)curDir != Mathf.RoundToInt(input.InputVec.x) && input.InputVec.x != 0) Flip();
        }
        if (!isGround)
            MoveAble(attackMoveSpeed);
    }
    private void PlayerAttack2Update()
    {
        if (isGround) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (attack3Able&& input.IsAttack&&isGround)
        {
            attack3Able = false;
            input.IsAttack = false;
            isAttack = true;
            attackColl2.enabled = false;
            currentState = PlayerState.Attack3;
            if ((int)curDir != Mathf.RoundToInt(input.InputVec.x) && input.InputVec.x != 0) Flip();
        }
        if (!isGround)
            MoveAble(attackMoveSpeed);
    }


    private void PlayerHitUpdate()
    {
        if(isGround&&!isHit)
        {
            currentState = PlayerState.Idle;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
    }



    private void PlayerCrouchUpdate()
    {
        if(input.InputVec.y >=0)
        {
            currentState = PlayerState.Idle;
        }
        if ((int)curDir != Mathf.RoundToInt(input.InputVec.x)&&input.InputVec.x!=0) Flip();
    }



    private void PlayerSlidingUpdate()
    {
        slidingTimer += Time.deltaTime;
        rb.linearVelocity = new Vector2((float)curDir * slideSpeed, rb.linearVelocity.y);
        if (slidingTimer > spawnDelayTime)
        {
            sidx = Mathf.Min(sidx++, 2);
            PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, transform.position).Init(sidx, (int)curDir == -1 ? true : false);
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
            Flip();
            rb.gravityScale = 1f;
            rb.AddForce(new Vector2((int)curDir*0.5f,1) * cjumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
            isMoveDelay = true;
        }

        if (isGround || !isWallClimbable || climbDir!= Mathf.RoundToInt(input.InputVec.x))
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
    private void ParrySuccessUpdate()
    {
        //패링 성공시 할 행동 추가
    }
    private void PlayerGrabUpdate()
    {
        if(input.InputVec.y <0)
        {
            currentState = PlayerState.Jump;
            rb.gravityScale = 1;
            m_grabColl.enabled = false;
            playerColl.enabled = true;
            isGrapDelay = true;
            transform.position += m_grabPos;
        }
        if(input.InputVec.y>0)
        {
            currentState = PlayerState.GrabSuccess;
            m_grabColl.enabled = false;
        }
    }

    #endregion

    #region 움직임 가능 함수
    void MoveAble(float currentMoveSpeed = 1f)
    {
        if (isMoveDelay) return;
        if (input.InputVec.x != 0)
        {
            rb.linearVelocity = new Vector2(input.InputVec.x * currentMoveSpeed * moveSpeed, rb.linearVelocity.y);
            if ((int)curDir != Mathf.RoundToInt(input.InputVec.x)&&(currentState == PlayerState.Jump|| currentState == PlayerState.Move)) Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if(currentState == PlayerState.Move) 
                currentState = PlayerState.Idle;
        }
    }

    void AttackAble()
    {
        if (isParrySuccess) return;
        if (input.IsAttack && !isAttack)
        {
            input.IsAttack = false;
            currentState = PlayerState.Attack1;
            isAttack = true;
            if (isGround) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void SkillAble()
    {
        //일단 공격 캔슬 불가 사용
        if (input.IsSkill1 && !isAttack && currentMp > 10)
        {
            input.IsSkill1 = false;
            isSkill = true;
            currentState = PlayerState.Skill1;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            currentMp -= 10;
            gameObject.layer = LayerMask.NameToLayer("Invincibility");
        }
    }
    void ParryAble()
    {
        if (input.IsParrying && !isParrying)
        {
            currentState = PlayerState.Parrying;
            rb.linearVelocity = Vector2.zero;
            input.IsParrying = false;   // 패링 입력 종료
            isParrying = true;          // 패링중 확인
            isParryAble = true;         // 패링 적용 확인 변수
        }
    }
    void JumpAble()
    {
        if (input.IsJump && isGround)
        {
            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            currentState = PlayerState.Jump;
            input.IsJump = false;
        }
    }
    void DoubleJumpAble()
    {

        if (isMoveDelay) return;
        if (input.IsJump)
        {
            float powerValue = rb.linearVelocity.y > 0 ? 0.7f : 1.5f;
            rb.AddForce(Vector2.up * jumpPower * powerValue, ForceMode2D.Impulse);
            input.IsJump = false;
            jumpCount++;
        }
    }

    void ＣrouchAble()
    {
        if (input.InputVec.y<0 && isGround)
        {
            rb.linearVelocity = Vector2.zero;
            currentState = PlayerState.Crouch;
        }
    }
    void SlidingAble()
    {
        if (input.IsSliding&& isGround && !isSliding)
        {
            rb.linearVelocity = Vector2.zero;//슬라이딩 Velocity 적용을 위한 초기화
            currentState = PlayerState.Sliding;
            Input.IsSliding = false;
            isSliding = true;
            slidingTimer = 0;
            sidx = 1;
            slidingDelayTimer = 0;
            PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, transform.position).Init(0, (int)curDir == -1 ? true : false);
            playerColl.enabled = false;
            slidingColl.enabled = true;
            gameObject.layer = LayerMask.NameToLayer("Invincibility");
        }
    }
    void ClimbingAble()
    {
        if (input.InputVec.x!=0 && isWallClimbable )//&& currentState == PlayerState.Jump)//감지 되었을때
        {
            currentState = PlayerState.Climbing;
            rb.linearVelocity = Vector2.zero;//초기화
            rb.gravityScale = 0.1f;
        }
    }

    void GrabAble()
    {
        if (isGrapDelay) return;
        if (m_grabSensor.State())
        {
            currentState = PlayerState.Grab;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
            playerleftdown = new Vector3((int)curDir == 1 ? playerColl.bounds.min.x : playerColl.bounds.max.x, playerColl.bounds.min.y, 0);
            m_grabColl.enabled = true;
            playerColl.enabled = false;
            grabBounds = m_grabSensor.GetColliderBounds();
            m_grabPos = new Vector3(m_grabTransform.position.x - ((int)curDir == 1? grabBounds.min.x : grabBounds.max.x), m_grabTransform.position.y - grabBounds.max.y , 0);
            transform.position -= m_grabPos;
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
        isParryAble = false;
    }
    private void ParryingFinish()
    {
        currentState = PlayerState.Idle;
        isParryAble = false;
    }
    private void SlidingFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        //isParryAble = false;
        playerColl.enabled = true;
        slidingColl.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
    private void AttackCollider()
    {
        attackColl.enabled = true;
    }
    private void AttackCollider2()
    {
        attackColl2.enabled = true;
    }
    private void AttackCollider3()
    {
        attackColl3.enabled = true;
    }
    private void SkillCollider1()
    {
        skillColl1.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl.enabled = false;
        attack2Able = false;
        isAttack = false;
    }
    private void Attack2Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl2.enabled = false;
        attack3Able = false;
        isAttack = false;
    }
    private void Attack3Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl3.enabled = false;
        isAttack = false;
    }
    private void Skill1Finish()
    {
        isSkill = false;
        skillColl1.enabled = false;
        currentState = PlayerState.Idle;
    }
    private void ProjectileSkill1()
    {
        PoolManager.Instance.Pop<ProjectilePlayer>(PoolType.ProjectilePlayer, transform.position).Init((int)curDir == -1 ? true : false);
    }

    private void Attack2Check()
    {
        attack2Able = true;
    }

    private void Attack3Check()
    {
        attack3Able = true;
    }

   
    public void PlayerHitFinish()
    {
        isHit = false;
    }

    public void GrabSuccessFinish()
    {
        transform.position += m_grabTransform.position - playerleftdown;

        playerColl.enabled = true;
        currentState = PlayerState.Idle;
        rb.gravityScale = 1;
    }

    private void ParryingCounterAttack()
    {
        parryingCounterColl.enabled = true;
    }

    private void ParryingCounterAttackFinish()
    {
        parryingCounterColl.enabled = false;
        currentState = PlayerState.Idle; 
        gameObject.layer = LayerMask.NameToLayer("Player");
    }

    #endregion

    #region Ʈ����
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Interactable>() != null && input.IsInteraction)
        {
            interactable = collision.gameObject.GetComponent<Interactable>();
            interactable.OnInteraction();
        }

        if (isLadderDelay) return;
        if (collision.CompareTag("Ladder")&&!isLadder && input.InputVec.y != 0)
        {
            float yDiff = transform.position.y - collision.transform.position.y;
            if (isGround && (Mathf.Sign(yDiff) != Mathf.Sign(input.InputVec.y) && Mathf.Abs(yDiff) > 0.1f) || !isGround)
            {
                // 사다리 타기 시작
                rb.gravityScale = 0f;
                currentState = PlayerState.Ladder;
                transform.position = new Vector3(collision.transform.position.x, transform.position.y, transform.position.z);
                isLadder = true;
                rb.linearVelocity = Vector2.zero;
                gameObject.layer = LayerMask.NameToLayer("Ladder");
                isLadderDelay = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder")&&isLadder)
        {
            rb.gravityScale = 1;
            if(CheckIsGround())
                currentState = PlayerState.Idle;
            else
                currentState = PlayerState.Jump;
            isLadder = false;
            isLadderDelay = true;
            animatorCtrl.AniSpeed = 1f;
            gameObject.layer = LayerMask.NameToLayer("Player");
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
        //A_AttackerController enemyscr = enemy.GetComponent<A_AttackerController>();
        if (IsDead) return;

        if (!isParryAble)
        {
            CurrentHp -= damage;
            currentMp += 5;
            currentState = PlayerState.Hit;
            curParryCount = 0;
            isHit = true;
            gameObject.layer = LayerMask.NameToLayer("Invincibility");//피격 중 무적 레이어
            rb.linearVelocity = Vector2.zero;//초기화 후 진행
            rb.gravityScale = 1;

            float direction;
            if (enemy.transform.position.x - transform.position.x > 0)
            {
                direction =  -1f;
                if((int)curDir == direction) Flip();
            }
            else
            {
                direction = 1f;
                if ((int)curDir == direction) Flip();
            }

            rb.AddForce(new Vector2(0.5f * direction, 1) * flyPower, ForceMode2D.Impulse);
            //하위 콜라이더 전부 초기화
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Invincibility")) return;
                col.enabled = false;
            }
            playerColl.enabled = true;
        }
        else
        {
            curParryCount++;//패링 성공 시 카운트 증가
            isParrySuccess = true;
            currentMp += 10;
            currentState = PlayerState.ParrySuccess;
            //enemyscr.Stun();
            //counterTime;
        }
        

    }

    private void PlayerDead()
    {
        OnDead?.Invoke();
        currentState = PlayerState.Dead;
        isDead =true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
    }

}
