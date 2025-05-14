using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 플레이어를 비추는 2D Light 컴포넌트를 가지고 있는 오브젝트의 클래스
/// </summary>
public class PlayerSpotLight : MonoBehaviour
{
    GameObject playerObject;
    Light2D spotLight;

    public Vector2 offset = Vector2.zero;

    private void Awake()
    {
        spotLight = transform.GetComponentInChildren<Light2D>();
        spotLight.lightType = Light2D.LightType.Point;
    }

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
    }

    private void LateUpdate()
    {
        if(playerObject != null)
        {
            transform.position = playerObject.transform.position + (Vector3)offset;
        }
    }

    public void SetSpotlight(float outerRadius, float intensity = 1f)
    {
        spotLight.pointLightOuterRadius = outerRadius;
        spotLight.intensity = intensity;
    }
}