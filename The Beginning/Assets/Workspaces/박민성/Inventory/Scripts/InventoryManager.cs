using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// InventoryManager.cs
public class InventoryManager : MonoBehaviour
{
    public PopupController popupController; // 0519

    PlayerInputActions actions;

    [Header("Slots & Cursor")]
    public List<Image> allSlots;
    public GameObject highlightCursor;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    [Header("Item Info UI")]
    public GameObject itemInfoPanel;
    public Text itemNameText;
    public Image itemIcon;
    public Text itemDescriptionText;

    [Header("UI �׷�")]
    public GameObject inventoryUI;

    private int selectedIndex = 0;
    private int lastInventoryIndex = 0;

    private bool isInventoryOpen = false;

    private List<int>[] grid = new List<int>[3];
    [SerializeField] int currentGridX = 0;
    [SerializeField] int currentGridY = 0;

    // �˾� ������
    private int savedGridX = 0;
    private int savedGridY = 0;

    private void Awake()
    {
        actions = new PlayerInputActions();
        actions.UI.InventoryOpen.Enable();

        popupController = GetComponent<PopupController>(); // 0519
    }

    private void OnEnable()
    {
        actions.UI.InventoryMove.Enable();
        actions.UI.InventoryMove.started += InventoryMove_started;
        actions.UI.InventoryConfirm.started += InventoryConfirm_started;

        // �˾� �̺�Ʈ ���� (PopupToggleManager �̺�Ʈ�� ���� ��� ����)
        actions.UI.OpenPopup.Enable();
        actions.UI.OpenPopup.started += OpenPopup_started;
        actions.UI.ClosePopup.Enable();
        actions.UI.ClosePopup.started += ClosePopup_started;
    }

    private void OnDisable()
    {
        actions.UI.InventoryMove.started -= InventoryMove_started;
        actions.UI.InventoryConfirm.started -= InventoryConfirm_started;
        actions.UI.InventoryMove.Disable();

        actions.UI.OpenPopup.started -= OpenPopup_started;
        actions.UI.OpenPopup.Disable();
        actions.UI.ClosePopup.started -= ClosePopup_started;
        actions.UI.ClosePopup.Disable();
    }

