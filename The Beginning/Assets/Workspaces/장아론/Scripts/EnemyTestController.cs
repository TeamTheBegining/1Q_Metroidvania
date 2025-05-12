// ������ EnemyTestController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using ��Ʒ�; // Enemy, MeleeEnemy Ŭ������ �� ���ӽ����̽� �ȿ� �����Ƿ� �ʿ��մϴ�.

// Test_00_TestInput (�������� TestBase ��ũ��Ʈ)�� ��ӹ޽��ϴ�.
// �� ��ũ��Ʈ�� �۵��Ϸ��� Test_00_TestInput�� OnTest1���� OnTest6����
// performed �̺�Ʈ�� ���� protected virtual �޼��尡 ���ǵǾ� �־�� �մϴ�.
public class EnemyTestController : Test_00_TestInput
{
    // Inspector���� �Ҵ��� ���ʹ� (MeleeEnemy�� Enemy�� ��ӹ����Ƿ� ���⿡ �Ҵ� ����)
    public MeleeEnemy testEnemy;

    // �޸��� ��� ���¸� �����ϴ� ���� (Ű 5 �����)
    private bool isWalking = false;

    // ���� Update �޼��� (Input.GetKeyDown ���) ����

    // Test1 �׼� (��: 1Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> 1Ÿ �޺� ����
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest1 �̺�Ʈ �߻�! (1Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� ���� ���°� �ƴϸ� 1Ÿ �޺� ���� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(1); // StartAttack �޼���� �޺� ���� (A1 -> Block)
            Debug.Log("EnemyTestController: 1Ű �Էµ� - 1Ÿ �޺� ����");
        }
        else
        {
            Debug.Log($"OnTest1: �޺� ���� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test2 �׼� (��: 2Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> 2Ÿ �޺� ����
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest2 �̺�Ʈ �߻�! (2Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� ���� ���°� �ƴϸ� 2Ÿ �޺� ���� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(2); // StartAttack �޼���� �޺� ���� (A1 -> A2 -> Block)
            Debug.Log("EnemyTestController: 2Ű �Էµ� - 2Ÿ �޺� ����");
        }
        else
        {
            Debug.Log($"OnTest2: �޺� ���� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test3 �׼� (��: 3Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> 3Ÿ �޺� ����
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest3 �̺�Ʈ �߻�! (3Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� ���� ���°� �ƴϸ� 3Ÿ �޺� ���� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(3); // StartAttack �޼���� �޺� ���� (A1 -> A2 -> A3 -> Block)
            Debug.Log("EnemyTestController: 3Ű �Էµ� - 3Ÿ �޺� ����");
        }
        else
        {
            Debug.Log($"OnTest3: �޺� ���� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test4 �׼� (��: 4Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> ���� �޺� ����
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest4 �̺�Ʈ �߻�! (4Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� ���� ���°� �ƴϸ� ���� �޺� ���� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.RandomAttack(); // RandomAttack �޼���� StartAttack�� ���������� ȣ�� (���� -> Block)
            Debug.Log("EnemyTestController: 4Ű �Էµ� - ���� �޺� ����");
        }
        else
        {
            Debug.Log($"OnTest4: ���� �޺� ���� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test5 �׼� (��: 5Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> �޸��� ���
    // performed �̺�Ʈ�� Ű�� ���� �� �߻��մϴ� (ª�� �����ų�, ������ �ִ� ���� �� ��).
    // ���⼭�� ���� ������ �޸���/���� ���¸� ����ϵ��� �����մϴ�.
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest5 �̺�Ʈ �߻�! (5Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� ���, ����, ��� ���°� �ƴ� ���� �̵� ���� ��� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death &&
            testEnemy.currentState != EnemyState.Attack1 && testEnemy.currentState != EnemyState.Attack2 &&
            testEnemy.currentState != EnemyState.Attack3 && testEnemy.currentState != EnemyState.Block)
        {
            isWalking = !isWalking; // �޸��� ���� ���

            if (isWalking)
            {
                // �޸��� ���� (������ ���� ���� - Move �޼��尡 ���� 1f�� ȣ��Ǹ� Walk ���·� ��ȯ)
                testEnemy.Move(1f);
                Debug.Log("EnemyTestController: 5Ű �Էµ� - �޸��� ���� (���)");
            }
            else
            {
                // ���� (Move �޼��尡 ���� 0f�� ȣ��Ǹ� Idle ���·� ��ȯ)
                testEnemy.Move(0f);
                Debug.Log("EnemyTestController: 5Ű �Էµ� - �޸��� ���� (���)");
            }
        }
        else
        {
            Debug.Log($"OnTest5: �޸��� ��� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test6 �׼� (��: 6Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�. -> ����
    protected override void OnTest6(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest6 �̺�Ʈ �߻�! (6Ű)");
        // testEnemy�� �Ҵ�Ǿ� �ְ� �̹� ���� ���°� �ƴϸ� ���� ���� �õ�
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            // Die �޼��带 ���� ȣ���Ͽ� ���� ���·� ��ȯ�մϴ�.
            // TakeDamage(maxHealth)�� ȣ���Ͽ� ���̴� ��ĵ� �����ϳ�, Die�� ���� ��ȯ�� �������Դϴ�.
            testEnemy.Die();
            Debug.Log("EnemyTestController: 6Ű �Էµ� - ���� ����");
        }
        else
        {
            Debug.Log($"OnTest6: ���� ���� ���� �Ҹ���. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // ����: �� ��ũ��Ʈ�� �������̵��ϴ� OnTest1���� OnTest6���� �޼������
    // �������� Test_00_TestInput (TestBase) ��ũ��Ʈ�� protected virtual�� ���ǵǾ� �־�� �մϴ�.
    // ���� Input Actions �ּ¿� Test1���� Test6���� �׼��� Ű (1~6)�� ���ε��Ǿ� �־�� �մϴ�.
}