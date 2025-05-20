using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    TutorialEnemyAttackCollider attackCollider;
    Animator animator;

    private bool isMove = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        attackCollider = transform.GetChild(0).GetComponent<TutorialEnemyAttackCollider>();
    }

    private void Update()
    {
        if(isMove)
        {
            transform.position += Time.deltaTime * Vector3.left * 5f;
        }
    }

    public void PlayIdle()
    {
        animator.Play("Idle");
    }

    public void PlayAttack()
    {
        animator.Play("Attack", 0, 0f);
    }

    public void PlayAttackReady()
    {
        animator.Play("AttackReady");
    }

    public void PlayRun()
    {
        animator.Play("Run",0 ,0f);        
    }

    public void PlayDead()
    {
        animator.Play("Dead");
        Destroy(this.gameObject, 5f);
    }

    public void PlayHit()
    {
        animator.Play("Hit");        
    }

    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void SetMoveActive(bool value)
    {
        isMove = value;
    }

    public float GetCurrentAnimationStateNormalizeTime()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void AttackToTarget()
    {
        if(attackCollider.Target != null)
        {
            attackCollider.Target.TakeDamage(0, this.gameObject);
        }
    }
}
