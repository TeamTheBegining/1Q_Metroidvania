using Unity.Cinemachine;
using UnityEngine;

public class CameraRegistrar : MonoBehaviour
{
    public CameraType type;

    void Awake()
    {
        CameraManager.Instance.Register(type, GetComponent<CinemachineCamera>());
    }

    void OnDisable()
    {
        if (Application.isPlaying && CameraManager.Instance != null)
        {
            CameraManager.Instance.Unregister(type);
        }
    }
}
