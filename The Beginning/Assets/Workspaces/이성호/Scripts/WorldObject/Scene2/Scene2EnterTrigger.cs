using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene2EnterTrigger : MonoBehaviour
{
    public SpawnPointDataSO targetData;

    private bool isTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isTrigger && collision.CompareTag("Player"))
        {
            isTrigger = true;

            GameManager.Instance.State = GameState.CutScene;
            CutSceneManager.Instance.ShowCutscene(1);
            GameSceneManager.Instance.RequestSceneChange("Scene2", targetData);
        }
    }
}
