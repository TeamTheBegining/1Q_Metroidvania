using System.Collections;
using UnityEngine;

public class Scene4ShopNpc : MonoBehaviour, Interactable
{
    private TextScrollWorld textObj;
    public TextDataSO[] textDatas;
    public GameObject[] itemObjects;

    private float timer;
    private float maxTimer = 10f;
    private int textIndex = 0;

    private void Awake()
    {
        textObj = transform.GetChild(0).GetComponent<TextScrollWorld>();
    }

    private void Start()
    {
        textObj.PlayScroll();        
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > maxTimer)
        {
            timer = 0f;        
            textObj.ResetText();

            textObj.data = textDatas[textIndex];
            textObj.maxIndex = textDatas[textIndex].text.Length;
            textObj.currIndex = 0;
            textObj.PlayScroll();

            textIndex = textIndex >= textDatas.Length ? 0 : textIndex++;
        }
    }

    public void OnInteraction()
    {
        StopAllCoroutines();
        GameManager.Instance.BottomMessagePanel.Close();
        GameManager.Instance.BottomMessagePanel.Show();
        GameManager.Instance.BottomMessagePanel.SetText("모든 스킬을 반환합니다.");
        StartCoroutine(MessageOutProcess());

        for(int i = 0; i < PlayerManager.Instance.IsSkillUnlock.Length; i++)
        {
            if (PlayerManager.Instance.IsSkillUnlock[i])
            {
                PlayerManager.Instance.AddCoin(1000);
            }

            itemObjects[i].SetActive(true);
            itemObjects[i].GetComponent<Scene3Item1>().isTriggered = false;

            // 텍스트 초기화 후 판매 텍스트 출력
            timer = 0f;
            textObj.ResetText();

            textObj.data.text = $"스킬 회수 했습니다.";
            textObj.maxIndex = textObj.data.text.Length;
            textObj.currIndex = 0;
            textObj.PlayScroll();
        }

        PlayerManager.Instance.ResetSkillUnlock();
    }

    private IEnumerator MessageOutProcess()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.BottomMessagePanel.FadeOutClose(2f);
    }
}
