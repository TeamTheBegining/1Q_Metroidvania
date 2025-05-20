using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_Cutscene : TestBase
{
#if UNITY_EDITOR
    public int sequenceIndex = 0;

    public CutSceneType type;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        CutSceneManager.Instance.ShowCutscene(sequenceIndex);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        CutSceneManager.Instance.ShowCutscene(type);
    }
#endif
}
