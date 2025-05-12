using UnityEngine;
using ��Ʒ�; // Enemy, MeleeEnemy Ŭ������ �� ���ӽ����̽� �ȿ� �����Ƿ� �ʿ��մϴ�.
using UnityEngine.InputSystem; // InputAction.CallbackContext ����� ���� �ʿ�

// TestBase�� ��ӹ޾� Input System �̺�Ʈ ���� ����� Ȱ���մϴ�.
// �� ��ũ��Ʈ�� �۵��Ϸ��� TestBase ��ũ��Ʈ�� ������Ʈ�� �����ؾ� �ϸ�,
// TestInputActions �ּ��� ����ϵ��� �����Ǿ� �־�� �մϴ�.
public class TestManager : TestBase
{
    
    public MeleeEnemy myEnemy; // Hierarchy���� �� �ʵ忡 �׽�Ʈ�� ���ʹ� ������Ʈ�� �巡���Ͽ� �����մϴ�.

                             // �ʿ��ϴٸ� Inspector���� ���� ������ ������ �߰��� �� �ֽ��ϴ�.
                         // ��: public int combo1Key = 1; // �̷� ������ ������ ����

    // Input System�� Update �޼���� �Է��� �����ϴ� ���,
    // Ư�� �׼��� �߻����� �� (performed, started, canceled ��)
    // �̸� ������ �� �޼��带 ȣ���մϴ�.
    // ���� TestManager ��ü���� Update �޼��尡 �ʿ� �����ϴ�.
    // �Է� ó���� TestBase���� ���� �����ϰ�, ���⼭ �������̵��Ͽ� �����մϴ�.


#if UNITY_EDITOR // TestBase ��ũ��Ʈ�� #if UNITY_EDITOR�� ������ �ִٸ� ���⵵ �����ݴϴ�.
    // TestBase ��ũ��Ʈ ���뿡 ���� �� �κ��� �޶��� �� �ֽ��ϴ�.

    // TestBase���� ���ǵ� ���� �޼������ �������̵��Ͽ� ���� ������ �����մϴ�.

