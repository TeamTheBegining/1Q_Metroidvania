using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions actions;

    private Vector2 inputVec;
    public Vector2 InputVec { get => inputVec; }
    
    bool isAttack = false;
    public bool IsAttack { get => isAttack; set => isAttack = value; }

    bool isRoll = false;
    public bool IsRoll { get => isRoll; set => isRoll = value; }

    bool isJump = false;

    public bool IsJump { get => isJump; set => isJump = value; }


    bool isInteraction = false;
    public bool IsInteraction { get => isInteraction; }

    bool isSkill1 = false;
    public bool IsSkill1 { get => isSkill1; set => isSkill1 = value; }

    bool isSkill2 = false;
    public bool IsSkill2 { get => isSkill2; set => isSkill2 = value; }

    bool isParrying = false;
    public bool IsParrying { get => isParrying; set => isParrying = value; }

    bool isSliding = false;

    public bool IsSliding { get => isSliding; set => isSliding = value; }


    bool isDashLeft = false;
    public bool IsDashLeft { get => isDashLeft; }

    bool isDashRight = false;
    public bool IsDashRight { get => isDashRight; }


    private void Awake()
    {
        actions = new PlayerInputActions();

        actions.Player.Move.performed += Move_performed;
        actions.Player.Move.canceled += Move_canceled;
        actions.Player.Attack.started += Attack_started;
        actions.Player.Attack.canceled += Attack_canceled;
        actions.Player.Jump.started += Jump_started;
        actions.Player.Jump.canceled += Jump_canceled;
        actions.Player.Parrying.started += Parrying_started;
        actions.Player.Parrying.canceled += Parrying_canceled;
        actions.Player.Skill1.started += Skill1_started;
        actions.Player.Skill1.canceled += Skill1_canceled;
        actions.Player.Skill2.started += Skill2_started;
        actions.Player.Skill2.canceled += Skill2_canceled;
        actions.Player.Interaction.started += Interaction_started;
        actions.Player.Interaction.canceled += Interaction_canceled;
        actions.Player.Sliding.started += Sliding_started;
        actions.Player.Sliding.canceled += Sliding_canceled;
        actions.Player.DashLeft.started += DashLeft_started;
        actions.Player.DashLeft.canceled += DashLeft_canceled;
        actions.Player.DashRight.started += DashRight_started;
        actions.Player.DashRight.canceled += DashRight_canceled;

    }

    private void OnEnable()
    {
        actions.Player.Enable();

        AllEnable();
    }

    private void OnDisable()
    {
        AllDisable();
        actions.Player.Disable();
    }
    public void AllDisable()
    {
        actions.Player.Attack.Disable();
        actions.Player.Move.Disable();
        actions.Player.Jump.Disable();
        actions.Player.Parrying.Disable();
        actions.Player.Skill1.Disable();
        actions.Player.Skill2.Disable();
        actions.Player.Interaction.Disable();
        actions.Player.Sliding.Disable();
    }
    public void AllEnable()
    {
        actions.Player.Attack.Enable();
        actions.Player.Move.Enable();
        actions.Player.Jump.Enable();
        actions.Player.Parrying.Enable();
        actions.Player.Skill1.Enable();
        actions.Player.Skill2.Enable();
        actions.Player.Interaction.Enable();
        actions.Player.Sliding.Enable();
    }

    public void OneEnable(int idx)
    {
        switch(idx)
        {
            case 0:
                actions.Player.Attack.Enable();
                break;
            case 1:
                actions.Player.Parrying.Enable();
                break;
            case 2:
                actions.Player.Skill1.Enable();
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
        }
    }


    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = true;
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = false;
    }

    private void Attack_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = true;
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = false;
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Vector2 value = obj.ReadValue<Vector2>();
        inputVec = value;
    }

    private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        inputVec = Vector2.zero;
    }

    private void Interaction_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isInteraction = true;
    }

    private void Interaction_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isInteraction = false;
    }

    private void Skill1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill1 = true;
    }

    private void Skill1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill1 = false;
    }

    private void Skill2_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill2 = true;
    }

    private void Skill2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill2 = false;
    }

    private void Parrying_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isParrying = true;
    }

    private void Parrying_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isParrying = false;
    }
    private void Sliding_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSliding = true;
    }

    private void Sliding_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSliding = false;
    }

    private void DashLeft_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDashLeft = false;
        //print("왼쪽 대쉬 끝");
    }

    private void DashLeft_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (obj.performed) Debug.Log("mtap");

        isDashLeft = true;
        //print("왼쪽 대쉬 시작");
    }

    private void DashRight_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDashRight = false;
        //print("대쉬 끝");
    }

    private void DashRight_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (obj.performed) Debug.Log("mtap");

        isDashRight = true;
        //print("대쉬 시작");
    }
}
