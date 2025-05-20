using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// ���02 (�ٰŸ�) ĳ���͸� �����ϴ� ��ũ��Ʈ�Դϴ�.
/// �� ��ũ��Ʈ�� CommonEnemyController�� ��ӹ޾� �⺻���� ���� �ൿ(����, ���, ����, ����)�� ����ϸ�,
/// ���02�� ������ �ɷ�ġ, ���� ���� ����, ����(�и� ��) ������ �����մϴ�.
/// </summary>
public class Mob02MeleeController : CommonEnemyController
{
    // --- �ִϸ��̼� �Ķ���� �̸� ��� ���� ---
    private const string ANIM_BOOL_MOB02_WALK = "Mob02_Walk";
    private const string ANIM_TRIGGER_MOB02_ATTACK_A = "Mob02_AttackA";
    private const string ANIM_TRIGGER_MOB02_HURT = "Mob02_Hurt";
    private const string ANIM_TRIGGER_MOB02_STUN = "Mob02_Stun";
    private const string ANIM_TRIGGER_MOB02_DEATH = "Mob02_Death";

    // --- �ִϸ��̼� Ŀ�� �̸� ��� ���� (�߰�) ---
    private const string ANIM_CURVE_ATTACK_FORWARD_SPEED = "AttackForwardSpeed"; // ���� �� ���� �ӵ� ����� Ŀ�� �̸�

    [Header("���02 ���� ���� ����")]
    [Tooltip("A ����(��������) �� Ȱ��ȭ�� ��Ʈ�ڽ� ���� ������Ʈ�� �����ϼ���.")]
    public GameObject attackAHitboxObject;

    private BoxCollider2D attackAHitboxCollider;
    private EnemyHitbox attackAEnemyHitbox;

    [Header("���02 ���� �ɷ�ġ")]
    [Tooltip("���02�� �ִ� ü���Դϴ�. CommonEnemyController�� MaxHp�� �� ������ �ʱ�ȭ�մϴ�.")]
    [SerializeField] private float mob02MaxHealth = 4f;

    [Header("���02 ���� ����")]
    [Tooltip("'��������' ���� �� ����� ���ط��Դϴ�.")]
    public float attackAValue = 0.4f;
    [Tooltip("���� �ִϸ��̼� ���� �� ���� ������ �õ��ϱ� ������ ��ٸ� �ð� (1��).")]
    public float attackACooldown = 1.0f;

    // ����(Stun) ���� ���� ����
    private bool isStunned = false;
    [Header("���02 ���� ����")]
    [Tooltip("�÷��̾��� �и� ���� � ���� �����Ǿ��� �� ���ӵǴ� �ð��Դϴ�.")]
    public float stunDuration = 2f;

    private Coroutine stunCoroutine;

    // --- �߰��� ����: SpriteRenderer ���� ---
    private SpriteRenderer spriteRenderer; // ĳ������ ��������Ʈ ������ ������Ʈ

    /// <summary>
    /// ������Ʈ�� ������ �� ���� ���� ȣ��˴ϴ�.
    /// CommonEnemyController�� Awake�� ȣ���ϰ�, ���02�� �ʱ� �ɷ�ġ�� �����ϸ�,
    /// SpriteRenderer ������Ʈ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // �θ� Ŭ������ CommonEnemyController�� Awake �޼��� ȣ��

        // ���02�� Ư�� �ɷ�ġ�� �ִ� ü�°� ���� ü���� �ʱ�ȭ�մϴ�.
        MaxHp = mob02MaxHealth;
        CurrentHp = MaxHp;

