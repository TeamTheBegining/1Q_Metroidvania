using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬 교체관련 내용이 있는 매니저
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    private SceneChangePanel sceneChangePanel;
    private float sceneChangeProcessDuration = 1f; // todo : 씬 전환 패널 연결하기

    private void Start()
    {
        sceneChangePanel = GetComponentInChildren<SceneChangePanel>();
    }

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(SceneChangeProcess(sceneName));
        SceneManager.GetSceneByName(sceneName);
    }

    public void ChangeScene(int buildIndex, bool isAdditive = false)
    {
        if(isAdditive)
        {
            SceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
        }
        else
        {
            StartCoroutine(SceneChangeProcess(buildIndex));
            SceneManager.LoadScene(buildIndex);
        }
    }

    private IEnumerator SceneChangeProcess(string sceneName)
    {
        sceneChangePanel.ShowPanel();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            Debug.Log("Load in process");
            yield return null;
        }
        Debug.Log("Load done !");
    }

    private IEnumerator SceneChangeProcess(int buildIndex)
    {
        sceneChangePanel.ForceShowPanel();

        AsyncOperation operation = SceneManager.LoadSceneAsync(buildIndex);

        while(!operation.isDone)
        {
            Debug.Log("Load in process");
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        Debug.Log("Load done !");
        sceneChangePanel.ClosePanel();
    }
}
