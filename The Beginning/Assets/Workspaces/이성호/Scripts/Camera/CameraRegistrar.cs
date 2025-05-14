using Unity.Cinemachine;
using UnityEngine;

public class CameraRegistrar : MonoBehaviour
{
    public CameraType type;

    void Awake()
    {
        CameraManager.Instance.Register(type, GetComponent<CinemachineCamera>());
    }

    void OnDestroy()
    {
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.Unregister(type);
        }
    }
}
