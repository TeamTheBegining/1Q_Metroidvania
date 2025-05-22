using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 씬 교체관련 내용이 있는 매니저
/// </summary>
public class GameSceneManager : Singleton<GameSceneManager>
{
    public SpawnPointDataSO[] pointsData;
    private SceneChangePanel sceneChangePanel;
    private SpawnPointDataSO nextSpawnData; // 씬 로드 후 스폰할 위치의 오브젝트 이름

    private void Start()
    {
        sceneChangePanel = GetComponentInChildren<SceneChangePanel>();
    }

    public void RequestSceneChange(string sceneName, SpawnPointDataSO data)
    {
        nextSpawnData = data;
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);

        if (nextSpawnData.id != "Scene2RootEnter")
        {
            sceneChangePanel.ShowPanel();
        }

        yield return new WaitUntil(() => async.isDone);

        PlayerSpawnPoint[] points = FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);

        foreach(var sp in points)
        {
            if(sp.data == nextSpawnData)    // 매개변수로 받은 데이터를 찾음
            {
                PlayerManager.Instance.SpawnPlayer(sp.transform.position);  // 해당 위치로 스폰
                break;
            }
        }

        LocalSceneManager manager = FindFirstObjectByType<LocalSceneManager>();         // 해당 씬 로컬 매니저 초기화 함수 호출
        if(manager != null)
        {
            manager.Init();
            
            if (manager.GetComponent<Scene5Manager>())
            {
                Debug.Log("Scene5 Load -------");
                CameraManager.Instance.SetCameraBlendingSpeed(0f);
            }
            else
            {
                CameraManager.Instance.SetCameraBlendingSpeed();
            }
        }

        if(nextSpawnData.id != "Scene2RootEnter")
        {
            sceneChangePanel.ClosePanel();
        }
    }

    public void LoadSceneAddive(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
    }

    public void UnloadScene(int buildIndex)
    {
        SceneManager.UnloadSceneAsync(buildIndex);
    }
}
