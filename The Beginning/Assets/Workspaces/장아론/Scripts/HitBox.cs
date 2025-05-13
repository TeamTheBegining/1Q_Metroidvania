using UnityEngine;

// 이 스크립트는 B_Girl 게임오브젝트의 자식인 히트박스 게임오브젝트에 붙습니다.
public class EnemyHitbox : MonoBehaviour
{
    // 부모 오브젝트(B_Girl)에 붙어있는 B_GirlController 스크립트 참조
    // GetComponentInParent를 사용하기 위해 히트박스 오브젝트가 B_Girl의 자식이어야 합니다.
    private B_GirlController enemyController;

    // 선택 사항: 같은 스윙으로 같은 대상을 여러 번 히트하는 것을 방지하는 플래그
    // private bool hasHitTargetInSwing = false; // 예시 변수

    void Awake()
    {
        // 시작 시 부모(또는 그 위) 오브젝트에서 B_GirlController 컴포넌트를 찾습니다.
        enemyController = GetComponentInParent<B_GirlController>();

        if (enemyController == null)
        {
            Debug.LogError("EnemyHitbox 스크립트는 부모 오브젝트에 B_GirlController(또는 상속받은 클래스)가 필요합니다.", this);
            // 컨트롤러를 찾지 못하면 스크립트 비활성화 (에러 방지)
            enabled = false;
            return; // 더 이상 진행하지 않음
        }

        // 이 스크립트가 붙은 게임오브젝트의 콜라이더가 Trigger로 설정되어 있는지 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            // Warning을 띄워 설정 오류를 알려줌
            Debug.LogWarning(gameObject.name + "의 콜라이더가 Trigger로 설정되어 있지 않습니다. OnTriggerEnter를 사용하려면 Trigger여야 합니다.", this);
        }

        // 중요: 이 히트박스 게임오브젝트 자체는 처음에 비활성화 상태여야 합니다.
        // 애니메이션 이벤트가 타이밍에 맞춰 활성화/비활성화를 제어합니다.
    }

    // 콜라이더가 다른 콜라이더 영역 안으로 들어왔을 때 호출됩니다.
    // 이 함수가 호출되려면 이 게임오브젝트의 콜라이더와 상대방 콜라이더 모두 있어야 하고,
    // 둘 중 하나 이상은 Is Trigger가 체크되어 있어야 합니다.
    void OnTriggerEnter(Collider other)
    {
        // 이 히트박스 게임오브젝트가 현재 활성화 상태인지 다시 한번 확인 (선택 사항, 안정성 증진)
        if (!gameObject.activeInHierarchy) return;

        // 선택 사항: 한 번의 스윙(활성화 기간) 동안 같은 대상을 여러 번 때리는 것을 방지
        // if (hasHitTargetInSwing) return; // EnableHitbox 등에서 이 플래그를 리셋해야 함

        // 충돌한 오브젝트가 플레이어인지 확인합니다.
        // 플레이어 오브젝트에 "Player" 태그를 붙여두는 것이 가장 일반적인 방법입니다.
        if (other.CompareTag("Player"))
        {
            // 충돌한 플레이어 오브젝트에서 데미지를 처리할 스크립트(예: PlayerHealth)를 가져옵니다.
            // 플레이어 오브젝트에 PlayerHealth라는 스크립트가 붙어있다고 가정합니다.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // PlayerHealth 스크립트가 존재하고, enemyController도 제대로 연결되어 있다면
            if (playerHealth != null && enemyController != null)
            {
                // PlayerHealth 스크립트의 데미지 처리 메소드(예: TakeDamage)를 호출합니다.
                // 이때, enemyController에 정의된 공격력 값을 전달합니다.
                // B_GirlController에 public float attackDamage; 변수가 있어야 합니다.
                playerHealth.TakeDamage(enemyController.attackDamage);

                Debug.Log(gameObject.name + "가 플레이어를 공격하여 " + enemyController.attackDamage + "만큼의 데미지를 주었습니다.");

                // 선택 사항: 여러 번 히트 방지 플래그 설정
                // hasHitTargetInSwing = true;

                // 선택 사항: 플레이어를 한 번 히트한 후 즉시 이 히트박스 비활성화 (매우 주의해서 사용)
                // gameObject.SetActive(false); // <-- 이렇게 하면 해당 애니메이션 이벤트가 끝날 때까지 켜지지 않으므로, 애니메이션 이벤트 로직과 잘 맞는지 확인 필요
            }
        }

        // 만약 다른 오브젝트(예: 파괴 가능한 환경 오브젝트)에게도 데미지를 줄 수 있다면 여기에 추가적인 충돌 체크 로직을 구현합니다.
        // else if (other.CompareTag("DestructibleObject")) { ... }
    }

    // 선택 사항: 같은 스윙 여러 번 히트 방지를 위해, 애니메이션 이벤트의 EnableHitbox에서 이 메소드를 호출하여 플래그 리셋
    // public void ResetHitFlag()
    // {
    //     hasHitTargetInSwing = false;
    // }
}