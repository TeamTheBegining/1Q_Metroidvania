using UnityEngine;
using UnityEngine.InputSystem;

public class Test_03_PopUpUI : TestBase
{
#if UNITY_EDITOR

    [Range(0f,1f)]
    public float posX;

    [Range(0f,1f)]
    public float posY;

    public PopUpPanel popUI;

    public string titleName;
    public string description;
    public PopUpShowtype type;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        popUI.ShowPopUp(new Vector2(posX, posY), titleName, description, 2f, type);
    }
#endif
} 