using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions actions;

    private Vector2 inputVec;
    public Vector2 InputVec { get => inputVec; }

    bool isAttack = false;
    public bool IsAttack { get => isAttack; }

    bool isRoll = false;
    public bool IsRoll { get => isRoll; set => isRoll = value; }

    bool isJump = false;

    public bool IsJump { get => isJump; set => isJump = value; }


    bool isInteraction = false;
    public bool IsInteraction { get => isInteraction; }

    bool isSkill1 = false;
    public bool IsSkill1 { get => isSkill1; }

    bool isSkill2 = false;
    public bool IsSkill2 { get => isSkill2; }

    bool isSkill3 = false;
    public bool IsSkill3 { get => isSkill3; }

    bool isParrying = false;
    public bool IsParrying { get => isParrying; }

    bool isDodging = false;
    public bool IsDodging { get => isDodging; }

    bool isSliding = false;
    public bool IsSliding { get => isSliding; }


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
        actions.Player.Dodging.started += Dodging_started;
        actions.Player.Dodging.canceled += Dodging_canceled;
        actions.Player.Roll.started += Roll_started;
        actions.Player.Roll.canceled += Roll_canceled;
        actions.Player.Skill1.canceled += Skill1_started;
        actions.Player.Skill1.canceled += Skill1_canceled;
        actions.Player.Skill2.canceled += Skill2_started;
        actions.Player.Skill2.canceled += Skill2_canceled;
        actions.Player.Skill3.canceled += Skill3_started;
        actions.Player.Skill3.canceled += Skill3_canceled;
        actions.Player.Interaction.canceled += Interaction_started;
        actions.Player.Interaction.canceled += Interaction_canceled;
        actions.Player.Sliding.canceled += Sliding_started;
        actions.Player.Sliding.canceled += Sliding_canceled;

    }


    private void OnEnable()
    {
        actions.Player.Enable();

        actions.Player.Move.Enable();
        actions.Player.Attack.Enable();
        actions.Player.Jump.Enable();
        actions.Player.Parrying.Enable();
        actions.Player.Dodging.Enable();
        actions.Player.Skill1.Enable();
        actions.Player.Skill2.Enable();
        actions.Player.Skill3.Enable();
        actions.Player.Interaction.Enable();
        actions.Player.Sliding.Enable();
    }

    private void OnDisable()
    {
        actions.Player.Attack.Disable();
        actions.Player.Move.Disable();
        actions.Player.Jump.Disable();
        actions.Player.Parrying.Disable();
        actions.Player.Dodging.Disable();
        actions.Player.Skill1.Disable();
        actions.Player.Skill2.Disable();
        actions.Player.Skill3.Disable();
        actions.Player.Interaction.Disable();
        actions.Player.Sliding.Disable();

        actions.Player.Disable();
    }

    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = true;
        print("���� ����");
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isJump = false;
        print("���� ��");
    }

    private void Attack_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = true;
        print("���� ����");
    }

    private void Attack_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isAttack = false;
        print("���� ��");
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Vector2 value = obj.ReadValue<Vector2>();
        inputVec = value;
        print("�̵� �Է�: " + inputVec);
    }

    private void Move_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        inputVec = Vector2.zero;
        print("�̵� �Է� ����");
    }

    private void Interaction_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isInteraction = true;
        print("��ȣ�ۿ� ����");
    }

    private void Interaction_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isInteraction = false;
        print("��ȣ�ۿ� ��");
    }

    private void Skill1_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill1 = true;
        print("��ų1 ����");
    }

    private void Skill1_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill1 = false;
        print("��ų1 ��");
    }

    private void Skill2_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill2 = true;
        print("��ų2 ����");
    }

    private void Skill2_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill2 = false;
        print("��ų2 ��");
    }

    private void Skill3_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill3 = true;
        print("��ų3 ����");
    }

    private void Skill3_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSkill3 = false;
        print("��ų3 ��");
    }

    private void Parrying_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isParrying = true;
        print("�и� ����");
    }

    private void Parrying_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isParrying = false;
        print("�и� ��");
    }

    private void Roll_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isRoll = true;
        print("ȸ��(��) ����");
    }

    private void Roll_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isRoll = false;
        print("ȸ��(��) ��");
    }

    private void Dodging_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDodging = true;
        print("ȸ��(����) ����");
    }

    private void Dodging_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isDodging = false;
        print("ȸ��(����) ��");
    }

    private void Sliding_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSliding = true;
        print("�����̵� ����");
    }

    private void Sliding_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isSliding = false;
        print("�����̵� ��");
    }
}
