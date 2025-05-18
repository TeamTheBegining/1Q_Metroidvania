using System;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static PlayerAnimation;
using static Unity.Burst.Intrinsics.X86.Sse4_2;

//�ش� ��ũ��Ʈ�� ���ٸ� �߰�
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimation))]

public class Player : MonoBehaviour, IDamageable
{
    [Header("���ǵ� ����")]
    [Tooltip("�÷��̾� �̵� �ӵ�")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float baseMoveSpeed = 4f;
    [Tooltip("�÷��̾� ��ٸ� �̵� �ӵ�")]
    [SerializeField] float lmoveSpeed = 2f;
    [Tooltip("�÷��̾� ������ �̵� �ӵ�")]
    [SerializeField] float jumpMoveSpeed = 0.7f;
    [Tooltip("�÷��̾� �����̵� �̵� �ӵ�")]
    [SerializeField] float slideSpeed = 5f;
    [Tooltip("�÷��̾� ������ �̵� �ӵ�")]
    [SerializeField] float attackMoveSpeed = 0.5f;
    [Space(2)]
    [Header("AddForce �Ŀ� ����")]
    [SerializeField] float jumpPower = 5f;
    [SerializeField] float baseJumpPower = 5f;
    [Tooltip("��ٸ����� �����ϴ� ��")]
    [SerializeField] float ljumpPower = 4f;
    [Tooltip("��Ÿ�� �� �����ϴ� ��")]
    [SerializeField] float cjumpPower = 4f;
    [Tooltip("�ǰݽ� ���ư��� ��")]
    [SerializeField] float flyPower = 3f;
    [Space(2)]
    [Header("HP,MP ��ġ ����")]
    [SerializeField] float maxHp = 10f;
    [SerializeField] float maxMp = 25f;
    [SerializeField] float currentHp = 10f;
    [SerializeField] float currentMp = 0f;
    [Space(2)]
    [Header("�÷��̾� ���ݷ�")]
    [SerializeField] float baseDamage = 2f;
    [SerializeField] float damage = 2f;

    float groundCheckRadius = 0.21f;//�ٴ� üũ �Ÿ�
    float jumpTimer = 0f;
    float parryDelayTimer = 0f;
    float slidingTimer = 0f;
    float slidingDelayTimer = 0f;
    float attackDelayTimer = 0f;
    float moveDelayTimer = 0f;
    float grabDelayTimer = 0f;
    float parryCounterTimer = 0;
    float healingTimer = 0;

    [Space(2)]
    [Header("������ �ð�")]
    [Tooltip("���� �� �ð���ŭ �� üũ ����")]
    [SerializeField] float jumpDisableGroundCheckTime = 0.1f;
    [SerializeField] float moveDelayTime = 0.2f;
    [SerializeField] float grabDelayTime = 0.2f;
    [SerializeField] float attackDelayTime = 0f;
    [SerializeField] float slidingDelayTime = 1f;
    [SerializeField] float parryDelayTime = 0.8f;
    [SerializeField] float spawnDelayTime = 0.1f;
    [SerializeField] float parryCountTime = 0.3f;

    [Space(2)]
    [Header("���� ���� - �ִϸ��̼� �� Ȯ��")]
    [SerializeField] private PlayerState currentState;

    int sidx = 0;//�����̵� �ε���
    int curParryCount;//�и� ī��Ʈ
    int preParryCount;//�и� ī��Ʈ
    int healingCount = 0;
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
    private Collider2D attackColl4;
    private Collider2D slidingColl;
    private Collider2D playerColl;
    private Bounds grabBounds;
    bool isGround = false;
    bool isParrying = false;
    bool isSliding = false;
    bool isParryAble = false; 
    bool isParrySuccess = false; 
    bool isLadder = false;
    bool isHit = false;
    bool isAttack = false;
    bool isMoveDelay = false;
    bool isGrapDelay = false;
    bool isWallClimbable = false;
    bool isHealing = false;
    bool attack2Able = false;
    bool attack3Able = false;
    bool attack4Able = false;
    bool getAtack4 = false;

    //��Ÿ�� üũ ����
    private WallSensor m_wallSensor1;
    private WallSensor m_wallSensor2;
    private GrabSensor m_grabSensor;
    private Collider2D m_grabColl;
    private Transform m_grabTransform;
    private Vector3 m_grabPos;
    Vector3 playerleftdown;

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
        Attack4,
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
        Climbing,
        Grab,
        GrabSuccess,
        Dash,
        Hit,
        Dead,
        Crouch
    }

