using UnityEngine;

// 에디터에서 Animator 컨트롤러 설정을 도와주는 클래스 (선택 사항)
public class AnimatorSetup : MonoBehaviour
{
    // 이 스크립트는 에디터에서만 작동합니다
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Setup Enemy Animator")]
    public static void SetupAnimatorController()
    {
        // Animator 컨트롤러 생성 (에디터에서만 사용)
        UnityEditor.Animations.AnimatorController controller =
            UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/EnemyController.controller");

        // 파라미터 추가
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Action1", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Action2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Action3", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Block", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        // 필요한 기본 상태들 추가 (실제 애니메이션 클립은 Unity에서 연결해야 함)
        var rootStateMachine = controller.layers[0].stateMachine;

        // 상태 추가
        var idleState = rootStateMachine.AddState("Idle");
        var walkState = rootStateMachine.AddState("Walk");
        var action1State = rootStateMachine.AddState("Action1");
        var action2State = rootStateMachine.AddState("Action2");
        var action3State = rootStateMachine.AddState("Action3");
        var blockState = rootStateMachine.AddState("Block");
        var deathState = rootStateMachine.AddState("Death");

        // 트랜지션 추가
        // Idle -> Walk
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsWalking");

        // Walk -> Idle
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0, "IsWalking");

        // Idle/Walk -> Action1
        var idleToAction1 = idleState.AddTransition(action1State);
        idleToAction1.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Action1");

        var walkToAction1 = walkState.AddTransition(action1State);
        walkToAction1.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Action1");

        // Action1 -> Action2
        var action1ToAction2 = action1State.AddTransition(action2State);
        action1ToAction2.hasExitTime = true;
        action1ToAction2.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Action2");

        // Action2 -> Action3
        var action2ToAction3 = action2State.AddTransition(action3State);
        action2ToAction3.hasExitTime = true;
        action2ToAction3.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Action3");

        // Action3 -> Block
        var action3ToBlock = action3State.AddTransition(blockState);
        action3ToBlock.hasExitTime = true;
        action3ToBlock.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Block");

        // Any State -> Death
        var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Death");

        // Block -> Idle (기본 복귀)
        var blockToIdle = blockState.AddTransition(idleState);
        blockToIdle.hasExitTime = true;

        Debug.Log("Enemy Animator Controller 설정 완료!");
    }
#endif
}