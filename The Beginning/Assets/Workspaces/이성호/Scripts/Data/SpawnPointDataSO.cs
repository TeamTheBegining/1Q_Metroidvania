using UnityEngine;

[CreateAssetMenu(fileName = "SpawnPoint_99", menuName = "ScriptableObject/SpawnPoint", order = 0)]
public class SpawnPointDataSO : ScriptableObject
{
    [Tooltip("씬 불러올 때 찾을 id")]
    public string id;
}
