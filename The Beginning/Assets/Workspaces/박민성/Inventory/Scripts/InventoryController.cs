using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    private GameInputActions inputActions;

    [Header("�κ��丮 �Ŵ��� ����")]
    [SerializeField] private InventoryManager inventoryManager;

    void Awake()
    {
        inputActions = new GameInputActions();

        inputActions.UI.OpenInventory.performed += ctx => inventoryManager.ToggleInventory();

/*        inputActions.UI.Navigate.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            inventoryManager.MoveSelection(input);
        };*/
    }

    void OnEnable()
    {
        inputActions.UI.Enable();
    }

    void OnDisable()
    {
        inputActions.UI.Disable();
    }
}