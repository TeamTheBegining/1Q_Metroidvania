using UnityEngine;
using System; // Action 사용 시 필요
using System.Collections; // 코루틴 사용 시 필요

// IDamageable 인터페이스 구현 명시
public class CommonEnemyController : MonoBehaviour, IDamageable
{
    // ===== 기본 속성 =====
    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 100f; // float
    [SerializeField] protected float currentHealth; // float
    public float moveSpeed = 3f; // 걷기/물러나기 속도
    // public float attackDamage = 2f; // 기본 공격 데미지 (파생 클래스나 애니메이션 이벤트에서 설정)

    // ===== AI 파라미터 =====
    [Header("AI Parameters")]
    public float detectionRange = 10f; // 플레이어 감지 거리
    public float attackRange = 2f; // 공격 시작 및 유지 거리
    public float maintainBuffer = 0.5f; // attackRange보다 얼마나 더 가까워져야 물러날지 결정

    // 공격 시작 전/후 대기 시간 파라미터
    public float preAttackPauseDuration = 0.5f; // 공격 애니메이션 시작 전 대기 시간
    public float postAttackPauseDuration = 1.0f; // 공격 애니메이션 종료 후 대기 시간

    public string playerObjectName = "Player"; // 플레이어 오브젝트 이름

    // ===== 애니메이터 및 애니메이션 상태 =====
    protected Animator animator;
    protected GameObject player;
    // protected bool isDead (수정 유지)
    protected bool isDead = false; // 사망 상태

    // isPerformingAttackAnimation는 공격 애니메이션 재생 중 + 이동 및 AI 판단 스킵 플래그
    protected bool isPerformingAttackAnimation = false;
    //  피격 애니메이션 재생 중 플래그 추가 
    protected bool isPerformingHurtAnimation = false;


    // 공격 시작 전 대기 상태를 위한 플래그 또는 타이머
    private bool isWaitingForAttack = false; // 공격 애니메이션 시작 전 대기 중인지 여부
    private float attackWaitTimer = 0f; // 공격 시작 전 대기 타이머

    // 공격 후 대기 상태를 위한 플래그 또는 타이머
    private bool isWaitingAfterAttack = false; // 공격 애니메이션 종료 후 대기 중인지 여부


    // 적의 현재 AI 상태
    protected enum EnemyState { Idle, Chase, Attack, Dead } // TODO: 필요시 Stunned 상태 추가
    protected EnemyState currentState = EnemyState.Idle; // 초기 상태

    // 적 캐릭터의 초기 방향 (좌우 반전에 사용)
    public float initialFacingDirection = 1f; // 오른쪽을 보면 1, 왼쪽을 보면 -1

    // IDamageable 인터페이스 멤버 명시적 구현 (수정 유지)
    // 인터페이스는 public set을 요구하지만, 클래스 내부에서는 protected set을 유지하기 위함
    float IDamageable.CurrentHp { get => currentHealth; set => currentHealth = value; }
    float IDamageable.MaxHp { get => maxHealth; set => maxHealth = value; }
    bool IDamageable.IsDead { get => isDead; } // protected isDead 필드 사용
    Action IDamageable.OnDead { get => OnDead; set => OnDead = value; } // 클래스의 OnDead 멤버를 사용


    // 사망 시 호출될 이벤트 액션 (클래스 멤버, 수정 유지)
    public Action OnDead { get; set; }


    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component not found on " + gameObject.name);

        // currentHealth는 Start에서 maxHealth로 초기화
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // "Player" 태그로 플레이어 찾음
        if (player == null)
        {
            Debug.LogError("Player GameObject with Tag 'Player' not found! Ensure Player has the 'Player' tag.");
        }

        currentHealth = maxHealth; // 체력 초기화

        SetState(EnemyState.Idle); // 초기 상태 설정

