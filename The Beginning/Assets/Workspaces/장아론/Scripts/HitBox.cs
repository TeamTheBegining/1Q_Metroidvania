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
    void OnTriggerEnter(Collider other)
    {
        // �� ��Ʈ�ڽ� ���ӿ�����Ʈ�� ���� Ȱ��ȭ �������� �ٽ� �ѹ� Ȯ�� (���� ����, ������ ����)
        if (!gameObject.activeInHierarchy) return;

        // ���� ����: �� ���� ����(Ȱ��ȭ �Ⱓ) ���� ���� ����� ���� �� ������ ���� ����
        // if (hasHitTargetInSwing) return; // EnableHitbox ��� �� �÷��׸� �����ؾ� ��

        // �浹�� ������Ʈ�� �÷��̾����� Ȯ���մϴ�.
        // �÷��̾� ������Ʈ�� "Player" �±׸� �ٿ��δ� ���� ���� �Ϲ����� ����Դϴ�.
        if (other.CompareTag("Player"))
        {
            // �浹�� �÷��̾� ������Ʈ���� �������� ó���� ��ũ��Ʈ(��: PlayerHealth)�� �����ɴϴ�.
            // �÷��̾� ������Ʈ�� PlayerHealth��� ��ũ��Ʈ�� �پ��ִٰ� �����մϴ�.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // PlayerHealth ��ũ��Ʈ�� �����ϰ�, enemyController�� ����� ����Ǿ� �ִٸ�
            if (playerHealth != null && enemyController != null)
            {
                // PlayerHealth ��ũ��Ʈ�� ������ ó�� �޼ҵ�(��: TakeDamage)�� ȣ���մϴ�.
                // �̶�, enemyController�� ���ǵ� ���ݷ� ���� �����մϴ�.
                // B_GirlController�� public float attackDamage; ������ �־�� �մϴ�.
                playerHealth.TakeDamage(enemyController.attackDamage);

                Debug.Log(gameObject.name + "�� �÷��̾ �����Ͽ� " + enemyController.attackDamage + "��ŭ�� �������� �־����ϴ�.");

                // ���� ����: ���� �� ��Ʈ ���� �÷��� ����
                // hasHitTargetInSwing = true;

                // ���� ����: �÷��̾ �� �� ��Ʈ�� �� ��� �� ��Ʈ�ڽ� ��Ȱ��ȭ (�ſ� �����ؼ� ���)
                // gameObject.SetActive(false); // <-- �̷��� �ϸ� �ش� �ִϸ��̼� �̺�Ʈ�� ���� ������ ������ �����Ƿ�, �ִϸ��̼� �̺�Ʈ ������ �� �´��� Ȯ�� �ʿ�
            }
        }

        // ���� �ٸ� ������Ʈ(��: �ı� ������ ȯ�� ������Ʈ)���Ե� �������� �� �� �ִٸ� ���⿡ �߰����� �浹 üũ ������ �����մϴ�.
        // else if (other.CompareTag("DestructibleObject")) { ... }
    }

    // ���� ����: ���� ���� ���� �� ��Ʈ ������ ����, �ִϸ��̼� �̺�Ʈ�� EnableHitbox���� �� �޼ҵ带 ȣ���Ͽ� �÷��� ����
    // public void ResetHitFlag()
    // {
    //     hasHitTargetInSwing = false;
    // }
}