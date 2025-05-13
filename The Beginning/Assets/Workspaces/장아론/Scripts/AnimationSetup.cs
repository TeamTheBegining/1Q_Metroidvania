using UnityEngine;

// �����Ϳ��� Animator ��Ʈ�ѷ� ������ �����ִ� Ŭ���� (���� ����)
public class AnimatorSetup : MonoBehaviour
{
    // �� ��ũ��Ʈ�� �����Ϳ����� �۵��մϴ�
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Setup Enemy Animator")]
    public static void SetupAnimatorController()
    {
        // Animator ��Ʈ�ѷ� ���� (�����Ϳ����� ���)
        UnityEditor.Animations.AnimatorController controller =
            UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath("Assets/EnemyController.controller");

        // �Ķ���� �߰�
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Action1", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Action2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Action3", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Block", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

        // �ʿ��� �⺻ ���µ� �߰� (���� �ִϸ��̼� Ŭ���� Unity���� �����ؾ� ��)
        var rootStateMachine = controller.layers[0].stateMachine;

        // ���� �߰�
        var idleState = rootStateMachine.AddState("Idle");
        var walkState = rootStateMachine.AddState("Walk");
        var action1State = rootStateMachine.AddState("Action1");
        var action2State = rootStateMachine.AddState("Action2");
        var action3State = rootStateMachine.AddState("Action3");
        var blockState = rootStateMachine.AddState("Block");
        var deathState = rootStateMachine.AddState("Death");

        // Ʈ������ �߰�
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

        // Block -> Idle (�⺻ ����)
        var blockToIdle = blockState.AddTransition(idleState);
        blockToIdle.hasExitTime = true;

        Debug.Log("Enemy Animator Controller ���� �Ϸ�!");
    }
#endif
}