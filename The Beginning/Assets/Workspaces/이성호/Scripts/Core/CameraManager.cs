using Unity.Cinemachine;
using UnityEngine;

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