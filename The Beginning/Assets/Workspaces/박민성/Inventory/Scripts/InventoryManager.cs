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

    [Header("UI 그룹")]
    public GameObject inventoryUI;

    private int selectedIndex = 0;
    private int lastInventoryIndex = 0;

    private bool isInventoryOpen = false;

    private List<int>[] grid = new List<int>[3];
    [SerializeField] int currentGridX = 0;
    [SerializeField] int currentGridY = 0;

    // 팝업 연동용
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

        // 팝업 이벤트 연결 (PopupToggleManager 이벤트가 있을 경우 연결)
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
        Debug.Log("i 키 누름");
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

        Debug.Log("인벤토리 상태: " + (isInventoryOpen ? "열림" : "닫힘"));
    }

    public void MoveSelection(Vector2 input)
    {
        int nextY = Mathf.Clamp(currentGridY - (int)input.y, 0, grid.Length - 1);
        int nextX = currentGridX + (int)input.x;

        // 선택된 Y행에서 가능한 X 인덱스 범위 체크
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

        // 커서 타입 업데이트 0519
        if(selectedIndex == 0 || selectedIndex == 2) // 장비
        {
            popupController.SetCursorType(PopupType.Equip);
        }
        else if(selectedIndex == 1) // 스킬
        {
            popupController.SetCursorType(PopupType.Skill);
        }
        else if (selectedIndex == 5) // 퀘스트
        {
            popupController.SetCursorType(PopupType.Quest); // TODO : 퀘스트 팝업 연결하기
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
    // 팝업이 열릴 때 호출됨
    private void OpenPopup_started(InputAction.CallbackContext obj)
    {
        isFocusToPopUI = true;
        savedGridX = currentGridX;
        savedGridY = currentGridY;
        popupController.TogglePopup(popupController.CurrentCursorType); // 0519

        // popup ui 분기
        if (!isFocusToPopUI)
        {
            if (highlightCursor != null) // 하이라이트 비활성화
            {
                highlightCursor.SetActive(false);
            }

            Debug.Log("popup UI active");
        }
        else // 팝업이 활성화 되었을 때
        {
            if (highlightCursor != null) // 하이라이트 활성화
            {
                highlightCursor.SetActive(true);
            }
            Debug.Log("popup UI deactive");
            // TODO: 아이템 상호작용 해야됌.
        }
    }

    // 팝업이 닫힐 때 호출됨
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

    // 외부에서 호출할 복귀용 함수 (유지)
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