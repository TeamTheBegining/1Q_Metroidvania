using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class EnemyController : TestBase
{
#if UNITY_EDITOR
    // �ִϸ����� ����
    private Animator animator;

    // �ִϸ��̼� ���� ������ ���� ������
    private bool isWalking = false; // �ȱ� �ִϸ��̼� ���� (Animator �Ķ���Ϳ� ����)
    private bool isDead = false; // ��� ���� (AI �� ��Ÿ �ý��ۿ��� ���)

    // �÷��̾� ������Ʈ ����
    private GameObject player;
    // �� ĳ���Ͱ� ó�� ������ �� �ٶ󺸴� ���� (��: �������� ���� 1, ������ ���� -1)
    public float initialFacingDirection = 1f;

    // �÷��̾� ������Ʈ�� �̸� (Inspector���� ����)
    public string playerObjectName = "Player";

    // ===== AI �Ķ���� =====
    // �÷��̾ �����ϴ� �Ÿ� (�� ���� �ȿ� ���;� �߰� ����)
    public float detectionRange = 10f;

    // �÷��̾�Լ� ���缭 ������ �����ϴ� �Ÿ� (�� ���� �ȿ� ������ ����/����)
    // ����, �� �Ÿ��� �����Ϸ��� ��ǥ ������ �˴ϴ�.
    public float attackRange = 2f;

    // �÷��̾ attackRange���� �󸶳� �� ��������� �ڷ� �������� �����ϴ� ����
    public float maintainBuffer = 0.5f;

    // �ȱ� ������ �� �̵� �ӵ� (�ڷ� �������� �ӵ��� ����)
    public float moveSpeed = 3f;

    // ���� ��Ÿ�� (AI ���� �� ���)
    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity; // ������ ���� �ð�


    // ���� ���� AI ���¸� ��Ÿ���� enum
    private enum EnemyState { B_Idle, Chase, Attack, Dead }
    private EnemyState currentState = EnemyState.B_Idle;
    // ===== AI �Ķ���� �� =====


    private void Start()
    {
        animator = GetComponent<Animator>();
        SetState(EnemyState.B_Idle);

        player = GameObject.Find(playerObjectName);
        if (player == null)
        {
            Debug.LogError("Player GameObject with name '" + playerObjectName + "' not found!");
        }
    }

    private void Update()
    {
        if (currentState == EnemyState.Dead) return;
        if (player == null)
        {
            if (currentState != EnemyState.B_Idle) SetState(EnemyState.B_Idle);
            return;
        }

        UpdateFacingDirection();

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // ===== AI ���� ��ȯ ���� =====
        switch (currentState)
        {
            case EnemyState.B_Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    Debug.Log("B_Idle -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    Debug.Log("Chase -> Attack (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Attack);
                }
                else if (distanceToPlayer > detectionRange)
                {
                    Debug.Log("Chase -> B_Idle (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.B_Idle);
                }
                else
                {
                    // Debug.Log("Chase State (Distance: " + distanceToPlayer.ToString("F2") + "), Moving Towards Player.");
                    MoveTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    Debug.Log("Attack -> Chase (Distance: " + distanceToPlayer.ToString("F2") + ")");
                    SetState(EnemyState.Chase);
                }
                else
                {
                    if (distanceToPlayer < attackRange - maintainBuffer)
                    {
                        Debug.Log("Too close (" + distanceToPlayer.ToString("F2") + "), retreating to maintain attack range.");
                        MoveAwayFromPlayer();
                    }
                    else
                    {
                        // Debug.Log("Within attack range (" + distanceToPlayer.ToString("F2") + "), attempting attack.");
                        PerformAttack(); // ���� ���� ���� (��Ÿ�� üũ ����)
                    }
                }
                break;

            case EnemyState.Dead:
                break;
        }
        // ===== AI ���� ��ȯ ���� �� =====
    }

    // ===== AI ���� �� ���� ���� �Լ��� =====

    private void SetState(EnemyState newState)
    {
        Debug.Log("SetState: " + currentState + " -> " + newState);
        if (currentState == newState) return;

        // ���� ���¿��� ���� �� ���� (�ʿ��ϴٸ�)

        currentState = newState;

        // �� ���¿� ������ �� ����
        switch (currentState)
        {
            case EnemyState.B_Idle:
                SetB_IdleStateAnimation();
                isWalking = false;
                animator.SetBool("IsWalking", false);
                break;

            case EnemyState.Chase:
                isWalking = true;
                animator.SetBool("IsWalking", true);
                // �߰� ���� ���� �� �ٸ� �ִϸ��̼� Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("B_Jump");
                animator.ResetTrigger("B_Attack1");
                animator.ResetTrigger("B_Attack2"); // B_Attack2 Ʈ���� �ʱ�ȭ �߰�
                animator.ResetTrigger("B_Death");
                break;

            case EnemyState.Attack:
                isWalking = false;
                animator.SetBool("IsWalking", false);
                // ���� ���� ���� �� �ٸ� �ִϸ��̼� Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("B_Jump");
                animator.ResetTrigger("B_Attack1"); // Attack ���� ���� �� Attack1 Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("B_Attack2"); // Attack ���� ���� �� Attack2 Ʈ���� �ʱ�ȭ
                animator.ResetTrigger("B_Death");
                // ���� �ִϸ��̼��� PerformAttack() ���� ������ Ʈ����
                break;

            case EnemyState.Dead:
                Die();
                break;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (player == null || currentState != EnemyState.Chase) return;

        Vector3 directionToPlayer = (player.transform.position - transform.position);
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }
    }

    private void MoveAwayFromPlayer()
    {
        if (player == null || currentState != EnemyState.Attack) return; // Attack ���¿��� ���� ������ ���� ������

        Vector3 directionToPlayer = (transform.position - player.transform.position); // �� -> �÷��̾� ����
        directionToPlayer.y = 0;

        if (directionToPlayer.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection = directionToPlayer.normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;

            transform.position += movement;
        }
    }

    private void PerformAttack()
    {
        if (player == null || currentState != EnemyState.Attack) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Debug.Log("���� ���� �õ� (��Ÿ�� �Ϸ�)");
            // ===== AI ���� ���� ���� �߰� =====
            int attackChoice = Random.Range(0, 2); // 0 �Ǵ� 1 ���� ����
            if (attackChoice == 0)
            {
                Debug.Log("AI ����: B_Attack1 �ߵ�");
                PlayB_Attack1Animation();
            }
            else
            {
                Debug.Log("AI ����: B_Attack2 �ߵ�");
                PlayB_Attack2Animation();
            }
            // ===== AI ���� ���� ���� �� =====

            lastAttackTime = Time.time;

            // TODO: ������ �÷��̾�� �������� �ִ� ���� �߰� (���õ� ���ݿ� ���� ������ �ٸ� �� ����)
        }
        // else { Debug.Log("���� ��� �� (��Ÿ��)"); }
    }

    private void Die()
    {
        Debug.Log("�� ��� ó��");
        isWalking = false;
        isDead = true;
        animator.SetBool("IsWalking", false);
        PlayB_DeathAnimationAnimation();

        // TODO: �ʿ��ϴٸ� ������Ʈ ��Ȱ��ȭ/�ı� �� �߰�
        // ��: Destroy(gameObject, 3f);
    }

    private void UpdateFacingDirection()
    {
        if (player == null) return;

        float directionX = player.transform.position.x - transform.position.x;
        Vector3 currentScale = transform.localScale;

        if (directionX > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * +Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
        else if (directionX < -0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x) * -Mathf.Sign(initialFacingDirection), currentScale.y, currentScale.z);
        }
    }
    // ===== AI ���� �� ���� ���� �Լ� �� =====


    // ===== Ű �Է� ó�� �Լ��� =====

    // Ű 1 �Է� ó�� (�ȱ� ���)
    protected override void OnTest1(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            if (currentState == EnemyState.B_Idle || currentState == EnemyState.Chase)
            {
                if (isWalking) SetState(EnemyState.B_Idle);
                else SetState(EnemyState.Chase);
            }
            else if (currentState == EnemyState.Attack) // ���� ���¿��� ���� �ȱ� ��� �� -> �߰� ���·� ��ȯ (���� �ߴ�)
            {
                SetState(EnemyState.Chase);
            }
        }
    }

    // Ű 2 �Է� ó�� (����)
    protected override void OnTest2(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("Ű 2: ���� ����");
            PlayB_JumpAnimationAnimation();
            // TODO: ���� �߿��� AI ���¸� ��� �����ϰų� B_Idle ������ ��ȯ�ϴ� ���� �߰�
            // SetState(EnemyState.B_Idle); // ����
        }
    }

    // Ű 3 �Է� ó�� (B_Attack1 - ����)
    protected override void OnTest3(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("Ű 3: B_Attack1 (����) ����");
            // AI ���¿� ������ ���� ���� �ִϸ��̼� Ʈ����
            PlayB_Attack1Animation();
            // TODO: ���� ���� �߿��� AI ���¸� ��� �����ϰų� B_Idle ������ ��ȯ�ϴ� ���� �߰�
            // SetState(EnemyState.B_Idle); // ����
        }
    }

    // Ű 4 �Է� ó�� (���)
    protected override void OnTest4(InputAction.CallbackContext context)
    {
        if (currentState != EnemyState.Dead)
        {
            if (context.performed)
            {
                Debug.Log("Ű 4: ��� �ִϸ��̼� ����");
                SetState(EnemyState.Dead);
            }
        }
    }

    // Ű 5 �Է� ó�� (���� B_Idle ����)
    protected override void OnTest5(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("Ű 5: B_Idle ���� ����");
            SetState(EnemyState.B_Idle);
        }
    }

    // ===== Ű 6 �Է� ó�� (B_Attack2 - ����) �߰� =====
    protected override void OnTest6(InputAction.CallbackContext context)
    {
        if (currentState == EnemyState.Dead) return;
        if (context.performed)
        {
            Debug.Log("Ű 6: B_Attack2 (����) ����");
            // AI ���¿� ������ ���� ���� �ִϸ��̼� Ʈ����
            PlayB_Attack2Animation(); // <-- B_Attack2 ��� �Լ� ȣ��
            // TODO: ���� ���� �߿��� AI ���¸� ��� �����ϰų� B_Idle ������ ��ȯ�ϴ� ���� �߰�
            // SetState(EnemyState.B_Idle); // ����
        }
    }
    // ===== Ű 6 �Է� ó�� �� =====


    #region �ִϸ��̼� ��� �Լ���

    private void SetB_IdleStateAnimation()
    {
        animator.ResetTrigger("B_Jump");
        animator.ResetTrigger("B_Death");
        animator.ResetTrigger("B_Attack1");
        animator.ResetTrigger("B_Attack2"); // B_Attack2 Ʈ���� �ʱ�ȭ �߰�
    }

    private void PlayB_JumpAnimationAnimation()
    {
        animator.SetTrigger("B_Jump");
    }

    private void PlayB_Attack1Animation()
    {
        animator.SetTrigger("B_Attack1"); // <-- Animator Trigger �Ķ���� �̸�
    }

    // ===== B_Attack2 �ִϸ��̼� ��� �Լ� �߰� =====
    private void PlayB_Attack2Animation()
    {
        animator.SetTrigger("B_Attack2"); // <-- Animator Trigger �Ķ���� �̸�
    }
    // ===== B_Attack2 �ִϸ��̼� ��� �Լ� �� =====


    private void PlayB_DeathAnimationAnimation()
    {
        animator.SetTrigger("B_Death");
    }

    #endregion

#endif // UNITY_EDITOR
}