using System;
using UnityEngine;
using System.Collections;

// S_Attacker의 AI 상태를 정의하는 열거형
public enum SAttackerState
{
    Idle,            // 대기
    Patrol,          // 순찰 (좌우 이동)
    ReturnToOrigin,  // 초기 위치로 돌아가기
    Chase,           // 플레이어 추격
    Attack,          // 공격 (대시 공격 포함)
    WaitAfterAttack, // << 새로 추가된 상태: 공격 후 대기
    Hurt,            // 피격
    Stun,            // 스턴
    Death            // 사망
}

// 잡몹02(근거리) 캐릭터 컨트롤러
// CommonEnemyController를 상속받습니다.
public class S_AttackerController : CommonEnemyController
{
    // S_Attacker 전용 애니메이션 파라미터 이름
    private const string ANIM_BOOL_S_WALK = "S_Walk";
    private const string ANIM_TRIGGER_S_ATTACK_A = "S_Attack"; // S 공격 (대시)
    private const string ANIM_TRIGGER_S_HURT = "S_Hurt";       // 피격 애니메이션
    private const string ANIM_TRIGGER_S_STUN = "S_Stun";       // 스턴 애니메이션
    private const string ANIM_TRIGGER_S_DEATH = "S_Death";     // 죽음 애니메이션

    [Header("S_Attacker State")]
    protected SAttackerState currentSAttackerState = SAttackerState.Idle; // S_Attacker의 현재 AI 상태

    [Header("S_Attacker Hitbox")]
    public GameObject attackSHitboxObject; // S 공격 히트박스 오브젝트

    // 히트박스 컴포넌트 참조
    private BoxCollider2D attackSHitboxCollider;
    private EnemyHitbox attackSEnemyHitbox; // EnemyHitbox 스크립트가 존재한다고 가정

    [Header("S_Attacker Combat & Stats")]
    public float attackSValue = 0.4f; // A 공격 데미지 (개요: 0.4)
    public float attackSCooldown = 1f; // 공격 모션이 끝난 후 1s 텀 (개요: 1s 텀)

    // 능력치 데이터 (개요 반영)
    // CurrentHp는 CommonEnemyController에서 상속받지만, 시작 시 초기값을 설정합니다.
    [SerializeField] private float initialHealth = 4f; // 체력 (개요: 4)

    [Header("S_Attacker Movement")]
    [SerializeField] private float chaseSpeed = 4f; // 추적 속도 (패트롤보다 빠르게 설정)

    [Header("S_Attacker Stun")]
    protected bool isStunned = false; // S_Attacker 전용 스턴 상태 플래그
    public float stunDuration = 2f; // 스턴 지속 시간 (플레이어 패링 시 경직)
    private Coroutine stunCoroutine; // 실행 중인 스턴 코루틴 참조

    [Header("S_Attacker Dash Attack")]
    // 애니메이션 커브: 대시 공격 중 속도 변화를 제어합니다.
    public AnimationCurve dashSpeedCurve;
    public float dashBaseSpeed = 8f; // 대시 공격의 기본 속도
    private Coroutine dashAttackCoroutine; // 대시 공격 코루틴 참조

    // 플레이어 충돌을 막기 위한 메인 콜라이더 (대시 중 일시 비활성화용)
    private Collider2D mainCollider;

    [Header("S_Attacker Patrol Settings")]
    [SerializeField] private float patrolRange = 10f; // 패트롤 범위 확장 (기본값: 10)
    [SerializeField] private float patrolSpeed = 3f; // 패트롤 시 이동 속도 (기본값: 3)
    private Vector2 leftPatrolBoundary;  // 패트롤 왼쪽 경계
    private Vector2 rightPatrolBoundary; // 패트롤 오른쪽 경계

    // << 새로 추가된 필드: 공격 후 대기 상태를 위한 코루틴 참조 >>
    private Coroutine waitAfterAttackCoroutine;

    protected override void Awake()
    {
        base.Awake(); // Base CommonEnemyController Awake 호출
        // S_AttackerController에 특화된 초기화
        mainCollider = GetComponent<Collider2D>();
        if (mainCollider == null) Debug.LogError("Main Collider2D component not found on S_Attacker!", this);
    }

