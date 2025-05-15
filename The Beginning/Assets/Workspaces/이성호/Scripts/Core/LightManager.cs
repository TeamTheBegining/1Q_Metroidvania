using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 light 머터리얼 관리 매니저
/// </summary>
public class LightManager : Singleton<LightManager>
{
    private PlayerShaderLight playerLight;

    protected override void Awake()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        PlayerShaderLight comp = FindFirstObjectByType<PlayerShaderLight>();
        if (comp != null)
        {
            playerLight = comp;
        }
    }

    /// <summary>
    /// Player의 FakeLight의 DarknessStrength 값 변경 ( 0 -> 밝음, 값이 클 수록 주변 어두워짐 )
    /// </summary>
    /// <param name="value"></param>
    public void SetPlayerLightValue(float value)
    {
        playerLight.SetRange(value);
    }

    /// <summary>
    /// Player의 FakeLight를 duration 동안 value값으로 speed * Time.deltaTime 속도 만큼 값을 변화 시키는 함수
    /// </summary>
    /// <param name="duration">지속시간</param>
    /// <param name="targetValue">목표 값</param>
    /// <param name="speed">속도</param>
    public void SpreadPlayerLight(float duration, float targetValue, float speed)
    {
        StartCoroutine(SpreadSpot(duration, targetValue, speed));
    }

    private IEnumerator SpreadSpot(float duration, float targetValue, float speed)
    {
        float startValue = playerLight.GetCurrentRange();
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;               // 진행률 (0~1)
            float currentValue = Mathf.Lerp(startValue, targetValue, t);  // 부드럽게 선형 보간
            playerLight.SetRange(currentValue);

            timeElapsed += Time.deltaTime * speed;
            yield return null;
        }
    }
}
