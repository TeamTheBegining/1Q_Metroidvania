using UnityEngine;

public class TutorialEnemyAttackCollider : MonoBehaviour
{
    private IDamageable target;
    public IDamageable Target => target;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            IDamageable triggerTarget = collision.GetComponent<IDamageable>();
            target = triggerTarget;
        }
    }

}
