using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_PlayerDeadUI : TestBase
{
    public PlayerDeadPanel panel;
    public Player player;

    private void Start()
    {
        panel.Init();
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        GameManager.Instance.State = GameState.Play;
        player.CurrentHp = 0;
    }
}