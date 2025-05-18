using UnityEngine;

/// �ܼ��� ���� ID ������ �Ŵ����� ���� ����ȭ�� �������� �� Ŭ����

/// <summary>
/// ���� ���¸� �Ŵ����� ����ȭ �ϱ� ���� ���� ������Ʈ
/// </summary>
public class EnemyStatusBridge : MonoBehaviour
{
    [Tooltip("���� Ȯ���� ���� ���� ���̵� ���� ����")]
    [SerializeField] private string enemyID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(enemyID))
        {
            Debug.LogError($"[EnemyStatusBridge] �� ID�� ����ֽ��ϴ�! GameObject: {gameObject.name}");
        }
        else
        {
            EnemyStateManager.Instance.RegisterEnemy(enemyID);
        }
    }

    private void Start()
    {
        if (EnemyStateManager.Instance.IsEnemyDead(enemyID))
        {
            gameObject.SetActive(false); // �̹� ���� ���� ��Ȱ��ȭ
        }
    }

    /// <summary>
    /// ��� �� �Ŵ���(EnemyStateManager)�� ����� �˸��� �Լ� ( �� ��� �� ȣ��� �Լ��� ȣ�� �� �� )
    /// </summary>
    public void MarkAsDead()
    {
        EnemyStateManager.Instance.SetEnemyDead(enemyID);
    }
}
