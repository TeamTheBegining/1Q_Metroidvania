using UnityEngine;

// �� ��ũ��Ʈ�� B_Girl ���ӿ�����Ʈ�� �ڽ��� ��Ʈ�ڽ� ���ӿ�����Ʈ�� �ٽ��ϴ�.
public class EnemyHitbox : MonoBehaviour
{
    // �θ� ������Ʈ(B_Girl)�� �پ��ִ� B_GirlController ��ũ��Ʈ ����
    // GetComponentInParent�� ����ϱ� ���� ��Ʈ�ڽ� ������Ʈ�� B_Girl�� �ڽ��̾�� �մϴ�.
    private B_GirlController enemyController;

    // --- �߰�: �� ��Ʈ�ڽ��� �� ���ݷ� �� ---
    public float attackDamage;
    // ------------------------------------

    // ���� ����: ���� �������� ���� ����� ���� �� ��Ʈ�ϴ� ���� �����ϴ� �÷���
    // private bool hasHitTargetInSwing = false; // ���� ����

    void Awake()
    {
        // ���� �� �θ�(�Ǵ� �� ��) ������Ʈ���� B_GirlController ������Ʈ�� ã���ϴ�.
        enemyController = GetComponentInParent<B_GirlController>();

        if (enemyController == null)
        {
            //Debug.LogError("EnemyHitbox ��ũ��Ʈ�� �θ� ������Ʈ�� B_GirlController(�Ǵ� ��ӹ��� Ŭ����)�� �ʿ��մϴ�.", this);
            // ��Ʈ�ѷ��� ã�� ���ϸ� ��ũ��Ʈ ��Ȱ��ȭ (���� ����)
            enabled = false;
            return; // �� �̻� �������� ����
        }

        // �� ��ũ��Ʈ�� ���� ���ӿ�����Ʈ�� �ݶ��̴��� Trigger�� �����Ǿ� �ִ��� Ȯ��
        Collider col = GetComponent<Collider>(); // Collider2D�� ���ɼ��� �����ϴ�.
        if (col != null && !col.isTrigger)
        {
            //Debug.LogWarning(gameObject.name + "�� �ݶ��̴��� Trigger�� �����Ǿ� ���� �ʽ��ϴ�. OnTriggerEnter�� ����Ϸ��� Trigger���� �մϴ�.", this);
        }

        // ����: B_GirlController���� ���� �� ��Ʈ�ڽ� �ݶ��̴��� ��Ȱ��ȭ�մϴ�.
        // �� ��ũ��Ʈ ��ü�� Ȱ��ȭ ���·� �ξ �˴ϴ�.
    }

    // �ݶ��̴��� �ٸ� �ݶ��̴� ���� ������ ������ �� ȣ��˴ϴ�.
    // �� �Լ��� ȣ��Ƿ��� �� ���ӿ�����Ʈ�� �ݶ��̴��� ���� �ݶ��̴� ��� �־�� �ϰ�,
    // �� �� �ϳ� �̻��� Is Trigger�� üũ�Ǿ� �־�� �մϴ�.

    private void OnTriggerEnter2D(Collider2D collision) // 2D �����̹Ƿ� OnTriggerEnter2D ���
    {
        // �浹�� ����� �÷��̾����� �±׷� Ȯ��
        if (collision.CompareTag("Player")) // CompareTag�� String �񱳺��� ���ɻ� �����ϴ�.
        {
            // �浹�� �÷��̾� ������Ʈ���� IDamageable ������Ʈ�� ������ ������ ����
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                // --- ����: ����� attackDamage ������ ���� ��� ---
                damageableTarget.TakeDamage(attackDamage); // ���⿡ B_GirlController���� �޾ƿ� ���� ����մϴ�.
                // -----------------------------------------------
                //Debug.Log("�÷��̾�� " + attackDamage + " ������ ����!");
            }
        }
        // TODO: ���� �������� ���� �� ��Ʈ ���� ���� ����
        // if (!hasHitTargetInSwing && collision.CompareTag("Player")) { ... hasHitTargetInSwing = true; ... }
    }

    // ���� ����: ���� ���� ���� �� ��Ʈ ������ ����, �ִϸ��̼� �̺�Ʈ�� EnableHitbox���� �� �޼ҵ带 ȣ���Ͽ� �÷��� ����
    // public void ResetHitFlag()
    // {
    //     hasHitTargetInSwing = false;
    // }
}