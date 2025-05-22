using UnityEngine;

public class BossAin : MonoBehaviour
{
    GameObject boss;
    void Start()
    {
        boss = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(3, boss);
        }
    }
}
