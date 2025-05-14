using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 메세지 패널 관리 클래스
/// </summary>
public class InteractiveMessagePanel : MonoBehaviour
{
    private CanvasGroup cg;
    private TextMeshProUGUI messageText;

    private float duration = 0.5f;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        messageText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Close();
    }

    public void Show()
    {
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    public void FadeInShow()
    {
        StartCoroutine(FadeInProcess());
    }

    public void SetText(string str)
    {
        messageText.text = $"{str}";
    }

    public void AddText(string str)
    {
        messageText.text += $"{str}";
    }

    public void Close()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    public void FadeOutClose()
    {
        StartCoroutine(FadeOutProcess());
    }

    private IEnumerator FadeInProcess()
    {
        float elpasedTime = 0f;
        while(elpasedTime < duration)
        {
            elpasedTime += Time.deltaTime;
            cg.alpha = elpasedTime / duration;
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess()
    {
        float elpasedTime = 0f;
        while (elpasedTime < duration)
        {
            elpasedTime += Time.deltaTime;
            cg.alpha = 1 - elpasedTime / duration;
            yield return null;
        }
    }
}