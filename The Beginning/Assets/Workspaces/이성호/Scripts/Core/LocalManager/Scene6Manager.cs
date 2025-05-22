using UnityEngine;

public class Scene6Manager : LocalSceneManager
{
    public override void Init()
    {
        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
        CameraManager.Instance.SetTarget(CameraType.Scene6Camera, playerObject.transform);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene6Camera, 20);
    }
}