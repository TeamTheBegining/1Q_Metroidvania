using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public ItemDataSO data;

    public void SetItem(ItemDataSO item)
    {
        data = item;

        if (icon != null)
        {
            icon.sprite = item != null ? item.icon : null;
            icon.enabled = item != null;
        }
    }

    public bool HasItem()
    {
        return data != null;
    }
}