    // Test1 �׼� (��: 1Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�.
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        // ����� ���ʹ̰� ���ų� ��� ���°� �ƴϸ� 1Ÿ �޺� ����
        // myEnemy.currentState ���� ���� (Enemy.currentState�� public���� �����ߴٰ� ����)
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // ���ʹ� ��ũ��Ʈ�� StartAttack �޼��� ȣ�� (1Ÿ �޺� ����)
            // StartAttack ���ο��� ���� ������ �������� üũ�մϴ�.
            myEnemy.StartAttack(1);
            Debug.Log("TestManager: OnTest1 �Էµ� - 1Ÿ ��bo ����"); // �α� �߰�
        }
    }

    // Test2 �׼� (��: 2Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�.
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        // ����� ���ʹ̰� ���ų� ��� ���°� �ƴϸ� 2Ÿ �޺� ����
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            myEnemy.StartAttack(2);
            Debug.Log("TestManager: OnTest2 �Էµ� - 2Ÿ ��bo ����"); // �α� �߰�
        }
    }

    // Test3 �׼� (��: 3Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�.
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        // ����� ���ʹ̰� ���ų� ��� ���°� �ƴϸ� 3Ÿ �޺� ����
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            myEnemy.StartAttack(3);
            Debug.Log("TestManager: OnTest3 �Էµ� - 3Ÿ ��bo ����"); // �α� �߰�
        }
    }

    // Test4 �׼� (��: 4Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�.
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        // ����� ���ʹ̰� ���ų� ��� ���°� �ƴϸ� ���� �޺� ����
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // ���ʹ� ��ũ��Ʈ�� RandomAttack�� �ִٸ� ȣ�� (���� �޺� �ý��ۿ����� StartAttack ȣ���ϵ��� ������)
            // myEnemy.RandomAttack();
            // �Ǵ� ���⼭ �������� �ܰ踦 ���ؼ� StartAttack ȣ��
            myEnemy.StartAttack(Random.Range(1, 4)); // 1~3�ܰ� �� ���� �޺� ����
            Debug.Log("TestManager: OnTest4 �Էµ� - ���� �޺� ����"); // �α� �߰�
        }
    }

    // Test5 �׼� (��: 5Ű)�� performed �Ǿ��� �� ȣ��˴ϴ�.
    // ������ 5Ű�� Idle, 6Ű�� Walk, 7Ű�� Block���� ���ǵǾ����ϴ�.
    // TestBase�� OnTest5������ ���ǵǾ� �־� ���⿡ ���ø� �Ӵϴ�.
    // ���� 5Ű�� ���� Idle�� ����Ѵٸ� �Ʒ��� ���� ������ �� �ֽ��ϴ�.
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        // ����� ���ʹ̰� ���ų� ���/����/��� ���� �ƴϸ� ���� Idle ��ȯ
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death &&
            myEnemy.currentState != EnemyState.Attack1 && myEnemy.currentState != EnemyState.Attack2 &&
            myEnemy.currentState != EnemyState.Attack3 && myEnemy.currentState != EnemyState.Block)
        {
            myEnemy.ChangeState(EnemyState.Idle);
            Debug.Log("TestManager: OnTest5 �Էµ� - ���� Idle ��ȯ (5Ű)"); // �α� �߰�
        }
    }


    // ����: �̵�(Walk)�̳� ���(Block)ó�� ������ �ִ� ���� ���°� �����ǰų�
    // ���� �о�;� �ϴ� �׼��� performed �̺�Ʈ �ܿ� started/canceled �̺�Ʈ�� �����ϰų�,
    // Input Action ���� ���� �д� ����� ����� �� �ֽ��ϴ�.
    // TestBase�� OnMove, OnBlockStarted/Canceled ���� ���� �޼��尡 ���ǵǾ� �ִٸ�
    // ���� TestManager���� �������̵��Ͽ� �����ؾ� �մϴ�.
    // Input Actions �ּ¿� 'Move' �׼� (Vector2 Ÿ��)�� 'Block' �׼� (Button Ÿ��)��
    // �ʿ��ϸ�, TestBase���� �� �׼ǵ��� �̺�Ʈ�� �����ؾ� �մϴ�.

    /*
    // ���� TestBase�� protected virtual void OnMove(InputAction.CallbackContext context)�� �ִٸ�
    protected override void OnMove(InputAction.CallbackContext context)
    {
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // Input System���� �о�� �̵� ���� �� (��: Vector2(1,0) �Ǵ� Vector2(-1,0))
            Vector2 moveInput = context.ReadValue<Vector2>();
            float direction = moveInput.x; // �¿� �̵��� ����ϴ� ���

            // ���ʹ��� Move �Լ� ȣ��
            myEnemy.Move(direction); // Move �Լ��� ���������� Idle/Walk ��ȯ ó��
             Debug.Log($"TestManager: OnMove �Էµ� - �̵� {direction}"); // �α� �߰�
        }
    }

    // ���� TestBase�� protected virtual void OnBlockStarted(InputAction.CallbackContext context)�� �ִٸ�
    protected override void OnBlockStarted(InputAction.CallbackContext context)
    {
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death &&
            myEnemy.currentState != EnemyState.Attack1 && myEnemy.currentState != EnemyState.Attack2 &&
            myEnemy.currentState != EnemyState.Attack3 && myEnemy.currentState != EnemyState.Block)
        {
            myEnemy.Block(); // ��� ���� ����
            Debug.Log("TestManager: OnBlockStarted �Էµ� - ��� ����"); // �α� �߰�
        }
    }

    // ���� TestBase�� protected virtual void OnBlockCanceled(InputAction.CallbackContext context)�� �ִٸ�
     protected override void OnBlockCanceled(InputAction.CallbackContext context)
     {
          // ��� ������ ���� ��� ���� ó��
          if (myEnemy != null && myEnemy.currentState == EnemyState.Block)
          {
               // ���ʹ� ��ũ��Ʈ�� ChangeState(EnemyState.Idle)�� ȣ���Ͽ� ����� �����ϴ�.
               // OnEnterState(Idle)���� �޺� �������� �ʱ�ȭ�� �˴ϴ�.
               myEnemy.ChangeState(EnemyState.Idle);
               Debug.Log("TestManager: OnBlockCanceled �Էµ� - ��� ����"); // �α� �߰�
          }
     }
     */


#endif // UNITY_EDITOR
}