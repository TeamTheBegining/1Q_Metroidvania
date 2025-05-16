using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene2EnterTrigger : MonoBehaviour
{
    private bool isTrigger = false;
    public Collider2D groundCollider;

    public float duration = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isTrigger && collision.CompareTag("Player"))
        {
            groundCollider.enabled = true;
            isTrigger = true;
            gameObject.SetActive(false);
        }

        CutSceneManager.Instance.ShowCutscene(1);
    }

    private IEnumerator CameraSetProcess()
    {
        float elapsedTime = 0.0f;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
