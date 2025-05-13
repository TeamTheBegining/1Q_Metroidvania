using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// TODO : Cutscene 시퀀스가 play되면 자동 재생될 수 있게 만들기 (각 시퀀스 개별 작동)

public class CutSceneManager : Singleton<CutSceneManager>
{
    public CutsceneSequenceSO[] sequences;
    private Canvas cutsceneCanvas;
    private GameObject currentCutsceneObject;

    private int currentSceneIndex = 0;

    void Start()
    {
        Transform child = transform.GetChild(0);
        cutsceneCanvas = child.GetComponent<Canvas>();
    }

    public void ShowCutscene(int sequenceIndex)
    {
        gameObject.SetActive(true);
        currentSceneIndex = 0;

        StartCoroutine(ProcessShowCutscene(sequenceIndex, sequences[sequenceIndex].lines[currentSceneIndex].showTime));
    }

    // 컷 씬 자동 넘기기를 위한 코루틴
    private IEnumerator ProcessShowCutscene(int sequenceIndex, float maxTime)
    {
        // 기존 컷 씬 제거
        if (currentCutsceneObject != null)
        {
            Destroy(currentCutsceneObject);
        }

        // 컷 씬 보여주기
        float elapsedTimer = 0.0f;

        currentCutsceneObject = ShowLine(sequenceIndex, currentSceneIndex);
        while(elapsedTimer < maxTime)
        {
            elapsedTimer += Time.deltaTime;
            yield return null;
        }

        // 다음 컷 씬 실행
        currentSceneIndex++;
        if (currentSceneIndex < sequences[sequenceIndex].lines.Length)
        {
            StartCoroutine(ProcessShowCutscene(sequenceIndex, sequences[sequenceIndex].lines[currentSceneIndex].showTime));
        }
        else
        {
            Destroy(currentCutsceneObject);
        }
    }

    GameObject ShowLine(int sequenceIndex, int index)
    {
        var line = sequences[sequenceIndex].lines[index];
        var obj = Instantiate(line.CutsceneObject, cutsceneCanvas.gameObject.transform);

        return obj;
    }

    void EndCutscene()
    {
        // 컷씬 종료 처리
        gameObject.SetActive(false); // 임시
    }
}