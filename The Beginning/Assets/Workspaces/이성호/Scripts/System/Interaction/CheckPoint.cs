using UnityEngine;

public class CheckPoint : MonoBehaviour, Interactable
{
    public void OnInteraction()
    {
        Debug.Log("-- [ 체크포인트 상호 작용 ] --");
        // TODO : 플레이어 체력 회복 추가
        EnemyStateManager.Instance.ResetAllEnemies(); // 모든 적 리스폰
    }
}
