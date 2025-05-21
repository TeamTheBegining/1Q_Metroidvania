using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// 잡몹02 (근거리) 캐릭터를 제어하는 스크립트입니다.
/// 이 스크립트는 CommonEnemyController를 상속받아 기본적인 적의 행동(피해, 사망, 추적, 순찰)을 계승하며,
/// 잡몹02의 고유한 능력치, 단일 공격 패턴, 경직(패링 시) 로직을 구현합니다.
/// </summary>
public class Mob02MeleeController : CommonEnemyController
{
    // --- 애니메이션 파라미터 이름 상수 정의 ---
    private const string ANIM_BOOL_MOB02_WALK = "Mob02_Walk";
    private const string ANIM_TRIGGER_MOB02_ATTACK_A = "Mob02_AttackA";
    private const string ANIM_TRIGGER_MOB02_HURT = "Mob02_Hurt";
    private const string ANIM_TRIGGER_MOB02_STUN = "Mob02_Stun";
    private const string ANIM_TRIGGER_MOB02_DEATH = "Mob02_Death";

    // --- 애니메이션 커브 이름 상수는 더 이상 Animator Layer에서 직접 읽지 않으므로 필요 없음 ---
    // private const string ANIM_CURVE_ATTACK_FORWARD_SPEED = "AttackForwardSpeed";
    // private const string ANIM_CURVE_ATTACK_BACKWARD_SPEED = "AttackBackwardSpeed";

    [Header("잡몹02 공격 판정 설정")]
    [Tooltip("A 공격(내지르기) 시 활성화될 히트박스 게임 오브젝트를 연결하세요.")]
    public GameObject attackAHitboxObject;

    private BoxCollider2D attackAHitboxCollider;
    private EnemyHitbox attackAEnemyHitbox;

    [Header("잡몹02 고유 능력치")]
    [Tooltip("잡몹02의 최대 체력입니다. CommonEnemyController의 MaxHp를 이 값으로 초기화합니다.")]
    [SerializeField] private float mob02MaxHealth = 4f;

    [Header("잡몹02 전투 설정")]
    [Tooltip("'내지르기' 공격 시 적용될 피해량입니다.")]
    public float attackAValue = 0.4f;
    [Tooltip("공격 애니메이션 종료 후 다음 공격을 시도하기 전까지 기다릴 시간 (1초).")]
    public float attackACooldown = 1.0f;

    [Header("잡몹02 감지 및 이동 설정")]
    [Tooltip("플레이어를 감지하는 범위입니다.")]
    public new float detectionRange = 7f;
    [Tooltip("플레이어를 감지했을 때 추적 속도입니다.")]
    public float chaseSpeed = 3f;

