using System;
using UnityEngine;

//PlayerInput 이 없다면 추가
[RequireComponent(typeof(PlayerInput))]

public class Play : MonoBehaviour//, IDamageable
{


    private PlayerInput input;
    Rigidbody2D rd;
    SpriteRenderer spriteRenderer;
    [SerializeField]float speed = 3f;
    [SerializeField]float power = 5f;
    Transform groundCheckTransform;
    [SerializeField] float groundCheckRadius = 0.1f;
    LayerMask groundLayer;

    public PlayerInput Input { get => input; }

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        rd = GetComponent<Rigidbody2D>();
        groundCheckTransform = transform.GetChild(0).transform;
        groundLayer = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        bool isGround = CheckIsGround();
        print(groundCheckTransform.position);
        if (input.InputVec.x != 0)
        {
            rd.linearVelocity = new Vector2(input.InputVec.x * speed, rd.linearVelocity.y);
            //if (input.InputVec.x<0) spriteRenderer.flipX = false;
        }
        else
            rd.linearVelocity = new Vector2(0f, rd.linearVelocity.y);

        if (input.IsJump && isGround)
        {
            rd.AddForce(Vector2.up * power, ForceMode2D.Impulse);
            input.IsJump = false;
        }
    }


    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (groundCheckTransform == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
#endif
    }

}
