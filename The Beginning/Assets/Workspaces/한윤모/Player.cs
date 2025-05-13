using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static PlayerAnimation;

//해당 스크립트가 없다면 추가
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimation))]

public class Player : MonoBehaviour//, IDamageable, Interactable
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
    //private float ladderInputHoldTime = 0;
    //[SerializeField] private float ladderEnterDelay = 0.2f;

    private PlayerInput input;
    private PlayerAnimation animatorCtrl;
    private Rigidbody2D rb;
    private Transform groundCheckTransform;
    private LayerMask groundLayer;
    private SpriteRenderer spriternderer;
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
        ParryingCounterAttack,  //패링 - 반격
        ParryingReflect,        //패링 - 반사
        ParryingKnockback,      //패링 - 밀림
        Ladder,
        Dodging,
        Dash
    }

    public PlayerInput Input { get => input; }
    /*public float CurrentHp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float MaxHp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action OnHit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Action OnDead { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }*/

    //public bool IsDead => throw new NotImplementedException();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInput>();
        animatorCtrl = GetComponent<PlayerAnimation>();
        spriternderer = gameObject.GetComponent<SpriteRenderer>();
        groundCheckTransform = transform.GetChild(0).transform;
        groundLayer = LayerMask.GetMask("Ground");
        currentState = PlayerState.Idle;
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
                //PlayerAttack();
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
            input.IsParrying = false;   // 패링 입력 종료
            isparrying = true;          // 패링중 확인
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
            input.IsParrying = false;   // 패링 입력 종료
            isparrying = true;          // 패링중 확인
            rb.linearVelocity = Vector2.zero;
        }

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
    private void LandingFish()
    {
        if(currentState == PlayerState.Landing) 
            currentState = PlayerState.Idle;
    }
    private void ParryingCheck()
    {
            //콜라이더 활성화
    }
    private void ParryingFish()
    {
        currentState = PlayerState.Idle;
    }




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
        throw new NotImplementedException();
        //OnHit?.Invoke();
    }

    public void OnInteraction()
    {
        
    }
}
