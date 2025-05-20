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
    // ����Ƽ Animator Controller�� ������ �̸��� �Ķ���͸� �����ؾ� �մϴ�.
    private const string ANIM_BOOL_MOB02_WALK = "Mob02_Walk";             // �ȱ� �ִϸ��̼� ����� ���� Bool �Ķ����
    private const string ANIM_TRIGGER_MOB02_ATTACK_A = "Mob02_AttackA";   // '��������(�Ϲ�)' ���� �ִϸ��̼��� ���� Trigger �Ķ����
    private const string ANIM_TRIGGER_MOB02_HURT = "Mob02_Hurt";         // �ǰ� �ִϸ��̼��� ���� Trigger �Ķ����
    private const string ANIM_TRIGGER_MOB02_STUN = "Mob02_Stun";         // ���� �ִϸ��̼��� ���� Trigger �Ķ����
    private const string ANIM_TRIGGER_MOB02_DEATH = "Mob02_Death";       // ��� �ִϸ��̼��� ���� Trigger �Ķ����

    [Header("���02 ���� ���� ����")]
    [Tooltip("A ����(��������) �� Ȱ��ȭ�� ��Ʈ�ڽ� ���� ������Ʈ�� �����ϼ���.")]
    public GameObject attackAHitboxObject; // '��������' ������ ��Ʈ�ڽ� ������Ʈ

    // ��Ʈ�ڽ� ������Ʈ ����
    // �� �������� ��ũ��Ʈ ���� �� �ڵ����� ã�Ƽ� �Ҵ�˴ϴ�.
    private BoxCollider2D attackAHitboxCollider; // ��Ʈ�ڽ� ������Ʈ�� �پ��ִ� BoxCollider2D
    private EnemyHitbox attackAEnemyHitbox;      // ��Ʈ�ڽ� ������Ʈ�� �پ��ִ� EnemyHitbox ��ũ��Ʈ (���ݷ� ���޿�)

    [Header("���02 ���� �ɷ�ġ")]
    [Tooltip("���02�� �ִ� ü���Դϴ�. CommonEnemyController�� MaxHp�� �� ������ �ʱ�ȭ�մϴ�.")]
    [SerializeField] private float mob02MaxHealth = 4f; // **���02�� ü��: 4**

    [Header("���02 ���� ����")]
    [Tooltip("'��������' ���� �� ����� ���ط��Դϴ�.")]
    public float attackAValue = 0.4f; // **'��������' ���ݷ�: 0.4**
    [Tooltip("���� �ִϸ��̼� ���� �� ���� ������ �õ��ϱ� ������ ��ٸ� �ð� (1��).")]
    public float attackACooldown = 1.0f; // **���� �ִϸ��̼� ���� �� 1�� ��**

    // ����(Stun) ���� ���� ����
    private bool isStunned = false; // ���� ���� �������� ��Ÿ���� �÷���
    [Header("���02 ���� ����")]
    [Tooltip("�÷��̾��� �и� ���� � ���� �����Ǿ��� �� ���ӵǴ� �ð��Դϴ�.")]
    public float stunDuration = 2f; // **�и� �� ���� ���� �ð�**

    private Coroutine stunCoroutine; // ���� ���� ���� ���� �ڷ�ƾ�� �����Ͽ� �ߺ� ���� ����

    /// <summary>
    /// ������Ʈ�� ������ �� ���� ���� ȣ��˴ϴ�.
    /// CommonEnemyController�� Awake�� ȣ���ϰ�, ���02�� �ʱ� �ɷ�ġ�� �����մϴ�.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // �θ� Ŭ������ CommonEnemyController�� Awake �޼��� ȣ��

        // ���02�� Ư�� �ɷ�ġ�� �ִ� ü�°� ���� ü���� �ʱ�ȭ�մϴ�.
        MaxHp = mob02MaxHealth; // CommonEnemyController�� MaxHp�� 4�� ����
        CurrentHp = MaxHp;      // ���� ü�µ� �ִ� ü������ ����
    }

    /// <summary>
    /// ��ũ��Ʈ�� Ȱ��ȭ�� �� �� �� ȣ��˴ϴ�.
    /// �÷��̾� Ÿ���� �����ϰ�, ��Ʈ�ڽ� ���� ������Ʈ�� �ʱ�ȭ�մϴ�.
    /// </summary>
    protected override void Start()
    {
        base.Start(); // �θ� Ŭ������ CommonEnemyController�� Start �޼��� ȣ��

        // �÷��̾� ������Ʈ�� ã�� Ÿ������ �����մϴ�.
        // ���� "Player" �±׸� ���� ���� ������Ʈ�� �־�� �մϴ�.
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            SetPlayerTarget(playerGameObject.transform);
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Player' �±׸� ���� ���� ������Ʈ�� ã�� �� �����ϴ�. �÷��̾ ���� �ִ���, �±װ� �ùٸ��� Ȯ���ϼ���.", this);
        }

        // ���� ��Ʈ�ڽ� ������Ʈ�� �Ҵ�Ǿ��ٸ�, �ش� ������Ʈ���� ã�� �����մϴ�.
        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>();

            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false; // ���� ���� �� ���� ��Ʈ�ڽ� �ݶ��̴��� ��Ȱ��ȭ
            }
            else
            {
                Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'�� BoxCollider2D ������Ʈ�� �����ϴ�.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'�� EnemyHitbox ������Ʈ�� �����ϴ�!", this);
            }
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'�� �ν����Ϳ� �Ҵ���� �ʾҽ��ϴ�. ���� ������ �۵����� ���� �� �ֽ��ϴ�.", this);
        }

        // �ʱ� ���� ���� ���� �� ���� ���� ���� �ð� �ʱ�ȭ
        isStunned = false;
        nextAttackTime = Time.time;
    }

    /// <summary>
    /// �� ������ ȣ��˴ϴ�.
    /// ���� ���� ����(���, ����, �ǰ� ��, ���� ��)�� ���� AI ������ �������� �����մϴ�.
    /// </summary>
    protected override void Update()
    {
        // 1. ���� ����߰ų�, ���� �����̰ų�, �ǰ� �ִϸ��̼� ���� ���� ��� AI ������ �������� �����մϴ�.
        if (IsDead || isStunned || isPerformingHurtAnimation)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // �������� ������(�ӵ�) ����
            }
            return; // �� �������� ������ Update ������ �ǳʶݴϴ�.
        }

        // 2. ���� �ִϸ��̼� ���̰ų� ���� �� ��� ���� ���� �������� ���߰� ��� �ִϸ��̼��� ����մϴ�.
        // �̴� CommonEnemyController�� Update���� ó���Ǵ� �κ��̱⵵ ������, ��������� ������� üũ�� �մϴ�.
        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            PlayIdleAnim(); // ����/��� �߿��� ��� �ִϸ��̼��� ���
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // �������� ������ ����
            }
            return; // �� �������� ������ Update ������ �ǳʶݴϴ�.
        }

        // 3. �� ���� ���°� �ƴϸ�, �θ� Ŭ������ �Ϲ����� AI ����(�÷��̾� ����, �̵�, ���� ���� üũ ��)�� �����մϴ�.
        base.Update();
    }

    /// <summary>
    /// ���� ���ظ� �Ծ��� �� ȣ��˴ϴ�.
    /// ���� ������ ���� �߰����� �ǰ� ������ �����մϴ�.
    /// </summary>
    /// <param name="damage">������ ���ط�.</param>
    /// <param name="attackObject">������ ���� ������Ʈ (��: �÷��̾��� ����).</param>
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return; // �̹� ��������� �ƹ��͵� ���� ����

        // ���� ���� ������ ���� ���ش� ������, �߰����� �ǰ� �ִϸ��̼��� �ǳʶ� �� �ֽ��ϴ�.
        if (isStunned)
        {
            base.TakeDamage(damage, attackObject); // �θ� Ŭ�������� ü�� ���� �� ��� üũ ���� ����
            return; // ���� ������ ���� �߰����� �ǰ�/���� ���� ��ŵ
        }

        // �θ� Ŭ������ TakeDamage ȣ��: ü�� ����, ü�� 0 ���� �� ��� ó��(`HandleDeathLogic` ȣ��)
        base.TakeDamage(damage, attackObject);

        // ���� ���� ���� �ʾҰ� ���� ���µ� �ƴ� ��� �ǰ� �ִϸ��̼��� ����մϴ�.
        if (CurrentHp > 0)
        {
            PlayHurtAnim();
        }
        // ��� ������ `base.TakeDamage` ������ `HandleDeathLogic`�� ȣ���Ͽ� ó���˴ϴ�.
    }

    /// <summary>
    /// ���� ���� ���·� ����ϴ�. (��: �÷��̾��� �и� ���� ���� �� ȣ��)
    /// </summary>
    public void Stun()
    {
        if (IsDead || isStunned) return; // �̹� �׾��ų� �̹� ���� ���¸� �ƹ��͵� ���� ����

        isStunned = true; // ���� ���� �÷��׸� true�� ����
        PlayStunAnim();   // ���� �ִϸ��̼� ���

        // ������ ���� ���� ���� �ڷ�ƾ�� �ִٸ� �����ϰ� ���ο� �ڷ�ƾ�� �����մϴ�.
        // �̴� �ߺ� �����̳� ���� �ð� �ʱ�ȭ�� �����մϴ�.
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    /// <summary>
    /// ������ �ð� �Ŀ� ���� ���¸� �����ϴ� �ڷ�ƾ�Դϴ�.
    /// </summary>
    /// <param name="duration">���� ���� �ð�.</param>
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration); // ������ �ð���ŭ ���

        if (!IsDead) // ��� �ð� �Ŀ��� ���� ���� �ʾҴٸ� ���� ���¸� ����
        {
            isStunned = false;
        }
    }

    /// <summary>
    /// ��� �ִϸ��̼��� ����մϴ�.
    /// </summary>
    protected override void PlayIdleAnim()
    {
        // ���, ����, �ǰ� ���°� �ƴϸ� �ȱ� �ִϸ��̼��� ���� ��� ���·� ����ϴ�.
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, false);
    }

    /// <summary>
    /// �ȱ� �ִϸ��̼��� ����մϴ�.
    /// '�̵� ���'�� �ִϸ��̼� Ŀ�긦 ����ϵ��� �ִϸ������� Bool �Ķ���͸� �����մϴ�.
    /// ���� �̵� �ӵ� ����� �ִϸ�����(Root Motion)�� �ִϸ��̼� �̺�Ʈ,
    /// �Ǵ� �ִϸ��̼� Ŀ�꿡�� ������ ���� ���� �̷�����ϴ�.
    /// </summary>
    protected override void PlayWalkAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, true);
    }

    /// <summary>
    /// ��� �ִϸ��̼��� ����մϴ�.
    /// </summary>
    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_MOB02_DEATH);
    }

    /// <summary>
    /// �ǰ� �ִϸ��̼��� ����մϴ�.
    /// </summary>
    protected override void PlayHurtAnim()
    {
        // ���, ���� ���°� �ƴϸ� �ǰ� �ִϸ��̼��� Ʈ�����մϴ�.
        if (!IsDead && !isStunned && animator != null)
        {
            isPerformingHurtAnimation = true; // �ǰ� �ִϸ��̼� ������ ��Ÿ���� �÷��� ����
            animator.SetTrigger(ANIM_TRIGGER_MOB02_HURT); // �ǰ� �ִϸ��̼� Ʈ����
        }
    }

    /// <summary>
    /// ���� �ִϸ��̼��� ����մϴ�. (Mob02 ������ ���� �ִϸ��̼� ��� �Լ�)
    /// </summary>
    protected void PlayStunAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_STUN);
        }
    }

    /// <summary>
    /// '��������(�Ϲ�)' ���� �ִϸ��̼��� ����մϴ�. (�θ� Ŭ������ PlayAttack1Anim �������̵�)
    /// '���� ���'�� �ִϸ��̼� Ŀ�긦 ����ϵ��� �ִϸ������� Trigger �Ķ���͸� �����մϴ�.
    /// ���� �� Ư�� ������ �̵��̳� ��Ʈ�ڽ� Ȱ��ȭ/��Ȱ��ȭ�� �ִϸ��̼� �̺�Ʈ�� ���� ����˴ϴ�.
    /// </summary>
    protected override void PlayAttack1Anim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_ATTACK_A); // '��������' ���� �ִϸ��̼� Ʈ����
        }
    }

    // ���02�� ���� ���� �����̹Ƿ� �ٸ� ���� �ִϸ��̼��� �ʿ� �����ϴ�.
    protected override void PlayAttack2Anim() { }
    protected override void PlayAttack3Anim() { }

    /// <summary>
    /// ��� ���� ���� �ִϸ��̼� Ʈ���Ÿ� �����մϴ�.
    /// </summary>
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
    /// ���02�� ���� ������ �����մϴ�.
    /// ��ٿ��� Ȯ���ϰ� �÷��̾ ���� ���� ���� ������ '��������' ������ �����մϴ�.
    /// </summary>
    protected override void PerformAttackLogic()
    {
        // �̹� �ٸ� ����(����, ����, �ǰ� ��, ���� ��, ���� �� ��� ��)�̸� �������� �ʽ��ϴ�.
        // �� üũ�� Update() �޼����� ���� �κа� base.Update() ������ �̹� ó���˴ϴ�.

        // ���� ���ݱ����� ��ٿ��� �Ǿ����� Ȯ��
        bool cooldownReady = Time.time >= nextAttackTime;

        // �÷��̾ ���� ���� ���� �ִ��� Ȯ��
        if (playerTransform == null || Vector2.Distance(transform.position, playerTransform.position) > attackRange)
        {
            // �÷��̾ ���� ���� �ۿ� ������ ���� �õ����� �ʰ� �̵� �������� ���ư��ϴ�.
            return;
        }

        if (!cooldownReady)
        {
            return; // ��ٿ� ���̸� ���� ��ŵ
        }

        // ��ٿ��� ������ �÷��̾ ���� ���� ���� ������ ���� ����
        isPerformingAttackAnimation = true; // ���� �ִϸ��̼� ������ ��Ÿ���� �÷��� ����
        PlayAttack1Anim(); // '��������' ���� �ִϸ��̼� ��� (�� �ȿ� ANIM_TRIGGER_MOB02_ATTACK_A Ʈ���� ����)
    }

    /// <summary>
    /// ���� �ִϸ��̼��� ����� �� �ִϸ��̼� �̺�Ʈ�� ȣ��˴ϴ�.
    /// </summary>
    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd(); // �θ� Ŭ������ ���� ȣ�� (isPerformingAttackAnimation �÷��� ����, ���� �� ��� �ڷ�ƾ ����)

        // ���02 ������ ���� ���� ���� �ð� ���� (���� �ִϸ��̼� ���� �� 1�� ��)
        nextAttackTime = Time.time + attackACooldown;
    }

    /// <summary>
    /// �ǰ� �ִϸ��̼��� ����� �� �ִϸ��̼� �̺�Ʈ�� ȣ��˴ϴ�.
    /// </summary>
    public override void OnHurtAnimationEnd()
    {
        base.OnHurtAnimationEnd(); // isPerformingHurtAnimation �÷��� ����
    }

    /// <summary>
    /// '��������' ������ ��Ʈ�ڽ��� Ȱ��ȭ�մϴ�. (�ִϸ��̼� �̺�Ʈ�� ȣ��)
    /// </summary>
    public void EnableAttackHitbox()
    {
        if (IsDead) return; // ��� ���¿����� ��Ʈ�ڽ� Ȱ��ȭ���� ����

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue; // ���ݷ� ����
            }
            else
            {
                Debug.LogWarning("Mob02MeleeController: EnemyHitbox ������Ʈ�� 'Attack A Hitbox Object'�� �����ϴ�!", attackAHitboxObject);
            }
            attackAHitboxCollider.enabled = true; // �ݶ��̴� Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object' �Ǵ� �ݶ��̴��� �Ҵ���� �ʾҰų� ã�� �� �����ϴ�. ��Ʈ�ڽ� Ȱ��ȭ ����.", this);
        }
    }

    /// <summary>
    /// '��������' ������ ��Ʈ�ڽ��� ��Ȱ��ȭ�մϴ�. (�ִϸ��̼� �̺�Ʈ�� ȣ��)
    /// </summary>
    public void DisableAttackHitbox()
    {
        if (IsDead) return; // ��� ���¿����� ��Ʈ�ڽ� ��Ȱ��ȭ���� ���� (�ǹ� ����)

        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false; // �ݶ��̴� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Collider'�� �Ҵ���� �ʾҰų� ã�� �� �����ϴ�. ��Ʈ�ڽ� ��Ȱ��ȭ ����.", this);
        }
    }

    /// <summary>
    /// ���� �ð����� ����(��������Ʈ)�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="faceLeft">������ �ٶ���� �ϸ� true, �������� �ٶ���� �ϸ� false.</param>
    protected override void Flip(bool faceLeft)
    {
        // 'Sprite'��� �̸��� �ڽ� ������Ʈ�� ã�� �������� �����մϴ�.
        // ��κ��� ��� ��������Ʈ�� ���� ���� ������Ʈ�� �ڽ����� �����մϴ�.
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform; // 'Sprite' �ڽ� ������Ʈ�� ������ ���� ������Ʈ(��Ʈ ������Ʈ)�� Transform�� ���
            Debug.LogWarning(gameObject.name + ": 'Sprite' �ڽ� ������Ʈ�� ã�� �� �����ϴ�. ���� ������Ʈ�� Transform�� ����Ͽ� �����⸦ �õ��մϴ�.", this);
        }

        Vector3 currentScale = spriteToFlip.localScale;

        // �� ������ 'faceLeft'�� true�� �� (������ �ٶ���� �� ��) localScale.x�� ������,
        // 'faceLeft'�� false�� �� (�������� �ٶ���� �� ��) localScale.x�� ����� ����ϴ�.
        // �߿�: ���� ��������Ʈ�� �⺻ ����(��: �������� �ٶ� �� scale.x�� ������� ��������)�� ����
        // Mathf.Abs(currentScale.x)�� ���ϴ� ��ȣ(+ �Ǵ� -)�� �����ؾ� �� �� �ֽ��ϴ�.
        if (faceLeft)
        {
            spriteToFlip.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else
        {
            spriteToFlip.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    /// <summary>
    /// ���� ������ �÷��̾��� Transform�� �����մϴ�. (PlayerDetector �� �ܺ� ��ũ��Ʈ���� ȣ��)
    /// </summary>
    /// <param name="newPlayerTransform">�÷��̾��� Transform ������Ʈ.</param>
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