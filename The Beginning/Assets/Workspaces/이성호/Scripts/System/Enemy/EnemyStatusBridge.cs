using UnityEngine;


/// <summary>
/// 적의 상태를 매니저에 동기화 하기 위한 독립 컴포넌트
/// </summary>
/// <remarks>
/// 단순히 고유 ID 부착과 매니저와 상태 동기화를 목적으로 한 클래스
/// </remarks>
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
            Debug.Log("deactive");
        }
    }

    /// <summary>
    /// 적이 사망할 때 EnemyStateManager에 상태를 알리는 함수.
    /// (적 사망 처리 함수 내에서 호출할 것)
    /// </summary>
    /// <remarks>
    /// 이 함수 호출로 씬 로드 후에도 적 사망 상태가 유지됩니다.
    /// 2025.05.18 - 작성자 : 이성호
    /// </remarks>
    public void MarkAsDead()
    {
        // 씬 로드 후에도 적 사망 유지를 위한 상태 설정

        EnemyStateManager.Instance.SetEnemyDead(enemyID);
    }
}
