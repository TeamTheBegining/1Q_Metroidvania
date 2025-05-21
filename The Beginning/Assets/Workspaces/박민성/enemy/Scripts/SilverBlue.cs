using System;
using UnityEngine;

public class SilverBlue : MonoBehaviour
{
    GameObject      player;
    Animator        animator;
    Rigidbody2D     rb;
    BoxCollider2D   boxCollider2D;

    float distance;
    float randomdir;
    bool  leftdir;
    
    [SerializeField] float findDistance     = 3.0f;
    [SerializeField] float moveChangetime   = 3.0f;
    [SerializeField] float idleChangetime   = 3.0f;
    [SerializeField] float attackRange      = 0.75f;

    float movespeed    = 1.0f;
    float chasingSpeed = 2.0f;

    bool isPlayerFind = false;

    float moveChangetimer;
    float idleChangetimer;

    public enum SilverBlueState
    {
        Idle,
        Move,
        Attack,
        Freeze,     // 패링 받았을 때
        Death
    }

    public SilverBlueState currentState = SilverBlueState.Idle;

    private void Awake()
    {
        animator        = GetComponent<Animator>();
        rb              = GetComponent<Rigidbody2D>();
        boxCollider2D   = GetComponent<BoxCollider2D>();
        moveChangetimer = 0;
        randomdir       = 1.0f;
        leftdir         = false;
        
    }

    private void OnEnable()
    {
        if (player == null) player = GameObject.FindWithTag("Player");
    }

    private void FixedUpdate()
    {
        distance = Vector2.Distance(transform.position, player.transform.position);
        leftdir = player.transform.position.x < transform.position.x ? true : false;

        switch (currentState)
        {
            case SilverBlueState.Idle:
                SilverBlueIdleUpdate();
                animator.Play("Idle");
                break;
            case SilverBlueState.Move:
                SilverBlueMoveUpdate();
                animator.Play("move");
                break;
            case SilverBlueState.Attack:
                SilverBlueAttackUpdate();
                animator.Play("attack");
                break;
            case SilverBlueState.Freeze:
                //SilverBlueFreezeUpdate();
                break;
            case SilverBlueState.Death:
                //SilverBlueDeathUpdate();
                break;
        }
    }


    private void SilverBlueIdleUpdate()
    {
        if (player != null)
        {
            if (distance < findDistance)
            {
                isPlayerFind = true;
                currentState = SilverBlueState.Move;
            }
            else
            {
                moveChangetimer += Time.deltaTime;
                if (moveChangetimer > moveChangetime)
                {
                    moveChangetimer = 0;
                    randomdir *= -1.0f;
                    currentState = SilverBlueState.Move;
                }
            }
        }
    }

    private void SilverBlueMoveUpdate()
    {
        if (isPlayerFind)
        {
            if (distance > attackRange)
            {
                int dir = leftdir ? -1 : 1;
                transform.position += Vector3.right * dir * chasingSpeed * Time.deltaTime;
            }
            else
            {
                currentState = SilverBlueState.Attack;
            }
        }
        else
        {
            // 아무 방향으로라도 이동
            transform.position += Vector3.right * randomdir * movespeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(0f, randomdir < 0 ? 180f : 0f, 0f);
            
            idleChangetimer += Time.deltaTime;
            if (idleChangetimer > idleChangetime)
            {
                idleChangetimer = 0;
                currentState = SilverBlueState.Idle;
            }
        }
    }
    private void SilverBlueAttackUpdate()
    {
        
    }
}
