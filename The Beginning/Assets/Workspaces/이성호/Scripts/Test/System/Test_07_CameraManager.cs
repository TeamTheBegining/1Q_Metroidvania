using UnityEngine;
using UnityEngine.InputSystem;

public class Test_07_CameraManager : TestBase
{
    public Transform target;
    public CameraType type;

    public int value;

    private void Start()
    {
        GameSceneManager.Instance.LoadSceneAddive(0);
        GameSceneManager.Instance.LoadSceneAddive(1);
        GameSceneManager.Instance.LoadSceneAddive(2);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        CameraManager.Instance.SetTarget(type, target);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        CameraManager.Instance.SetVirtualCameraPriority(type, value);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        GameSceneManager.Instance.UnloadScene(value);
    }
}