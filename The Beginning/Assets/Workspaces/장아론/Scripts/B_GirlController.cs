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
    private const string ANIM_TRIGGER_B_ATTACK2 = "B_Attack2"; // 공격 2 애니메이션 트리거
    private const string ANIM_TRIGGER_B_DEATH = "B_Death"; // 사망 애니메이션 트리거
    private const string ANIM_TRIGGER_B_HURT = "B_Hurt"; // 피격 애니메이션 트리거 이름

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject; // 인스펙터에서 할당 (공격 1 히트박스 오브젝트)
    public GameObject attack2HitboxObject; // 인스펙터에서 할당 (공격 2 히트박스 오브젝트)

    // 히트박스 컴포넌트 참조
    private BoxCollider2D attack1HitboxCollider;
    private BoxCollider2D attack2HitboxCollider;
    private EnemyHitbox attack1EnemyHitbox; // EnemyHitbox 스크립트 필요
    private EnemyHitbox attack2EnemyHitbox; // EnemyHitbox 스크립트 필요


    [Header("B_Girl Combat")]
    public float attack1Cooldown = 3f; // 공격 1 쿨타임
    public float attack2Cooldown = 4f; // 공격 2 쿨타임

    private float nextAttackTime = 0f; // 다음 공격 가능 시간

    [Header("B_Girl Attack Values")]
    public float attack1Value = 1.5f; // 공격 1 데미지 값
    public float attack2Value = 2.5f; // 공격 2 데미지 값

    // B_Girl 고유의 공격 패턴 관리 변수
    private int nextAttackIndex = 1; // 다음에 실행할 공격 (1: Attack1, 2: Attack2)

    // 체력 및 사망 관련 변수는 Base 클래스 (currentHealth, maxHealth, isDead, OnDead)를 사용합니다.


    // Base 클래스의 TakeDamage 오버라이드 (IDamageable 구현)
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (isDead) return;
        base.TakeDamage(damage, attackObject);
        if (currentHealth > 0)
        {
            PlayHurtAnim();
        }
    }


    // Base 클래스의 Start 오버라이드
    protected override void Start()
    {
        base.Start(); // Base CommonEnemyController Start 호출 (Animator, Player 참조 설정, 초기 상태 설정 등)

        // 히트박스 오브젝트 및 컴포넌트 참조 초기화
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

        nextAttackTime = Time.time;
        nextAttackIndex = 1;
    }

    // Update는 Base 클래스의 것을 사용합니다.


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드 오버라이드) =====
    protected override void PlayIdleAnim()
    {
        if (!isDead && animator != null)
            animator.SetBool(ANIM_BOOL_B_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        if (!isDead && animator != null)
            animator.SetBool(ANIM_BOOL_B_WALK, true);
    }

    protected override void PlayJumpAnim()
    {
        if (!isDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_JUMP);
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
    }

    // 피격 애니메이션 재생
    protected void PlayHurtAnim()
    {
        if (!isDead && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_B_HURT);
        }
    }

    // Base 클래스의 PlayAttack1Anim 오버라이드
    protected override void PlayAttack1Anim()
    {
        if (!isDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK1);
    }

    // Base 클래스의 PlayAttack2Anim 오버라이드
    protected override void PlayAttack2Anim()
    {
        if (!isDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK2);
    }

    // Base 클래스의 ResetAttackTriggers 오버라이드
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK2);
            // 필요시 다른 트리거 리셋 추가
        }
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====
    protected override void PerformAttackLogic()
    {
        //Debug.Log("B_Girl PerformAttackLogic override called.");
        if (Time.time < nextAttackTime)
        {
            return;
        }

        //Debug.Log("B_Girl ready to attack! Triggering B_Attack" + nextAttackIndex);
        isPerformingAttackAnimation = true;

        if (nextAttackIndex == 1)
        {
            PlayAttack1Anim();
            nextAttackIndex = 2;
        }
        else // nextAttackIndex == 2
        {
            PlayAttack2Anim();
            nextAttackIndex = 1;
        }
    }

    // ===== Animation Event Callbacks =====

    // Base 클래스의 OnAttackAnimationEnd 오버라이드
    protected override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();

        float cooldownToApply;
        if (nextAttackIndex == 2) // 방금 Attack 1이 끝난 경우
        {
            cooldownToApply = attack1Cooldown;
        }
        else // 방금 Attack 2가 끝난 경우
        {
            cooldownToApply = attack2Cooldown;
        }
        nextAttackTime = Time.time + cooldownToApply;
    }

    //  공격 1 히트박스 활성화 (Animation Event에서 호출) 
    public void EnableAttack1Hitbox()
    {
        Debug.Log("B_Girl: EnableAttack1Hitbox called!"); //  호출 확인 로그 
        if (isDead) return;

        //  히트박스 오브젝트 및 콜라이더 참조 확인 로그 
        Debug.Log($"B_Girl: EnableAttack1Hitbox - Object null? {attack1HitboxObject == null}, Collider null? {attack1HitboxCollider == null}, EnemyHitbox null? {attack1EnemyHitbox == null}");

        if (attack1HitboxObject != null && attack1HitboxCollider != null)
        {
            if (attack1EnemyHitbox != null)
            {
                attack1EnemyHitbox.attackDamage = attack1Value;
                Debug.Log($"B_Girl: Attack 1 Hitbox damage set to {attack1Value}, enabling collider."); //  데미지 설정 및 활성화 로그 
            }
            else
            {
                Debug.LogWarning("B_Girl: EnemyHitbox component not found on Attack 1 Hitbox Object!", attack1HitboxObject);
            }
            attack1HitboxCollider.enabled = true; // 콜라이더 활성화
            Debug.Log($"B_Girl: Attack 1 Hitbox Collider enabled: {attack1HitboxCollider.enabled}"); //  활성화 결과 로그 
        }
        else
        {
            Debug.LogWarning("B_Girl: Attack 1 Hitbox Object or Collider not assigned or found.", this);
        }
    }

    //  공격 1 히트박스 비활성화 (Animation Event에서 호출) 
    public void DisableAttack1Hitbox()
    {
        Debug.Log("B_Girl: DisableAttack1Hitbox called!"); //  호출 확인 로그 
        if (isDead) return;

        //  히트박스 콜라이더 참조 확인 로그 
        Debug.Log($"B_Girl: DisableAttack1Hitbox - Collider null? {attack1HitboxCollider == null}");


        if (attack1HitboxCollider != null)
        {
            Debug.Log($"B_Girl: Attack 1 Hitbox Collider enabled state before disabling: {attack1HitboxCollider.enabled}"); //  비활성화 전 상태 로그 
            attack1HitboxCollider.enabled = false; // 콜라이더 비활성화
            Debug.Log($"B_Girl: Attack 1 Hitbox Collider enabled state after disabling: {attack1HitboxCollider.enabled}"); //  비활성화 후 상태 로그 
        }
        else
        {
            Debug.LogWarning("B_Girl: Attack 1 Hitbox Collider not assigned or found.", this); //  경고 로그 
        }
    }

    //  공격 2 히트박스 활성화 (Animation Event에서 호출) 
    public void EnableAttack2Hitbox()
    {
        Debug.Log("B_Girl: EnableAttack2Hitbox called!"); //  호출 확인 로그 
        if (isDead) return;

        //  히트박스 오브젝트 및 콜라이더 참조 확인 로그 
        Debug.Log($"B_Girl: EnableAttack2Hitbox - Object null? {attack2HitboxObject == null}, Collider null? {attack2HitboxCollider == null}, EnemyHitbox null? {attack2EnemyHitbox == null}");

        if (attack2HitboxObject != null && attack2HitboxCollider != null)
        {
            if (attack2EnemyHitbox != null)
            {
                attack2EnemyHitbox.attackDamage = attack2Value;
                Debug.Log($"B_Girl: Attack 2 Hitbox damage set to {attack2Value}, enabling collider."); //  데미지 설정 및 활성화 로그 
            }
            else
            {
                Debug.LogWarning("B_Girl: EnemyHitbox component not found on Attack 2 Hitbox Object!", attack2HitboxObject);
            }
            attack2HitboxCollider.enabled = true; // 콜라이더 활성화
            Debug.Log($"B_Girl: Attack 2 Hitbox Collider enabled: {attack2HitboxCollider.enabled}"); //  활성화 결과 로그 
        }
        else
        {
            Debug.LogWarning("B_Girl: Attack 2 Hitbox Object or Collider not assigned or found.", this); //  경고 로그 
        }
    }

    //  공격 2 히트박스 비활성화 (Animation Event에서 호출) 
    public void DisableAttack2Hitbox()
    {
        Debug.Log("B_Girl: DisableAttack2Hitbox called!"); //  호출 확인 로그 
        if (isDead) return;

        //  히트박스 콜라이더 참조 확인 로그 
        Debug.Log($"B_Girl: DisableAttack2Hitbox - Collider null? {attack2HitboxCollider == null}");

        if (attack2HitboxCollider != null)
        {
            Debug.Log($"B_Girl: Attack 2 Hitbox Collider enabled state before disabling: {attack2HitboxCollider.enabled}"); //  비활성화 전 상태 로그 
            attack2HitboxCollider.enabled = false; // 콜라이더 비활성화
            Debug.Log($"B_Girl: Attack 2 Hitbox Collider enabled state after disabling: {attack2HitboxCollider.enabled}"); //  비활성화 후 상태 로그 
        }
        else
        {
            Debug.LogWarning("B_Girl: Attack 2 Hitbox Collider not assigned or found.", this); //  경고 로그 
        }
    }

    // 피격 애니메이션 종료 시 호출될 Animation Event 메서드 오버라이드
    public override void OnHurtAnimationEnd()
    {
        Debug.Log("B_Girl OnHurtAnimationEnd called.");
        isPerformingHurtAnimation = false;
        Debug.Log("isPerformingHurtAnimation set to false in OnHurtAnimationEnd.");

        if (!isDead)
        {
            // Update()에서 자연스럽게 상태 전환
        }
    }
}