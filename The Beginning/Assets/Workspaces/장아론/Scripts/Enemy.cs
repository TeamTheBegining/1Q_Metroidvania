// Enemy.cs
using UnityEngine;

namespace 장아론
{
    // Enemy state enum for state machine
    public enum EnemyState
    {
        Idle,       // 0
        Walk,       // 1
        Attack1,    // 2
        Attack2,    // 3
        Attack3,    // 4
        Block,      // 5
        Death       // 6
    }

    public class Enemy : MonoBehaviour
    {
        [Header("Base Properties")]
        public float moveSpeed = 3f;
        public int maxHealth = 5;
        public int attackPower = 1;

        [Header("Attack Settings")]
        public float attackRange = 1.5f;
        // Cooldowns are used here as animation durations for Invoke timing.
        // Set these values in the Inspector to match or be slightly longer
        // than the actual animation clip durations for smooth transitions.
        public float attack1Cooldown = 0.5f;
        public float attack2Cooldown = 0.7f;
        public float attack3Cooldown = 1.0f;
        public float blockDuration = 1.5f;

        // Internal variables
        protected int currentHealth;
        protected Rigidbody2D rb;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        // Changed from protected to public to allow TestManager access (CS0122 fix)
        public EnemyState currentState = EnemyState.Idle;
        protected bool facingRight = true;
        protected float lastStateChangeTime;

        // Combo state tracking variable
        // 0: not in combo, 1: 1-hit combo, 2: 2-hit combo, 3: 3-hit combo
        protected int currentComboStage = 0;


        // Initialization
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
            // Ensure initial state is Idle (currentState is initialized above, but can double check)
            // ChangeState 호출 시 OnEnterState(Idle)이 실행되어 rb 속도 0 및 콤보 리셋 처리
            if (currentState != EnemyState.Idle)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            // Update animation state based on currentState every frame
            UpdateAnimation();
            // AI logic (in MeleeEnemy) or Input (in TestManager) will call StartAttack/Move/Block etc.
            // The Move() method handles the Idle/Walk state transitions based on velocity.
        }

        // Update the animation based on current state
        protected virtual void UpdateAnimation()
        {
            if (animator != null)
            {
                // Pass the integer value of the current state enum to the Animator's "State" parameter
                animator.SetInteger("State", (int)currentState);
            }
        }

        // Change the current state of the enemy
        public virtual void ChangeState(EnemyState newState)
        {
            // If trying to change to the same state, ignore (usually) - Can uncomment if needed
            // if (currentState == newState) return;

            // Handle exiting the previous state
            OnExitState(currentState);

            // Set new state
            currentState = newState;
            lastStateChangeTime = Time.time;

            // Handle entering the new state
            OnEnterState(newState);

            // Animation update happens in Update loop on the next frame, which reads the new currentState
        }

        // Called when entering a new state
        protected virtual void OnEnterState(EnemyState state)
        {
            // Cancel any pending AttackComplete Invoke before setting a new one (important for chaining/cancelling combos)
            CancelInvoke(nameof(AttackComplete));
            // Cancel any pending EndBlock Invoke if entering a non-block state
            if (state != EnemyState.Block)
            {
                CancelInvoke(nameof(EndBlock));
            }


            switch (state)
            {
                case EnemyState.Idle:
                    rb.linearVelocity = Vector2.zero; // Stop movement when idling
                    currentComboStage = 0; // Reset combo stage
                    Debug.Log($"{gameObject.name} entered Idle. Combo Stage Reset.");
                    break;
                case EnemyState.Walk:
                    // Movement speed is handled by the Move() method setting linearVelocity.
                    // rb.linearVelocity = ... ; // Move() handles this
                    currentComboStage = 0; // Reset combo stage when walking (Can start a new combo from walk)
                    Debug.Log($"{gameObject.name} entered Walk. Combo Stage Reset.");
                    break;
                case EnemyState.Attack1:
                    rb.linearVelocity = Vector2.zero; // Stop movement during attacks
                    Debug.Log($"{gameObject.name} entered Attack 1. Combo Stage: {currentComboStage}");
                    // Schedule AttackComplete after Attack1 animation duration/cooldown
                    // This timing is crucial for combo chaining. Set attack1Cooldown in Inspector.
                    Invoke(nameof(AttackComplete), attack1Cooldown);
                    // Actual hitbox/damage logic should ideally be an Animation Event or triggered here.
                    break;
                case EnemyState.Attack2:
                    rb.linearVelocity = Vector2.zero; // Stop movement during attacks
                    Debug.Log($"{gameObject.name} entered Attack 2. Combo Stage: {currentComboStage}");
                    // Schedule AttackComplete after Attack2 animation duration/cooldown
                    Invoke(nameof(AttackComplete), attack2Cooldown);
                    break;
                case EnemyState.Attack3:
                    rb.linearVelocity = Vector2.zero; // Stop movement during attacks
                    Debug.Log($"{gameObject.name} entered Attack 3. Combo Stage: {currentComboStage}");
                    // Schedule AttackComplete after Attack3 animation duration/cooldown
                    Invoke(nameof(AttackComplete), attack3Cooldown);
                    break;
                case EnemyState.Block:
                    rb.linearVelocity = Vector2.zero; // Stop movement during block
                    currentComboStage = 0; // Reset combo stage when blocking (Can start a new combo after block)
                    Debug.Log($"{gameObject.name} is blocking! Combo Stage Reset.");
                    // Schedule EndBlock after block duration. Set blockDuration in Inspector.
                    Invoke(nameof(EndBlock), blockDuration);
                    break;
                case EnemyState.Death:
                    rb.linearVelocity = Vector2.zero; // Stop movement on death
                    // Disable collider so enemy doesn't interact after dying
                    Collider2D enemyCollider = GetComponent<Collider2D>();
                    if (enemyCollider != null) enemyCollider.enabled = false;
                    Debug.Log($"{gameObject.name} has died!");
                    // Schedule object destruction after death animation duration. Adjust time as needed.
                    Destroy(gameObject, 2f);
                    break;
            }
        }