    private void Start()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        InitializeSlotGridPositions();
        HideItemInfo();
        actions.UI.InventoryOpen.started += InventoryOpen_started;
    }

    private void InitializeSlotGridPositions()
    {
        grid[0] = new List<int> { 0, 1 };
        grid[1] = new List<int> { 2, 3, 4 };
        grid[2] = new List<int> { 5, 6, 7 };
    }

    private void InventoryOpen_started(InputAction.CallbackContext obj)
    {
        Debug.Log("i Ű ����");
        ToggleInventory();
    }

    private void InventoryConfirm_started(InputAction.CallbackContext obj)
    {
        Debug.Log("Enter");
        Debug.Log($"--- InventoryConfirm_started");

        HandleSlotSelect();
    }

    private void InventoryMove_started(InputAction.CallbackContext obj)
    {
        Vector2 dir = obj.ReadValue<Vector2>();
        Debug.Log($"Move : {dir}");
        MoveSelection(dir);
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryUI != null)
            inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            UpdateSlotHighlight();
        else
            HideItemInfo();

        Debug.Log("�κ��丮 ����: " + (isInventoryOpen ? "����" : "����"));
    }

    public void MoveSelection(Vector2 input)
    {
        int nextY = Mathf.Clamp(currentGridY - (int)input.y, 0, grid.Length - 1);
        int nextX = currentGridX + (int)input.x;

        // ���õ� Y�࿡�� ������ X �ε��� ���� üũ
        int maxX = grid[nextY].Count - 1;
        nextX = Mathf.Clamp(nextX, 0, maxX);

        currentGridY = nextY;
        currentGridX = nextX;

        UpdateSlotHighlight();
    }

    private void UpdateSlotHighlight()
    {
        if (grid[currentGridY].Count <= currentGridX)
            return;

        selectedIndex = grid[currentGridY][currentGridX];

        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i] != null)
                allSlots[i].color = (i == selectedIndex) ? highlightColor : normalColor;
        }

        if (highlightCursor != null && selectedIndex < allSlots.Count)
        {
            RectTransform cursorRect = highlightCursor.GetComponent<RectTransform>();
            RectTransform slotRect = allSlots[selectedIndex].GetComponent<RectTransform>();
            cursorRect.position = slotRect.position;
        }

        // Ŀ�� Ÿ�� ������Ʈ 0519
        if(selectedIndex == 0 || selectedIndex == 2) // ���
        {
            popupController.SetCursorType(PopupType.Equip);
        }
        else if(selectedIndex == 1) // ��ų
        {
            popupController.SetCursorType(PopupType.Skill);
        }
        else if (selectedIndex == 5) // ����Ʈ
        {
            popupController.SetCursorType(PopupType.Quest); // TODO : ����Ʈ �˾� �����ϱ�
        }
        else if(selectedIndex == 3 ||
            selectedIndex == 4 ||
            selectedIndex == 6 ||
            selectedIndex == 7)
        {
            popupController.SetCursorType(PopupType.Special);
        }
    }

    private void HandleSlotSelect()
    {
        var selectedSlot = allSlots[selectedIndex];
        var slotUI = selectedSlot.GetComponent<InventorySlotUI>();

        if (slotUI != null && slotUI.HasItem())
            ShowItemInfo(slotUI.heldItem);
        else
            HideItemInfo();
    }

    private void ShowItemInfo(ItemData item)
    {
        if (itemInfoPanel == null) return;

        itemInfoPanel.SetActive(true);
        itemNameText.text = item.itemName;
        itemIcon.sprite = item.icon;
        itemDescriptionText.text = item.description;
    }

    private void HideItemInfo()
    {
        if (itemInfoPanel != null)
            itemInfoPanel.SetActive(false);
    }

    private bool isFocusToPopUI = false;
    // �˾��� ���� �� ȣ���
    private void OpenPopup_started(InputAction.CallbackContext obj)
    {
        isFocusToPopUI = true;
        savedGridX = currentGridX;
        savedGridY = currentGridY;
        popupController.TogglePopup(popupController.CurrentCursorType); // 0519

        // popup ui �б�
        if (!isFocusToPopUI)
        {
            if (highlightCursor != null) // ���̶���Ʈ ��Ȱ��ȭ
            {
                highlightCursor.SetActive(false);
            }

            Debug.Log("popup UI active");
        }
        else // �˾��� Ȱ��ȭ �Ǿ��� ��
        {
            if (highlightCursor != null) // ���̶���Ʈ Ȱ��ȭ
            {
                highlightCursor.SetActive(true);
            }
            Debug.Log("popup UI deactive");
            // TODO: ������ ��ȣ�ۿ� �ؾ߉�.
        }
    }

    // �˾��� ���� �� ȣ���
    private void ClosePopup_started(InputAction.CallbackContext obj)
    {
        currentGridX = savedGridX;
        currentGridY = savedGridY;
        actions.UI.InventoryMove.Enable();
        if (highlightCursor != null)
        {
            highlightCursor.SetActive(true);
        }

        popupController.TogglePopup(popupController.CurrentCursorType); // 0519
        UpdateSlotHighlight();

        Debug.Log("Inventory input re-enabled and cursor restored (popup closed).");
    }

    // �ܺο��� ȣ���� ���Ϳ� �Լ� (����)
    public void ReturnCursorToLastInventorySlot()
    {
        selectedIndex = lastInventoryIndex;

        for (int y = 0; y < grid.Length; y++)
        {
            int x = grid[y].IndexOf(selectedIndex);
            if (x >= 0)
            {
                currentGridX = x;
                currentGridY = y;
                break;
            }
        }

        UpdateSlotHighlight();
    }

    public void SaveCurrentInventoryIndex()
    {
        lastInventoryIndex = selectedIndex;
    }

    public int GetSelectedIndex()
    {
        return selectedIndex;
    }

    public void MoveCursorToSelectedIndex(int index)
    {
        selectedIndex = index;

        for (int y = 0; y < grid.Length; y++)
        {
            int x = grid[y].IndexOf(index);
            if (x >= 0)
            {
                currentGridX = x;
                currentGridY = y;
                break;
            }
        }

        UpdateSlotHighlight();
    }
}