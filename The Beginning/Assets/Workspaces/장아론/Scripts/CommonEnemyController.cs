using UnityEngine;
using System.Collections;

// 기본적인 적 캐릭터의 동작 및 속성을 관리하는 공통 컨트롤러 (Base Class)
public class CommonEnemyController : MonoBehaviour
{
    // ===== 기본 속성 =====
    [Header("Base Stats")]
    public int maxHealth = 100;
    protected int currentHealth;
    public float moveSpeed = 3f; // 걷기/물러나기 속도
    public float attackDamage = 10f; // 기본 공격 데미지 (실제 적용은 파생 클래스나 애니메이션 이벤트에서)

    // ===== AI 파라미터 =====
    [Header("AI Parameters")]
    public float detectionRange = 10f; // 플레이어 감지 거리
    public float attackRange = 2f; // 공격 시작 및 유지 거리
    public float maintainBuffer = 0.5f; // attackRange보다 얼마나 더 가까워져야 물러날지 결정
    public string playerObjectName = "Player"; // 플레이어 오브젝트 이름

    // ===== 애니메이터 및 애니메이션 상태 =====
    protected Animator animator;
    protected GameObject player;
    protected bool isDead = false; // 사망 상태
    protected bool isPerformingAttackAnimation = false; // <-- 공격 애니메이션 재생 중인지 여부 (위치 고정용)

    // 적의 현재 AI 상태
    protected enum EnemyState { Idle, Chase, Attack, Dead } // Enum 이름은 공통적으로 사용
    protected EnemyState currentState = EnemyState.Idle; // 초기 상태

    // 적 캐릭터의 초기 방향 (좌우 반전에 사용)
    public float initialFacingDirection = 1f; // 오른쪽을 보면 1, 왼쪽을 보면 -1


    protected virtual void Awake()
    {
        // Start보다 먼저 호출될 수 있도록 Awake 사용 (참조 설정 등에 유리)
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component not found on " + gameObject.name);

        currentHealth = maxHealth; // 체력 초기화
    }

    protected virtual void Start()
    {
        SetState(EnemyState.Idle); // 초기 상태 설정

        player = GameObject.Find(playerObjectName);
        if (player == null)
        {
            Debug.LogError("Player GameObject with name '" + playerObjectName + "' not found! Check name/scene.");
        }
    }

