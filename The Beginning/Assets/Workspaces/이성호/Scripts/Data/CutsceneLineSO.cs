using UnityEngine;

[CreateAssetMenu(fileName = "CutsceneLine_99", menuName = "ScriptableObject/Cutscene/CutsceneLine", order = 0)]
public class CutsceneLineSO : ScriptableObject
{
    public Sprite image;
    public string speaker;
    [TextArea]
    public string dialogue;
}
