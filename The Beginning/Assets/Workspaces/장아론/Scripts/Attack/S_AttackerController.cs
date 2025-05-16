using System;
using UnityEngine;
using System.Collections;

// S_Attacker (dash attacking enemy) character controller
// Inherits from CommonEnemyController.
public class S_AttackerController : CommonEnemyController
{
    // S_Attacker specific animation parameter names
    private const string ANIM_BOOL_S_WALK = "S_Walk";
    private const string ANIM_TRIGGER_S_ATTACK_A = "S_Attack"; // S Attack (dash)
    private const string ANIM_TRIGGER_S_HURT = "S_Hurt"; // Hit animation
    private const string ANIM_TRIGGER_S_STUN = "S_Stun"; // Stun animation
    private const string ANIM_TRIGGER_S_DEATH = "S_Death"; // Death animation

    [Header("S_Attacker Hitbox")]
    public GameObject attackSHitboxObject; // S attack hitbox object

    // Hitbox component references
    private BoxCollider2D attackSHitboxCollider;
    private EnemyHitbox attackSEnemyHitbox; // Assumes EnemyHitbox script exists

    [Header("S_Attacker Stats")]
    // Health and other stats are managed by the Base class (maxHealth, currentHealth)

    [Header("S_Attacker Combat")]
    public float attackSValue = 0.5f; // S Attack damage
    public float attackSCooldown = 3f; // S Attack cooldown

    private float nextAttackTime = 0f; // Time when next attack is possible

    // Stun state flag (managed in derived class as it's specific to S_Attacker)
    private bool isStunned = false;
    [Header("S_Attacker Stun")]
    public float stunDuration = 2f; // Stun duration

    private Coroutine stunCoroutine; // To hold reference to the running stun coroutine

    //  대시 공격 전용 설정 
    [Header("S_Attacker Dash Attack")]
    // 애니메이션 커브: 대시 공격 중 속도 변화를 제어합니다.
    // X축은 정규화된 시간(0~1), Y축은 속도 배율(0~1 이상)입니다.
    public AnimationCurve dashSpeedCurve;
    public float dashBaseSpeed = 8f; // 대시 공격의 기본 속도 (커브 배율과 곱해짐)
    // 실제 애니메이션 클립 길이와 일치하도록 인스펙터에서 설정하는 것이 좋지만,
    // 코루틴 내부에서 애니메이터 클립 길이를 가져와 사용할 것입니다.
    // public float dashAttackDuration = 0.5f;

    private Coroutine dashAttackCoroutine; // 실행 중인 대시 코루틴 참조

    // 플레이어 통과를 위한 적의 메인 콜라이더
    private Collider2D mainCollider; // 적 본체의 메인 콜라이더 (예: CapsuleCollider2D)

    // CommonEnemyController에서 상속받은 attackRange를 S_Attacker에 맞춰 1.5로 설정합니다.
    //  여기에서 public new float attackRange = 1.5f; 줄을 삭제했습니다. 
    // 이제 CommonEnemyController의 attackRange를 상속받습니다.


    // TakeDamage 오버라이드 (Base 클래스에서 처리되므로 필요한 부분만 남김)
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (isDead) return;

        // 스턴 중에는 데미지만 입고 피격 애니메이션은 스킵
        if (isStunned)
        {
            base.TakeDamage(damage, attackObject); // Base 클래스에서 체력 감소, 사망 처리
            return;
        }

        base.TakeDamage(damage, attackObject); // Base 클래스에서 체력 감소, 사망 처리

