using Mono.Cecil;
using UnityEngine;

public class S_AttackerSoundEvent : MonoBehaviour
{
    private void S_AttackerAttackSound()
    {
        SoundManager.Instance.PlaySound(SFXType.attack_01);
    }

    private void S_AttackerDamagedSound()
    {
        int rand = UnityEngine.Random.Range(33, 35);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

    public void S_AttackerDeathSound()
    {
        SoundManager.Instance.PlaySound(SFXType.down_01);
    }

    private void S_AttackerMoveSound()
    {
        int rand = UnityEngine.Random.Range(37, 40);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

}
