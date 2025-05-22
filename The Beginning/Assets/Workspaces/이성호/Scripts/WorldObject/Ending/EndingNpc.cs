using UnityEngine;

public class EndingNpc : MonoBehaviour
{
    public TextDataSO[] datas;
    public TextScrollWorld textScroll;

    private void Awake()
    {
        textScroll = transform.GetChild(0).GetComponent<TextScrollWorld>();
    }

    public void ShowText(int index)
    {
        textScroll.ResetText();
        textScroll.currIndex = 0;
        textScroll.maxIndex = datas[index].text.Length;
        textScroll.data = datas[index];
        textScroll.PlayScroll();
    }
}
