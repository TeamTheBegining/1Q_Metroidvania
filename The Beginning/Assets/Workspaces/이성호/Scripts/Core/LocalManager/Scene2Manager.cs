using System.Collections;
using UnityEngine;

public class Scene2Manager : MonoBehaviour
{
    private CanvasGroup cg;
    private TextScroll textScroll;

    private void Awake()
    {
        cg = transform.GetChild(0).GetComponent<CanvasGroup>();
        textScroll = transform.GetChild(0).GetChild(0).GetComponent<TextScroll>();
    }

    public void PlayText()
    {
        textScroll.PlayScroll();
    }

    public void DisableText()
    {
        StartCoroutine(CloseProcess(0.5f));
    }

    private IEnumerator CloseProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = 1 - timeElapsed / duration;
            yield return null;
        }
    }
}
