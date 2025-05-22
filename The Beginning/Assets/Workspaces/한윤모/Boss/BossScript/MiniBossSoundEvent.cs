using UnityEngine;

public class MiniBossSoundEvent : MonoBehaviour
{
    private void MiniBossShieldAttackSound()
    {
        SoundManager.Instance.PlaySound(SFXType.attack_A01);
    }
    private void MiniBossSmashAttackSound()
    {
        SoundManager.Instance.PlaySound(SFXType.attack_B01);
    }
    private void MiniBossTornadoSound()
    {
        SoundManager.Instance.PlaySound(SFXType.attack_C01);
    }
    private void MiniBossDamagedSound()
    {
        int rand = UnityEngine.Random.Range(28, 30);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

    private void MiniBossDeathSound()
    {
        SoundManager.Instance.PlaySound(SFXType.MiniBoss_Death_01);
    }
}
