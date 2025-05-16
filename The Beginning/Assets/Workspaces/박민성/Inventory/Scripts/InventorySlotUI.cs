using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public ItemData heldItem;

    public void SetItem(ItemData item)
    {
        heldItem = item;

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null;
        }
    }

    public bool HasItem()
    {
        return heldItem != null;
    }
}
