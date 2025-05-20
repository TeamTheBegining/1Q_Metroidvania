using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// 잡몹02 (근거리) 캐릭터를 제어하는 스크립트입니다.
/// 이 스크립트는 CommonEnemyController를 상속받아 기본적인 적의 행동(피해, 사망, 추적, 순찰)을 계승하며,
/// 잡몹02의 고유한 능력치, 단일 공격 패턴, 경직(패링 시) 로직을 구현합니다.
/// </summary>
public class Mob02MeleeController : CommonEnemyController
{
    // --- 애니메이션 파라미터 이름 상수 정의 ---
    // 유니티 Animator Controller에 동일한 이름의 파라미터를 생성해야 합니다.
    private const string ANIM_BOOL_MOB02_WALK = "Mob02_Walk";             // 걷기 애니메이션 재생을 위한 Bool 파라미터
    private const string ANIM_TRIGGER_MOB02_ATTACK_A = "Mob02_AttackA";   // '내지르기(일반)' 공격 애니메이션을 위한 Trigger 파라미터
    private const string ANIM_TRIGGER_MOB02_HURT = "Mob02_Hurt";         // 피격 애니메이션을 위한 Trigger 파라미터
    private const string ANIM_TRIGGER_MOB02_STUN = "Mob02_Stun";         // 경직 애니메이션을 위한 Trigger 파라미터
    private const string ANIM_TRIGGER_MOB02_DEATH = "Mob02_Death";       // 사망 애니메이션을 위한 Trigger 파라미터

    [Header("잡몹02 공격 판정 설정")]
    [Tooltip("A 공격(내지르기) 시 활성화될 히트박스 게임 오브젝트를 연결하세요.")]
    public GameObject attackAHitboxObject; // '내지르기' 공격의 히트박스 오브젝트

    // 히트박스 컴포넌트 참조
    // 이 변수들은 스크립트 시작 시 자동으로 찾아서 할당됩니다.
    private BoxCollider2D attackAHitboxCollider; // 히트박스 오브젝트에 붙어있는 BoxCollider2D
    private EnemyHitbox attackAEnemyHitbox;      // 히트박스 오브젝트에 붙어있는 EnemyHitbox 스크립트 (공격력 전달용)

    [Header("잡몹02 고유 능력치")]
    [Tooltip("잡몹02의 최대 체력입니다. CommonEnemyController의 MaxHp를 이 값으로 초기화합니다.")]
    [SerializeField] private float mob02MaxHealth = 4f; // **잡몹02의 체력: 4**

    [Header("잡몹02 전투 설정")]
    [Tooltip("'내지르기' 공격 시 적용될 피해량입니다.")]
    public float attackAValue = 0.4f; // **'내지르기' 공격력: 0.4**
    [Tooltip("공격 애니메이션 종료 후 다음 공격을 시도하기 전까지 기다릴 시간 (1초).")]
    public float attackACooldown = 1.0f; // **공격 애니메이션 종료 후 1초 텀**

    // 경직(Stun) 상태 관리 변수
    private bool isStunned = false; // 현재 경직 상태인지 나타내는 플래그
    [Header("잡몹02 경직 설정")]
    [Tooltip("플레이어의 패링 공격 등에 의해 경직되었을 때 지속되는 시간입니다.")]
    public float stunDuration = 2f; // **패링 시 경직 지속 시간**

    private Coroutine stunCoroutine; // 현재 실행 중인 경직 코루틴을 참조하여 중복 실행 방지

    /// <summary>
    /// 오브젝트가 생성될 때 가장 먼저 호출됩니다.
    /// CommonEnemyController의 Awake를 호출하고, 잡몹02의 초기 능력치를 설정합니다.
    /// </summary>
    protected override void Awake()
    {
        base.Awake(); // 부모 클래스인 CommonEnemyController의 Awake 메서드 호출

        // 잡몹02의 특정 능력치로 최대 체력과 현재 체력을 초기화합니다.
        MaxHp = mob02MaxHealth; // CommonEnemyController의 MaxHp를 4로 설정
        CurrentHp = MaxHp;      // 현재 체력도 최대 체력으로 설정
    }

    /// <summary>
    /// 스크립트가 활성화될 때 한 번 호출됩니다.
    /// 플레이어 타겟을 설정하고, 히트박스 관련 컴포넌트를 초기화합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start(); // 부모 클래스인 CommonEnemyController의 Start 메서드 호출

