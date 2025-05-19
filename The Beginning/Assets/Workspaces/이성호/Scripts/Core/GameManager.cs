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
    private Canvas globalCanvas;

    private MessagePanel middleMessagePanel;
    public MessagePanel MiddleMessagePanel => middleMessagePanel;

    private MessagePanel bottomMessagePanel;
    public MessagePanel BottomMessagePanel => bottomMessagePanel;

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
        middleMessagePanel = transform.GetChild(0).GetChild(2).GetComponent<MessagePanel>();
        bottomMessagePanel = transform.GetChild(0).GetChild(3).GetComponent<MessagePanel>();
        globalCanvas = transform.GetChild(0).GetComponent<Canvas>();
    }

    private void Start()
    {
        globalCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        globalCanvas.worldCamera = Camera.main;
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
                OnPlay();
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

    private void OnPlay()
    {
        globalCanvas.worldCamera = Camera.main;
        FindObjectsByType<PlayerDeadPanel>(FindObjectsInactive.Include).Init();
    }

    private void OnPause()
    {
        Time.timeScale = 0f;
    }

    private void OnUnpause()
    {
        Time.timeScale = 1f;
    }

    #region CheatInputActions

    private CheatInputActions cheatActions;

    private void CheatInit()
    {
        cheatActions = new CheatInputActions();
        cheatActions.Enable();

        cheatActions.Cheat.F1.started += F1_started;
    }

    private void F1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Player player = FindAnyObjectByType<Player>();
        if(player != null)
        {
            player.CurrentMp = player.MaxMp;
        }
    }


    #endregion

#if UNITY_EDITOR
    [Space(20f)]
    [Header("Test Section")]
    public bool isDebug = false;
    public int nextSceneIndex = 0;
    public string spawnPointName;
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
        Player player = FindFirstObjectByType<Player>();
        if(player != null)
        {
            player.CurrentHp = 0f;
        }
    }

    private void PageUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }
#endif
}
