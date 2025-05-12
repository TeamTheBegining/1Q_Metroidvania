using UnityEngine;

[CreateAssetMenu(fileName = "Text_99", menuName = "ScriptableObject/TextData", order = 0)]
public class TextDataSO : ScriptableObject
{
    [TextArea]
    public string text;
}
