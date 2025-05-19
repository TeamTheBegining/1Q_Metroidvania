using System;
using UnityEngine;
using System.Collections;

// B_Girl 캐릭터의 고유 동작 및 패턴을 관리하는 컨트롤러 (Derived Class)
// Inherits from CommonEnemyController.
// IDamageable interface implementation is handled by the Base class.
public class B_GirlController : CommonEnemyController
{
    // B_Girl 캐릭터 고유의 애니메이션 파라미터 이름 (Animator 설정과 일치해야 함)
    private const string ANIM_BOOL_B_WALK = "B_Walk"; // 걷기 애니메이션 Bool 파라미터
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump"; // 점프 애니메이션 트리거 (점프 애니메이션이 있다면 사용)
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1"; // 공격 1 애니메이션 트리거
    private const string ANIM_TRIGGER_B_ATT2 = "B_Attack2"; // 공격 2 애니메이션 트리거
    private const string ANIM_TRIGGER_B_ATTACK3 = "B_Attack3"; // 공격 3 (회오리) 애니메이션 트리거
    private const string ANIM_TRIGGER_B_DEATH = "B_Death"; // 사망 애니메이션 트리거
    // private const string ANIM_TRIGGER_B_HURT = "B_Hurt"; // 피격 애니메이션 트리거 이름 (삭제 또는 주석 처리)

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject; // 인스펙터에서 할당 (공격 1 히트박스 오브젝트)
    public GameObject attack2HitboxObject; // 인스펙터에서 할당 (공격 2 히트박스 오브젝트)
    public GameObject attack3HitboxObject; // 인스펙터에서 할당 (공격 3 히트박스 오브젝트)

    // 히트박스 컴포넌트 참조
    private BoxCollider2D attack1HitboxCollider;
    private BoxCollider2D attack2HitboxCollider;
    private BoxCollider2D attack3HitboxCollider; // Attack3 추가
    private EnemyHitbox attack1EnemyHitbox; // EnemyHitbox 스크립트 필요
    private EnemyHitbox attack2EnemyHitbox; // EnemyHitbox 스크립트 필요
    private EnemyHitbox attack3EnemyHitbox; // Attack3 추가


    [Header("B_Girl Combat")]
    public float attack1Cooldown = 3f; // 공격 1 쿨타임
    public float attack2Cooldown = 4f; // 공격 2 쿨타임
    public float attack3Cooldown = 5f; // 공격 3 쿨타임 (새로운 공격)

    private float nextAttackTime = 0f; // 다음 공격 가능 시간

    [Header("B_Girl Attack Values")]
    public float attack1Value = 1.5f; // 공격 1 데미지 값
    public float attack2Value = 2.5f; // 공격 2 데미지 값
    public float attack3Value = 3.5f; // 공격 3 데미지 값 (새로운 공격)

    [Header("B_Girl Attack3 Movement")]
    public float attack3DashSpeed = 5f; // 공격 3 (회오리) 돌진 속도
    public float attack3DashDuration = 0.5f; // 공격 3 (회오리) 돌진 지속 시간
    private Coroutine attack3MoveCoroutine; // 공격 3 이동 코루틴 참조

    // B_Girl 고유의 공격 패턴 관리 변수
    private int nextAttackIndex = 1; // 다음에 실행할 공격 (1: Attack1, 2: Attack2, 3: Attack3)

    // 슈퍼 아머를 위한 추가 변수
    private SpriteRenderer spriteRenderer; // 스프라이트 렌더러 참조
    private Color originalColor; // 원래 색상
    [Header("Super Armor Visuals")]
    public Color hurtColor = Color.red; // 피격 시 변경될 색상
    public float hurtColorDuration = 0.2f; // 피격 색상이 유지될 시간
    private Coroutine hurtColorCoroutine; // 색상 변경 코루틴 참조

    // 체력 및 사망 관련 변수는 Base 클래스 (currentHealth, maxHealth, isDead, OnDead)를 사용합니다.


    // Base 클래스의 TakeDamage 오버라이드 (IDamageable 구현)
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용

