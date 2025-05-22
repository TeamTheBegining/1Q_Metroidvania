using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    public EndingNpc player;
    public EndingNpc ghost;

    public float delay = 3f;

    private void Start()
    {
        StartCoroutine(EndingSceneProcess());
    }

    private IEnumerator EndingSceneProcess()
    {
        player.ShowText(0);
        yield return new WaitForSeconds(delay);
        ghost.ShowText(0);
        yield return new WaitForSeconds(delay);
        player.ShowText(1);
        yield return new WaitForSeconds(delay);
        ghost.ShowText(1);
        yield return new WaitForSeconds(delay);
        player.ShowText(2);
        yield return new WaitForSeconds(delay);
        ghost.ShowText(2);
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(0);
    }
}
