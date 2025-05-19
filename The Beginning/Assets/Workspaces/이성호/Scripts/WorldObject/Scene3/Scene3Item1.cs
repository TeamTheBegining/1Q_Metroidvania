using UnityEngine;

public class Scene3Item1 : MonoBehaviour, Interactable
{
    public void OnInteraction()
    {
        PlayerManager.Instance.UnlockPlayerSkill(PlayerSkillType.DoubleJump);
        Destroy(this.gameObject);
    }
}
