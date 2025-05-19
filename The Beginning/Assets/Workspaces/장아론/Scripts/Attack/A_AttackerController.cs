using System;
using UnityEngine;
using System.Collections;

// A_Attacker (grunt - melee) character controller
// Inherits from CommonEnemyController.
// IDamageable interface implementation is handled by the Base class.
public class A_AttackerController : CommonEnemyController
{
    // A_Attacker specific animation parameter names
    private const string ANIM_BOOL_A_WALK = "A_Walk";
    private const string ANIM_TRIGGER_A_ATTACK_A = "A_AttackA"; // A Attack (slash)
    private const string ANIM_TRIGGER_A_HURT = "A_Hurt"; // Hit animation
    private const string ANIM_TRIGGER_A_STUN = "A_Stun"; // Stun animation
    private const string ANIM_TRIGGER_A_DEATH = "A_Death"; // Death animation

    [Header("A_Attacker Hitbox")]
    public GameObject attackAHitboxObject; // A attack hitbox object

    // Hitbox component references
    private BoxCollider2D attackAHitboxCollider;
    private EnemyHitbox attackAEnemyHitbox; // Assumes EnemyHitbox script exists

    [Header("A_Attacker Stats")]
    // Health and other stats are managed by the Base class (MaxHp, CurrentHp)

    [Header("A_Attacker Combat")]
    public float attackAValue = 0.5f; // A Attack damage
    public float attackACooldown = 3f; // A Attack cooldown

    private float nextAttackTime = 0f; // Time when next attack is possible

    // Stun state flag (managed in derived class as it's specific to A_Attacker)
    private bool isStunned = false;
    [Header("A_Attacker Stun")]
    public float stunDuration = 2f; // Stun duration

    private Coroutine stunCoroutine; // To hold reference to the running stun coroutine


    // Override TakeDamage from Base class
    // 시그니처를 Base 클래스 및 인터페이스와 동일하게 유지
    public override void TakeDamage(float damage, GameObject attackObject) // GameObject attackObject 추가
    {
        Debug.Log("A_Attacker TakeDamage called! Damage: " + damage + " from " + (attackObject != null ? attackObject.name : "unknown")); // 피격 로직 시작 로그 

        // Base class checks if already dead
        if (IsDead) // isDead -> IsDead 속성 사용
        {
            Debug.Log("A_Attacker is already dead, skipping damage."); // 이미 죽었는지 로그 
            return;
        }

        // If stunned, apply damage but skip hit reaction (optional logic)
        if (isStunned)
        {
            Debug.Log("A_Attacker is stunned, applying damage but skipping hit reaction."); // 경직 상태인지 로그 
            // Apply damage in Base class (updates CurrentHp, checks for death, calls HandleDeathLogic)
            // Base TakeDamage 호출 시 attackObject 함께 전달
            base.TakeDamage(damage, attackObject);
            // If Base TakeDamage resulted in death, the Base HandleDeathLogic will handle death animation/cleanup.
            // If still alive, just skip hit reaction animations.
            return; // Skip additional hit/stun reactions
        }

        // Apply damage in Base class (updates CurrentHp, checks for death, calls HandleDeathLogic)
        base.TakeDamage(damage, attackObject);

        // If not dead and not stunned, play hit animation
        // Check CurrentHp AFTER calling base.TakeDamage
        // CurrentHp는 Base 클래스에서 업데이트됩니다.
        if (CurrentHp > 0) // currentHealth -> CurrentHp 속성 사용
        {
            // IsDead 플래그는 Base.TakeDamage 호출 후 HandleDeathLogic에서 업데이트됩니다.
            // isStunned는 이 메서드 진입 전 체크했습니다.
            Debug.Log("A_Attacker not dead and not stunned. Current Health: " + CurrentHp.ToString("F2") + ". Calling PlayHurtAnim."); // PlayHurtAnim 호출 조건 만족 로그 
            PlayHurtAnim(); // Play hit animation (includes IsDead/isStunned check inside PlayHurtAnim)
        }
        else
        {
            Debug.Log("A_Attacker died from damage. Current Health: " + CurrentHp.ToString("F2") + ". Death handled by base."); // 데미지로 사망 로그 
                                                                                                                                // Death is handled by base.TakeDamage calling HandleDeathLogic
        }
    }

