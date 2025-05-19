using UnityEngine;
using UnityEngine.InputSystem;

public class Test_08_ShowUI : TestBase
{
    public WorldText text;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        text.ShowText();
    }
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        text.HideText();
    }
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        text.FadeInText();
    }
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        text.FadeOutText();
    }
}
