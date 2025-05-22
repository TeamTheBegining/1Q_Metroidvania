using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// 공격의 한 단계를 정의하는 Serializable 구조체
// 인스펙터에서 편집 가능하도록 [Serializable] 어트리뷰트 추가
[Serializable]
public struct AttackPhase
{
    [Tooltip("이 공격 단계에 사용할 애니메이션 트리거 이름. (예: B_Attack1, B_Attack2, B_Attack3)")]
    public string animationTriggerName;
    [Tooltip("이 공격 단계에서 활성화할 공격 판정(히트박스) 번호. (1: 방패, 2: 내려찍기, 3: 돌진)")]
    public int hitboxIndex;
    [Tooltip("이 공격 단계 애니메이션이 끝난 후, 다음 공격 단계까지 대기할 시간(초).")]
    public float delayAfterThisPhase;
    [Tooltip("이 공격 단계가 플레이어에게 줄 피해량.")]
    public float damageValue;
}

// 하나의 전체 공격 시퀀스(콤보)를 정의하는 Serializable 구조체
[Serializable]
public class AttackSequence
{
    [Tooltip("이 공격 시퀀스의 이름. 기획자가 식별하기 쉽게 작성하세요. (예: '방패 연타 콤보', '내려찍기 연속기')")]
    public string sequenceName;
    [Tooltip("이 공격 시퀀스를 구성하는 각 단계들.")]
    public AttackPhase[] phases;
    [Tooltip("이 시퀀스가 시작되기 위한 최소 플레이어 거리. (0으로 설정하면 거리 조건 무시)")]
    public float minPlayerDistance;
    [Tooltip("이 시퀀스가 시작되기 위한 최대 플레이어 거리. (0으로 설정하면 거리 조건 무시)")]
    public float maxPlayerDistance;
    [Tooltip("이 시퀀스가 선택될 확률. (0.0 ~ 1.0 사이 값, 0.5는 50% 확률)")]
    [Range(0f, 1f)]
    public float selectionChance;
    [Tooltip("이 공격 시퀀스 전체가 끝난 후, 다음 새로운 시퀀스 선택까지 대기할 쿨타임(초).")]
    public float cooldownAfterSequence;
    [Tooltip("이 시퀀스가 선택되기 위한 최소 체력 비율 (0.0 ~ 1.0). 0이면 무시.")]
    [Range(0f, 1f)]
    public float minHpRatio;
    [Tooltip("이 시퀀스가 선택되기 위한 최대 체력 비율 (0.0 ~ 1.0). 0이면 무시.")]
    [Range(0f, 1f)]
    public float maxHpRatio;
}


public class B_GirlController : CommonEnemyController
{
    // 미니보스 캐릭터 고유의 애니메이션 파라미터 이름 (애니메이터 설정과 일치해야 함)
    private const string ANIM_BOOL_B_WALK = "B_Walk";
    private const string ANIM_TRIGGER_B_JUMP = "B_Jump";
    private const string ANIM_TRIGGER_B_ATTACK1 = "B_Attack1"; // 방패 공격 (기존 잽)
    private const string ANIM_TRIGGER_B_ATT2 = "B_Attack2";    // 내려찍기 (기존 강펀치)
    private const string ANIM_TRIGGER_B_ATTACK3 = "B_Attack3"; // 회오리 돌진
    private const string ANIM_TRIGGER_B_DEATH = "B_Death";

    [Header("미니보스 공격 판정(히트박스)")] // Header 이름 변경
    [Tooltip("방패 공격에 사용될 히트박스 오브젝트를 연결하세요.")]
    public GameObject attack1HitboxObject;
    [Tooltip("내려찍기 공격에 사용될 히트박스 오브젝트를 연결하세요.")]
    public GameObject attack2HitboxObject;
    [Tooltip("돌진 공격에 사용될 히트박스 오브젝트를 연결하세요.")]
    public GameObject attack3HitboxObject;

    // 히트박스 컴포넌트 참조 (내부용, 기획자에게는 노출 불필요)
    private BoxCollider2D attack1HitboxCollider;
    private BoxCollider2D attack2HitboxCollider;
    private BoxCollider2D attack3HitboxCollider;
    private EnemyHitbox attack1EnemyHitbox;
    private EnemyHitbox attack2EnemyHitbox;
    private EnemyHitbox attack3EnemyHitbox;

