using System;
using UnityEditor;
using UnityEngine;

public class EffectObject : MonoBehaviour, IPoolable
{
    private Animator animator;
    private Player player;
    float MoveSpeed;
    AnimatorStateInfo stateInfo;
    SpriteRenderer spriteRenderer;
    int state = 1;
    [SerializeField] float posXmax = 1f;
    Vector3 maxpos = Vector3.zero;
    bool iscol = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    private void OnEnable()
    {
        maxpos = transform.position + new Vector3(posXmax * state, 0, 0);
    }

    public Action ReturnAction { get; set; }

    private void FixedUpdate()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        MoveSpeed = 25;

        if (maxpos.x > transform.position.x && !iscol)
            transform.position += Vector3.right * state * MoveSpeed * Time.deltaTime;

        if (IsAnimationEnd())
        {
            gameObject.SetActive(false);
            iscol = false;
            player.CurrentState = Player.PlayerState.Idle;
        }
    }

    public void Init(bool isFilp = false)
    {
        spriteRenderer.flipX = isFilp;
        state = isFilp ? -1 : 1;
    }

    private void OnDisable()
    {
        ReturnAction?.Invoke();
    }

    public void OnDespawn()
    {
    }

    public void OnSpawn()
    {
    }

    private bool IsAnimationEnd()
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
    private void OnTriggerEnter(Collider other)
    {
        transform.position = other.transform.position;
        iscol = true;
    }
}
