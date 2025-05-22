using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene5Room1Door : MonoBehaviour
{
    private Animator animator;
    private Collider2D coll2d;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        coll2d = GetComponent<Collider2D>();
    }

    public void Open()
    {
        animator.Play("Open");
        coll2d.enabled = false;
    }
}
