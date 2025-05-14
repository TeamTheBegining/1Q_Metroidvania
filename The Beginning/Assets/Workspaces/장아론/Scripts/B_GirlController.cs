using UnityEngine;
using System.Collections;

// B_Girl ĳ������ ���� ���� �� ������ �����ϴ� ��Ʈ�ѷ� (Derived Class)
// CommonEnemyController�� ��ӹ޽��ϴ�.
public class B_GirlController : CommonEnemyController
{
    // B_Girl ĳ���� ������ �ִϸ��̼� �Ķ���� �̸� (Inspector�� ������ �̸��� ��ġ�ؾ� ��)
    // CommonEnemyController�� ���� �Ķ���� �̸��� �ٸ� �� �ֽ��ϴ�.
    private const string ANIM_BOOL_B_WALK = "B_Walk";
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump";
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1";
    private const string ANIM_TRIGGER_B_ATTACK2 = "B_Attack2";
    private const string ANIM_TRIGGER_B_DEATH = "B_Death";

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject; // Inspector���� �Ҵ�
    public GameObject attack2HitboxObject; // Inspector���� �Ҵ�

    // B_Girl ĳ������ ����
    /*[Header("B_Girl Stats")]
    public float attackDamage = 15f; // <--- ���⿡ B_Girl�� ���ݷ� ����*/

    // B_Girl�� ���� ���� ������ ���� ����
    private int nextAttackIndex = 1; // ������ ������ ���� (1: Attack1, 2: Attack2)

    // B_Girl�� ���� ���� ��Ÿ��
    [Header("B_Girl Attack")]
    public float attack1Cooldown = 3f; // Attack1 ��Ÿ��
    public float attack2Cooldown = 4f; // Attack2 ��Ÿ��
    private float lastAttack1Time = -Mathf.Infinity; // ������ Attack1 �ߵ� �ð�
    private float lastAttack2Time = -Mathf.Infinity; // ������ Attack2 �ߵ� �ð�

