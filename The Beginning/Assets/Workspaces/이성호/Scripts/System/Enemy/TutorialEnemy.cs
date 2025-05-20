using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAttack()
    {
        animator.Play("Attack");
    }

    public void PlayRun()
    {
        animator.Play("Attack");        
    }
}
