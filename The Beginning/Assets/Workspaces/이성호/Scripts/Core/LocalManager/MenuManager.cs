using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        //SoundManager.Instance.Play(BGMType.menu);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.buildIndex == 0)
        {
            DestroyAllPersistentObjects();

            Camera cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            if (cam == null)
            {
                GameObject obj = Resources.Load<GameObject>("Prefabs/Main Camera");
                Instantiate(obj);
            }
        }
    }

    void DestroyAllPersistentObjects()
    {
        // 1. 현재의 임시 오브젝트 생성
        GameObject temp = new GameObject("Temp");
        DontDestroyOnLoad(temp);

        // 2. 모든 루트 오브젝트 수집
        GameObject[] allRootObjects = temp.scene.GetRootGameObjects();

        // 3. temp 삭제
        Destroy(temp);

        // 4. 해당 씬의 오브젝트들 삭제
        foreach (GameObject obj in allRootObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }

    public void GameStart()
    {
        SceneManager.LoadScene(1);
    }

    public void Exit()
    {
        Application.Quit();
    }
}
