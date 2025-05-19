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
    // Health and other stats are managed by the Base class (MaxHp, CurrentHp)

    [Header("S_Attacker Combat")]
    public float attackSValue = 0.5f; // S Attack damage
    public float attackSCooldown = 3f; // S Attack cooldown

    private float nextAttackTime = 0f; // Time when next attack is possible

    // Stun state flag (managed in derived class as it's specific to S_Attacker)
    private bool isStunned = false;
    [Header("S_Attacker Stun")]
    public float stunDuration = 2f; // Stun duration

    private Coroutine stunCoroutine; // To hold reference to the running stun coroutine

    //  ��� ���� ���� ���� 
    [Header("S_Attacker Dash Attack")]
    // �ִϸ��̼� Ŀ��: ��� ���� �� �ӵ� ��ȭ�� �����մϴ�.
    // X���� ����ȭ�� �ð�(0~1), Y���� �ӵ� ����(0~1 �̻�)�Դϴ�.
    public AnimationCurve dashSpeedCurve;
    public float dashBaseSpeed = 8f; // ��� ������ �⺻ �ӵ� (Ŀ�� ������ ������)

    private Coroutine dashAttackCoroutine; // ���� ���� ��� �ڷ�ƾ ����

    // �÷��̾� ����� ���� ���� ���� �ݶ��̴�
    private Collider2D mainCollider; // �� ��ü�� ���� �ݶ��̴� (��: CapsuleCollider2D)

    // CommonEnemyController���� ��ӹ��� attackRange�� S_Attacker�� ���� 1.5�� �����մϴ�.
    // ���⿡�� public new float attackRange = 1.5f; ���� �����߽��ϴ�. 
    // ���� CommonEnemyController�� attackRange�� ��ӹ޽��ϴ�.


    // TakeDamage �������̵� (Base Ŭ�������� ó���ǹǷ� �ʿ��� �κи� ����)
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return; // <-- isDead -> IsDead

        // ���� �߿��� �������� �԰� �ǰ� �ִϸ��̼��� ��ŵ
        if (isStunned)
        {
            base.TakeDamage(damage, attackObject); // Base Ŭ�������� ü�� ����, ��� ó��
            return;
        }

        base.TakeDamage(damage, attackObject); // Base Ŭ�������� ü�� ����, ��� ó��

        if (CurrentHp > 0) // <-- currentHealth -> CurrentHp, ���� ��������� �ǰ� �ִϸ��̼� ���
        {
            PlayHurtAnim();
        }
    }

    // ���� ���� ó��
    public void Stun() // �ܺο��� ȣ�� ���� (��: �÷��̾� �и�)
    {
        if (IsDead || isStunned) return; // <-- isDead -> IsDead

        isStunned = true;
        PlayStunAnim(); // ���� �ִϸ��̼� ���

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    // ���� ���� �ڷ�ƾ
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!IsDead) // <-- isDead -> IsDead
        {
            isStunned = false;
            // SetState(EnemyState.Chase); // �� �κ��� CommonEnemyController�� EnemyState�� ���ǵǾ� �ִٸ� ���
            // ���� ������ CommonEnemyController���� EnemyState�� �����Ƿ� �ּ� ó��
        }
    }

    // Start �������̵�
    protected override void Start()
    {
        base.Start(); // Base CommonEnemyController Start ȣ��

        // CommonEnemyController���� ��ӹ��� attackRange ���� S_Attacker�� ���� �����մϴ�.
        // �ν����Ϳ��� ���� �����ϴ� ���� ����������, �ڵ�� �����Ϸ��� ���⿡ �߰��ϼ���.
        // this.attackRange = 1.5f; 

        // ��Ʈ�ڽ� ������Ʈ �� ������Ʈ ���� �ʱ�ȭ
        if (attackSHitboxObject != null)
        {
            attackSHitboxCollider = attackSHitboxObject.GetComponent<BoxCollider2D>();
            attackSEnemyHitbox = attackSHitboxObject.GetComponent<EnemyHitbox>();
            if (attackSHitboxCollider != null)
            {
                attackSHitboxCollider.enabled = false; // ���� �� �ݶ��̴� ��Ȱ��ȭ
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

        // ���� ���� �ݶ��̴� ���� ��������
        mainCollider = GetComponent<Collider2D>();
        if (mainCollider == null) Debug.LogError("Main Collider2D component not found on S_Attacker!", this);


        isStunned = false; // ���� �÷��� �ʱ�ȭ
        nextAttackTime = Time.time; // ���� ��ٿ� �ð� �ʱ�ȭ

        // �ν����Ϳ��� AnimationCurve�� �������� �ʾ��� ��� �⺻ Ŀ�� ����
        // IDE0090 ����� �� `new AnimationCurve(...)` ǥ������ `new()`�� �ܼ�ȭ�� �� �ִٴ� �ǹ��Դϴ�.
        // ��ɻ� ������ ������, C# ������ ������ `new()`�� �����ص� �˴ϴ�.
        if (dashSpeedCurve == null || dashSpeedCurve.length == 0)
        {
            dashSpeedCurve = new AnimationCurve(
                new Keyframe(0, 0, 0, 4), // 0�� ����, �ӵ� 0, Tangent Out 4 (�ʱ� �ӵ� ���ĸ��� ����)
                new Keyframe(0.25f, 1, 0, 0), // 0.25�� ����, �ӵ� 1 (�ְ� �ӵ�), Tangent Flat (���� Ű�����ӱ��� ����)
                new Keyframe(0.75f, 1, 0, 0), // 0.75�� ����, �ӵ� 1 (�ְ� �ӵ� ����), Tangent Flat
                new Keyframe(1, 0, -4, 0) // 1�� ����, �ӵ� 0, Tangent In -4 (������ �ӵ� �ް��� ����)
            );
            Debug.LogWarning("Dash Speed Curve not set in Inspector. Using default curve.", this);
        }
    }

    // Update �������̵�: ����, ����, �Ǵ� ��� ���� �߿��� Base AI ���� ��ŵ
    protected override void Update()
    {
        // isPerformingHurtAnimation üũ�� Base CommonEnemyController.Update()���� �̹� ó��
        if (IsDead || isStunned || isPerformingAttackAnimation) // <-- isDead -> IsDead
        {
            return;
        }

        base.Update(); // Base Ŭ������ AI ���� �ӽ�, �̵� �� ȣ��
    }

    // ===== �ִϸ��̼� ���� �Լ� (Base Ŭ�������� �������̵�) =====

    protected override void PlayIdleAnim()
    {
        // ����, ����, �ǰ�, ��� ���� ���� �ƴ� ���� Idle �ִϸ��̼� ���
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null) // <-- isDead -> IsDead, isPerformingHurtAnimation ����
            animator.SetBool(ANIM_BOOL_S_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        // ����, ����, �ǰ�, ��� ���� ���� �ƴ� ���� Walk �ִϸ��̼� ���
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && !isPerformingAttackAnimation && animator != null) // <-- isDead -> IsDead, isPerformingHurtAnimation ����
            animator.SetBool(ANIM_BOOL_S_WALK, true);
    }

    // S_Attacker�� ���� �ִϸ��̼��� �����Ƿ� �����
    protected override void PlayJumpAnim()
    {
        // Debug.LogWarning("PlayJumpAnim called on S_AttackerController, but it has no jump.");
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_S_DEATH);
    }

    // �ǰ� �ִϸ��̼� ��� (Base Ŭ������ virtual �޼��� �������̵�)
    protected override void PlayHurtAnim() // <-- 'override' Ű���� ��� �� Base Ŭ������ �ش� �޼��� ���� Ȯ��
    {
        if (!IsDead && !isStunned && animator != null) // <-- isDead -> IsDead
        {
            isPerformingHurtAnimation = true; // �ǰ� �ִϸ��̼� ���� �÷��� ����
            animator.SetTrigger(ANIM_TRIGGER_S_HURT);
        }
    }

    // ���� �ִϸ��̼� ���
    protected void PlayStunAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_S_STUN);
        }
    }

    // Base Ŭ������ PlayAttack1Anim �������̵� (S Attack�� ���)
    protected override void PlayAttack1Anim()
    {
        // ����, ����, �ǰ� ���� �ƴ� ���� ���� �ִϸ��̼� Ʈ����
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null) // <-- isDead -> IsDead, isPerformingHurtAnimation ����
        {
            animator.SetTrigger(ANIM_TRIGGER_S_ATTACK_A);
        }
    }

    // Base Ŭ������ ResetAttackTriggers �������̵�
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            // Ư�� ����, �ǰ�, ���� Ʈ���� ����
            animator.ResetTrigger(ANIM_TRIGGER_S_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_S_HURT);
            animator.ResetTrigger(ANIM_TRIGGER_S_STUN);
        }
    }

    // ===== AI ���� ���� (Base Ŭ������ PerformAttackLogic �������̵�) =====
    protected override void PerformAttackLogic()
    {
        bool cooldownReady = Time.time >= nextAttackTime;

        // ��ٿ��� �ƴϰų� �̹� ��� ���̸� ���� ��ŵ
        if (!cooldownReady || isPerformingAttackAnimation)
        {
            return;
        }

        // ���� ��� �ڷ�ƾ�� �ִٸ� �����ϰ� ���ο� �ڷ�ƾ ����
        if (dashAttackCoroutine != null)
        {
            StopCoroutine(dashAttackCoroutine);
        }
        dashAttackCoroutine = StartCoroutine(DashAttackCoroutine());
    }

    //  ��� ���� �ڷ�ƾ ���� 
    private IEnumerator DashAttackCoroutine()
    {
        isPerformingAttackAnimation = true; // AI �̵��� ���߱� ���� �÷��� ����

        // ��� ���� �ִϸ��̼� ���
        PlayAttack1Anim();

        // ���� ���� �� �÷��̾� ����� ���� ���� �ݶ��̴��� Ʈ���ŷ� ���� �Ǵ� ��Ȱ��ȭ
        if (mainCollider != null)
        {
            // �� ����� ���� �հ� ������ �� ������ ����.
            // ���� ĳ���Ͱ� ���� �հ� �������ٸ�, mainCollider.isTrigger = true; �� �����ϰų� Collision Layer ������ ���� �÷��̾�͸� �浹���� �ʵ��� �����ϴ� ���� ���� �����ϴ�.
            mainCollider.enabled = false;
        }

        float timer = 0f;
        float currentClipLength = 0.5f; // �⺻��, ���� �ִϸ��̼� Ŭ�� ���̷� ������Ʈ�� ��

        // �ִϸ����Ϳ��� "S_AttackA" �ִϸ��̼� Ŭ���� ���� ���̸� �����ɴϴ�.
        // �̰��� ������ dashAttackDuration���� �����մϴ�.
        if (animator != null)
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0); // ���� ���̾� 0�� Ŭ�� ����
            foreach (AnimatorClipInfo clip in clipInfo)
            {
                if (clip.clip.name.Contains("S_AttackA")) // �ִϸ��̼� Ŭ�� �̸��� "S_AttackA"�� ���ԵǾ� �ִٰ� ����
                {
                    currentClipLength = clip.clip.length;
                    break;
                }
            }
        }

        Debug.Log($"[S_Attacker] Dash Attack Animation Length: {currentClipLength}s");

        while (timer < currentClipLength)
        {
            if (IsDead) // <-- isDead -> IsDead (��� �� ��� ����)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                yield break;
            }

            // ������ �̵� �ӵ��� ����Ͽ� �ε巴�� �̵�
            Vector3 moveDirection = new Vector3(Mathf.Sign(transform.localScale.x), 0, 0);
            float currentSpeedMultiplier = dashSpeedCurve.Evaluate(timer / currentClipLength); // Ŀ�꿡�� ���� �ð��� �ӵ� ���� ��������
            float currentDashMovement = currentSpeedMultiplier * dashBaseSpeed * Time.deltaTime; // ���� �̵��� ���
            transform.position += moveDirection * currentDashMovement;

            timer += Time.deltaTime;
            yield return null; // ���� �����ӱ��� ���
        }

        // ��� ���� �� ���� �ݶ��̴� �ٽ� Ȱ��ȭ
        if (mainCollider != null)
        {
            mainCollider.enabled = true; // �Ǵ� mainCollider.isTrigger = false;
        }

        // Base Ŭ������ OnAttackAnimationEnd�� ȣ���Ͽ� ���� �� �Ͻ� ���� �� �÷��� ���� ó��
        OnAttackAnimationEnd();
    }


    // Base Ŭ������ OnAttackAnimationEnd �������̵�
    public override void OnAttackAnimationEnd() // <-- public override ����
    {
        base.OnAttackAnimationEnd(); // isPerformingAttackAnimation = false ����, ���� �� �Ͻ� ���� �ڷ�ƾ ����
        nextAttackTime = Time.time + attackSCooldown; // S_Attacker ������ ��ٿ� �ð� ����
    }

    // �ǰ� �ִϸ��̼� ���� �� ȣ��� Animation Event �޼��� �������̵�
    // S_Hurt �ִϸ��̼� Ŭ�� ���� �� �̸��� Animation Event�� �߰��ؾ� �մϴ�.
    public override void OnHurtAnimationEnd() // <-- 'public override' Ű���� ��� (Base Ŭ������ �߰���)
    {
        base.OnHurtAnimationEnd(); // Base Ŭ������ isPerformingHurtAnimation = false ����
    }

    // S Attack ��Ʈ�ڽ� Ȱ��ȭ (S_AttackA Ŭ���� �ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void EnableAttackHitbox()
    {
        if (IsDead) return; // <-- isDead -> IsDead

        if (attackSHitboxObject != null && attackSHitboxCollider != null)
        {
            if (attackSEnemyHitbox != null)
            {
                attackSEnemyHitbox.attackDamage = attackSValue; // S_Attacker�� ���ݷ� ����
            }
            else
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack S Hitbox Object!", attackSHitboxObject);
            }
            attackSHitboxCollider.enabled = true; // �ݶ��̴� Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("Attack S Hitbox Object or Collider not assigned or found.", this);
        }
    }

    // S Attack ��Ʈ�ڽ� ��Ȱ��ȭ (S_AttackA Ŭ���� �ִϸ��̼� �̺�Ʈ���� ȣ��)
    public void DisableAttackHitbox()
    {
        if (IsDead) return; // <-- isDead -> IsDead

        if (attackSHitboxCollider != null)
        {
            attackSHitboxCollider.enabled = false; // �ݶ��̴� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("Attack S Hitbox Collider not assigned or found.", this);
        }
    }
}