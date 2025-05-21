using UnityEngine;
using UnityEngine.InputSystem;

public class Test_09_Shop : TestBase
{
#if UNITY_EDITOR

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PlayerManager.Instance.AddCoin(100000);
    }
#endif
}