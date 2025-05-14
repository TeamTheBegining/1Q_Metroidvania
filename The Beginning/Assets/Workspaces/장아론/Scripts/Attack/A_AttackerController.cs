using System;
using UnityEngine;
using System.Collections;

// A_Attacker (잡몹 - 근거리) 캐릭터의 동작을 관리하는 컨트롤러
// CommonEnemyController를 상속받습니다.
public class A_AttackerController : CommonEnemyController, IDamageable
{
    // A_Attacker 고유의 애니메이션 파라미터 이름
    private const string ANIM_BOOL_A_WALK = "A_Walk";
    //private const string ANIM_TRIGGER_A_JUMP = "A_Jump"; // 점프 없다고 가정
    private const string ANIM_TRIGGER_A_ATTACK_A = "A_AttackA"; // A 공격 (베기)
    // --- 추가: 피격 애니메이션 트리거 ---
    private const string ANIM_TRIGGER_A_HURT = "A_Hurt"; // 피격 애니메이션
    // ------------------------------------
    private const string ANIM_TRIGGER_A_STUN = "A_Stun"; // 경직 애니메이션
    private const string ANIM_TRIGGER_A_DEATH = "A_Death"; // 사망 애니메이션 (필요시)

    [Header("A_Attacker Hitbox")]
    public GameObject attackAHitboxObject; // A 공격 히트박스 오브젝트

    // 히트박스 오브젝트에 붙어있는 컴포넌트 참조
    private BoxCollider2D attackAHitboxCollider;
    private EnemyHitbox attackAEnemyHitbox; // EnemyHitbox 스크립트 사용 가정

