using UnityEngine;

// �� ��ũ��Ʈ�� �÷��̾� ���ӿ�����Ʈ�� �ٽ��ϴ�.
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // �ִ� ü��
    private float currentHealth; // ���� ü��

    void Start()
    {
        currentHealth = maxHealth; // ���� ���� �� ü�� �ִ�ġ�� ����
        Debug.Log("�÷��̾� ü�� �ʱ�ȭ: " + currentHealth);
    }

    // �ܺ�(��: ���ʹ� ��Ʈ�ڽ� ��ũ��Ʈ)���� ȣ���Ͽ� �������� �ִ� �޼ҵ�
    public void TakeDamage(float amount)
    {
        // �̹� �׾��ٸ� ������ ���� ����
        if (currentHealth <= 0) return;

        // ���� ��������ŭ ü�� ����
        currentHealth -= amount;
        Debug.Log("�÷��̾ " + amount + "��ŭ �������� �޾ҽ��ϴ�. ���� ü��: " + currentHealth);

        // ü���� 0 ���ϰ� �Ǹ� ��� ó��
        if (currentHealth <= 0)
        {
            Die();
        }

        // TODO: ü�� UI ������Ʈ ���� �߰� ���� ����
    }

    // �÷��̾ ������� �� ȣ��� �޼ҵ�
    void Die()
    {
        Debug.Log("�÷��̾� ���!");
        // TODO: �÷��̾� ��� �ִϸ��̼� ���, ���� ���� ó��, �� ����� ���� ���� ����
        gameObject.SetActive(false); // ������ ����: �÷��̾� ���ӿ�����Ʈ ��Ȱ��ȭ
    }

    // ���� ü���� �ܺο� �˷��ִ� Getter �޼ҵ� (���� ����)
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}