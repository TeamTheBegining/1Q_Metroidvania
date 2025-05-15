using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Test_00_RootInteraction : TestBase
{
#if UNITY_EDITOR
    Interactable target;

    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        target = collision.gameObject.GetComponent<Interactable>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        target = null;
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if (target != null)
        {
            target.OnInteraction();
        }
    }
#endif
}