using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private float inputCooldown = 0.05f; // 0.05초 간격
    private float lastInputTime = 0f;

    [Header("UI 그룹")]
    public GameObject inventoryUI;

    [Header("슬롯 이미지들")]
    public List<Image> allSlots; // 총 8칸
    public Color normalColor = new Color(1, 1, 1, 0.1f);
    public Color highlightColor = Color.yellow;

    [Header("스페셜 아이템 슬롯")]
    public List<Image> specialItemSlots;
    public List<Sprite> itemSprites;

    [Header("스킬 슬롯")]
    public Image skillIcon;
    public Sprite newSkillSprite;

    private bool isInventoryOpen = false;
    private int selectedIndex = 0;
    private bool hasSkill = false;

    // 슬롯 위치 (UI 상 가상 그리드 기준)
    private readonly Dictionary<int, Vector2Int> slotGridPositions = new()
    {
        { 0, new Vector2Int(0, 0) },
        { 1, new Vector2Int(0, 1) },
        { 2, new Vector2Int(0, 2) },
        { 3, new Vector2Int(1, 0) },
        { 4, new Vector2Int(1, 1) },
        { 5, new Vector2Int(2, 1) },
        { 6, new Vector2Int(1, 2) },
        { 7, new Vector2Int(2, 2) },
    };

    void Start()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        ResetAllSlots();
        UpdateSlotHighlight();

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
            skillIcon.color = normalColor;
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryUI != null)
            inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
            UpdateSlotHighlight();

        Debug.Log("인벤토리 상태: " + (isInventoryOpen ? "열림" : "닫힘"));
    }

    void Update()
    {
        if (!isInventoryOpen)
            return;

        // 아이템 장착 (1~4번 키) 및 스킬 획득
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetItemToSlot(0, 0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetItemToSlot(1, 1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetItemToSlot(2, 2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetItemToSlot(3, 3);

        // 스킬 획득
        if (!hasSkill && Input.GetKeyDown(KeyCode.K))
            AcquireSkill();
        // 아이템 창 비우기
        if (Input.GetKeyDown(KeyCode.E))
            ResetAllSlots();
    }

    void MoveCursor(Vector2Int direction)
    {
        if (!slotGridPositions.ContainsKey(selectedIndex)) return;

        Vector2Int currentPos = slotGridPositions[selectedIndex];
        Vector2Int targetPos = currentPos + direction;

        foreach (var kvp in slotGridPositions)
        {
            if (kvp.Value == targetPos)
            {
                selectedIndex = kvp.Key;
                UpdateSlotHighlight();
                return;
            }
        }
    }

    void UpdateSlotHighlight()
    {
        for (int i = 0; i < allSlots.Count; i++)
        {
            if (allSlots[i] == null) continue;
            allSlots[i].color = (i == selectedIndex) ? highlightColor : normalColor;
        }
    }

    void SetItemToSlot(int slotIndex, int spriteIndex)
    {
        if (slotIndex >= specialItemSlots.Count || spriteIndex >= itemSprites.Count)
        {
            Debug.LogWarning("인덱스 범위 오류!");
            return;
        }

        Image targetSlot = specialItemSlots[slotIndex];
        targetSlot.sprite = itemSprites[spriteIndex];
        targetSlot.color = Color.white;
        Debug.Log($"슬롯 {slotIndex + 1}에 아이템 장착!");
    }

    void AcquireSkill()
    {
        if (skillIcon != null && newSkillSprite != null)
        {
            skillIcon.sprite = newSkillSprite;
            skillIcon.enabled = true;
            hasSkill = true;
            skillIcon.color = Color.white;
            Debug.Log("스킬 습득 완료!");
        }
    }

    void ResetAllSlots()
    {
        foreach (var slot in specialItemSlots)
        {
            if (slot != null)
            {
                slot.sprite = null;
                slot.color = normalColor;
            }
        }
    }

    public void MoveSelection(Vector2 input)
    {
        if (Time.time - lastInputTime < inputCooldown)
            return;

        Vector2Int direction = Vector2Int.zero;

        // 입력값이 위쪽이면 Up 이동 (UI 그리드에서 y축 기준 주의!)
        if (input.y > 0.5f)
            direction = Vector2Int.down;
        else if (input.y < -0.5f)
            direction = Vector2Int.up;

        if (input.x < -0.5f)
            direction = Vector2Int.left;
        else if (input.x > 0.5f)
            direction = Vector2Int.right;

        if (direction != Vector2Int.zero)
        {
            MoveCursor(direction);
            lastInputTime = Time.time;
        }
    }
}


//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class InventoryManager : MonoBehaviour
//{

//    private float inputCooldown = 0.2f; // 0.2초 간격
//    private float lastInputTime = 0f;

//    [Header("UI 그룹")]
//    public GameObject inventoryUI;

//    [Header("슬롯 이미지들")]
//    public List<Image> allSlots; // 총 8칸
//    public Color normalColor = new Color(1, 1, 1, 0.1f);
//    public Color highlightColor = Color.yellow;

//    [Header("스페셜 아이템 슬롯")]
//    public List<Image> specialItemSlots;
//    public List<Sprite> itemSprites;

//    [Header("스킬 슬롯")]
//    public Image skillIcon;
//    public Sprite newSkillSprite;

//    private bool isInventoryOpen = false;
//    private int selectedIndex = 0;
//    private bool hasSkill = false;

//    // 슬롯 위치 (UI 상 가상 그리드 기준)
//    private readonly Dictionary<int, Vector2Int> slotGridPositions = new()
//    {
//        { 0, new Vector2Int(0, 0) },
//        { 1, new Vector2Int(0, 1) },
//        { 2, new Vector2Int(0, 2) },
//        { 3, new Vector2Int(1, 0) },
//        { 4, new Vector2Int(1, 1) },
//        { 5, new Vector2Int(2, 1) },
//        { 6, new Vector2Int(1, 2) },
//        { 7, new Vector2Int(2, 2) },
//    };

//    void Start()
//    {
//        if (inventoryUI != null)
//            inventoryUI.SetActive(false);

//        ResetAllSlots();
//        UpdateSlotHighlight();

//        if (skillIcon != null)
//        {
//            skillIcon.sprite = null;
//            skillIcon.color = normalColor;
//            skillIcon.enabled = false;
//        }
//    }

//    public void ToggleInventory()
//    {
//        isInventoryOpen = !isInventoryOpen;
//        if (inventoryUI != null)
//            inventoryUI.SetActive(isInventoryOpen);

//        if (isInventoryOpen)
//            UpdateSlotHighlight();

//        Debug.Log("인벤토리 상태: " + (isInventoryOpen ? "열림" : "닫힘"));
//    }

//    void Update()
//    {

//        if (!isInventoryOpen)
//            return;

//        // 방향키 및 WASD 이동
//        Vector2Int move = Vector2Int.zero;
//        if (Input.GetKeyDown(KeyCode.UpArrow)) move = Vector2Int.down;
//        if (Input.GetKeyDown(KeyCode.DownArrow)) move = Vector2Int.up;
//        if (Input.GetKeyDown(KeyCode.LeftArrow)) move = Vector2Int.left;
//        if (Input.GetKeyDown(KeyCode.RightArrow)) move = Vector2Int.right;

//        if (move != Vector2Int.zero)
//            MoveCursor(move);

//        // 아이템 장착 (1~4번 키)
//        if (Input.GetKeyDown(KeyCode.Alpha1)) SetItemToSlot(0, 0);
//        if (Input.GetKeyDown(KeyCode.Alpha2)) SetItemToSlot(1, 1);
//        if (Input.GetKeyDown(KeyCode.Alpha3)) SetItemToSlot(2, 2);
//        if (Input.GetKeyDown(KeyCode.Alpha4)) SetItemToSlot(3, 3);

//        // 스킬 획득
//        if (!hasSkill && Input.GetKeyDown(KeyCode.K))
//            AcquireSkill();

//        // 아이템 제거
//        if (Input.GetKeyDown(KeyCode.E))
//            ResetAllSlots();
//    }

//    void MoveCursor(Vector2Int direction)
//    {
//        if (!slotGridPositions.ContainsKey(selectedIndex)) return;

//        Vector2Int currentPos = slotGridPositions[selectedIndex];
//        Vector2Int targetPos = currentPos + direction;

//        foreach (var kvp in slotGridPositions)
//        {
//            if (kvp.Value == targetPos)
//            {
//                selectedIndex = kvp.Key;
//                UpdateSlotHighlight();
//                return;
//            }
//        }
//    }

//    void UpdateSlotHighlight()
//    {
//        for (int i = 0; i < allSlots.Count; i++)
//        {
//            if (allSlots[i] == null) continue;
//            allSlots[i].color = (i == selectedIndex) ? highlightColor : normalColor;
//        }
//    }

//    void SetItemToSlot(int slotIndex, int spriteIndex)
//    {
//        if (slotIndex >= specialItemSlots.Count || spriteIndex >= itemSprites.Count)
//        {
//            Debug.LogWarning("인덱스 범위 오류!");
//            return;
//        }

//        Image targetSlot = specialItemSlots[slotIndex];
//        targetSlot.sprite = itemSprites[spriteIndex];
//        targetSlot.color = Color.white;
//        Debug.Log($"슬롯 {slotIndex + 1}에 아이템 장착!");
//    }

//    void AcquireSkill()
//    {
//        if (skillIcon != null && newSkillSprite != null)
//        {
//            skillIcon.sprite = newSkillSprite;
//            skillIcon.enabled = true;
//            hasSkill = true;
//            skillIcon.color = Color.white;
//            Debug.Log("스킬 습득 완료!");
//        }
//    }

//    void ResetAllSlots()
//    {
//        foreach (var slot in specialItemSlots)
//        {
//            if (slot != null)
//            {
//                slot.sprite = null;
//                slot.color = normalColor;
//            }
//        }
//    }
//    public void MoveSelection(Vector2 input)
//    {
//        if (Time.time - lastInputTime < inputCooldown)
//            return;

//        Vector2Int direction = Vector2Int.zero;

//        if (input.y > 0.5f) direction = Vector2Int.down;     // 위 → 아래 이동
//        else if (input.y < -0.5f) direction = Vector2Int.up;

//        if (input.x < -0.5f) direction = Vector2Int.left;
//        else if (input.x > 0.5f) direction = Vector2Int.right;

//        if (direction != Vector2Int.zero)
//        {
//            MoveCursor(direction);
//            lastInputTime = Time.time;
//        }
//    }


//}
