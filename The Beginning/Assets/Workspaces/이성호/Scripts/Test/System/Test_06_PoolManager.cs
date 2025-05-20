using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_06_PoolManager : TestBase
{
    public Transform targetposition;
    public float duration = 0.25f;
    public float spawnDelay = 0.25f;

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(PoolType.Hit1, targetposition.position);
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        StartCoroutine(PopProcess());
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(PoolType.ProjectilePlayer, targetposition.position);
    }

    protected override void OnTest4(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(PoolType.ProjectileEnemy, targetposition.position);
    }

    protected override void OnTest5(InputAction.CallbackContext context)
    {
        PoolManager.Instance.Pop(PoolType.UltEffect, targetposition.position);
    }

    private IEnumerator PopProcess()
    {
        PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, targetposition.position).Init(0);
        yield return new WaitForSeconds(spawnDelay);
        PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, targetposition.position + Vector3.right * 0.5f).Init(1);
        yield return new WaitForSeconds(spawnDelay);
        PoolManager.Instance.Pop<PlayerSlideAfterImage>(PoolType.PlayerSlideAfterImage, targetposition.position + Vector3.right).Init(2);
    }
}
