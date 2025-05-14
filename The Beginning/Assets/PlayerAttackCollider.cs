using UnityEngine;

public class PlayerAttackCollider : MonoBehaviour
{
    Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamageable enemy = collision.gameObject.GetComponent<IDamageable>();
        if (enemy != null)
        {
            enemy.TakeDamage(player.Damage);
        }
    }
}