    // --- 새로운 공격 패턴 시스템 변수 ---
    [Header("미니보스 공격 패턴 설정")] // Header 이름 변경
    [Tooltip("미니보스가 사용할 공격 시퀀스(콤보) 목록입니다. '+' 버튼으로 추가/제거하세요.")]
    public List<AttackSequence> attackPatterns; // 인스펙터에서 설정할 공격 시퀀스 목록
    private AttackSequence _currentAttackSequence; // 현재 진행 중인 공격 시퀀스 (내부용)
    private int _currentPhaseIndex = -1; // 현재 진행 중인 시퀀스의 페이즈 인덱스 (내부용)
    private Coroutine _attackSequenceCoroutine; // 공격 시퀀스를 관리하는 코루틴 (내부용)

    [Header("미니보스 회오리 돌진 이동 설정")] // Header 이름 변경
    [Tooltip("회오리 돌진 시 이동하는 속도.")]
    public float attack3DashSpeed = 5f;
    [Tooltip("회오리 돌진이 지속되는 시간(초).")]
    public float attack3DashDuration = 0.5f;
    private Coroutine attack3MoveCoroutine; // (내부용)

    // 슈퍼 아머를 위한 추가 변수 (내부용)
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    [Header("슈퍼 아머 피격 시 시각 효과")] // Header 이름 변경
    [Tooltip("슈퍼 아머 상태에서 피해를 입었을 때 잠시 변하는 색상.")]
    public Color hurtColor = Color.red;
    [Tooltip("피해 색상이 유지되는 시간(초).")]
    public float hurtColorDuration = 0.2f;
    private Coroutine hurtColorCoroutine; // (내부용)

    [Header("슈퍼 아머 설정")] // Header 이름 변경
    [Tooltip("미니보스가 슈퍼 아머 상태인지 여부. (체력 감소는 되지만 경직되지 않음)")]
    public bool isSuperArmored { get; private set; } = true; // 기본적으로 항상 슈퍼 아머 적용

    // Base 클래스의 Awake 오버라이드
    protected override void Awake()
    {
        base.Awake(); // CommonEnemyController의 Awake 호출

        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // 자식 오브젝트에서 SpriteRenderer 찾기
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: SpriteRenderer 컴포넌트를 찾을 수 없습니다. 피격 색상 변경이 동작하지 않습니다.", this);
        }

