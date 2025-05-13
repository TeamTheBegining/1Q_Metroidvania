using UnityEngine;
using System.Collections;

// �⺻���� �� ĳ������ ���� �� �Ӽ��� �����ϴ� ���� ��Ʈ�ѷ� (Base Class)
public class CommonEnemyController : MonoBehaviour
{
    // ===== �⺻ �Ӽ� =====
    [Header("Base Stats")]
    public int maxHealth = 100;
    protected int currentHealth;
    public float moveSpeed = 3f; // �ȱ�/�������� �ӵ�
    public float attackDamage = 10f; // �⺻ ���� ������ (���� ������ �Ļ� Ŭ������ �ִϸ��̼� �̺�Ʈ����)

    // ===== AI �Ķ���� =====
    [Header("AI Parameters")]
    public float detectionRange = 10f; // �÷��̾� ���� �Ÿ�
    public float attackRange = 2f; // ���� ���� �� ���� �Ÿ�
    public float maintainBuffer = 0.5f; // attackRange���� �󸶳� �� ��������� �������� ����
    public string playerObjectName = "Player"; // �÷��̾� ������Ʈ �̸�

    // ===== �ִϸ����� �� �ִϸ��̼� ���� =====
    protected Animator animator;
    protected GameObject player;
    protected bool isDead = false; // ��� ����
    protected bool isPerformingAttackAnimation = false; // <-- ���� �ִϸ��̼� ��� ������ ���� (��ġ ������)

    // ���� ���� AI ����
    protected enum EnemyState { Idle, Chase, Attack, Dead } // Enum �̸��� ���������� ���
    protected EnemyState currentState = EnemyState.Idle; // �ʱ� ����

    // �� ĳ������ �ʱ� ���� (�¿� ������ ���)
    public float initialFacingDirection = 1f; // �������� ���� 1, ������ ���� -1


    protected virtual void Awake()
    {
        // Start���� ���� ȣ��� �� �ֵ��� Awake ��� (���� ���� � ����)
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator component not found on " + gameObject.name);

        currentHealth = maxHealth; // ü�� �ʱ�ȭ
    }

    protected virtual void Start()
    {
        SetState(EnemyState.Idle); // �ʱ� ���� ����

        player = GameObject.Find(playerObjectName);
        if (player == null)
        {
            Debug.LogError("Player GameObject with name '" + playerObjectName + "' not found! Check name/scene.");
        }
    }

