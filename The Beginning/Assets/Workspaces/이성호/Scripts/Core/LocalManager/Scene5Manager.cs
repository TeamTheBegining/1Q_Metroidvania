using UnityEngine;

public class Scene5Manager : LocalSceneManager
{
    PlayerShaderLight playerShaderLight;

    public override void Init()
    {
        // 임시
        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
        CameraManager.Instance.SetTarget(CameraType.Scene5CameraUpper, playerObject.transform);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene5CameraUpper, 20);

        playerShaderLight = FindFirstObjectByType<PlayerShaderLight>();
    }

    public void SetScene5Priority(CameraType type)
    {
        if (type < CameraType.Scene5CameraUpper)
        {
            Debug.LogWarning($"{type}은 현재 씬에 존재하지 않는 카메라");
            return;
        }

        int startIndex = (int)CameraType.Scene5CameraUpper;
        int endIndex = (int)CameraType.Scene5CameraBottomRoom1;

        for(int i = startIndex; i < endIndex + 1; i++)
        {
            if ((int)type == i)
            {
                GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
                CameraManager.Instance.SetVirtualCameraPriority(type, 20);
                CameraManager.Instance.SetTarget(type, playerObject.transform);
            }
            else
            {
                CameraManager.Instance.SetVirtualCameraPriority((CameraType)i, 0);
                CameraManager.Instance.SetTarget((CameraType)i, null);
            }
        }
    }
}
