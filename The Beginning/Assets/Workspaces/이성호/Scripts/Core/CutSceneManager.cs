using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 캔버스를 가져서 해당 캔버스에 Cutscene sequence의 이미지들을 실행하는 매니저
/// </summary>
public class CutSceneManager : Singleton<CutSceneManager>
{
    public CutsceneSequenceSO[] sequences;
    private Canvas cutsceneCanvas;
    private GameObject currentCutsceneObject;

    private int currentSceneIndex = 0;

    public bool isPlay { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        cutsceneCanvas = transform.GetChild(0).GetComponent<Canvas>();

        sequences = new CutsceneSequenceSO[(int)CutSceneType.CutSceneTypeCount];

        sequences[0] = Resources.Load<CutsceneSequenceSO>("Data/CutScene/CutsceneSequence_Intro");
        sequences[1] = Resources.Load<CutsceneSequenceSO>("Data/CutScene/CutsceneSequence_Credit");
        sequences[2] = Resources.Load<CutsceneSequenceSO>("Data/CutScene/CutsceneSequence_PlayerUlt");
    }

    public void ShowCutscene(CutSceneType type)
    {
        ShowCutscene((int)type);
    }

    public void ShowCutscene(int sequenceIndex)
    {
        isPlay = true;
        gameObject.SetActive(true);
            currentSceneIndex = 0;
        StartCoroutine(ProcessShowCutscene(sequenceIndex, sequences[sequenceIndex].lines[currentSceneIndex].showTime));
    }

    public float GetSequenceTime(int sequenceIndex)
    {
        float result = 0.0f;
        foreach(var data in sequences[sequenceIndex].lines)
        {
            result += data.showTime;
        }

        return result;
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
        else // 종료
        {
            Destroy(currentCutsceneObject);
            isPlay = false;
        }
    }

    GameObject ShowLine(int sequenceIndex, int index)
    {
        var line = sequences[sequenceIndex].lines[index];
        var obj = Instantiate(line.CutsceneObject, cutsceneCanvas.gameObject.transform);

        return obj;
    }
}