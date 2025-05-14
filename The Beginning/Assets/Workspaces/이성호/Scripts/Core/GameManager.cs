using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    BeforeStart = 0, // 프로그램 시작 후 State 호출 전 상태
    Play,
    CutScene,
    Pause,
    PlayEnd, // 플레이어 사망이나 게임 클리어일 때 
}

public class GameManager : Singleton<GameManager>
{
    private CinemachineBrain camBrain;
    private CinemachineCamera titleVCam;
    private MessagePanel middleMessagePanel;
    public MessagePanel MiddleMessagePanel => middleMessagePanel;

    private MessagePanel bottomMessagePanel;
    public MessagePanel BottomMessagePanel => bottomMessagePanel;

    [Tooltip("PoolType 순서대로 오브젝트를 배치 할 것")]
    public GameObject[] poolPrefabs = new GameObject[(int)PoolType.PoolTypeCount];
    public AudioClip[] audioClips; 

    [SerializeField] private GameState state;
    public GameState State
    {
        get => state;
        set
        {
            if (state == value) return; // 중복 호출 방지

            state = value;
            Initialize(state);
        }
    }

    private int enemyCount = 0;
    public int EnemyCount
    {
        get => enemyCount;
        set
        {
            enemyCount = value;
            OnEnemyCountChange?.Invoke(EnemyCount);

            if(State == GameState.Play && enemyCount == 0)
            {
                State = GameState.PlayEnd;
            }
        }
    }

    public Action<int> OnEnemyCountChange;

    protected override void Awake()
    {
        base.Awake();
        camBrain = GameObject.Find("Main Camera").GetComponent<CinemachineBrain>();
        titleVCam = transform.GetChild(0).GetComponent<CinemachineCamera>();
        middleMessagePanel = transform.GetChild(2).GetComponent<MessagePanel>();
        bottomMessagePanel = transform.GetChild(3).GetComponent<MessagePanel>();
    }

    private void Start()
    {
        State = GameState.BeforeStart;
#if UNITY_EDITOR
        TestInit();
#endif
}

    private void Update()
    {
        
    }

    private void Initialize(GameState state)
    {
        if(state != GameState.Pause)
        {
            OnUnpause();
        }

        switch (state)
        {
            case GameState.BeforeStart:
                break;
            case GameState.Play:
                break;
            case GameState.Pause:
                OnPause();
                break;
            case GameState.CutScene:
                break;
            default:
                break;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    /*    public void SceneChange(int sceneIndex)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneIndex);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // 중복 호출 방지
            PoolManager.Instance.ClearPoolData();
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                State = GameState.Menu;
            }
            else
            {
                State = GameState.Play;
            }
        }*/

    private void OnPause()
    {
        Time.timeScale = 0f;
    }

    private void OnUnpause()
    {
        Time.timeScale = 1f;
    }

    // NOTE 밑의 내용 카메라 매니저로 새로 생성해서 분리하기

    #region Title Cam
    public void ShowTitleCamera()
    {
        titleVCam.Priority = 20;
    }

    public void HideTitleCamera()
    {
        titleVCam.Priority = 0;
    }
    #endregion

    #region Cinemachine Brain
    public void SetCameraBlendingSpeed(float blendTime = 2f)
    {
        camBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendTime);
    }
    #endregion

#if UNITY_EDITOR

    [Space(20f)]
    [Header("Test Section")]
    public int nextSceneIndex = 0;
    private TestInputActions testActions;

    private void TestInit()
    {
        testActions = new TestInputActions();
        testActions.Enable();
        testActions.Test.PageUp.performed += PageUp_performed;
        testActions.Test.PageDown.performed += PageDown_performed;
    }

    private void PageDown_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        GameSceneManager.Instance.ChangeScene(1, true);
        LightManager.Instance.SetGlobalLight(Color.white);
    }

    private void PageUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        GameSceneManager.Instance.ChangeScene(nextSceneIndex);
        LightManager.Instance.SetGlobalLight(Color.white);
    }


#endif
}
