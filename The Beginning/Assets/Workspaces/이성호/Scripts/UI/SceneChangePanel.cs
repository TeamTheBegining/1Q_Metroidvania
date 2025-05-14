using System.Collections;
using TMPro;
using UnityEngine;

public class SceneChangePanel : MonoBehaviour
{
    CanvasGroup cg;

    void Start()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    public void ForceShowPanel()
    {
        cg.alpha = 1.0f;
    }

    public void ShowPanel(float duration = 2f)
    {
        StartCoroutine(FadeInProcess(duration));        
    }

    public void ClosePanel(float duration = 2f)
    {
        StartCoroutine(FadeOutProcess(duration));
    }

    private IEnumerator FadeInProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(timeElapsed / duration);
            yield return null;
        }

        cg.alpha = 1.0f;
    }

    private IEnumerator FadeOutProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(1 - timeElapsed / duration);
            yield return null;
        }

        cg.alpha = 0.0f;
    }
}
