using UnityEngine;

[CreateAssetMenu(fileName = "Item_99", menuName = "ScriptableObject/ItemData", order = 1)]
public class ItemDataSO : ScriptableObject
{
    public Sprite icon;

    /// <summary>
    /// 아이템 이름
    /// </summary>
    public string itemName;

    /// <summary>
    /// 아이템 설명
    /// </summary>
    [TextArea]
    public string itemDescription;
}
