using UnityEngine;
using UnityEngine.InputSystem;

public class Test_05_SoundManager : TestBase
{
#if UNITY_EDITOR

    public BGMType bgmType;
    public SFXType sfxType;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        SoundManager.Instance.PlayBGM(bgmType);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        SoundManager.Instance.PlaySound(sfxType);
    }
#endif
}