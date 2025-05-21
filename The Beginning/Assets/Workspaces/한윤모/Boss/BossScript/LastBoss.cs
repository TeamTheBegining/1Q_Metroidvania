using System;
using Unity.VisualScripting;
using UnityEngine;

public class LastBoss : MonoBehaviour
{
    private LastBossStaye currentState;
    private Animator bossani;
    float upSpeed = 2f;
    public enum LastBossStaye
    { 
        Start,
        Idle,
        Move,
        Attack1,
        Attack2,
        Attack3,
        Dead
    }

    private void Awake()
    {
        currentState = LastBossStaye.Start;
        bossani = GetComponent<Animator>();
        print("Ω√¿€");
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case LastBossStaye.Start:
                StartUpdate();
                bossani.Play("Start");
                break;
            case LastBossStaye.Idle:
                IdleUpdate();
                bossani.Play("Idle");
                break;
            case LastBossStaye.Move:
                MoveUpdate();
                bossani.Play("Move");
                break;
            case LastBossStaye.Attack1:
                Attack1Update();
                bossani.Play("Attack1");
                break;
            case LastBossStaye.Attack2:
                Attack2Update();
                bossani.Play("Attack2");
                break;
            case LastBossStaye.Attack3:
                Attack3Update();
                bossani.Play("Attack3");
                break;
            case LastBossStaye.Dead:
                DeadUpdate();
                bossani.Play("Dead");
                break;
        }

    }

    private void StartUpdate()
    {
        if(transform.position.y < 0)
            transform.position += Vector3.up * Time.time * upSpeed;
    }

    private void IdleUpdate()
    {
        throw new NotImplementedException();
    }

    private void MoveUpdate()
    {
        throw new NotImplementedException();
    }

    private void Attack1Update()
    {
        throw new NotImplementedException();
    }

    private void Attack2Update()
    {
        throw new NotImplementedException();
    }

    private void Attack3Update()
    {
        throw new NotImplementedException();
    }

    private void DeadUpdate()
    {
        throw new NotImplementedException();
    }
}
