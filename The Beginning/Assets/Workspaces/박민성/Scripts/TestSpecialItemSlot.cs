using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSpecialItemSlot : MonoBehaviour
{
    [Header("UI 슬롯들 (slot1 ~ slot4)")]
    public List<Image> specialItemSlots;

    [Header("아이템 스프라이트들")]
    public List<Sprite> itemSprites;

    void Start()
    {
        foreach (var slot in specialItemSlots)
        {
            if (slot != null)
            {
                slot.sprite = null;
                slot.color = new Color(1, 1, 1, 0.1f); // 투명하게 시작
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetItemToSlot(0, 0); // slot1에 itemSprites[0]
            Debug.Log("아이템 습득!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetItemToSlot(1, 1); // slot2에 itemSprites[1]
            Debug.Log("아이템 습득!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetItemToSlot(2, 2); // slot3에 itemSprites[2]
            Debug.Log("아이템 습득!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetItemToSlot(3, 3); // slot4에 itemSprites[3]
            Debug.Log("아이템 습득!");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var slot in specialItemSlots)
            {
                if (slot != null)
                {
                    slot.sprite = null;
                    slot.color = new Color(1, 1, 1, 0.1f); // 다시 투명하게
                }
            }
        }
    }

    void SetItemToSlot(int slotIndex, int spriteIndex)
    {
        if (slotIndex >= specialItemSlots.Count || spriteIndex >= itemSprites.Count)
        {
            Debug.LogWarning("인덱스 초과! 슬롯 또는 스프라이트 범위를 확인하세요.");
            return;
        }

        Image targetSlot = specialItemSlots[slotIndex];
        targetSlot.sprite = itemSprites[spriteIndex];
        targetSlot.color = Color.white; // 보이게 설정
    }
}