        // 플레이어 오브젝트를 찾아 타겟으로 설정합니다.
        // 씬에 "Player" 태그를 가진 게임 오브젝트가 있어야 합니다.
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            SetPlayerTarget(playerGameObject.transform);
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Player' 태그를 가진 게임 오브젝트를 찾을 수 없습니다. 플레이어가 씬에 있는지, 태그가 올바른지 확인하세요.", this);
        }

        // 공격 히트박스 오브젝트가 할당되었다면, 해당 컴포넌트들을 찾아 참조합니다.
        if (attackAHitboxObject != null)
        {
            attackAHitboxCollider = attackAHitboxObject.GetComponent<BoxCollider2D>();
            attackAEnemyHitbox = attackAHitboxObject.GetComponent<EnemyHitbox>();

            if (attackAHitboxCollider != null)
            {
                attackAHitboxCollider.enabled = false; // 게임 시작 시 공격 히트박스 콜라이더를 비활성화
            }
            else
            {
                Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'에 BoxCollider2D 컴포넌트가 없습니다.", this);
            }
            if (attackAEnemyHitbox == null)
            {
                Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'에 EnemyHitbox 컴포넌트가 없습니다!", this);
            }
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object'가 인스펙터에 할당되지 않았습니다. 공격 판정이 작동하지 않을 수 있습니다.", this);
        }

        // 초기 경직 상태 설정 및 다음 공격 가능 시간 초기화
        isStunned = false;
        nextAttackTime = Time.time;
    }

    /// <summary>
    /// 매 프레임 호출됩니다.
    /// 적의 현재 상태(사망, 경직, 피격 중, 공격 중)에 따라 AI 로직과 움직임을 제어합니다.
    /// </summary>
    protected override void Update()
    {
        // 1. 적이 사망했거나, 경직 상태이거나, 피격 애니메이션 중일 때는 모든 AI 로직과 움직임을 중지합니다.
        if (IsDead || isStunned || isPerformingHurtAnimation)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 물리적인 움직임(속도) 정지
            }
            return; // 이 프레임의 나머지 Update 로직을 건너뜁니다.
        }

        // 2. 공격 애니메이션 중이거나 공격 후 대기 중일 때는 움직임을 멈추고 대기 애니메이션을 재생합니다.
        // 이는 CommonEnemyController의 Update에서 처리되는 부분이기도 하지만, 명시적으로 방어적인 체크를 합니다.
        if (isPerformingAttackAnimation || isWaitingAfterAttack)
        {
            PlayIdleAnim(); // 공격/대기 중에는 대기 애니메이션을 재생
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero; // 물리적인 움직임 정지
            }
            return; // 이 프레임의 나머지 Update 로직을 건너뜁니다.
        }

        // 3. 위 예외 상태가 아니면, 부모 클래스의 일반적인 AI 로직(플레이어 추적, 이동, 공격 범위 체크 등)을 실행합니다.
        base.Update();
    }

    /// <summary>
    /// 적이 피해를 입었을 때 호출됩니다.
    /// 경직 상태일 때의 추가적인 피격 반응을 제어합니다.
    /// </summary>
    /// <param name="damage">적용할 피해량.</param>
    /// <param name="attackObject">공격을 가한 오브젝트 (예: 플레이어의 무기).</param>
    public override void TakeDamage(float damage, GameObject attackObject)
    {
        if (IsDead) return; // 이미 사망했으면 아무것도 하지 않음

        // 적이 경직 상태일 때는 피해는 입히되, 추가적인 피격 애니메이션을 건너뛸 수 있습니다.
        if (isStunned)
        {
            base.TakeDamage(damage, attackObject); // 부모 클래스에서 체력 감소 및 사망 체크 로직 실행
            return; // 경직 상태일 때는 추가적인 피격/경직 반응 스킵
        }

        // 부모 클래스의 TakeDamage 호출: 체력 감소, 체력 0 이하 시 사망 처리(`HandleDeathLogic` 호출)
        base.TakeDamage(damage, attackObject);

        // 적이 아직 죽지 않았고 경직 상태도 아닐 경우 피격 애니메이션을 재생합니다.
        if (CurrentHp > 0)
        {
            PlayHurtAnim();
        }
        // 사망 로직은 `base.TakeDamage` 내에서 `HandleDeathLogic`을 호출하여 처리됩니다.
    }

    /// <summary>
    /// 적을 경직 상태로 만듭니다. (예: 플레이어의 패링 공격 성공 시 호출)
    /// </summary>
    public void Stun()
    {
        if (IsDead || isStunned) return; // 이미 죽었거나 이미 경직 상태면 아무것도 하지 않음

        isStunned = true; // 경직 상태 플래그를 true로 설정
        PlayStunAnim();   // 경직 애니메이션 재생

        // 이전에 실행 중인 경직 코루틴이 있다면 중지하고 새로운 코루틴을 시작합니다.
        // 이는 중복 경직이나 경직 시간 초기화를 방지합니다.
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(ReleaseStunCoroutine(stunDuration));
    }

    /// <summary>
    /// 지정된 시간 후에 경직 상태를 해제하는 코루틴입니다.
    /// </summary>
    /// <param name="duration">경직 지속 시간.</param>
    IEnumerator ReleaseStunCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration); // 지정된 시간만큼 대기

        if (!IsDead) // 대기 시간 후에도 적이 죽지 않았다면 경직 상태를 해제
        {
            isStunned = false;
        }
    }

    /// <summary>
    /// 대기 애니메이션을 재생합니다.
    /// </summary>
    protected override void PlayIdleAnim()
    {
        // 사망, 경직, 피격 상태가 아니면 걷기 애니메이션을 멈춰 대기 상태로 만듭니다.
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, false);
    }

    /// <summary>
    /// 걷기 애니메이션을 재생합니다.
    /// '이동 모션'에 애니메이션 커브를 사용하도록 애니메이터의 Bool 파라미터를 설정합니다.
    /// 실제 이동 속도 제어는 애니메이터(Root Motion)나 애니메이션 이벤트,
    /// 또는 애니메이션 커브에서 가져온 값을 통해 이루어집니다.
    /// </summary>
    protected override void PlayWalkAnim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
            animator.SetBool(ANIM_BOOL_MOB02_WALK, true);
    }

    /// <summary>
    /// 사망 애니메이션을 재생합니다.
    /// </summary>
    protected override void PlayDeathAnim()
    {
        if (animator != null)
            animator.SetTrigger(ANIM_TRIGGER_MOB02_DEATH);
    }

    /// <summary>
    /// 피격 애니메이션을 재생합니다.
    /// </summary>
    protected override void PlayHurtAnim()
    {
        // 사망, 경직 상태가 아니면 피격 애니메이션을 트리거합니다.
        if (!IsDead && !isStunned && animator != null)
        {
            isPerformingHurtAnimation = true; // 피격 애니메이션 중임을 나타내는 플래그 설정
            animator.SetTrigger(ANIM_TRIGGER_MOB02_HURT); // 피격 애니메이션 트리거
        }
    }

    /// <summary>
    /// 경직 애니메이션을 재생합니다. (Mob02 고유의 경직 애니메이션 재생 함수)
    /// </summary>
    protected void PlayStunAnim()
    {
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_STUN);
        }
    }

    /// <summary>
    /// '내지르기(일반)' 공격 애니메이션을 재생합니다. (부모 클래스의 PlayAttack1Anim 오버라이드)
    /// '공격 모션'에 애니메이션 커브를 사용하도록 애니메이터의 Trigger 파라미터를 설정합니다.
    /// 공격 중 특정 시점의 이동이나 히트박스 활성화/비활성화는 애니메이션 이벤트를 통해 제어됩니다.
    /// </summary>
    protected override void PlayAttack1Anim()
    {
        if (!IsDead && !isStunned && !isPerformingHurtAnimation && animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_MOB02_ATTACK_A); // '내지르기' 공격 애니메이션 트리거
        }
    }

    // 잡몹02는 단일 공격 패턴이므로 다른 공격 애니메이션은 필요 없습니다.
    protected override void PlayAttack2Anim() { }
    protected override void PlayAttack3Anim() { }

    /// <summary>
    /// 모든 공격 관련 애니메이션 트리거를 리셋합니다.
    /// </summary>
    protected override void ResetAttackTriggers()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_ATTACK_A);
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_HURT);
            animator.ResetTrigger(ANIM_TRIGGER_MOB02_STUN);
        }
    }

    /// <summary>
    /// 잡몹02의 공격 로직을 수행합니다.
    /// 쿨다운을 확인하고 플레이어가 공격 범위 내에 있으면 '내지르기' 공격을 시작합니다.
    /// </summary>
    protected override void PerformAttackLogic()
    {
        // 이미 다른 상태(죽음, 경직, 피격 중, 공격 중, 공격 후 대기 중)이면 공격하지 않습니다.
        // 이 체크는 Update() 메서드의 시작 부분과 base.Update() 내에서 이미 처리됩니다.

        // 다음 공격까지의 쿨다운이 되었는지 확인
        bool cooldownReady = Time.time >= nextAttackTime;

        // 플레이어가 공격 범위 내에 있는지 확인
        if (playerTransform == null || Vector2.Distance(transform.position, playerTransform.position) > attackRange)
        {
            // 플레이어가 공격 범위 밖에 있으면 공격 시도하지 않고 이동 로직으로 돌아갑니다.
            return;
        }

        if (!cooldownReady)
        {
            return; // 쿨다운 중이면 공격 스킵
        }

        // 쿨다운이 끝나고 플레이어가 공격 범위 내에 있으면 공격 실행
        isPerformingAttackAnimation = true; // 공격 애니메이션 중임을 나타내는 플래그 설정
        PlayAttack1Anim(); // '내지르기' 공격 애니메이션 재생 (이 안에 ANIM_TRIGGER_MOB02_ATTACK_A 트리거 포함)
    }

    /// <summary>
    /// 공격 애니메이션이 종료될 때 애니메이션 이벤트로 호출됩니다.
    /// </summary>
    public override void OnAttackAnimationEnd()
    {
        base.OnAttackAnimationEnd(); // 부모 클래스의 로직 호출 (isPerformingAttackAnimation 플래그 해제, 공격 후 대기 코루틴 시작)

        // 잡몹02 고유의 다음 공격 가능 시간 설정 (공격 애니메이션 종료 후 1초 텀)
        nextAttackTime = Time.time + attackACooldown;
    }

    /// <summary>
    /// 피격 애니메이션이 종료될 때 애니메이션 이벤트로 호출됩니다.
    /// </summary>
    public override void OnHurtAnimationEnd()
    {
        base.OnHurtAnimationEnd(); // isPerformingHurtAnimation 플래그 해제
    }

    /// <summary>
    /// '내지르기' 공격의 히트박스를 활성화합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void EnableAttackHitbox()
    {
        if (IsDead) return; // 사망 상태에서는 히트박스 활성화하지 않음

        if (attackAHitboxObject != null && attackAHitboxCollider != null)
        {
            if (attackAEnemyHitbox != null)
            {
                attackAEnemyHitbox.attackDamage = attackAValue; // 공격력 설정
            }
            else
            {
                Debug.LogWarning("Mob02MeleeController: EnemyHitbox 컴포넌트가 'Attack A Hitbox Object'에 없습니다!", attackAHitboxObject);
            }
            attackAHitboxCollider.enabled = true; // 콜라이더 활성화
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Object' 또는 콜라이더가 할당되지 않았거나 찾을 수 없습니다. 히트박스 활성화 실패.", this);
        }
    }

    /// <summary>
    /// '내지르기' 공격의 히트박스를 비활성화합니다. (애니메이션 이벤트로 호출)
    /// </summary>
    public void DisableAttackHitbox()
    {
        if (IsDead) return; // 사망 상태에서는 히트박스 비활성화하지 않음 (의미 없음)

        if (attackAHitboxCollider != null)
        {
            attackAHitboxCollider.enabled = false; // 콜라이더 비활성화
        }
        else
        {
            Debug.LogWarning("Mob02MeleeController: 'Attack A Hitbox Collider'가 할당되지 않았거나 찾을 수 없습니다. 히트박스 비활성화 실패.", this);
        }
    }

    /// <summary>
    /// 적의 시각적인 방향(스프라이트)을 전환합니다.
    /// </summary>
    /// <param name="faceLeft">왼쪽을 바라봐야 하면 true, 오른쪽을 바라봐야 하면 false.</param>
    protected override void Flip(bool faceLeft)
    {
        // 'Sprite'라는 이름의 자식 오브젝트를 찾아 스케일을 조절합니다.
        // 대부분의 경우 스프라이트는 메인 게임 오브젝트의 자식으로 존재합니다.
        Transform spriteToFlip = transform.Find("Sprite");

        if (spriteToFlip == null)
        {
            spriteToFlip = transform; // 'Sprite' 자식 오브젝트가 없으면 메인 오브젝트(루트 오브젝트)의 Transform을 사용
            Debug.LogWarning(gameObject.name + ": 'Sprite' 자식 오브젝트를 찾을 수 없습니다. 메인 오브젝트의 Transform을 사용하여 뒤집기를 시도합니다.", this);
        }

        Vector3 currentScale = spriteToFlip.localScale;

        // 이 로직은 'faceLeft'가 true일 때 (왼쪽을 바라봐야 할 때) localScale.x를 음수로,
        // 'faceLeft'가 false일 때 (오른쪽을 바라봐야 할 때) localScale.x를 양수로 만듭니다.
        // 중요: 실제 스프라이트의 기본 방향(예: 오른쪽을 바라볼 때 scale.x가 양수인지 음수인지)에 따라
        // Mathf.Abs(currentScale.x)에 곱하는 부호(+ 또는 -)를 조정해야 할 수 있습니다.
        if (faceLeft)
        {
            spriteToFlip.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else
        {
            spriteToFlip.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    /// <summary>
    /// 적이 추적할 플레이어의 Transform을 설정합니다. (PlayerDetector 등 외부 스크립트에서 호출)
    /// </summary>
    /// <param name="newPlayerTransform">플레이어의 Transform 컴포넌트.</param>
    public void SetPlayerTarget(Transform newPlayerTransform)
    {
        if (newPlayerTransform != null)
        {
            playerTransform = newPlayerTransform;
            Debug.Log($"{gameObject.name}: 플레이어 타겟이 설정되었습니다: {playerTransform.name}", this);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: SetPlayerTarget 함수에 전달된 플레이어 Transform이 null입니다. 플레이어를 추적할 수 없습니다.", this);
        }
    }
}