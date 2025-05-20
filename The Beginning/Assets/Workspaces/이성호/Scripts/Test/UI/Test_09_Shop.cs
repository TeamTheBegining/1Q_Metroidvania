using UnityEngine;
using UnityEngine.InputSystem;

public class Test_09_Shop : TestBase
{
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.AddCoin(100000);
    }
}