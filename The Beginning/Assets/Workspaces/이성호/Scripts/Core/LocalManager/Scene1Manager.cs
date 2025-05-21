using System.Collections;
using UnityEngine;

public class Scene1Manager : LocalSceneManager
{
    public GameObject playerObj;
    public Transform spawnPosition;
    private bool isFirstGameStart = false;

    float duration = 3f;
    float targetRadius = 20f;

    private void OnEnable()
    {
/*        if(!isFirstGameStart)
        {
            playerObj = PlayerManager.Instance.SpawnPlayer(spawnPosition.position).gameObject;
            PlayerManager.Instance.AddCoin(1000);
        }*/
    }

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerManager.Instance != null);

        if (!isFirstGameStart)
        {
            playerObj = PlayerManager.Instance.SpawnPlayer(spawnPosition.position).gameObject;
            PlayerManager.Instance.AddCoin(1000);
        }

        // 생성 완료 후 적용
        CameraManager.Instance.SetTarget(CameraType.Scene1Camera, playerObj.transform);
        LightManager.Instance.SetPlayerShadowTarget(playerObj);
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
