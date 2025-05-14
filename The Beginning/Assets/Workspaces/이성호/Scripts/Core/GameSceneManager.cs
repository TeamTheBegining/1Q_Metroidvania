using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬 교체관련 내용이 있는 매니저
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    public void ChangeScene(string sceneName)
    {
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
            SceneManager.LoadScene(buildIndex);
        }
    }
}
