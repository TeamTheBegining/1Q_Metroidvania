using System.Collections;
using TMPro;
using UnityEngine;

public class Scene2Tutorial : MonoBehaviour, Interactable
{
    CheatInputActions actions;

    public TextDataSO[] upper;
    public TextDataSO[] middle;
    public TextDataSO[] bottom;

    private TextMeshPro textUpper;
    private TextMeshPro textMiddle;
    private TextMeshPro textBottom;

    public PlayerInput input;

    private int currentCount = 0;
    private int maxCount = 0;
    private float duration = 2f;
    private bool isTriggered = false; // 상호작용 여부

    private void Awake()
    {
        textUpper = transform.GetChild(0).GetComponent<TextMeshPro>();
        textMiddle = transform.GetChild(1).GetComponent<TextMeshPro>();
        textBottom = transform.GetChild(2).GetComponent<TextMeshPro>();
        textUpper.color = new Color(1f, 1f, 1f, 0f);
        textMiddle.color = new Color(1f, 1f, 1f, 0f);
        textBottom.color = new Color(1f, 1f, 1f, 0f);

        maxCount = upper.Length;
    }

    public void Play()
    {
        StartCoroutine(Tutorial());
    }

    private IEnumerator Tutorial()
    {
        currentCount = 0;
        StartCoroutine(FadeInProcess(currentCount));
        while(currentCount < maxCount - 1)
        {
            if(CheckInput())
            {
                StartCoroutine(FadeOutProcess(currentCount));
                currentCount++;

                // 최대값 확인
                if (currentCount == maxCount - 1)
                {
                    break;
                }

                yield return new WaitForSeconds(duration);

                StartCoroutine(FadeInProcess(currentCount));
                yield return new WaitForSeconds(duration);
            }

            yield return null;
        }
    }

    private IEnumerator FadeInProcess(int index)
    {
        float timeElapsed = 0.0f;

        textUpper.text = upper[index].text;
        textMiddle.text = middle[index].text;
        textBottom.text = bottom[index].text;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textUpper.color = new Color(1, 1, 1, timeElapsed / duration);
            textMiddle.color = new Color(1, 1, 1, timeElapsed / duration);
            textBottom.color = new Color(1, 1, 1, timeElapsed / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOutProcess(int index)
    {
        float timeElapsed = 0.0f;

        textUpper.text = upper[index].text;
        textMiddle.text = middle[index].text;
        textBottom.text = bottom[index].text;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textUpper.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            textMiddle.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            textBottom.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            yield return null;
        }
    }

    private bool CheckInput()
    {
        if (currentCount == 0)
        {
            return input.IsAttack;
        }
        else if (currentCount >= 1 && currentCount <= 5)
        {
            return input.IsParrying;
        }
        else if (currentCount == 6)
        {
            return input.IsSkill1;
        }
        else
        {
            return true;
        }
    }

    public void OnInteraction()
    {
        if (isTriggered) return;

        input = FindFirstObjectByType<PlayerInput>();
        StartCoroutine(Tutorial());
        isTriggered = true;
    }
}
