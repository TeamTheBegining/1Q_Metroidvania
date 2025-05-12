using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightManager : Singleton<LightManager>
{
    private Light2D globalLight;

    protected override void Awake()
    {
        Init();    
    }

    private void Init()
    {
        globalLight = GetComponentInChildren<Light2D>();

        if(globalLight == null) // 빛 컴포넌트 추가
        {
            globalLight = gameObject.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
        }
    }

    public void SetGlobalLight(Color color, float intensity = 1f)
    {
        globalLight.color = color;
        globalLight.intensity = intensity;
    }
}
