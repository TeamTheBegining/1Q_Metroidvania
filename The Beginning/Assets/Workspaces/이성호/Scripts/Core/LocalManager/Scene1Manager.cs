using System.Collections;
using UnityEngine;

public class Scene1Manager : MonoBehaviour
{
    private bool isFirstGameStart = false;

    float duration = 3f;
    float targetRadius = 20f;

    private void Start()
    {
        LightManager.Instance.SetPlayerLightValue(200f);
        CameraManager.Instance.SetTarget(CameraType.Scene1Camera, FindFirstObjectByType<Player>().gameObject.transform);
    }

    private void Update()
    {
        if(!isFirstGameStart && GameManager.Instance.State == GameState.Play)
        {
            isFirstGameStart = true;
            LightManager.Instance.SpreadPlayerLight(duration, targetRadius, 1f);
        }
    }
}
