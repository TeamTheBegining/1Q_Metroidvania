﻿using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestBase : MonoBehaviour
{
#if UNITY_EDITOR
    // TestCode Input
    TestInputActions inputActions;

    private void Awake()
    {
        inputActions = new TestInputActions();
    }

    private void OnEnable() 
    {
        inputActions.Enable();
        inputActions.Test.Test1.performed += OnTest1;
        inputActions.Test.Test2.performed += OnTest2;
        inputActions.Test.Test3.performed += OnTest3;
        inputActions.Test.Test4.performed += OnTest4;
        inputActions.Test.Test5.performed += OnTest5;
        inputActions.Test.Test5.performed += OnTest6;
    }

    private void OnDisable()
    {
        inputActions.Test.Test5.performed += OnTest6;
        inputActions.Test.Test5.performed -= OnTest5;
        inputActions.Test.Test4.performed -= OnTest4;
        inputActions.Test.Test3.performed -= OnTest3;
        inputActions.Test.Test1.performed -= OnTest1;
        inputActions.Test.Test2.performed -= OnTest2;
        inputActions.Disable();
    }

    protected virtual void OnTest6(InputAction.CallbackContext context)
    {
        //
    }
    protected virtual void OnTest5(InputAction.CallbackContext context)
    {
        //
    }

    protected virtual void OnTest4(InputAction.CallbackContext context)
    {
        //
    }

    protected virtual void OnTest3(InputAction.CallbackContext context)
    {
        //
    }

    protected virtual void OnTest2(InputAction.CallbackContext context)
    {
        //
    }

    protected virtual void OnTest1(InputAction.CallbackContext context)
    {
        //
    }
#endif
}