    protected virtual void Update()
    {
        if (isDead) return; // 죽은 상태면 아무것도 안함

        // 플레이어 없으면 Idle 상태 유지
        if (player == null)
        {
            if (currentState != EnemyState.Idle) SetState(EnemyState.Idle);
            return;
        }

        UpdateFacingDirection(); // 플레이어 방향 바라보기

        // 공격 애니메이션 중이면 AI 판단 및 이동 로직 스킵
        if (isPerformingAttackAnimation)
        {
            // Debug.Log("공격 애니메이션 중. AI 판단 스킵.");
            return;
        }

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // ===== AI 상태 전환 로직 (공통) =====
        switch (currentState)
        {
            case EnemyState.Idle:
                // 플레이어가 감지 범위에 들어오면 추적
                if (distanceToPlayer <= detectionRange)
                {
                    Debug.Log("Idle -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // 플레이어가 공격 범위에 들어오면 공격
                if (distanceToPlayer <= attackRange)
                {
                    Debug.Log("Chase -> Attack (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Attack);
                }
                // 플레이어가 감지 범위를 벗어나면 Idle
                else if (distanceToPlayer > detectionRange)
                {
                    Debug.Log("Chase -> Idle (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Idle);
                }
                // 감지 범위 내, 공격 범위 밖 -> 계속 추적
                else
                {
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                // 플레이어가 공격 범위를 벗어나면 추적
                if (distanceToPlayer > attackRange)
                {
                    Debug.Log("Attack -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                // 플레이어 공격 범위 내
                else
                {
                    // 간격 유지
                    if (distanceToPlayer < attackRange - maintainBuffer)
                    {
                        Debug.Log("Too close, retreating.");
                        MoveAwayFromPlayer();
                    }
                    // 적절한 공격 거리 -> 공격 로직 수행 (PerformAttackLogic은 파생 클래스에서 구현)
                    else
                    {
                        // Debug.Log("Attempting attack logic.");
                        PerformAttackLogic(); // <-- 파생 클래스에서 오버라이드하여 공격 패턴 구현
                    }
                }
                break;

            case EnemyState.Dead:
                // 죽은 상태에서는 AI 로직 없음
                break;
        }
        // ===== AI 상태 전환 로직 끝 =====
    }

    // ===== AI 상태 및 동작 관련 함수들 (공통) =====

    // AI 상태를 설정하고 애니메이션 및 변수 업데이트
    protected virtual void SetState(EnemyState newState)
    {
        Debug.Log(">>> SetState: " + currentState.ToString() + " -> " + newState.ToString());
        if (currentState == newState) return;

        // 이전 상태 종료 로직 (필요시 파생 클래스에서 오버라이드)
        // 예: case EnemyState.Attack: isPerformingAttackAnimation = false; break;

        currentState = newState; // 상태 변경

        // 새 상태 진입 로직
        switch (currentState)
        {
            case EnemyState.Idle:
                PlayIdleAnim(); // Idle 애니메이션 재생 (파생 클래스에서 오버라이드)
                animator.SetBool("IsWalking", false); // <-- 공통적인 걷기 Bool 파라미터 (애니메이터에 "IsWalking" Bool 필요)
                isPerformingAttackAnimation = false; // 공격 애니메이션 중 상태 해제
                ResetAttackTriggers(); // 공격 트리거 초기화
                break;

            case EnemyState.Chase:
                PlayWalkAnim(); // Walk 애니메이션 재생 (파생 클래스에서 오버라이드)
                animator.SetBool("IsWalking", true); // <-- 공통적인 걷기 Bool 파라미터
                isPerformingAttackAnimation = false; // 공격 애니메이션 중 상태 해제
                ResetAttackTriggers(); // 공격 트리거 초기화
                // 점프 트리거도 초기화 (필요하다면)
                animator.ResetTrigger("Jump"); // <-- 공통적인 Jump 트리거 (애니메이터에 "Jump" Trigger 필요)
                break;

            case EnemyState.Attack:
                PlayIdleAnim(); // 공격 준비 중에는 Idle 상태 (파생 클래스에서 오버라이드)
                animator.SetBool("IsWalking", false); // <-- 공통적인 걷기 Bool 파라미터
                                                      // isPerformingAttackAnimation는 PerformAttackLogic에서 설정
                                                      // 공격 상태 진입 시 다른 트리거 초기화
                animator.ResetTrigger("Jump"); // <-- 공통적인 Jump 트리거
                ResetAttackTriggers(); // 공격 트리거 초기화
                break;

            case EnemyState.Dead:
                isDead = true; // 사망 플래그
                isPerformingAttackAnimation = false; // 공격 애니메이션 중 상태 해제
                animator.SetBool("IsWalking", false); // 걷기 멈춤
                ResetAttackTriggers(); // 공격 트리거 초기화
                animator.ResetTrigger("Jump"); // 점프 트리거 초기화
                PlayDeathAnim(); // 사망 애니메이션 재생 (파생 클래스에서 오버라이드)
                // TODO: 사망 후 추가 처리 (오브젝트 비활성화/파괴 등)
                break;
        }
    }

    // 플레이어 방향으로 이동 (Chase 상태)
    protected virtual void MoveTowardsPlayer()
    {
        if (player == null || currentState != EnemyState.Chase || isPerformingAttackAnimation) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position);
        directionToPlayer.y = 0; // Y축 무시 (2D나 평지 3D)

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    // 플레이어 반대 방향으로 이동 (Attack 상태에서 너무 가까울 때)
    protected virtual void MoveAwayFromPlayer()
    {
        if (player == null || currentState != EnemyState.Attack || isPerformingAttackAnimation) return;

        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position);
        directionAwayFromPlayer.y = 0; // Y축 무시

        if (directionAwayFromPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionAwayFromPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

            transform.position += movement;
        }
    }

    // 파생 클래스에서 오버라이드하여 실제 공격 로직 구현 (쿨타임, 패턴 등)
    protected virtual void PerformAttackLogic()
    {
        // 기본 클래스에서는 특별한 공격 로직 없음.
        // 파생 클래스에서 이 메소드를 오버라이드하여
        // 쿨타임 체크, 공격 애니메이션 트리거 발동, isPerformingAttackAnimation = true 설정 등을 구현
        Debug.Log("Base PerformAttackLogic called. Override this in derived class.");
    }

    // 플레이어 방향으로 캐릭터 좌우 반전
    protected virtual void UpdateFacingDirection()
    {
        if (player == null) return;

        float directionX = player.transform.position.x - transform.position.x;
        Vector3 currentScale = transform.localScale;

        if (directionX > 0.01f)
        {
            // 플레이어가 오른쪽에 있을 때 (캐릭터가 오른쪽을 바라보도록 스케일 조정)
            // Mathf.Sign(initialFacingDirection)이 1이면 -Scale.x, -1이면 +Scale.x
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
        else if (directionX < -0.01f)
        {
            // 플레이어가 왼쪽에 있을 때 (캐릭터가 왼쪽을 바라보도록 스케일 조정)
            // Mathf.Sign(initialFacingDirection)이 1이면 +Scale.x, -1이면 -Scale.x
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * +Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
    }

    // ===== 애니메이션 관련 함수들 (파생 클래스에서 오버라이드) =====
    // 각 애니메이션 재생 함수는 파생 클래스에서 실제로 해당 애니메이션 트리거를 발동시킵니다.

    protected virtual void PlayIdleAnim() { } // 파생 클래스에서 구현 (예: animator.SetBool("B_Walk", false);)
    protected virtual void PlayWalkAnim() { } // 파생 클래스에서 구현 (예: animator.SetBool("B_Walk", true);)
    protected virtual void PlayJumpAnim() { } // 파생 클래스에서 구현 (예: animator.SetTrigger("B_Jump");)
    protected virtual void PlayDeathAnim() { } // 파생 클래스에서 구현 (예: animator.SetTrigger("B_Death");)
    protected virtual void PlayAttack1Anim() { } // 파생 클래스에서 구현 (예: animator.SetTrigger("B_Attack1");)
    protected virtual void PlayAttack2Anim() { } // 파생 클래스에서 구현 (예: animator.SetTrigger("B_Attack2");)
    // ... 다른 공격 애니메이션이 있다면 추가 ...

    // 모든 공격 트리거 초기화 (애니메이션 전환이 꼬이는 것을 방지)
    protected virtual void ResetAttackTriggers()
    {
        // 파생 클래스에서 사용하는 실제 공격 트리거 이름을 리셋
        // 예: animator.ResetTrigger("B_Attack1"); animator.ResetTrigger("B_Attack2");
    }

    // ===== 애니메이션 이벤트에서 호출될 함수 (공통) =====
    // Animator의 공격 애니메이션 클립 끝에 이 이벤트를 추가해야 합니다.
    public void OnAttackAnimationEnd()
    {
        Debug.Log("공격 애니메이션 종료 이벤트 발생 (Base Class).");
        isPerformingAttackAnimation = false; // 공격 애니메이션 종료 시 위치 고정 플래그 끔
        // Note: 애니메이션 종료 후 AI 상태는 다음 Update 프레임에서 자동으로 판단됩니다.
    }

    // ===== 데미지 처리 예시 =====
    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead); // 체력이 0 이하면 사망 상태로 전환
        }
        // TODO: 데미지 피격 애니메이션 재생 등 추가
    }
}