        if (currentHealth > 0) // 아직 살아있으면 피격 애니메이션 재생
        {
            PlayHurtAnim();
        }
    }

    // 스턴 상태 처리
    public void Stun() // 외부에서 호출 가능 (예: 플레이어 패리)
    {
        if (isDead || isStunned) return;

        isStunned = true;
        PlayStunAnim(); // 스턴 애니메이션 재생

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    // 스턴 해제 코루틴
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!isDead)
        {
            isStunned = false;
            SetState(EnemyState.Chase); // 스턴 해제 후 추적 상태로 복귀
        }
    }

    // Start 오버라이드
    protected override void Start()
    {
        base.Start(); // Base CommonEnemyController Start 호출

        // CommonEnemyController에서 상속받은 attackRange 값을 S_Attacker에 맞춰 설정합니다.
        // 인스펙터에서 직접 설정하는 것을 권장하지만, 코드로 강제하려면 여기에 추가하세요.
        // this.attackRange = 1.5f; 

        // 히트박스 오브젝트 및 컴포넌트 참조 초기화
        if (attackSHitboxObject != null)
        {
            attackSHitboxCollider = attackSHitboxObject.GetComponent<BoxCollider2D>();
            attackSEnemyHitbox = attackSHitboxObject.GetComponent<EnemyHitbox>();
            if (attackSHitboxCollider != null)
            {
                attackSHitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
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

        // 적의 메인 콜라이더 참조 가져오기
        mainCollider = GetComponent<Collider2D>();
        if (mainCollider == null) Debug.LogError("Main Collider2D component not found on S_Attacker!", this);


        isStunned = false; // 스턴 플래그 초기화
        nextAttackTime = Time.time; // 공격 쿨다운 시간 초기화

        // 인스펙터에서 AnimationCurve가 설정되지 않았을 경우 기본 커브 생성
        // 0에서 시작하여 0.25에서 최고 속도(1), 0.75에서 최고 속도를 유지하고 1에서 0으로 떨어지는 커브
        if (dashSpeedCurve == null || dashSpeedCurve.length == 0)
        {
            dashSpeedCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 4), // 0초 지점, 속도 0, Tangent Out 4 (초기 속도 가파르게 증가)
                new Keyframe(0.25f, 1, 0, 0), // 0.25초 지점, 속도 1 (최고 속도), Tangent Flat (다음 키프레임까지 유지)
                new Keyframe(0.75f, 1, 0, 0), // 0.75초 지점, 속도 1 (최고 속도 유지), Tangent Flat
                new Keyframe(1, 0, -4, 0) // 1초 지점, 속도 0, Tangent In -4 (마무리 속도 급격히 감소)
            );
            Debug.LogWarning("Dash Speed Curve not set in Inspector. Using default curve.", this);
        }
    }

    // Update 오버라이드: 죽음, 스턴, 또는 대시 공격 중에는 Base AI 로직 스킵
    protected override void Update()
    {
        // isPerformingHurtAnimation 체크는 Base CommonEnemyController.Update()에서 이미 처리
        if (isDead || isStunned || isPerformingAttackAnimation)
        {
            return;
        }

        base.Update(); // Base 클래스의 AI 상태 머신, 이동 등 호출
    }

    // ===== 애니메이션 관련 함수 (Base 클래스에서 오버라이드) =====

    protected override void PlayIdleAnim()
    {
        // 죽음, 스턴, 피격, 대시 공격 중이 아닐 때만 Idle 애니메이션 재생
        if (!isDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_S_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        // 죽음, 스턴, 피격, 대시 공격 중이 아닐 때만 Walk 애니메이션 재생
        if (!isDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_S_WALK, true);
    }

    // S_Attacker는 점프 애니메이션이 없으므로 비워둠
    protected override void PlayJumpAnim()
    {
        // Debug.LogWarning("PlayJumpAnim called on S_AttackerController, but it has no jump.");
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_S_DEATH);
    }

    // 피격 애니메이션 재생
    protected void PlayHurtAnim()
    {
        if (!isDead && !isStunned && animator != null)
        {
            isPerformingHurtAnimation = true; // 피격 애니메이션 시작 플래그 설정
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

    // Base 클래스의 PlayAttack1Anim 오버라이드 (S Attack에 사용)
    protected override void PlayAttack1Anim()
    {
        // 죽음, 스턴, 피격 중이 아닐 때만 공격 애니메이션 트리거
        if (!isDead && !isStunned && !isPerformingHurtAnimation && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_S_ATTACK_A);
        }
    }

    // Base 클래스의 ResetAttackTriggers 오버라이드
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            // 특정 공격, 피격, 스턴 트리거 리셋
            animator.ResetTrigger(ANIM_TRIGGER_S_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_S_HURT);
            animator.ResetTrigger(ANIM_TRIGGER_S_STUN);
        }
    }

    // ===== AI 공격 로직 (Base 클래스의 PerformAttackLogic 오버라이드) =====
    protected override void PerformAttackLogic()
    {
        bool cooldownReady = Time.time >= nextAttackTime;

        // 쿨다운이 아니거나 이미 대시 중이면 공격 스킵
        if (!cooldownReady || isPerformingAttackAnimation)
        {
            return;
        }

        // 기존 대시 코루틴이 있다면 중지하고 새로운 코루틴 시작
        if (dashAttackCoroutine != null)
        {
            StopCoroutine(dashAttackCoroutine);
        }
        dashAttackCoroutine = StartCoroutine(DashAttackCoroutine());
    }

    //  대시 공격 코루틴 구현 
    private IEnumerator DashAttackCoroutine()
    {
        isPerformingAttackAnimation = true; // AI 이동을 멈추기 위한 플래그 설정

        // 대시 공격 애니메이션 재생
        PlayAttack1Anim();

        // 공격 시작 시 플레이어 통과를 위해 메인 콜라이더를 트리거로 변경 또는 비활성화
        if (mainCollider != null)
        {
            // 이 방법은 땅을 뚫고 떨어질 수 있으니 주의.
            // 만약 캐릭터가 땅을 뚫고 떨어진다면, mainCollider.isTrigger = true; 로 변경하거나 Collision Layer 설정을 통해 플레이어와만 충돌하지 않도록 조정하는 것이 가장 좋습니다.
            mainCollider.enabled = false;
        }

        float timer = 0f;
        float currentClipLength = 0.5f; // 기본값, 실제 애니메이션 클립 길이로 업데이트될 것

        // 애니메이터에서 "S_AttackA" 애니메이션 클립의 실제 길이를 가져옵니다.
        // 이것이 고정된 dashAttackDuration보다 안전합니다.
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0); // 현재 레이어 0의 클립 정보
            foreach (AnimatorClipInfo clip in clipInfo)
            {
                if (clip.clip.name.Contains("S_AttackA")) // 애니메이션 클립 이름에 "S_AttackA"가 포함되어 있다고 가정
                {
                    currentClipLength = clip.clip.length;
                    break;
                }
            }
        }

        Debug.Log($"[S_Attacker] Dash Attack Animation Length: {currentClipLength}s");

        while (timer < currentClipLength)
        {
            float normalizedTime = timer / currentClipLength; // 0에서 1까지의 정규화된 시간
            float currentSpeedMultiplier = dashSpeedCurve.Evaluate(normalizedTime); // 커브에서 현재 시간의 속도 배율 가져오기
            float currentDashMovement = currentSpeedMultiplier * dashBaseSpeed * Time.deltaTime; // 실제 이동량 계산

            // 적의 현재 바라보는 방향 (transform.localScale.x의 부호를 사용)으로 이동
            Vector3 moveDirection = new Vector3(Mathf.Sign(transform.localScale.x), 0, 0);
            transform.position += moveDirection * currentDashMovement;

            timer += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 대시 종료 후 메인 콜라이더 다시 활성화
        if (mainCollider != null)
        {
            mainCollider.enabled = true; // 또는 mainCollider.isTrigger = false;
        }

        // Base 클래스의 OnAttackAnimationEnd를 호출하여 공격 후 일시 정지 및 플래그 리셋 처리
        OnAttackAnimationEnd();
    }


    // Base 클래스의 OnAttackAnimationEnd 오버라이드
    protected override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd(); // isPerformingAttackAnimation = false 설정, 공격 후 일시 정지 코루틴 시작
        nextAttackTime = Time.time + attackSCooldown; // S_Attacker 고유의 쿨다운 시간 설정
    }

    // 피격 애니메이션 종료 시 호출될 Animation Event 메서드 오버라이드
    // S_Hurt 애니메이션 클립 끝에 이 이름의 Animation Event를 추가해야 합니다.
    public override void OnHurtAnimationEnd()
    {
        base.OnHurtAnimationEnd(); // Base 클래스의 isPerformingHurtAnimation = false 설정
    }

    // S Attack 히트박스 활성화 (S_AttackA 클립의 애니메이션 이벤트에서 호출)
    public void EnableAttackHitbox()
    {
        if (isDead) return;

        if (attackSHitboxObject != null && attackSHitboxCollider != null)
        {
            if (attackSEnemyHitbox != null)
            {
                attackSEnemyHitbox.attackDamage = attackSValue; // S_Attacker의 공격력 설정
            }
            else
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack S Hitbox Object!", attackSHitboxObject);
            }
            attackSHitboxCollider.enabled = true; // 콜라이더 활성화
        }
        else
        {
            Debug.LogWarning("Attack S Hitbox Object or Collider not assigned or found.", this);
        }
    }

    // S Attack 히트박스 비활성화 (S_AttackA 클립의 애니메이션 이벤트에서 호출)
    public void DisableAttackHitbox()
    {
        if (isDead) return;

        if (attackSHitboxCollider != null)
        {
            attackSHitboxCollider.enabled = false; // 콜라이더 비활성화
        }
        else
        {
            Debug.LogWarning("Attack S Hitbox Collider not assigned or found.", this);
        }
    }

   
}