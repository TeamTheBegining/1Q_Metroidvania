using System;
using UnityEngine;
using System.Collections;

// B_Girl 캐릭터의 고유 동작 및 패턴을 관리하는 컨트롤러 (Derived Class)
public class B_GirlController : CommonEnemyController
{
    // B_Girl 캐릭터 고유의 애니메이션 파라미터 이름 (Animator 설정과 일치해야 함)
    private const string ANIM_BOOL_B_WALK = "B_Walk";
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump";
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1";
    private const string ANIM_TRIGGER_B_ATT2 = "B_Attack2";
    private const string ANIM_TRIGGER_B_ATTACK3 = "B_Attack3";
    private const string ANIM_TRIGGER_B_DEATH = "B_Death";

    [Header("B_Girl Hitboxes")]
    public GameObject attack1HitboxObject;
    public GameObject attack2HitboxObject;
    public GameObject attack3HitboxObject;

    // 히트박스 컴포넌트 참조
    private BoxCollider2D attack1HitboxCollider;
    private BoxCollider2D attack2HitboxCollider;
    private BoxCollider2D attack3HitboxCollider;
    private EnemyHitbox attack1EnemyHitbox;
    private EnemyHitbox attack2EnemyHitbox;
    private EnemyHitbox attack3EnemyHitbox;

    [Header("B_Girl Combat Specifics")]
    // B_Girl 고유의 공격 쿨타임 (CommonEnemyController의 쿨타임과 다르게 설정 가능)
    public float attack1Cooldown = 0.8f; // 빠른 잽 공격 쿨타임 (이전 3f에서 줄임)
    // attack2Cooldown과 attack3Cooldown은 CommonEnemyController의 것을 사용합니다.

    [Header("B_Girl Attack Values")]
    public float attack1Value = 1.5f;
    public float attack2Value = 2.5f;
    public float attack3Value = 3.5f;

    [Header("B_Girl Attack3 Movement")]
    public float attack3DashSpeed = 5f;
    public float attack3DashDuration = 0.5f;
    private Coroutine attack3MoveCoroutine;

    // 콤보 리셋 시간과 콤보 연결 지연 시간은 CommonEnemyController의 것을 사용합니다.
    // private float lastAttackAttemptTime = 0f; // CommonEnemyController의 lastAttackAttemptTime을 사용합니다.
    // private float nextAttackTime = 0f; // CommonEnemyController의 nextAttackTime을 사용합니다.

    //new add
    [Header("B_Girl Custom Combo Delays")] // B_Girl 고유의 엇박 패턴을 위한 지연 시간
    public float heavyPunchFirstDelay = 0.6f;  // "퉁퉁 퉁~" 첫 번째 '퉁' 후 다음 '퉁'까지의 지연
    public float heavyPunchSecondDelay = 0.8f; // "퉁퉁 퉁~" 두 번째 '퉁' 후 마지막 '퉁'까지의 지연

    public float offBeatFirstDelay = 0.7f;     // 엇박 콤보 첫 번째 공격 후 다음 공격까지의 지연 (길게)
    public float offBeatSecondDelay = 0.3f;    // 엇박 콤보 두 번째 공격 후 다음 공격까지의 지연 (짧게)


    // 슈퍼 아머를 위한 추가 변수
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    [Header("Super Armor Visuals")]
    public Color hurtColor = Color.red;
    public float hurtColorDuration = 0.2f;
    private Coroutine hurtColorCoroutine;

    // 슈퍼 아머 상태를 나타내는 변수 추가
    [Header("Super Armor Settings")]
    public bool isSuperArmored { get; private set; } = true; // 기본적으로 항상 슈퍼 아머 적용

    // Base 클래스의 TakeDamage 오버라이드
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        Debug.Log($"[B_Girl] TakeDamage called. isSuperArmored: {isSuperArmored}, damage: {damage}");

