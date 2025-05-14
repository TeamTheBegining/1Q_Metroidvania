using UnityEngine;

/// <summary>
// 특정 스크립트들이 사용할 텍스트 데이터
/// </summary>
[CreateAssetMenu(fileName = "Text_99", menuName = "ScriptableObject/TextData", order = 0)]
public class TextDataSO : ScriptableObject
{
    [TextArea]
    public string text;
}
