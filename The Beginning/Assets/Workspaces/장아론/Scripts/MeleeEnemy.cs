using UnityEngine;

namespace 장아론
{
    public class MeleeEnemy : Enemy
    {
        [Header("Melee AI Settings")]
        public float detectionRange = 5f;
        public float chaseRange = 8f;
        public float attackRange = 1.5f;
        public float timeBetweenAttacks = 2f; // 공격 간 딜레이
        public float idleMoveInterval = 3f; // 아이들 상태에서 움직이는 간격
        public float idleMoveRange = 2f; // 아이들 상태에서 움직이는 최대 거리
        private float lastIdleMoveTime;
        private Vector2 idleMoveTarget;

        private Transform target; // 플레이어 타겟

        protected override void Update()
        {
            base.Update(); // 부모 클래스의 Update() 호출

            if (target == null)
            {
                FindTarget();
            }
            else
            {
                // 상태 변경 직후 짧은 시간 동안 AI 재평가 방지
                if (Time.time >= lastStateChangeTime + 0.1f)
                {
                    float distanceToTarget = Vector2.Distance(transform.position, target.position);

                    if (currentState != EnemyState.Death)
                    {
                        if (currentState == EnemyState.Block)
                        {
                            // 블록 상태에서는 특별한 AI 없음 (EndBlock은 Invoke로 처리)
                        }
                        else if (distanceToTarget <= attackRange)
                        {
                            // 공격 범위 내
                            if (currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                // 공격 쿨다운이 끝났으면 랜덤 공격 시작
                                // (StartAttack 내부에서 상태 및 콤보 관리)
                                if (Time.time >= lastAttackTime + timeBetweenAttacks)
                                {
                                    RandomAttack();
                                    lastAttackTime = Time.time;
                                }
                            }
                        }
                        else if (distanceToTarget <= chaseRange)
                        {
                            // 추격 범위 내, 공격 중이 아니면 이동
                            if (currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                MoveTowardsTarget();
                            }
                        }
                        else
                        {
                            // 추격 범위 밖, 공격 중이 아니면 아이들 상태로 전환
                            if (currentState != EnemyState.Idle && currentState != EnemyState.Attack1 && currentState != EnemyState.Attack2 && currentState != EnemyState.Attack3)
                            {
                                ChangeState(EnemyState.Idle);
                            }
                            // 아이들 상태에서의 움직임 처리
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
                                rb.velocity = Vector2.zero; // 목표 지점 도착 시 멈춤
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

        private float lastAttackTime; // 마지막 공격 시간

        // AttackComplete 오버라이드 (MeleeEnemy 특화 동작)
        protected override void AttackComplete()
        {
            base.AttackComplete(); // 기본 AttackComplete 로직 실행

            // 콤보 공격이 끝난 후 블록 상태로 전환하는 대신,
            // 공격 후 잠시 딜레이를 가지고 다시 Idle 상태로 돌아가거나,
            // 다른 행동을 할 수 있도록 변경할 수 있습니다.
            if (currentState == EnemyState.Block)
            {
                Invoke(nameof(ReturnToIdle), blockDuration + 0.2f); // 블록 종료 후 잠시 후 아이들로
            }
            else if (currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3)
            {
                // 공격 애니메이션이 끝난 후, 다음 행동을 결정 (여기서는 일단 Idle로)
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
                // 피격 시 일정 확률로 블록 시도
                if (Random.value < 0.3f)
                {
                    Block();
                }
            }
        }
    }
}