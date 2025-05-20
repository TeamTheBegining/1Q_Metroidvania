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

    public GameObject TutorialEnemyPrefab;

    public PlayerInput input;

    private int currentFlowCount = 0;
    private int maxFlowCount = 0;

    private float duration = 1f;
    private bool isTriggered = false; // 상호작용 여부

    private void Awake()
    {
        textUpper = transform.GetChild(0).GetComponent<TextMeshPro>();
        textMiddle = transform.GetChild(1).GetComponent<TextMeshPro>();
        textBottom = transform.GetChild(2).GetComponent<TextMeshPro>();
        textUpper.color = new Color(1f, 1f, 1f, 0f);
        textMiddle.color = new Color(1f, 1f, 1f, 0f);
        textBottom.color = new Color(1f, 1f, 1f, 0f);

        maxFlowCount = upper.Length;
    }

    public void Play()
    {
        StartCoroutine(Tutorial());
    }

    private IEnumerator Tutorial()
    {
        Player player = FindFirstObjectByType<Player>(); // 마나 충전을 위한 플레이어 찾기

        currentFlowCount = 0;
        
        // 첫 인풋 막기
        input.AllDisable();
        input.OneEnable(0); // 1 공격 2 패링 3 스킬

        StartCoroutine(FadeInProcess(currentFlowCount));
        while(currentFlowCount < maxFlowCount - 1)
        {
            if(CheckInput())
            {
                // 각 구간별 특정 인풋활성화
                if(0 <= currentFlowCount && currentFlowCount <= 5 - 1) // 패링
                {
                    if(currentFlowCount == 0)
                    {
                        yield return new WaitForSeconds(1f);
                    }

                    input.AllDisable();
                    input.OneEnable(1);
                }
                else if(currentFlowCount == 6 - 1) // 스킬1
                {
                    input.AllDisable();
                    input.OneEnable(2);

                    player.CurrentMp = player.MaxMp;
                }

                currentFlowCount++;  
                StartCoroutine(FadeOutProcess(currentFlowCount));
                yield return new WaitForSeconds(duration);

                StartCoroutine(FadeInProcess(currentFlowCount));
                yield return new WaitForSeconds(duration);

                // 최대값 확인
                if (currentFlowCount == maxFlowCount - 1)
                {
                    StartCoroutine(FadeOutProcess(currentFlowCount));
                    Debug.Log("종료");
                    break;
                }
            }

            yield return null;
        }

        input.AllEnable();
        player.CurrentMp = 0f; // 마나 제거
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

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            textUpper.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            textMiddle.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            textBottom.color = new Color(1, 1, 1, 1 - timeElapsed / duration);
            yield return null;
        }

        textUpper.color = new Color(1, 1, 1, 0f);
        textMiddle.color = new Color(1, 1, 1, 0f);
        textBottom.color = new Color(1, 1, 1, 0f);
    }

    private bool CheckInput()
    {
        if (currentFlowCount == 0)
        {
            return input.IsAttack;
        }
        else if (currentFlowCount >= 1 && currentFlowCount <= 5)
        {
            return input.IsParrying;
        }
        else if (currentFlowCount == 6)
        {
            return input.IsSkill1;
        }
        else
        {
            return true;
        }
    }

    private void SpawnEnemy()
    {
        GameObject obj = Instantiate(TutorialEnemyPrefab);
    }

    public void OnInteraction()
    {
        if (isTriggered) return;

        input = FindFirstObjectByType<PlayerInput>();
        StartCoroutine(Tutorial());
        isTriggered = true;
    }
}
