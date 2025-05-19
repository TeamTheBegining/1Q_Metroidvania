using UnityEngine;
using UnityEngine.InputSystem;

public class Test_07_ScrollUI : TestBase
{
    public TextScroll scroll;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        scroll.currIndex = 0;
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        scroll.PlayScroll();
    }
}