    // Start 메서드에 추가할 코드
    protected override void Start()
    {
        // =========================================================================
        // 플레이어 찾는 코드
        // =========================================================================
        GameObject playerGameObject = GameObject.FindWithTag("Player");

        if (playerGameObject != null)
        {
            SetPlayerTarget(playerGameObject.transform);
            Debug.Log($"S_Attacker: Start()에서 플레이어 '{playerGameObject.name}'를 찾았습니다. 플레이어 위치: {playerGameObject.transform.position}", this);
        }
        else
        {
            Debug.LogError("S_Attacker: Start()에서 'Player' 태그를 가진 게임 오브젝트를 찾을 수 없습니다! 플레이어가 씬에 있는지, 태그가 올바른지 확인하세요.", this);
        }
        // =========================================================================

        base.Start(); // Base CommonEnemyController Start 호출

        // 능력치 데이터 초기화
        CurrentHp = initialHealth; // 체력 설정

        // 공격 관련 파라미터 확인 및 조정
        if (attackRange <= 0)
        {
            attackRange = 2.0f; // 공격 범위 기본값 설정
            Debug.LogWarning($"공격 범위가 0 이하입니다. 기본값 {attackRange}로 설정합니다.");
        }
        if (detectionRange <= 0)
        {
            detectionRange = 5.0f; // 감지 범위 기본값 설정
            Debug.LogWarning($"감지 범위가 0 이하입니다. 기본값 {detectionRange}로 설정합니다.");
        }

        // 이동 속도 확인 (CommonEnemyController에서 상속 받은 moveSpeed가 설정되지 않았을 경우)
        if (moveSpeed <= 0)
        {
            moveSpeed = 3.0f; // 기본 이동 속도 설정
            Debug.LogWarning($"이동 속도가 0 이하입니다. 기본값 {moveSpeed}로 설정합니다.");
        }

        // chaseSpeed가 설정되지 않았을 경우 (클래스에 추가한 새 필드)
        if (chaseSpeed <= 0)
        {
            chaseSpeed = moveSpeed * 1.2f; // moveSpeed보다 약간 빠르게 설정
            Debug.LogWarning($"추적 속도가 0 이하입니다. 기본값 {chaseSpeed}로 설정합니다.");
        }

        // 넓은 패트롤 범위 설정 - 시작 위치 기준으로 좌우로 patrolRange만큼 이동하도록 설정
        leftPatrolBoundary = new Vector2(originalPosition.x - patrolRange, originalPosition.y);
        rightPatrolBoundary = new Vector2(originalPosition.x + patrolRange, originalPosition.y);

        // 모든 초기화 파라미터 로깅
        Debug.Log($"초기화 완료: 공격 범위={attackRange}, 감지 범위={detectionRange}, 기본 이동 속도={moveSpeed}, 추적 속도={chaseSpeed}, 공격 쿨다운={attackSCooldown}, 패트롤 범위={patrolRange}");
        Debug.Log($"패트롤 경계: 왼쪽={leftPatrolBoundary}, 오른쪽={rightPatrolBoundary}, 시작 위치={originalPosition}");

        // 히트박스 오브젝트 및 컴포넌트 참조 초기화
        if (attackSHitboxObject != null)
        {
            attackSHitboxCollider = attackSHitboxObject.GetComponent<BoxCollider2D>();
            attackSEnemyHitbox = attackSHitboxObject.GetComponent<EnemyHitbox>();
            if (attackSHitboxCollider != null)
            {
                attackSHitboxCollider.enabled = false; // 초기 공격 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("BoxCollider2D component not found on Attack S Hitbox Object.", this);
            }
            if (attackSEnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack S Hitbox Object!", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack S Hitbox Object is not assigned in the Inspector.", this);
        }

        isStunned = false; // 스턴 플래그 초기화
        nextAttackTime = Time.time; // 다음 공격 시간 초기화 (즉시 공격 가능)

        // 인스펙터에서 AnimationCurve가 설정되지 않은 경우 기본 커브 사용
        if (dashSpeedCurve == null || dashSpeedCurve.length == 0)
        {
            dashSpeedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // 기본 Ease-in/out 커브
            Debug.LogWarning("Dash Speed Curve not set in Inspector. Using default EaseInOut curve.", this);
        }

        // S_Attacker는 시작 시 바로 순찰 상태로 진입하도록 설정
        SetSAttackerState(SAttackerState.Patrol);
    }

