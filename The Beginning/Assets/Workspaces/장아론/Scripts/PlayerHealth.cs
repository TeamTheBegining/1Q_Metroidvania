using UnityEngine;

// 이 스크립트는 플레이어 게임오브젝트에 붙습니다.
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f; // 최대 체력
    private float currentHealth; // 현재 체력

    void Start()
    {
        currentHealth = maxHealth; // 게임 시작 시 체력 최대치로 설정
        Debug.Log("플레이어 체력 초기화: " + currentHealth);
    }

    // 외부(예: 에너미 히트박스 스크립트)에서 호출하여 데미지를 주는 메소드
    public void TakeDamage(float amount)
    {
        // 이미 죽었다면 데미지 받지 않음
        if (currentHealth <= 0) return;

        // 받은 데미지만큼 체력 감소
        currentHealth -= amount;
        Debug.Log("플레이어가 " + amount + "만큼 데미지를 받았습니다. 현재 체력: " + currentHealth);

        // 체력이 0 이하가 되면 사망 처리
        if (currentHealth <= 0)
        {
            Die();
        }

        // TODO: 체력 UI 업데이트 등의 추가 로직 구현
    }

    // 플레이어가 사망했을 때 호출될 메소드
    void Die()
    {
        Debug.Log("플레이어 사망!");
        // TODO: 플레이어 사망 애니메이션 재생, 게임 오버 처리, 씬 재시작 등의 로직 구현
        gameObject.SetActive(false); // 간단한 예시: 플레이어 게임오브젝트 비활성화
    }

    // 현재 체력을 외부에 알려주는 Getter 메소드 (선택 사항)
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}