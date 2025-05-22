using UnityEngine;
using UnityEngine.InputSystem;

public class Test_05_SoundManager : TestBase
{
#if UNITY_EDITOR

    public BGMType bgmType;
    public SFXType sfxType;

    public float duration;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        SoundManager.Instance.PlayBGM(bgmType);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        SoundManager.Instance.PlaySound(sfxType);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        SoundManager.Instance.FadeInBGM(duration);
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        SoundManager.Instance.FadeOutBGM(duration);
    }
#endif
}