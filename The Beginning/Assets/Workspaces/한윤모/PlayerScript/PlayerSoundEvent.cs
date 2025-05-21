using UnityEngine;

public class PlayerSoundEvent : MonoBehaviour
{
    private void PlayerSwordSound()
    {
        int rand = UnityEngine.Random.Range(0, 3);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

    private void PlayerDeathSound()
    {
        int rand = UnityEngine.Random.Range(4, 6);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }

    private void PlayerFootstepSound()
    {
        int rand = UnityEngine.Random.Range(7, 9);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
    private void PlayerJumpSound()
    {
        //int rand = UnityEngine.Random.Range(10, 10);      랜덤하게 할 경우
        //SoundManager.Instance.PlaySound((SFXType)rand);
        SoundManager.Instance.PlaySound(SFXType.jump_01);
    }
    private void PlayerLadderSound()
    {
        int rand = UnityEngine.Random.Range(11, 14);
        SoundManager.Instance.PlaySound((SFXType)rand);
    }
    private void PlayerLandingSound()
    {
        //int rand = UnityEngine.Random.Range(15, 15);      랜덤하게 할 경우
        //SoundManager.Instance.PlaySound((SFXType)rand);
        SoundManager.Instance.PlaySound(SFXType.landing_01);
    }
    private void PlayerParryingHitSound()
    {
        //int rand = UnityEngine.Random.Range(16, 16);      랜덤하게 할 경우
        //SoundManager.Instance.PlaySound((SFXType)rand);
        SoundManager.Instance.PlaySound(SFXType.parrying_Hit_01);
    }
    private void PlayerParryingSwingSound()
    {
        //int rand = UnityEngine.Random.Range(17, 17);      랜덤하게 할 경우
        //SoundManager.Instance.PlaySound((SFXType)rand);
        SoundManager.Instance.PlaySound(SFXType.parrying_Swing_01);
    }
    private void PlayerSlidingSound()
    {
        //int rand = UnityEngine.Random.Range(18, 18);      랜덤하게 할 경우
        //SoundManager.Instance.PlaySound((SFXType)rand);
        SoundManager.Instance.PlaySound(SFXType.slide_01);
    }

}
