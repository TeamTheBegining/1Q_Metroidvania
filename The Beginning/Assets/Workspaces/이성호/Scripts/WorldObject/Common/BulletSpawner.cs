using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public float maxTimer = 5f;
    private float timer = 0f;

    public bool isLeft = false;

    private void Update()
    {
        timer += Time.deltaTime;

        if(timer > maxTimer)
        {
            timer = 0f;
            // 풀링
            PoolManager.Instance.Pop<Bullet>(PoolType.ProjectileObstacle, transform.position).SetDirection(isLeft ? Vector3.left : Vector3.right, 5f);
        }
    }
}
