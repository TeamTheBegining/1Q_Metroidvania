using UnityEngine;
using UnityEngine.InputSystem;

public class Test_00_Cutscene : TestBase
{
    public int sequenceIndex = 0;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        CutSceneManager.Instance.ShowCutScene(sequenceIndex);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        CutSceneManager.Instance.OnNextClicked(sequenceIndex);
    }
}
