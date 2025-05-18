using UnityEngine;

/// <summary>
/// 상호작용 시 targetBuildIndex 번호의 씬으로 교체하는 상호작용 클래스
/// </summary>
public class SceneChangePortal : MonoBehaviour, Interactable
{
    public SpawnPointDataSO targetData;
    public string targetSceneName;

    private bool isTrigger = false;

    public void OnInteraction()
    {
        if (isTrigger) return;

        GameSceneManager.Instance.RequestSceneChange(targetSceneName, targetData);
        isTrigger = true;
    }
}
