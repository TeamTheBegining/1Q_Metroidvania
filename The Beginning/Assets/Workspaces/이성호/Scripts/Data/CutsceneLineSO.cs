using UnityEngine;

/// <summary>
/// 컷 씬 오브젝트 데이터 ( UI 오브젝트 )
/// </summary>
[CreateAssetMenu(fileName = "CutsceneLine_99", menuName = "ScriptableObject/Cutscene/CutsceneLine", order = 0)]
public class CutsceneLineSO : ScriptableObject
{
    [Tooltip("보여줄 오브젝트")]
    public GameObject CutsceneObject;
    [Tooltip("보여주는 시간")]
    public float showTime; 
}
