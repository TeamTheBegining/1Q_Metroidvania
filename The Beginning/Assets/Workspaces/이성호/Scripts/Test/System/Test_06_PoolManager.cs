using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_PoolManager : TestBase
{
    public Transform targetposition;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(PoolType.Hit1, targetposition.position);
    }
}
