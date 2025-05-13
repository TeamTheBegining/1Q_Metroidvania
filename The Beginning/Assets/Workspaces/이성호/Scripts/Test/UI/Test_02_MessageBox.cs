using UnityEngine;
using UnityEngine.InputSystem;

public class Test_02_MessageBox : TestBase
{
#if UNITY_EDITOR
    public InteractiveMessagePanel panel;
    public TextDataSO data;

    [TextArea]
    public string addData;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        panel.Show();
        panel.SetText(data.text);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        panel.AddText(addData);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        panel.Close();
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        panel.FadeInShow();
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        panel.FadeOutClose();
    }
#endif
}
