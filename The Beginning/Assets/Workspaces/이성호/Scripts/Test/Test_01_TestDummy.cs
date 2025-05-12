using UnityEngine;
using UnityEngine.InputSystem;

public class Test_01_TestDummy : TestBase
{
    public TestDummy testDummy;
    public float damage = 1f;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        IDamageable target = testDummy.GetComponent<IDamageable>();
        target.TakeDamage(damage);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        testDummy.ResetHp();
    }
}
