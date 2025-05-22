using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 메세지 패널 관리 클래스
/// </summary>
public class MessagePanel : MonoBehaviour
{
    private CanvasGroup cg;
    private TextMeshProUGUI messageText;
    private TextMeshProUGUI glowMessageText;
    private TextMeshProUGUI koreanText;
    private Material glowMaterial; // -1 에서 1 사이값 조절하기 mat.SetFloat("_Dissolve_Thredshold", value);

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        messageText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        glowMessageText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        if(transform.childCount > 3)koreanText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        glowMaterial = glowMessageText.materialForRendering;

        glowMaterial.SetFloat("_Dissolve_Thredshold", -1f);
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

        messageText.gameObject.SetActive(false);
        glowMessageText.gameObject.SetActive(false);
    }
    public void Close()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    #region Normal Text
    public void FadeInShow(float duration = 0.5f)
    {
        StartCoroutine(FadeInProcess(duration));
    }

    public void SetText(string str)
    {
        messageText.gameObject.SetActive(true);
        glowMessageText.gameObject.SetActive(false);
        messageText.text = $"{str}";
    }

    public void AddText(string str)
    {
        messageText.gameObject.SetActive(true);
        glowMessageText.gameObject.SetActive(false);
        messageText.text += $"{str}";
    }

    public void FadeOutClose(float duration = 0.5f)
    {
        StartCoroutine(FadeOutProcess(duration));
    }

    private IEnumerator FadeInProcess(float duration)
    {
        float elpasedTime = 0f;
        while(elpasedTime < duration)
        {
            elpasedTime += Time.deltaTime;
            cg.alpha = elpasedTime / duration;
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess(float duration)
    {
        float elpasedTime = 0f;
        while (elpasedTime < duration)
        {
            elpasedTime += Time.deltaTime;
            cg.alpha = 1 - elpasedTime / duration;
            yield return null;
        }
    }
    #endregion

    #region GlowText
    public void SetGlowText(string str)
    {
        messageText.gameObject.SetActive(false);
        glowMessageText.gameObject.SetActive(true);
        glowMessageText.text = $"{str}";
    }
    public void AddGlowText(string str)
    {
        messageText.gameObject.SetActive(false);
        glowMessageText.gameObject.SetActive(true);
        glowMessageText.text += $"{str}";
    }

    public void GlowFadeOutClose(float duration)
    {
        StartCoroutine(GlowTextFadeOutProcess(duration));
    }

    public void GlowFadeInOpen(float duration)
    {
        StartCoroutine(GlowTextFadeInProcess(duration));
    }

    private IEnumerator GlowTextFadeInProcess(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float mapped = (2.0f * elapsedTime / duration) - 1.0f;
            //koreanText.faceColor = new Color(1f, 1f, 1f, mapped);
            glowMaterial.SetFloat("_Dissolve_Thredshold", mapped);
            yield return null;
        }
    }

    private IEnumerator GlowTextFadeOutProcess(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float mapped = 1.0f - 2.0f * (elapsedTime / duration);
            //koreanText.faceColor = new Color(1f, 1f, 1f, mapped);
            glowMaterial.SetFloat("_Dissolve_Thredshold", mapped);
            yield return null;
        }
    }

    #endregion
}