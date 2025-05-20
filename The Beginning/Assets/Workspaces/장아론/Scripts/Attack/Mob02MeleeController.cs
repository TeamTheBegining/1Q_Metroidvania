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

    // --- 애니메이션 커브 이름 상수 정의 (추가) ---
    private const string ANIM_CURVE_ATTACK_FORWARD_SPEED = "AttackForwardSpeed"; // 공격 시 전진 속도 제어용 커브 이름

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

    // 경직(Stun) 상태 관리 변수
    private bool isStunned = false;
    [Header("잡몹02 경직 설정")]
    [Tooltip("플레이어의 패링 공격 등에 의해 경직되었을 때 지속되는 시간입니다.")]
    public float stunDuration = 2f;

    private Coroutine stunCoroutine;

    // --- 추가된 변수: SpriteRenderer 참조 ---
    private SpriteRenderer spriteRenderer; // 캐릭터의 스프라이트 렌더러 컴포넌트

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

        // --- SpriteRenderer 컴포넌트 참조 초기화 (추가) ---
        // 이 스크립트가 붙은 게임 오브젝트에서 SpriteRenderer 컴포넌트를 찾습니다.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // 만약 SpriteRenderer가 현재 오브젝트에 없고 자식 오브젝트에 있다면
            // 자식 오브젝트 중 "Sprite"라는 이름의 오브젝트에서 찾아봅니다.
            Transform spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Mob02MeleeController: SpriteRenderer 컴포넌트를 찾을 수 없습니다. 스프라이트 뒤집기(Flip)가 정상적으로 작동하지 않을 수 있습니다.", this);
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
        nextAttackTime = Time.time;
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
            return; // 현재 프레임의 모든 이후 로직을 건너뜁니다.
        }

        // 2. 공격 애니메이션이 활성화된 상태:
        //    이때는 애니메이션 커브를 이용한 공격 중 이동(`ApplyAttackAnimationMovement()`)만 수행하고,
        //    다른 일반 AI 로직(추적 등)은 잠시 중단됩니다.
        if (isPerformingAttackAnimation)
        {
            ApplyAttackAnimationMovement(); // 공격 애니메이션에 따른 이동 수행
            return; // 현재 프레임의 모든 이후 로직을 건너뜁니다.
        }

        // 3. 공격 후 쿨다운 대기 상태:
        //    공격 애니메이션이 끝난 후 다음 공격까지 쿨다운 시간 동안 대기 상태를 유지합니다.
        //    이때도 일반 AI 로직(추적 등)은 중단됩니다.
        if (isWaitingAfterAttack)
        {
            PlayIdleAnim(); // 대기 애니메이션 재생
            if (rb != null) rb.linearVelocity = Vector2.zero; // 제자리에 멈춰있도록 함
            return; // 현재 프레임의 모든 이후 로직을 건너뜁니다.
        }

        // 4. 위의 어떤 특별한 상태도 아닐 때:
        //    기본적인 AI 로직(플레이어 추적, 순찰 등)과 일반적인 이동이 이루어집니다.
        //    CommonEnemyController의 Update() 메서드가 호출되어 이 역할을 합니다.
        base.Update();
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
    /// 공격 애니메이션 중 애니메이션 커브 값을 읽어 전진 이동을 적용합니다.
    /// 이 메서드는 'Mob02_AttackA' 애니메이션 클립에 'AttackForwardSpeed'라는 이름의 커브가 존재한다고 가정합니다.
    /// 이 커브는 공격 시 절정 부분에서 빠르게 이동했다가 감속하는 속도 변화를 제어합니다.
    /// </summary>
    private void ApplyAttackAnimationMovement()
    {
        if (animator == null || rb == null) return;

        float curveSpeedFactor = animator.GetFloat(ANIM_CURVE_ATTACK_FORWARD_SPEED);

        // SpriteRenderer의 flipX를 사용하여 캐릭터의 현재 바라보는 방향을 감지합니다.
        // spriteRenderer가 초기화되었는지 확인합니다.
        Vector2 currentFacingDirection = Vector2.zero; // 기본값
        if (spriteRenderer != null)
        {
            // spriteRenderer.flipX가 true면 스프라이트가 왼쪽을 바라봄 (Vector2.left)
            // false면 스프라이트가 오른쪽을 바라봄 (Vector2.right)
            currentFacingDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        else // spriteRenderer를 찾지 못했다면, Rigidbody의 현재 속도 방향을 임시로 사용
        {
            // 이 fallback은 SpriteRenderer가 없는 경우에만 사용되며,
            // Flip() 함수가 Transform.localScale을 직접 조절했다면 이 로직을 수정해야 할 수 있습니다.
            if (rb != null)
            {
                if (rb.linearVelocity.x < 0) currentFacingDirection = Vector2.left;
                else if (rb.linearVelocity.x > 0) currentFacingDirection = Vector2.right;
                else currentFacingDirection = Vector2.right; // 기본적으로 오른쪽
            }
            else
            {
                currentFacingDirection = Vector2.right; // Rigidbody도 없으면 기본 오른쪽
            }
        }

        float actualMoveSpeed = moveSpeed * curveSpeedFactor;

        // Rigidbody에 계산된 속도를 적용합니다. y축은 기존 속도를 유지하여 중력 등 영향을 받도록 합니다.
        rb.linearVelocity = new Vector2(currentFacingDirection.x * actualMoveSpeed, rb.linearVelocity.y);
    }

    // --- AI 공격 로직 ---

    protected override void PerformAttackLogic()
    {
        bool cooldownReady = Time.time >= nextAttackTime;

        if (playerTransform == null || Vector2.Distance(transform.position, playerTransform.position) > attackRange)
        {
            return;
        }

        if (!cooldownReady)
        {
            return;
        }

        isPerformingAttackAnimation = true;
        PlayAttack1Anim();
    }

    // --- 애니메이션 이벤트 콜백 함수 ---

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        // 공격 애니메이션이 끝난 후 다음 공격까지의 쿨다운 시간을 설정합니다.
        nextAttackTime = Time.time + attackACooldown;
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
        if (spriteRenderer == null)
        {
            Debug.LogWarning(gameObject.name + ": SpriteRenderer 컴포넌트가 없습니다. 스프라이트 뒤집기를 수행할 수 없습니다.", this);
            return;
        }

        // faceLeft가 true이면 flipX를 true로 (왼쪽을 바라봄), 아니면 false로 (오른쪽을 바라봄)
        spriteRenderer.flipX = faceLeft;
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