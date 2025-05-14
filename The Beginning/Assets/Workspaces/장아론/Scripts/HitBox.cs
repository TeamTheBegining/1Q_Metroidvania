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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(enemyController.attackDamage);
    }

    // 선택 사항: 같은 스윙 여러 번 히트 방지를 위해, 애니메이션 이벤트의 EnableHitbox에서 이 메소드를 호출하여 플래그 리셋
    // public void ResetHitFlag()
    // {
    //     hasHitTargetInSwing = false;
    // }
}