    // Update 오버라이드: S_Attacker의 모든 AI 로직을 이곳에서 직접 관리합니다.
    // Update() 메서드 내 주석 처리된 디버그 로그를 활성화하고 추가 정보 로깅
    protected override void Update()
    {
        // 상태 디버깅을 위한 로그 추가
        Debug.Log($"Current State: {currentSAttackerState}, Player Distance: {(playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position).ToString() : "N/A")}, Detection Range: {detectionRange}");

        // 1. 행동 불가 상태 (사망, 스턴, 피격, 공격 애니메이션 중, 공격 후 대기 중) 일 때는
        // 모든 움직임을 멈추고 AI 로직 스킵
        if (IsDead || isStunned || isPerformingHurtAnimation || isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 행동 불가 상태일 때는 움직임을 강제 정지
            }
            PlayIdleAnim(); // 이 시간 동안은 대기 애니메이션 재생
            return; // 이후 AI 로직을 실행하지 않고 종료
        }

        // 2. 현재 상태에 따른 AI 동작 수행 (S_Attacker 전용 로직)
        HandleSAttackerStateLogic();
    }

    // S_Attacker 전용 상태 로직 처리
    protected virtual void HandleSAttackerStateLogic()
    {
        // 플레이어 트랜스폼이 없으면 다시 찾아보기
        if (playerTransform == null)
        {
            GameObject playerGameObject = GameObject.FindWithTag("Player");
            if (playerGameObject != null)
            {
                SetPlayerTarget(playerGameObject.transform);
                Debug.Log($"플레이어를 다시 찾음: {playerGameObject.name}", this);
            }
            else
            {
                // 플레이어가 없으면 순찰 활성화 여부에 따라 순찰 또는 원점 복귀
                SetSAttackerState(enablePatrol ? SAttackerState.Patrol : SAttackerState.ReturnToOrigin);
                Debug.LogWarning("플레이어를 찾을 수 없습니다!", this);
                return;
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        Debug.Log($"플레이어와의 거리: {distanceToPlayer}, 감지 범위: {detectionRange}, 공격 범위: {attackRange}");

        // 여기서부터는 기존 코드와 동일...
        SAttackerState nextState;

        // 3. 상태 전환 결정 로직
        // 주인공이 인식 범위에 들어오면 자동으로 추적
        if (distanceToPlayer <= detectionRange)
        {
            // 유효 공격범위 안에 들어오고, 공격 쿨다운이 지났다면
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                nextState = SAttackerState.Attack; // 공격
                Debug.Log($"공격 범위 내 플레이어 감지! 거리: {distanceToPlayer}, 공격 범위: {attackRange}, 공격 상태로 전환");
            }
            else // 플레이어가 감지 범위 내에 있지만 공격 범위 밖이거나, 공격 쿨다운 중이라면
            {
                nextState = SAttackerState.Chase; // 계속 추격
                Debug.Log($"플레이어 추격 중. 거리: {distanceToPlayer}, 추격 상태 유지/전환");
            }
        }
        else // 플레이어가 감지 범위 밖에 있을 경우
        {
            // 플레이어를 놓쳤거나, 공격 후 쿨다운 중일 때: 순찰 활성화 여부에 따라 순찰 또는 원점 복귀
            if (enablePatrol)
            {
                nextState = SAttackerState.Patrol;
                Debug.Log("플레이어가 감지 범위를 벗어남. 순찰 상태로 전환");
            }
            else
            {
                nextState = SAttackerState.ReturnToOrigin;
                Debug.Log("플레이어가 감지 범위를 벗어남. 원점 복귀 상태로 전환");
            }
        }

        // 상태 변경이 필요한 경우에만 SetSAttackerState 호출
        if (nextState != currentSAttackerState)
        {
            Debug.Log($"상태 전환: {currentSAttackerState} -> {nextState}");
            SetSAttackerState(nextState);
        }

        // 4. 현재 상태에 따른 행동 수행
        switch (currentSAttackerState)
        {
            case SAttackerState.Idle:
                PlayIdleAnim();
                if (rb != null) rb.linearVelocity = Vector2.zero; // 정지
                break;
            case SAttackerState.Patrol:
                // 확장된 패트롤 로직 사용
                MoveWiderPatrol();
                PlayWalkAnim();
                break;
            case SAttackerState.ReturnToOrigin:
                base.MoveTowardsOrigin(); // Base 클래스의 원점 복귀 로직 사용 (CommonEnemyController)
                PlayWalkAnim();
                break;
            // Chase 상태 부분을 수정합니다. HandleSAttackerStateLogic() 메서드 내의 switch 문에서 Chase 케이스를 다음과 같이 수정:
            // HandleSAttackerStateLogic() 메서드 내의 switch 문에서 Chase 케이스를 다음과 같이 수정:
            case SAttackerState.Chase:
                FlipTowardsPlayer(); // 플레이어 방향을 바라보도록 즉시 뒤집기

                // 플레이어 추적을 위한 이동 로직 완전 재작성
                if (playerTransform != null && rb != null)
                {
                    // 플레이어 방향 명확히 계산
                    float directionX = Mathf.Sign(playerTransform.position.x - transform.position.x);

                    // 직접 transform 위치 이동 (Rigidbody 제약이 있을 수 있으므로)
                    Vector3 movement = new Vector3(directionX * chaseSpeed * Time.deltaTime, 0, 0);
                    transform.position += movement;

                    // 디버그 로그 추가
                    Debug.Log($"Chase: Moving towards player, direction={directionX}, movement={movement}, position={transform.position}");
                }
                PlayWalkAnim();
                break;
            case SAttackerState.Attack:
                // 공격 중에는 이동 멈춤
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                PerformAttackLogic(); // 공격 로직 호출 (PerformAttackLogic 내부에서 isPerformingAttackAnimation 체크)
                // Debug.Log($"공격 상태 진입: isPerformingAttackAnimation={isPerformingAttackAnimation}, isWaitingAfterAttack={isWaitingAfterAttack}");
                break;
            case SAttackerState.WaitAfterAttack: // << 새로 추가된 상태 처리 >>
                PlayIdleAnim(); // 대기 애니메이션 재생
                if (rb != null) rb.linearVelocity = Vector2.zero; // 완벽히 정지
                break;
                // Hurt, Stun, Death 상태는 Update 상단에서 이미 행동 불가로 처리되므로 여기에 추가할 필요 없음
        }
    }

    // 더 넓은 패트롤 이동 로직
    protected void MoveWiderPatrol()
    {
        if (rb == null) return; // Rigidbody2D가 없으면 이동하지 않음

        // 현재 위치가 순찰 경계를 벗어났는지 확인
        if (_currentPatrolDirection > 0 && transform.position.x >= rightPatrolBoundary.x)
        {
            // 오른쪽 경계에 도달했으면 방향 전환
            _currentPatrolDirection = -1;
            // << 수정: FlipSprite() 대신 Flip(true) 호출 >>
            Flip(true); // 왼쪽을 바라보도록 뒤집기
            Debug.Log("패트롤: 오른쪽 경계 도달, 방향 전환");
        }
        else if (_currentPatrolDirection < 0 && transform.position.x <= leftPatrolBoundary.x)
        {
            // 왼쪽 경계에 도달했으면 방향 전환
            _currentPatrolDirection = 1;
            // << 수정: FlipSprite() 대신 Flip(false) 호출 >>
            Flip(false); // 오른쪽을 바라보도록 뒤집기
            Debug.Log("패트롤: 왼쪽 경계 도달, 방향 전환");
        }

        rb.linearVelocity = new Vector2(_currentPatrolDirection * patrolSpeed, rb.linearVelocity.y);
    }

    // TakeDamage 오버라이드 (스턴 상태 처리 추가)
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        // 스턴 중에는 데미지를 받지만, 피격 애니메이션은 재생하지 않음 (스턴이 풀리지 않도록)
        if (isStunned)
        {
            CurrentHp -= damage; // 체력만 감소
        }
        else // 스턴 중이 아니라면 일반 피격 처리
        {
            base.TakeDamage(damage, attackObject); // Base 클래스의 피격 및 사망 처리 호출
        }

        if (CurrentHp <= 0 && !IsDead) // 스턴 중이든 아니든 체력이 0 이하면 사망 처리
        {
            HandleDeathLogic();
            OnDead?.Invoke();
        }
        else if (CurrentHp > 0 && !isStunned) // 체력이 남았고 스턴 중이 아닐 때만 피격 애니메이션
        {
            isPerformingHurtAnimation = true;
            PlayHurtAnim();
        }
    }

    // 스턴 상태 처리 (플레이어 패링 시 경직)
    public void Stun() // 외부에서 호출 가능 (예: 플레이어 패링)
    {
        if (IsDead || isStunned || isPerformingHurtAnimation || isPerformingAttackAnimation) return;

        isStunned = true;
        SetSAttackerState(SAttackerState.Stun); // 상태를 Stun으로 변경
        PlayStunAnim(); // 스턴 애니메이션 재생

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    // 스턴 해제 코루틴
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            isStunned = false;
            // 스턴이 풀리면 다시 상태 판단 로직으로 돌아감
            // HandleSAttackerStateLogic()가 다음 Update에서 적절한 상태로 전환할 것임
            SetSAttackerState(SAttackerState.Patrol); // 스턴 후 패트롤 복귀 (플레이어가 감지 범위 밖에 있다면 추후 변경됨)
        }
    }

    // ===== 애니메이션 재생 함수 (Base 클래스 오버라이드) =====

    protected override void PlayIdleAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_S_WALK, false); // S_Attacker는 S_Walk를 사용
    }

    protected override void PlayWalkAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_S_WALK, true); // S_Attacker는 S_Walk를 사용
    }

    // S_Attacker는 점프 애니메이션이 없으므로 비어있음
    protected override void PlayJumpAnim() { }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_S_DEATH);
    }

    // 피격 애니메이션 재생 (isPerformingHurtAnimation 플래그 설정)
    protected override void PlayHurtAnim()
    {
        if (!IsDead && !isStunned && animator != null) // 죽거나 스턴 중이 아닐 때만 피격 가능
        {
            isPerformingHurtAnimation = true;
            animator.SetTrigger(ANIM_TRIGGER_S_HURT);
        }
    }

    // 스턴 애니메이션 재생
    protected void PlayStunAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_S_STUN);
        }
    }

    // Base 클래스의 PlayAttack1Anim 오버라이드 (S Attack용)
    protected override void PlayAttack1Anim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
        {
            Debug.Log("S_Attack 애니메이션 트리거 활성화");
            isPerformingAttackAnimation = true; // 공격 중 플래그 설정
            animator.SetBool(ANIM_BOOL_S_WALK, false); // 공격 중에는 걷기 애니메이션 비활성화
            animator.SetTrigger(ANIM_TRIGGER_S_ATTACK_A);

            // 대시 공격 코루틴 시작
            if (dashAttackCoroutine != null) StopCoroutine(dashAttackCoroutine);
            dashAttackCoroutine = StartCoroutine(DashAttackCoroutine());
        }
        else
        {
            Debug.LogWarning($"공격 애니메이션 재생 실패! IsDead={IsDead}, isStunned={isStunned}, isPerformingHurtAnimation={isPerformingHurtAnimation}, animator={(animator != null ? "존재함" : "없음")}");
        }
    }

    // Base 클래스의 ResetAttackTriggers 오버라이드
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_S_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_S_HURT);
            animator.ResetTrigger(ANIM_TRIGGER_S_STUN);
        }
    }

    // ===== AI 공격 로직 (Base 클래스 PerformAttackLogic 오버라이드) =====
    protected override void PerformAttackLogic()
    {
        // 공격 애니메이션이 이미 진행 중이거나, 공격 후 대기 중이라면 다시 공격하지 않음
        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            // Debug.Log($"공격 로직 무시: 이미 공격 중이거나 대기 중. isPerformingAttackAnimation={isPerformingAttackAnimation}, isWaitingAfterAttack={isWaitingAfterAttack}");
            return;
        }

        Debug.Log("공격 애니메이션 실행!");
        PlayAttack1Anim(); // S_AttackA 트리거 및 DashAttackCoroutine 시작
    }

    // 대시 공격 코루틴
    private IEnumerator DashAttackCoroutine()
    {
        Debug.Log("대시 공격 코루틴 시작");
        isPerformingAttackAnimation = true; // AI 이동을 막기 위한 플래그 설정

        // 대시 공격 중 플레이어와의 충돌을 막기 위해 메인 콜라이더 일시 비활성화
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
            Debug.Log("공격 중 메인 콜라이더 비활성화");
        }

        float timer = 0f;
        float dashAnimationDuration = 1f; // 대시 공격 애니메이션의 예상 지속 시간 (애니메이터에서 확인)

        // 애니메이터에서 현재 재생 중인 공격 클립의 길이를 가져오기
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            foreach (AnimatorClipInfo info in clipInfo)
            {
                // S_AttackA라는 이름이 포함된 클립을 찾아 길이를 사용 (실제 애니메이션 클립 이름에 따라 수정 필요)
                if (info.clip.name.Contains("S_AttackA"))
                {
                    dashAnimationDuration = info.clip.length;
                    // Debug.Log($"대시 공격 애니메이션 길이: {dashAnimationDuration}초");
                    break;
                }
            }
        }

        while (timer < dashAnimationDuration)
        {
            // 대시 공격 중 스턴/사망/피격 시 즉시 중지
            if (IsDead || isStunned || isPerformingHurtAnimation)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                if (mainCollider != null) mainCollider.enabled = true; // 콜라이더 다시 활성화
                isPerformingAttackAnimation = false; // 공격 중 플래그 해제
                yield break;
            }

            Vector3 moveDirection = new(Mathf.Sign(transform.localScale.x), 0, 0); // 현재 바라보는 방향으로 이동
            float currentSpeedMultiplier = dashSpeedCurve.Evaluate(timer / dashAnimationDuration);
            float currentDashMovement = currentSpeedMultiplier * dashBaseSpeed * Time.deltaTime;

            transform.position += moveDirection * currentDashMovement; // << 대시 공격은 transform 직접 이동 >>

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 대시 공격 후 메인 콜라이더 다시 활성화
        if (mainCollider != null)
        {
            mainCollider.enabled = true;
        }

        // Base 클래스의 OnAttackAnimationEnd를 호출하여 공격 후 딜레이 및 플래그 초기화 처리
        OnAttackAnimationEnd();
    }


    // 공격 애니메이션 종료 시 호출될 이벤트
    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd(); // CommonEnemyController의 OnAttackAnimationEnd 호출

        // S_Attacker 전용 쿨다운 시간 설정
        nextAttackTime = Time.time + attackSCooldown;

        // << 변경: 공격이 끝나면 바로 순찰이 아닌, '대기' 상태로 진입 >>
        SetSAttackerState(SAttackerState.WaitAfterAttack);
        Debug.Log($"공격 종료! {attackSCooldown}초 동안 대기 상태 진입.");

        // 패트롤 방향을 초기 위치(originalPosition) 쪽으로 향하게 하여 '제자리 돌아가기' 느낌을 줌
        // 즉, 플레이어를 바라보는 방향으로 패트롤을 시작하지 않고, 원래 초기 위치 방향으로 패트롤 시작
        _currentPatrolDirection = (originalPosition.x > transform.position.x) ? 1 : -1;
    }

    // << 새로 추가된 코루틴: 공격 후 쿨다운 동안 대기 >>
    private IEnumerator WaitForAttackCooldown()
    {
        isWaitingAfterAttack = true; // 공격 후 대기 플래그 설정
        yield return new WaitForSeconds(attackSCooldown);
        isWaitingAfterAttack = false; // 대기 시간 종료

        // 대기 시간이 끝나면 다음 행동 결정 (플레이어와의 거리에 따라 추격 또는 순찰/원점복귀)
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= detectionRange)
            {
                SetSAttackerState(SAttackerState.Chase);
                Debug.Log("대기 후 플레이어가 감지 범위 내에 있어 추격 시작.");
            }
            else
            {
                if (enablePatrol)
                {
                    SetSAttackerState(SAttackerState.Patrol);
                    Debug.Log("대기 후 플레이어가 감지 범위 밖에 있어 순찰 시작.");
                }
                else
                {
                    SetSAttackerState(SAttackerState.ReturnToOrigin);
                    Debug.Log("대기 후 플레이어 감지 실패, 원점 복귀.");
                }
            }
        }
        else
        {
            if (enablePatrol)
            {
                SetSAttackerState(SAttackerState.Patrol); // 플레이어가 없으면 순찰
                Debug.Log("대기 후 플레이어 트랜스폼이 없어 순찰 시작.");
            }
            else
            {
                SetSAttackerState(SAttackerState.ReturnToOrigin);
                Debug.Log("대기 후 플레이어 트랜스폼이 없어 원점 복귀.");
            }
        }
    }


    // 피격 애니메이션 종료 시 호출될 이벤트
    public override void OnHurtAnimationEnd()
    {
        base.OnHurtAnimationEnd(); // Base 클래스에서 isPerformingHurtAnimation = false 설정
        // 피격 후 S_Attacker는 무조건 순찰 상태로 복귀
        SetSAttackerState(SAttackerState.Patrol);
    }

    // SAttackerState 변경 메서드
    protected void SetSAttackerState(SAttackerState newState)
    {
        if (currentSAttackerState == newState) return; // 불필요한 상태 변경 방지

        // 기존 상태에서 실행 중이던 특정 코루틴이 있다면 정지 (중요!)
        switch (currentSAttackerState)
        {
            case SAttackerState.Attack:
                if (dashAttackCoroutine != null)
                {
                    StopCoroutine(dashAttackCoroutine);
                    dashAttackCoroutine = null;
                }
                // 공격 중 콜라이더 비활성화했다면 여기서 다시 활성화
                if (mainCollider != null && !mainCollider.enabled)
                {
                    mainCollider.enabled = true;
                }
                break;
            case SAttackerState.WaitAfterAttack:
                if (waitAfterAttackCoroutine != null)
                {
                    StopCoroutine(waitAfterAttackCoroutine);
                    waitAfterAttackCoroutine = null; // 참조 해제
                }
                break;
            case SAttackerState.Stun:
                if (stunCoroutine != null)
                {
                    StopCoroutine(stunCoroutine);
                    stunCoroutine = null;
                }
                break;
        }

        currentSAttackerState = newState;
        Debug.Log($"S_Attacker transitioned to state: {currentSAttackerState}"); // 디버깅용

        // 새로운 상태 진입 시 필요한 코루틴 시작
        switch (currentSAttackerState)
        {
            case SAttackerState.WaitAfterAttack:
                waitAfterAttackCoroutine = StartCoroutine(WaitForAttackCooldown());
                break;
            case SAttackerState.Stun: // Stun 상태 진입 시 스턴 코루틴 다시 시작 (Stun()에서 이미 시작하지만 안전 장치)
                if (stunCoroutine != null) StopCoroutine(stunCoroutine);
                stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
                break;
                // Attack 상태는 PerformAttackLogic()에서 PlayAttack1Anim()을 호출하고, 
                // PlayAttack1Anim()이 DashAttackCoroutine을 시작하므로 여기서 별도 시작 로직 불필요
        }
    }

    // ===== 히트박스 제어 메서드 (애니메이션 이벤트에 연결) =====

    /// <summary>
    /// 공격 히트박스를 활성화합니다. 애니메이션 이벤트에 연결하여 사용합니다.
    /// </summary>
    public void EnableAttackHitbox()
    {
        if (attackSHitboxCollider != null)
        {
            attackSHitboxCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning("[S_Attacker] Attack S Hitbox Collider is null. Cannot enable.", this);
        }
    }

    /// <summary>
    /// 공격 히트박스를 비활성화합니다. 애니메이션 이벤트에 연결하여 사용합니다.
    /// </summary>
    public void DisableAttackHitbox()
    {
        if (attackSHitboxCollider != null)
        {
            attackSHitboxCollider.enabled = false;
        }
        else
        {
            Debug.LogWarning("[S_Attacker] Attack S Hitbox Collider is null. Cannot disable.", this);
        }
    }

    // ===== 플레이어 방향으로 뒤집는 메서드 =====
    /// <summary>
    /// 플레이어의 위치에 따라 적의 Sprite를 뒤집습니다.
    /// </summary>
    protected void FlipTowardsPlayer()
    {
        if (playerTransform == null) return;

        // 플레이어의 x 위치와 자신의 x 위치를 비교
        float directionToPlayer = playerTransform.position.x - transform.position.x;

        // 현재 바라보는 방향과 플레이어 방향이 다르면 뒤집기
        // (현재 scale.x가 양수면 오른쪽, 음수면 왼쪽을 바라봄)
        if (directionToPlayer > 0 && transform.localScale.x < 0) // 플레이어가 오른쪽에 있는데 왼쪽을 보고 있다면
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1; // x 스케일을 반전
            transform.localScale = scale;
        }
        else if (directionToPlayer < 0 && transform.localScale.x > 0) // 플레이어가 왼쪽에 있는데 오른쪽을 보고 있다면
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1; // x 스케일을 반전
            transform.localScale = scale;
        }
    }
}