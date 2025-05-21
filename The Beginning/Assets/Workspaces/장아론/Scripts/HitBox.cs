using UnityEngine;
using System.Collections.Generic; // HashSet�� ����Ϸ��� �ʿ�

// �� ��ũ��Ʈ�� ���� ���ӿ�����Ʈ�� �ݶ��̴��� Trigger�� �����Ǿ� �ִ��� Ȯ��
[RequireComponent(typeof(Collider2D))] // Collider2D�� �ʼ��� �䱸�ǵ��� �߰�
public class EnemyHitbox : MonoBehaviour
{
    // �θ� ������Ʈ �Ǵ� �ٸ� ��Ʈ�ѷ����� ������ ���ݷ� ��
    public float attackDamage;

    // �� ��Ʈ�ڽ��� �̹� ������ ����� �����ϴ� HashSet
    // HashSet�� �ߺ��� ������� �ʰ�, �˻� �� �߰�/������ ȿ�����Դϴ�.
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    // B_GirlController�� ���� �ش� ��Ʈ�ڽ��� �����ϴ� ��ü (���� ����, ���� �������� �ʰ� �̺�Ʈ ������ �����ϴ� ���� �� ����)
    // private CommonEnemyController enemyController; // �ʿ��ϴٸ� �ּ� �����Ͽ� ���

    void Awake()
    {
        // Collider2D�� �ʼ��� �䱸�����Ƿ�, ���⼭ null üũ�� �ʿ� �����ϴ�.
        // ������ isTrigger ���� ���δ� Ȯ���ϴ� ���� �����ϴ�.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + "�� Collider2D�� Trigger�� �����Ǿ� ���� �ʽ��ϴ�. OnTriggerEnter2D�� ����Ϸ��� Trigger���� �մϴ�.", this);
        }

        // CommonEnemyController�� ������ �ʿ䰡 ���ٸ� (��, �� ��Ʈ�ڽ��� ���� �������� �� ��� �˸� �ȴٸ�)
        // �Ʒ� �ڵ�� �ʿ� �����ϴ�.
        // enemyController = GetComponentInParent<CommonEnemyController>();
        // if (enemyController == null)
        // {
        //     Debug.LogWarning($"EnemyHitbox on {gameObject.name}: CommonEnemyController (or derived) not found in parent. This hitbox may not function as expected for some enemies.", this);
        // }
    }

    private void OnTriggerEnter2D(Collider2D collision) // 2D �����̹Ƿ� OnTriggerEnter2D ���
    {
        // ��� �����̰ų�, �̹� ������ ����̶�� �ٽ� ������ ���� ����
        // ���⼭�� ��Ʈ�ڽ��� �浹�� �����ϹǷ�, ���� ���� üũ�� ���� ��Ʈ�ѷ����� ���ְų�
        // EnemyStatusBridge�� ���� ������Ʈ�� ���� Ȯ���ؾ� �մϴ�.
        // ����� �ܼ��� �̹� hitTargets�� ���Ե� ������� Ȯ���մϴ�.
        if (hitTargets.Contains(collision.gameObject))
        {
            return;
        }

        // �浹�� ����� �÷��̾����� �±׷� Ȯ��
        if (collision.CompareTag("Player"))
        {
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(attackDamage, this.gameObject); // �� ��Ʈ�ڽ� ������Ʈ�� ���� ��ü�� ����
                hitTargets.Add(collision.gameObject); // ������ ����� ���
                //Debug.Log($"�÷��̾�� {attackDamage} ������ ����! (��Ʈ�ڽ�: {gameObject.name})");
            }
        }
        // TODO: �ٸ� ���� ������ ���(��: �ı� ������ ������Ʈ)�� �ִٸ� ���⿡ �߰�
    }

    /// <summary>
    /// �� ��Ʈ�ڽ��� �̹� �����ߴ� ����� �ʱ�ȭ�մϴ�.
    /// ���ο� ���� �ִϸ��̼��� ���۵� �� ȣ���Ͽ� �ߺ� ���ظ� �����մϴ�.
    /// </summary>
    public void ResetHitPlayers()
    {
        hitTargets.Clear();
        //Debug.Log($"{gameObject.name} ��Ʈ�ڽ��� hitTargets�� �ʱ�ȭ�Ǿ����ϴ�.");
    }

    // ���� ����: ��Ʈ�ڽ��� Ȱ��ȭ�� ������ ResetHitPlayers�� �ڵ����� ȣ��
    private void OnEnable()
    {
        // ResetHitPlayers(); // Uncomment if you want to reset every time the hitbox GameObject is enabled
    }

    // ���� ����: ��Ʈ�ڽ��� ��Ȱ��ȭ�� ������ ResetHitPlayers�� �ڵ����� ȣ�� (���� ��ġ)
    private void OnDisable()
    {
        ResetHitPlayers();
    }
}