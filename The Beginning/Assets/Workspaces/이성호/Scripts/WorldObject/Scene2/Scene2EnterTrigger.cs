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
            //SoundManager.Instance.PlayBGM(BGMType.bgm_Credit_01);
            //Debug.Log("credit 재생!");
            GameSceneManager.Instance.RequestSceneChange("Scene2", targetData);
        }
    }
}
