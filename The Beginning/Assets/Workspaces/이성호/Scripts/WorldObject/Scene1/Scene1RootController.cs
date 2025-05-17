using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene1RootController : MonoBehaviour
{
    private Collider2D coll;

    private void Awake()
    {
        coll = GetComponent<Collider2D>();
        coll.enabled = false;
    }

    public void ColliderActive()
    {
        coll.enabled = true;
    }
}
