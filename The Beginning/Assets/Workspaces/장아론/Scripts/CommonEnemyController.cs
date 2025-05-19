using System;
using UnityEngine;
using System.Collections;

public abstract class CommonEnemyController : MonoBehaviour, IDamageable
{
    [Header("Base Enemy Stats")]
    // 이 변수들은 내부적으로 사용하며, IDamageable 인터페이스 속성으로 노출됩니다.
    protected float _currentHealth; // IDamageable.CurrentHp의 백킹 필드
    public float _maxHealth = 10f;  // IDamageable.MaxHp의 백킹 필드
    protected bool _isDead = false; // IDamageable.IsDead의 백킹 필드
    protected Animator animator;

    // 피격 애니메이션 중인지 나타내는 플래그
    protected bool isPerformingHurtAnimation = false;

    // IDamageable 인터페이스 구현 (속성들)
    public float CurrentHp
    {
        get => _currentHealth;
        set => _currentHealth = value; // IDamageable.CurrentHp.set 구현
    }

    public float MaxHp
    {
        get => _maxHealth;
        set => _maxHealth = value; // IDamageable.MaxHp.set 구현
    }

    public bool IsDead => _isDead; // IDamageable.IsDead 구현 (읽기 전용이므로 기존과 동일)

    // IDamageable.OnDead (Action 타입 속성) 구현
    private Action _onDeadAction; // OnDead Action 속성의 실제 값을 저장할 필드
    public Action OnDead // IDamageable.OnDead.get 및 .set 구현
    {
        get => _onDeadAction;
        set => _onDeadAction = value;
    }

    [Header("Player Tracking")]
    protected Transform playerTransform; // 플레이어의 Transform 참조
    public float detectionRange = 5f; // 플레이어 감지 범위
    public float attackRange = 1.5f; // 공격 가능 범위
    public float moveSpeed = 2f; // 이동 속도

    [Header("Attack State")]
    protected bool isPerformingAttackAnimation = false; // 공격 애니메이션 재생 중인지 여부
    protected bool isWaitingAfterAttack = false; // 공격 후 잠시 멈춤 상태인지 여부
    public float postAttackWaitDuration = 0.5f; // 공격 후 대기 시간

    // Rigidbody2D 컴포넌트 참조 (이동 제어용)
    protected Rigidbody2D rb;

    // ============== 적 사망 이벤트 추가 (스폰 매니저용) ===================
    public static event Action<GameObject> OnEnemyDiedGlobal;
    // =====================================================================


    protected virtual void Awake()
    {
        CurrentHp = MaxHp; // 인터페이스 속성 통해 초기 체력 설정
        animator = GetComponent<Animator>();

        // Rigidbody2D 컴포넌트 참조 얻기
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError(gameObject.name + ": Rigidbody2D 컴포넌트를 찾을 수 없습니다! 이동이 제대로 동작하지 않을 수 있습니다.", this);
        }

