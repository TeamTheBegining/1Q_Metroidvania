using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_09_ImpluseCamera : TestBase
{
    public Vector3 velocity;
    public float force;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        CameraManager.Instance.MakeImpulse();
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        CameraManager.Instance.MakeImpulse(force);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        CameraManager.Instance.MakeImpulse(velocity);
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        GameManager.Instance.MiddleMessagePanel.Show();
        GameManager.Instance.MiddleMessagePanel.SetText("09. Text Message box Impluse Test");
    }
}
