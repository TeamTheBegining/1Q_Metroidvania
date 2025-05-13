using System.Collections;
using UnityEngine;

public class Scene1Manager : MonoBehaviour
{
    private bool isFirstGameStart = false;

    private void Start()
    {
        LightManager.Instance.SetGlobalLight(Color.black);
        LightManager.Instance.PlayerSpotlihgt.SetSpotlight(4f, 0f);
    }

    private void Update()
    {
        if(!isFirstGameStart && GameManager.Instance.State == GameState.Play)
        {
            isFirstGameStart = true;
            StartCoroutine(FirstSpreadSpot());
        }
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
}
