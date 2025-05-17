using System.Collections;
using UnityEngine;

public class Scene1Manager : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPosition;
    private bool isFirstGameStart = false;

    float duration = 3f;
    float targetRadius = 20f;

    private void Start()
    {
        GameObject obj = Instantiate(playerPrefab, spawnPosition.position, Quaternion.identity);
        CameraManager.Instance.SetTarget(CameraType.Scene1Camera, FindFirstObjectByType<Player>().gameObject.transform);
        LightManager.Instance.SetPlayerShadowTarget(obj);
        LightManager.Instance.SetPlayerLightValue(200f);
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
