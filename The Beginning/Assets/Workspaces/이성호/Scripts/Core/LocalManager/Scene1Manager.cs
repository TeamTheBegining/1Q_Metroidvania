using System.Collections;
using UnityEngine;

public class Scene1Manager : MonoBehaviour
{
    private bool isFirstGameStart = false;

    private void Start()
    {
        LightManager.Instance.SetGlobalLight(Color.black);          // 처음 빛 색 - 검정
        LightManager.Instance.PlayerSpotlihgt.SetSpotlight(4f, 0f); // 플레이어 스폿 라이트 초기화
    }

    private void Update()
    {
        if(!isFirstGameStart && GameManager.Instance.State == GameState.Play)
        {
            isFirstGameStart = true;
            StartCoroutine(FirstSpreadSpot());
        }
    }

    /// <summary>
    /// 게임 시작 시 플레이어 라이트 설정 코루틴 ( 플레이어 스폿 라이트 크게 만들기 )
    /// </summary>
    private IEnumerator FirstSpreadSpot()
    {
        float timeElapsed = 0f;
        float duration = 3f;
        float targetRadius = 2f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(targetRadius * (timeElapsed / duration), 1f);
            yield return null;
        }
    }
}
