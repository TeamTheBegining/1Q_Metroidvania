using UnityEngine;

public class SilverBlueHitBox : MonoBehaviour
{
    SilverBlue    silverBlue;

    private void Awake()
    {
        silverBlue  = gameObject.GetComponentInParent<SilverBlue>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<IDamageable>() != null)
        {
            collision.GetComponent<IDamageable>().TakeDamage(silverBlue.Damage, silverBlue.gameObject );
            Debug.Log(silverBlue.Damage);
        }
    }
}
