using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public enum PopUpShowtype
{
    Up = 0,
    Down,
    Left,
    Right
}


public class PopUpPanel : MonoBehaviour
{
    RectTransform rect;
    CanvasGroup cg;
    TextMeshProUGUI titleText;
    TextMeshProUGUI descriptionText;

    public AnimationCurve curve;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        titleText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        descriptionText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        Close();
    }

    public void ShowPopUp(Vector2 viewportVector, string title, string description, float duration, PopUpShowtype type = PopUpShowtype.Up)
    {
        titleText.text = title;
        descriptionText.text = description;

        Vector3 screenVector = Camera.main.ViewportToScreenPoint(viewportVector);       

        StartCoroutine(ShowProcess(duration));
    }

    public void Show()
    {
        cg.alpha = 1;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    public void Close()
    {
        cg.alpha = 0;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }


    private IEnumerator ShowProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = timeElapsed / duration;
            curve.Evaluate(timeElapsed / duration);

            rect.position += new Vector3(0f, curve.Evaluate(timeElapsed / duration), 0f);

            yield return null;
        }

        StartCoroutine(CloseProcess(duration));
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