using UnityEngine;
using UnityEngine.InputSystem;

public class Test_07_CameraManager : TestBase
{
    public Transform target;
    public CameraType type;

    public int value;

    private void Start()
    {
        GameSceneManager.Instance.ChangeScene(0, true);
        GameSceneManager.Instance.ChangeScene(1, true);
        GameSceneManager.Instance.ChangeScene(2, true);
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