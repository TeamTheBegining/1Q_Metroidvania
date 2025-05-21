using UnityEngine;

public class LavaArea : MonoBehaviour
{
    public float attackDamage = 10000;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.TakeDamage(attackDamage, this.gameObject);
        }
    }
}
