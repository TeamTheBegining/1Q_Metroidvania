using UnityEngine;
using UnityEngine.Rendering.Universal;

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

    public void SetOuterRadius(float outerRadius)
    {
        spotLight.pointLightOuterRadius = outerRadius;
    }
}