        if (isSuperArmored)
        {
            // 슈퍼 아머 활성화 시: 데미지는 받지만, 일반적인 피격 반응(경직, 넉백 등)은 무시
            // CommonEnemyController의 CurrentHp 속성이 protected 이상으로 노출되어 있다고 가정합니다.
            CurrentHp -= damage; // 직접 HP 감소

            Debug.Log($"[B_Girl] Super Armor Active! Took {damage} damage. Current HP: {CurrentHp:F2}");

            if (CurrentHp <= 0 && !IsDead) // 이미 죽은 상태가 아닐 때만 처리하도록 !IsDead 추가
            {
                Debug.Log($"[B_Girl] DIED while super armored!");
                HandleDeathLogic(); // CommonEnemyController의 죽음 처리 메서드 호출

                // --- 여기부터 추가된 내용입니다 ---
                // EnemyStatusBridge 컴포넌트를 찾아 MarkAsDead() 메서드 호출
                // '?.' (null-conditional operator)를 사용하여 컴포넌트가 없어도 오류가 발생하지 않도록 합니다.
                GetComponent<EnemyStatusBridge>()?.MarkAsDead();
                // --- 여기까지 추가된 내용입니다 ---

                OnDead?.Invoke(); // 선택 사항: 죽음 이벤트를 외부에 알릴 필요가 있다면 추가
            }
            else
            {
                IndicateDamage(); // 피격 시 색상 변경은 유지
            }

            // 슈퍼 아머 상태에서는 base.TakeDamage()를 호출하지 않아 경직/넉백 등 일반적인 피격 반응을 방지합니다.
        }
        else
        {
            // 슈퍼 아머 비활성화 시: 기본 TakeDamage 로직 수행 (경직, 넉백 포함)
            base.TakeDamage(damage, attackObject);

            if (CurrentHp > 0)
            {
                IndicateDamage();
            }
        }
    }

    protected void IndicateDamage()
    {
        Debug.Log($"[B_Girl] IndicateDamage called. spriteRenderer is null: {spriteRenderer == null}"); // 이 줄 추가
        if (spriteRenderer == null) return;

        if (hurtColorCoroutine != null)
        {
            StopCoroutine(hurtColorCoroutine);
        }

        spriteRenderer.color = hurtColor;
        hurtColorCoroutine = StartCoroutine(RevertColorCoroutine(hurtColorDuration));
    }

    IEnumerator RevertColorCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (spriteRenderer != null && !IsDead)
        {
            spriteRenderer.color = originalColor;
        }
    }

    // Base 클래스의 Start 오버라이드
    protected override void Start()
    {
        base.Start();

        GameObject playerGameObject = GameObject.FindWithTag("Player");

        if (playerGameObject != null)
        {
            SetPlayerTarget(playerGameObject.transform);
            Debug.Log($"B_GirlController: Start()에서 플레이어 '{playerGameObject.name}'를 찾았습니다.", this);
        }
        else
        {
            Debug.LogWarning("B_GirlController: Start()에서 'Player' 태그를 가진 게임 오브젝트를 찾을 수 없습니다! 플레이어가 씬에 있는지, 태그가 올바른지 확인하세요.", this);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("B_GirlController: SpriteRenderer 컴포넌트를 찾을 수 없습니다! 피격 시 색상 변경이 동작하지 않습니다.", this);
        }

        // --- 히트박스 초기화 ---
        InitializeHitbox(attack1HitboxObject, ref attack1HitboxCollider, ref attack1EnemyHitbox, "Attack 1");
        InitializeHitbox(attack2HitboxObject, ref attack2HitboxCollider, ref attack2EnemyHitbox, "Attack 2");
        InitializeHitbox(attack3HitboxObject, ref attack3HitboxCollider, ref attack3EnemyHitbox, "Attack 3");

        // 시작부터 슈퍼아머 활성화
        SetSuperArmor(true);
        Debug.Log("[B_Girl] Initialized with Super Armor ENABLED by default");

        // CommonEnemyController의 nextAttackTime과 currentComboState를 사용
        // nextAttackTime = Time.time; // 이미 CommonEnemyController에서 초기화됨
        // currentComboState = ComboState.None; // 이미 CommonEnemyController에서 초기화됨
    }

    // 히트박스 초기화 도우미 함수
    private void InitializeHitbox(GameObject hitboxObj, ref BoxCollider2D collider, ref EnemyHitbox enemyHitbox, string name)
    {
        if (hitboxObj != null)
        {
            collider = hitboxObj.GetComponent<BoxCollider2D>();
            enemyHitbox = hitboxObj.GetComponent<EnemyHitbox>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            else
            {
                Debug.LogWarning($"{name} Hitbox Object에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (enemyHitbox == null)
            {
                Debug.LogWarning($"{name} Hitbox Object에 EnemyHitbox 컴포넌트가 할당되지 않았습니다!", this);
            }
        }
        else
        {
            Debug.LogWarning($"{name} Hitbox Object가 인스펙터에 할당되지 않았습니다.", this);
        }
    }


    // ===== 애니메이션 관련 함수들 (Base 클래스의 virtual 메소드 오버라이드) =====
    protected override void PlayIdleAnim()
    {
        if (!IsDead && animator != null)
            animator.SetBool(ANIM_BOOL_B_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        if (!IsDead && animator != null)
            animator.SetBool(ANIM_BOOL_B_WALK, true);
    }

    protected override void PlayJumpAnim()
    {
        if (!IsDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_JUMP);
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
        if (hurtColorCoroutine != null)
        {
            StopCoroutine(hurtColorCoroutine);
            hurtColorCoroutine = null;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        // 죽음 시 슈퍼 아머 비활성화
        SetSuperArmor(false);
    }

    protected override void PlayAttack1Anim()
    {
        if (!IsDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK1);
    }

    protected override void PlayAttack2Anim()
    {
        if (!IsDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_ATT2);
    }

    protected override void PlayAttack3Anim()
    {
        if (!IsDead && animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_ATTACK3);
    }

    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK1);
            animator.ResetTrigger(ANIM_TRIGGER_B_ATT2);
            animator.ResetTrigger(ANIM_TRIGGER_B_ATTACK3);
        }
    }

    // ===== AI 공격 로직 (Base 클래스의 virtual PerformAttackLogic 오버라이드) =====

protected override void PerformAttackLogic()
    {
        // Debug.Log($"[B_Girl] PerformAttackLogic 호출됨. 현재 콤보 상태: {currentComboState}, 다음 공격 가능 시간: {nextAttackTime:F2}, 현재 시간: {Time.time:F2}");

        // 1. 전역 쿨다운 및 콤보 리셋 시간 체크
        if (Time.time < nextAttackTime)
        {
            // Debug.Log($"[B_Girl] 공격 쿨다운 중. 남은 시간: {nextAttackTime - Time.time:F2}");
            return;
        }

        // 마지막 공격 시도 후 충분한 시간이 지났다면 콤보 상태를 초기화합니다.
        if (Time.time - lastAttackAttemptTime > comboResetTime)
        {
            Debug.Log($"[B_Girl] 콤보 리셋됨! (시간 초과: {Time.time - lastAttackAttemptTime:F2}s, 리셋 기준: {comboResetTime:F2}s)");
            currentComboState = ComboState.None; // 콤보 상태를 None으로 초기화
        }

        // 2. 새로운 패턴을 시작할지, 기존 콤보를 이어나갈지 결정
        if (currentComboState == ComboState.None) // 콤보가 진행 중이 아닐 때, 새로운 패턴 시작 결정
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // A) 패턴 1: 회오리 돌진 (Attack3) - 중거리 우선순위
            // 플레이어가 공격 범위보다 약간 멀리 있지만, 탐지 범위 안쪽에 있을 때 (중거리)
            if (distanceToPlayer > attackRange * 1.5f && distanceToPlayer <= detectionRange * 0.8f)
            {
                if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% 확률로 회오리 돌진 시도
                {
                    isPerformingAttackAnimation = true;
                    PlayAttack3Anim(); // Attack3 애니메이션 재생
                    nextAttackTime = Time.time + base.attack3Cooldown; // Base 클래스의 attack3Cooldown 적용
                    lastAttackAttemptTime = Time.time;
                    Debug.Log($"[B_Girl] (새로운 패턴) 회오리 돌진 (Attack3) 발동! 플레이어 거리: {distanceToPlayer:F2}");
                    return; // 회오리 돌진 발동 시 다른 공격은 시도하지 않고 함수 종료
                }
            }

            // B) 근접 패턴 선택 (회오리 돌진이 발동하지 않았을 경우)
            float patternRoll = UnityEngine.Random.Range(0f, 1f);

            if (patternRoll < 0.4f) // 40% 확률로 '빠른 잽 연타' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack1Anim();
                currentComboState = ComboState.QuickJab_Initial; // 새로운 잽 콤보 시작 상태
                nextAttackTime = Time.time + base.comboChainDelay; // Base 클래스의 comboChainDelay 사용
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 빠른 잽 연타 (Attack1)");
            }
            else if (patternRoll < 0.75f) // 35% 확률로 '묵직한 펀치 연타' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack2Anim(); // 첫 번째 '퉁'
                currentComboState = ComboState.HeavyAttack_Initial; // 묵직한 펀치 콤보 시작 상태
                nextAttackTime = Time.time + heavyPunchFirstDelay; // 첫 번째 퉁 후 지연 (B_Girl 고유)
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 묵직한 펀치 연타 (Attack2) - 퉁!");
            }
            else // 25% 확률로 '엇박 콤보' 시작
            {
                isPerformingAttackAnimation = true;
                PlayAttack1Anim(); // 엇박 콤보의 시작은 잽
                currentComboState = ComboState.OffBeat_Initial; // 엇박 콤보 시작 상태
                nextAttackTime = Time.time + offBeatFirstDelay; // 첫 번째 엇박 공격 후 지연 (B_Girl 고유)
                Debug.Log($"[B_Girl] (새로운 패턴) 시작: 엇박 콤보 (Attack1)");
            }
        }
        else // 현재 콤보 진행 중 (ComboState.None이 아님) - 콤보 이어나가기
        {
            isPerformingAttackAnimation = true; // 콤보 중이니 애니메이션 플래그 유지
            switch (currentComboState)
            {
                case ComboState.QuickJab_Initial: // 빠른 잽 연타: 첫 번째 잽 후
                    if (UnityEngine.Random.Range(0f, 1f) < 0.6f) // 60% 확률로 두 번째 잽 ('원원' 패턴으로 이어짐)
                    {
                        PlayAttack1Anim();
                        currentComboState = ComboState.QuickJab_Second; // 두 번째 잽 상태
                        nextAttackTime = Time.time + base.comboChainDelay;
                        Debug.Log($"[B_Girl] 콤보 진행: 빠른 잽 연타 (두 번째 Attack1)");
                    }
                    else // 40% 확률로 강타 ('원투' 패턴으로 마무리)
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.None; // 콤보 끝
                        nextAttackTime = Time.time + base.attack2Cooldown; // 콤보 마무리 후 쿨다운
                        Debug.Log($"[B_Girl] 콤보 마무리: 빠른 잽 연타 (Attack2 마무리)");
                    }
                    break;

                case ComboState.QuickJab_Second: // 빠른 잽 연타: 두 번째 잽 후 ('원원투' 패턴으로 마무리)
                    PlayAttack2Anim();
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + base.attack2Cooldown; // 콤보 마무리 후 쿨다운
                    Debug.Log($"[B_Girl] 콤보 완료: 빠른 잽 연타 (세 번째 Attack2 마무리)");
                    break;

                case ComboState.HeavyAttack_Initial: // 묵직한 펀치 연타: 첫 번째 '퉁' 후
                                                     // --- 이 부분이 수정되었습니다: 70% 확률 대신 100% 확률로 다음 콤보로 이어집니다. ---
                                                     // 이전 코드: if (UnityEngine.Random.Range(0f, 1f) < 0.7f)
                    if (UnityEngine.Random.Range(0f, 1f) < 1.0f) // 100% 확률로 두 번째 '퉁'으로 이어짐
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.HeavyAttack_Second; // 두 번째 '퉁' 상태
                        nextAttackTime = Time.time + heavyPunchSecondDelay; // 두 번째 '퉁' 후 지연 (B_Girl 고유)
                        Debug.Log($"[B_Girl] 콤보 진행: 묵직한 펀치 연타 (두 번째 Attack2) - 퉁!");
                    }
                    // 이전의 'else' 블록 (30% 확률로 콤보 마무리)은 이제 실행되지 않으므로 제거되었습니다.
                    break;

                case ComboState.HeavyAttack_Second: // 묵직한 펀치 연타: 두 번째 '퉁' 후
                    PlayAttack2Anim(); // 마지막 세 번째 '퉁'
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + base.attack2Cooldown;
                    Debug.Log($"[B_Girl] 콤보 완료: 묵직한 펀치 연타 (세 번째 Attack2) - 퉁~!");
                    break;

                case ComboState.OffBeat_Initial: // 엇박 콤보: 첫 번째 잽 후
                    if (UnityEngine.Random.Range(0f, 1f) < 0.5f) // 50% 확률로 잽으로 이어짐 (느린 박자)
                    {
                        PlayAttack1Anim();
                        currentComboState = ComboState.OffBeat_Second; // 엇박 콤보 두 번째 상태
                        nextAttackTime = Time.time + offBeatSecondDelay; // 두 번째 엇박 공격 후 지연 (B_Girl 고유)
                        Debug.Log($"[B_Girl] 콤보 진행: 엇박 콤보 (Attack1으로 이어짐 - 느린 박자)");
                    }
                    else // 50% 확률로 강타로 마무리 (빠른 박자)
                    {
                        PlayAttack2Anim();
                        currentComboState = ComboState.None; // 콤보 끝
                        nextAttackTime = Time.time + base.attack2Cooldown;
                        Debug.Log($"[B_Girl] 콤보 마무리: 엇박 콤보 (Attack2 마무리 - 빠른 박자)");
                    }
                    break;

                case ComboState.OffBeat_Second: // 엇박 콤보: 두 번째 공격 후
                    PlayAttack2Anim(); // 최종 강타로 마무리
                    currentComboState = ComboState.None; // 콤보 끝
                    nextAttackTime = Time.time + base.attack2Cooldown;
                    Debug.Log($"[B_Girl] 콤보 완료: 엇박 콤보 (최종 Attack2 마무리)");
                    break;
            }
        }
        lastAttackAttemptTime = Time.time; // 마지막 공격 시도 시간 업데이트
    }

    // CommonEnemyController의 OnAttackAnimationEnd가 public virtual로 변경되었으므로,
    // 여기도 public override로 변경해야 합니다.
    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        // 공격 애니메이션이 끝나도 슈퍼 아머는 계속 유지 (비활성화 코드 제거)
        // SetSuperArmor(false); // 이 줄을 제거하거나 주석 처리
    }

    /// <summary>
    /// 슈퍼 아머 상태를 설정합니다.
    /// </summary>
    /// <param name="value">슈퍼 아머 활성화 여부 (true: 활성화, false: 비활성화)</param>
    public void SetSuperArmor(bool value)
    {
        isSuperArmored = value;
        Debug.Log($"[B_Girl] Super Armor is now: {isSuperArmored}");
    }

    // 공격 1 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack1Hitbox()
    {
        if (IsDead) return;
        if (attack1HitboxObject != null && attack1HitboxCollider != null)
        {
            if (attack1EnemyHitbox != null)
            {
                attack1EnemyHitbox.attackDamage = attack1Value;
            }
            attack1HitboxCollider.enabled = true;
        }
        // Attack1에도 슈퍼아머 보장 (이미 기본적으로 활성화되어 있지만, 혹시 모를 경우를 대비)
        SetSuperArmor(true);
    }

    // 공격 1 히트박스 비활성화 (Animation Event에서 호출)
    public void DisableAttack1Hitbox()
    {
        if (IsDead) return;
        if (attack1HitboxCollider != null)
        {
            attack1HitboxCollider.enabled = false;
        }
        // Attack1 후에도 슈퍼아머 유지 (비활성화 코드 제거)
        // SetSuperArmor(false); // 이 줄을 제거하거나 주석 처리
    }

    // 공격 2 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack2Hitbox()
    {
        if (IsDead) return;
        if (attack2HitboxObject != null && attack2HitboxCollider != null)
        {
            if (attack2EnemyHitbox != null)
            {
                attack2EnemyHitbox.attackDamage = attack2Value;
            }
            attack2HitboxCollider.enabled = true;
        }
        // Attack2에도 슈퍼아머 보장
        SetSuperArmor(true);
    }

    // 공격 2 히트박스 비활성화 (Animation Event에서 호출)
    public void DisableAttack2Hitbox()
    {
        if (IsDead) return;
        if (attack2HitboxCollider != null)
        {
            attack2HitboxCollider.enabled = false;
        }
        // Attack2 후에도 슈퍼아머 유지 (비활성화 코드 제거)
        // SetSuperArmor(false); // 이 줄을 제거하거나 주석 처리
    }

    // 공격 3 히트박스 활성화 (Animation Event에서 호출)
    public void EnableAttack3Hitbox()
    {
        if (IsDead) return;
        if (attack3HitboxObject != null && attack3HitboxCollider != null)
        {
            if (attack3EnemyHitbox != null)
            {
                attack3EnemyHitbox.attackDamage = attack3Value;
            }
            attack3HitboxCollider.enabled = true;
        }
        // Attack3 (돌진 공격) 시에도 슈퍼 아머 활성화 (이미 기본적으로 활성화되어 있지만, 혹시 모를 경우를 대비)
        SetSuperArmor(true);
    }

    public void DisableAttack3Hitbox()
    {
        if (IsDead) return;
        if (attack3HitboxCollider != null)
        {
            attack3HitboxCollider.enabled = false;
        }
    }

    // ===== B_Attack3 돌진 관련 함수 =====
    public void StartAttack3Movement()
    {
        if (IsDead) return;
        if (attack3MoveCoroutine != null)
        {
            StopCoroutine(attack3MoveCoroutine);
        }
        attack3MoveCoroutine = StartCoroutine(MoveDuringAttack3(attack3DashDuration));
    }

    public void StopAttack3Movement()
    {
        if (attack3MoveCoroutine != null)
        {
            StopCoroutine(attack3MoveCoroutine);
            attack3MoveCoroutine = null;
        }
        if (rb != null && !IsDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    IEnumerator MoveDuringAttack3(float duration)
    {
        float timer = 0f;
        if (playerTransform == null)
        {
            Debug.LogWarning("B_GirlController: Player Transform이 할당되지 않았습니다. Attack3 이동이 불가능합니다.");
            yield break;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction.x != 0)
        {
            Flip(direction.x < 0);
        }

        if (rb == null)
        {
            Debug.LogError("B_GirlController: Rigidbody2D 컴포넌트를 찾을 수 없습니다! Attack3 이동이 동작하지 않습니다.");
            yield break;
        }

        while (timer < duration)
        {
            if (IsDead)
            {
                if (rb != null) rb.linearVelocity = Vector2.zero;
                yield break;
            }

            Vector2 targetVelocity = direction * attack3DashSpeed;
            rb.linearVelocity = new Vector2(targetVelocity.x, rb.linearVelocity.y);

            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null && !IsDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // =========================================================================
    // **B_Girl의 Flip 로직 오버라이드**
    // B_Girl의 스프라이트 방향이 일반적인 몬스터와 반대일 경우 사용
    // =========================================================================
    protected override void Flip(bool faceLeft)
    {
        Transform spriteToFlip = transform.Find("Sprite");
        if (spriteToFlip == null)
        {
            spriteToFlip = transform;
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        float desiredSign;
        // B_Girl의 경우, localScale.x가 1일 때 왼쪽을 바라본다고 가정합니다. (일반적인 몬스터와 반대)
        if (faceLeft) // 왼쪽을 바라보고 싶다면
        {
            desiredSign = 1f; // B_Girl은 localScale.x가 1일 때 왼쪽을 바라봅니다.
        }
        else // 오른쪽을 바라보고 싶다면
        {
            desiredSign = -1f; // B_Girl은 localScale.x가 -1일 때 오른쪽을 바라봅니다.
        }

        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);
        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);
    }
}