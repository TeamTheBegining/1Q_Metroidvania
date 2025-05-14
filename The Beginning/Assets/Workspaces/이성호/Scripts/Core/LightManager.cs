using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Global Light와 Player Spot Light 등 빛관련 컴포넌트 관리 매니저
/// </summary>
public class LightManager : Singleton<LightManager>
{
    private Light2D globalLight;
    private PlayerSpotLight playerSpotlight;
    public PlayerSpotLight PlayerSpotlihgt { get => playerSpotlight; }

    protected override void Awake()
    {
        Init();    
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
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

        PlayerSpotLight playerSpotlightComp = FindFirstObjectByType<PlayerSpotLight>();
        if(playerSpotlightComp != null)
        {
            playerSpotlight = playerSpotlightComp;
        }
    }

    public void SetGlobalLight(Color color, float intensity = 1f)
    {
        globalLight.color = color;
        globalLight.intensity = intensity;
    }
}