    PlayerDirectionState curDir = PlayerDirectionState.Right;//���� ����
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
        attackColl4 = transform.GetChild(4).GetComponent<Collider2D>();
        slidingColl = transform.GetChild(5).GetComponent<Collider2D>();
        m_wallSensor1 = transform.GetChild(6).GetComponent<WallSensor>();
        m_wallSensor2 = transform.GetChild(7).GetComponent<WallSensor>();
        m_grabSensor = transform.GetChild(8).GetComponent<GrabSensor>();
        m_grabColl = transform.GetChild(9).GetComponent<Collider2D>();
        m_grabTransform = transform.GetChild(9).GetComponent<Transform>();
        playerColl = transform.GetComponent<Collider2D>();
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
        curParryCount = 0;
    }

    void Start()
    {

    }

    private void FixedUpdate()
    {
        if (input.IsParrying)
        {
            TakeDamage(95,gameObject);
            input.IsParrying = false;
        }
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
                PlayerAttack3Update();
                break;
            case PlayerState.Attack4:
                //PlayerAttack4Update();
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
        WallCheck();
        DeadCheck();
        ParryCountCheck();
        DelayCheck();
        EnergyOverCheck();
        ParryingCounterCheck();
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
                    RandomBuff();
                    break;
            }
            preParryCount = curParryCount;
        }
    }

    private void RandomBuff()
    {
        //�и� ī��Ʈ���� ����ġ�� ����ٸ� �����ؾ���
        switch(UnityEngine.Random.Range(0, 4))
        {
            case 0:
                isHealing = true;
                print("0 �ߵ�");
                break;
            case 1:
                jumpPower = baseJumpPower * 1.5f;//���� �ʿ�
                print("1 �ߵ�");
                break;
            case 2:
                moveSpeed = baseMoveSpeed * 1.2f;
                print("2 �ߵ�");
                break;
            case 3:
                print("3 �ߵ�");
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
        if (isAttack)
            attackDelayTimer += Time.deltaTime;

        if (attackDelayTimer > attackDelayTime)
        {
            isAttack = false;
            attackDelayTimer = 0;
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
    private void ParryingCounterCheck()
    {
        if (isParrySuccess)
        {

            parryCounterTimer += Time.deltaTime;
            //���� ���� �� �Ÿ� �߰��ؾ���
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
    }
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


    private bool CheckIsGround()
    {
       // return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
        bool onGround = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);

        // �����ϰų� ���� ���� ���� ������ ó������ ����
        if (onGround && rb.linearVelocity.y <= 0.1f)
        {
            return true;
        }
        return false;
    }



    #region Update ����
    private void PlayerIdleUpdate()
    {
        //Ȥ�� �� �ʱ�ȭ
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (input.InputVec.x != 0)
            currentState = PlayerState.Move;
        JumpAble();
        AttackAble();
        ParryAble();
        ��rouchAble(); 
        SlidingAble();
    }

    private void PlayerMoveUpdate()
    {
        if (!isGround)
            currentState = PlayerState.Jump;
        MoveAble();
        JumpAble();
        ParryAble();
        AttackAble();
        ��rouchAble();
        SlidingAble();
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
            attackColl.enabled = false;
            attackColl2.enabled = true;
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
            attackColl2.enabled = false;
            attackColl3.enabled = true;
            currentState = PlayerState.Attack3;
            if ((int)curDir != Mathf.RoundToInt(input.InputVec.x) && input.InputVec.x != 0) Flip();
        }
        if (!isGround)
            MoveAble(attackMoveSpeed);
    }
    private void PlayerAttack3Update()
    {
        if (isGround) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (attack4Able&& input.IsAttack)
        {
            attack4Able = false;
            input.IsAttack = false;
            attackColl3.enabled = false;
            attackColl4.enabled = true;
            currentState = PlayerState.Attack4;
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
        //�����̵�Ű�� ����
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
        //�и� ������ �� �ൿ �߰�
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

    #region ������ ���� �Լ�
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
            attackColl.enabled = true;
            isAttack = true;
            if (isGround) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }
    void ParryAble()
    {
        if (input.IsParrying && !isParrying)
        {
            currentState = PlayerState.Parrying;
            rb.linearVelocity = Vector2.zero;
            input.IsParrying = false;   // �и� �Է� ����
            isParrying = true;          // �и��� Ȯ��
            isParryAble = true;         // �и� ���� Ȯ�� ����
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
    void ��rouchAble()
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
            rb.linearVelocity = Vector2.zero;//�����̵� Velocity ������ ���� �ʱ�ȭ
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
        if (input.InputVec.x!=0 && isWallClimbable )//&& currentState == PlayerState.Jump)//���� �Ǿ�����
        {
            currentState = PlayerState.Climbing;
            rb.linearVelocity = Vector2.zero;//�ʱ�ȭ
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

    #region �̺�Ʈ �Լ�
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
        //attackColl.enabled = true;
    }
    private void AttackFinish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl.enabled = false;
        attack2Able = false;
    }
    private void Attack2Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl2.enabled = false;
        attack3Able = false;
    }
    private void Attack3Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl3.enabled = false;
        attack4Able = false;
    }
    private void Attack4Finish()
    {
        currentState = isGround ? PlayerState.Idle : PlayerState.Jump;
        attackColl4.enabled = false;
    }
    

    private void Attack2Check()
    {
        attack2Able = true;
    }

    private void Attack3Check()
    {
        attack3Able = true;
    }

    private void Attack4Check()
    {
        //�⺻ ���� 4 ��� ��� ��� ����
        if(getAtack4)
            attack4Able = true;
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
            gameObject.layer = LayerMask.NameToLayer("Ladder");
        }

        if (collision.gameObject.GetComponent<Interactable>() != null && input.IsInteraction)
        {
            collision.gameObject.GetComponent<Interactable>().OnInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ladder"&&isLadder)
        {
            rb.gravityScale = 1;
            if(CheckIsGround())
                currentState = PlayerState.Idle;
            else
                currentState = PlayerState.Jump;
            isLadder = false;
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
            gameObject.layer = LayerMask.NameToLayer("Invincibility");//�ǰ� �� ���� ���̾�
            rb.linearVelocity = Vector2.zero;//�ʱ�ȭ �� ����
            rb.gravityScale = 1;
            animatorCtrl.AniSpeed = 1f;
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
            //���� �ݶ��̴� ���� �ʱ�ȭ
            foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Invincibility")) return;
                col.enabled = false;
            }
            playerColl.enabled = true;
            /*m_wallSensor1.enabled = true;
            m_wallSensor2.enabled = true;
            m_grabSensor.enabled = true;*/
        }
        else
        {
            curParryCount++;//�и� ���� �� ī��Ʈ ����
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
