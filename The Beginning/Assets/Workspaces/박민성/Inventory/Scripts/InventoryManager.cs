using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System;

// InventoryManager.cs
public class InventoryManager : MonoBehaviour
{
    public PopupController popupController; // 0519

    PlayerInputActions actions;

    [Header("Slots & Cursor")]
    public List<Image> allSlots;
    public List<Image> popupSlots;
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
    private int popupSelectedIndex = 0;

    private bool isInventoryOpen = false;

    private List<int>[] grid = new List<int>[3];
    private List<int>[] p_grid = new List<int>[2];
    [SerializeField] int currentGridX = 0;
    [SerializeField] int currentGridY = 0;
    [SerializeField] int currentPGridX = 0;
    [SerializeField] int currentPGridY = 0;

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
        EnterPopupGridPositions();
        HideItemInfo();
        actions.UI.InventoryOpen.started += InventoryOpen_started;
    }

    private void InitializeSlotGridPositions()
    {
        grid[0] = new List<int> { 0, 1 };
        grid[1] = new List<int> { 2, 3, 4 };
        grid[2] = new List<int> { 5, 6, 7 };
    }
    private void EnterPopupGridPositions()
    {
        p_grid[0] = new List<int> { 0, 1 };
        p_grid[1] = new List<int> { 2, 3 };
    }

    private void InventoryOpen_started(InputAction.CallbackContext obj)
    {
        Debug.Log("i Ű ����");
        ToggleInventory();
    }

    private void InventoryConfirm_started(InputAction.CallbackContext obj)
    {
        if (IsPopupOpen())
        {
            ConfirmSlot(); // �˾� ���� ó��
        }
        else
        {
            HandleSlotSelect();
        }
    }

    private void InventoryMove_started(InputAction.CallbackContext obj)
    {
        Vector2 dir = obj.ReadValue<Vector2>();
        Debug.Log($"Move : {dir}");
        if (IsPopupOpen())
            PopupSelection(dir);
        else 
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

    public void PopupSelection(Vector2 input)
    {
        int nextY = Mathf.Clamp(currentPGridY - (int)input.y, 0, p_grid.Length - 1);
        int nextX = currentPGridX + (int)input.x;
        // ���õ� Y�࿡�� ������ X �ε��� ���� üũ
        int maxX = p_grid[nextY].Count - 1;
        nextX = Mathf.Clamp(nextX, 0, maxX);

        currentPGridY = nextY;
        currentPGridX = nextX;

        UpdatePopupSlotHighlight();
    }

    private void UpdatePopupSlotHighlight()
    {
        if (p_grid[currentPGridY].Count <= currentPGridX)
            return;

        popupSelectedIndex = p_grid[currentPGridY][currentPGridX];

        for (int i = 0; i < popupSlots.Count; i++)
        {
            if (popupSlots[i] != null)
                popupSlots[i].color = (i == popupSelectedIndex) ? highlightColor : normalColor;
        }

        if (highlightCursor != null && popupSelectedIndex < popupSlots.Count)
        {
            RectTransform cursorRect = highlightCursor.GetComponent<RectTransform>();
            RectTransform slotRect = popupSlots[popupSelectedIndex].GetComponent<RectTransform>();
            cursorRect.position = slotRect.position;
        }
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

        //itemInfoPanel.SetActTogglePopupive(true);
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
    private bool IsPopupOpen()
    {
        // Ensure we are referencing the correct instance of PopupController
        return popupController != null && popupController.currentActivePopup != null && popupController.currentActivePopup.activeSelf;
    }


    private void OpenPopup_started(InputAction.CallbackContext obj)
    {
        // �˾��� ���� ������ �ݱ�
        if (IsPopupOpen())
        {
            ClosePopup_started(obj);
            return;
        }

        // �˾� ����
        isFocusToPopUI = true;
        savedGridX = currentGridX;
        savedGridY = currentGridY;

        popupController.TogglePopup(popupController.CurrentCursorType);
        foreach (Transform child in popupController.currentActivePopup.transform)
        {
            popupSlots.Add(child.gameObject.GetComponent<Image>());
        }
        RectTransform cursorRect = highlightCursor.GetComponent<RectTransform>();
        RectTransform slotRect = popupSlots[0].GetComponent<RectTransform>();
        cursorRect.position = slotRect.position;
        currentPGridX = 0;
        currentPGridY = 0;
    }

    // �˾��� ���� �� ȣ���
    private void ClosePopup_started(InputAction.CallbackContext obj)
    {
        popupController.TogglePopup(popupController.CurrentCursorType); // �ݱ� �õ�
        isFocusToPopUI = false;

        currentGridX = savedGridX;
        currentGridY = savedGridY;

        if (highlightCursor != null)
            highlightCursor.SetActive(true);

        actions.UI.InventoryMove.Enable();
        UpdateSlotHighlight();

        Debug.Log("Popup ����, Ŀ�� ���� �Ϸ�");
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

    private void ConfirmSlot()
    {
        if (popupController == null || popupController.slotList == null || popupController.slotList.Count == 0) return;

        var selectedSlot = popupController.slotList[popupController.popupIndex]; // ���� ���õ� �˾� ���� ����
        if (selectedSlot != null && selectedSlot.HasItem())
        {
            Debug.Log("�˾� ������ ���: " + selectedSlot.heldItem.itemName);
            // TODO: ���� ������ ��� ���� ����
        }
    }

}