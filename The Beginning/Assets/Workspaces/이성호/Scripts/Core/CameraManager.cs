using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Cinemachine virtual camera를 찾아서 설정하거나 Cinemachine brain을 설정하는 매니저
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    public void SetVirtualCameraPriority(string objectName, int priority) // 임시 함수 
    {
        CinemachineCamera cam = GameObject.Find(objectName).GetComponent<CinemachineCamera>();
        if(cam != null)
        {
            cam.Priority = priority;
        }
        else
        {
            Debug.LogWarning($"Invaild GameObject {objectName}");
        }
    }
}