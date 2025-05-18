using UnityEngine;


/// <summary>
/// ���� ���¸� �Ŵ����� ����ȭ �ϱ� ���� ���� ������Ʈ
/// </summary>
/// <remarks>
/// �ܼ��� ���� ID ������ �Ŵ����� ���� ����ȭ�� �������� �� Ŭ����
/// </remarks>
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
            Debug.Log("deactive");
        }
    }

    /// <summary>
    /// ���� ����� �� EnemyStateManager�� ���¸� �˸��� �Լ�.
    /// (�� ��� ó�� �Լ� ������ ȣ���� ��)
    /// </summary>
    /// <remarks>
    /// �� �Լ� ȣ��� �� �ε� �Ŀ��� �� ��� ���°� �����˴ϴ�.
    /// 2025.05.18 - �ۼ��� : �̼�ȣ
    /// </remarks>
    public void MarkAsDead()
    {
        // �� �ε� �Ŀ��� �� ��� ������ ���� ���� ����

        EnemyStateManager.Instance.SetEnemyDead(enemyID);
    }
}
