using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class Test_04_TmpCustomShader : TestBase
{
#if UNITY_EDITOR

    public MessagePanel panel;
    public TextMeshProUGUI text;
    private Material mat;

    [Range(-1f, 1f)]
    public float value = 0f;

    public TextDataSO data;

    [TextArea]
    public string addData;

    public float duration = 2f;

    private void Start()
    {
        mat = text.materialForRendering;
    }

    private void Update()
    {
        int a = 0;
        if (mat != null)
        {
            mat.SetFloat("_Dissolve_Thredshold", value);
        }
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        panel.Show();
        panel.SetGlowText(data.text);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        panel.AddGlowText(addData);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        panel.Close();
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        panel.GlowFadeOutClose(duration);
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        panel.GlowFadeInOpen(duration);
    }
#endif
}