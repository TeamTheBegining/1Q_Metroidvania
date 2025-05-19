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
    // --- 다음 콤보 상태를 추적하기 위한 enum과 변수들 ---
    private enum ComboState
    {
        None,   // 콤보 비활성 또는 콤보 완료/초기화 상태
        Jab1,   // 첫 번째 잽(Attack1) 수행 후
        Jab2,   // 두 번째 잽(Attack1) 수행 후
    }
    private ComboState currentComboState = ComboState.None;
    private float lastAttackAttemptTime = 0f; // 마지막 공격 시도 시간을 추적하여 콤보 초기화 로직에 사용

    [Header("B_Girl Combo Settings")]
    public float comboResetTime = 1.2f; // 마지막 공격 후 이 시간 동안 다음 콤보 공격이 없으면 콤보를 초기화합니다.
    public float comboChainDelay = 0.4f; // 콤보 공격 간의 짧은 지연 시간 (예: 잽 -> 잽)

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

        // =========================================================================
        // **여기에 플레이어 찾는 코드 추가**
        // GameObject.FindWithTag("Player")를 사용하여 "Player" 태그를 가진 오브젝트를 찾습니다.
        // =========================================================================
        GameObject playerGameObject = GameObject.FindWithTag("Player");

        // 찾은 오브젝트가 null이 아닌지 확인하여 오류를 방지합니다.
        if (playerGameObject != null)
        {
            // SetPlayerTarget은 CommonEnemyController에 정의된 protected 메서드로
            // playerTransform을 설정하고 관련 초기화 작업을 수행합니다.
            SetPlayerTarget(playerGameObject.transform);
            Debug.Log($"B_GirlController: Start()에서 플레이어 '{playerGameObject.name}'를 찾았습니다.", this); // B_GirlController로 로그 변경
        }
        else
        {
            Debug.LogWarning("B_GirlController: Start()에서 'Player' 태그를 가진 게임 오브젝트를 찾을 수 없습니다! 플레이어가 씬에 있는지, 태그가 올바른지 확인하세요.", this); // B_GirlController로 로그 변경
        }
        // =========================================================================
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

        nextAttackTime = Time.time; // 초기 공격 가능 시간 설정
        currentComboState = ComboState.None; // 콤보 상태 초기화
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
        // 1. 전역 쿨다운 및 콤보 리셋 시간 체크 (기존 로직 유지)
        Debug.Log($"[B_Girl] PerformAttackLogic 호출됨. 현재 콤보 상태: {currentComboState}, 다음 공격 가능 시간: {nextAttackTime:F2}, 현재 시간: {Time.time:F2}");

        if (Time.time < nextAttackTime)
        {
            Debug.Log($"[B_Girl] 공격 쿨다운 중. 남은 시간: {nextAttackTime - Time.time:F2}");
            return;
        }

        // 마지막 공격 시도 후 충분한 시간이 지났다면 콤보 상태를 초기화합니다.
        if (Time.time - lastAttackAttemptTime > comboResetTime)
        {
            Debug.Log($"[B_Girl] 콤보 리셋됨! (시간 초과: {Time.time - lastAttackAttemptTime:F2}s, 리셋 기준: {comboResetTime:F2}s)");
            currentComboState = ComboState.None;
        }

        // 2. 패턴 3: 회오리 돌진 (중거리에서 접근 및 공격 - 콤보 중이 아닐 때만 발동)
        // 현재 콤보 중이 아니고 (None 상태) 플레이어가 중거리(공격 범위 밖, 탐지 범위 안)에 있을 때만 회오리 돌진을 고려합니다.
        if (currentComboState == ComboState.None) // 콤보가 진행 중이 아닐 때만 돌진 고려
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // 플레이어가 공격 범위보다 약간 멀리 있지만, 탐지 범위 안쪽에 있을 때 (중거리)
            if (distanceToPlayer > attackRange * 1.5f && distanceToPlayer <= detectionRange * 0.8f) // *1.5f와 *0.8f는 예시, 값 조절 가능
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% 확률로 회오리 돌진 시도
                {
                    isPerformingAttackAnimation = true;
                    PlayAttack3Anim(); // Attack3 애니메이션 재생
                    nextAttackTime = Time.time + attack3Cooldown; // Attack3 쿨다운 적용
                                                                  // 회오리 돌진은 독립적인 공격이므로 콤보 상태를 None으로 유지
                    Debug.Log($"[B_Girl] 패턴 3 (회오리 돌진) 시도! 플레이어 거리: {distanceToPlayer:F2}");
                    return; // 회오리 돌진이 발동했으니 다른 공격은 시도하지 않고 함수 종료
                }
            }
        }


        // 3. 근접 공격 패턴 선택 및 실행 (회오리 돌진이 발동하지 않았을 때)
        isPerformingAttackAnimation = true; // 공격 애니메이션 시작 플래그 설정

        if (currentComboState == ComboState.None) // 새로운 근접 공격 시퀀스를 시작할 때만 패턴 선택
        {
            // 70% 확률로 패턴 1 (잽-잽-라이트 콤보), 30% 확률로 패턴 2 (단일 라이트 강타)
            if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
            {
                // 패턴 1 시작: 잽-잽-라이트 콤보의 첫 번째 잽
                PlayAttack1Anim();
                currentComboState = ComboState.Jab1; // 콤보 상태를 Jab1으로 변경
                nextAttackTime = Time.time + comboChainDelay;
                Debug.Log($"[B_Girl] 패턴 1 (콤보 시작): 첫 번째 잽 (Attack1) 시도!");
            }
            else
            {
                // 패턴 2 시작: 단일 라이트 강타
                PlayAttack2Anim(); // 바로 라이트 펀치 (Attack2) 재생
                currentComboState = ComboState.None; // 단일 공격이므로 콤보 상태 바로 초기화
                nextAttackTime = Time.time + attack2Cooldown; // 단일 강타 후 전체 쿨다운 적용
                Debug.Log($"[B_Girl] 패턴 2 (단일 강타): 라이트 펀치 (Attack2) 시도!");
            }
        }
        else // 기존 콤보가 진행 중일 때 (Jab1 또는 Jab2)
        {
            // currentComboState에 따라 콤보를 이어나갑니다.
            switch (currentComboState)
            {
                case ComboState.Jab1:
                    PlayAttack1Anim(); // 두 번째 잽
                    currentComboState = ComboState.Jab2;
                    nextAttackTime = Time.time + comboChainDelay;
                    Debug.Log($"[B_Girl] 패턴 1 (콤보 중): 두 번째 잽 (Attack1) 시도!");
                    break;

                case ComboState.Jab2:
                    PlayAttack2Anim(); // 라이트 펀치 (콤보 마무리)
                    currentComboState = ComboState.None; // 콤보 완료 후 초기화
                    nextAttackTime = Time.time + attack2Cooldown;
                    Debug.Log($"[B_Girl] 패턴 1 (콤보 완료): 라이트 펀치 (Attack2) 시도!");
                    break;
            }
        }
        lastAttackAttemptTime = Time.time;
    }

    // ===== Animation Event Callbacks =====

    // CommonEnemyController의 OnAttackAnimationEnd가 public virtual로 변경되었으므로,
    // 여기도 public override로 변경해야 합니다.
    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd(); // isPerformingAttackAnimation = false 설정, 공격 후 일시 정지 코루틴 시작
        // 다음 공격 시간 (nextAttackTime)은 이제 PerformAttackLogic()에서 직접 설정되므로,
        // 이 부분의 쿨다운 계산 로직은 더 이상 필요하지 않습니다.
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
            // B_Girl의 Flip 로직은 오버라이드되어 있으므로, 여기서는 단순히 호출하면 됩니다.
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

    // =========================================================================
    // **추가된 부분: B_Girl의 Flip 로직 오버라이드**
    // =========================================================================
    protected override void Flip(bool faceLeft)
    {
        // 'Sprite'라는 이름의 자식 오브젝트를 찾거나, 없으면 현재 오브젝트 자체를 사용합니다.
        Transform spriteToFlip = transform.Find("Sprite");
        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        float desiredSign;
        // B_Girl의 경우, localScale.x가 1일 때 왼쪽을 바라본다고 가정합니다.
        // 따라서, 왼쪽을 바라보려면 localScale.x를 1로, 오른쪽을 바라보려면 -1로 설정합니다.
        if (faceLeft) // 왼쪽을 바라보고 싶다면
        {
            desiredSign = 1f; // B_Girl은 localScale.x가 1일 때 왼쪽을 바라봅니다.
        }
        else // 오른쪽을 바라보고 싶다면
        {
            desiredSign = -1f; // B_Girl은 localScale.x가 -1일 때 오른쪽을 바라봅니다.
        }

        // 현재 스케일의 절대값을 유지하면서 방향만 바꿉니다.
        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);
        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);
    }
    // =========================================================================
}