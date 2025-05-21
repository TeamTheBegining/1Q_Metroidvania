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

    // --- �ִϸ��̼� Ŀ�� �̸� ����� �� �̻� Animator Layer���� ���� ���� �����Ƿ� �ʿ� ���� ---
    // private const string ANIM_CURVE_ATTACK_FORWARD_SPEED = "AttackForwardSpeed";
    // private const string ANIM_CURVE_ATTACK_BACKWARD_SPEED = "AttackBackwardSpeed";

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

    [Header("���02 ���� �� �̵� ����")]
    [Tooltip("�÷��̾ �����ϴ� �����Դϴ�.")]
    public new float detectionRange = 7f;
    [Tooltip("�÷��̾ �������� �� ���� �ӵ��Դϴ�.")]
    public float chaseSpeed = 3f;

    // --- �ν����Ϳ��� ���� ������ AnimationCurve ���� �߰� ---
    [Header("���� �ִϸ��̼� Ŀ�� ����")]
    [Tooltip("���� �� ���� �̵� �ӵ��� �ð��� ���� �����ϴ� Ŀ���Դϴ�. X���� �ִϸ��̼� ���൵ (0~1), Y���� �ӵ� ����.")]
    public AnimationCurve attackForwardMovementCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1), new Keyframe(0.5f, 0));
    [Tooltip("���� �� ���� �̵� �ӵ��� �ð��� ���� �����ϴ� Ŀ���Դϴ�. X���� �ִϸ��̼� ���൵ (0~1), Y���� �ӵ� ����.")]
    public AnimationCurve attackBackwardMovementCurve = new AnimationCurve(new Keyframe(0.5f, 0), new Keyframe(0.7f, 1), new Keyframe(1, 0));


    [Tooltip("���� �ִϸ��̼� �� �ִ� ���� �ӵ� �����Դϴ�.")]
    public float attackForwardSpeedMultiplier = 1.5f;
    [Tooltip("���� �ִϸ��̼� ���� �� ���� �ӵ� �����Դϴ�. (���� ������ ���� �� ����)")]
    public float attackBackwardSpeedMultiplier = 1f;

    // ����(Stun) ���� ���� ����
    private bool isStunned = false;
    [Header("���02 ���� ����")]
    [Tooltip("�÷��̾��� �и� ���� � ���� �����Ǿ��� �� ���ӵǴ� �ð��Դϴ�.")]
    public float stunDuration = 2f;

    private Coroutine stunCoroutine;

    // --- �߰��� ����: SpriteRenderer ���� ---
    private SpriteRenderer spriteRenderer;

    // --- �߰��� ����: �÷��̾� ���� �� ���� ���� ---
    private bool isPlayerDetected = false;
    private Vector2 lastKnownPlayerPosition;

    // --- ���� �� ����/���� ������ ���� �߰� ���� ---
    private bool isAttackingForward = false;
    private bool isAttackingBackward = false;

    // --- �߰��� ����: �ִϸ��̼� ���൵ ���� ---
    private float attackAnimationProgress = 0f;
    private float currentAttackAnimationLength = 0f; // ���� ���� �ִϸ��̼��� ���� (Ŭ�� ���� �ʿ�)


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

        // SpriteRenderer ������Ʈ ���� Ȯ�� �� �ʱ�ȭ
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
            Debug.LogError("Mob02MeleeController: SpriteRenderer ������Ʈ�� ã�� �� �����ϴ�. ��������Ʈ ������(Flip)�� ���������� �۵����� ���� �� �ֽ��ϴ�.", this);
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
            lastKnownPlayerPosition = playerGameObject.transform.position;
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
        isPlayerDetected = false;
        nextAttackTime = Time.time;

        // --- ó�� ������ �� ������ ������ �߰� (Flip �޼��� ȣ��) ---
        // ���� ���� �����Ǵ� ������ �������̶��, �������� �������ϴ�.
        Flip(true); // ó�� ������ �� ������ ������
        Debug.Log($"Mob02 Start: Initial Flip to Left applied. Current localScale.x: {transform.Find("Sprite")?.localScale.x ?? transform.localScale.x}");
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
            PlayIdleAnim(); // �������� �� ���̵� �ִϸ��̼� ���
            return; // ���� �������� ��� ���� ������ �ǳʶݴϴ�.
        }

        // 2. ���� �ִϸ��̼��� Ȱ��ȭ�� ���� (����/���� ��� ����):
        if (isPerformingAttackAnimation)
        {
            // ���� ��� ���� �ִϸ��̼� Ŭ���� ���̸� �����ɴϴ�. (���� 1ȸ�� ����)
            if (currentAttackAnimationLength == 0f && animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.IsName("Mob02_AttackA")) // Animator State�� �̸����� üũ
                {
                    AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
                    if (clipInfo.Length > 0)
                    {
                        // Ŭ���� �̸��� ��������� Ȯ���Ͽ� �ش� Ŭ���� ���̸� �����ɴϴ�.
                        // �� �κ��� �ִϸ��̼� Ŭ���� ���� �̸��� ����ؾ� �մϴ�.
                        // ��: animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Mob02_Attack_Animation_Clip"
                        currentAttackAnimationLength = clipInfo[0].clip.length;
                    }
                }
            }

            if (currentAttackAnimationLength > 0)
            {
                attackAnimationProgress += Time.deltaTime / currentAttackAnimationLength;
                attackAnimationProgress = Mathf.Clamp01(attackAnimationProgress); // 0�� 1 ���̷� Ŭ����
                ApplyAttackAnimationMovement(); // ���� �ִϸ��̼ǿ� ���� �̵� ���� (���� �Ǵ� ����)
            }
            return; // ���� �������� ��� ���� ������ �ǳʶݴϴ�.
        }
        else
        {
            // ���� �ִϸ��̼��� �ƴ� ���� ���൵�� ���̸� �ʱ�ȭ
            attackAnimationProgress = 0f;
            currentAttackAnimationLength = 0f;
        }

        // 3. ���� �� ��ٿ� ��� ����:
        //    ���� �ִϸ��̼��� ���� �� ���� ���ݱ��� ��ٿ� �ð� ���� ��� ���¸� �����մϴ�.
        if (isWaitingAfterAttack)
        {
            PlayIdleAnim(); // ��� �ִϸ��̼� ���
            if (rb != null) rb.linearVelocity = Vector2.zero; // ���ڸ��� �����ֵ��� ��
            return; // ���� �������� ��� ���� ������ �ǳʍ��ϴ�.
        }

        // 4. �÷��̾� ���� �� ����/���� ����
        CheckPlayerDetection(); // <-- ���⼭ isPlayerDetected ���¸� ������Ʈ�մϴ�.

        if (isPlayerDetected)
        {
            ChasePlayer();
            PerformAttackLogic(); // ���� ������ ChasePlayer()�� ���������� ȣ��
        }
        else // �÷��̾ �������� �ʾ��� ��
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
    /// �÷��̾� ���� ���θ� Ȯ���ϴ� �Լ� (�߰���)
    /// </summary>
    private void CheckPlayerDetection()
    {
        if (playerTransform == null) return;

        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ���� ���� ���� �÷��̾ �ִ��� Ȯ��
        isPlayerDetected = distanceToPlayer <= detectionRange;

        // �÷��̾ �����Ǹ� ������ ��ġ ������Ʈ (�̰��� ���� �ڵ忡���� ������ ������ ������ �� �ֽ��ϴ�)
        if (isPlayerDetected)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }
    }

    /// <summary>
    /// �÷��̾ �����ϴ� �Լ� (�߰���)
    /// </summary>
    private void ChasePlayer()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ���� ���� �ȿ� �ְų� ���� ���� �ٷ� �ٱ� (���� ����)�� ������ ����
        // CommonEnemyController�� attackStopBuffer�� �״�� ���
        if (distanceToPlayer <= attackRange + attackStopBuffer)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            PlayIdleAnim(); // �������� �� ���̵� �ִϸ��̼� ���
            return; // ���� ���� �������� �� �̻� �������� ����
        }

        // �÷��̾� �������� �̵�
        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        // ���⿡ ���� ��������Ʈ ������
        Flip(direction.x < 0);

        // �̵� �ӵ� ���� (���� ��忡���� chaseSpeed ���)
        Vector2 velocity = direction * chaseSpeed;

        // y�� �ӵ��� ���� �ӵ� ���� (�߷� �� ���� �޵���)
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
        }

        // �ȱ� �ִϸ��̼� ���
        PlayWalkAnim();
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
            // ���� ���� �� ���� �ܰ�� �����ϰ� ���൵ �ʱ�ȭ
            isAttackingForward = true;
            isAttackingBackward = false;
            attackAnimationProgress = 0f; // �ִϸ��̼� ���൵ �ʱ�ȭ
            currentAttackAnimationLength = 0f; // �ִϸ��̼� ���̸� �ٽ� ���������� �ʱ�ȭ
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
    /// ���� �ִϸ��̼� �� �ִϸ��̼� Ŀ�� ���� �о� ���� �Ǵ� ���� �̵��� �����մϴ�.
    /// �� �޼���� 'Mob02MeleeController' ��ũ��Ʈ ���� 'attackForwardMovementCurve'�� 'attackBackwardMovementCurve'�� ����մϴ�.
    /// </summary>
    private void ApplyAttackAnimationMovement()
    {
        if (rb == null) return;

        float curveSpeedFactor = 0f;
        float actualMoveSpeed = 0f;
        float directionX = Mathf.Sign(transform.Find("Sprite")?.localScale.x ?? transform.localScale.x); // ���� �ٶ󺸴� ����

        // attackAnimationProgress�� ����Ͽ� AnimationCurve���� ���� ���ø��մϴ�.
        if (isAttackingForward) // ���� ���� �ܰ�
        {
            curveSpeedFactor = attackForwardMovementCurve.Evaluate(attackAnimationProgress);
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackForwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(directionX * actualMoveSpeed, rb.linearVelocity.y);
            // Debug.Log($"Attack Forward Movement: Direction={directionX}, Speed={actualMoveSpeed}, Curve={curveSpeedFactor}, Progress={attackAnimationProgress}");
        }
        else if (isAttackingBackward) // ���� ���� �ܰ�
        {
            curveSpeedFactor = attackBackwardMovementCurve.Evaluate(attackAnimationProgress);
            // �����̹Ƿ� ������ �ݴ�� ���մϴ�.
            actualMoveSpeed = moveSpeed * curveSpeedFactor * attackBackwardSpeedMultiplier;
            rb.linearVelocity = new Vector2(-directionX * actualMoveSpeed, rb.linearVelocity.y); // ���� ����
            // Debug.Log($"Attack Backward Movement: Direction={-directionX}, Speed={actualMoveSpeed}, Curve={curveSpeedFactor}, Progress={attackAnimationProgress}");
        }
        else
        {
            // ���� �ִϸ��̼��� Ȱ��ȭ�Ǿ� ������, ����/���� �ܰ谡 ��Ȯ���� ���� ��� (��: ���� ��� ������)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            PlayIdleAnim(); // �ӽ÷� ���̵� �ִϸ��̼� ���
        }
    }


    // --- AI ���� ���� ---

    protected override void PerformAttackLogic()
    {
        // ��ٿ� üũ
        bool cooldownReady = Time.time >= nextAttackTime;
        if (!cooldownReady) return;

        // �÷��̾ �ְ� ���� ���� ���� �ִ��� Ȯ��
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // ���� ���� ���� ������ �� ���� ����
        if (distanceToPlayer <= attackRange)
        {
            // ���� �ִϸ��̼� ����
            isPerformingAttackAnimation = true; // ���� �ִϸ��̼� ������ �˸�
            attackAnimationProgress = 0f; // ���� ���� �� ���൵ �ʱ�ȭ
            currentAttackAnimationLength = 0f; // �ִϸ��̼� ���̸� �ٽ� ���������� �ʱ�ȭ
            PlayAttack1Anim();
            // ���� �Ŀ��� isWaitingAfterAttack�� OnAttackAnimationEnd���� true�� �����˴ϴ�.
        }
    }

    // --- �ִϸ��̼� �̺�Ʈ �ݹ� �Լ� ---

    // �� �Լ����� �ִϸ��̼� Ŭ���� �̺�Ʈ�� ����Ǿ�� �մϴ�.
    // Mob02_AttackA �ִϸ��̼� Ŭ�� ������ ������ �����ӿ� ���� �̺�Ʈ���� �߰��ؾ� �մϴ�.

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        // ���� �ִϸ��̼��� ������ ���� �� ���� ���ݱ����� ��ٿ� �ð��� �����մϴ�.
        nextAttackTime = Time.time + attackACooldown;

        // ���� �ִϸ��̼��� �������Ƿ� ����/���� �÷��׸� �����ϰ� �̵� �����մϴ�.
        isAttackingForward = false;
        isAttackingBackward = false;
        isPerformingAttackAnimation = false; // ���� �ִϸ��̼��� ������ �������� �˸�
        attackAnimationProgress = 0f; // �ִϸ��̼� ���൵ �ʱ�ȭ
        currentAttackAnimationLength = 0f; // �ִϸ��̼� ���̵� �ʱ�ȭ
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // ���� ���� �� �ٷ� ����

        Debug.Log("OnAttackAnimationEnd: Attack animation finished.");
    }

    // ���Ӱ� �߰�: ���� �ִϸ��̼� �� ���� ���� ���� �� ȣ��
    public void OnAttackForwardStart()
    {
        isAttackingForward = true;
        isAttackingBackward = false;
        // attackAnimationProgress�� Update���� ��� ������Ű�Ƿ� ���⼭ �ʱ�ȭ�� �ʿ� ����
        Debug.Log("Mob02: OnAttackForwardStart called.");
    }

    // ���Ӱ� �߰�: ���� �ִϸ��̼� �� ���� ���� ���� �� ȣ�� (�Ǵ� ���� ���� �� ���� ����)
    public void OnAttackBackwardStart()
    {
        isAttackingForward = false;
        isAttackingBackward = true;
        // attackAnimationProgress�� Update���� ��� ������Ű�Ƿ� ���⼭ �ʱ�ȭ�� �ʿ� ����
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
                Debug.LogWarning("Mob02MeleeController: EnemyHitbox ������Ʈ�� 'Attack A Hitbox Object'�� �����ϴ�!", attackAHitboxObject);
            }
            attackAHitboxCollider.enabled = true;
            Debug.Log("Mob02: Attack Hitbox Enabled.");
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
            Debug.Log("Mob02: Attack Hitbox Disabled.");
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
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
        }

        float desiredSign = faceLeft ? -1f : 1f;
        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);

        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);

        // SpriteRenderer ��� ����� �����ϰ� Transform.localScale ��ĸ� ���
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

    // --- ����� �̿��� ����� �ð�ȭ ---
    private void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ (�ʷϻ�)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // ���� ���� �ð�ȭ (������)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // --- CommonEnemyController�� �̵� ���� �߻� �޼��� �Ǵ� �������̵� ������ �޼��� ���� ---
    protected override void MoveTowardsPlayer()
    {
        // Mob02MeleeController���� ChasePlayer()�� ���� ����ϹǷ� �� �Լ��� ������ �ʽ��ϴ�.
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