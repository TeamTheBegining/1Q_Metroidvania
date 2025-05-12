using UnityEngine;
using 장아론; // Enemy, MeleeEnemy 클래스가 이 네임스페이스 안에 있으므로 필요합니다.
using UnityEngine.InputSystem; // InputAction.CallbackContext 사용을 위해 필요

// TestBase를 상속받아 Input System 이벤트 구독 기능을 활용합니다.
// 이 스크립트가 작동하려면 TestBase 스크립트가 프로젝트에 존재해야 하며,
// TestInputActions 애셋을 사용하도록 설정되어 있어야 합니다.
public class TestManager : TestBase
{
    
    public MeleeEnemy myEnemy; // Hierarchy에서 이 필드에 테스트할 에너미 오브젝트를 드래그하여 연결합니다.

                             // 필요하다면 Inspector에서 설정 가능한 변수를 추가할 수 있습니다.
                         // 예: public int combo1Key = 1; // 이런 식으로 설정도 가능

    // Input System은 Update 메서드로 입력을 폴링하는 대신,
    // 특정 액션이 발생했을 때 (performed, started, canceled 등)
    // 미리 구독해 둔 메서드를 호출합니다.
    // 따라서 TestManager 자체에는 Update 메서드가 필요 없습니다.
    // 입력 처리는 TestBase에서 구독 설정하고, 여기서 오버라이드하여 구현합니다.


#if UNITY_EDITOR // TestBase 스크립트가 #if UNITY_EDITOR로 감싸져 있다면 여기도 맞춰줍니다.
    // TestBase 스크립트 내용에 따라 이 부분은 달라질 수 있습니다.

    // TestBase에서 정의된 가상 메서드들을 오버라이드하여 실제 로직을 구현합니다.

