using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_TestInput : TestBase
{
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Debug.Log("Test 1");    
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        Debug.Log("Test 2");
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        Debug.Log("Test 3");
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        Debug.Log("Test 4");
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        Debug.Log("Test 5");
    }
}