        // Called when exiting a state
        protected virtual void OnExitState(EnemyState state)
        {
            // Cleanup or transition logic if needed when leaving a state.
            // Cancelling Invokes when leaving a state is mostly handled in OnEnterState for the new state.
        }

        // Move the enemy in the specified direction (-1 for left, 1 for right, 0 for stop)
        public virtual void Move(float direction)
        {
            // Debug.Log($"---> Move({direction}) called. Current State: {currentState}, Time: {Time.time}"); // Debug log for tracking Move calls
            // Don't move if in states where movement is restricted
            if (currentState == EnemyState.Death ||
                currentState == EnemyState.Attack1 ||
                currentState == EnemyState.Attack2 ||
                currentState == EnemyState.Attack3 ||
                currentState == EnemyState.Block)
                return; // Exit if movement is not allowed in current state

            // Apply velocity for movement based on direction and speed
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            // Flip sprite to face movement direction if necessary
            if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            {
                Flip();
            }

            // Update state based on movement speed
            if (Mathf.Abs(direction) > 0.01f) // Check if moving significantly (use a small threshold due to float precision)
            {
                if (currentState != EnemyState.Walk)
                    ChangeState(EnemyState.Walk); // Transition to Walk if moving
            }
            else // If not moving significantly
            {
                if (currentState == EnemyState.Walk)
                    ChangeState(EnemyState.Idle); // Transition back to Idle if was walking
            }
        }

