using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Windows;
//using static Player;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float aniSpeed;
    public float AniSpeed
    {
        get => aniSpeed; 
        set => aniSpeed = value;
    }

    private Player player;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateState();
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        animator.speed = aniSpeed;
        switch (player.CurrentState)
        {
            case Player.PlayerState.Idle:
                animator.Play("Idle");
                break;
            case Player.PlayerState.Move:
                animator.Play("Move");
                break;
            case Player.PlayerState.Jump:
                animator.Play("Jump");
                break;
            case Player.PlayerState.Landing:
                animator.Play("Landing");
                break;
            case Player.PlayerState.Attack:
                //animator.Play("Attack");
                break;
            case Player.PlayerState.Skill1:
                //animator.Play("Skill1");
                break;
            case Player.PlayerState.Skill2:
                //animator.Play("Skill2");
                break;
            case Player.PlayerState.Skill3:
                //animator.Play("Skill3");
                break;
            case Player.PlayerState.Parrying:
                animator.Play("Parrying");
                break;
            case Player.PlayerState.ParryingCounterAttack:
                //animator.Play("ParryingCounterAttack");
                break;
            case Player.PlayerState.ParryingReflect:
                //animator.Play("ParryingReflect");
                break;
            case Player.PlayerState.ParryingKnockback:
                //animator.Play("ParryingKnockback");
                break;
            case Player.PlayerState.Ladder:
                animator.Play("Ladder");
                break;
            case Player.PlayerState.Dodging:
                //animator.Play("Dodge");
                break;
            case Player.PlayerState.Dash:
                //animator.Play("Dash");
                break;
        }
    }

    private void UpdateState()
    {
        
    }
}
