using UnityEngine;

namespace ��Ʒ�
{
    public class MeleeEnemy : Enemy
    {
        [Header("Melee AI Settings")]
        public float detectionRange = 5f;
        public float chaseRange = 8f;
        public float attackRange = 1.5f;
        public float timeBetweenAttacks = 2f; // ���� �� ������
        public float idleMoveInterval = 3f; // ���̵� ���¿��� �����̴� ����
        public float idleMoveRange = 2f; // ���̵� ���¿��� �����̴� �ִ� �Ÿ�
        private float lastIdleMoveTime;
        private Vector2 idleMoveTarget;

        private Transform target; // �÷��̾� Ÿ��

        protected override void Update()
        {
            base.Update(); // �θ� Ŭ������ Update() ȣ��

            if (target == null)
            {
                FindTarget();
            }
            else
            {
                // ���� ���� ���� ª�� �ð� ���� AI ���� ����
                if (Time.time >= lastStateChangeTime + 0.1f)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, target.position);

                    if (currentState != EnemyState.Death)
                    {
                        if (currentState == EnemyState.Block)
                        {
                            // ��� ���¿����� Ư���� AI ���� (EndBlock�� Invoke�� ó��)
                        }
                        else if (distanceToTarget <= attackRange)
                        {
                            // ���� ���� ��
                            if (currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                // ���� ��ٿ��� �������� ���� ���� ����
                                // (StartAttack ���ο��� ���� �� �޺� ����)
                                if (Time.time >= lastAttackTime + timeBetweenAttacks)
                                {
                                    RandomAttack();
                                    lastAttackTime = Time.time;
                                }
                            }
                        }
                        else if (distanceToTarget <= chaseRange)
                        {
                            // �߰� ���� ��, ���� ���� �ƴϸ� �̵�
                            if (currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                MoveTowardsTarget();
                            }
                        }
                        else
                        {
                            // �߰� ���� ��, ���� ���� �ƴϸ� ���̵� ���·� ��ȯ
                            if (currentState != EnemyState.Idle && currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                ChangeState(EnemyState.Idle);
                            }
                            // ���̵� ���¿����� ������ ó��
                            if (currentState == EnemyState.Idle && Time.time >= lastIdleMoveTime + idleMoveInterval)
                            {
                                StartIdleMove();
                            }
                            if (currentState == EnemyState.Idle && rb.velocity.magnitude < 0.1f && Vector2.Distance(transform.position, idleMoveTarget) > 0.1f)
                            {
                                MoveTowardsIdleTarget();
                            }
                            else if (currentState == EnemyState.Idle && Vector2.Distance(transform.position, idleMoveTarget) <= 0.1f)
                            {
                                rb.velocity = Vector2.zero; // ��ǥ ���� ���� �� ����
                            }
                        }
                    }
                }
            }
        }

        private void FindTarget()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log($"{gameObject.name} found target: {target.name}");
            }
        }

        private void MoveTowardsTarget()
        {
            if (target != null)
            {
                float direction = (target.position.x > transform.position.x) ? 1f : -1f;
                Move(direction);
            }
        }

        private void StartIdleMove()
        {
            lastIdleMoveTime = Time.time;
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            idleMoveTarget = (Vector2)transform.position + randomDirection * Random.Range(0.5f, idleMoveRange);
        }

        private void MoveTowardsIdleTarget()
        {
            float direction = (idleMoveTarget.x > transform.position.x) ? 1f : -1f;
            Move(direction);
        }

        private float lastAttackTime; // ������ ���� �ð�

        // AttackComplete �������̵� (MeleeEnemy Ưȭ ����)
        protected override void AttackComplete()
        {
            base.AttackComplete(); // �⺻ AttackComplete ���� ����

            // �޺� ������ ���� �� ��� ���·� ��ȯ�ϴ� ���,
            // ���� �� ��� �����̸� ������ �ٽ� Idle ���·� ���ư��ų�,
            // �ٸ� �ൿ�� �� �� �ֵ��� ������ �� �ֽ��ϴ�.
            if (currentState == EnemyState.Block)
            {
                Invoke(nameof(ReturnToIdle), blockDuration + 0.2f); // ��� ���� �� ��� �� ���̵��
            }
            else if (currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3)
            {
                // ���� �ִϸ��̼��� ���� ��, ���� �ൿ�� ���� (���⼭�� �ϴ� Idle��)
                Invoke(nameof(ReturnToIdle), GetAttackCooldown(currentState) + 0.1f);
            }
        }

        private float GetAttackCooldown(EnemyState attackState)
        {
            switch (attackState)
            {
                case EnemyState.Attack1:
                    return attack1Cooldown;
                case EnemyState.Attack2:
                    return attack2Cooldown;
                case EnemyState.Attack3:
                    return attack3Cooldown;
                default:
                    return 0f;
            }
        }

        private void ReturnToIdle()
        {
            if (currentState == EnemyState.Block || currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);

            if (currentState != EnemyState.Death && currentState != EnemyState.Block)
            {
                // �ǰ� �� ���� Ȯ���� ��� �õ�
                if (Random.value < 0.3f)
                {
                    Block();
                }
            }
        }
    }
}