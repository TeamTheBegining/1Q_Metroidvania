using UnityEngine;

public class Scene3Manager : LocalSceneManager
{
    public override void Init()
    {
        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
        CameraManager.Instance.SetTarget(CameraType.Scene3Camera, playerObject.transform);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene3Camera, 20);
        //SoundManager.Instance.Play
        //(BGMType.guardian_01);
    }
}