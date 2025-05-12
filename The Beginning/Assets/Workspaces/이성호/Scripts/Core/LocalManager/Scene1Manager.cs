using System.Collections;
using UnityEngine;

public class Scene1Manager : MonoBehaviour
{
    bool isSceneStart = false;

    private void Start()
    {
        LightManager.Instance.SetGlobalLight(Color.black);
        LightManager.Instance.PlayerSpotlihgt.SetSpotlight(4f, 0f);

        StartCoroutine(GameStateChange());
    }

    private void Update()
    {
    }

    private IEnumerator FirstSpreadSpot()
    {
        float timeElapsed = 0f;
        float duration = 3f;
        float targetRadius = 4f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(targetRadius * (timeElapsed / duration), 1f);
            yield return null;
        }
    }

    // 임시 : 나중에 컷 씬 후 전환하게 바꿀 것
    private IEnumerator GameStateChange()
    {
        float timeElapsed = 0f;

        while (timeElapsed < 4f)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
            GameManager.Instance.State = GameState.Play;
        }

        StartCoroutine(FirstSpreadSpot());
    }
}