    // Test1 액션 (예: 1키)이 performed 되었을 때 호출됩니다.
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        // 연결된 에너미가 없거나 사망 상태가 아니면 1타 콤보 시작
        // myEnemy.currentState 접근 가능 (Enemy.currentState를 public으로 변경했다고 가정)
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // 에너미 스크립트의 StartAttack 메서드 호출 (1타 콤보 시작)
            // StartAttack 내부에서 공격 가능한 상태인지 체크합니다.
            myEnemy.StartAttack(1);
            Debug.Log("TestManager: OnTest1 입력됨 - 1타 콤bo 시작"); // 로그 추가
        }
    }

    // Test2 액션 (예: 2키)이 performed 되었을 때 호출됩니다.
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        // 연결된 에너미가 없거나 사망 상태가 아니면 2타 콤보 시작
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            myEnemy.StartAttack(2);
            Debug.Log("TestManager: OnTest2 입력됨 - 2타 콤bo 시작"); // 로그 추가
        }
    }

    // Test3 액션 (예: 3키)이 performed 되었을 때 호출됩니다.
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        // 연결된 에너미가 없거나 사망 상태가 아니면 3타 콤보 시작
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            myEnemy.StartAttack(3);
            Debug.Log("TestManager: OnTest3 입력됨 - 3타 콤bo 시작"); // 로그 추가
        }
    }

    // Test4 액션 (예: 4키)이 performed 되었을 때 호출됩니다.
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        // 연결된 에너미가 없거나 사망 상태가 아니면 랜덤 콤보 시작
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // 에너미 스크립트에 RandomAttack이 있다면 호출 (현재 콤보 시스템에서는 StartAttack 호출하도록 수정됨)
            // myEnemy.RandomAttack();
            // 또는 여기서 랜덤으로 단계를 정해서 StartAttack 호출
            myEnemy.StartAttack(Random.Range(1, 4)); // 1~3단계 중 랜덤 콤보 시작
            Debug.Log("TestManager: OnTest4 입력됨 - 랜덤 콤보 시작"); // 로그 추가
        }
    }

    // Test5 액션 (예: 5키)이 performed 되었을 때 호출됩니다.
    // 이전에 5키는 Idle, 6키는 Walk, 7키는 Block으로 논의되었습니다.
    // TestBase에 OnTest5까지만 정의되어 있어 여기에 예시를 둡니다.
    // 만약 5키를 강제 Idle로 사용한다면 아래와 같이 구현할 수 있습니다.
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        // 연결된 에너미가 없거나 사망/공격/블록 중이 아니면 강제 Idle 전환
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death &&
            myEnemy.currentState != EnemyState.Attack1 && myEnemy.currentState != EnemyState.Attack2 &&
            myEnemy.currentState != EnemyState.Attack3 && myEnemy.currentState != EnemyState.Block)
        {
            myEnemy.ChangeState(EnemyState.Idle);
            Debug.Log("TestManager: OnTest5 입력됨 - 강제 Idle 전환 (5키)"); // 로그 추가
        }
    }


    // 참고: 이동(Walk)이나 블록(Block)처럼 누르고 있는 동안 상태가 유지되거나
    // 값을 읽어와야 하는 액션은 performed 이벤트 외에 started/canceled 이벤트를 조합하거나,
    // Input Action 값을 직접 읽는 방식을 사용할 수 있습니다.
    // TestBase에 OnMove, OnBlockStarted/Canceled 등의 가상 메서드가 정의되어 있다면
    // 여기 TestManager에서 오버라이드하여 구현해야 합니다.
    // Input Actions 애셋에 'Move' 액션 (Vector2 타입)과 'Block' 액션 (Button 타입)이
    // 필요하며, TestBase에서 이 액션들의 이벤트를 구독해야 합니다.

    /*
    // 만약 TestBase에 protected virtual void OnMove(InputAction.CallbackContext context)가 있다면
    protected override void OnMove(InputAction.CallbackContext context)
    {
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death)
        {
            // Input System에서 읽어온 이동 벡터 값 (예: Vector2(1,0) 또는 Vector2(-1,0))
            Vector2 moveInput = context.ReadValue<Vector2>();
            float direction = moveInput.x; // 좌우 이동만 사용하는 경우

            // 에너미의 Move 함수 호출
            myEnemy.Move(direction); // Move 함수가 내부적으로 Idle/Walk 전환 처리
             Debug.Log($"TestManager: OnMove 입력됨 - 이동 {direction}"); // 로그 추가
        }
    }

    // 만약 TestBase에 protected virtual void OnBlockStarted(InputAction.CallbackContext context)가 있다면
    protected override void OnBlockStarted(InputAction.CallbackContext context)
    {
        if (myEnemy != null && myEnemy.currentState != EnemyState.Death &&
            myEnemy.currentState != EnemyState.Attack1 && myEnemy.currentState != EnemyState.Attack2 &&
            myEnemy.currentState != EnemyState.Attack3 && myEnemy.currentState != EnemyState.Block)
        {
            myEnemy.Block(); // 블록 상태 시작
            Debug.Log("TestManager: OnBlockStarted 입력됨 - 블록 시작"); // 로그 추가
        }
    }

    // 만약 TestBase에 protected virtual void OnBlockCanceled(InputAction.CallbackContext context)가 있다면
     protected override void OnBlockCanceled(InputAction.CallbackContext context)
     {
          // 블록 상태일 때만 블록 종료 처리
          if (myEnemy != null && myEnemy.currentState == EnemyState.Block)
          {
               // 에너미 스크립트의 ChangeState(EnemyState.Idle)을 호출하여 블록을 끝냅니다.
               // OnEnterState(Idle)에서 콤보 스테이지 초기화가 됩니다.
               myEnemy.ChangeState(EnemyState.Idle);
               Debug.Log("TestManager: OnBlockCanceled 입력됨 - 블록 종료"); // 로그 추가
          }
     }
     */


#endif // UNITY_EDITOR
}