        base.TakeDamage(damage, attackObject); // Base 클래스에서 체력 감소 및 사망 처리

        // 체력이 남아있다면 피격 색상 변경
        if (CurrentHp > 0) // currentHealth 대신 CurrentHp 속성 사용
        {
            IndicateDamage(); // 피격 애니메이션 대신 색상 변경 함수 호출
        }
    }

    // 피격 시 색상 변경 처리 함수
    protected void IndicateDamage()
    {
        if (spriteRenderer == null) return;

        // 기존에 실행 중인 색상 변경 코루틴이 있다면 중지
        if (hurtColorCoroutine != null)
        {
            StopCoroutine(hurtColorCoroutine);
        }

        spriteRenderer.color = hurtColor; // 스프라이트 색상을 피격 색상으로 변경
        hurtColorCoroutine = StartCoroutine(RevertColorCoroutine(hurtColorDuration));
    }

    // 일정 시간 후 원래 색상으로 되돌리는 코루틴
    IEnumerator RevertColorCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (spriteRenderer != null && !IsDead) // 죽지 않았다면 원래 색상으로 복귀 (isDead 대신 IsDead 속성 사용)
        {
            spriteRenderer.color = originalColor;
        }
    }


    // Base 클래스의 Start 오버라이드
    protected override void Start()
    {
        base.Start(); // Base CommonEnemyController Start 호출 (Animator, Player 참조 설정, 초기 상태 설정 등)
        // CommonEnemyController의 Awake에서 Rigidbody2D와 PlayerTransform을 초기화하므로,
        // 여기서는 B_GirlController 고유의 추가 초기화만 수행합니다.

        // SpriteRenderer 컴포넌트 참조 및 원래 색상 저장
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("B_GirlController: SpriteRenderer 컴포넌트를 찾을 수 없습니다! 피격 시 색상 변경이 동작하지 않습니다.", this);
        }

        // --- 기존 히트박스 초기화 ---
        if (attack1HitboxObject != null)
        {
            attack1HitboxCollider = attack1HitboxObject.GetComponent<BoxCollider2D>();
            attack1EnemyHitbox = attack1HitboxObject.GetComponent<EnemyHitbox>();
            if (attack1HitboxCollider != null)
            {
                attack1HitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("Attack 1 Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (attack1EnemyHitbox == null)
            {
                Debug.LogWarning("Attack 1 Hitbox Object에 EnemyHitbox 컴포넌트가 할당되지 않았습니다!", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack 1 Hitbox Object가 인스펙터에 할당되지 않았습니다.", this);
        }

        if (attack2HitboxObject != null)
        {
            attack2HitboxCollider = attack2HitboxObject.GetComponent<BoxCollider2D>();
            attack2EnemyHitbox = attack2HitboxObject.GetComponent<EnemyHitbox>();
            if (attack2HitboxCollider != null)
            {
                attack2HitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("Attack 2 Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (attack2EnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack 2 Hitbox Object!", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack 2 Hitbox Object가 인스펙터에 할당되지 않았습니다.", this);
        }

        // --- Attack 3 히트박스 초기화 추가 ---
        if (attack3HitboxObject != null)
        {
            attack3HitboxCollider = attack3HitboxObject.GetComponent<BoxCollider2D>();
            attack3EnemyHitbox = attack3HitboxObject.GetComponent<EnemyHitbox>();
            if (attack3HitboxCollider != null)
            {
                attack3HitboxCollider.enabled = false; // 시작 시 콜라이더 비활성화
            }
            else
            {
                Debug.LogWarning("Attack 3 Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (attack3EnemyHitbox == null)
            {
                Debug.LogWarning("EnemyHitbox component not found on Attack 3 Hitbox Object!", this);
            }
        }
        else
        {
            Debug.LogWarning("Attack 3 Hitbox Object가 인스펙터에 할당되지 않았습니다.", this);
        }


        nextAttackTime = Time.time;
        nextAttackIndex = 1;
    }

    // Update는 Base 클래스의 것을 사용합니다.


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드 오버라이드) =====
    protected override void PlayIdleAnim()
    {
        // isPerformingHurtAnimation 체크는 Base CommonEnemyController.Update()에서 처리되므로,
        // 슈퍼 아머 상태에서는 isPerformingHurtAnimation이 false가 유지되어 자연스럽게 행동을 이어갑니다.
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetBool(ANIM_BOOL_B_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetBool(ANIM_BOOL_B_WALK, true);
    }

    protected override void PlayJumpAnim()
    {
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetTrigger(ANIM_TRIGGER_B_JUMP);
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
        // 사망 시 색상 원상 복구 및 코루틴 중지
        if (hurtColorCoroutine != null)
        {
            StopCoroutine(hurtColorCoroutine);
            hurtColorCoroutine = null;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // 피격 애니메이션 재생 함수 (이제 색상 변경으로 대체되므로 내용이 사라짐)
    // protected void PlayHurtAnim() { /* 이 함수는 이제 호출되지 않음 */ }

    // Base 클래스의 PlayAttack1Anim 오버라이드
    protected override void PlayAttack1Anim()
    {
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK1);
    }

    // Base 클래스의 PlayAttack2Anim 오버라이드
    protected override void PlayAttack2Anim()
    {
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetTrigger(ANIM_TRIGGER_B_ATT2);
    }

    // Attack 3 애니메이션 재생 함수 (새로 추가)
    protected override void PlayAttack3Anim()
    {
        if (!IsDead && animator != null) // isDead 대신 IsDead 속성 사용
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK3);
    }

    // Base 클래스의 ResetAttackTriggers 오버라이드
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
            animator.ResetTrigger(ANIM_TRIGGER_B_ATT2);
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK3); // Attack3 추가
            // B_Hurt 트리거는 삭제했으므로 리셋할 필요 없음
        }
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====
    protected override void PerformAttackLogic()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        isPerformingAttackAnimation = true; // 공격 애니메이션 중임을 플래그로 표시

        // 순환식 공격 로직 (Attack1 -> Attack2 -> Attack3 -> Attack1 ...)
        if (nextAttackIndex == 1)
        {
            PlayAttack1Anim();
            nextAttackIndex = 2;
        }
        else if (nextAttackIndex == 2)
        {
            PlayAttack2Anim();
            nextAttackIndex = 3;
        }
        else // nextAttackIndex == 3
        {
            PlayAttack3Anim();
            nextAttackIndex = 1;
        }
    }

    // ===== Animation Event Callbacks =====

    // CommonEnemyController의 OnAttackAnimationEnd가 public virtual로 변경되었으므로,
    // 여기도 public override로 변경해야 합니다.
    public override void OnAttackAnimationEnd() // <-- public override로 변경됨
    {
        base.OnAttackAnimationEnd(); // isPerformingAttackAnimation = false 설정, 공격 후 일시 정지 코루틴 시작

        float cooldownToApply;
        // 다음에 실행할 공격이 무엇이었는지에 따라 쿨타임 적용
        if (nextAttackIndex == 2) // 방금 Attack 1이 끝난 경우 (다음 공격은 Attack 2)
        {
            cooldownToApply = attack1Cooldown;
        }
        else if (nextAttackIndex == 3) // 방금 Attack 2가 끝난 경우 (다음 공격은 Attack 3)
        {
            cooldownToApply = attack2Cooldown;
        }
        else // 방금 Attack 3이 끝난 경우 (다음 공격은 Attack 1)
        {
            cooldownToApply = attack3Cooldown;
        }
        nextAttackTime = Time.time + cooldownToApply;
    }

    // 공격 1 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack1Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack1HitboxObject != null && attack1HitboxCollider != null)
        {
            if (attack1EnemyHitbox != null)
            {
                attack1EnemyHitbox.attackDamage = attack1Value;
            }
            attack1HitboxCollider.enabled = true; // 콜라이더 활성화
        }
    }

    // 공격 1 히트박스 비활성화 (Animation Event에서 호출)
    public void DisableAttack1Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack1HitboxCollider != null)
        {
            attack1HitboxCollider.enabled = false; // 콜라이더 비활성화
        }
    }

    // 공격 2 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack2Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack2HitboxObject != null && attack2HitboxCollider != null)
        {
            if (attack2EnemyHitbox != null)
            {
                attack2EnemyHitbox.attackDamage = attack2Value;
            }
            attack2HitboxCollider.enabled = true; // 콜라이더 활성화
        }
    }

    // 공격 2 히트박스 비활성화 (Animation Event에서 호출)
    public void DisableAttack2Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack2HitboxCollider != null)
        {
            attack2HitboxCollider.enabled = false; // 콜라이더 비활성화
        }
    }

    // 공격 3 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack3Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack3HitboxObject != null && attack3HitboxCollider != null)
        {
            if (attack3EnemyHitbox != null)
            {
                attack3EnemyHitbox.attackDamage = attack3Value;
            }
            attack3HitboxCollider.enabled = true; // 콜라이더 활성화
        }
    }

    // 공격 3 히트박스 비활성화 (Animation Event에서 호출)
    public void DisableAttack3Hitbox()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        if (attack3HitboxCollider != null)
        {
            attack3HitboxCollider.enabled = false; // 콜라이더 비활성화
        }
    }

    // ===== B_Attack3 돌진 관련 함수 (새로 추가) =====

    // B_Attack3 애니메이션 시작 시 호출 (Animation Event)
    public void StartAttack3Movement()
    {
        if (IsDead) return; // isDead 대신 IsDead 속성 사용
        // 기존 이동 코루틴이 있다면 중지
        if (attack3MoveCoroutine != null)
        {
            StopCoroutine(attack3MoveCoroutine);
        }
        attack3MoveCoroutine = StartCoroutine(MoveDuringAttack3(attack3DashDuration));
    }

    // B_Attack3 애니메이션 종료 직전/직후 호출 (Animation Event)
    public void StopAttack3Movement()
    {
        if (attack3MoveCoroutine != null)
        {
            StopCoroutine(attack3MoveCoroutine);
            attack3MoveCoroutine = null;
        }
        // 필요하다면 리지드바디 속도 초기화
        if (rb != null && !IsDead) // 사망 시에는 건드리지 않음 (isDead 대신 IsDead 속성 사용)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // X축 속도만 0으로, Y축 속도는 유지
        }
    }

    // 공격 중 이동 코루틴
    IEnumerator MoveDuringAttack3(float duration)
    {
        float timer = 0f;
        // playerTransform이 Base 클래스에서 playerTransform으로 참조되어 있다고 가정
        if (playerTransform == null)
        {
            Debug.LogWarning("B_GirlController: Player Transform이 할당되지 않았습니다. Attack3 이동이 불가능합니다.");
            yield break;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized; // 플레이어 방향

        // 2D 게임에서 X축 이동만 고려 (수직 이동은 점프 등으로 처리)
        direction.y = 0;
        if (direction.x != 0)
        {
            // CommonEnemyController의 Flip 함수를 사용하여 플레이어 방향으로 캐릭터 뒤집기
            Flip(direction.x < 0);
        }

        // rb (Rigidbody2D)가 Base 클래스에서 초기화되어 있다고 가정
        if (rb == null)
        {
            Debug.LogError("B_GirlController: Rigidbody2D 컴포넌트를 찾을 수 없습니다! Attack3 이동이 동작하지 않습니다.");
            yield break;
        }

        while (timer < duration)
        {
            if (IsDead) // isDead 대신 IsDead 속성 사용
            {
                if (rb != null) rb.linearVelocity = Vector2.zero; // 사망 시 즉시 멈춤
                yield break;
            }

            // 몬스터의 이동 속도를 사용하여 부드럽게 이동
            Vector2 targetVelocity = direction * attack3DashSpeed;
            rb.linearVelocity = new Vector2(targetVelocity.x, rb.linearVelocity.y); // y축 속도 유지 (중력 등)

            timer += Time.deltaTime;
            yield return null;
        }

        // 이동 종료 시 속도 0으로 설정
        if (rb != null && !IsDead) // isDead 대신 IsDead 속성 사용
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // X축 속도만 0으로, Y축 속도는 유지
        }
    }
}