        // 초기 대기 상태 변수 초기화
        isWaitingForAttack = false;
        attackWaitTimer = 0f;
        isWaitingAfterAttack = false;
        isDead = false; // protected isDead 초기화
        isPerformingHurtAnimation = false; //  피격 애니메이션 플래그 초기화 
        OnDead = null; // OnDead 이벤트 초기화 (중요!)
    }

    protected virtual void Update()
    {
        // 죽었거나 피격 애니메이션 중이면 Update 전체 로직 스킵
        if (isDead) return; // 죽었으면 아무것도 안 함

        //  isPerformingHurtAnimation 중일 때 Update 로직 스킵 
        if (isPerformingHurtAnimation)
        {
            Debug.Log(gameObject.name + ": Update skipped due to isPerformingHurtAnimation being true."); //  Update 스킵 로그 
            return; // 피격 중일 때는 Update의 나머지 AI/이동 로직 건너뛰기
        }

        //  isPerformingHurtAnimation가 false일 때만 이 아래 로직 실행 
        Debug.Log(gameObject.name + ": Update loop is running. isPerformingHurtAnimation=" + isPerformingHurtAnimation + ", CurrentState=" + currentState); //  Update 재개 로그 

        if (player == null)
        {
            if (currentState != EnemyState.Idle) SetState(EnemyState.Idle);
            PlayIdleAnim();
            return;
        }

        UpdateFacingDirection(); // 플레이어 방향 바라보기는 항상 수행 (죽거나 피격 중이 아니라면)

        //  AI 상태 머신 실행 
        switch (currentState)
        {
            case EnemyState.Idle:
                //  상세 디버그 로그 추가
                //Debug.Log(gameObject.name + ": State=Idle. Distance=" + Vector3.Distance(transform.position, player.transform.position).ToString("F2"));
                // 플레이어가 감지 범위에 들어오면 추적
                if (Vector3.Distance(transform.position, player.transform.position) <= detectionRange)
                {
                    SetState(EnemyState.Chase);
                }
                // PlayIdleAnim(); // 애니메이션 재생은 SetState에서 관리
                break;

            case EnemyState.Chase:
                //  Chase 상태 진입 로그 (추적 중 Update 매 프레임 실행 확인) 
                Debug.Log(gameObject.name + ": In Chase State Update. Distance=" + Vector3.Distance(transform.position, player.transform.position).ToString("F2"));
                float distanceToPlayerChase = Vector3.Distance(transform.position, player.transform.position);
                // 플레이어가 공격 범위에 들어오면 공격 상태로 전환
                if (distanceToPlayerChase <= attackRange)
                {
                    SetState(EnemyState.Attack);
                }
                // 플레이어가 감지 범위를 벗어나면 Idle
                else if (distanceToPlayerChase > detectionRange)
                {
                    SetState(EnemyState.Idle);
                }
                // 감지 범위 내, 공격 범위 밖 -> 계속 추적
                //  이동은 공격 애니메이션 중이거나 대기 중이 아닐 때만 허용 
                //  이동 허용 조건 상세 로그 
                bool canChaseMove = !(isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation); // isPerformingHurtAnimation는 Update 진입 시 이미 체크
                Debug.Log(gameObject.name + ": Checking Chase Move Condition. Result: " + canChaseMove +
                          " (isPerformingAttackAnim=" + isPerformingAttackAnimation +
                          ", isWaitingForAttack=" + isWaitingForAttack +
                          ", isWaitingAfterAttack=" + isWaitingAfterAttack + ")"); // isPerformingHurtAnimation는 여기서 이미 false

                if (canChaseMove)
                {
                    Debug.Log(gameObject.name + ": Chase Move Condition Met. Calling MoveTowardsPlayer and PlayWalkAnim."); //  이동 메서드 호출 직전 로그 
                    MoveTowardsPlayer(); // <-- 이동 허용 시에만 이동 메서드 호출
                    PlayWalkAnim(); // 이동 시 걷기 애니메이션 재생
                }
                //  이동이 허용되지 않는 Chase 상태 (대기/애니메이션 중) 
                else
                {
                    Debug.Log(gameObject.name + ": Chase Move Condition NOT Met. Remaining Idle/Waiting."); //  이동 스킵 로그 
                    // 이 경우 Idle 애니메이션을 재생하는 것이 자연스러울 수 있습니다.
                    PlayIdleAnim();
                }
                // 애니메이션 재생은 SetState 또는 위 이동/대기 로직에서 관리
                // PlayWalkAnim(); // 무조건 Walk 애니메이션 재생 대신 조건부 재생
                break;

            case EnemyState.Attack:
                float distanceToPlayerAttack = Vector3.Distance(transform.position, player.transform.position);
                // 플레이어가 공격 범위를 벗어나면 추적 상태로 전환
                if (distanceToPlayerAttack > attackRange)
                {
                    SetState(EnemyState.Chase);
                }
                else // 플레이어 공격 범위 내
                {
                    // 간격 유지 (너무 가까우면 물러나기)
                    //  물러나기도 공격 애니메이션 중이거나 대기 중이 아닐 때만 허용 
                    //  물러나기 허용 조건 상세 로그 
                    bool canAttackMove = !(isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation); // isPerformingHurtAnimation는 Update 진입 시 이미 체크
                    Debug.Log(gameObject.name + ": Checking Attack Move Condition. Result: " + canAttackMove +
                              " (isPerformingAttackAnim=" + isPerformingAttackAnimation +
                              ", isWaitingForAttack=" + isWaitingForAttack +
                              ", isWaitingAfterAttack=" + isWaitingAfterAttack + ")"); // isPerformingHurtAnimation는 여기서 이미 false


                    if (canAttackMove && distanceToPlayerAttack < attackRange - maintainBuffer)
                    {
                        //  상세 디버그 로그 추가
                        Debug.Log(gameObject.name + ": Too close (" + distanceToPlayerAttack.ToString("F2") + "), retreating. Resetting attack wait.");
                        MoveAwayFromPlayer(); // <-- 물러나기 허용 시에만 이동
                        PlayWalkAnim(); // 물러날 때는 걷기 애니메이션
                        isWaitingForAttack = false; // 물러나는 중 공격 준비 상태 해제
                        attackWaitTimer = preAttackPauseDuration; // 타이머 리셋 (물러난 후 다시 공격 준비 시작)
                    }
                    else // 적절한 공격 거리 또는 이동이 허용되지 않는 상태 (대기/애니메이션 중)
                    {
                        //  공격 준비 시작 조건: 공격 애니메이션 중이거나 공격 후 대기 중이 아닐 때 
                        //  공격 시작 조건 상세 로그 
                        bool canStartAttackPrep = !(isPerformingAttackAnimation || isWaitingAfterAttack); // isPerformingHurtAnimation는 Update 진입 시 이미 체크
                        Debug.Log(gameObject.name + ": Checking Attack Start Prep Condition. Result: " + canStartAttackPrep +
                                  " (isPerformingAttackAnim=" + isPerformingAttackAnimation +
                                  ", isWaitingAfterAttack=" + isWaitingAfterAttack + ")");

                        if (canStartAttackPrep)
                        {
                            //  아직 공격 준비 대기 상태가 아니라면 시작 
                            if (!isWaitingForAttack)
                            {
                                Debug.Log(gameObject.name + ": Within attack range (" + distanceToPlayerAttack.ToString("F2") + "). Condition met. Starting attack wait (" + preAttackPauseDuration.ToString("F2") + "s).");
                                isWaitingForAttack = true; // 공격 준비 상태 진입
                                attackWaitTimer = preAttackPauseDuration; // 공격 준비 타이머 시작
                                PlayIdleAnim(); // 공격 준비 중에는 Idle 모션
                            }
                            // else: 이미 isWaitingForAttack 상태이면 타이머 감소 로직으로 이동 (아래 if 블록)
                        }
                        // else: isPerformingAttackAnimation 또는 isWaitingAfterAttack 상태이면 공격 준비 시작 안 함.

                        //  타이머 감소 및 PerformAttackLogic 호출 
                        // isWaitingForAttack 상태일 때만 타이머 감소 및 PerformAttackLogic 호출
                        if (isWaitingForAttack)
                        {
                            //Debug.Log(gameObject.name + ": Currently in Attack Wait state. Timer: " + attackWaitTimer.ToString("F2"));
                            attackWaitTimer -= Time.deltaTime;
                            if (attackWaitTimer <= 0)
                            {
                                Debug.Log(gameObject.name + ": Attack wait finished. Timer <= 0. Calling PerformAttackLogic.");
                                isWaitingForAttack = false; // 공격 준비 상태 해제
                                PerformAttackLogic(); // <-- 파생 클래스의 오버라이드 메서드 호출
                            }
                        }
                        // Animation: PlayIdleAnim while waiting/paused, PlayAttackAnim while isPerformingAttackAnimation
                        // 애니메이션은 주로 SetState와 애니메이션 이벤트에서 관리
                    }
                }
                break;

            case EnemyState.Dead:
                // 죽은 상태에서는 AI 로직 없음
                break;
        }
    }

    // AI 상태를 설정하고 애니메이션 및 변수 업데이트
    protected virtual void SetState(EnemyState newState)
    {
        Debug.Log(">>> " + gameObject.name + " SetState: " + currentState.ToString() + " -> " + newState.ToString()); //  상태 전환 로그 (유지) 
        if (currentState == newState) return;
        if (isDead && newState != EnemyState.Dead) return; // protected isDead 사용

        currentState = newState;

        // 상태 전환 시 초기화 및 애니메이션 설정
        switch (currentState)
        {
            case EnemyState.Idle:
                PlayIdleAnim(); // Idle 애니메이션
                isPerformingAttackAnimation = false;
                // isPerformingHurtAnimation = false; // Idle 진입 시 피격 상태 해제 (애니메이션 이벤트에서도 함)
                isWaitingForAttack = false; // 공격 준비 상태 해제
                attackWaitTimer = 0f;
                isWaitingAfterAttack = false; // 공격 후 대기 상태 해제
                ResetAttackTriggers(); // 공격 트리거 리셋
                break;

            case EnemyState.Chase:
                PlayWalkAnim(); // 걷기 애니메이션
                isPerformingAttackAnimation = false;
                // isPerformingHurtAnimation = false; // Chase 진입 시 피격 상태 해제
                isWaitingForAttack = false; // 공격 준비 상태 해제
                attackWaitTimer = 0f;
                isWaitingAfterAttack = false; // 공격 후 대기 상태 해제
                ResetAttackTriggers(); // 공격 트리거 리셋
                break;

            case EnemyState.Attack:
                PlayIdleAnim(); // 공격 준비 중에는 Idle 모션
                isWaitingAfterAttack = false; // 공격 후 대기 상태 해제
                ResetAttackTriggers(); // 공격 트리거 리셋
                isWaitingForAttack = true; //  공격 준비 상태 진입 
                attackWaitTimer = preAttackPauseDuration; // 공격 준비 타이머 시작
                isPerformingAttackAnimation = false; // 공격 애니메이션 시작 전이므로 false
                // isPerformingHurtAnimation = false; // Attack 진입 시 피격 상태 해제
                //  상세 디버그 로그 추가
                Debug.Log(gameObject.name + ": Entered Attack state. Initializing attack wait timer (" + preAttackPauseDuration.ToString("F2") + "s).");
                break;

            case EnemyState.Dead:
                isDead = true; // protected isDead 설정
                isPerformingAttackAnimation = false;
                isPerformingHurtAnimation = false; //  사망 시 피격 상태 해제 
                isWaitingForAttack = false;
                attackWaitTimer = 0f;
                isWaitingAfterAttack = false;
                ResetAttackTriggers(); // 공격 트리거 리셋
                PlayDeathAnim(); // 사망 애니메이션

                // OnDead 이벤트 호출
                OnDead?.Invoke(); // 구독자가 있다면 호출 (null 체크)

                // 사망 후 처리
                Collider2D mainCollider = GetComponent<Collider2D>();
                if (mainCollider != null) mainCollider.enabled = false;
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.isKinematic = true;
                }
                // GameManager에 사망 알림 (필요 없는 경우 주석 처리 또는 삭제)
                /* if (GameManager.Instance != null) { ... } */
                Destroy(gameObject, 1f);
                break;
        }
    }

    //  이동 관련 함수들: isPerformingHurtAnimation 중일 때도 이동 막도록 조건 추가 
    protected virtual void MoveTowardsPlayer()
    {
        //  이동은 공격 애니메이션/대기 중이거나 피격 중, 죽었을 때 스킵 
        if (player == null || currentState != EnemyState.Chase || isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation || isDead)
        {
            Debug.Log(gameObject.name + ": MoveTowardsPlayer skipped. Reason: State=" + currentState + ", Paused/Hurt/Dead=" + (isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation || isDead)); //  이동 스킵 상세 로그 
            return;
        }
        Debug.Log(gameObject.name + ": MoveTowardsPlayer executing."); //  이동 실행 로그 
        Vector3 directionToPlayer = (player.transform.position - transform.position);
        directionToPlayer.z = 0;
        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    protected virtual void MoveAwayFromPlayer()
    {
        //  이동은 공격 애니메이션/대기 중이거나 피격 중, 죽었을 때 스킵 
        if (player == null || currentState != EnemyState.Attack || isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation || isDead)
        {
            Debug.Log(gameObject.name + ": MoveAwayFromPlayer skipped. Reason: State=" + currentState + ", Paused/Hurt/Dead=" + (isPerformingAttackAnimation || isWaitingForAttack || isWaitingAfterAttack || isPerformingHurtAnimation || isDead)); //  이동 스킵 상세 로그 
            return;
        }
        Debug.Log(gameObject.name + ": MoveAwayFromPlayer executing."); //  이동 실행 로그 
        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position);
        directionAwayFromPlayer.z = 0;

        if (directionAwayFromPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionAwayFromPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    protected virtual void PerformAttackLogic()
    {
        // Overridden in derived class
    }

    protected virtual void UpdateFacingDirection()
    {
        //  피격 애니메이션 중에는 방향 전환도 막을 수 있음 (선택 사항) 
        if (player == null || isPerformingHurtAnimation || isDead)
        {
            // Debug.Log(gameObject.name + ": UpdateFacingDirection skipped due to Hurt/Dead state."); // 디버그 추가
            return;
        }
        // Debug.Log(gameObject.name + ": UpdateFacingDirection executing."); // 디버그 추가

        float directionX = player.transform.position.x - transform.position.x;
        Vector3 currentScale = transform.localScale;
        if (directionX > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * Mathf.Sign(initialFacingDirection) * -1f, currentScale.y, currentScale.z);
        }
        else if (directionX < -0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * Mathf.Sign(initialFacingDirection) * +1f, currentScale.y, currentScale.z);
        }
    }

    // ... (나머지 애니메이션 관련, TakeDamage 코드) ...
    // PlayIdleAnim, PlayWalkAnim, PlayDeathAnim, PlayAttack1Anim, PlayAttack2Anim, ResetAttackTriggers
    // OnAttackAnimationEnd, PostAttackPauseRoutine, TakeDamage 메서드는 이전 코드와 동일합니다.
    // isPerformingHurtAnimation 체크 조건이 이미 Update 상단에서 전체 로직을 스킵하도록 변경되었으므로,
    // 여기서 다시 isPerformingHurtAnimation 체크 조건을 추가할 필요는 없습니다.

    protected virtual void PlayIdleAnim() { /* 오버라이드에서 구현 */ }
    protected virtual void PlayWalkAnim() { /* 오버라이드에서 구현 */ }
    protected virtual void PlayJumpAnim() { /* 오버라이드에서 구현 */ }
    protected virtual void PlayDeathAnim() { /* 오버라이드에서 구현 */ }
    protected virtual void PlayAttack1Anim() { /* 오버라이드에서 구현 */ }
    protected virtual void PlayAttack2Anim() { /* 오버라이드에서 구현 */ }
    protected virtual void ResetAttackTriggers() { /* 오버라이드에서 구현 */ }
    protected virtual void OnAttackAnimationEnd()
    {
        isPerformingAttackAnimation = false;
        StartCoroutine(PostAttackPauseRoutine(postAttackPauseDuration));
    }

    protected IEnumerator PostAttackPauseRoutine(float duration)
    {
        isWaitingAfterAttack = true;
        PlayIdleAnim();
        yield return new WaitForSeconds(duration);
        isWaitingAfterAttack = false;
        if (!isDead)
        {
            SetState(EnemyState.Chase);
        }
    }

    public virtual void TakeDamage(float damage, GameObject attackObject)
    {
        if (isDead) return;
        currentHealth -= damage;
        Debug.Log(gameObject.name + " took " + damage + " damage. Current Health: " + currentHealth);
        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead);
        }
        // PlayHurtAnim() 호출은 파생 클래스의 TakeDamage 오버라이드에서 담당합니다.
    }

    // 피격 애니메이션 종료 시 호출될 Animation Event 메서드 (가상 메서드로 선언)
    public virtual void OnHurtAnimationEnd() // Base 클래스에서 virtual로 선언
    {
        Debug.Log(gameObject.name + ": Base OnHurtAnimationEnd called. Override this in derived class.");
    }
}