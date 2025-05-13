using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_Cutscene : TestBase
{
#if UNITY_EDITOR
    public int sequenceIndex = 0;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        CutSceneManager.Instance.ShowCutscene(sequenceIndex);
    }
#endif
}
