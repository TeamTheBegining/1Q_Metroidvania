using System.Collections;
using UnityEngine;

/// <summary>
/// 트리거 내에 플레이어가 들어왔을 때 target값으로 빛 프로퍼티 값을 바꾸는 클래스
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerLightTriggerArea : MonoBehaviour
{
    [Tooltip("트리거 되었을 때 바뀔 반지름 값")]
    public float targetRadius;

    [Tooltip("트리거 되었을 때 바뀔 빛 강도")]
    public float targetIntensity;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    private bool isTriggered = false;

    [Tooltip("트리거 후 해당 스폿라이트를 끌껀지 설정 (true : spotLight 끄기, false : spotlight 유지")]
    public bool disableSpotLightAfterTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isTriggered && collision.CompareTag("Player")) // 플레이어가 오면 발동
        {
            StartCoroutine(SpreadSpot());
            isTriggered = true;
        }
    }
    private IEnumerator SpreadSpot()
    {
        float timeElapsed = 0f;

        while (timeElapsed < targetDuration)
        {
            //Debug.Log($"{timeElapsed} triggered");
            timeElapsed += Time.deltaTime;
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(targetRadius * (timeElapsed / targetDuration));
            yield return null;
        }

        if(disableSpotLightAfterTrigger)
        {
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(0f, 0f);
            LightManager.Instance.SetGlobalLight(Color.white);
        }
    }
}