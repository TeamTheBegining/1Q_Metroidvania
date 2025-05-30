using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum PopupType { Skill = 0, Equip, Special, Quest}

public class PopupController : MonoBehaviour
{
    [Header("Popup Windows")]
    [SerializeField] private GameObject SkillPopup;
    [SerializeField] private GameObject EquipPopup;
    [SerializeField] private GameObject SpecialPopup;
    [SerializeField] private GameObject QuestPopup;
    
    [Header("Slot Settings")]
    public Transform slotParent;
    public Image highlightImage;

    public GameObject currentActivePopup;
    public InventoryManager inventoryManager;

    public List<InventorySlotUI> slotList = new List<InventorySlotUI>();
    public int popupIndex = 0;

    private Dictionary<PopupType, GameObject> popupDict;
    private PopupType currentPopupType;

    // 현재 인벤토리 커서가 가리키는 타입을 외부에서 세팅
    private PopupType currentCursorType = PopupType.Skill;
    public PopupType CurrentCursorType => currentCursorType;

    private void OnEnable()
    {
        RefreshSlotList();
    }

    // 외부에서 현재 커서가 가리키는 팝업 타입 세팅
    public void SetCursorType(PopupType type)
    {
        currentCursorType = type;
    }

    public void TogglePopup(PopupType type) // 0519
    {
        // 팝업 사전 초기화
        if (popupDict == null)
        {
            popupDict = new Dictionary<PopupType, GameObject>
            {
                { PopupType.Skill, SkillPopup },
                { PopupType.Equip, EquipPopup },
                { PopupType.Special, SpecialPopup },
                { PopupType.Quest, QuestPopup }
            };
        }

        GameObject targetPopup = popupDict[type];

        bool isActive = targetPopup.activeSelf;

        if (isActive)
        {
            if (inventoryManager.isFocusToPopUI)
            {
                Debug.Log("inventory confirm!");
                inventoryManager.ConfirmSlot();
            }
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

        // 슬롯 타입
        if (currentCursorType == PopupType.Skill)
        {
            foreach (Transform child in SkillPopup.transform)
            {
                InventorySlotUI slot = child.GetComponent<InventorySlotUI>();
                if (slot != null)
                    slotList.Add(slot);
            }
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