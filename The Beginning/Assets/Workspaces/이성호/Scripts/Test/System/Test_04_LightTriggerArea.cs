﻿using UnityEditor;
using UnityEngine;

public class Test_04_LightTriggerArea : TestBase
{
#if UNITY_EDITOR
    public PlayerLightTriggerArea trigger;

    private void Start()
    {
        LightManager.Instance.SetPlayerLightValue(20f);
    }
#endif
}