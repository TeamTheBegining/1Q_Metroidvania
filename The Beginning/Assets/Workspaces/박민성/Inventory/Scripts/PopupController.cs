using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum PopupType { Skill, Equip, Special }

public class PopupController : MonoBehaviour
{
    [Header("Popup Windows")]
    [SerializeField] private GameObject skillPopup;
    [SerializeField] private GameObject equipPopup;
    [SerializeField] private GameObject specialPopup;

    [Header("Slot Settings")]
    public Transform slotParent;
    public Image highlightImage;

    private List<InventorySlotUI> slotList = new List<InventorySlotUI>();
    private int popupIndex = 0;

    private Dictionary<PopupType, GameObject> popupDict;
    private PopupType currentPopupType;
    private GameObject currentActivePopup;

    private PlayerInputActions actions;

    // 현재 인벤토리 커서가 가리키는 타입을 외부에서 세팅
    private PopupType currentCursorType = PopupType.Skill;

    private void Awake()
    {
        actions = new PlayerInputActions();
        actions.UI.Enable();

        actions.UI.OpenPopup.started += OpenPopup_started;
        actions.UI.PopupMove.started += PopupMove_started;
        actions.UI.PopupConfirm.started += PopupConfirm_started;
        actions.UI.ClosePopup.started += ClosePopup_started;
    }

    private void OnEnable()
    {
        RefreshSlotList();
    }

    private void OnDisable()
    {
        actions.UI.PopupMove.started -= PopupMove_started;
        actions.UI.PopupConfirm.started -= PopupConfirm_started;
        actions.UI.OpenPopup.started -= OpenPopup_started;
        actions.UI.ClosePopup.started -= ClosePopup_started;
    }

    // 외부에서 현재 커서가 가리키는 팝업 타입 세팅
    public void SetCursorType(PopupType type)
    {
        currentCursorType = type;
    }

    private void OpenPopup_started(InputAction.CallbackContext ctx)
    {
        // 커서가 가리키는 팝업 타입으로 열기
        TogglePopup(currentCursorType);
    }

    private void TogglePopup(PopupType type)
    {
        // 팝업 사전 초기화
        if (popupDict == null)
        {
            popupDict = new Dictionary<PopupType, GameObject>
            {
                { PopupType.Skill, skillPopup },
                { PopupType.Equip, equipPopup },
                { PopupType.Special, specialPopup }
            };
        }

        GameObject targetPopup = popupDict[type];

        bool isActive = targetPopup.activeSelf;

        if (isActive)
        {
            targetPopup.SetActive(false);
            currentActivePopup = null;
            // 팝업 닫기 → 인벤토리 입력 다시 가능
        }
        else
        {
            // 기존 열린 팝업 닫기
            if (currentActivePopup != null)
                currentActivePopup.SetActive(false);

            targetPopup.SetActive(true);
            currentActivePopup = targetPopup;

            currentPopupType = type;
            RefreshSlotList(); // 커서 슬롯 초기화
        }
    }

    private void RefreshSlotList()
    {
        slotList.Clear();

        if (slotParent == null) return;

        foreach (Transform child in slotParent)
        {
            InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
            if (slot != null)
                slotList.Add(slot);
        }

        popupIndex = 0;
        MoveHighlightToIndex();
    }

    private void PopupMove_started(InputAction.CallbackContext context)
    {
        if (slotList.Count == 0) return;

        Vector2 dir = context.ReadValue<Vector2>();

        if (dir.x < 0)
            popupIndex--;
        else if (dir.x > 0)
            popupIndex++;

        if (popupIndex < 0) popupIndex = slotList.Count - 1;
        else if (popupIndex >= slotList.Count) popupIndex = 0;

        MoveHighlightToIndex();
    }

    private void MoveHighlightToIndex()
    {
        if (highlightImage == null) return;

        if (popupIndex >= 0 && popupIndex < slotList.Count)
        {
            RectTransform slotRect = slotList[popupIndex].GetComponent<RectTransform>();
            highlightImage.transform.position = slotRect.position;
        }
    }

    private void PopupConfirm_started(InputAction.CallbackContext ctx)
    {
        // TODO: 팝업 내 아이템 선택 처리
        Debug.Log($"Popup {currentPopupType} Confirm at index {popupIndex}");
    }

    private void ClosePopup_started(InputAction.CallbackContext ctx)
    {
        if (currentActivePopup != null)
        {
            currentActivePopup.SetActive(false);
            currentActivePopup = null;
        }
    }
}