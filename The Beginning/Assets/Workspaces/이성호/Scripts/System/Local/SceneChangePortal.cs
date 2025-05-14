using UnityEngine;

/// <summary>
/// 상호작용 시 targetBuildIndex 번호의 씬으로 교체하는 상호작용 클래스
/// </summary>
public class SceneChangePortal : MonoBehaviour, Interactable
{
    public int targetBuildIndex = 0;
    public void OnInteraction()
    {
        GameSceneManager.Instance.ChangeScene(targetBuildIndex);
    }
}
