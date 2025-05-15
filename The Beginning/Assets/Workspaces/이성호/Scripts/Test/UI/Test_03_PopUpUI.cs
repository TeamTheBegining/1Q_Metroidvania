using UnityEngine;
using UnityEngine.InputSystem;

public class Test_03_PopUpUI : TestBase
{
    [Range(0f,1f)]
    public float posX;

    [Range(0f,1f)]
    public float posY;

    public PopUpPanel popUI;

    public string titleName;
    public string description;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        //popUI.ShowPopUp(titleName, description, 3f);
    }
} 