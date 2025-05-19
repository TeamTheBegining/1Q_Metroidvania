using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum PopupType { Skill = 0, Equip, Special, Quest}

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

    // ���� �κ��丮 Ŀ���� ����Ű�� Ÿ���� �ܺο��� ����
    private PopupType currentCursorType = PopupType.Skill;
    public PopupType CurrentCursorType => currentCursorType;

    private void OnEnable()
    {
        RefreshSlotList();
    }

    // �ܺο��� ���� Ŀ���� ����Ű�� �˾� Ÿ�� ����
    public void SetCursorType(PopupType type)
    {
        currentCursorType = type;
    }

    public void TogglePopup(PopupType type) // 0519
    {
        // �˾� ���� �ʱ�ȭ
        if (popupDict == null)
        {
            popupDict = new Dictionary<PopupType, GameObject>
            {
                { PopupType.Skill, skillPopup },
                { PopupType.Equip, equipPopup },
                { PopupType.Special, specialPopup },
                //{ PopupType.Quest, ����Ʈ�˾� }
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

    private void MoveHighlightToIndex()
    {
        if (highlightImage == null) return;

        if (popupIndex >= 0 && popupIndex < slotList.Count)
        {
            RectTransform slotRect = slotList[popupIndex].GetComponent<RectTransform>();
            highlightImage.transform.position = slotRect.position;
        }
    }
}