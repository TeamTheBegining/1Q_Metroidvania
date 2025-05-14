using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSpecialItemSlot : MonoBehaviour
{
    [Header("UI ���Ե� (slot1 ~ slot4)")]
    public List<Image> specialItemSlots;

    [Header("������ ��������Ʈ��")]
    public List<Sprite> itemSprites;

    void Start()
    {
        foreach (var slot in specialItemSlots)
        {
            if (slot != null)
            {
                slot.sprite = null;
                slot.color = new Color(1, 1, 1, 0.1f); // �����ϰ� ����
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetItemToSlot(0, 0); // slot1�� itemSprites[0]
            Debug.Log("������ ����!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetItemToSlot(1, 1); // slot2�� itemSprites[1]
            Debug.Log("������ ����!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetItemToSlot(2, 2); // slot3�� itemSprites[2]
            Debug.Log("������ ����!");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetItemToSlot(3, 3); // slot4�� itemSprites[3]
            Debug.Log("������ ����!");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var slot in specialItemSlots)
            {
                if (slot != null)
                {
                    slot.sprite = null;
                    slot.color = new Color(1, 1, 1, 0.1f); // �ٽ� �����ϰ�
                }
            }
        }
    }

    void SetItemToSlot(int slotIndex, int spriteIndex)
    {
        if (slotIndex >= specialItemSlots.Count || spriteIndex >= itemSprites.Count)
        {
            Debug.LogWarning("�ε��� �ʰ�! ���� �Ǵ� ��������Ʈ ������ Ȯ���ϼ���.");
            return;
        }

        Image targetSlot = specialItemSlots[slotIndex];
        targetSlot.sprite = itemSprites[spriteIndex];
        targetSlot.color = Color.white; // ���̰� ����
    }
}
