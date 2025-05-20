using UnityEngine;
using UnityEngine.InputSystem;

public class Test_08_Scene2Tutorial : TestBase
{
    public Scene2Tutorial obj;
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        obj.Play();
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Player player = FindFirstObjectByType<Player>();
        player.CurrentMp = player.MaxMp;
    }
}