        // 히트박스 컴포넌트 초기화
        if (attack1HitboxObject != null)
        {
            attack1HitboxCollider = attack1HitboxObject.GetComponent<BoxCollider2D>();
            attack1EnemyHitbox = attack1HitboxObject.GetComponent<EnemyHitbox>();
            attack1HitboxObject.SetActive(true); // 오브젝트는 활성화
            attack1HitboxCollider.enabled = false; // 콜라이더만 비활성화
        }
        if (attack2HitboxObject != null)
        {
            attack2HitboxCollider = attack2HitboxObject.GetComponent<BoxCollider2D>();
            attack2EnemyHitbox = attack2HitboxObject.GetComponent<EnemyHitbox>();
            attack2HitboxObject.SetActive(true); // 오브젝트는 활성화
            attack2HitboxCollider.enabled = false; // 콜라이더만 비활성화
        }
        if (attack3HitboxObject != null)
        {
            attack3HitboxCollider = attack3HitboxObject.GetComponent<BoxCollider2D>();
            attack3EnemyHitbox = attack3HitboxObject.GetComponent<EnemyHitbox>();
            attack3HitboxObject.SetActive(true); // 오브젝트는 활성화
            attack3HitboxCollider.enabled = false; // 콜라이더만 비활성화
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
            Debug.Log($"미니보스: Start()에서 플레이어 '{playerGameObject.name}'를 찾았습니다.");
        }
        else
        {
            Debug.LogWarning("미니보스: 씬에서 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다. 미니보스가 플레이어를 추적하지 못할 수 있습니다.");
        }
        SetSuperArmor(true);
    }

    // Base 클래스의 TakeDamage 오버라이드
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return;

        Debug.Log($"[미니보스] TakeDamage called. isSuperArmored: {isSuperArmored}, damage: {damage}");

        if (isSuperArmored)
        {
            CurrentHp -= damage;
            Debug.Log($"[미니보스] 슈퍼 아머 활성! {damage} 피해를 입었습니다. 현재 HP: {CurrentHp:F2}");

            if (CurrentHp <= 0 && !IsDead)
            {
                Debug.Log($"[미니보스] 슈퍼 아머 상태에서 사망!");
                HandleDeathLogic();
                GetComponent<EnemyStatusBridge>()?.MarkAsDead();
                OnDead?.Invoke(); // 0522 추가
            }
            else
            {
                IndicateDamage();
            }
        }
        else
        {
            base.TakeDamage(damage, attackObject);
            if (CurrentHp > 0)
            {
                IndicateDamage();
            }
        }
    }

    protected void IndicateDamage()
    {
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

    // --- 애니메이션 재생 메서드 오버라이드 ---
    protected override void PlayIdleAnim()
    {
        if (animator != null && !isPerformingHurtAnimation && !isPerformingAttackAnimation)
            animator.SetBool(ANIM_BOOL_B_WALK, false);
    }

    protected override void PlayWalkAnim()
    {
        if (animator != null && !isPerformingHurtAnimation && !isPerformingAttackAnimation)
            animator.SetBool(ANIM_BOOL_B_WALK, true);
    }

    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_B_DEATH);
    }

    protected override void PlayHurtAnim()
    {
        if (animator != null && !isSuperArmored)
        {
            Debug.Log("미니보스: 피격 애니메이션 재생 (임시 처리)");
            PlayIdleAnim();
        }
    }

    // 애니메이션 이벤트에서 호출될 메서드 (트리거용)
    public void OnAttack1() { /* PlayAttack1Anim(); */ } // 직접 애니메이션 호출 불필요
    public void OnAttack2() { /* PlayAttack2Anim(); */ }
    public void OnAttack3() { /* PlayAttack3Anim(); */ }

    // 애니메이션 이벤트에서 호출할 히트박스 활성화/비활성화 메서드들 (파라미터 없는 버전)
    public void EnableAttack1Hitbox() { SetActiveHitbox(1, true); }
    public void DisableAttack1Hitbox() { SetActiveHitbox(1, false); }

    public void EnableAttack2Hitbox() { SetActiveHitbox(2, true); }
    public void DisableAttack2Hitbox() { SetActiveHitbox(2, false); }

    public void EnableAttack3Hitbox() { SetActiveHitbox(3, true); }
    public void DisableAttack3Hitbox() { SetActiveHitbox(3, false); }

    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd();
        Debug.Log("미니보스: 공격 애니메이션 종료 이벤트 발생.");
        // 애니메이션 종료 시 모든 히트박스 비활성화 (안전 장치)
        SetActiveHitbox(1, false);
        SetActiveHitbox(2, false);
        SetActiveHitbox(3, false);
    }

    // 히트박스 활성화/비활성화 메서드 (애니메이션 이벤트용)
    public void SetActiveHitbox(int hitboxIndex, bool isActive)
    {
        if (IsDead) return;

        GameObject targetHitboxObject = null;
        BoxCollider2D targetCollider = null;
        EnemyHitbox targetEnemyHitbox = null;

        switch (hitboxIndex)
        {
            case 1:
                targetHitboxObject = attack1HitboxObject;
                targetCollider = attack1HitboxCollider;
                targetEnemyHitbox = attack1EnemyHitbox;
                break;
            case 2:
                targetHitboxObject = attack2HitboxObject;
                targetCollider = attack2HitboxCollider;
                targetEnemyHitbox = attack2EnemyHitbox;
                break;
            case 3:
                targetHitboxObject = attack3HitboxObject;
                targetCollider = attack3HitboxCollider;
                targetEnemyHitbox = attack3EnemyHitbox;
                break;
            default:
                Debug.LogWarning($"유효하지 않은 공격 판정(히트박스) 번호: {hitboxIndex}");
                return;
        }

        if (targetHitboxObject != null && targetCollider != null && targetEnemyHitbox != null)
        {
            targetCollider.enabled = isActive;
            Debug.Log($"공격 판정(히트박스) {hitboxIndex}: {isActive}");

            if (isActive)
            {
                // 히트박스 초기화 및 데미지 설정 로직
                if (targetEnemyHitbox != null)
                {
                    // 현재 공격 단계의 데미지 값 설정
                    if (_currentAttackSequence != null && _currentPhaseIndex >= 0 && _currentPhaseIndex < _currentAttackSequence.phases.Length)
                    {
                        AttackPhase currentPhase = _currentAttackSequence.phases[_currentPhaseIndex];
                        // 현재 활성화하려는 히트박스 인덱스와 AttackPhase에 정의된 인덱스가 일치하는지 확인
                        if (currentPhase.hitboxIndex == hitboxIndex)
                        {
                            targetEnemyHitbox.attackDamage = currentPhase.damageValue;
                            Debug.Log($"히트박스 {hitboxIndex}의 데미지 값을 {currentPhase.damageValue}로 설정했습니다.");
                        }
                    }

                    // ResetHitPlayers() 메서드가 EnemyHitbox 스크립트에 있다면 호출
                    // 이 메서드는 해당 히트박스가 이미 공격한 플레이어를 초기화하여,
                    // 같은 공격 애니메이션 중에도 여러 번 피해를 줄 수 있도록 할 때 필요합니다.
                    targetEnemyHitbox.ResetHitPlayers();
                }
            }
        }
    }

    // EnemyHitbox에 ResetHitPlayers 메서드가 없다면 추가해주세요 (혹은 CommonEnemyController에)
    // 이 메서드는 각 히트박스가 이미 공격한 대상을 초기화하는 역할을 합니다.
    public void ResetHitPlayers()
    {
        if (attack1EnemyHitbox != null) attack1EnemyHitbox.ResetHitPlayers();
        if (attack2EnemyHitbox != null) attack2EnemyHitbox.ResetHitPlayers();
        if (attack3EnemyHitbox != null) attack3EnemyHitbox.ResetHitPlayers();
    }

    // --- 미니보스 고유의 공격 로직 재구성 ---
    protected override void PerformAttackLogic()
    {
        // 사망, 피격 애니메이션 중, 공격 애니메이션/대기 중에는 공격 로직 수행 안함
        if (IsDead || isPerformingHurtAnimation || isWaitingAfterAttack)
        {
            return;
        }

        // 현재 공격 시퀀스가 진행 중이라면, 다음 페이즈로 진행 (ExecuteAttackSequence 코루틴이 여전히 동작 중인지 확인)
        if (_currentAttackSequence != null && isPerformingAttackAnimation)
        {
            // 이미 코루틴이 돌고 있으므로, 추가적인 StartCoroutine 호출은 필요 없습니다.
            // ExecuteAttackSequence 코루틴 내부에서 다음 페이즈로 자동 진행됩니다.
            return;
        }

        // 다음 공격 쿨타임이 아직 안 지났으면 대기
        if (Time.time < nextAttackTime)
        {
            return;
        }

        // 새로운 공격 시퀀스 선택
        SelectAndStartNewAttackSequence();
    }

    // 새로운 공격 시퀀스를 선택하고 시작하는 메서드
    private void SelectAndStartNewAttackSequence()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        // 현재 체력 비율 계산 (CurrentHp는 CommonEnemyController에 정의되어 있을 것으로 예상)
        float currentHpRatio = MaxHp > 0 ? CurrentHp / MaxHp : 0f; // MaxHealth가 0인 경우 방지

        List<AttackSequence> availableSequences = new List<AttackSequence>();
        foreach (var seq in attackPatterns)
        {
            // 거리 조건 확인 (min/maxPlayerDistance가 0이면 무시)
            bool distanceMet = (seq.minPlayerDistance == 0 || distanceToPlayer >= seq.minPlayerDistance) &&
                               (seq.maxPlayerDistance == 0 || distanceToPlayer <= seq.maxPlayerDistance);

            // 체력 조건 확인 (min/maxHpRatio가 0이면 무시. 0으로 설정하면 해당 조건 무시)
            bool hpMet = (seq.minHpRatio == 0 || currentHpRatio >= seq.minHpRatio) &&
                         (seq.maxHpRatio == 0 || currentHpRatio <= seq.maxHpRatio);

            // maxHpRatio가 0이 아닌 경우에만 체력 상한선 조건 검사 (0이면 무시)
            if (seq.maxHpRatio > 0 && currentHpRatio > seq.maxHpRatio)
            {
                hpMet = false; // 체력 비율이 최대 체력 비율을 초과하면 해당 시퀀스 제외
            }
            // minHpRatio가 0이 아닌 경우에만 체력 하한선 조건 검사 (0이면 무시)
            if (seq.minHpRatio > 0 && currentHpRatio < seq.minHpRatio)
            {
                hpMet = false; // 체력 비율이 최소 체력 비율보다 낮으면 해당 시퀀스 제외
            }
            // minHpRatio와 maxHpRatio가 모두 0이면 항상 true (조건 무시)
            if (seq.minHpRatio == 0 && seq.maxHpRatio == 0)
            {
                hpMet = true;
            }


            if (distanceMet && hpMet) // 거리 조건과 체력 조건 모두 만족
            {
                // 확률에 따라 추가
                if (Random.Range(0f, 1f) < seq.selectionChance)
                {
                    availableSequences.Add(seq);
                }
            }
        }

        if (availableSequences.Count > 0)
        {
            _currentAttackSequence = availableSequences[Random.Range(0, availableSequences.Count)];
            _currentPhaseIndex = 0;
            if (_attackSequenceCoroutine != null) StopCoroutine(_attackSequenceCoroutine);
            _attackSequenceCoroutine = StartCoroutine(ExecuteAttackSequence());
            Debug.Log($"[미니보스] 새로운 공격 시퀀스 시작: {_currentAttackSequence.sequenceName}");
        }
        else
        {
            // 적절한 시퀀스를 찾지 못했을 때의 로직 (예: 잠시 대기, 플레이어에게 이동 등)
            Debug.Log("[미니보스] 적절한 공격 시퀀스를 찾을 수 없습니다. 대기 중...");
            nextAttackTime = Time.time + 1f; // 잠시 대기 후 다시 시도
        }
    }

    // 현재 공격 시퀀스를 실행하는 코루틴
    private IEnumerator ExecuteAttackSequence()
    {
        isPerformingAttackAnimation = true; // 공격 시퀀스 중으로 간주

        for (_currentPhaseIndex = 0; _currentPhaseIndex < _currentAttackSequence.phases.Length; _currentPhaseIndex++)
        {
            if (IsDead) yield break; // 사망 시 즉시 종료

            AttackPhase currentPhase = _currentAttackSequence.phases[_currentPhaseIndex];
            Debug.Log($"[미니보스] 시퀀스 '{_currentAttackSequence.sequenceName}' - 단계 {_currentPhaseIndex + 1}: {currentPhase.animationTriggerName}");

            // 플레이어 방향으로 회전 (돌진과 같이 특정 공격 전에는 필요할 수 있음)
            if (playerTransform != null)
            {
                float directionX = playerTransform.position.x - transform.position.x;
                if (directionX > 0) { Flip(false); }
                else if (directionX < 0) { Flip(true); }
            }

            // 애니메이션 트리거
            ResetAttackTriggers(); // 이전 트리거 초기화
            animator.SetTrigger(currentPhase.animationTriggerName);

            // 히트박스 데미지 값은 SetActiveHitbox 메서드가 활성화될 때 설정합니다.
            // 여기서는 히트박스를 직접 활성화하지 않고 애니메이션 이벤트에 맡깁니다.
            if (GetEnemyHitbox(currentPhase.hitboxIndex) == null)
            {
                Debug.LogWarning($"공격 판정(히트박스) 번호 {currentPhase.hitboxIndex} 에 해당하는 EnemyHitbox가 없습니다.");
            }

            // Attack3(돌진) 특별 처리
            if (currentPhase.animationTriggerName == ANIM_TRIGGER_B_ATTACK3)
            {
                if (attack3MoveCoroutine != null) StopCoroutine(attack3MoveCoroutine);
                attack3MoveCoroutine = StartCoroutine(MoveDuringAttack3(attack3DashDuration));
            }

            // 애니메이션이 끝날 때까지 대기 (OnAttackAnimationEnd 이벤트에 의존)
            // isPerformingAttackAnimation 플래그는 OnAttackAnimationEnd() 이벤트에서 false로 변경됩니다.
            float phaseStartTime = Time.time;
            isPerformingAttackAnimation = true; // 애니메이션 재생 시작 시 다시 true로 설정 (중요)

            while (isPerformingAttackAnimation && !IsDead)
            {
                // 특정 시간 이상 애니메이션이 끝나지 않으면 타임아웃 처리 (오류 방지)
                if (Time.time - phaseStartTime > 5f) // 최대 5초 대기
                {
                    Debug.LogWarning($"[미니보스] {currentPhase.animationTriggerName} 애니메이션이 시간 초과되었습니다. 강제 종료.");
                    isPerformingAttackAnimation = false;
                    break;
                }
                yield return null;
            }

            // 다음 페이즈까지의 딜레이
            yield return new WaitForSeconds(currentPhase.delayAfterThisPhase);

            // 다음 페이즈가 있다면 isPerformingAttackAnimation을 다시 true로 설정 (연속 공격을 위함)
            if (_currentPhaseIndex < _currentAttackSequence.phases.Length - 1)
            {
                isPerformingAttackAnimation = true;
            }
        }

        // 시퀀스 완료 후 처리
        Debug.Log($"[미니보스] 공격 시퀀스 '{_currentAttackSequence.sequenceName}' 완료.");
        nextAttackTime = Time.time + _currentAttackSequence.cooldownAfterSequence; // 시퀀스 전체 쿨타임
        _currentAttackSequence = null; // 시퀀스 종료
        _currentPhaseIndex = -1;
        isPerformingAttackAnimation = false; // 공격 시퀀스 완전히 종료

        StartCoroutine(PostAttackWaitCoroutine(0.5f));
    }

    // EnemyHitbox를 인덱스로 가져오는 헬퍼 함수
    private EnemyHitbox GetEnemyHitbox(int index)
    {
        switch (index)
        {
            case 1: return attack1EnemyHitbox;
            case 2: return attack2EnemyHitbox;
            case 3: return attack3EnemyHitbox;
            default: return null;
        }
    }

    // ===== B_Attack3 돌진 관련 함수 =====
    // 이 함수들은 더 이상 애니메이션 이벤트에서 직접 호출되지 않고, ExecuteAttackSequence 코루틴을 통해 관리됩니다.
    // 하지만 public으로 남겨두어 혹시 모를 외부 호출이나 디버깅에 활용할 수 있습니다.
    public void StartAttack3Movement()
    {
        Debug.LogWarning("StartAttack3Movement는 더 이상 애니메이션 이벤트에서 직접 호출되지 않습니다. ExecuteAttackSequence 코루틴을 통해 관리됩니다.");
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
            Debug.LogWarning("미니보스: 플레이어 오브젝트를 찾을 수 없습니다. 돌진 이동이 불가능합니다.");
            yield break;
        }

        Vector2 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0; // y축 이동은 무시
        if (direction.x != 0)
        {
            Flip(direction.x < 0);
        }

        if (rb == null)
        {
            Debug.LogError("미니보스: Rigidbody2D 컴포넌트를 찾을 수 없습니다! 돌진 이동이 동작하지 않습니다.");
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
    // **미니보스 뒤집기 로직 오버라이드**
    // 미니보스의 스프라이트 방향이 일반적인 몬스터와 반대일 경우 사용
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
        // 미니보스의 경우, localScale.x가 1일 때 왼쪽을 바라본다고 가정합니다. (일반적인 몬스터와 반대)
        if (faceLeft) // 왼쪽을 바라보고 싶다면
        {
            desiredSign = 1f; // 미니보스는 localScale.x가 1일 때 왼쪽을 바라봅니다.
        }
        else // 오른쪽을 바라보고 싶다면
        {
            desiredSign = -1f; // 미니보스는 localScale.x가 -1일 때 오른쪽을 바라봅니다.
        }

        float currentMagnitude = Mathf.Abs(spriteToFlip.localScale.x);
        spriteToFlip.localScale = new Vector3(desiredSign * currentMagnitude, spriteToFlip.localScale.y, spriteToFlip.localScale.z);
    }

    /// <summary>
    /// 슈퍼 아머 상태를 설정합니다.
    /// </summary>
    /// <param name="value">슈퍼 아머 활성화 여부 (true: 활성화, false: 비활성화)</param>
    public void SetSuperArmor(bool value)
    {
        isSuperArmored = value;
        Debug.Log($"[미니보스] 슈퍼 아머 상태: {isSuperArmored}");
    }
}