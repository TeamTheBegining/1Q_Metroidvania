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

    // ���� �κ��丮 Ŀ���� ����Ű�� Ÿ���� �ܺο��� ����
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

    // �ܺο��� ���� Ŀ���� ����Ű�� �˾� Ÿ�� ����
    public void SetCursorType(PopupType type)
    {
        currentCursorType = type;
    }

    private void OpenPopup_started(InputAction.CallbackContext ctx)
    {
        // Ŀ���� ����Ű�� �˾� Ÿ������ ����
        TogglePopup(currentCursorType);
    }

    private void TogglePopup(PopupType type)
    {
        // �˾� ���� �ʱ�ȭ
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
            // �˾� �ݱ� �� �κ��丮 �Է� �ٽ� ����
        }
        else
        {
            // ���� ���� �˾� �ݱ�
            if (currentActivePopup != null)
                currentActivePopup.SetActive(false);

            targetPopup.SetActive(true);
            currentActivePopup = targetPopup;

            currentPopupType = type;
            RefreshSlotList(); // Ŀ�� ���� �ʱ�ȭ
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
        // TODO: �˾� �� ������ ���� ó��
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