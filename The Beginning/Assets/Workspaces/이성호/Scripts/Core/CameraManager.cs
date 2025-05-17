using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Cinemachine virtual camera를 찾아서 설정하거나 Cinemachine brain을 설정하는 매니저
/// </summary>
public class CameraManager : Singleton<CameraManager>
{
    private GameObject mainCamera;
    private CinemachineBrain camBrain;
    private Dictionary<CameraType, CinemachineCamera> cameraDictionary = new Dictionary<CameraType, CinemachineCamera>((int)CameraType.CameraTypeCount);

    protected override void Awake()
    {
        base.Awake();

        GameObject localCam = GameObject.Find("Main Camera");
        if (localCam != null)
        {
            Destroy(localCam);
        }
    }

    private void Start()
    {
        mainCamera = Resources.Load<GameObject>("Prefabs/Camera/Main Camera");
        DontDestroyOnLoad(Instantiate(mainCamera));
        camBrain = mainCamera.GetComponent<CinemachineBrain>();

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        GameObject localCam = GameObject.Find("Main Camera");
        if (localCam != mainCamera)
        {
            Destroy(localCam);
        }
    }

    /// <summary>
    /// 카메라 등록함수
    /// </summary>
    /// <param name="type">등록할 카메라 타입</param>
    /// <param name="comp">컴포넌트</param>
    public void Register(CameraType type, CinemachineCamera comp)
    {
        if (cameraDictionary.ContainsKey(type))
        {
            Debug.Log($"{type.ToString()} is already registered");
            return;
        }
        cameraDictionary.Add(type, comp);
    }

    /// <summary>
    /// 카메라 등록 제거 함수 ( 카메라 파괴 시 )
    /// </summary>
    /// <param name="type">카메라 타입</param>
    public void Unregister(CameraType type)
    {
        cameraDictionary.Remove(type);
    }

    /// <summary>
    /// 카메라 우선순위 강제로 설정하는 함수
    /// </summary>
    /// <param name="type">카메라 타입</param>
    /// <param name="priority">우선순위 값</param>
    public void SetVirtualCameraPriority(CameraType type, int priority) // 임시 함수 
    {
        cameraDictionary.TryGetValue(type, out CinemachineCamera camera);
        if (camera != null)
        {
            camera.Priority = priority;
        }
        else
        {
            Debug.LogWarning($"{type.ToString()} 타입 카메라가 존재하지 않습니다.");
        }
    }

    #region Title Cam

    /// <summary>
    /// 카메라 우선순위 20으로 올리는 함수
    /// </summary>
    /// <param name="type">카메라 타입</param>
    public void ShowTitleCamera(CameraType type)
    {
        cameraDictionary.TryGetValue(type, out CinemachineCamera camera);
        if (camera != null)
        {
            camera.Priority = 20;
        }
        else
        {
            Debug.LogWarning($"{type.ToString()} 타입 카메라가 존재하지 않습니다.");
        }

    }

    /// <summary>
    /// 카메라 우선순위 0으로 낮추는 함수
    /// </summary>
    /// <param name="type">카메라 타입</param>
    public void HideTitleCamera(CameraType type)
    {
        cameraDictionary.TryGetValue(type, out CinemachineCamera camera);
        if (camera != null)
        {
            camera.Priority = 0;
        }
        else
        {
            Debug.LogWarning($"{type.ToString()} 타입 카메라가 존재하지 않습니다.");
        }
    }

    /// <summary>
    /// 카메라가 따라갈 타겟 설정 함수
    /// </summary>
    /// <param name="type">카메라 타입</param>
    /// <param name="target">따라갈 타켓 트랜스폼</param>
    public void SetTarget(CameraType type, Transform target)
    {
        cameraDictionary.TryGetValue(type, out CinemachineCamera camera);
        if (camera != null)
        {
            camera.Target.TrackingTarget = target;
        }
        else
        {
            Debug.LogWarning($"{type.ToString()} 타입 카메라가 존재하지 않습니다.");
        }
    }

    #endregion

    #region Cinemachine Brain
    public void SetCameraBlendingSpeed(float blendTime = 2f)
    {
        camBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendTime);
    }
    #endregion
}