using UnityEngine;

// �� ��ũ��Ʈ�� B_Girl ���ӿ�����Ʈ�� �ڽ��� ��Ʈ�ڽ� ���ӿ�����Ʈ�� �ٽ��ϴ�.
public class EnemyHitbox : MonoBehaviour
{
    // �θ� ������Ʈ(B_Girl)�� �پ��ִ� B_GirlController ��ũ��Ʈ ����
    // GetComponentInParent�� ����ϱ� ���� ��Ʈ�ڽ� ������Ʈ�� B_Girl�� �ڽ��̾�� �մϴ�.
    private B_GirlController enemyController;

    // ���� ����: ���� �������� ���� ����� ���� �� ��Ʈ�ϴ� ���� �����ϴ� �÷���
    // private bool hasHitTargetInSwing = false; // ���� ����

    void Awake()
    {
        // ���� �� �θ�(�Ǵ� �� ��) ������Ʈ���� B_GirlController ������Ʈ�� ã���ϴ�.
        enemyController = GetComponentInParent<B_GirlController>();

        if (enemyController == null)
        {
            Debug.LogError("EnemyHitbox ��ũ��Ʈ�� �θ� ������Ʈ�� B_GirlController(�Ǵ� ��ӹ��� Ŭ����)�� �ʿ��մϴ�.", this);
            // ��Ʈ�ѷ��� ã�� ���ϸ� ��ũ��Ʈ ��Ȱ��ȭ (���� ����)
            enabled = false;
            return; // �� �̻� �������� ����
        }

        // �� ��ũ��Ʈ�� ���� ���ӿ�����Ʈ�� �ݶ��̴��� Trigger�� �����Ǿ� �ִ��� Ȯ��
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            // Warning�� ��� ���� ������ �˷���
            Debug.LogWarning(gameObject.name + "�� �ݶ��̴��� Trigger�� �����Ǿ� ���� �ʽ��ϴ�. OnTriggerEnter�� ����Ϸ��� Trigger���� �մϴ�.", this);
        }

        // �߿�: �� ��Ʈ�ڽ� ���ӿ�����Ʈ ��ü�� ó���� ��Ȱ��ȭ ���¿��� �մϴ�.
        // �ִϸ��̼� �̺�Ʈ�� Ÿ�ֿ̹� ���� Ȱ��ȭ/��Ȱ��ȭ�� �����մϴ�.
    }

    // �ݶ��̴��� �ٸ� �ݶ��̴� ���� ������ ������ �� ȣ��˴ϴ�.
    // �� �Լ��� ȣ��Ƿ��� �� ���ӿ�����Ʈ�� �ݶ��̴��� ���� �ݶ��̴� ��� �־�� �ϰ�,
    // �� �� �ϳ� �̻��� Is Trigger�� üũ�Ǿ� �־�� �մϴ�.

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(enemyController.attackDamage);
    }

    // ���� ����: ���� ���� ���� �� ��Ʈ ������ ����, �ִϸ��̼� �̺�Ʈ�� EnableHitbox���� �� �޼ҵ带 ȣ���Ͽ� �÷��� ����
    // public void ResetHitFlag()
    // {
    //     hasHitTargetInSwing = false;
    // }
}