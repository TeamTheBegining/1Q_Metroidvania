using UnityEngine;

/// 단순히 고유 ID 부착과 매니저와 상태 동기화를 목적으로 한 클래스

/// <summary>
/// 적의 상태를 매니저에 동기화 하기 위한 독립 컴포넌트
/// </summary>
public class EnemyStatusBridge : MonoBehaviour
{
    [Tooltip("상태 확인할 적의 고유 아이디 값을 정의")]
    [SerializeField] private string enemyID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(enemyID))
        {
            Debug.LogError($"[EnemyStatusBridge] 적 ID가 비어있습니다! GameObject: {gameObject.name}");
        }
        else
        {
            EnemyStateManager.Instance.RegisterEnemy(enemyID);
        }
    }

    private void Start()
    {
        if (EnemyStateManager.Instance.IsEnemyDead(enemyID))
        {
            gameObject.SetActive(false); // 이미 죽은 적은 비활성화
        }
    }

    /// <summary>
    /// 사망 시 매니저(EnemyStateManager)에 사망을 알리는 함수 ( 적 사망 시 호출될 함수에 호출 할 것 )
    /// </summary>
    public void MarkAsDead()
    {
        EnemyStateManager.Instance.SetEnemyDead(enemyID);
    }
}
