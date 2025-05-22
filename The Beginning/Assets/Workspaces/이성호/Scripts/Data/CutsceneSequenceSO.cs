using UnityEngine;

/// <summary>
/// CutsceneLine들을 가지고 있는 시퀀스 데이터
/// </summary>
[CreateAssetMenu(fileName = "CutsceneSequence_99", menuName = "ScriptableObject/Cutscene/CutsceneSequence", order = 1)]
public class CutsceneSequenceSO : ScriptableObject
{
    public CutsceneLineSO[] lines;
        
}
