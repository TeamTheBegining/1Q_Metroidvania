using System;
using System.Collections;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class Thunder : MonoBehaviour, IPoolable
{
    private Animator animator;
    private Player player;
    private float anitimer;
    private float anitime;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public Action ReturnAction { get; set; }

    private void OnEnable()
    {
        if (player == null) player = GameObject.FindWithTag("Player").GetComponent<Player>();
        anitimer = 0; 
        anitime = 5;
    }

    private void Update()
    {
        anitimer += Time.deltaTime;
        if (anitimer > anitime)
        {
            gameObject.SetActive(false);
            anitimer = 0;
        }
    }

    private void OnDisable()
    {
        ReturnAction?.Invoke();
    }

    public void OnDespawn()
    {
    }

    public void OnSpawn()
    {
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(3, gameObject);
        }

    }
}