    // Handle Stun state
    public void Stun() // Can be called externally (e.g., Player parry)
    {
        // Do not stun if already dead or stunned
        if (IsDead || isStunned) return; // isDead -> IsDead 속성 사용

        isStunned = true;
        //Debug.Log("A_Attacker entering Stun state!");
        PlayStunAnim(); // Play stun animation (includes IsDead check)

        // Stop any existing stun coroutine and start a new one
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));

        // TODO: Stop AI movement (handled by Update override and Base checks)
        // TODO: Disable colliders if needed during stun
    }

    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration); // Wait for specified duration

        // Release stun if not dead
        if (!IsDead) // isDead -> IsDead 속성 사용
        {
            isStunned = false;
            //Debug.Log("A_Attacker exiting Stun state!");
            // TODO: Return to appropriate state (e.g., Chase) - 
            // CommonEnemyController는 이제 EnemyState enum을 사용하지 않으므로,
            // isStunned 플래그가 해제되면 Update() 루프에서 자동으로 플레이어와의 거리에 따라 행동을 결정합니다.
            // SetState(EnemyState.Chase); // 이 줄은 제거합니다.
            // TODO: Reset animator state if needed (Animator Controller setup is key)
        }
    }

    // Override Start from Base class
    protected override void Start()
    {
        base.Start(); // Call Base CommonEnemyController Start

        // Initialize hitbox object and component references
        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>(); // Assumes EnemyHitbox component
            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false; // Disable collider at start
            }
            else
            {
                Debug.LogWarning("BoxCollider2D component not found on Attack A Hitbox Object.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack A Hitbox Object!", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack A Hitbox Object is not assigned in the Inspector.", this);
        }

        // Initialize A_Attacker specific state
        // Health is initialized in Base Start, IsDead is handled by Base, isPerformingHurtAnimation is Base
        isStunned = false; // Initialize stun state
        nextAttackTime = Time.time; // Or 0f; Initialize attack cooldown
    }

    // Override Update: Skip Base AI logic if stunned or performing hurt animation
    protected override void Update()
    {
        // 피격 애니메이션 중일 때도 Base Update 스킵 (CommonEnemyController에서 이미 처리) 
        // A_AttackerController의 Update 오버라이드는 isStunned만 추가로 체크하여 Base.Update() 호출 여부를 결정합니다.
        // isPerformingHurtAnimation 체크는 Base CommonEnemyController.Update() 시작 부분에서 전체 Update 내용을 스킵하도록 처리했습니다.
        if (IsDead || isStunned) // IsDead 속성 사용, isStunned 사용
        {
            // If dead, stunned, or performing hurt animation (handled in Base Update), skip Base AI logic and movement
            return;
        }

        // Call Base class Update for AI state machine, movement, etc.
        base.Update();
    }


    // ===== Animation related functions (Override virtual methods from Base class) =====
    // Called by Base CommonEnemyController's state/logic. These trigger the specific animations.
    // isPerformingHurtAnimation 체크 조건은 Base CommonEnemyController의 PlayAnim 메서드에 추가해야 할 수 있습니다.
    // 현재 구조에서는 Base Update 시작에서 isPerformingHurtAnimation 시 전체 스킵하므로,
    // PlayAnim 메서드 내부에서는 다시 체크하지 않아도 될 수 있습니다. (애니메이터 설정에 따라 다름)

    protected override void PlayIdleAnim()
    {
        // Check Base IsDead and local isStunned/isPerformingHurtAnimation flags
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null) // IsDead 속성 사용
            animator.SetBool(ANIM_BOOL_A_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        // Check Base IsDead and local isStunned/isPerformingHurtAnimation flags
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null) // IsDead 속성 사용
            animator.SetBool(ANIM_BOOL_A_WALK, true);
    }

    // A_Attacker does not have a Jump animation, override if Base calls it
    protected override void PlayJumpAnim()
    {
        // Leave empty or add debug if Base calls this unexpectedly
        // Debug.LogWarning("PlayJumpAnim called on A_AttackerController, but it has no jump.");
    }

    protected override void PlayDeathAnim()
    {
        // Trigger Death animation
        // Base HandleDeathLogic calls this after setting IsDead = true.
        if (animator != null) // IsDead 체크는 Base HandleDeathLogic에서 이미 함
            animator.SetTrigger(ANIM_TRIGGER_A_DEATH);
        // TODO: Add A_Attacker specific death effects if any
        // Object destruction and GameManager notification are handled in Base HandleDeathLogic
    }

    // Play hit animation (called from TakeDamage)
    protected override void PlayHurtAnim() // protected -> protected override (Base 클래스에 virtual로 정의됨)
    {
        Debug.Log("PlayHurtAnim called. IsDead=" + IsDead + ", isStunned=" + isStunned + ", animator=" + (animator != null)); // PlayHurtAnim 진입 로그 
        // Check Base IsDead and local isStunned flags
        // isPerformingHurtAnimation은 여기서 설정하므로 이 조건에는 포함시키지 않음 
        if (!IsDead && !isStunned && animator != null) // IsDead 속성 사용
        {
            Debug.Log("PlayHurtAnim conditions met. Setting trigger: " + ANIM_TRIGGER_A_HURT); // 트리거 설정 직전 로그 
            isPerformingHurtAnimation = true; // 피격 애니메이션 시작 플래그 설정 
            animator.SetTrigger(ANIM_TRIGGER_A_HURT); // Trigger hit animation
            //Debug.Log("PlayHurtAnim triggered.");
        }
        else
        {
            Debug.Log("PlayHurtAnim conditions NOT met. Skipping trigger. IsDead=" + IsDead + ", isStunned=" + isStunned); // 트리거 설정 스킵 로그 
        }
    }

    // Play stun animation (called from Stun)
    protected void PlayStunAnim() // Protected access level
    {
        // Check Base IsDead flag
        // Stun() method already checks IsDead before calling this.
        if (animator != null) // IsDead 체크는 이미 Stun 함수에서 함
        {
            // isPerformingHurtAnimation 플래그는 Stun에서는 사용하지 않음 
            animator.SetTrigger(ANIM_TRIGGER_A_STUN);
            //Debug.Log("PlayStunAnim triggered.");
        }
    }


    // Override PlayAttack1Anim from Base (used for A Attack)
    protected override void PlayAttack1Anim() // Base CommonEnemyController has PlayAttack1Anim virtual
    {
        // Check Base IsDead and local isStunned/isPerformingHurtAnimation flags
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null) // IsDead 속성 사용
        {
            animator.SetTrigger(ANIM_TRIGGER_A_ATTACK_A); // Trigger A Attack animation
            //Debug.Log("A_AttackA animation triggered!");
        }
    }

    // Override ResetAttackTriggers from Base
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            // Reset specific attack, hit, and stun triggers
            animator.ResetTrigger(ANIM_TRIGGER_A_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_A_HURT); // 피격 트리거도 리셋
            animator.ResetTrigger(ANIM_TRIGGER_A_STUN);
            // Call Base ResetAttackTriggers if it resets other common triggers
            // base.ResetTriggers(); // Example: if Base has a common trigger
        }
    }

    // ===== AI Attack Logic (Override virtual PerformAttackLogic from Base) =====
    // Called by CommonEnemyController's Attack state when pre-attack pause is over.
    protected override void PerformAttackLogic()
    {
        Debug.Log("A_Attacker PerformAttackLogic override called."); // 디버그 로그 유지 
        // Base class already checks isPerformingAttackAnimation, isDead, isWaitingForAttack, isWaitingAfterAttack before calling this.
        // isStunned, isPerformingHurtAnimation 중이면 PerformAttackLogic이 호출되지 않도록 Base Update에서 스킵합니다. 

        // Check A_Attacker's specific cooldown
        bool cooldownReady = Time.time >= nextAttackTime;
        Debug.Log("A_Attacker checking cooldown. Current Time: " + Time.time.ToString("F2") + ", Next Attack Time: " + nextAttackTime.ToString("F2") + ". Cooldown Ready: " + cooldownReady); // 디버그 로그 유지 

        if (!cooldownReady)
        {
            return; // Skip attack logic if cooldown is active
        }

        // If cooldown is ready, perform attack
        Debug.Log("A_Attacker cooldown READY! Triggering A_AttackA animation."); // 디버그 로그 유지 

        isPerformingAttackAnimation = true; // Set Base class flag
        // isPerformingHurtAnimation는 Hurt 애니메이션 이벤트에서 해제됩니다.

        PlayAttack1Anim(); // <-- Call the overridden animation method

        // Cooldown calculation happens in OnAttackAnimationEnd
    }

    // ===== Animation Event Callbacks (Override virtual methods or public methods called by Animator) =====

    // Override OnAttackAnimationEnd from Base
    public override void OnAttackAnimationEnd()
    {
        Debug.Log("A_Attacker Attack Animation End Event (Override)."); // 디버그 로그 유지 
        // Call Base class logic (sets isPerformingAttackAnimation = false, starts post-attack pause coroutine)
        base.OnAttackAnimationEnd();

        // Calculate A_Attacker's specific next attack time based on its cooldown
        nextAttackTime = Time.time + attackACooldown;
        Debug.Log("--> A_AttackA finished. Next attack possible in " + attackACooldown.ToString("F2") + "s (at " + nextAttackTime.ToString("F2") + ")."); // 디버그 로그 유지 

        // Returning to Chase state is handled by the Base class PostAttackPauseRoutine after the delay.
    }

    // 피격 애니메이션 종료 시 호출될 Animation Event 메서드 오버라이드 
    // A_Hurt 애니메이션 클립 끝에 이 이름의 Animation Event를 추가해야 합니다.
    public override void OnHurtAnimationEnd() // Base 클래스의 virtual 메서드 오버라이드
    {
        Debug.Log("A_Attacker OnHurtAnimationEnd called."); // 디버그 로그 추가 
        // Base 클래스의 OnHurtAnimationEnd()를 호출하여 isPerformingHurtAnimation 플래그를 false로 설정합니다.
        base.OnHurtAnimationEnd();
        Debug.Log("isPerformingHurtAnimation set to false by base.OnHurtAnimationEnd()."); // 플래그 해제 확인 로그 


        // 피격 애니메이션 종료 후 적이 죽지 않았다면 AI 상태를 재평가하도록 할 수 있습니다.
        // SetState(EnemyState.Chase); 와 같이 상태를 직접 설정하거나,
        // 아니면 플래그만 해제하면 Update()에서 거리에 따라 알아서 다음 상태로 전환됩니다.
        // 여기서는 플래그만 해제하고 Update에서 자연스럽게 상태 전환되도록 합니다.
        if (!IsDead) // IsDead 속성 사용
        {
            // 피격 종료 후 바로 Idle로 보낼 수도 있습니다 (선택 사항)
            // PlayIdleAnim(); 
            // 또는 현재 거리에 따라 Update에서 알아서 Chase/Attack/Idle 결정 (기본 동작)
        }
    }


    // Enable A Attack Hitbox (Called by Animation Event on A_AttackA clip)
    // Keep public for Animation Events. Signature must match Animation Event setup.
    public void EnableAttackHitbox() // 수정된 이름 유지
    {
        // Check Base IsDead flag
        if (IsDead) return; // IsDead 속성 사용 // Do not enable hitbox if dead

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            // Set damage value on the EnemyHitbox component
            // Assumes EnemyHitbox.cs has a public float attackDamage; variable.
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue; // Set damage from A_Attacker's value
                //Debug.Log("Attack A Hitbox damage set to: " + attackAValue);
            }
            else
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack A Hitbox Object!", attackAHitboxObject);
            }

            attackAHitboxCollider.enabled = true; // Enable collider
            //Debug.Log(attackAHitboxObject.name + " Collider enabled.");

            // TODO: Reset hit flag on EnemyHitbox if needed to prevent hitting the same target multiple times per swing
            // if (attackAEnemyHitbox != null) attackAEnemyHitbox.ResetHitFlag();
        }
        else
        {
            Debug.LogWarning("Attack A Hitbox Object or Collider not assigned or found.", this);
        }
    }

    // Disable A Attack Hitbox (Called by Animation Event on A_AttackA clip)
    // Keep public for Animation Events. Signature must match Animation Event setup.
    public void DisableAttackHitbox() // 수정된 이름 유지
    {
        // Check Base IsDead flag
        if (IsDead) return; // IsDead 속성 사용 // Do not disable hitbox if dead (already disabled in Base HandleDeathLogic)

        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false; // Disable collider
            //Debug.Log("Attack A Hitbox Collider disabled.");
        }
        else
        {
            Debug.LogWarning("Attack A Hitbox Collider not assigned or found.", this);
        }
    }

    // ======================================================================
    // **새로 추가/수정된 Flip (좌우 방향 전환) 메서드 오버라이드**
    // CommonEnemyController의 Flip 로직을 반대로 적용하여 A_Attacker의 방향을 맞춥니다.
    // 만약 A_Attacker의 스프라이트가 기본적으로 '왼쪽'을 바라보고 있고
    // 'localScale.x = 1'일 때 왼쪽을 바라보게 되어 있다면, 이 로직이 맞을 수 있습니다.
    // ======================================================================
    protected override void Flip(bool faceLeft)
    {
        Transform spriteToFlip = transform.Find("Sprite"); // 'Sprite'라는 자식 오브젝트를 찾음

        if (spriteToFlip == null)
        {
            spriteToFlip = transform; // 자식 오브젝트가 없으면 자기 자신(루트 오브젝트)을 사용
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        Vector3 currentScale = spriteToFlip.localScale;

        // 여기서는 'faceLeft'의 의미를 반전시켜서 적용해봅니다.
        // 즉, 'faceLeft'가 true일 때 (왼쪽으로 가야 할 때) 실제로는 '오른쪽'을 바라보도록 스케일을 만들고,
        // 'faceLeft'가 false일 때 (오른쪽으로 가야 할 때) 실제로는 '왼쪽'을 바라보도록 스케일을 만듭니다.
        // 이는 스프라이트의 기본 방향이 일반적인 예상과 반대일 때 유용합니다.
        if (faceLeft) // 왼쪽을 바라봐야 한다고 코드(MoveTowardsPlayer)가 지시할 때
        {
            // 실제로는 오른쪽을 보도록 스케일을 양수로 만듭니다.
            spriteToFlip.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else // 오른쪽을 바라봐야 한다고 코드(MoveTowardsPlayer)가 지시할 때
        {
            // 실제로는 왼쪽을 보도록 스케일을 음수로 만듭니다.
            spriteToFlip.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }
    // ======================================================================


    // ======================================================================
    // 외부에서 플레이어 Transform을 설정하기 위한 함수
    // "전체 관리하는 친구"가 이 함수를 호출하여 플레이어 정보를 전달할 것입니다.
    // ======================================================================
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