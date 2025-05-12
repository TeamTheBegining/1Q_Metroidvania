using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

// TODO : Cutscene 시퀀스가 play되면 자동 재생될 수 있게 만들기 (각 시퀀스 개별 작동)

public class CutSceneManager : Singleton<CutSceneManager>
{
    public CutsceneSequenceSO[] sequences;

    private int currentIndex = 0;

    public Image sceneImage;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    void Start()
    {
        Transform child = transform.GetChild(0);
        sceneImage = child.transform.GetChild(0).GetComponent<Image>();
        speakerText = child.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        dialogueText = child.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    public void ShowCutScene(int sequenceIndex)
    {
        gameObject.SetActive(true);
        ShowLine(sequenceIndex, currentIndex);
    }

    public void OnNextClicked(int sequenceIndex)
    {
        currentIndex++;
        if (currentIndex < sequences[sequenceIndex].lines.Length)
            ShowLine(sequenceIndex, currentIndex);
        else
            EndCutscene();
    }

    void ShowLine(int sequenceIndex, int index)
    {
        var line = sequences[sequenceIndex].lines[index];
        sceneImage.sprite = line.image;
        speakerText.text = line.speaker;
        dialogueText.text = line.dialogue;
    }

    void EndCutscene()
    {
        // 컷씬 종료 처리
        gameObject.SetActive(false); // 임시
    }
}