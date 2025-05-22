using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Scene1Manager : LocalSceneManager
{
    public GameObject playerObj;
    public Transform spawnPosition;

    private bool isFirstGameStart = false;

    float duration = 3f;
    float targetRadius = 20f;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerManager.Instance != null);

        if (!isFirstGameStart)
        {
            playerObj = PlayerManager.Instance.SpawnPlayer(spawnPosition.position).gameObject;
            PlayerInput playerInput = playerObj.GetComponent<PlayerInput>();
            PlayerManager.Instance.AddCoin(1000);
            playerInput.AllDisable();
        }

        // 생성 완료 후 적용
        SoundManager.Instance.PlayBGM(BGMType.bgm_Ambience_01);
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

            PlayerInput playerInput = playerObj.GetComponent<PlayerInput>();
            playerInput.AllEnable();
        }
    }
}
