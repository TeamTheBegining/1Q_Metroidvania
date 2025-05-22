using UnityEngine;

public class Scene4Manager : LocalSceneManager
{
    public override void Init()
    {
        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
        CameraManager.Instance.SetTarget(CameraType.Scene4Camera, playerObject.transform);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene4Camera, 20);
    }
}
