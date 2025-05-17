using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraRegistrar : MonoBehaviour
{
    public CameraType type;

    void Awake()
    {
        CameraManager.Instance.Register(type, GetComponent<CinemachineCamera>());
        //SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
    }

    private void SceneManager_sceneUnloaded(Scene arg0)
    {
        if (Application.isPlaying && CameraManager.Instance != null)
        {
            CameraManager.Instance.Unregister(type);
        }

        SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
    }
}
