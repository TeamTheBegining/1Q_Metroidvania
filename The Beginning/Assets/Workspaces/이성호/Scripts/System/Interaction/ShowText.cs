using System.Collections;
using TMPro;
using UnityEngine;

public class ShowText : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    public TextDataSO textData;

    private IEnumerator fadeIn;
    private IEnumerator fadeOut;

    private float fadeOutDelayTime = 4f;

    private void Awake()
    {
        textMeshPro = GetComponentInChildren<TextMeshPro>();
        textMeshPro.text = textData.text;
    }

    private void Start()
    {
        fadeIn = FadeInProcess();
        fadeOut = FadeOutProcess();

        textMeshPro.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            textMeshPro.gameObject.SetActive(true);
            StartCoroutine(fadeIn);
        }
    }

    private IEnumerator FadeInProcess()
    {
        float timeElapsed = 0.0f;
        float duration = 2f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textMeshPro.color = new Color(1, 1, 1, duration * timeElapsed / duration);
            yield return null;
        }

        StartCoroutine(fadeOut);
    }

    private IEnumerator FadeOutProcess()
    {
        yield return new WaitForSeconds(fadeOutDelayTime);

        float timeElapsed = 0.0f;
        float duration = 1f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textMeshPro.color = new Color(1, 1, 1, 1 - duration * timeElapsed / duration);
            yield return null;
        }
    }
}
