using UnityEngine;

public class PlayerSoundEvent : MonoBehaviour
{
    private void PlayerFootstepSound()
    {
        int rand = UnityEngine.Random.Range(0, 2);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

    private void PlayerSwordSound()
    {
        int rand = UnityEngine.Random.Range(3, 6);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
}
