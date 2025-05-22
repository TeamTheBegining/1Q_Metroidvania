using System;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Scene2Tutorial : MonoBehaviour
{
    private CinemachineCamera tutorialCam;
    private CinemachineCamera tutorialCamToEnemy;

    public TextDataSO[] upper;
    public TextDataSO[] middle;
    public TextDataSO[] bottom;

    private TextMeshPro textUpper;
    private TextMeshPro textMiddle;
    private TextMeshPro textBottom;

    public GameObject TutorialEnemyPrefab;
    private TutorialEnemy enemy;
    public Transform enemySpawnPoint;

    private PlayerInput input;

    private int currentFlowCount = 0;
    private int maxFlowCount = 0;

    private float duration = 1f;
    private bool isTriggered = false; // 상호작용 여부

    private bool isEnemyMove = false;
    //private bool isEnemyAttacking = false; // 현재 적이 애니메이션 공격을 하고 있는지 확인 변수

    private void Awake()
    {
        textUpper = transform.GetChild(0).GetComponent<TextMeshPro>();
        textMiddle = transform.GetChild(1).GetComponent<TextMeshPro>();
        textBottom = transform.GetChild(2).GetComponent<TextMeshPro>();
        textUpper.color = new Color(1f, 1f, 1f, 0f);
        textMiddle.color = new Color(1f, 1f, 1f, 0f);
        textBottom.color = new Color(1f, 1f, 1f, 0f);

        maxFlowCount = upper.Length;

        tutorialCam = transform.GetChild(4).GetComponent<CinemachineCamera>();
        tutorialCamToEnemy = transform.GetChild(5).GetComponent<CinemachineCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if (isTriggered) return;

            input = FindFirstObjectByType<PlayerInput>();
            tutorialCam.Priority = 100;

            StartCoroutine(Tutorial());
            isTriggered = true;
        }
    }

    private void Update()
    {
        UpdateEnemyMove();
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
                //isEnemyAttacking = false;

                yield return new WaitForFixedUpdate();
                if(enemy && 0 <= currentFlowCount && currentFlowCount <= 5)
                {
                    enemy.PlayAttack();
                    enemy.AttackToTarget();

                    if(currentFlowCount == 5)
                    {
                        enemy.PlayIdle();
                    }
                }

                input.AllDisable();

                if(currentFlowCount == 6)
                {
                    yield return new WaitForSeconds(1.2f);
                    enemy.PlayDead();
                }
                else if(currentFlowCount == 7)
                {
                    tutorialCamToEnemy.Priority = 110;
                    yield return new WaitForSeconds(3f);
                }
                else if(currentFlowCount == 8)
                {
                    tutorialCamToEnemy.Priority = 0;
                    yield return new WaitForSeconds(3f);
                }
                else if(currentFlowCount == 9)
                {
                    tutorialCam.Priority = 0;
                }

                // 적 시나리오
                currentFlowCount++;
                EnemyScearioContorller(currentFlowCount);

                // 글자 애니메이션
                StartCoroutine(FadeOutProcess(currentFlowCount));
                yield return new WaitForSeconds(duration);

                StartCoroutine(FadeInProcess(currentFlowCount));
                yield return new WaitForSeconds(duration);

                // 각 구간별 특정 인풋활성화
                if (0 <= currentFlowCount && currentFlowCount <= 5) // 패링
                {
                    input.OneEnable(1);
                }
                else if (currentFlowCount == 6) // 스킬1
                {
                    input.OneEnable(2);

                    player.CurrentMp = player.MaxMp;
                }

                // 최대값 확인
                if (currentFlowCount == maxFlowCount - 1)
                {
                    StartCoroutine(FadeOutProcess(currentFlowCount));
                    break;
                }
            }

            yield return null;
        }

        input.AllEnable();
        player.CurrentMp = 0f; // 마나 제거
        tutorialCam.Priority = 0; // 카메라 포커스 제거
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

    #region enemyControll
    private void EnemyScearioContorller(int flowIndex)
    {
        if(flowIndex == 2)
        {
            SpawnEnemy();
            SetEnemyMove();
        }
        else if(flowIndex == 3)
        {
            isEnemyMove = true;
        }
    }

    private void UpdateEnemyMove()
    {
        if (input == null || enemy == null) return;

        float distance = Vector3.Distance(input.gameObject.transform.position, enemy.gameObject.transform.position);

        if (distance < 1f && isEnemyMove) // 일정거리까지 오면 정지 후 공격 애니메이션 시작
        {
            enemy.PlayAttackReady();
            enemy.SetMoveActive(false);
            isEnemyMove = false;
        }
    }

    private void SpawnEnemy()
    {
        if (enemy != null) return;

        GameObject obj = Instantiate(TutorialEnemyPrefab, enemySpawnPoint ? enemySpawnPoint.position : Vector3.zero, Quaternion.identity);
        enemy = obj.GetComponent<TutorialEnemy>();
    }

    private void SetEnemyMove()
    {
        if (input == null || enemy == null) return;

        enemy.PlayRun();
        enemy.SetMoveActive(true);
        isEnemyMove = true;
    }
    #endregion
}
