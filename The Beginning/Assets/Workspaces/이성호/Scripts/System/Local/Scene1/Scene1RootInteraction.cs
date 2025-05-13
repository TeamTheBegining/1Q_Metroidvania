using System.Collections;
using UnityEngine;

public class Scene1RootInteraction : MonoBehaviour, Interactable
{
    [Tooltip("트리거 되었을 때 바뀔 반지름 값")]
    public float targetRadius;

    [Tooltip("트리거 되었을 때 바뀔 빛 강도")]
    public float targetIntensity;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    public void OnInteraction()
    {
        StartCoroutine(SpreadSpot());
    }

    private IEnumerator SpreadSpot()
    {
        float timeElapsed = 0f;

        while (timeElapsed < targetDuration)
        {
            Debug.Log($"{timeElapsed} triggered");
            timeElapsed += Time.deltaTime;
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(targetRadius * (timeElapsed / targetDuration));
            yield return null;
        }

        LightManager.Instance.PlayerSpotlihgt.SetSpotlight(0f, 0f);
        LightManager.Instance.SetGlobalLight(Color.white);
        Destroy(this.gameObject);
    }
}
