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
    private IEnumerator processCourutine;

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

        rect.position = screenVector;

        StopAllCoroutines();
        StartCoroutine(ShowProcess(duration, type));
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

    private IEnumerator ShowProcess(float duration, PopUpShowtype type)
    {
        float timeElapsed = 0.0f;

        Vector2 startPos = rect.anchoredPosition;

        // 이동 방향 설정
        Vector2 direction = type switch
        {
            PopUpShowtype.Up => Vector2.up,
            PopUpShowtype.Down => Vector2.down,
            PopUpShowtype.Left => Vector2.left,
            PopUpShowtype.Right => Vector2.right,
            _ => Vector2.up,
        };

        float moveDistance = 100f; // 이동 거리 크기 (원하는 대로 조절 가능)

        Show(); // UI 보이도록 설정

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            float delta = curve.Evaluate(t); // 0 ~ 1 사이의 곡선 값

            // 절대 위치 계산 방식
            rect.anchoredPosition = startPos + direction * delta * moveDistance;

            cg.alpha = t;

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