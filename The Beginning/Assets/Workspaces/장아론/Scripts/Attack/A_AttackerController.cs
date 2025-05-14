using System;
using UnityEngine;
using System.Collections;

// A_Attacker (��� - �ٰŸ�) ĳ������ ������ �����ϴ� ��Ʈ�ѷ�
// CommonEnemyController�� ��ӹ޽��ϴ�.
public class A_AttackerController : CommonEnemyController, IDamageable
{
    // A_Attacker ������ �ִϸ��̼� �Ķ���� �̸�
    private const string ANIM_BOOL_A_WALK = "A_Walk";
    //private const string ANIM_TRIGGER_A_JUMP = "A_Jump"; // ���� ���ٰ� ����
    private const string ANIM_TRIGGER_A_ATTACK_A = "A_AttackA"; // A ���� (����)
    // --- �߰�: �ǰ� �ִϸ��̼� Ʈ���� ---
    private const string ANIM_TRIGGER_A_HURT = "A_Hurt"; // �ǰ� �ִϸ��̼�
    // ------------------------------------
    private const string ANIM_TRIGGER_A_STUN = "A_Stun"; // ���� �ִϸ��̼�
    private const string ANIM_TRIGGER_A_DEATH = "A_Death"; // ��� �ִϸ��̼� (�ʿ��)

    [Header("A_Attacker Hitbox")]
    public GameObject attackAHitboxObject; // A ���� ��Ʈ�ڽ� ������Ʈ

    // ��Ʈ�ڽ� ������Ʈ�� �پ��ִ� ������Ʈ ����
    private BoxCollider2D attackAHitboxCollider;
    private EnemyHitbox attackAEnemyHitbox; // EnemyHitbox ��ũ��Ʈ ��� ����

