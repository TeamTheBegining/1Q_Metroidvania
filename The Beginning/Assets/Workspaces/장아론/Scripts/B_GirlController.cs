using UnityEngine;
using System.Collections;

// B_Girl 캐릭터의 고유 동작 및 패턴을 관리하는 컨트롤러 (Derived Class)
// CommonEnemyController를 상속받습니다.
public class B_GirlController : CommonEnemyController
{
    // B_Girl 캐릭터 고유의 애니메이션 파라미터 이름 (Inspector에 설정된 이름과 일치해야 함)
    // CommonEnemyController의 공통 파라미터 이름과 다를 수 있습니다.
    private const string ANIM_BOOL_B_WALK = "B_Walk";
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump";
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1";
    private const string ANIM_TRIGGER_B_ATTACK2 = "B_Attack2";
    private const string ANIM_TRIGGER_B_DEATH = "B_Death";

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject; // Inspector에서 할당
    public GameObject attack2HitboxObject; // Inspector에서 할당

    // B_Girl 캐릭터의 스탯
    /*[Header("B_Girl Stats")]
    public float attackDamage = 15f; // <--- 여기에 B_Girl의 공격력 설정*/

    // B_Girl의 공격 패턴 관리를 위한 변수
    private int nextAttackIndex = 1; // 다음에 실행할 공격 (1: Attack1, 2: Attack2)

    // B_Girl의 개별 공격 쿨타임
    [Header("B_Girl Attack")]
    public float attack1Cooldown = 3f; // Attack1 쿨타임
    public float attack2Cooldown = 4f; // Attack2 쿨타임
    private float lastAttack1Time = -Mathf.Infinity; // 마지막 Attack1 발동 시간
    private float lastAttack2Time = -Mathf.Infinity; // 마지막 Attack2 발동 시간

    protected override void Start()
    {
        base.Start(); // CommonEnemyController의 Start 호출 (Animator, Player 참조 설정 등)

        // B_Girl 고유의 초기 설정
        // 초기 쿨타임 설정 (게임 시작 시 바로 공격 가능하도록)
        lastAttack1Time = Time.time - attack1Cooldown;
        lastAttack2Time = Time.time - attack2Cooldown;
        nextAttackIndex = 1; // 순서는 Attack1부터 시작

        // 시작 시 모든 히트박스 비활성화
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(false);
        }
        if (attack2HitboxObject != null)
        {
            attack2HitboxObject.SetActive(false);
        }

    }

    // Update는 Base 클래스의 것을 사용하되, 필요하면 override 후 base.Update() 호출 가능
    // protected override void Update() { base.Update(); }


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드를 오버라이드하여 실제 애니메이션 재생) =====

    protected override void PlayIdleAnim()
    {
        // animator.SetBool(ANIM_BOOL_B_WALK, false); // SetState에서 이미 처리
    }

    protected override void PlayWalkAnim()
    {
        // animator.SetBool(ANIM_BOOL_B_WALK, true); // SetState에서 이미 처리
    }

    protected override void PlayJumpAnim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_JUMP);
    }

    protected override void PlayDeathAnim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
    }

    protected override void PlayAttack1Anim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_ATTACK1);
        // Note: Hitbox activation/deactivation will be handled by Animation Events
    }

    protected override void PlayAttack2Anim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_ATTACK2);
        // Note: Hitbox activation/deactivation will be handled by Animation Events
    }

    protected override void ResetAttackTriggers()
    {
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK2);
        // B_Attack3 트리거도 있다면 추가
        // animator.ResetTrigger("B_Attack3");
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====
    // B_Girl의 교차 공격 패턴 및 개별 쿨타임 로직 구현

    protected override void PerformAttackLogic()
    {
        // 이미 공격 애니메이션 중이라면 중복 발동 안함 (Base 클래스 Update에서 스킵되지만 안전 장치)
        if (isPerformingAttackAnimation) return;

        float cooldownEndTime = (nextAttackIndex == 1) ? (lastAttack1Time + attack1Cooldown) : (lastAttack2Time + attack2Cooldown);

        // Debug.Log("B_Girl PerformAttackLogic - 다음 공격: " + nextAttackIndex + ", Ready at: " + cooldownEndTime.ToString("F2") + ", 현재 Time: " + Time.time.ToString("F2") + ", 쿨타임 완료 여부: " + (Time.time >= cooldownEndTime)); // 디버그

        if (Time.time < cooldownEndTime)
        {
            // Debug.Log("B_Girl 공격 쿨타임 대기 중. 남은 시간: " + (cooldownEndTime - Time.time).ToString("F2")); // 디버그
            return; // 쿨타임 지나지 않았으면 대기
        }

        // 쿨타임이 지났다면 공격 실행
        Debug.Log("B_Girl 공격 쿨타임 완료! 다음 공격: B_Attack" + nextAttackIndex + " 발동 시도.");

        isPerformingAttackAnimation = true; // <-- 공격 애니메이션 시작 전 위치 고정 플래그 켬 (Base Class 변수 사용)

        if (nextAttackIndex == 1)
        {
            Debug.Log("--> B_Attack1 발동!");
            PlayAttack1Anim(); // B_Attack1 애니메이션 발동
            lastAttack1Time = Time.time; // Attack 1 마지막 발동 시간 기록
            nextAttackIndex = 2; // 다음 공격은 Attack 2
        }
        else // nextAttackIndex == 2
        {
            Debug.Log("--> B_Attack2 발동!");
            PlayAttack2Anim(); // B_Attack2 애니메이션 발동
            lastAttack2Time = Time.time; // Attack 2 마지막 발동 시간 기록
            nextAttackIndex = 1; // 다음 공격은 Attack 1
        }
    }

    // ===== Animation Event Callbacks =====

    // 애니메이션 이벤트에서 호출될 함수: 히트박스 활성화
    // attackObject 매개변수는 Animation Event에서 어떤 GameObject를 활성화할지 지정합니다.
    public void EnableHitbox()
    {
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(true);
            Debug.Log(attack1HitboxObject.name + " 활성화됨"); // 디버그
        }
    }

    // 애니메이션 이벤트에서 호출될 함수: 히트박스 비활성화
    public void DisableHitbox()
    {
        if (attack1HitboxObject != null)
        {
            attack1HitboxObject.SetActive(false);
            Debug.Log(attack1HitboxObject.name + " 비활성화됨"); // 디버그
        }
    }


    // TODO: Attack1.에서 플레이어가 벗어나면 Attack2로 공격하게 하는 로직 구현 (요구사항 중 하나)
    // 이 로직은 약간 복잡하며, Attack 상태에서 Chase로 전환되는 기본 로직을 재정의해야 할 수 있습니다.
    // 예를 들어, Update 함수나 SetState 함수를 오버라이드하여 거리 체크 후 특정 조건을 추가하거나,
    // Attack 애니메이션 도중 거리를 지속적으로 체크하는 별도의 코루틴 또는 로직이 필요할 수 있습니다.
    // 현재는 플레이어가 공격 범위 벗어나면 바로 Chase 상태로 전환됩니다.
    // 이 특정 요구사항은 Base 클래스의 AI 로직을 상당히 변경해야 하므로,
    // 우선 기본 교차 공격 패턴이 잘 작동하는지 확인 후 다시 논의하는 것을 추천합니다.
}