    protected override void Start()
    {
        base.Start(); // CommonEnemyController�� Start ȣ�� (Animator, Player ���� ���� ��)

        // B_Girl ������ �ʱ� ����
        // �ʱ� ��Ÿ�� ���� (���� ���� �� �ٷ� ���� �����ϵ���)
        lastAttack1Time = Time.time - attack1Cooldown;
        lastAttack2Time = Time.time - attack2Cooldown;
        nextAttackIndex = 1; // ������ Attack1���� ����

        // ���� �� ��� ��Ʈ�ڽ� ��Ȱ��ȭ
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(false);
        }
        if (attack2HitboxObject != null)
        {
            attack2HitboxObject.SetActive(false);
        }

    }

    // Update�� Base Ŭ������ ���� ����ϵ�, �ʿ��ϸ� override �� base.Update() ȣ�� ����
    // protected override void Update() { base.Update(); }


    // ===== �ִϸ��̼� ���� �Լ��� (Base Ŭ������ virtual �޼ҵ带 �������̵��Ͽ� ���� �ִϸ��̼� ���) =====

    protected override void PlayIdleAnim()
    {
        // animator.SetBool(ANIM_BOOL_B_WALK, false); // SetState���� �̹� ó��
    }

    protected override void PlayWalkAnim()
    {
        // animator.SetBool(ANIM_BOOL_B_WALK, true); // SetState���� �̹� ó��
    }

    protected override void PlayJumpAnim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_JUMP);
    }

    protected override void PlayDeathAnim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
    }

    protected override void PlayAttack1Anim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_ATTACK1);
        // Note: Hitbox activation/deactivation will be handled by Animation Events
    }

    protected override void PlayAttack2Anim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_ATTACK2);
        // Note: Hitbox activation/deactivation will be handled by Animation Events
    }

    protected override void ResetAttackTriggers()
    {
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK2);
        // B_Attack3 Ʈ���ŵ� �ִٸ� �߰�
        // animator.ResetTrigger("B_Attack3");
    }

    // ===== AI ���� ���� (Base Ŭ������ virtual PerformAttackLogic �������̵�) =====
    // B_Girl�� ���� ���� ���� �� ���� ��Ÿ�� ���� ����

    protected override void PerformAttackLogic()
    {
        // �̹� ���� �ִϸ��̼� ���̶�� �ߺ� �ߵ� ���� (Base Ŭ���� Update���� ��ŵ������ ���� ��ġ)
        if (isPerformingAttackAnimation) return;

        float cooldownEndTime = (nextAttackIndex == 1) ? (lastAttack1Time + attack1Cooldown) : (lastAttack2Time + attack2Cooldown);

        // Debug.Log("B_Girl PerformAttackLogic - ���� ����: " + nextAttackIndex + ", Ready at: " + cooldownEndTime.ToString("F2") + ", ���� Time: " + Time.time.ToString("F2") + ", ��Ÿ�� �Ϸ� ����: " + (Time.time >= cooldownEndTime)); // �����

        if (Time.time < cooldownEndTime)
        {
            // Debug.Log("B_Girl ���� ��Ÿ�� ��� ��. ���� �ð�: " + (cooldownEndTime - Time.time).ToString("F2")); // �����
            return; // ��Ÿ�� ������ �ʾ����� ���
        }

        // ��Ÿ���� �����ٸ� ���� ����
        Debug.Log("B_Girl ���� ��Ÿ�� �Ϸ�! ���� ����: B_Attack" + nextAttackIndex + " �ߵ� �õ�.");

        isPerformingAttackAnimation = true; // <-- ���� �ִϸ��̼� ���� �� ��ġ ���� �÷��� �� (Base Class ���� ���)

        if (nextAttackIndex == 1)
        {
            Debug.Log("--> B_Attack1 �ߵ�!");
            PlayAttack1Anim(); // B_Attack1 �ִϸ��̼� �ߵ�
            lastAttack1Time = Time.time; // Attack 1 ������ �ߵ� �ð� ���
            nextAttackIndex = 2; // ���� ������ Attack 2
        }
        else // nextAttackIndex == 2
        {
            Debug.Log("--> B_Attack2 �ߵ�!");
            PlayAttack2Anim(); // B_Attack2 �ִϸ��̼� �ߵ�
            lastAttack2Time = Time.time; // Attack 2 ������ �ߵ� �ð� ���
            nextAttackIndex = 1; // ���� ������ Attack 1
        }
    }

    // ===== Animation Event Callbacks =====

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �Լ�: ��Ʈ�ڽ� Ȱ��ȭ
    // attackObject �Ű������� Animation Event���� � GameObject�� Ȱ��ȭ���� �����մϴ�.
    public void EnableHitbox()
    {
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(true);
            Debug.Log(attack1HitboxObject.name + " Ȱ��ȭ��"); // �����
        }
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �Լ�: ��Ʈ�ڽ� ��Ȱ��ȭ
    public void DisableHitbox()
    {
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(false);
            Debug.Log(attack1HitboxObject.name + " ��Ȱ��ȭ��"); // �����
        }
    }


    // TODO: Attack1.���� �÷��̾ ����� Attack2�� �����ϰ� �ϴ� ���� ���� (�䱸���� �� �ϳ�)
    // �� ������ �ణ �����ϸ�, Attack ���¿��� Chase�� ��ȯ�Ǵ� �⺻ ������ �������ؾ� �� �� �ֽ��ϴ�.
    // ���� ���, Update �Լ��� SetState �Լ��� �������̵��Ͽ� �Ÿ� üũ �� Ư�� ������ �߰��ϰų�,
    // Attack �ִϸ��̼� ���� �Ÿ��� ���������� üũ�ϴ� ������ �ڷ�ƾ �Ǵ� ������ �ʿ��� �� �ֽ��ϴ�.
    // ����� �÷��̾ ���� ���� ����� �ٷ� Chase ���·� ��ȯ�˴ϴ�.
    // �� Ư�� �䱸������ Base Ŭ������ AI ������ ����� �����ؾ� �ϹǷ�,
    // �켱 �⺻ ���� ���� ������ �� �۵��ϴ��� Ȯ�� �� �ٽ� �����ϴ� ���� ��õ�մϴ�.
}