    // A_Attacker ���� (�̹��� ���)
    [Header("A_Attacker Stats")]
    private float maxhp = 5f; // �ִ� ü�� 5
    [SerializeField] private float currentHp = 5f; // ���� ü�� 5
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxhp; set => maxhp = value; }
    public bool IsDead { get; private set; } = false; // ��� �÷���

    [Header("A_Attacker Combat")]
    public float attackAValue = 0.5f; // A ���ݷ� 0.5
    public float attackACooldown = 3f; // A ���� ��Ÿ�� 3��

    // ��Ÿ�� üũ
    private float nextAttackTime = 0f; // ���� ���� ���� �ð�

    // ���� ���� �÷��� (CommonEnemyController�� ���¿� ������ ������ �� �ִ� ���� ����)
    private bool isStunned = false;
    // ���� �ð� ���� (�ʿ��)
    private float stunDuration = 2f;
    public float StunDuration { get => stunDuration; set => stunDuration = value; }

    public Action OnDead { get; set; }

    // Damageable �������̽� ����
    public void TakeDamage(float damage , GameObject player)
    {
        // �̹� �׾����� ó�� ����
        if (IsDead) return;

        // ���� �߿��� �������� ������, �߰� ����/�ǰ� ������ ���� (�ʿ�� ���� ����)
        if (isStunned)
        {
            currentHp -= damage; // ���� �߿��� �������� ��
                                 //Debug.Log("A_Attacker ���� �� �ǰ�! ü��: " + currentHp);
            if (currentHp <= 0) Die(); // ���� �߿� �¾Ƶ� ü�� 0 ���ϸ� ���
            return; // �߰� �ǰ� �ִϸ��̼�/���� Ʈ���� ����
        }

        // ���� ���°� �ƴ� �� ������ ó��
        currentHp -= damage; // ü�� ����
        //Debug.Log("A_Attacker �ǰ�! ü��: " + currentHp);

        // TODO: �и� ���� ���� �߰� �ʿ� - ���� TakeDamage �Լ������δ� �и� ���� �Ǵ� �Ұ�.
        // �и� ���� �� ȣ��� �ܺ� �Լ�(��: PlayerCombat ��ũ��Ʈ)���� Stun()�� ȣ���ϴ� ���� �Ϲ����Դϴ�.
        // Ȥ�� ������ ������ 'isParryAttack' ���� bool ���� �߰��Ͽ� ���޹޴� ��ĵ� �ֽ��ϴ�.
        // ����: if (damageInfo.isParryAttack) { Stun(stunDuration); return; } // �и� �����̸� ���� �ɰ� �Լ� ����

        // ü���� 0 ���� �� ��� ó��
        if (currentHp <= 0)
        {
            //Debug.Log("A_Attacker ü�� 0 ����! ��� ó�� ����.");
            Die();
        }
        // ü���� 0���� ũ��, ���� ���°� �ƴ϶�� �Ϲ� �ǰ� �ִϸ��̼� �ߵ�
        else // currentHp > 0 && !isStunned
        {
            PlayHurtAnim(); // �ǰ� �ִϸ��̼� ��� �Լ� ȣ��
        }
    }

    // --- �߰�: ���� ���� ó�� �Լ� ---
    // �÷��̾��� �и� ���� ���� �� �ܺο��� ȣ��� �� �ֽ��ϴ�.
    public void Stun()
    {
        // �̹� �׾��ų� ���� ���̸� �ٽ� �������� ����
        if (IsDead || isStunned) return;

        isStunned = true;
        //Debug.Log("A_Attacker ���� ���� ����!");

        // TODO: ���� �� AI ���� �� �̵� ����
        // CommonEnemyController�� AI ���¸� Stun ���·� �����ϰų�, Update ��� isStunned üũ
        // ��: SetState(EnemyState.Stunned); // CommonEnemyController�� Stunned ���°� ���ǵǾ� �ִٸ�
        // �׺���̼� ������Ʈ ��� ��: navMeshAgent.isStopped = true;
        // ���� �ִϸ��̼� ���̶�� �ߴ��ϰ� ���� �ִϸ��̼����� ��ȯ
        if (animator != null) animator.SetTrigger(ANIM_TRIGGER_A_STUN);

        // ���� �ð� �� ���� ���� �ڷ�ƾ ����
        StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    IEnumerator ReleaseStunCoroutine(float duration)
    {
        // ���� �ִϸ��̼� ���̸�ŭ ��ٸ� �� �߰� ��� (�ʿ��)
        // yield return new WaitForSeconds(GetStunAnimationLength()); // �ִϸ��̼� ���̸� �˸�
        yield return new WaitForSeconds(duration); // �ܼ��� ������ �ð���ŭ ���

        // ���� ���°� �ƴϸ� ���� ����
        if (!IsDead)
        {
            isStunned = false;
            //Debug.Log("A_Attacker ���� ���� ����!");

            // TODO: ���� ���� �� AI ���� �簳
            // ��: SetState(EnemyState.Chase); // CommonEnemyController�� �⺻ ���·� ����
            // �׺���̼� ������Ʈ ��� ��: navMeshAgent.isStopped = false;

            // TODO: �ִϸ����� ���� ��ȯ (���� ���� �� Idle/Walk ������)
            // Animator Controller���� Stun -> Idle/Walk ������ ���� Transition�� �����ϴ� ���� �Ϲ����Դϴ�.
        }
    }
    // ------------------------------


    // --- �߰�: ��� ó�� �Լ� (IDamageable ���� �Ϻ�) ---
    void Die()
    {
        if (IsDead) return; // �̹� ���� ó�� ���̸� �ߺ� ����

        IsDead = true; // ���� ���� �÷��� ����
        //Debug.Log("A_Attacker ���!");

        // ���� �ڷ�ƾ ���� (��� �� ���� ���� ������ ������� �ʵ���)
        StopAllCoroutines();
        isStunned = false; // ��������� ���� ���µ� ����

        // ��� �ִϸ��̼� ���
        PlayDeathAnim();

        // TODO: ��� �� �ʿ��� �߰� ���� ���� (B_GirlController�� ����)
        // - Collider ��Ȱ��ȭ (�ǰ� �� ���� ���� ����)
        Collider2D mainCollider = GetComponent<Collider2D>();
        if (mainCollider != null) mainCollider.enabled = false;

        // - Rigidbody ���� ���� (������ ���� ��)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // ������ ��� ���� (Unity 2019.3+ ����)
            // rb.velocity = Vector2.zero; // ���� ����
            rb.isKinematic = true; // ���� ȿ�� ��Ȱ��ȭ (�ִϸ��̼��� �������� ������ ���)
        }

        // - AI ��ũ��Ʈ ��Ȱ��ȭ (�� �̻� ����/���� ���� ���� �ʵ���)
        // ��: this.enabled = false; // ��ũ��Ʈ ��ü ��Ȱ��ȭ (Update �� ����)
        // CommonEnemyController�� AI ���� ���� ������Ʈ�� �ִٸ� �װ��� ��Ȱ��ȭ

        // TODO: ���� �Ŵ��� � ���ʹ� ����� �˸��� �̺�Ʈ ȣ�� �Ǵ� ó��
        // ��: GameManager.Instance.EnemyDied(this);

        // - ���� �ð� �� ���� ������Ʈ ���� �Ǵ� ������Ʈ Ǯ ��ȯ
        // ��: Destroy(gameObject, 3f); // 3�� �� �ı� (���� �ִϸ��̼� ���̿� �°� ����)
    }
    // --------------------------------------------


    protected override void Start()
    {
        base.Start(); // CommonEnemyController�� Start ȣ�� (Animator, Player ���� ���� ��)

        // ��Ʈ�ڽ� ������Ʈ �� ��ũ��Ʈ ���� �ʱ�ȭ
        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>(); // EnemyHitbox ������Ʈ ����
            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false; // ���� �� �ݶ��̴� ��Ȱ��ȭ
            }
            else
            {
                //Debug.LogWarning("Attack A Hitbox Object�� BoxCollider2D ������Ʈ�� �����ϴ�.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                //Debug.LogWarning("Attack A Hitbox Object�� EnemyHitbox ������Ʈ�� �����ϴ�!", this);
            }
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Object�� A_AttackerController �ν����Ϳ� �Ҵ���� �ʾҽ��ϴ�.", this);
        }

        // ���� �ʱ�ȭ
        currentHp = maxhp; // ü�� �ʱ�ȭ
        IsDead = false;
        isStunned = false; // ���� ���� �ʱ�ȭ
        nextAttackTime = Time.time; // �Ǵ� 0f; // ��Ÿ�� �ʱ�ȭ
    }

    // Update �������̵�: �װų� ���� ������ �� �⺻ AI ���� ��ŵ
    protected override void Update()
    {
        if (IsDead || isStunned)
        {
            // �׾��ų� ���� ���̸� AI ���� ���� ����
            // ���� �� �ִϸ��̼� ��� �� �ٸ� ó���� �ʿ��ϸ� ���⿡ �߰�
            // ���� �ִϸ��̼��� Stun()���� �̹� Ʈ�����ϹǷ� ���⼭ Ư���� �� �� ���� �� ����
            return;
        }

        base.Update(); // CommonEnemyController�� Update ȣ�� (�̵�, ���� ��ȯ ��)
    }


    // ===== �ִϸ��̼� ���� �Լ��� (Base Ŭ������ virtual �޼ҵ带 �������̵�) =====

    protected override void PlayIdleAnim()
    {
        // �װų� ���� ���� �ƴ� ���� ���
        if (!IsDead && !isStunned && animator != null) animator.SetBool(ANIM_BOOL_A_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        // �װų� ���� ���� �ƴ� ���� ���
        if (!IsDead && !isStunned && animator != null) animator.SetBool(ANIM_BOOL_A_WALK, true);
    }

    protected override void PlayJumpAnim()
    {
        // A_Attacker�� ���� ���ٰ� ����
        // base.PlayJumpAnim(); // CommonEnemyController�� ���� ������ �ִٸ� �ּ� ����
        // if (!IsDead && !isStunned && animator != null) animator.SetTrigger(ANIM_TRIGGER_A_JUMP); // ���� Ʈ���� �ִٸ�
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null) animator.SetTrigger(ANIM_TRIGGER_A_DEATH);
    }

    // --- �߰�: �ǰ� �ִϸ��̼� ��� �Լ� ---
    // TakeDamage �Լ����� ȣ��˴ϴ�.
    protected void PlayHurtAnim()
    {
        // �װų� ���� ���� �ƴ� ���� ���
        if (!IsDead && !isStunned && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_HURT); // �ǰ� �ִϸ��̼� Ʈ���� �ߵ�!
                                                      //Debug.Log("PlayHurtAnim ȣ��");
        }
    }
    // -----------------------------------

    // --- �߰�: ���� �ִϸ��̼� ��� �Լ� (Stun()���� ȣ��) ---
    // Protected�� �����Ͽ� ��ӹ��� Ŭ���������� ���� �����ϰ� �� �� �ֽ��ϴ�.
    protected void PlayStunAnim()
    {
        // �̹� �׾��ٸ� ��� ���� (Stun()���� �̹� üũ������ ���� ��ġ)
        if (IsDead) return;
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_STUN);
            //Debug.Log("PlayStunAnim ȣ��");
        }
    }
    // ----------------------------------------------------


    // --- A ���� �ִϸ��̼� ��� �Լ� �������̵� ---
    // CommonEnemyController�� PlayAttack1Anim, PlayAttack2Anim �� �� �ϳ��� �������̵��մϴ�.
    // ���⼭�� PlayAttack1Anim�� ����Ͽ� A ���� �ִϸ��̼��� �ߵ���ŵ�ϴ�.
    protected override void PlayAttack1Anim() // CommonEnemyController�� PlayAttack1Anim ���� �Լ��� �ִٰ� ����
    {
        // �װų� ���� ���� �ƴ� ���� ���� �ִϸ��̼� ��� �õ�
        if (!IsDead && !isStunned && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_ATTACK_A);
            //Debug.Log("A_AttackA �ִϸ��̼� �ߵ�!");
        }
    }

    // CommonEnemyController�� PlayAttack2Anim �� �ٸ� ���� �Լ��� A_Attacker���� ������� �����Ƿ� �������̵����� �ʰų� �⺻ ������ �Ӵϴ�.
    /*
    protected override void PlayAttack2Anim()
    {
        // A_Attacker�� Attack 2�� �����Ƿ� ����ΰų� base ȣ��
        // base.PlayAttack2Anim();
    }
    */


    // Attack Ʈ���� ���� �Լ�
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_A_ATTACK_A); // A ���� Ʈ���� ����
            animator.ResetTrigger(ANIM_TRIGGER_A_HURT); // �ǰ� Ʈ���� ����
            animator.ResetTrigger(ANIM_TRIGGER_A_STUN); // ���� Ʈ���� ����
                                                        // CommonEnemyController�� ResetAttackTriggers�� �ٸ� Ʈ���� ������ �ִٸ� base ȣ��
                                                        // base.ResetAttackTriggers();
        }
    }

    // ===== AI ���� ���� (Base Ŭ������ virtual PerformAttackLogic �������̵�) =====

    protected override void PerformAttackLogic()
    {
        // �̹� ���� �ִϸ��̼� ���̰ų� �װų� ���� ���̶�� �ߺ� �ߵ� ����
        if (isPerformingAttackAnimation || IsDead || isStunned) return;

        // ��Ÿ���� �������� üũ
        if (Time.time < nextAttackTime)
        {
            // ��Ÿ�� ��� ��
            return;
        }

        // ��Ÿ���� �����ٸ� ���� ����
        //Debug.Log("A_Attacker ���� ����! A_AttackA �ߵ� �õ�.");

        isPerformingAttackAnimation = true; // ���� �ִϸ��̼� ���� �÷��� �� (Base Class ����)

        // A ���� �ִϸ��̼��� �ߵ���Ű�� ���� PlayAttack1Anim �Լ��� ȣ���մϴ�.
        PlayAttack1Anim(); // <-- PlayAttackAnim() ��� PlayAttack1Anim() ȣ��

        // nextAttackTime ������ Animation Event �ݹ� �Լ�(OnAttackAnimationEnd)���� ����
    }

    // ===== Animation Event Callbacks (Base Ŭ������ virtual �޼ҵ� �������̵�) =====

    // --- ���� �ִϸ��̼� ���� �� ȣ��� �ݹ� �Լ� �������̵� ---
    protected override void OnAttackAnimationEnd()
    {
        // �⺻ Ŭ������ ���� ȣ�� (isPerformingAttackAnimation = false ���� ��)
        base.OnAttackAnimationEnd();
        //Debug.Log("A_Attacker ���� �ִϸ��̼� ����! ���� ���� ���� �ð� ���.");

        // A ���� ��Ÿ�� ��� (�ִϸ��̼� ���� �������� ����)
        nextAttackTime = Time.time + attackACooldown;
        //Debug.Log("--> A_AttackA ����. ���� ������ " + attackACooldown.ToString("F2") + "�� �� (" + nextAttackTime.ToString("F2") + "�� ����).");

        // TODO: �ִϸ��̼� ���� �� AI ���� ��ȯ ���� �߰� (��: Chase ���·� ���ư���)
        // ��: SetState(EnemyState.Chase); // CommonEnemyController�� Chase ���°� �ִٸ�
    }

    // --- A ���� ��Ʈ�ڽ� Ȱ��ȭ �޼ҵ� (Animation Event���� ȣ��) ---
    // Animation Event���� EnableAttack1Hitbox �Ǵ� EnableAttack2Hitbox ���� ȣ���ϵ��� �����Ǿ� ���� ���Դϴ�.
    // A_Attacker�� ������ �ϳ��̹Ƿ�, A ���� �ִϸ��̼��� Animation Event���� �� �Լ��� ȣ���ϵ��� �����մϴ�.
    // �Լ� �̸��� ���� B_GirlController�� ��Ī�� ��������, A_Attacker�� ��Ʈ�ڽ� ������ �����մϴ�.
    // (���� Animation Event���� PlayAttack1Anim �Լ�ó�� EnableAttack1Hitbox�� ȣ���ϵ��� �����Ǿ� �ִٸ� �� �Լ��� ���)
    public void EnableAttackAHitbox() // Animation Event �ñ״�ó�� ��ġ (�Ű����� ����)
    {
        // B_GirlController�� EnableAttack1Hitbox �Լ��� �������̵��ϴ� ���� �ƴϹǷ�,
        // Animation Event ���� �� �Լ� �̸��� �� �̸�(EnableAttackAHitbox)���� ���� �����ؾ� �� �� �ֽ��ϴ�.
        // Ȥ�� CommonEnemyController�� virtual public void EnableAttackHitbox(int attackIndex) ���� �Լ��� �ִٸ�
        // �ش� �Լ��� �������̵��Ͽ� attackIndex�� ���� �ٸ� ������ �����ϵ��� ������ ���� �ֽ��ϴ�.
        // ���⼭�� �������� public �Լ��� �����մϴ�.

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            // --- ��Ʈ�ڽ� ������Ʈ�� EnemyHitbox ��ũ��Ʈ�� ���ݷ� �� ���� ---
            // �� ������ EnemyHitbox.cs ���Ͽ� public float attackDamage; ������ ���� �� �۵��մϴ�.
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue; // EnemyHitbox�� attackDamage ������ �� ���� (0.5)
                //Debug.Log("Attack A Hitbox�� ������ �� ������: " + attackAValue);
            }
            else
            {
                //Debug.LogWarning("Attack A Hitbox Object�� EnemyHitbox ������Ʈ�� �Ҵ���� �ʾҽ��ϴ�!", attackAHitboxObject);
            }
            // -----------------------------------------------------------------

            attackAHitboxCollider.enabled = true; // �ݶ��̴� Ȱ��ȭ
            //Debug.Log(attackAHitboxObject.name + " Collider Ȱ��ȭ��");

            // TODO: �ʿ��ϴٸ� ���⼭ ResetHitFlag() ȣ���Ͽ� ���� ���� �� �ߺ� ��Ʈ ����
            // if (attackAEnemyHitbox != null) attackAEnemyHitbox.ResetHitFlag();
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Object �Ǵ� Collider�� �Ҵ���� �ʾҽ��ϴ�.", this);
        }
    }

    // --- A ���� ��Ʈ�ڽ� ��Ȱ��ȭ �޼ҵ� (Animation Event���� ȣ��) ---
    // Animation Event ���� �� �Լ� �̸��� �� �̸�(DisableAttackAHitbox)���� ���� �����ؾ� �� �� �ֽ��ϴ�.
    public void DisableAttackAHitbox() // Animation Event �ñ״�ó�� ��ġ (�Ű����� ����)
    {
        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false; // �ݶ��̴� ��Ȱ��ȭ
            //Debug.Log(attackAHitboxObject.name + " Collider ��Ȱ��ȭ��");
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Collider�� �Ҵ���� �ʾҰų� ������Ʈ�� ã�� �� �����ϴ�.", this);
        }
    }


    // TODO: ��Ÿ �ʿ��� AI ���� �Ǵ� ���� ��ȯ ���� (��: �÷��̾� ����, ���� ���� üũ ��)
    // CommonEnemyController�� virtual �޼ҵ带 �������̵��Ͽ� �����մϴ�.
    // ��: protected override void CheckForPlayer() { ... }
    // ��: protected override void MoveTowardsPlayer() { ... }
}