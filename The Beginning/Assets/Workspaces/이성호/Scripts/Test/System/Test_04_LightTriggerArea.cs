using UnityEditor;
using UnityEngine;

public class Test_04_LightTriggerArea : TestBase
{
    public PlayerLightTriggerArea trigger;

    private void Start()
    {
        LightManager.Instance.SetGlobalLight(Color.black);
    }
}