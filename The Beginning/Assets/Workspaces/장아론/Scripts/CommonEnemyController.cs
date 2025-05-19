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
    public float attackStopBuffer = 0.1f;

    [Header("Patrol Settings")]
    protected Vector3 originalPosition;
    public bool enablePatrol = false;
    public Vector3 patrolLeftLimit;
    public Vector3 patrolRightLimit;
    private int _currentPatrolDirection = 1;

    [Header("Attack State")]
    protected bool isPerformingAttackAnimation = false;
    protected bool isWaitingAfterAttack = false;
    public float postAttackWaitDuration = 0.5f;

    // --- 콤보 및 공격 관련 변수들 ---
    [Header("Attack Combo Settings")]
    protected float nextAttackTime = 0f; // 다음 공격이 가능한 시간
    public ComboState currentComboState = ComboState.None; // 콤보 상태 추적
    protected float lastAttackAttemptTime = 0f; // 마지막 공격 시도 시간 기록 (콤보 리셋용)
    public float comboChainDelay = 0.3f; // 일반적인 콤보 공격 사이의 지연 시간 (짧게 설정)
    public float comboResetTime = 2.5f; // 콤보가 완전히 리셋되는 시간 (인스펙터에서 조절, 이전보다 길게 설정)
    public float attack2Cooldown = 1.0f; // Attack2 (강타)의 쿨다운 (콤보 마무리시 사용)
    public float attack3Cooldown = 2.0f; // Attack3 (회오리 돌진)의 쿨다운

    [Header("Custom Combo Delays")] // 새로운 패턴을 위한 지연 시간 변수들
    public float heavyPunchFirstDelay = 0.6f;  // "퉁퉁 퉁~" 첫 번째 '퉁' 후 다음 '퉁'까지의 지연
    public float heavyPunchSecondDelay = 0.8f; // "퉁퉁 퉁~" 두 번째 '퉁' 후 마지막 '퉁'까지의 지연

    public float offBeatFirstDelay = 0.7f;     // 엇박 콤보 첫 번째 공격 후 다음 공격까지의 지연 (길게)
    public float offBeatSecondDelay = 0.3f;    // 엇박 콤보 두 번째 공격 후 다음 공격까지의 지연 (짧게)
    // --- (여기까지) ---

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

        originalPosition = transform.position;
    }

    protected virtual void Start()
    {
        // SetPlayerTarget은 일반적으로 PlayerDetector 스크립트 등에서 호출됩니다.
    }

    protected virtual void Update()
    {
        // 1. 사망, 피격 애니메이션 중, 공격 애니메이션/대기 중에는 다른 행동을 하지 않음
        if (IsDead || isPerformingHurtAnimation)
        {
            rb.linearVelocity = Vector2.zero; // 움직임 정지
            return;
        }

        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            PlayIdleAnim(); // 공격 중에는 대기 애니메이션 재생
            rb.linearVelocity = Vector2.zero; // 움직임 정지
            return;
        }

        // 2. 플레이어의 존재 여부에 따른 행동 결정
        if (playerTransform != null) // 플레이어가 씬에 존재하고 타겟이 설정되어 있을 경우
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // 플레이어가 감지 범위 안에 있을 때 항상 플레이어 방향을 바라보도록
            if (distanceToPlayer <= detectionRange)
            {
                float directionX = playerTransform.position.x - transform.position.x;
                if (directionX > 0) { Flip(false); } // 오른쪽 바라보기
                else if (directionX < 0) { Flip(true); } // 왼쪽 바라보기

                // 플레이어가 감지 범위 내에 있으면, B_Girl의 공격 패턴을 시도합니다.
                // PerformAttackLogic() 안에서 거리와 콤보 상태에 따라 적절한 공격(회오리 돌진 or 근접 콤보)을 결정합니다.
                PerformAttackLogic();

                // 공격 범위 안에 있거나 공격 범위 바로 바깥 (정지 버퍼)에 있으면 멈춤
                if (distanceToPlayer <= attackRange + attackStopBuffer)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                else // 플레이어가 공격 범위 밖, 감지 범위 내에 있으면 추격
                {
                    MoveTowardsPlayer(); // 플레이어를 향해 이동
                }
            }
            else // 플레이어가 감지 범위 밖에 있을 경우: 순찰 또는 원점 복귀
            {
                if (enablePatrol)
                {
                    MovePatrol();
                }
                else
                {
                    float distanceToOrigin = Vector2.Distance(transform.position, originalPosition);
                    if (distanceToOrigin > 0.1f)
                    {
                        MoveTowardsOrigin();
                    }
                    else
                    {
                        PlayIdleAnim();
                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    }
                }
            }
        }
        else // 플레이어 Transform이 설정되지 않았거나 null인 경우
        {
            if (enablePatrol)
            {
                MovePatrol();
            }
            else
            {
                float distanceToOrigin = Vector2.Distance(transform.position, originalPosition);
                if (distanceToOrigin > 0.1f)
                {
                    MoveTowardsOrigin();
                }
                else
                {
                    PlayIdleAnim();
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
        }
    }

    protected virtual void MoveTowardsPlayer()
    {
        if (playerTransform == null) return;

        Vector2 direction = (playerTransform.position - transform.position).normalized;

        PlayWalkAnim();
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void MoveTowardsOrigin()
    {
        float distanceToOrigin = Vector2.Distance(transform.position, originalPosition);

        if (distanceToOrigin <= 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            PlayIdleAnim();
            transform.position = originalPosition;
            return;
        }

        Vector2 directionToOrigin = (originalPosition - transform.position).normalized;

        if (directionToOrigin.x > 0)
        {
            Flip(false);
        }
        else if (directionToOrigin.x < 0)
        {
            Flip(true);
        }

        PlayWalkAnim();
        rb.linearVelocity = new Vector2(directionToOrigin.x * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void MovePatrol()
    {
        Vector3 targetPoint;
        if (_currentPatrolDirection == 1)
        {
            targetPoint = patrolRightLimit;
            Flip(false);
        }
        else
        {
            targetPoint = patrolLeftLimit;
            Flip(true);
        }

        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            _currentPatrolDirection *= -1;
            PlayIdleAnim();
            rb.linearVelocity = Vector2.zero;
            return;
        }

        PlayWalkAnim();
        rb.linearVelocity = new Vector2(_currentPatrolDirection * moveSpeed, rb.linearVelocity.y);
    }

    protected virtual void PerformAttackLogic()
    {
        // 1. 전역 쿨다운 및 콤보 리셋 시간 체크
        if (Time.time < nextAttackTime)
        {
            // Debug.Log($"[B_Girl] 공격 쿨다운 중. 남은 시간: {nextAttackTime - Time.time:F2}");
            return;
        }

        // 마지막 공격 시도 후 충분한 시간이 지났다면 콤보 상태를 초기화합니다.
        if (Time.time - lastAttackAttemptTime > comboResetTime)
        {
            Debug.Log($"[B_Girl] 콤보 리셋됨! (시간 초과: {Time.time - lastAttackAttemptTime:F2}s, 리셋 기준: {comboResetTime:F2}s)");
            currentComboState = ComboState.None; // 콤보 상태를 None으로 초기화
        }

        // 2. 새로운 패턴을 시작할지, 기존 콤보를 이어나갈지 결정
        if (currentComboState == ComboState.None) // 콤보가 진행 중이 아닐 때, 새로운 패턴 시작 결정
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // A) 패턴 1: 회오리 돌진 (Attack3) - 중거리 우선순위
            if (distanceToPlayer > attackRange * 1.5f && distanceToPlayer <= detectionRange * 0.8f) // 중거리 범위 설정
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% 확률로 회오리 돌진 시도
                {
                    isPerformingAttackAnimation = true;
                    PlayAttack3Anim();
                    nextAttackTime = Time.time + attack3Cooldown;
                    lastAttackAttemptTime = Time.time;
                    Debug.Log($"[B_Girl] (새로운 패턴) 회오리 돌진 (Attack3) 발동!");
                    return; // 회오리 돌진 발동 시 다른 공격은 시도하지 않음
                }
            }

            // B) 근접 패턴 선택 (회오리 돌진이 발동하지 않았을 경우)
            float patternRoll = UnityEngine.Random.Range(0f, 1f);

            if (patternRoll < 0.4f) // 40% 확률로 '빠른 잽 연타' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack1Anim();
                currentComboState = ComboState.QuickJab_Initial; // 새로운 잽 콤보 시작 상태
                nextAttackTime = Time.time + comboChainDelay;
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 빠른 잽 연타 (Attack1)");
            }
            else if (patternRoll < 0.75f) // 35% 확률로 '묵직한 펀치 연타' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack2Anim(); // 첫 번째 '퉁'
                currentComboState = ComboState.HeavyAttack_Initial; // 묵직한 펀치 콤보 시작 상태
                nextAttackTime = Time.time + heavyPunchFirstDelay; // 첫 번째 퉁 후 지연
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 묵직한 펀치 연타 (Attack2) - 퉁!");
            }
            else // 25% 확률로 '엇박 콤보' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack1Anim(); // 엇박 콤보의 시작은 잽
                currentComboState = ComboState.OffBeat_Initial; // 엇박 콤보 시작 상태
                nextAttackTime = Time.time + offBeatFirstDelay; // 첫 번째 엇박 공격 후 지연
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 엇박 콤보 (Attack1)");
            }
        }
        else // 현재 콤보 진행 중 (ComboState.None이 아님) - 콤보 이어나가기
        {
            isPerformingAttackAnimation = true; // 콤보 중이니 애니메이션 플래그 유지
            switch (currentComboState)
            {
                case ComboState.QuickJab_Initial: // 빠른 잽 연타: 첫 번째 잽 후
                    if (UnityEngine.Random.Range(0f, 1f) < 0.6f) // 60% 확률로 두 번째 잽 ('원원' 패턴으로 이어짐)
                    {
                        PlayAttack1Anim();
                        currentComboState = ComboState.QuickJab_Second; // 두 번째 잽 상태
                        nextAttackTime = Time.time + comboChainDelay;
                        Debug.Log($"[B_Girl] 콤보 진행: 빠른 잽 연타 (두 번째 Attack1)");
                    }
                    else // 40% 확률로 강타 ('원투' 패턴으로 마무리)
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.None; // 콤보 끝
                        nextAttackTime = Time.time + attack2Cooldown; // 콤보 마무리 후 쿨다운
                        Debug.Log($"[B_Girl] 콤보 마무리: 빠른 잽 연타 (Attack2 마무리)");
                    }
                    break;

                case ComboState.QuickJab_Second: // 빠른 잽 연타: 두 번째 잽 후 ('원원투' 패턴으로 마무리)
                    PlayAttack2Anim();
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + attack2Cooldown; // 콤보 마무리 후 쿨다운
                    Debug.Log($"[B_Girl] 콤보 완료: 빠른 잽 연타 (세 번째 Attack2 마무리)");
                    break;

                case ComboState.HeavyAttack_Initial: // 묵직한 펀치 연타: 첫 번째 '퉁' 후
                    if (UnityEngine.Random.Range(0f, 1f) < 0.7f) // 70% 확률로 두 번째 '퉁'
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.HeavyAttack_Second; // 두 번째 '퉁' 상태
                        nextAttackTime = Time.time + heavyPunchSecondDelay; // 두 번째 '퉁' 후 지연
                        Debug.Log($"[B_Girl] 콤보 진행: 묵직한 펀치 연타 (두 번째 Attack2) - 퉁!");
                    }
                    else
                    {
                        currentComboState = ComboState.None; // 콤보 끝 (중간에 종료)
                        nextAttackTime = Time.time + attack2Cooldown;
                        Debug.Log($"[B_Girl] 콤보 마무리: 묵직한 펀치 연타 (두 번째 퉁 건너뛰고 마무리)");
                    }
                    break;

                case ComboState.HeavyAttack_Second: // 묵직한 펀치 연타: 두 번째 '퉁' 후
                    PlayAttack2Anim(); // 마지막 세 번째 '퉁'
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + attack2Cooldown;
                    Debug.Log($"[B_Girl] 콤보 완료: 묵직한 펀치 연타 (세 번째 Attack2) - 퉁~!");
                    break;

                case ComboState.OffBeat_Initial: // 엇박 콤보: 첫 번째 잽 후
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% 확률로 잽으로 이어짐 (느린 박자)
                    {
                        PlayAttack1Anim();
                        currentComboState = ComboState.OffBeat_Second; // 엇박 콤보 두 번째 상태
                        nextAttackTime = Time.time + offBeatSecondDelay; // 두 번째 엇박 공격 후 지연 (여기서 박자 조절)
                        Debug.Log($"[B_Girl] 콤보 진행: 엇박 콤보 (Attack1으로 이어짐 - 느린 박자)");
                    }
                    else // 50% 확률로 강타로 마무리 (빠른 박자)
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.None; // 콤보 끝
                        nextAttackTime = Time.time + attack2Cooldown;
                        Debug.Log($"[B_Girl] 콤보 마무리: 엇박 콤보 (Attack2 마무리 - 빠른 박자)");
                    }
                    break;

                case ComboState.OffBeat_Second: // 엇박 콤보: 두 번째 공격 후
                    PlayAttack2Anim(); // 최종 강타로 마무리
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + attack2Cooldown;
                    Debug.Log($"[B_Girl] 콤보 완료: 엇박 콤보 (최종 Attack2 마무리)");
                    break;
            }
        }
        lastAttackAttemptTime = Time.time; // 마지막 공격 시도 시간 업데이트
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
            Debug.Log($"[{gameObject.name}] Hurt 애니메이션 시작! isPerformingHurtAnimation = true");
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
    protected virtual void PlayAttack1Anim() { }
    protected virtual void PlayAttack2Anim() { }
    protected virtual void PlayAttack3Anim() { }

    public virtual void OnHurtAnimationEnd()
    {
        isPerformingHurtAnimation = false;
        Debug.Log($"[{gameObject.name}] Hurt 애니메이션 종료! isPerformingHurtAnimation = false. 다시 움직임 가능.");
    }
    protected virtual void ResetAttackTriggers() { }

    protected virtual void Flip(bool faceLeft)
    {
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        float desiredSign = faceLeft ? -1f : 1f;
        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);

        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);
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

// ComboState enum은 CommonEnemyController 클래스 내부에 정의합니다.
public enum ComboState
{
    None,                     // 콤보가 진행 중이 아님, 새로운 공격을 시작할 준비 완료

    // 빠른 잽 연타 콤보 상태
    QuickJab_Initial,        // 첫 번째 Attack1 (잽)을 방금 수행함
    QuickJab_Second,         // 두 번째 Attack1 (잽)을 방금 수행함

    // 묵직한 펀치 연타 콤보 상태 ("퉁퉁 퉁~")
    HeavyAttack_Initial,      // 첫 번째 Attack2 (퉁)을 방금 수행함
    HeavyAttack_Second,       // 두 번째 Attack2 (퉁)을 방금 수행함

    // 엇박 콤보 상태
    OffBeat_Initial,          // 엇박 콤보 첫 번째 공격 (Attack1)을 방금 수행함
    OffBeat_Second            // 엇박 콤보 두 번째 공격 (Attack1 또는 Attack2)을 방금 수행함
}