using NUnit.Framework.Interfaces;
using System.Collections;
using TMPro;
using UnityEngine;

public class WorldTextInteractionGuid : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    public TextDataSO data;
    public float duration;

    private void Awake()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = data.text;
    }

    public void ShowText()
    {
        textMeshPro.color = new Color(1, 1, 1, 1);
    }

    public void HideText()
    {
        textMeshPro.color = new Color(1, 1, 1, 0);
    }

    public void FadeInText()
    {
        StartCoroutine(FadeInProcess());
    }

    public void FadeOutText()
    {
        StartCoroutine(FadeOutProcess());
    }

    private IEnumerator FadeInProcess()
    {
        float timeElapsed = 0.0f;
        float duration = 2f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textMeshPro.color = new Color(1, 1, 1, timeElapsed / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess()
    {
        float timeElapsed = 0.0f;
        float duration = 1f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textMeshPro.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            yield return null;
        }
    }
}
