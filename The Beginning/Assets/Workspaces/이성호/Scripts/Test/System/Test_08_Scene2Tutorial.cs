using UnityEngine;
using UnityEngine.InputSystem;

public class Test_08_Scene2Tutorial : TestBase
{
    public Scene2Tutorial obj;
    public TutorialEnemy enemy;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        obj.Play();
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Player player = FindFirstObjectByType<Player>();
        player.CurrentMp = player.MaxMp;
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        if (!enemy) return;

        enemy.AttackToTarget();
    }
}