    // --- 인스펙터에서 직접 조절할 AnimationCurve 변수 추가 ---
    [Header("공격 애니메이션 커브 설정")]
    [Tooltip("공격 시 전진 이동 속도를 시간에 따라 제어하는 커브입니다. X축은 애니메이션 진행도 (0~1), Y축은 속도 배율.")]
    public AnimationCurve attackForwardMovementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1), new Keyframe(0.5f, 0));
    [Tooltip("공격 후 후진 이동 속도를 시간에 따라 제어하는 커브입니다. X축은 애니메이션 진행도 (0~1), Y축은 속도 배율.")]
    public AnimationCurve attackBackwardMovementCurve = new AnimationCurve(new Keyframe(0.5f, 0), new Keyframe(0.7f, 1), new Keyframe(1, 0));


    [Tooltip("공격 애니메이션 중 최대 전진 속도 배율입니다.")]
    public float attackForwardSpeedMultiplier = 1.5f;
    [Tooltip("공격 애니메이션 종료 후 후진 속도 배율입니다. (음수 값으로 사용될 수 있음)")]
    public float attackBackwardSpeedMultiplier = 1f;

    // 경직(Stun) 상태 관리 변수
    private bool isStunned = false;
    [Header("잡몹02 경직 설정")]
    [Tooltip("플레이어의 패링 공격 등에 의해 경직되었을 때 지속되는 시간입니다.")]
    public float stunDuration = 2f;

    private Coroutine stunCoroutine;

    // --- 추가된 변수: SpriteRenderer 참조 ---
    private SpriteRenderer spriteRenderer;

    // --- 추가된 변수: 플레이어 감지 및 추적 관련 ---
    private bool isPlayerDetected = false;
    private Vector2 lastKnownPlayerPosition;

    // --- 공격 시 전진/후진 로직을 위한 추가 변수 ---
    private bool isAttackingForward = false;
    private bool isAttackingBackward = false;

    // --- 추가된 변수: 애니메이션 진행도 추적 ---
    private float attackAnimationProgress = 0f;
    private float currentAttackAnimationLength = 0f; // 현재 공격 애니메이션의 길이 (클립 정보 필요)


    /// <summary>
    /// 오브젝트가 생성될 때 가장 먼저 호출됩니다.
    /// CommonEnemyController의 Awake를 호출하고, 잡몹02의 초기 능력치를 설정하며,
    /// SpriteRenderer 컴포넌트를 초기화합니다.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // 부모 클래스인 CommonEnemyController의 Awake 메서드 호출

        // 잡몹02의 특정 능력치로 최대 체력과 현재 체력을 초기화합니다.
        MaxHp = mob02MaxHealth;
        CurrentHp = MaxHp;

        // SpriteRenderer 컴포넌트 참조 확인 및 초기화
        Transform spriteTransform = transform.Find("Sprite");
        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Mob02MeleeController: SpriteRenderer 컴포넌트를 찾을 수 없습니다. 스프라이트 뒤집기(Flip)가 정상적으로 작동하지 않을 수 있습니다.", this);
        }
    }

    /// <summary>
    /// 스크립트가 활성화될 때 한 번 호출됩니다.
    /// 플레이어 타겟을 설정하고, 히트박스 관련 컴포넌트를 초기화합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            SetPlayerTarget(playerGameObject.transform);
            lastKnownPlayerPosition = playerGameObject.transform.position;
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Player' 태그를 가진 게임 오브젝트를 찾을 수 없습니다. 플레이어가 씬에 있는지, 태그가 올바른지 확인하세요.", this);
        }

        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>();
            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false;
            }
            else
            {
                Debug.LogWarning("BoxCollider2D 컴포넌트가 'Attack A Hitbox Object'에 없습니다.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox 컴포넌트가 'Attack A Hitbox Object'에 없습니다!", this);
            }
        }
        else
        {
            Debug.LogWarning("'Attack A Hitbox Object'가 인스펙터에 할당되지 않았습니다.", this);
        }

        isStunned = false;
        isPlayerDetected = false;
        nextAttackTime = Time.time;

        // --- 처음 시작할 때 왼쪽을 보도록 추가 (Flip 메서드 호출) ---
        // 원래 몹이 스폰되는 방향이 오른쪽이라면, 왼쪽으로 뒤집습니다.
        Flip(true); // 처음 시작할 때 왼쪽을 보도록
        Debug.Log($"Mob02 Start: Initial Flip to Left applied. Current localScale.x: {transform.Find("Sprite")?.localScale.x ?? transform.localScale.x}");
    }

    /// <summary>
    /// 매 프레임 호출됩니다.
    /// 적의 현재 상태에 따라 AI 로직과 움직임을 제어합니다.
    /// </summary>
    protected override void Update()
    {
        // 1. 치명적인 상태 체크 (사망, 경직, 피격 애니메이션 중):
        //    이 상태에서는 모든 움직임과 AI 로직을 중지하고 Update() 함수를 바로 종료합니다.
        if (IsDead || isStunned || isPerformingHurtAnimation)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero; // 물리적인 움직임 정지
            PlayIdleAnim(); // 멈춰있을 때 아이들 애니메이션 재생
            return; // 현재 프레임의 모든 이후 로직을 건너뜁니다.
        }

        // 2. 공격 애니메이션이 활성화된 상태 (전진/후진 모두 포함):
        if (isPerformingAttackAnimation)
        {
            // 현재 재생 중인 애니메이션 클립의 길이를 가져옵니다. (최초 1회만 수행)
            if (currentAttackAnimationLength == 0f && animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Mob02_AttackA")) // Animator State의 이름으로 체크
                {
                    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        // 클립의 이름을 명시적으로 확인하여 해당 클립의 길이를 가져옵니다.
                        // 이 부분은 애니메이션 클립의 실제 이름을 사용해야 합니다.
                        // 예: animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Mob02_Attack_Animation_Clip"
                        currentAttackAnimationLength = clipInfo[0].clip.length;
                    }
                }
            }

            if (currentAttackAnimationLength > 0)
            {
                attackAnimationProgress += Time.deltaTime / currentAttackAnimationLength;
                attackAnimationProgress = Mathf.Clamp01(attackAnimationProgress); // 0과 1 사이로 클램프
                ApplyAttackAnimationMovement(); // 공격 애니메이션에 따른 이동 수행 (전진 또는 후진)
            }
            return; // 현재 프레임의 모든 이후 로직을 건너뜁니다.
        }
        else
        {
            // 공격 애니메이션이 아닐 때는 진행도와 길이를 초기화
            attackAnimationProgress = 0f;
            currentAttackAnimationLength = 0f;
        }

        // 3. 공격 후 쿨다운 대기 상태:
        //    공격 애니메이션이 끝난 후 다음 공격까지 쿨다운 시간 동안 대기 상태를 유지합니다.
        if (isWaitingAfterAttack)
        {
            PlayIdleAnim(); // 대기 애니메이션 재생
            if (rb != null) rb.linearVelocity = Vector2.zero; // 제자리에 멈춰있도록 함
            return; // 현재 프레임의 모든 이후 로직을 건너뜠니다.
        }

        // 4. 플레이어 감지 및 추적/순찰 로직
        CheckPlayerDetection(); // <-- 여기서 isPlayerDetected 상태를 업데이트합니다.

        if (isPlayerDetected)
        {
            ChasePlayer();
            PerformAttackLogic(); // 공격 로직은 ChasePlayer()와 독립적으로 호출
        }
        else // 플레이어가 감지되지 않았을 때
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
                    if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                }
            }
        }
    }

    /// <summary>
    /// 플레이어 감지 여부를 확인하는 함수 (추가됨)
    /// </summary>
    private void CheckPlayerDetection()
    {
        if (playerTransform == null) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 감지 범위 내에 플레이어가 있는지 확인
        isPlayerDetected = distanceToPlayer <= detectionRange;

        // 플레이어가 감지되면 마지막 위치 업데이트 (이것은 현재 코드에서는 사용되지 않지만 유용할 수 있습니다)
        if (isPlayerDetected)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }
    }

    /// <summary>
    /// 플레이어를 추적하는 함수 (추가됨)
    /// </summary>
    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 공격 범위 안에 있거나 공격 범위 바로 바깥 (정지 버퍼)에 있으면 멈춤
        // CommonEnemyController의 attackStopBuffer는 그대로 사용
        if (distanceToPlayer <= attackRange + attackStopBuffer)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            PlayIdleAnim(); // 멈춰있을 때 아이들 애니메이션 재생
            return; // 공격 범위 내에서는 더 이상 추적하지 않음
        }

        // 플레이어 방향으로 이동
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        // 방향에 따른 스프라이트 뒤집기
        Flip(direction.x < 0);

        // 이동 속도 설정 (추적 모드에서는 chaseSpeed 사용)
        Vector2 velocity = direction * chaseSpeed;

        // y축 속도는 기존 속도 유지 (중력 등 영향 받도록)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        }

        // 걷기 애니메이션 재생
        PlayWalkAnim();
    }

    /// <summary>
    /// 적이 피해를 입었을 때 호출됩니다.
    /// </summary>
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        if (isStunned)
        {
            base.TakeDamage(damage, attackObject);
            return;
        }

        base.TakeDamage(damage, attackObject);

        if (CurrentHp > 0)
        {
            PlayHurtAnim();
        }
    }

    /// <summary>
    /// 적을 경직 상태로 만듭니다. (예: 플레이어의 패링 공격 성공 시 호출)
    /// </summary>
    public void Stun()
    {
        if (IsDead || isStunned) return;

        isStunned = true;
        PlayStunAnim();

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    /// <summary>
    /// 지정된 시간 후에 경직 상태를 해제하는 코루틴입니다.
    /// </summary>
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            isStunned = false;
        }
    }

    // --- 애니메이션 관련 함수 ---

    protected override void PlayIdleAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, true);
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_MOB02_DEATH);
    }

    protected override void PlayHurtAnim()
    {
        if (!IsDead && !isStunned && animator != null)
        {
            isPerformingHurtAnimation = true;
            animator.SetTrigger(ANIM_TRIGGER_MOB02_HURT);
        }
    }

    protected void PlayStunAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_STUN);
        }
    }

    protected override void PlayAttack1Anim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_ATTACK_A);
            // 공격 시작 시 전진 단계로 설정하고 진행도 초기화
            isAttackingForward = true;
            isAttackingBackward = false;
            attackAnimationProgress = 0f; // 애니메이션 진행도 초기화
            currentAttackAnimationLength = 0f; // 애니메이션 길이를 다시 가져오도록 초기화
        }
    }

    protected override void PlayAttack2Anim() { }
    protected override void PlayAttack3Anim() { }

    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_HURT);
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_STUN);
        }
    }

    /// <summary>
    /// 공격 애니메이션 중 애니메이션 커브 값을 읽어 전진 또는 후진 이동을 적용합니다.
    /// 이 메서드는 'Mob02MeleeController' 스크립트 내의 'attackForwardMovementCurve'와 'attackBackwardMovementCurve'를 사용합니다.
    /// </summary>
    private void ApplyAttackAnimationMovement()
    {
        if (rb == null) return;

        float curveSpeedFactor = 0f;
        float actualMoveSpeed = 0f;
        float directionX = Mathf.Sign(transform.Find("Sprite")?.localScale.x ?? transform.localScale.x); // 현재 바라보는 방향

        // attackAnimationProgress를 사용하여 AnimationCurve에서 값을 샘플링합니다.
        if (isAttackingForward) // 공격 전진 단계
        {
            curveSpeedFactor = attackForwardMovementCurve.Evaluate(attackAnimationProgress);
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackForwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(directionX * actualMoveSpeed, rb.linearVelocity.y);
            // Debug.Log($"Attack Forward Movement: Direction={directionX}, Speed={actualMoveSpeed}, Curve={curveSpeedFactor}, Progress={attackAnimationProgress}");
        }
        else if (isAttackingBackward) // 공격 후진 단계
        {
            curveSpeedFactor = attackBackwardMovementCurve.Evaluate(attackAnimationProgress);
            // 후진이므로 방향을 반대로 곱합니다.
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackBackwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(-directionX * actualMoveSpeed, rb.linearVelocity.y); // 방향 반전
            // Debug.Log($"Attack Backward Movement: Direction={-directionX}, Speed={actualMoveSpeed}, Curve={curveSpeedFactor}, Progress={attackAnimationProgress}");
        }
        else
        {
            // 공격 애니메이션은 활성화되어 있지만, 전진/후진 단계가 명확하지 않은 경우 (예: 공격 대기 프레임)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            PlayIdleAnim(); // 임시로 아이들 애니메이션 재생
        }
    }


    // --- AI 공격 로직 ---

    protected override void PerformAttackLogic()
    {
        // 쿨다운 체크
        bool cooldownReady = Time.time >= nextAttackTime;
        if (!cooldownReady) return;

        // 플레이어가 있고 공격 범위 내에 있는지 확인
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 공격 범위 내에 들어왔을 때 공격 시작
        if (distanceToPlayer <= attackRange)
        {
            // 공격 애니메이션 시작
            isPerformingAttackAnimation = true; // 공격 애니메이션 중임을 알림
            attackAnimationProgress = 0f; // 공격 시작 시 진행도 초기화
            currentAttackAnimationLength = 0f; // 애니메이션 길이를 다시 가져오도록 초기화
            PlayAttack1Anim();
            // 공격 후에는 isWaitingAfterAttack이 OnAttackAnimationEnd에서 true로 설정됩니다.
        }
    }

    // --- 애니메이션 이벤트 콜백 함수 ---

    // 이 함수들은 애니메이션 클립의 이벤트로 연결되어야 합니다.
    // Mob02_AttackA 애니메이션 클립 내에서 적절한 프레임에 다음 이벤트들을 추가해야 합니다.

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        // 공격 애니메이션이 완전히 끝난 후 다음 공격까지의 쿨다운 시간을 설정합니다.
        nextAttackTime = Time.time + attackACooldown;

        // 공격 애니메이션이 끝났으므로 전진/후진 플래그를 리셋하고 이동 정지합니다.
        isAttackingForward = false;
        isAttackingBackward = false;
        isPerformingAttackAnimation = false; // 공격 애니메이션이 완전히 끝났음을 알림
        attackAnimationProgress = 0f; // 애니메이션 진행도 초기화
        currentAttackAnimationLength = 0f; // 애니메이션 길이도 초기화
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 공격 종료 후 바로 멈춤

        Debug.Log("OnAttackAnimationEnd: Attack animation finished.");
    }

    // 새롭게 추가: 공격 애니메이션 중 전진 구간 시작 시 호출
    public void OnAttackForwardStart()
    {
        isAttackingForward = true;
        isAttackingBackward = false;
        // attackAnimationProgress는 Update에서 계속 증가시키므로 여기서 초기화할 필요 없음
        Debug.Log("Mob02: OnAttackForwardStart called.");
    }

    // 새롭게 추가: 공격 애니메이션 중 후진 구간 시작 시 호출 (또는 전진 종료 후 후진 시작)
    public void OnAttackBackwardStart()
    {
        isAttackingForward = false;
        isAttackingBackward = true;
        // attackAnimationProgress는 Update에서 계속 증가시키므로 여기서 초기화할 필요 없음
        Debug.Log("Mob02: OnAttackBackwardStart called.");
    }

    public override void OnHurtAnimationEnd()
    {
        base.OnHurtAnimationEnd();
    }

    public void EnableAttackHitbox()
    {
        if (IsDead) return;

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue;
            }
            else
            {
                Debug.LogWarning("Mob02MeleeController: EnemyHitbox 컴포넌트가 'Attack A Hitbox Object'에 없습니다!", attackAHitboxObject);
            }
            attackAHitboxCollider.enabled = true;
            Debug.Log("Mob02: Attack Hitbox Enabled.");
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object' 또는 콜라이더가 할당되지 않았거나 찾을 수 없습니다. 히트박스 활성화 실패.", this);
        }
    }

    public void DisableAttackHitbox()
    {
        if (IsDead) return;

        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false;
            Debug.Log("Mob02: Attack Hitbox Disabled.");
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Collider'가 할당되지 않았거나 찾을 수 없습니다. 히트박스 비활성화 실패.", this);
        }
    }

    /// <summary>
    /// 적의 시각적인 방향(스프라이트)을 전환합니다.
    /// SpriteRenderer의 `flipX` 속성을 사용합니다.
    /// </summary>
    /// <param name="faceLeft">왼쪽을 바라봐야 하면 true, 오른쪽을 바라봐야 하면 false.</param>
    protected override void Flip(bool faceLeft)
    {
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
        }

        float desiredSign = faceLeft ? -1f : 1f;
        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);

        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);

        // SpriteRenderer 기반 방식은 제거하고 Transform.localScale 방식만 사용
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

    // --- 기즈모를 이용한 디버그 시각화 ---
    private void OnDrawGizmosSelected()
    {
        // 감지 범위 시각화 (초록색)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위 시각화 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // --- CommonEnemyController의 이동 관련 추상 메서드 또는 오버라이드 가능한 메서드 구현 ---
    protected override void MoveTowardsPlayer()
    {
        // Mob02MeleeController에서 ChasePlayer()를 직접 사용하므로 이 함수는 사용되지 않습니다.
    }

    protected override void MoveTowardsOrigin()
    {
        float distanceToOrigin = Vector2.Distance(transform.position, originalPosition);

        if (distanceToOrigin <= 0.1f)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
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
        if (rb != null) rb.linearVelocity = new Vector2(directionToOrigin.x * moveSpeed, rb.linearVelocity.y);
    }

    protected override void MovePatrol()
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
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        PlayWalkAnim();
        if (rb != null) rb.linearVelocity = new Vector2(_currentPatrolDirection * moveSpeed, rb.linearVelocity.y);
    }
}