using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_Interaction : TestBase
{
#if UNITY_EDITOR
    public GameObject objTarget;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        Interactable target = objTarget.GetComponent<Interactable>();
        if (target == null) return;

        target.OnInteraction();
    }
#endif
}