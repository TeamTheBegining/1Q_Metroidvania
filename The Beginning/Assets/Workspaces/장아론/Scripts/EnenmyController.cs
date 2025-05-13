using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EnemyController : TestBase
{
#if UNITY_EDITOR
    // 애니메이터 참조
    private Animator animator;

    // 애니메이션 상태 관리를 위한 변수들
    private bool isWalking = false; // 걷기 애니메이션 상태 (Animator 파라미터와 연동)
    private bool isDead = false; // 사망 상태 (AI 및 기타 시스템에서 사용)

    // 플레이어 오브젝트 참조
    private GameObject player;
    // 적 캐릭터가 처음 생성될 때 바라보는 방향 (예: 오른쪽을 보면 1, 왼쪽을 보면 -1)
    public float initialFacingDirection = 1f;

    // 플레이어 오브젝트의 이름 (Inspector에서 설정)
    public string playerObjectName = "Player";

    // ===== AI 파라미터 =====
    // 플레이어를 감지하는 거리 (이 범위 안에 들어와야 추격 시작)
    public float detectionRange = 10f;

    // 플레이어에게서 멈춰서 공격을 시작하는 거리 (이 범위 안에 들어오면 멈춤/공격)
    // 또한, 이 거리가 유지하려는 목표 간격이 됩니다.
    public float attackRange = 2f;

    // 플레이어가 attackRange보다 얼마나 더 가까워져야 뒤로 물러날지 결정하는 버퍼
    public float maintainBuffer = 0.5f;

    // 걷기 상태일 때 이동 속도 (뒤로 물러나는 속도도 동일)
    public float moveSpeed = 3f;

    // 공격 쿨타임 (AI 공격 시 사용)
    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity; // 마지막 공격 시간


    // 적의 현재 AI 상태를 나타내는 enum
    private enum EnemyState { B_Idle, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.B_Idle;
    // ===== AI 파라미터 끝 =====


    private void Start()
    {
        animator = GetComponent<Animator>();
        SetState(EnemyState.B_Idle);

        player = GameObject.Find(playerObjectName);
        if (player == null)
        {
            Debug.LogError("Player GameObject with name '" + playerObjectName + "' not found!");
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        if (player == null)
        {
            if (currentState != EnemyState.B_Idle) SetState(EnemyState.B_Idle);
            return;
        }

        UpdateFacingDirection();

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // ===== AI 상태 전환 로직 =====
        switch (currentState)
        {
            case EnemyState.B_Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    Debug.Log("B_Idle -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    Debug.Log("Chase -> Attack (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Attack);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    Debug.Log("Chase -> B_Idle (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.B_Idle);
                }
                else
                {
                    // Debug.Log("Chase State (Distance: " + distanceToPlayer.ToString("F2") + "), Moving Towards Player.");
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    Debug.Log("Attack -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                else
                {
                    if (distanceToPlayer < attackRange - maintainBuffer)
                    {
                        Debug.Log("Too close (" + distanceToPlayer.ToString("F2") + "), retreating to maintain attack range.");
                        MoveAwayFromPlayer();
                    }
                    else
                    {
                        // Debug.Log("Within attack range (" + distanceToPlayer.ToString("F2") + "), attempting attack.");
                        PerformAttack(); // 공격 로직 실행 (쿨타임 체크 포함)
                    }
                }
                break;

            case EnemyState.Dead:
                break;
        }
        // ===== AI 상태 전환 로직 끝 =====
    }

    // ===== AI 상태 및 동작 관련 함수들 =====

    private void SetState(EnemyState newState)
    {
        Debug.Log("SetState: " + currentState + " -> " + newState);
        if (currentState == newState) return;

        // 이전 상태에서 나갈 때 정리 (필요하다면)

        currentState = newState;

        // 새 상태에 진입할 때 설정
        switch (currentState)
        {
            case EnemyState.B_Idle:
                SetB_IdleStateAnimation();
                isWalking = false;
                animator.SetBool("IsWalking", false);
                break;

            case EnemyState.Chase:
                isWalking = true;
                animator.SetBool("IsWalking", true);
                // 추격 상태 진입 시 다른 애니메이션 트리거 초기화
                animator.ResetTrigger("B_Jump");
                animator.ResetTrigger("B_Attack1");
                animator.ResetTrigger("B_Attack2"); // B_Attack2 트리거 초기화 추가
                animator.ResetTrigger("B_Death");
                break;

            case EnemyState.Attack:
                isWalking = false;
                animator.SetBool("IsWalking", false);
                // 공격 상태 진입 시 다른 애니메이션 트리거 초기화
                animator.ResetTrigger("B_Jump");
                animator.ResetTrigger("B_Attack1"); // Attack 상태 진입 시 Attack1 트리거 초기화
                animator.ResetTrigger("B_Attack2"); // Attack 상태 진입 시 Attack2 트리거 초기화
                animator.ResetTrigger("B_Death");
                // 공격 애니메이션은 PerformAttack() 에서 별도로 트리거
                break;

            case EnemyState.Dead:
                Die();
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || currentState != EnemyState.Chase) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position);
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    private void MoveAwayFromPlayer()
    {
        if (player == null || currentState != EnemyState.Attack) return; // Attack 상태에서 간격 유지를 위해 물러남

        Vector3 directionToPlayer = (transform.position - player.transform.position); // 적 -> 플레이어 방향
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

            transform.position += movement;
        }
    }

    private void PerformAttack()
    {
        if (player == null || currentState != EnemyState.Attack) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("공격 실행 시도 (쿨타임 완료)");
            // ===== AI 공격 선택 로직 추가 =====
            int attackChoice = Random.Range(0, 2); // 0 또는 1 랜덤 선택
            if (attackChoice == 0)
            {
                Debug.Log("AI 공격: B_Attack1 발동");
                PlayB_Attack1Animation();
            }
            else
            {
                Debug.Log("AI 공격: B_Attack2 발동");
                PlayB_Attack2Animation();
            }
            // ===== AI 공격 선택 로직 끝 =====

            lastAttackTime = Time.time;

            // TODO: 실제로 플레이어에게 데미지를 주는 로직 추가 (선택된 공격에 따라 데미지 다를 수 있음)
        }
        // else { Debug.Log("공격 대기 중 (쿨타임)"); }
    }

    private void Die()
    {
        Debug.Log("적 사망 처리");
        isWalking = false;
        isDead = true;
        animator.SetBool("IsWalking", false);
        PlayB_DeathAnimationAnimation();

        // TODO: 필요하다면 오브젝트 비활성화/파괴 등 추가
        // 예: Destroy(gameObject, 3f);
    }

    private void UpdateFacingDirection()
    {
        if (player == null) return;

        float directionX = player.transform.position.x - transform.position.x;
        Vector3 currentScale = transform.localScale;

        if (directionX > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * +Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
        else if (directionX < -0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
    }
    // ===== AI 상태 및 동작 관련 함수 끝 =====


    // ===== 키 입력 처리 함수들 =====

    // 키 1 입력 처리 (걷기 토글)
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            if (currentState == EnemyState.B_Idle || currentState == EnemyState.Chase)
            {
                if (isWalking) SetState(EnemyState.B_Idle);
                else SetState(EnemyState.Chase);
            }
            else if (currentState == EnemyState.Attack) // 공격 상태에서 수동 걷기 토글 시 -> 추격 상태로 전환 (공격 중단)
            {
                SetState(EnemyState.Chase);
            }
        }
    }

    // 키 2 입력 처리 (점프)
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("키 2: 점프 실행");
            PlayB_JumpAnimationAnimation();
            // TODO: 점프 중에는 AI 상태를 잠시 중지하거나 B_Idle 등으로 전환하는 로직 추가
            // SetState(EnemyState.B_Idle); // 예시
        }
    }

    // 키 3 입력 처리 (B_Attack1 - 수동)
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("키 3: B_Attack1 (수동) 실행");
            // AI 상태와 별개로 수동 공격 애니메이션 트리거
            PlayB_Attack1Animation();
            // TODO: 수동 공격 중에는 AI 상태를 잠시 중지하거나 B_Idle 등으로 전환하는 로직 추가
            // SetState(EnemyState.B_Idle); // 예시
        }
    }

    // 키 4 입력 처리 (사망)
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        if (currentState != EnemyState.Dead)
        {
            if (context.performed)
            {
                Debug.Log("키 4: 사망 애니메이션 실행");
                SetState(EnemyState.Dead);
            }
        }
    }

    // 키 5 입력 처리 (강제 B_Idle 상태)
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("키 5: B_Idle 상태 설정");
            SetState(EnemyState.B_Idle);
        }
    }

    // ===== 키 6 입력 처리 (B_Attack2 - 수동) 추가 =====
    protected override void OnTest6(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("키 6: B_Attack2 (수동) 실행");
            // AI 상태와 별개로 수동 공격 애니메이션 트리거
            PlayB_Attack2Animation(); // <-- B_Attack2 재생 함수 호출
            // TODO: 수동 공격 중에는 AI 상태를 잠시 중지하거나 B_Idle 등으로 전환하는 로직 추가
            // SetState(EnemyState.B_Idle); // 예시
        }
    }
    // ===== 키 6 입력 처리 끝 =====


    #region 애니메이션 재생 함수들

    private void SetB_IdleStateAnimation()
    {
        animator.ResetTrigger("B_Jump");
        animator.ResetTrigger("B_Death");
        animator.ResetTrigger("B_Attack1");
        animator.ResetTrigger("B_Attack2"); // B_Attack2 트리거 초기화 추가
    }

    private void PlayB_JumpAnimationAnimation()
    {
        animator.SetTrigger("B_Jump");
    }

    private void PlayB_Attack1Animation()
    {
        animator.SetTrigger("B_Attack1"); // <-- Animator Trigger 파라미터 이름
    }

    // ===== B_Attack2 애니메이션 재생 함수 추가 =====
    private void PlayB_Attack2Animation()
    {
        animator.SetTrigger("B_Attack2"); // <-- Animator Trigger 파라미터 이름
    }
    // ===== B_Attack2 애니메이션 재생 함수 끝 =====


    private void PlayB_DeathAnimationAnimation()
    {
        animator.SetTrigger("B_Death");
    }

    #endregion

#endif // UNITY_EDITOR
}