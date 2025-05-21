using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    private Vector3 moveDir = Vector3.zero;
    private float speed = 0f;

    public float damage = 1f;

    public Action ReturnAction { get; set; }

    private void Update()
    {
        if(moveDir != Vector3.zero && speed != 0f)
        {
            transform.position += speed * moveDir * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.TakeDamage(damage, this.gameObject);
        }
    }

    public void SetDirection(Vector3 dirVec, float speedValue)
    {
        moveDir = dirVec;
        speed = speedValue;
    }

    private void OnDisable()
    {
        ReturnAction?.Invoke();
    }

    public void OnSpawn()
    {
        
    }

    public void OnDespawn()
    {
        moveDir = Vector3.zero;
        speed = 0f;
    }
}
