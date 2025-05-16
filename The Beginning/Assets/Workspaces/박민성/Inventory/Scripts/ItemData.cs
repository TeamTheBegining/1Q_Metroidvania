using UnityEngine;
public enum ItemType
{
    Skill,
    Equipment,
    //Consumable,
    QuestItem,
    Special
}

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public string description;
    public ItemType itemType;
}
