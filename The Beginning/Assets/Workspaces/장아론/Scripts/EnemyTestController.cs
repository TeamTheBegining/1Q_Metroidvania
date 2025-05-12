// 수정된 EnemyTestController.cs
using UnityEngine;
using UnityEngine.InputSystem;
using 장아론; // Enemy, MeleeEnemy 클래스가 이 네임스페이스 안에 있으므로 필요합니다.

// Test_00_TestInput (여러분의 TestBase 스크립트)를 상속받습니다.
// 이 스크립트가 작동하려면 Test_00_TestInput에 OnTest1부터 OnTest6까지
// performed 이벤트에 대한 protected virtual 메서드가 정의되어 있어야 합니다.
public class EnemyTestController : Test_00_TestInput
{
    // Inspector에서 할당할 에너미 (MeleeEnemy는 Enemy를 상속받으므로 여기에 할당 가능)
    public MeleeEnemy testEnemy;

    // 달리기 토글 상태를 추적하는 변수 (키 5 제어용)
    private bool isWalking = false;

    // 기존 Update 메서드 (Input.GetKeyDown 사용) 삭제

    // Test1 액션 (예: 1키)이 performed 되었을 때 호출됩니다. -> 1타 콤보 시작
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest1 이벤트 발생! (1키)");
        // testEnemy가 할당되어 있고 죽은 상태가 아니면 1타 콤보 시작 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(1); // StartAttack 메서드로 콤보 시작 (A1 -> Block)
            Debug.Log("EnemyTestController: 1키 입력됨 - 1타 콤보 시작");
        }
        else
        {
            Debug.Log($"OnTest1: 콤보 시작 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test2 액션 (예: 2키)이 performed 되었을 때 호출됩니다. -> 2타 콤보 시작
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest2 이벤트 발생! (2키)");
        // testEnemy가 할당되어 있고 죽은 상태가 아니면 2타 콤보 시작 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(2); // StartAttack 메서드로 콤보 시작 (A1 -> A2 -> Block)
            Debug.Log("EnemyTestController: 2키 입력됨 - 2타 콤보 시작");
        }
        else
        {
            Debug.Log($"OnTest2: 콤보 시작 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test3 액션 (예: 3키)이 performed 되었을 때 호출됩니다. -> 3타 콤보 시작
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest3 이벤트 발생! (3키)");
        // testEnemy가 할당되어 있고 죽은 상태가 아니면 3타 콤보 시작 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.StartAttack(3); // StartAttack 메서드로 콤보 시작 (A1 -> A2 -> A3 -> Block)
            Debug.Log("EnemyTestController: 3키 입력됨 - 3타 콤보 시작");
        }
        else
        {
            Debug.Log($"OnTest3: 콤보 시작 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test4 액션 (예: 4키)이 performed 되었을 때 호출됩니다. -> 랜덤 콤보 시작
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest4 이벤트 발생! (4키)");
        // testEnemy가 할당되어 있고 죽은 상태가 아니면 랜덤 콤보 시작 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            testEnemy.RandomAttack(); // RandomAttack 메서드는 StartAttack을 내부적으로 호출 (랜덤 -> Block)
            Debug.Log("EnemyTestController: 4키 입력됨 - 랜덤 콤보 시작");
        }
        else
        {
            Debug.Log($"OnTest4: 랜덤 콤보 시작 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test5 액션 (예: 5키)이 performed 되었을 때 호출됩니다. -> 달리기 토글
    // performed 이벤트는 키를 누를 때 발생합니다 (짧게 누르거나, 누르고 있는 동안 한 번).
    // 여기서는 누를 때마다 달리기/멈춤 상태를 토글하도록 구현합니다.
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest5 이벤트 발생! (5키)");
        // testEnemy가 할당되어 있고 사망, 공격, 블록 상태가 아닐 때만 이동 상태 토글 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death &&
            testEnemy.currentState != EnemyState.Attack1 && testEnemy.currentState != EnemyState.Attack2 &&
            testEnemy.currentState != EnemyState.Attack3 && testEnemy.currentState != EnemyState.Block)
        {
            isWalking = !isWalking; // 달리기 상태 토글

            if (isWalking)
            {
                // 달리기 시작 (오른쪽 방향 예시 - Move 메서드가 방향 1f로 호출되면 Walk 상태로 전환)
                testEnemy.Move(1f);
                Debug.Log("EnemyTestController: 5키 입력됨 - 달리기 시작 (토글)");
            }
            else
            {
                // 멈춤 (Move 메서드가 방향 0f로 호출되면 Idle 상태로 전환)
                testEnemy.Move(0f);
                Debug.Log("EnemyTestController: 5키 입력됨 - 달리기 멈춤 (토글)");
            }
        }
        else
        {
            Debug.Log($"OnTest5: 달리기 토글 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // Test6 액션 (예: 6키)이 performed 되었을 때 호출됩니다. -> 죽음
    protected override void OnTest6(InputAction.CallbackContext context)
    {
        Debug.Log(">>> OnTest6 이벤트 발생! (6키)");
        // testEnemy가 할당되어 있고 이미 죽은 상태가 아니면 죽음 실행 시도
        if (testEnemy != null && testEnemy.currentState != EnemyState.Death)
        {
            // Die 메서드를 직접 호출하여 죽음 상태로 전환합니다.
            // TakeDamage(maxHealth)를 호출하여 죽이는 방식도 가능하나, Die가 상태 전환의 진입점입니다.
            testEnemy.Die();
            Debug.Log("EnemyTestController: 6키 입력됨 - 죽음 실행");
        }
        else
        {
            Debug.Log($"OnTest6: 죽음 실행 조건 불만족. testEnemy: {(testEnemy == null ? "Null" : "Not Null")}, State: {(testEnemy == null ? "N/A" : testEnemy.currentState.ToString())}");
        }
    }

    // 참고: 이 스크립트가 오버라이드하는 OnTest1부터 OnTest6까지 메서드들이
    // 여러분의 Test_00_TestInput (TestBase) 스크립트에 protected virtual로 정의되어 있어야 합니다.
    // 또한 Input Actions 애셋에 Test1부터 Test6까지 액션이 키 (1~6)에 바인딩되어 있어야 합니다.
}