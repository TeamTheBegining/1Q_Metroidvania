using UnityEngine;
using System.Collections;

// B_Girl 캐릭터의 고유 동작 및 패턴을 관리하는 컨트롤러 (Derived Class)
// CommonEnemyController를 상속받습니다.
public class B_GirlController : CommonEnemyController , IDamageable
{
    // B_Girl 캐릭터 고유의 애니메이션 파라미터 이름 (Inspector에 설정된 이름과 일치해야 함)
    // CommonEnemyController의 공통 파라미터 이름과 다를 수 있습니다.
    private const string ANIM_BOOL_B_WALK = "B_Walk";
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump"; // 점프 트리거가 있다면 사용
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1";
    private const string ANIM_TRIGGER_B_ATTACK2 = "B_Attack2";
    private const string ANIM_TRIGGER_B_DEATH = "B_Death";

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject; // Inspector에서 할당 (오브젝트 참조 유지)
    public GameObject attack2HitboxObject; // Inspector에서 할당 (오브젝트 참조 유지)

    // --- 새로 추가: 히트박스 오브젝트에 붙어있는 BoxCollider2D 컴포넌트 참조 ---
    private BoxCollider2D attack1HitboxCollider;
    private BoxCollider2D attack2HitboxCollider;
    // ----------------------------------------------------------


    // B_Girl 캐릭터의 스탯 (CommonEnemyController에 //Debug.Log가 정의되어 있으므로 여기서 다시 선언하지 않습니다.)
    /*[Header("B_Girl Stats")]
    public float //Debug.Log = 15f; // <--- 여기에 B_Girl의 공격력 설정*/

    // B_Girl의 공격 패턴 관리를 위한 변수
    private int nextAttackIndex = 1; // 다음에 실행할 공격 (1: Attack1, 2: Attack2)

    // B_Girl의 개별 공격 쿨타임
    [Header("B_Girl Attack")]
    public float attack1Cooldown = 3f; // Attack1 쿨타임 (이 시간만큼 다음 공격까지 대기)
    public float attack2Cooldown = 4f; // Attack2 쿨타임 (이 시간만큼 다음 공격까지 대기)

    // --- 수정: 쿨타임 체크를 위한 변수 변경 ---
    // private float lastAttack1Time = -Mathf.Infinity; // 삭제
    // private float lastAttack2Time = -Mathf.Infinity; // 삭제
    private float nextAttackTime = 0f; // 다음 공격이 가능해지는 시간
    // ------------------------------------------

    //Damageable 인터페이스 구현  
    private float maxhp = 30f; // 최대 체력   
    [SerializeField]private float currentHp = 30f; // 현재 체력   
    public float CurrentHp { get=>currentHp; set=>currentHp = value; }
    public float MaxHp { get => maxhp; set=> maxhp = value; }
    public bool IsDead { get => CurrentHp <=0; }
    public void TakeDamage(float damage)
    {
        if(!IsDead)
            currentHp -= damage; // 체력 감소   
    }

