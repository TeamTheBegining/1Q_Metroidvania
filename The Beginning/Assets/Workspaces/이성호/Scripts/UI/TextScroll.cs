using System.Collections;
using TMPro;
using UnityEngine;

public class TextScroll : MonoBehaviour
{
    private TextMeshProUGUI text;
    public TextDataSO data;

    public int currIndex;
    public int maxIndex;
    public float delay = 0.5f;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        maxIndex = data.text.Length;
    }

    public void PlayScroll()
    {
        StartCoroutine(ScrollTextProcess());
    }

    private IEnumerator ScrollTextProcess()
    {
        float timeElapsed = 0.0f;

        while (currIndex < maxIndex)
        {
            timeElapsed += Time.deltaTime;

            if(timeElapsed > delay)
            {
                text.text += $"{data.text[currIndex]}";
                timeElapsed = 0f;
                currIndex++;
            }

            yield return null;
        }
    }
}