    // A_Attacker 스탯 (이미지 기반)
    [Header("A_Attacker Stats")]
    private float maxhp = 5f; // 최대 체력 5
    [SerializeField] private float currentHp = 5f; // 현재 체력 5
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxhp; set => maxhp = value; }
    public bool IsDead { get; private set; } = false; // 사망 플래그

    [Header("A_Attacker Combat")]
    public float attackAValue = 0.5f; // A 공격력 0.5
    public float attackACooldown = 3f; // A 공격 쿨타임 3초

    // 쿨타임 체크
    private float nextAttackTime = 0f; // 다음 공격 가능 시간

    // 상태 관리 플래그 (CommonEnemyController의 상태와 별개로 관리될 수 있는 경직 상태)
    private bool isStunned = false;
    // 경직 시간 변수 (필요시)
    private float stunDuration = 2f;
    public float StunDuration { get => stunDuration; set => stunDuration = value; }

    public Action OnDead { get; set; }

    // Damageable 인터페이스 구현
    public void TakeDamage(float damage , GameObject player)
    {
        // 이미 죽었으면 처리 안함
        if (IsDead) return;

        // 경직 중에는 데미지는 받지만, 추가 경직/피격 반응은 막음 (필요시 로직 수정)
        if (isStunned)
        {
            currentHp -= damage; // 경직 중에도 데미지는 들어감
                                 //Debug.Log("A_Attacker 경직 중 피격! 체력: " + currentHp);
            if (currentHp <= 0) Die(); // 경직 중에 맞아도 체력 0 이하면 사망
            return; // 추가 피격 애니메이션/경직 트리거 막음
        }

        // 경직 상태가 아닐 때 데미지 처리
        currentHp -= damage; // 체력 감소
        //Debug.Log("A_Attacker 피격! 체력: " + currentHp);

        // TODO: 패링 감지 로직 추가 필요 - 현재 TakeDamage 함수만으로는 패링 여부 판단 불가.
        // 패링 성공 시 호출될 외부 함수(예: PlayerCombat 스크립트)에서 Stun()을 호출하는 것이 일반적입니다.
        // 혹은 데미지 정보에 'isParryAttack' 등의 bool 값을 추가하여 전달받는 방식도 있습니다.
        // 예시: if (damageInfo.isParryAttack) { Stun(stunDuration); return; } // 패링 공격이면 스턴 걸고 함수 종료

        // 체력이 0 이하 시 사망 처리
        if (currentHp <= 0)
        {
            //Debug.Log("A_Attacker 체력 0 이하! 사망 처리 시작.");
            Die();
        }
        // 체력이 0보다 크고, 경직 상태가 아니라면 일반 피격 애니메이션 발동
        else // currentHp > 0 && !isStunned
        {
            PlayHurtAnim(); // 피격 애니메이션 재생 함수 호출
        }
    }

    // --- 추가: 경직 상태 처리 함수 ---
    // 플레이어의 패링 성공 로직 등 외부에서 호출될 수 있습니다.
    public void Stun()
    {
        // 이미 죽었거나 경직 중이면 다시 경직되지 않음
        if (IsDead || isStunned) return;

        isStunned = true;
        //Debug.Log("A_Attacker 경직 상태 진입!");

        // TODO: 경직 중 AI 로직 및 이동 중지
        // CommonEnemyController의 AI 상태를 Stun 상태로 변경하거나, Update 등에서 isStunned 체크
        // 예: SetState(EnemyState.Stunned); // CommonEnemyController에 Stunned 상태가 정의되어 있다면
        // 네비게이션 에이전트 사용 시: navMeshAgent.isStopped = true;
        // 현재 애니메이션 중이라면 중단하고 경직 애니메이션으로 전환
        if (animator != null) animator.SetTrigger(ANIM_TRIGGER_A_STUN);

        // 일정 시간 후 경직 해제 코루틴 시작
        StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    IEnumerator ReleaseStunCoroutine(float duration)
    {
        // 경직 애니메이션 길이만큼 기다린 후 추가 대기 (필요시)
        // yield return new WaitForSeconds(GetStunAnimationLength()); // 애니메이션 길이를 알면
        yield return new WaitForSeconds(duration); // 단순히 지정된 시간만큼 대기

        // 죽은 상태가 아니면 경직 해제
        if (!IsDead)
        {
            isStunned = false;
            //Debug.Log("A_Attacker 경직 상태 해제!");

            // TODO: 경직 해제 후 AI 로직 재개
            // 예: SetState(EnemyState.Chase); // CommonEnemyController의 기본 상태로 복귀
            // 네비게이션 에이전트 사용 시: navMeshAgent.isStopped = false;

            // TODO: 애니메이터 상태 전환 (경직 해제 후 Idle/Walk 등으로)
            // Animator Controller에서 Stun -> Idle/Walk 등으로 가는 Transition을 설정하는 것이 일반적입니다.
        }
    }
    // ------------------------------


    // --- 추가: 사망 처리 함수 (IDamageable 구현 일부) ---
    void Die()
    {
        if (IsDead) return; // 이미 죽음 처리 중이면 중복 방지

        IsDead = true; // 죽음 상태 플래그 설정
        //Debug.Log("A_Attacker 사망!");

        // 경직 코루틴 중지 (사망 시 경직 해제 로직이 실행되지 않도록)
        StopAllCoroutines();
        isStunned = false; // 사망했으니 경직 상태도 해제

        // 사망 애니메이션 재생
        PlayDeathAnim();

        // TODO: 사망 후 필요한 추가 로직 구현 (B_GirlController와 유사)
        // - Collider 비활성화 (피격 및 공격 판정 방지)
        Collider2D mainCollider = GetComponent<Collider2D>();
        if (mainCollider != null) mainCollider.enabled = false;

        // - Rigidbody 설정 변경 (움직임 멈춤 등)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // 움직임 즉시 정지 (Unity 2019.3+ 권장)
            // rb.velocity = Vector2.zero; // 이전 버전
            rb.isKinematic = true; // 물리 효과 비활성화 (애니메이션이 움직임을 제어할 경우)
        }

        // - AI 스크립트 비활성화 (더 이상 추적/공격 등을 하지 않도록)
        // 예: this.enabled = false; // 스크립트 전체 비활성화 (Update 등 멈춤)
        // CommonEnemyController에 AI 관련 별도 컴포넌트가 있다면 그것을 비활성화

        // TODO: 게임 매니저 등에 에너미 사망을 알리는 이벤트 호출 또는 처리
        // 예: GameManager.Instance.EnemyDied(this);

        // - 일정 시간 후 게임 오브젝트 제거 또는 오브젝트 풀 반환
        // 예: Destroy(gameObject, 3f); // 3초 후 파괴 (데스 애니메이션 길이에 맞게 조절)
    }
    // --------------------------------------------


    protected override void Start()
    {
        base.Start(); // CommonEnemyController의 Start 호출 (Animator, Player 참조 설정 등)

        // 히트박스 오브젝트 및 스크립트 참조 초기화
        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>(); // EnemyHitbox 컴포넌트 참조
            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                //Debug.LogWarning("Attack A Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                //Debug.LogWarning("Attack A Hitbox Object에 EnemyHitbox 컴포넌트가 없습니다!", this);
            }
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Object가 A_AttackerController 인스펙터에 할당되지 않았습니다.", this);
        }

        // 상태 초기화
        currentHp = maxhp; // 체력 초기화
        IsDead = false;
        isStunned = false; // 경직 상태 초기화
        nextAttackTime = Time.time; // 또는 0f; // 쿨타임 초기화
    }

    // Update 오버라이드: 죽거나 경직 상태일 때 기본 AI 로직 스킵
    protected override void Update()
    {
        if (IsDead || isStunned)
        {
            // 죽었거나 경직 중이면 AI 로직 실행 안함
            // 경직 중 애니메이션 재생 등 다른 처리가 필요하면 여기에 추가
            // 경직 애니메이션은 Stun()에서 이미 트리거하므로 여기서 특별히 할 일 없을 수 있음
            return;
        }

        base.Update(); // CommonEnemyController의 Update 호출 (이동, 상태 전환 등)
    }


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드를 오버라이드) =====

    protected override void PlayIdleAnim()
    {
        // 죽거나 경직 중이 아닐 때만 재생
        if (!IsDead && !isStunned && animator != null) animator.SetBool(ANIM_BOOL_A_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        // 죽거나 경직 중이 아닐 때만 재생
        if (!IsDead && !isStunned && animator != null) animator.SetBool(ANIM_BOOL_A_WALK, true);
    }

    protected override void PlayJumpAnim()
    {
        // A_Attacker는 점프 없다고 가정
        // base.PlayJumpAnim(); // CommonEnemyController에 점프 관련이 있다면 주석 해제
        // if (!IsDead && !isStunned && animator != null) animator.SetTrigger(ANIM_TRIGGER_A_JUMP); // 점프 트리거 있다면
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null) animator.SetTrigger(ANIM_TRIGGER_A_DEATH);
    }

    // --- 추가: 피격 애니메이션 재생 함수 ---
    // TakeDamage 함수에서 호출됩니다.
    protected void PlayHurtAnim()
    {
        // 죽거나 경직 중이 아닐 때만 재생
        if (!IsDead && !isStunned && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_HURT); // 피격 애니메이션 트리거 발동!
                                                      //Debug.Log("PlayHurtAnim 호출");
        }
    }
    // -----------------------------------

    // --- 추가: 경직 애니메이션 재생 함수 (Stun()에서 호출) ---
    // Protected로 선언하여 상속받은 클래스에서도 접근 가능하게 할 수 있습니다.
    protected void PlayStunAnim()
    {
        // 이미 죽었다면 재생 안함 (Stun()에서 이미 체크하지만 안전 장치)
        if (IsDead) return;
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_STUN);
            //Debug.Log("PlayStunAnim 호출");
        }
    }
    // ----------------------------------------------------


    // --- A 공격 애니메이션 재생 함수 오버라이드 ---
    // CommonEnemyController의 PlayAttack1Anim, PlayAttack2Anim 등 중 하나를 오버라이드합니다.
    // 여기서는 PlayAttack1Anim을 사용하여 A 공격 애니메이션을 발동시킵니다.
    protected override void PlayAttack1Anim() // CommonEnemyController에 PlayAttack1Anim 가상 함수가 있다고 가정
    {
        // 죽거나 경직 중이 아닐 때만 공격 애니메이션 재생 시도
        if (!IsDead && !isStunned && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_A_ATTACK_A);
            //Debug.Log("A_AttackA 애니메이션 발동!");
        }
    }

    // CommonEnemyController의 PlayAttack2Anim 등 다른 공격 함수는 A_Attacker에서 사용하지 않으므로 오버라이드하지 않거나 기본 구현을 둡니다.
    /*
    protected override void PlayAttack2Anim()
    {
        // A_Attacker는 Attack 2가 없으므로 비워두거나 base 호출
        // base.PlayAttack2Anim();
    }
    */


    // Attack 트리거 리셋 함수
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_A_ATTACK_A); // A 공격 트리거 리셋
            animator.ResetTrigger(ANIM_TRIGGER_A_HURT); // 피격 트리거 리셋
            animator.ResetTrigger(ANIM_TRIGGER_A_STUN); // 경직 트리거 리셋
                                                        // CommonEnemyController의 ResetAttackTriggers에 다른 트리거 리셋이 있다면 base 호출
                                                        // base.ResetAttackTriggers();
        }
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====

    protected override void PerformAttackLogic()
    {
        // 이미 공격 애니메이션 중이거나 죽거나 경직 중이라면 중복 발동 안함
        if (isPerformingAttackAnimation || IsDead || isStunned) return;

        // 쿨타임이 지났는지 체크
        if (Time.time < nextAttackTime)
        {
            // 쿨타임 대기 중
            return;
        }

        // 쿨타임이 지났다면 공격 실행
        //Debug.Log("A_Attacker 공격 가능! A_AttackA 발동 시도.");

        isPerformingAttackAnimation = true; // 공격 애니메이션 시작 플래그 켬 (Base Class 변수)

        // A 공격 애니메이션을 발동시키기 위해 PlayAttack1Anim 함수를 호출합니다.
        PlayAttack1Anim(); // <-- PlayAttackAnim() 대신 PlayAttack1Anim() 호출

        // nextAttackTime 설정은 Animation Event 콜백 함수(OnAttackAnimationEnd)에서 수행
    }

    // ===== Animation Event Callbacks (Base 클래스의 virtual 메소드 오버라이드) =====

    // --- 공격 애니메이션 종료 시 호출될 콜백 함수 오버라이드 ---
    protected override void OnAttackAnimationEnd()
    {
        // 기본 클래스의 로직 호출 (isPerformingAttackAnimation = false 설정 등)
        base.OnAttackAnimationEnd();
        //Debug.Log("A_Attacker 공격 애니메이션 종료! 다음 공격 가능 시간 계산.");

        // A 공격 쿨타임 계산 (애니메이션 종료 시점부터 시작)
        nextAttackTime = Time.time + attackACooldown;
        //Debug.Log("--> A_AttackA 종료. 다음 공격은 " + attackACooldown.ToString("F2") + "초 후 (" + nextAttackTime.ToString("F2") + "에 가능).");

        // TODO: 애니메이션 종료 후 AI 상태 전환 로직 추가 (예: Chase 상태로 돌아가기)
        // 예: SetState(EnemyState.Chase); // CommonEnemyController에 Chase 상태가 있다면
    }

    // --- A 공격 히트박스 활성화 메소드 (Animation Event에서 호출) ---
    // Animation Event에서 EnableAttack1Hitbox 또는 EnableAttack2Hitbox 등을 호출하도록 설정되어 있을 것입니다.
    // A_Attacker는 공격이 하나이므로, A 공격 애니메이션의 Animation Event에서 이 함수를 호출하도록 설정합니다.
    // 함수 이름은 기존 B_GirlController의 명칭을 따르지만, A_Attacker의 히트박스 로직을 수행합니다.
    // (만약 Animation Event에서 PlayAttack1Anim 함수처럼 EnableAttack1Hitbox를 호출하도록 설정되어 있다면 이 함수를 사용)
    public void EnableAttackAHitbox() // Animation Event 시그니처와 일치 (매개변수 없음)
    {
        // B_GirlController의 EnableAttack1Hitbox 함수를 오버라이드하는 것은 아니므로,
        // Animation Event 설정 시 함수 이름을 이 이름(EnableAttackAHitbox)으로 직접 연결해야 할 수 있습니다.
        // 혹은 CommonEnemyController에 virtual public void EnableAttackHitbox(int attackIndex) 같은 함수가 있다면
        // 해당 함수를 오버라이드하여 attackIndex에 따라 다른 로직을 수행하도록 구현할 수도 있습니다.
        // 여기서는 독립적인 public 함수로 구현합니다.

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            // --- 히트박스 오브젝트의 EnemyHitbox 스크립트에 공격력 값 설정 ---
            // 이 로직은 EnemyHitbox.cs 파일에 public float attackDamage; 변수가 있을 때 작동합니다.
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue; // EnemyHitbox의 attackDamage 변수에 값 설정 (0.5)
                //Debug.Log("Attack A Hitbox에 데미지 값 설정됨: " + attackAValue);
            }
            else
            {
                //Debug.LogWarning("Attack A Hitbox Object에 EnemyHitbox 컴포넌트가 할당되지 않았습니다!", attackAHitboxObject);
            }
            // -----------------------------------------------------------------

            attackAHitboxCollider.enabled = true; // 콜라이더 활성화
            //Debug.Log(attackAHitboxObject.name + " Collider 활성화됨");

            // TODO: 필요하다면 여기서 ResetHitFlag() 호출하여 같은 스윙 중 중복 히트 방지
            // if (attackAEnemyHitbox != null) attackAEnemyHitbox.ResetHitFlag();
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Object 또는 Collider가 할당되지 않았습니다.", this);
        }
    }

    // --- A 공격 히트박스 비활성화 메소드 (Animation Event에서 호출) ---
    // Animation Event 설정 시 함수 이름을 이 이름(DisableAttackAHitbox)으로 직접 연결해야 할 수 있습니다.
    public void DisableAttackAHitbox() // Animation Event 시그니처와 일치 (매개변수 없음)
    {
        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false; // 콜라이더 비활성화
            //Debug.Log(attackAHitboxObject.name + " Collider 비활성화됨");
        }
        else
        {
            //Debug.LogWarning("Attack A Hitbox Collider가 할당되지 않았거나 컴포넌트를 찾을 수 없습니다.", this);
        }
    }


    // TODO: 기타 필요한 AI 로직 또는 상태 전환 구현 (예: 플레이어 추적, 공격 범위 체크 등)
    // CommonEnemyController의 virtual 메소드를 오버라이드하여 구현합니다.
    // 예: protected override void CheckForPlayer() { ... }
    // 예: protected override void MoveTowardsPlayer() { ... }
}