    protected override void Start()
    {
        base.Start(); // CommonEnemyController의 Start 호출 (Animator, Player 참조 설정 등)

        // --- 수정: 히트박스 오브젝트에서 BoxCollider2D 컴포넌트 참조 얻기 ---
        if (attack1HitboxObject != null)
        {
            attack1HitboxCollider = attack1HitboxObject.GetComponent<BoxCollider2D>();
            if (attack1HitboxCollider != null)
            {
                attack1HitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("Attack 1 Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack 1 Hitbox Object가 B_GirlController 인스펙터에 할당되지 않았습니다.", this);
        }


        if (attack2HitboxObject != null)
        {
            attack2HitboxCollider = attack2HitboxObject.GetComponent<BoxCollider2D>();
            if (attack2HitboxCollider != null)
            {
                attack2HitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("Attack 2 Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack 2 Hitbox Object가 B_GirlController 인스펙터에 할당되지 않았습니다.", this);
        }
        // ----------------------------------------------------------


        // B_Girl 고유의 초기 설정
        // 초기 쿨타임 설정 -> 게임 시작 시 바로 공격 가능하게 nextAttackTime 초기화
        nextAttackTime = Time.time; // 또는 0f;
        nextAttackIndex = 1; // 순서는 Attack1부터 시작

        // 이전의 GameObject.SetActive(false) 대신 BoxCollider2D.enabled = false 사용
        // 이 로직은 위에서 BoxCollider2D 참조를 얻은 후 바로 수행했습니다.
    }

    // Update는 Base 클래스의 것을 사용하되, 필요하면 override 후 base.Update() 호출 가능
    // protected override void Update() { base.Update(); }


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드를 오버라이드하여 실제 애니메이션 재생) =====

    protected override void PlayIdleAnim()
    {
        animator.SetBool(ANIM_BOOL_B_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        animator.SetBool(ANIM_BOOL_B_WALK, true);
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
    }

    protected override void PlayAttack2Anim()
    {
        animator.SetTrigger(ANIM_TRIGGER_B_ATTACK2);
    }

    protected override void ResetAttackTriggers()
    {
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
        animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK2);
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====

    protected override void PerformAttackLogic()
    {
        // 이미 공격 애니메이션 중이라면 중복 발동 안함 (Base 클래스 Update에서 스킵되지만 안전 장치)
        if (isPerformingAttackAnimation) return;

        // --- 수정: 다음 공격 가능 시간 체크 ---
        // lastAttackTime 변수와 cooldownEndTime 계산 로직 삭제
        if (Time.time < nextAttackTime)
        {
            // Debug.Log("B_Girl 공격 쿨타임 대기 중. 남은 시간: " + (nextAttackTime - Time.time).ToString("F2")); // 대기 중 디버그 (선택 사항)
            return; // 다음 공격 가능 시간 전이면 대기
        }
        // --- 쿨타임 체크 로직 수정 끝 ---


        // 쿨타임이 지났다면 공격 실행
        Debug.Log("B_Girl 공격 가능! 다음 공격: B_Attack" + nextAttackIndex + " 발동 시도."); // 로그 수정

        isPerformingAttackAnimation = true; // <-- 공격 애니메이션 시작 전 위치 고정 플래그 켬 (Base Class 변수 사용)

        if (nextAttackIndex == 1)
        {
            Debug.Log("--> B_Attack1 발동!");
            PlayAttack1Anim(); // B_Attack1 애니메이션 발동
            // nextAttackTime 설정은 OnAttackAnimationEnd에서 수행
            nextAttackIndex = 2; // 다음 공격은 Attack 2
        }
        else // nextAttackIndex == 2
        {
            Debug.Log("--> B_Attack2 발동!");
            PlayAttack2Anim(); // B_Attack2 애니메이션 발동
            // nextAttackTime 설정은 OnAttackAnimationEnd에서 수행
            nextAttackIndex = 1; // 다음 공격은 Attack 1
        }
    }

    // ===== Animation Event Callbacks (BoxCollider2D 제어 방식) =====

    // --- 수정: CommonEnemyController의 OnAttackAnimationEnd 메소드를 오버라이드 ---
    protected override void OnAttackAnimationEnd()
    {
        // 기본 클래스의 로직 호출 (isPerformingAttackAnimation = false 설정)
        base.OnAttackAnimationEnd();
        Debug.Log("B_Girl 공격 애니메이션 종료! 다음 공격 가능 시간 계산."); // 로그 추가

        // 애니메이션이 끝났을 때, 다음 공격이 가능해지는 시간을 계산하여 설정
        // 방금 끝난 애니메이션의 인덱스를 알아야 함 (nextAttackIndex를 활용)
        // nextAttackIndex가 2이면 Attack 1이 방금 끝난 것, 1이면 Attack 2가 방금 끝난 것.
        if (nextAttackIndex == 2) // Attack 1이 방금 끝남
        {
            nextAttackTime = Time.time + attack1Cooldown; // Attack 1 쿨타임 적용
            Debug.Log("--> Attack 1 종료. 다음 공격은 " + attack1Cooldown.ToString("F2") + "초 후 (" + nextAttackTime.ToString("F2") + "에 가능).");
        }
        else // nextAttackIndex == 1 (Attack 2가 방금 끝남)
        {
            nextAttackTime = Time.time + attack2Cooldown; // Attack 2 쿨타임 적용
            Debug.Log("--> Attack 2 종료. 다음 공격은 " + attack2Cooldown.ToString("F2") + "초 후 (" + nextAttackTime.ToString("F2") + "에 가능).");
        }

        // TODO: 애니메이션 종료 후 바로 Chase 상태로 전환되어야 하는 경우 처리
        // 예: 플레이어가 공격 범위 밖으로 벗어났는지 여기서 다시 체크하고 SetState(EnemyState.Chase);
        // 이 부분은 AI 로직 요구사항에 따라 추가해야 합니다.
    }
    // ----------------------------------------------------------

    // --- Attack 1 히트박스 (Collider) 활성화/비활성 메소드 (유지) ---
    public void EnableAttack1Hitbox() // <-- 매개변수 없음
    {
        if (attack1HitboxCollider != null)
        {
            attack1HitboxCollider.enabled = true; // 콜라이더 활성화
            Debug.Log(attack1HitboxObject.name + " Collider 활성화됨"); // 디버그 로그 수정
        }
        else
        {
            Debug.LogWarning("Attack 1 Hitbox Collider가 할당되지 않았거나 컴포넌트를 찾을 수 없습니다.", this);
        }
    }

    public void DisableAttack1Hitbox() // <-- 매개변수 없음
    {
        if (attack1HitboxCollider != null)
        {
            attack1HitboxCollider.enabled = false; // 콜라이더 비활성화
            Debug.Log(attack1HitboxObject.name + " Collider 비활성화됨"); // 디버그 로그 수정
        }
        else
        {
            Debug.LogWarning("Attack 1 Hitbox Collider가 할당되지 않았거나 컴포넌트를 찾을 수 없습니다.", this);
        }
    }
    // ----------------------------------------------------------

    // --- Attack 2 히트박스 (Collider) 활성화/비활성 메소드 (유지) ---
    public void EnableAttack2Hitbox() // <-- 매개변수 없음
    {
        if (attack2HitboxCollider != null)
        {
            attack2HitboxCollider.enabled = true; // 콜라이더 활성화
            Debug.Log(attack2HitboxObject.name + " Collider 활성화됨"); // 디버그 로그 수정
        }
        else
        {
            Debug.LogWarning("Attack 2 Hitbox Collider가 할당되지 않았거나 컴포넌트를 찾을 수 없습니다.", this);
        }
    }

    public void DisableAttack2Hitbox() // <-- 매개변수 없음
    {
        if (attack2HitboxCollider != null)
        {
            attack2HitboxCollider.enabled = false; // 콜라이더 비활성화
            Debug.Log(attack2HitboxObject.name + " Collider 비활성화됨"); // 디버그 로그 수정
        }
        else
        {
            Debug.LogWarning("Attack 2 Hitbox Collider가 할당되지 않았거나 컴포넌트를 찾을 수 없습니다.", this);
        }
    }
    // ----------------------------------------------------------


    // TODO: Attack1.에서 플레이어가 벗어나면 Attack2로 공격하게 하는 로직 구현 (요구사항 중 하나)
    // 이 로직은 PerformAttackLogic 이나 Update, SetState 등을 오버라이드하여 구현해야 합니다.

}