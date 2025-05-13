using UnityEngine;
using UnityEngine.SceneManagement;

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