        // Flip the enemy sprite to face the opposite direction
        protected virtual void Flip()
        {
            facingRight = !facingRight;
            // Efficient way to flip the sprite visually by scaling
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        // --- Combo System Methods ---

        // Start an attack combo up to maxStage (called by Input/AI)
        // comboMaxStage should be 1, 2, or 3
        public virtual void StartAttack(int comboMaxStage)
        {
            // Debug.Log($"---> StartAttack({comboMaxStage}) 메서드 시작."); // Debug log for tracking StartAttack calls

            // Cannot start a new attack combo if already in a restricted state
            if (currentState == EnemyState.Death ||
                currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3 || // Cannot start new combo while attacking
                currentState == EnemyState.Block) // Cannot start new combo while blocking
                return; // Exit if not in a state where a new combo can be started

            // Set the target combo stage (ensuring it's between 1 and 3)
            currentComboStage = Mathf.Clamp(comboMaxStage, 1, 3);
            // Always start the combo sequence by transitioning to Attack1 state
            ChangeState(EnemyState.Attack1);
            // Debug.Log($"StartAttack called. Setting combo stage to {currentComboStage} and changing state to Attack1."); // Debug log
        }

        // Called by Invoke when an attack animation/cooldown completes
        // Decides the next state based on the current attack that finished and the combo stage
        protected virtual void AttackComplete()
        {
            Debug.Log($"---> AttackComplete 메서드 시작. 현재 상태: {currentState}, 콤보 단계: {currentComboStage}."); // Debug log

            // Check which attack just finished based on currentState
            if (currentState == EnemyState.Attack1)
            {
                // If the target combo stage requires Attack2 or Attack3, transition to Attack2
                if (currentComboStage >= 2)
                {
                    Debug.Log("AttackComplete(A1): 콤보 단계 2 이상 -> Attack2로 전환.");
                    ChangeState(EnemyState.Attack2); // Transition to next attack in combo
                }
                // Otherwise (if it was a 1-hit combo - Stage 1), transition to BLOCK
                else // currentComboStage == 1
                {
                    Debug.Log("AttackComplete(A1): 콤보 단계 1 -> BLOCK으로 전환.");
                    ChangeState(EnemyState.Block); // End stage 1 combo by going to Block
                }
            }
            else if (currentState == EnemyState.Attack2)
            {
                // If the target combo stage requires Attack3, transition to Attack3
                if (currentComboStage >= 3)
                {
                    Debug.Log("AttackComplete(A2): 콤보 단계 3 이상 -> Attack3로 전환.");
                    ChangeState(EnemyState.Attack3); // Transition to next attack in combo
                }
                // Otherwise (if it was a 2-hit combo - Stage 2), transition to BLOCK
                else // currentComboStage == 2
                {
                    Debug.Log("AttackComplete(A2): 콤보 단계 2 -> BLOCK으로 전환.");
                    ChangeState(EnemyState.Block); // End stage 2 combo by going to Block
                }
            }
            else if (currentState == EnemyState.Attack3)
            {
                // Attack3 is always the last hit in its combo (Stage 3), so transition to BLOCK
                Debug.Log("AttackComplete(A3): 콤보 종료 (단계 3) -> BLOCK으로 전환.");
                ChangeState(EnemyState.Block); // End stage 3 combo by going to Block
            }
            // Note: currentComboStage is reset to 0 in OnEnterState for Idle/Walk/Block/Death

            // If AttackComplete was called from an unexpected state (shouldn't happen with correct logic)
            else
            {
                Debug.LogWarning($"AttackComplete called from unexpected state: {currentState}");
                ChangeState(EnemyState.Idle); // Fallback to Idle in unexpected cases
            }
        }

        // --- Original Attack Methods (Kept for structure/debug, state change is managed elsewhere) ---
        // These methods are generally NOT called directly to initiate state changes in the combo system.
        // State changes are managed by StartAttack and AttackComplete calling ChangeState.

        // Perform attack 1 (kept for structure/debug)
        public virtual void Attack1()
        {
            // Debug.Log($"{gameObject.name} performed Attack 1! (Method called)"); // Debug log
            // Actual hitbox/damage logic should ideally be an Animation Event or OnEnterState(Attack1)
            // State change and Invoke are managed by StartAttack and OnEnterState.
        }

        // Perform attack 2 (kept for structure/debug)
        public virtual void Attack2()
        {
            // Debug.Log($"{gameObject.name} performed Attack 2! (Method called)"); // Debug log
            // Actual hitbox/damage logic should ideally be an Animation Event or OnEnterState(Attack2)
            // State change and Invoke are managed by AttackComplete and OnEnterState.
        }

        // Perform attack 3 (kept for structure/debug)
        public virtual void Attack3()
        {
            // Debug.Log($"{gameObject.name} performed Attack 3! (Method called)"); // Debug log
            // Actual hitbox/damage logic should ideally be an Animation Event or OnEnterState(Attack3)
            // State change and Invoke are managed by AttackComplete and OnEnterState.
        }

        // Enter blocking state (Can be called directly or from AttackComplete)
        public virtual void Block()
        {
            // Cannot block if dead or currently attacking (unless attack allows block cancel - not implemented)
            if (currentState == EnemyState.Death ||
                currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3)
                return;

            ChangeState(EnemyState.Block);
            // Debug.Log($"{gameObject.name} is blocking!"); // Debug log is in OnEnterState
            // Invoke for EndBlock handled by OnEnterState(EnemyState.Block)
        }

        // End blocking state (Called by Invoke after blockDuration)
        protected virtual void EndBlock()
        {
            // Only transition out of Block if currently in Block state
            if (currentState == EnemyState.Block)
            {
                ChangeState(EnemyState.Idle); // Return to Idle after blocking duration
            }
        }

        // Take damage from attacks
        public virtual void TakeDamage(int damage)
        {
            // Don't take damage if dead
            if (currentState == EnemyState.Death) return;

            // Reduce damage if blocking
            if (currentState == EnemyState.Block)
            {
                damage = Mathf.Max(1, damage / 2); // Minimum 1 damage even when blocking
                Debug.Log($"{gameObject.name} blocked some damage!");
            }

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

            if (currentHealth <= 0)
            {
                Die(); // Transition to Death if health is zero or less
            }
            else
            {
                // Visual hit effect (Coroutine)
                StartCoroutine(FlashEffect());
            }
        }

        // Handle enemy death
        public virtual void Die()
        {
            // Only transition to Death if not already in death state
            if (currentState == EnemyState.Death) return;

            ChangeState(EnemyState.Death); // Change state to Death
            // Debug.Log($"{gameObject.name} has died!"); // Debug log is in OnEnterState
            // Object destruction handled in OnEnterState(EnemyState.Death)
        }

        // Visual flash effect when taking damage (Coroutine)
        protected virtual System.Collections.IEnumerator FlashEffect()
        {
            if (spriteRenderer != null)
            {
                // Temporarily change color to red for hit effect
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f); // Flash duration (adjust as needed)
                spriteRenderer.color = Color.white; // Return to normal color
            }
        }

        // Perform a random attack combo (1-3) - Now initiates a combo
        public virtual void RandomAttack()
        {
            // Check if a new combo can be started (handled in StartAttack, but kept for clarity if called directly)
            if (currentState == EnemyState.Death ||
               currentState == EnemyState.Attack1 || currentState == EnemyState.Attack2 || currentState == EnemyState.Attack3 ||
               currentState == EnemyState.Block)
                return;

            int attackType = Random.Range(1, 4); // Random combo stage (1, 2, or 3)

            // Use StartAttack to initiate the combo based on the randomly chosen stage
            StartAttack(attackType);
            Debug.Log($"{gameObject.name} initiating RandomAttack combo stage {attackType}!"); // Debug log
        }
    }
}