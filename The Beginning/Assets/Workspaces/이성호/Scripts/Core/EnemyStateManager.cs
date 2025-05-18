using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemyState
{
    public string uniqueID;
    public bool isDead;
}

public class EnemyStateManager : Singleton<EnemyStateManager>
{
    public GameObject[] enemyPrefabs;
    private Dictionary<string, EnemyState> allEnemies = new Dictionary<string, EnemyState>();

    /// <summary>
    /// 적 아이디 등록 함수
    /// </summary>
    /// <param name="id">적 고유 아이디 문자열</param>
    public void RegisterEnemy(string id) // 적 처음 시작시 저장
    {
        if (!allEnemies.ContainsKey(id))
        {
            allEnemies.Add(id, new EnemyState { uniqueID = id, isDead = false });
        }
        else
        {
            Debug.LogWarning($"Enemy with ID '{id}' is already registered."); // 중복된 아이디
        }
    }

    /// <summary>
    /// 적이 사망했는지 확인하는 함수
    /// </summary>
    /// <param name="id">적 고유 아이디</param>
    /// <returns>사망하면 true, 살아있으면 false</returns>
    public bool IsEnemyDead(string id) // 적 사망 체크 함수
    {
        // ID가 존재하는지 확인 후 상태 리턴
        return allEnemies.ContainsKey(id) && allEnemies[id].isDead;
    }

    /// <summary>
    /// 사망 설정 함수
    /// </summary>
    /// <param name="id">적 고유 아이디</param>
    public void SetEnemyDead(string id) // 적 사망시 호출될 함수
    {
        if (allEnemies.ContainsKey(id))
        {
            allEnemies[id].isDead = true;
        }
    }

    /// <summary>
    /// 리스트에 있는 모든 적 사망 초기화 함수
    /// </summary>
    public void ResetAllEnemies() // 세이브 포인트, 플레이어 사망 시 호출될 함수
    {
        foreach (var enemy in allEnemies.Values)
        {
            enemy.isDead = false;
        }
    }
}