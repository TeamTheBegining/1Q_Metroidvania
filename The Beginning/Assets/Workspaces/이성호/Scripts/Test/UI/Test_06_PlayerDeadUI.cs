using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_PlayerDeadUI : TestBase
{
    public Player player;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        player.CurrentHp = 0;
    }
}