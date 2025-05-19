using UnityEngine;

[CreateAssetMenu(fileName = "Item_99", menuName = "ScriptableObject/ItemData/StatUp", order = 0)]
public class SkillUnlockItemSO : ItemDataSO
{
    public PlayerSkillType type;

    public void UnlockSkill()
    {
        PlayerManager.Instance.UnlockPlayerSkill(type);
    }
}
