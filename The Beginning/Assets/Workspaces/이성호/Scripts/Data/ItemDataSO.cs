using UnityEngine;

public class ItemDataSO : ScriptableObject
{
    public Sprite icon;

    [Tooltip("저장할 위치를 찾기 위한 타입")] 
    public PopupType type; 

    /// <summary>
    /// 아이템 이름
    /// </summary>
    public string itemName;

    /// <summary>
    /// 아이템 설명
    /// </summary>
    [TextArea]
    public string itemDescription;

    public virtual void OnUseItem()
    { 
        // 인벤토리에서 confirm 시 호출될 내용
    }
}
