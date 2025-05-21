using UnityEngine;

/// <summary>
/// 상호작용 시 targetBuildIndex 번호의 씬으로 교체하는 상호작용 클래스
/// </summary>
public class SceneChangePortal : MonoBehaviour, Interactable
{
    public SpawnPointDataSO targetData;
    public string targetSceneName;

    public bool useTrigger = false;
    private bool isTriggerd = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(useTrigger)
            {
                GameSceneManager.Instance.RequestSceneChange(targetSceneName, targetData);
                isTriggerd = true;
            }
        }
    }

    public void OnInteraction()
    {
        if (isTriggerd) return;

        GameSceneManager.Instance.RequestSceneChange(targetSceneName, targetData);
        isTriggerd = true;
    }
}
