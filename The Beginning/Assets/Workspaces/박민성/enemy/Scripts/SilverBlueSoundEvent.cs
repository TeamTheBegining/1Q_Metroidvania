using UnityEngine;

public class SilverBlueSoundEvent : MonoBehaviour
{
    private void SilverBlueAttackSound()
    {
        int rand = UnityEngine.Random.Range(41, 42);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
    public void SilverBlueDamagedSound()
    {
        int rand = UnityEngine.Random.Range(43, 44);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
    private void SilverBlueDeathSound()
    {
        SoundManager.Instance.PlaySound(SFXType.t_DOWN_01);
    }
    private void SilverBlueMoveSound()
    {
        int rand = UnityEngine.Random.Range(46, 49);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
}
