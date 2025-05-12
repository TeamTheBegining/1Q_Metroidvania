using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    BeforeStart = 0, // 프로그램 시작 후 State 호출 전 상태
    Menu,
    Play,
    PlayEnd, // 플레이어 사망이나 게임 클리어일 때 
}

public class GameManager : Singleton<GameManager>
{
    [Tooltip("PoolType 순서대로 오브젝트를 배치 할 것")]
    public GameObject[] poolPrefabs = new GameObject[(int)PoolType.PoolTypeCount];
    public AudioClip[] audioClips; 

    private GameState state;
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
    }

    private void Start()
    {
        State = GameState.Menu;
    }

    private void Initialize(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                break;
            case GameState.Play:
                break;
            default:
                break;
        }
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    public void SceneChange(int sceneIndex)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 중복 호출 방지
        PoolManager.Instacne.ClearPoolData();
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            State = GameState.Menu;
        }
        else
        {
            State = GameState.Play;
        }
    }
}
