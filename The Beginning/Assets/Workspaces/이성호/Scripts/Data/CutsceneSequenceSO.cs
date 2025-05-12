using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneSequence_99", menuName = "ScriptableObject/Cutscene/CutsceneSequence", order = 1)]
public class CutsceneSequenceSO : ScriptableObject
{
    public CutsceneLineSO[] lines;
}
