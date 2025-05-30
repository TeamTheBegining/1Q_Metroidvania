﻿using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 트리거 내에 들어왔을 떄 textData를 월드에 보여주는 클래스
/// </summary>
public class TextTriggerArea : MonoBehaviour
{
    private TextMeshPro textMeshPro;
    public TextDataSO textData;

    private IEnumerator fadeIn;
    private IEnumerator fadeOut;

    public float fadeOutDelayTime = 4f;

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
            textMeshPro.color = new Color(1, 1, 1, timeElapsed / duration);
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
            textMeshPro.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            yield return null;
        }
    }
}
