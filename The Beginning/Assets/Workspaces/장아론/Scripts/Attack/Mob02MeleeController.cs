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

    S_AttackerSoundEvent s_AttackerSoundEvent;

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
   // public new float detectionRange = 3f;
    //[Tooltip("플레이어를 감지했을 때 추적 속도입니다.")]
    public float chaseSpeed = 3f;

    // --- 인스펙터에서 직접 조절할 AnimationCurve 변수 추가 ---
    [Header("공격 애니메이션 커브 설정")]
    [Tooltip("공격 시 전진 이동 속도를 시간에 따라 제어하는 커브입니다. X축은 애니메이션 진행도 (0~1), Y축은 속도 배율.")]
    public AnimationCurve attackForwardMovementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1), new Keyframe(0.5f, 0)); // 기본값 설정
    [Tooltip("공격 후 후진 이동 속도를 시간에 따라 제어하는 커브입니다. X축은 애니메이션 진행도 (0~1), Y축은 속도 배율.")]
    public AnimationCurve attackBackwardMovementCurve = new AnimationCurve(new Keyframe(0.5f, 0), new Keyframe(0.7f, 1), new Keyframe(1, 0)); // 기본값 설정

    [Tooltip("공격 애니메이션 중 최대 전진 속도 배율입니다.")]
    public float attackForwardSpeedMultiplier = 1.5f;
    [Tooltip("공격 애니메이션 종료 후 후진 속도 배율입니다. (음수 값으로 사용될 수 있음)")]
    public float attackBackwardSpeedMultiplier = 1f;

    [Tooltip("공격 시 플레이어에게 추가적으로 전진할 거리입니다. 이 거리를 고려하여 공격 시작점을 결정합니다.")]
    public float attackDashDistance = 1.5f; // 새로 추가: 공격 시 돌진할 거리

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
        base.Awake();

        s_AttackerSoundEvent = GetComponent<S_AttackerSoundEvent>();
        MaxHp = mob02MaxHealth;
        CurrentHp = MaxHp;

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

        Flip(true);
        Debug.Log($"Mob02 Start: Initial Flip to Left applied. Current localScale.x: {transform.Find("Sprite")?.localScale.x ?? transform.localScale.x}");
    }

    /// <summary>
    /// 매 프레임 호출됩니다.
    /// 적의 현재 상태에 따라 AI 로직과 움직임을 제어합니다.
    /// </summary>
    protected override void Update()
    {

        //Debug.Log($"{gameObject.name} : Player Vector : {playerTransform.position} ----------");
        if (IsDead || isStunned || isPerformingHurtAnimation)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            PlayIdleAnim();
            return;
        }

        if (isPerformingAttackAnimation)
        {
            if (currentAttackAnimationLength == 0f && animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                // Animator Controller의 State 이름이 "AttackA"이므로, 이 부분은 그대로 둡니다.
                if (stateInfo.IsName("AttackA")) // <--- 이 부분은 "AttackA"가 맞습니다.
                {
                    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        // 이 "Mob02_AttackA_ClipName"을 실제 애니메이션 클립 이름인 "S_Attack"으로 변경해야 합니다.
                        if (clipInfo[0].clip.name == "S_Attack") // <--- 이 부분을 "S_Attack"으로 변경!!!
                        {
                            currentAttackAnimationLength = clipInfo[0].clip.length;
                        }
                    }
                }
            }

            if (currentAttackAnimationLength > 0)
            {
                attackAnimationProgress += Time.deltaTime / currentAttackAnimationLength;
                attackAnimationProgress = Mathf.Clamp01(attackAnimationProgress);
                ApplyAttackAnimationMovement();
            }
            return;
        }
        else
        {
            attackAnimationProgress = 0f;
            currentAttackAnimationLength = 0f;
        }


        if (isWaitingAfterAttack)
        {
            PlayIdleAnim();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        CheckPlayerDetection();

        if (isPlayerDetected)
        {
            ChasePlayer();
            PerformAttackLogic();
        }
        else
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

    private void CheckPlayerDetection()
    {
        if (playerTransform == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        isPlayerDetected = distanceToPlayer <= detectionRange;
        if (isPlayerDetected)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }
    }

    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 공격 범위 + 돌진 거리보다 가까우면 멈추고 공격 준비
        if (distanceToPlayer <= attackRange + attackStopBuffer + attackDashDistance)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            PlayIdleAnim();
            return;
        }

        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        Flip(direction.x < 0);
        Vector2 velocity = direction * chaseSpeed;
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        }
        PlayWalkAnim();
    }

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

    public void Stun()
    {
        if (IsDead || isStunned) return;
        isStunned = true;
        PlayStunAnim();
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!IsDead)
        {
            isStunned = false;
        }
    }

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
            s_AttackerSoundEvent.S_AttackerDeathSound();
            Debug.Log("deathsound 출력 완료!");
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
            isAttackingForward = true;
            isAttackingBackward = false;
            attackAnimationProgress = 0f;
            currentAttackAnimationLength = 0f;
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

    private void ApplyAttackAnimationMovement()
    {
        if (rb == null) return;

        float curveSpeedFactor = 0f;
        float actualMoveSpeed = 0f;
        float directionX = Mathf.Sign(transform.Find("Sprite")?.localScale.x ?? transform.localScale.x);

        if (isAttackingForward)
        {
            curveSpeedFactor = attackForwardMovementCurve.Evaluate(attackAnimationProgress);
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackForwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(directionX * actualMoveSpeed, rb.linearVelocity.y);
            // Debug.Log($"Attack Forward Movement: Speed={actualMoveSpeed}, Progress={attackAnimationProgress}");
        }
        else if (isAttackingBackward)
        {
            curveSpeedFactor = attackBackwardMovementCurve.Evaluate(attackAnimationProgress);
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackBackwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(-directionX * actualMoveSpeed, rb.linearVelocity.y);
            // Debug.Log($"Attack Backward Movement: Speed={actualMoveSpeed}, Progress={attackAnimationProgress}");
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            PlayIdleAnim();
        }
    }


    // --- AI 공격 로직 ---

    protected override void PerformAttackLogic()
    {
        bool cooldownReady = Time.time >= nextAttackTime;
        if (!cooldownReady) return;

        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 플레이어가 공격 시작 거리 내에 있을 때 공격 시작
        // attackRange + attackDashDistance + (추가 버퍼)
        if (distanceToPlayer <= attackRange + attackDashDistance + attackStopBuffer) // attackStopBuffer도 고려
        {
            // 공격 애니메이션 시작
            isPerformingAttackAnimation = true;
            attackAnimationProgress = 0f;
            currentAttackAnimationLength = 0f; // 애니메이션 길이를 다시 가져오도록 초기화
            PlayAttack1Anim();
        }
    }

    // --- 애니메이션 이벤트 콜백 함수 ---

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        nextAttackTime = Time.time + attackACooldown;

        isAttackingForward = false;
        isAttackingBackward = false;
        isPerformingAttackAnimation = false;
        attackAnimationProgress = 0f;
        currentAttackAnimationLength = 0f;
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        Debug.Log("OnAttackAnimationEnd: Attack animation finished.");
    }

    public void OnAttackForwardStart()
    {
        isAttackingForward = true;
        isAttackingBackward = false;
        Debug.Log("Mob02: OnAttackForwardStart called.");
    }

    public void OnAttackBackwardStart()
    {
        isAttackingForward = false;
        isAttackingBackward = true;
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
                attackAEnemyHitbox.ResetHitPlayers();
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
    }

    public override void SetPlayerTarget(Transform newPlayerTransform)
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 새로운 공격 시작점 시각화 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange + attackDashDistance + attackStopBuffer);
    }

    protected override void MoveTowardsPlayer() { }

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