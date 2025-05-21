using UnityEngine;
using System.Collections.Generic; // HashSet을 사용하려면 필요

// 이 스크립트가 붙은 게임오브젝트의 콜라이더가 Trigger로 설정되어 있는지 확인
[RequireComponent(typeof(Collider2D))] // Collider2D가 필수로 요구되도록 추가
public class EnemyHitbox : MonoBehaviour
{
    // 부모 오브젝트 또는 다른 컨트롤러에서 설정할 공격력 값
    public float attackDamage;

    // 이 히트박스가 이미 공격한 대상을 저장하는 HashSet
    // HashSet은 중복을 허용하지 않고, 검색 및 추가/삭제에 효율적입니다.
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    // B_GirlController와 같이 해당 히트박스를 제어하는 주체 (선택 사항, 직접 접근하지 않고 이벤트 등으로 제어하는 것이 더 유연)
    // private CommonEnemyController enemyController; // 필요하다면 주석 해제하여 사용

    void Awake()
    {
        // Collider2D를 필수로 요구했으므로, 여기서 null 체크는 필요 없습니다.
        // 하지만 isTrigger 설정 여부는 확인하는 것이 좋습니다.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning(gameObject.name + "의 Collider2D가 Trigger로 설정되어 있지 않습니다. OnTriggerEnter2D를 사용하려면 Trigger여야 합니다.", this);
        }

        // CommonEnemyController를 참조할 필요가 없다면 (즉, 이 히트박스가 직접 데미지를 줄 대상만 알면 된다면)
        // 아래 코드는 필요 없습니다.
        // enemyController = GetComponentInParent<CommonEnemyController>();
        // if (enemyController == null)
        // {
        //     Debug.LogWarning($"EnemyHitbox on {gameObject.name}: CommonEnemyController (or derived) not found in parent. This hitbox may not function as expected for some enemies.", this);
        // }
    }

    private void OnTriggerEnter2D(Collider2D collision) // 2D 게임이므로 OnTriggerEnter2D 사용
    {
        // 사망 상태이거나, 이미 공격한 대상이라면 다시 데미지 주지 않음
        // 여기서는 히트박스가 충돌을 감지하므로, 죽은 상태 체크는 상위 컨트롤러에서 해주거나
        // EnemyStatusBridge와 같은 컴포넌트를 통해 확인해야 합니다.
        // 현재는 단순히 이미 hitTargets에 포함된 대상인지 확인합니다.
        if (hitTargets.Contains(collision.gameObject))
        {
            return;
        }

        // 충돌한 대상이 플레이어인지 태그로 확인
        if (collision.CompareTag("Player"))
        {
            IDamageable damageableTarget = collision.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(attackDamage, this.gameObject); // 이 히트박스 오브젝트를 공격 주체로 전달
                hitTargets.Add(collision.gameObject); // 공격한 대상을 기록
                //Debug.Log($"플레이어에게 {attackDamage} 데미지 적용! (히트박스: {gameObject.name})");
            }
        }
        // TODO: 다른 공격 가능한 대상(예: 파괴 가능한 오브젝트)이 있다면 여기에 추가
    }

    /// <summary>
    /// 이 히트박스가 이미 공격했던 대상을 초기화합니다.
    /// 새로운 공격 애니메이션이 시작될 때 호출하여 중복 피해를 방지합니다.
    /// </summary>
    public void ResetHitPlayers()
    {
        hitTargets.Clear();
        //Debug.Log($"{gameObject.name} 히트박스의 hitTargets가 초기화되었습니다.");
    }

    // 선택 사항: 히트박스가 활성화될 때마다 ResetHitPlayers를 자동으로 호출
    private void OnEnable()
    {
        // ResetHitPlayers(); // Uncomment if you want to reset every time the hitbox GameObject is enabled
    }

    // 선택 사항: 히트박스가 비활성화될 때마다 ResetHitPlayers를 자동으로 호출 (안전 장치)
    private void OnDisable()
    {
        ResetHitPlayers();
    }
}