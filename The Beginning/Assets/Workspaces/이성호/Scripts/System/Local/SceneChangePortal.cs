using UnityEngine;

public class SceneChangePortal : MonoBehaviour, Interactable
{
    public int targetBuildIndex = 0;
    public void OnInteraction()
    {
        GameSceneManager.Instance.ChangeScene(targetBuildIndex);
    }
}
