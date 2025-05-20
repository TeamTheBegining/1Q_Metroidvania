using System.Collections;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SkillCutsceneController : MonoBehaviour
{
    SkillCutsceneLightController backlight; // 뒷 배경 머터리얼 래퍼런스를 설정하기 위한 스크립트
    DissolveUIController[] dissolveControllers;

    private void Awake()
    {
        backlight = transform.GetChild(0).GetComponent<SkillCutsceneLightController>();
        dissolveControllers = GetComponentsInChildren<DissolveUIController>();
    }

    public void SetDissolveThredshold(float value)
    {
        for(int i = 0; i < dissolveControllers.Length; i++)
        {
            dissolveControllers[i].SetThredhold(value);
        }
    }

    public void SetDissolveThredsholdToZero(float duration)
    {
        StartCoroutine(DcreaseDissolveProcess(duration));
    }

    public IEnumerator DcreaseDissolveProcess(float duration)
    {
        float timeElapsed = 0f;
        float currentValue = backlight.GetCurrentRange();
        float targetValue = -1f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float dissolveValue = Mathf.Lerp(currentValue, targetValue, timeElapsed / duration);
            for (int i = 0; i < dissolveControllers.Length; i++)
            {
                dissolveControllers[i].SetThredhold(dissolveValue);
            }
            yield return null;
        }

        backlight.SetRange(targetValue);
    }

    public void SetLightRange(float value)
    {
        backlight.SetRange(value);        
    }

    public void DcreaseLightRangeToZero(float duration)
    {
        StartCoroutine(DcreaseProcess(duration));
    }

    private IEnumerator DcreaseProcess(float duration)
    {
        float timeElapsed = 0f;
        float currentValue = backlight.GetCurrentRange();
        float targetValue = 0f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float lightValue = Mathf.Lerp(currentValue, targetValue, timeElapsed / duration);
            backlight.SetRange(lightValue);
            Debug.Log($"{lightValue}");
            yield return null;
        }

        backlight.SetRange(targetValue);
    }
}
