using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : Singleton<PlayerManager>
{
    public GameObject playerPrefab;
    private string spawnSceneName;
    private SpawnPointDataSO spawnData;

    private int coin;

    /// <summary>
    /// coin 접근용 프로퍼티
    /// </summary>
    public int Coin => coin;

    private float remainHp; // NOTE : 플레이어 체력 이벤트에 연결할 것

    private bool[] isSkillUnlock;

    /// <summary>
    /// isSkillUnlock 접근용 프로퍼티
    /// </summary>
    public bool[] IsSkillUnlock => isSkillUnlock;

    private void Start()
    {
        isSkillUnlock = new bool[(int)PlayerSkillType.PlayerSkillTypeCount];
    }

    /// <summary>
    /// 플레이어 남은 체력 저장하는 함수
    /// </summary>
    /// <param name="value">저장할 체력 값</param>
    public void SetRemainHp(float value)
    {
        remainHp = value;
    }

    /// <summary>
    /// 플레이어 돈 사용 함수 ( value가 소지한 coin값보다 많으면 무시 )
    /// </summary>
    /// <param name="value">사용할 돈의 값</param>
    public void UseCoin(int value)
    {
        if (value > coin) // 소비할 값이 소지한 돈보다 많음
        {
            Debug.Log($"{value}가 소지한 값 [{coin}] 보다 많습니다. (소비 안됨)");            
        }
        else
        {
            coin -= value;
        }        
    }

    /// <summary>
    /// 플레이어 돈 추가 함수
    /// </summary>
    /// <param name="value">추가할 돈의 값</param>
    public void AddCoin(int value)
    {
        coin += value;
    }

    /// <summary>
    /// 스킬 언락 함수 ( bool 값 true로 변경 )
    /// </summary>
    /// <param name="type">활성화할 플레이어 스킬 타입</param>
    public void UnlockPlayerSkill(PlayerSkillType type)
    {
        isSkillUnlock[(int)type] = true;
    }

    /// <summary>
    /// 스폰 포인트 정보 저장 함수 
    /// </summary>
    /// <param name="sceneName">스폰할 씬 빌드 인덱스</param>
    /// <param name="data">스폰할 위치 데이터</param>
    public void SetSpawn(string sceneName, SpawnPointDataSO data)
    {
        spawnSceneName = sceneName;
        spawnData = data;  
    }

    public void Respawn()
    {
        if(spawnData == null) // 스폰 데이터가 존재하지 않으면
        {
            SceneManager.LoadScene(0); // 강제로 시작 씬 불러오기
        }
        else
        {
            GameSceneManager.Instance.RequestSceneChange(spawnSceneName, spawnData);
        }

        GameManager.Instance.State = GameState.Play;
    }

    /// <summary>
    /// 플레이어 스폰 함수 ( 플레이어 제거 확인 및 씬 전환 후 호출 할 것 )
    /// </summary>
    public Player SpawnPlayer(Vector2 spawnVector)
    {
        Player player = Instantiate(playerPrefab, spawnVector, Quaternion.identity).GetComponent<Player>();

        if (remainHp <= 0f)
        {
            player.CurrentHp = player.MaxHp;
            remainHp = player.MaxHp;
        }
        else
        {
            player.CurrentHp = remainHp;
        }

        PlayerDeadPanel deadPanel = FindFirstObjectByType<PlayerDeadPanel>();
        if (deadPanel != null)
        {
            deadPanel.Init();
        }

        return player;
    }

    public void StatUp(SkillUnlockItemSO data)
    {
        Player player = FindFirstObjectByType<Player>();

        // 플레이어 스탯업 함수 사용
    }
}