        // "Player" 태그를 가진 GameObject를 찾아 Transform을 할당
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": 'Player' 태그를 가진 플레이어 GameObject를 찾을 수 없습니다! AI가 플레이어를 추적하지 못할 수 있습니다.", this);
        }
    }

    protected virtual void Start()
    {
        // (Awake에서 초기화 했으므로 Start에서는 추가 초기화가 필요 없으면 비워둡니다.)
    }

    protected virtual void Update()
    {
        // 죽은 상태이거나 피격 애니메이션 중이면 아무것도 하지 않음
        if (IsDead || isPerformingHurtAnimation)
        {
            rb.linearVelocity = Vector2.zero; // 이동 정지
            return;
        }

        // 공격 애니메이션 재생 중이거나 공격 후 대기 중이면 이동 및 공격 로직 스킵
        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            PlayIdleAnim(); // 이 동안에도 Idle 애니메이션을 유지하거나 특정 상태 애니메이션을 유지할 수 있습니다.
            rb.linearVelocity = Vector2.zero; // 이동 정지
            return;
        }

        // 플레이어 추적 및 공격 로직
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= attackRange)
            {
                // 플레이어가 공격 범위 안에 있으면 공격
                PerformAttackLogic();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                // 플레이어가 감지 범위 안에 있으면 추적
                MoveTowardsPlayer();
            }
            else
            {
                // 플레이어가 감지 범위 밖에 있으면 대기
                PlayIdleAnim();
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 이동 중지
            }
        }
        else
        {
            // 플레이어가 없으면 대기
            PlayIdleAnim();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 이동 중지
        }
    }

    // 플레이어를 향해 이동
    protected virtual void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        // 이동 방향에 따라 캐릭터 스프라이트 뒤집기
        if (direction.x > 0)
        {
            Flip(false); // 오른쪽 바라보기
        }
        else if (direction.x < 0)
        {
            Flip(true); // 왼쪽 바라보기
        }

        // 이동 애니메이션 재생
        PlayWalkAnim();

        // Rigidbody를 이용하여 이동 (물리적 이동)
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    // 공격 로직 (자식 클래스에서 오버라이드)
    protected virtual void PerformAttackLogic()
    {
        // 자식 클래스에서 이 메서드를 오버라이드하여 특정 공격 애니메이션 재생
        // 예: PlayAttack1Anim();
    }

    // 공격 애니메이션 종료 시 호출될 콜백 (애니메이션 이벤트에 연결)
    public virtual void OnAttackAnimationEnd()
    {
        isPerformingAttackAnimation = false;
        // 공격 후 잠시 멈추기 (코루틴 시작)
        StartCoroutine(PostAttackWaitCoroutine(postAttackWaitDuration));
        ResetAttackTriggers(); // 공격 트리거 리셋
    }

    // 공격 후 대기 코루틴
    protected IEnumerator PostAttackWaitCoroutine(float duration)
    {
        isWaitingAfterAttack = true;
        rb.linearVelocity = Vector2.zero; // 공격 후 이동 잠시 중지
        yield return new WaitForSeconds(duration);
        isWaitingAfterAttack = false;
    }

    // IDamageable 인터페이스 구현 (피격 처리)
    public virtual void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        CurrentHp -= damage;
        Debug.Log($"{gameObject.name}이(가) {damage}만큼 피해를 입었습니다. 현재 체력: {CurrentHp}");

        if (CurrentHp <= 0 && !IsDead)
        {
            HandleDeathLogic(); // 실제 사망 처리 로직을 담은 메서드 호출
            OnDead?.Invoke(); // IDamageable 인터페이스의 OnDead Action 속성 Invoke
        }
        else
        {
            // 피격 애니메이션이 있다면 여기서 재생
            PlayHurtAnim();
        }
    }

    // 실제 사망 처리 로직
    public virtual void HandleDeathLogic()
    {
        if (IsDead) return;

        _isDead = true; // 내부 _isDead 변수 업데이트
        Debug.Log($"{gameObject.name}이(가) 사망했습니다.");

        PlayDeathAnim(); // 사망 애니메이션 재생

        // ============== 적 사망 이벤트 발생 (스폰 매니저용) ===================
        OnEnemyDiedGlobal?.Invoke(gameObject); // 이벤트를 구독하는 모든 객체에게 사망한 GameObject를 전달
        // =====================================================================

        // 물리적 상호작용 비활성화
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true; // 물리 영향 받지 않음
        }
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = false; // 콜라이더 비활성화
        }

        // 일정 시간 후 GameObject 비활성화 또는 파괴
        StartCoroutine(DestroyAfterDelay(1.5f)); // 사망 애니메이션 재생 후 3초 뒤 파괴
    }

    // GameObject를 지연 파괴하는 코루틴
    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    // ===== 애니메이션 관련 가상 함수들 (자식 클래스에서 오버라이드) =====
    protected virtual void PlayIdleAnim() { }
    protected virtual void PlayWalkAnim() { }
    protected virtual void PlayJumpAnim() { }
    protected virtual void PlayDeathAnim() { }
    protected virtual void PlayHurtAnim() { }

    // 피격 애니메이션 종료 시 호출될 애니메이션 이벤트 메서드
    public virtual void OnHurtAnimationEnd()
    {
        isPerformingHurtAnimation = false;
    }
    protected virtual void PlayAttack1Anim() { }
    protected virtual void PlayAttack2Anim() { }
    protected virtual void PlayAttack3Anim() { }
    protected virtual void ResetAttackTriggers() { }

    // 캐릭터 스프라이트 또는 GameObject 방향 뒤집기
    protected virtual void Flip(bool faceLeft)
    {
        // 팁: 대부분의 2D 게임에서 스프라이트 렌더러는 메인 게임 오브젝트의 자식 오브젝트에 있습니다.
        // 예를 들어, 자식 오브젝트 이름이 "Sprite"라면 아래와 같이 자식의 Transform을 사용해야 합니다.
        // 만약 스프라이트 렌더러가 이 스크립트가 붙어있는 메인 오브젝트에 직접 있다면,
        // 아래 'transform.Find("Sprite");' 줄을 주석 처리하고 'spriteToFlip = transform;' 라인을 활성화하세요.
        Transform spriteToFlip = transform.Find("Sprite");

        // "Sprite"라는 자식 오브젝트를 찾지 못했다면, 현재 오브젝트의 Transform을 사용합니다.
        // 이 경우, 스프라이트 렌더러가 메인 오브젝트에 직접 붙어있다고 가정합니다.
        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        Vector3 currentScale = spriteToFlip.localScale;

        if (faceLeft) // 왼쪽을 바라보도록
        {
            // 현재 x 스케일이 양수이면 음수로 뒤집기 (왼쪽 바라봄)
            // Mathf.Abs를 사용하여 이미 음수여도 정확하게 양수값으로 만들어서 뒤집습니다.
            spriteToFlip.localScale = new Vector3(+Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else // 오른쪽을 바라보도록 (faceRight)
        {
            // 현재 x 스케일이 음수이면 양수로 뒤집기 (오른쪽 바라봄)
            spriteToFlip.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }
}