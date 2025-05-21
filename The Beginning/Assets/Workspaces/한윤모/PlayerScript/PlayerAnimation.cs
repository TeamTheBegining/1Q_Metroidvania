using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Windows;
//using static Player;

public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float aniSpeed;
    AnimatorStateInfo stateInfo;
        
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
        animator.Play("Idle");
        aniSpeed = 1;
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
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
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
            case Player.PlayerState.DoubleJump:
                animator.Play("DoubleJump");
                break;
            case Player.PlayerState.Landing:
                animator.Play("Landing");
                break;
            case Player.PlayerState.Charging:
                animator.Play("Charging");
                break;
            case Player.PlayerState.ChargingAttack:
                animator.Play("ChargingAttack");
                break;
            case Player.PlayerState.Attack1:
                animator.Play("Attack1");
                if (stateInfo.normalizedTime >= 1 && stateInfo.IsName("Attack1")) player.CurrentState = Player.PlayerState.Idle;
                break;
            case Player.PlayerState.Attack2:
                animator.Play("Attack2");
                break;
            case Player.PlayerState.Attack3:
                animator.Play("Attack3");
                break;
            case Player.PlayerState.Skill1:
                animator.Play("Skill1");
                if (stateInfo.normalizedTime >= 1 && stateInfo.IsName("Skill1")) player.CurrentState = Player.PlayerState.Idle;
                break;
            case Player.PlayerState.Skill2:
                animator.Play("Skill2");
                break;
            case Player.PlayerState.Skill2CutScene:
                animator.Play("Skill2CutScene");
                break;
            case Player.PlayerState.Parrying:
                animator.Play("Parrying");
                break;
            case Player.PlayerState.ParrySuccess:
                animator.Play("ParrySuccess");
                break;
            case Player.PlayerState.ParryCounterAttack:
                animator.Play("ParryCounterAttack");
                break;
            case Player.PlayerState.ParryReflect:
                //animator.Play("ParryReflect");
                break;
            case Player.PlayerState.ParryKnockback:
                //animator.Play("ParryKnockback");
                break;
            case Player.PlayerState.Ladder:
                animator.Play("Ladder");
                break;
            case Player.PlayerState.Sliding:
                animator.Play("Sliding");
                break;
            case Player.PlayerState.Climbing:
                animator.Play("Climbing");
                break;
            case Player.PlayerState.Grab:
                animator.Play("Grab");
                break;
            case Player.PlayerState.GrabSuccess:
                animator.Play("GrabSuccess");
                break;
            case Player.PlayerState.Dash:
                //animator.Play("Dash");
                break;
            case Player.PlayerState.Hit:
                animator.Play("Hit");
                //if(stateInfo.IsName("Hit") && stateInfo.normalizedTime >= 1) player.PlayerHitFinish();
                break;
            case Player.PlayerState.Dead:
                animator.Play("Dead");
                break;
            case Player.PlayerState.Crouch:
                animator.Play("Crouch");
                break;
        }
        //슬라이딩 딜레이 없어서 나오는 예외사항 발생 시 Idle로 초기화
        //if (stateInfo.IsName("Sliding") &&stateInfo.normalizedTime > 1) player.CurrentState = Player.PlayerState.Idle;
    }

    private void UpdateState()
    {
        
    }
}