    protected virtual void Update()
    {
        if (isDead) return; // ���� ���¸� �ƹ��͵� ����

        // �÷��̾� ������ Idle ���� ����
        if (player == null)
        {
            if (currentState != EnemyState.Idle) SetState(EnemyState.Idle);
            return;
        }

        UpdateFacingDirection(); // �÷��̾� ���� �ٶ󺸱�

        // ���� �ִϸ��̼� ���̸� AI �Ǵ� �� �̵� ���� ��ŵ
        if (isPerformingAttackAnimation)
        {
            // Debug.Log("���� �ִϸ��̼� ��. AI �Ǵ� ��ŵ.");
            return;
        }

        // �÷��̾���� �Ÿ� ���
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // ===== AI ���� ��ȯ ���� (����) =====
        switch (currentState)
        {
            case EnemyState.Idle:
                // �÷��̾ ���� ������ ������ ����
                if (distanceToPlayer <= detectionRange)
                {
                    Debug.Log("Idle -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                // �÷��̾ ���� ������ ������ ����
                if (distanceToPlayer <= attackRange)
                {
                    Debug.Log("Chase -> Attack (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Attack);
                }
                // �÷��̾ ���� ������ ����� Idle
                else if (distanceToPlayer > detectionRange)
                {
                    Debug.Log("Chase -> Idle (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Idle);
                }
                // ���� ���� ��, ���� ���� �� -> ��� ����
                else
                {
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                // �÷��̾ ���� ������ ����� ����
                if (distanceToPlayer > attackRange)
                {
                    Debug.Log("Attack -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                // �÷��̾� ���� ���� ��
                else
                {
                    // ���� ����
                    if (distanceToPlayer < attackRange - maintainBuffer)
                    {
                        Debug.Log("Too close, retreating.");
                        MoveAwayFromPlayer();
                    }
                    // ������ ���� �Ÿ� -> ���� ���� ���� (PerformAttackLogic�� �Ļ� Ŭ�������� ����)
                    else
                    {
                        // Debug.Log("Attempting attack logic.");
                        PerformAttackLogic(); // <-- �Ļ� Ŭ�������� �������̵��Ͽ� ���� ���� ����
                    }
                }
                break;

            case EnemyState.Dead:
                // ���� ���¿����� AI ���� ����
                break;
        }
        // ===== AI ���� ��ȯ ���� �� =====
    }

    // ===== AI ���� �� ���� ���� �Լ��� (����) =====

    // AI ���¸� �����ϰ� �ִϸ��̼� �� ���� ������Ʈ
    protected virtual void SetState(EnemyState newState)
    {
        Debug.Log(">>> SetState: " + currentState.ToString() + " -> " + newState.ToString());
        if (currentState == newState) return;

        // ���� ���� ���� ���� (�ʿ�� �Ļ� Ŭ�������� �������̵�)
        // ��: case EnemyState.Attack: isPerformingAttackAnimation = false; break;

        currentState = newState; // ���� ����

        // �� ���� ���� ����
        switch (currentState)
        {
            case EnemyState.Idle:
                PlayIdleAnim(); // Idle �ִϸ��̼� ��� (�Ļ� Ŭ�������� �������̵�)
                animator.SetBool("IsWalking", false); // <-- �������� �ȱ� Bool �Ķ���� (�ִϸ����Ϳ� "IsWalking" Bool �ʿ�)
                isPerformingAttackAnimation = false; // ���� �ִϸ��̼� �� ���� ����
                ResetAttackTriggers(); // ���� Ʈ���� �ʱ�ȭ
                break;

            case EnemyState.Chase:
                PlayWalkAnim(); // Walk �ִϸ��̼� ��� (�Ļ� Ŭ�������� �������̵�)
                animator.SetBool("IsWalking", true); // <-- �������� �ȱ� Bool �Ķ����
                isPerformingAttackAnimation = false; // ���� �ִϸ��̼� �� ���� ����
                ResetAttackTriggers(); // ���� Ʈ���� �ʱ�ȭ
                // ���� Ʈ���ŵ� �ʱ�ȭ (�ʿ��ϴٸ�)
                animator.ResetTrigger("Jump"); // <-- �������� Jump Ʈ���� (�ִϸ����Ϳ� "Jump" Trigger �ʿ�)
                break;

            case EnemyState.Attack:
                PlayIdleAnim(); // ���� �غ� �߿��� Idle ���� (�Ļ� Ŭ�������� �������̵�)
                animator.SetBool("IsWalking", false); // <-- �������� �ȱ� Bool �Ķ����
                                                      // isPerformingAttackAnimation�� PerformAttackLogic���� ����
                                                      // ���� ���� ���� �� �ٸ� Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("Jump"); // <-- �������� Jump Ʈ����
                ResetAttackTriggers(); // ���� Ʈ���� �ʱ�ȭ
                break;

            case EnemyState.Dead:
                isDead = true; // ��� �÷���
                isPerformingAttackAnimation = false; // ���� �ִϸ��̼� �� ���� ����
                animator.SetBool("IsWalking", false); // �ȱ� ����
                ResetAttackTriggers(); // ���� Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("Jump"); // ���� Ʈ���� �ʱ�ȭ
                PlayDeathAnim(); // ��� �ִϸ��̼� ��� (�Ļ� Ŭ�������� �������̵�)
                // TODO: ��� �� �߰� ó�� (������Ʈ ��Ȱ��ȭ/�ı� ��)
                break;
        }
    }

    // �÷��̾� �������� �̵� (Chase ����)
    protected virtual void MoveTowardsPlayer()
    {
        if (player == null || currentState != EnemyState.Chase || isPerformingAttackAnimation) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position);
        directionToPlayer.y = 0; // Y�� ���� (2D�� ���� 3D)

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    // �÷��̾� �ݴ� �������� �̵� (Attack ���¿��� �ʹ� ����� ��)
    protected virtual void MoveAwayFromPlayer()
    {
        if (player == null || currentState != EnemyState.Attack || isPerformingAttackAnimation) return;

        Vector3 directionAwayFromPlayer = (transform.position - player.transform.position);
        directionAwayFromPlayer.y = 0; // Y�� ����

        if (directionAwayFromPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionAwayFromPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

            transform.position += movement;
        }
    }

    // �Ļ� Ŭ�������� �������̵��Ͽ� ���� ���� ���� ���� (��Ÿ��, ���� ��)
    protected virtual void PerformAttackLogic()
    {
        // �⺻ Ŭ���������� Ư���� ���� ���� ����.
        // �Ļ� Ŭ�������� �� �޼ҵ带 �������̵��Ͽ�
        // ��Ÿ�� üũ, ���� �ִϸ��̼� Ʈ���� �ߵ�, isPerformingAttackAnimation = true ���� ���� ����
        Debug.Log("Base PerformAttackLogic called. Override this in derived class.");
    }

    // �÷��̾� �������� ĳ���� �¿� ����
    protected virtual void UpdateFacingDirection()
    {
        if (player == null) return;

        float directionX = player.transform.position.x - transform.position.x;
        Vector3 currentScale = transform.localScale;

        if (directionX > 0.01f)
        {
            // �÷��̾ �����ʿ� ���� �� (ĳ���Ͱ� �������� �ٶ󺸵��� ������ ����)
            // Mathf.Sign(initialFacingDirection)�� 1�̸� -Scale.x, -1�̸� +Scale.x
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
        else if (directionX < -0.01f)
        {
            // �÷��̾ ���ʿ� ���� �� (ĳ���Ͱ� ������ �ٶ󺸵��� ������ ����)
            // Mathf.Sign(initialFacingDirection)�� 1�̸� +Scale.x, -1�̸� -Scale.x
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * +Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
    }

    // ===== �ִϸ��̼� ���� �Լ��� (�Ļ� Ŭ�������� �������̵�) =====
    // �� �ִϸ��̼� ��� �Լ��� �Ļ� Ŭ�������� ������ �ش� �ִϸ��̼� Ʈ���Ÿ� �ߵ���ŵ�ϴ�.

    protected virtual void PlayIdleAnim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetBool("B_Walk", false);)
    protected virtual void PlayWalkAnim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetBool("B_Walk", true);)
    protected virtual void PlayJumpAnim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetTrigger("B_Jump");)
    protected virtual void PlayDeathAnim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetTrigger("B_Death");)
    protected virtual void PlayAttack1Anim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetTrigger("B_Attack1");)
    protected virtual void PlayAttack2Anim() { } // �Ļ� Ŭ�������� ���� (��: animator.SetTrigger("B_Attack2");)
    // ... �ٸ� ���� �ִϸ��̼��� �ִٸ� �߰� ...

    // ��� ���� Ʈ���� �ʱ�ȭ (�ִϸ��̼� ��ȯ�� ���̴� ���� ����)
    protected virtual void ResetAttackTriggers()
    {
        // �Ļ� Ŭ�������� ����ϴ� ���� ���� Ʈ���� �̸��� ����
        // ��: animator.ResetTrigger("B_Attack1"); animator.ResetTrigger("B_Attack2");
    }

    // ===== �ִϸ��̼� �̺�Ʈ���� ȣ��� �Լ� (����) =====
    // Animator�� ���� �ִϸ��̼� Ŭ�� ���� �� �̺�Ʈ�� �߰��ؾ� �մϴ�.
    public void OnAttackAnimationEnd()
    {
        Debug.Log("���� �ִϸ��̼� ���� �̺�Ʈ �߻� (Base Class).");
        isPerformingAttackAnimation = false; // ���� �ִϸ��̼� ���� �� ��ġ ���� �÷��� ��
        // Note: �ִϸ��̼� ���� �� AI ���´� ���� Update �����ӿ��� �ڵ����� �Ǵܵ˴ϴ�.
    }

    // ===== ������ ó�� ���� =====
    public virtual void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Current Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            SetState(EnemyState.Dead); // ü���� 0 ���ϸ� ��� ���·� ��ȯ
        }
        // TODO: ������ �ǰ� �ִϸ��̼� ��� �� �߰�
    }
}