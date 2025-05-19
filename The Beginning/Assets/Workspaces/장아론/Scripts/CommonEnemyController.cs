using System;
using UnityEngine;
using System.Collections;

public abstract class CommonEnemyController : MonoBehaviour, IDamageable
{
    [Header("Base Enemy Stats")]
    protected float _currentHealth;
    public float _maxHealth = 10f;
    protected bool _isDead = false;
    protected Animator animator;

    protected bool isPerformingHurtAnimation = false;

    public float CurrentHp
    {
        get => _currentHealth;
        set => _currentHealth = value;
    }

    public float MaxHp
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    public bool IsDead => _isDead;

    private Action _onDeadAction;
    public Action OnDead
    {
        get => _onDeadAction;
        set => _onDeadAction = value;
    }

    [Header("Player Tracking")]
    [SerializeField] protected Transform playerTransform;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float moveSpeed = 2f;
    // ======================================================================
    // **새로운 변수: 공격 범위 진입 전 이동을 멈출 여유 공간**
    public float attackStopBuffer = 0.1f; // 0.1 ~ 0.3 정도의 작은 값을 설정해 보세요.
    // ======================================================================

    [Header("Attack State")]
    protected bool isPerformingAttackAnimation = false;
    protected bool isWaitingAfterAttack = false;
    public float postAttackWaitDuration = 0.5f;

    protected Rigidbody2D rb;

    public static event Action<GameObject> OnEnemyDiedGlobal;

    protected virtual void Awake()
    {
        CurrentHp = MaxHp;
        animator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError(gameObject.name + ": Rigidbody2D 컴포넌트를 찾을 수 없습니다! 이동이 제대로 동작하지 않을 수 있습니다.", this);
        }
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
        if (IsDead || isPerformingHurtAnimation)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            PlayIdleAnim();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // ======================================================================
            // **수정: 플레이어 감지 시 항상 플레이어 방향을 바라보도록**
            // 이동 로직보다 먼저 실행되어 공격/이동 결정 전 항상 정면을 보게 함.
            // ======================================================================
            if (distanceToPlayer <= detectionRange) // 감지 범위 안에 있을 때만 방향 전환 로직 실행
            {
                float directionX = playerTransform.position.x - transform.position.x;
                if (directionX > 0)
                {
                    Flip(false); // 오른쪽 바라보기
                }
                else if (directionX < 0)
                {
                    Flip(true); // 왼쪽 바라보기
                }
            }
            // ======================================================================

            if (distanceToPlayer <= attackRange)
            {
                PerformAttackLogic();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                PlayIdleAnim();
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            PlayIdleAnim();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // 플레이어를 향해 이동
    protected virtual void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ======================================================================
        // **수정: 공격 범위 직전에서 멈추도록**
        // 플레이어가 attackRange + attackStopBuffer 안에 들어오면 이동을 멈춤.
        // ======================================================================
        if (distanceToPlayer <= attackRange + attackStopBuffer)
        {
            rb.linearVelocity = Vector2.zero; // 이동 정지
            PlayIdleAnim(); // 대기 애니메이션 재생 (옵션)
            return; // 더 이상 이동하지 않음
        }
        // ======================================================================

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // Flip 로직은 이제 Update()에서 처리되므로 여기서는 제거합니다.
        // if (direction.x > 0) { Flip(false); }
        // else if (direction.x < 0) { Flip(true); }

        PlayWalkAnim();
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void PerformAttackLogic()
    {
        // 자식 클래스에서 이 메서드를 오버라이드하여 특정 공격 애니메이션 재생
        // 예: PlayAttack1Anim();
    }

    public virtual void OnAttackAnimationEnd()
    {
        isPerformingAttackAnimation = false;
        StartCoroutine(PostAttackWaitCoroutine(postAttackWaitDuration));
        ResetAttackTriggers();
    }

    protected IEnumerator PostAttackWaitCoroutine(float duration)
    {
        isWaitingAfterAttack = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        isWaitingAfterAttack = false;
    }

    public virtual void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        CurrentHp -= damage;
        Debug.Log($"{gameObject.name}이(가) {damage}만큼 피해를 입었습니다. 현재 체력: {CurrentHp}");

        if (CurrentHp <= 0 && !IsDead)
        {
            HandleDeathLogic();
            OnDead?.Invoke();
        }
        else
        {
            isPerformingHurtAnimation = true;
            PlayHurtAnim();
        }
    }

    public virtual void HandleDeathLogic()
    {
        if (IsDead) return;

        _isDead = true;
        Debug.Log($"{gameObject.name}이(가) 사망했습니다.");

        PlayDeathAnim();

        OnEnemyDiedGlobal?.Invoke(gameObject);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false;
        }

        StartCoroutine(DestroyAfterDelay(1.5f));
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        EnemyStatusBridge statusBridge = GetComponent<EnemyStatusBridge>();
        if (statusBridge != null)
        {
            statusBridge.MarkAsDead();
            Debug.Log($"{gameObject.name}: EnemyStatusBridge.MarkAsDead() 호출 완료.");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: EnemyStatusBridge 컴포넌트를 찾을 수 없습니다. MarkAsDead()를 호출할 수 없습니다.", this);
        }

        Destroy(gameObject);
    }

    protected virtual void PlayIdleAnim() { }
    protected virtual void PlayWalkAnim() { }
    protected virtual void PlayJumpAnim() { }
    protected virtual void PlayDeathAnim() { }
    protected virtual void PlayHurtAnim() { }

    public virtual void OnHurtAnimationEnd()
    {
        isPerformingHurtAnimation = false;
        Debug.Log(gameObject.name + ": Hurt 애니메이션 종료. 다시 움직임 가능.", this);
    }
    protected virtual void PlayAttack1Anim() { }
    protected virtual void PlayAttack2Anim() { }
    protected virtual void PlayAttack3Anim() { }
    protected virtual void ResetAttackTriggers() { }

    protected virtual void Flip(bool faceLeft)
    {
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        Vector3 currentScale = spriteToFlip.localScale;

        if (faceLeft) // 왼쪽을 바라보도록
        {
            spriteToFlip.localScale = new Vector3(-Mathf.Abs(currentScale.x) * -1, currentScale.y, currentScale.z); // 오른쪽을 바라볼 때 양수라면 왼쪽을 볼 때는 음수로
        }
        else // 오른쪽을 바라보도록
        {
            spriteToFlip.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z); // 오른쪽을 바라볼 때 양수로
        }
    }

    public void SetPlayerTarget(Transform newPlayerTransform)
    {
        if (newPlayerTransform != null)
        {
            playerTransform = newPlayerTransform;
            Debug.Log($"{gameObject.name}: 플레이어 타겟이 설정되었습니다: {playerTransform.name}", this);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: SetPlayerTarget 함수에 전달된 플레이어 Transform이 null입니다. 플레이어를 추적할 수 없습니다.", this);
        }
    }
}