        // --- SpriteRenderer ������Ʈ ���� �ʱ�ȭ (�߰�) ---
        // �� ��ũ��Ʈ�� ���� ���� ������Ʈ���� SpriteRenderer ������Ʈ�� ã���ϴ�.
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // ���� SpriteRenderer�� ���� ������Ʈ�� ���� �ڽ� ������Ʈ�� �ִٸ�
            // �ڽ� ������Ʈ �� "Sprite"��� �̸��� ������Ʈ���� ã�ƺ��ϴ�.
            Transform spriteChild = transform.Find("Sprite");
            if (spriteChild != null)
            {
                spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            }
        }

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Mob02MeleeController: SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�. ��������Ʈ ������(Flip)�� ���������� �۵����� ���� �� �ֽ��ϴ�.", this);
        }
    }

    /// <summary>
    /// ��ũ��Ʈ�� Ȱ��ȭ�� �� �� �� ȣ��˴ϴ�.
    /// �÷��̾� Ÿ���� �����ϰ�, ��Ʈ�ڽ� ���� ������Ʈ�� �ʱ�ȭ�մϴ�.
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
            Debug.LogWarning("Mob02MeleeController: 'Player' �±׸� ���� ���� ������Ʈ�� ã�� �� �����ϴ�. �÷��̾ ���� �ִ���, �±װ� �ùٸ��� Ȯ���ϼ���.", this);
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
                Debug.LogWarning("BoxCollider2D ������Ʈ�� 'Attack A Hitbox Object'�� �����ϴ�.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox ������Ʈ�� 'Attack A Hitbox Object'�� �����ϴ�!", this);
            }
        }
        else
        {
            Debug.LogWarning("'Attack A Hitbox Object'�� �ν����Ϳ� �Ҵ���� �ʾҽ��ϴ�.", this);
        }

        isStunned = false;
        nextAttackTime = Time.time;
    }

    /// <summary>
    /// �� ������ ȣ��˴ϴ�.
    /// ���� ���� ���¿� ���� AI ������ �������� �����մϴ�.
    /// </summary>
    protected override void Update()
    {
        // 1. ġ������ ���� üũ (���, ����, �ǰ� �ִϸ��̼� ��):
        //    �� ���¿����� ��� �����Ӱ� AI ������ �����ϰ� Update() �Լ��� �ٷ� �����մϴ�.
        if (IsDead || isStunned || isPerformingHurtAnimation)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero; // �������� ������ ����
            return; // ���� �������� ��� ���� ������ �ǳʶݴϴ�.
        }

        // 2. ���� �ִϸ��̼��� Ȱ��ȭ�� ����:
        //    �̶��� �ִϸ��̼� Ŀ�긦 �̿��� ���� �� �̵�(`ApplyAttackAnimationMovement()`)�� �����ϰ�,
        //    �ٸ� �Ϲ� AI ����(���� ��)�� ��� �ߴܵ˴ϴ�.
        if (isPerformingAttackAnimation)
        {
            ApplyAttackAnimationMovement(); // ���� �ִϸ��̼ǿ� ���� �̵� ����
            return; // ���� �������� ��� ���� ������ �ǳʶݴϴ�.
        }

        // 3. ���� �� ��ٿ� ��� ����:
        //    ���� �ִϸ��̼��� ���� �� ���� ���ݱ��� ��ٿ� �ð� ���� ��� ���¸� �����մϴ�.
        //    �̶��� �Ϲ� AI ����(���� ��)�� �ߴܵ˴ϴ�.
        if (isWaitingAfterAttack)
        {
            PlayIdleAnim(); // ��� �ִϸ��̼� ���
            if (rb != null) rb.linearVelocity = Vector2.zero; // ���ڸ��� �����ֵ��� ��
            return; // ���� �������� ��� ���� ������ �ǳʶݴϴ�.
        }

        // 4. ���� � Ư���� ���µ� �ƴ� ��:
        //    �⺻���� AI ����(�÷��̾� ����, ���� ��)�� �Ϲ����� �̵��� �̷�����ϴ�.
        //    CommonEnemyController�� Update() �޼��尡 ȣ��Ǿ� �� ������ �մϴ�.
        base.Update();
    }

    /// <summary>
    /// ���� ���ظ� �Ծ��� �� ȣ��˴ϴ�.
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
    /// ���� ���� ���·� ����ϴ�. (��: �÷��̾��� �и� ���� ���� �� ȣ��)
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
    /// ������ �ð� �Ŀ� ���� ���¸� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (!IsDead)
        {
            isStunned = false;
        }
    }

    // --- �ִϸ��̼� ���� �Լ� ---

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
    /// ���� �ִϸ��̼� �� �ִϸ��̼� Ŀ�� ���� �о� ���� �̵��� �����մϴ�.
    /// �� �޼���� 'Mob02_AttackA' �ִϸ��̼� Ŭ���� 'AttackForwardSpeed'��� �̸��� Ŀ�갡 �����Ѵٰ� �����մϴ�.
    /// �� Ŀ��� ���� �� ���� �κп��� ������ �̵��ߴٰ� �����ϴ� �ӵ� ��ȭ�� �����մϴ�.
    /// </summary>
    private void ApplyAttackAnimationMovement()
    {
        if (animator == null || rb == null) return;

        float curveSpeedFactor = animator.GetFloat(ANIM_CURVE_ATTACK_FORWARD_SPEED);

        // SpriteRenderer�� flipX�� ����Ͽ� ĳ������ ���� �ٶ󺸴� ������ �����մϴ�.
        // spriteRenderer�� �ʱ�ȭ�Ǿ����� Ȯ���մϴ�.
        Vector2 currentFacingDirection = Vector2.zero; // �⺻��
        if (spriteRenderer != null)
        {
            // spriteRenderer.flipX�� true�� ��������Ʈ�� ������ �ٶ� (Vector2.left)
            // false�� ��������Ʈ�� �������� �ٶ� (Vector2.right)
            currentFacingDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
        else // spriteRenderer�� ã�� ���ߴٸ�, Rigidbody�� ���� �ӵ� ������ �ӽ÷� ���
        {
            // �� fallback�� SpriteRenderer�� ���� ��쿡�� ���Ǹ�,
            // Flip() �Լ��� Transform.localScale�� ���� �����ߴٸ� �� ������ �����ؾ� �� �� �ֽ��ϴ�.
            if (rb != null)
            {
                if (rb.linearVelocity.x < 0) currentFacingDirection = Vector2.left;
                else if (rb.linearVelocity.x > 0) currentFacingDirection = Vector2.right;
                else currentFacingDirection = Vector2.right; // �⺻������ ������
            }
            else
            {
                currentFacingDirection = Vector2.right; // Rigidbody�� ������ �⺻ ������
            }
        }

        float actualMoveSpeed = moveSpeed * curveSpeedFactor;

        // Rigidbody�� ���� �ӵ��� �����մϴ�. y���� ���� �ӵ��� �����Ͽ� �߷� �� ������ �޵��� �մϴ�.
        rb.linearVelocity = new Vector2(currentFacingDirection.x * actualMoveSpeed, rb.linearVelocity.y);
    }

    // --- AI ���� ���� ---

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

    // --- �ִϸ��̼� �̺�Ʈ �ݹ� �Լ� ---

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        // ���� �ִϸ��̼��� ���� �� ���� ���ݱ����� ��ٿ� �ð��� �����մϴ�.
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
                Debug.LogWarning("Mob02MeleeController: EnemyHitbox ������Ʈ�� 'Attack A Hitbox Object'�� �����ϴ�!", attackAHitboxObject);
            }
            attackAHitboxCollider.enabled = true;
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object' �Ǵ� �ݶ��̴��� �Ҵ���� �ʾҰų� ã�� �� �����ϴ�. ��Ʈ�ڽ� Ȱ��ȭ ����.", this);
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
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Collider'�� �Ҵ���� �ʾҰų� ã�� �� �����ϴ�. ��Ʈ�ڽ� ��Ȱ��ȭ ����.", this);
        }
    }

    /// <summary>
    /// ���� �ð����� ����(��������Ʈ)�� ��ȯ�մϴ�.
    /// SpriteRenderer�� `flipX` �Ӽ��� ����մϴ�.
    /// </summary>
    /// <param name="faceLeft">������ �ٶ���� �ϸ� true, �������� �ٶ���� �ϸ� false.</param>
    protected override void Flip(bool faceLeft)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning(gameObject.name + ": SpriteRenderer ������Ʈ�� �����ϴ�. ��������Ʈ �����⸦ ������ �� �����ϴ�.", this);
            return;
        }

        // faceLeft�� true�̸� flipX�� true�� (������ �ٶ�), �ƴϸ� false�� (�������� �ٶ�)
        spriteRenderer.flipX = faceLeft;
    }

    public void SetPlayerTarget(Transform newPlayerTransform)
    {
        if (newPlayerTransform != null)
        {
            playerTransform = newPlayerTransform;
            Debug.Log($"{gameObject.name}: �÷��̾� Ÿ���� �����Ǿ����ϴ�: {playerTransform.name}", this);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: SetPlayerTarget �Լ��� ���޵� �÷��̾� Transform�� null�Դϴ�. �÷��̾ ������ �� �����ϴ�.", this);
        }
    }
}