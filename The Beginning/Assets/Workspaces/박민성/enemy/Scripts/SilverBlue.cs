using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class SilverBlue : MonoBehaviour
{
    public enum SilverBlueState
    {
        Idle,
        Run,
        Attack,
        Damaged,
        Freeze,
        Death
    }

    public SilverBlueState currentState = SilverBlueState.Idle;

    private void Start()
    {
         
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case SilverBlueState.Idle:
                //SilverBlueIdleUpdate();
                break;
            case SilverBlueState.Run:
                //SilverBlueRunUpdate();
                break;
            case SilverBlueState.Attack:
                //SilverBlueAttackUpdate();
                break;
            case SilverBlueState.Damaged:
                //SilverBlueDamagedUpdate();
                break;
            case SilverBlueState.Freeze:
                //SilverBlueFreezeUpdate();
                break;
            case SilverBlueState.Death:
                //SilverBlueDeathUpdate();
                break;
        }
    }

    //private void SilverBlueIdleUpdate()
    //{
    //    RunAble();
    //    AttackAble();
    //    DamagedAble();
    //    FreezeAble();
    //    DeathAble();
    //}

}
