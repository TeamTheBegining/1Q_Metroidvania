using System;
using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class LastBoss : MonoBehaviour, IDamageable
{
    [SerializeField] private LastBossState currentState;
    private Animator bossani;
    private LineRenderer lr;
    private GameObject player;
    private SpriteRenderer spr;
    private Color originalColor;
    private float movetime = 0f;
    private Vector3 centerPos; // 무한대 곡선의 중심 위치
    private bool isChangeColor = false;
    private bool playerhit = false;

    private Vector3 targetWorldPos;
    private Collider2D bodycol;
    private Collider2D anicol;


    AnimatorStateInfo stateInfo;
    GameObject line;


    float changeColorTimer = 0f;
    float attack3timer = 0f;
    private bool isend = false;
    float attackTimer = 0f;
    int attack3count = 0;
    int attackcount = 0;
    //[SerializeField]float attack3Timer = 0f;


    [SerializeField]float upSpeed = 0.1f;
    [SerializeField]float moveSpeed = 1.5f;
    [SerializeField]float addSpeed = 2f;
    [SerializeField]float lerpT = 0f;
    [SerializeField]float damage = 3f;
    [SerializeField]float changeColorTime = 0.1f;
    [SerializeField]float attackTime = 3f;
    //[SerializeField]float attack3Time = 1f;



    public Transform laserLeftPos;
    public Transform laserRightPos;
    public enum LastBossState
    {
        WaitingForPlayer,
        Start,
        Idle,
        Move,
        Attack1,
        Attack2,
        Attack3,
        Dead
    }

    float currentHp = 100;
    float maxHp = 100;
    public float CurrentHp { get => currentHp; set => currentHp = value; }
    public float MaxHp { get => maxHp; set => maxHp = value; }
    public Action OnDead { get; set; }
    public bool IsDead { get; }

    public void TakeDamage(float damage, GameObject attackObject)
    {
        currentHp -= damage;
        if (currentHp < 0)
        {
            OnDead?.Invoke();
            return;
        }
        spr.color = Color.red;
        isChangeColor = true;


    }

    private void HitCheck()
    {
        changeColorTimer += Time.deltaTime;
        if (changeColorTimer>changeColorTime)
        {
            spr.color = originalColor;
            changeColorTimer = 0;
        }
    }
    public void FindPlayer()
    {
        player = GameObject.FindWithTag("Player");
    }
    private void Awake()
    {
        currentState = LastBossState.WaitingForPlayer;
        bossani = GetComponentInChildren<Animator>();
        spr = GetComponentInChildren<SpriteRenderer>();
        line = transform.Find("Laser").GameObject();
        lr = line.GetComponent<LineRenderer>();
        originalColor = spr.color;
        attack3count = 0;
        bodycol = GetComponent<BoxCollider2D>();
        anicol = transform.Find("Animation").GetComponent<BoxCollider2D>();
        anicol.enabled = false;
    }

    void FixedUpdate()
    {
        attackTimer += Time.deltaTime;
        stateInfo = bossani.GetCurrentAnimatorStateInfo(0);
        targetWorldPos = Camera.main.transform.position + new Vector3(2.5f, -0.27f, 0f);
        targetWorldPos = new Vector3(targetWorldPos.x, targetWorldPos.y,0);
        centerPos = targetWorldPos - new Vector3(3, 0, 0);
        HitCheck();
        switch (currentState)
        {
            case LastBossState.WaitingForPlayer:
                WaitingForPlayerUpdate();
                break;
            case LastBossState.Start:
                StartUpdate();
                bossani.Play("Start");
                break;
            case LastBossState.Idle:
                IdleUpdate();
                bossani.Play("Idle");
                break;
            case LastBossState.Move:
                MoveUpdate();
                bossani.Play("Move");
                break;
            case LastBossState.Attack1:
                Attack1Update();
                bossani.Play("Move");
                break;
            case LastBossState.Attack2:
                Attack2Update();
                bossani.Play("Attack2");
                break;
            case LastBossState.Attack3:
                Attack3Update();
                break;
            case LastBossState.Dead:
                DeadUpdate();
                bossani.Play("Dead");
                break;
        }

    }
    float GetDistanceToPlayer()
    {
        if(player != null)
            return Vector3.Distance(transform.position, player.transform.position);
        return 999;
    }
    private void WaitingForPlayerUpdate()
    {
        if(GetDistanceToPlayer() < 5.0f)
        {
            currentState = LastBossState.Start;
        }
    }

    private void StartUpdate()
    {
        if (transform.position.y < -0.3f)
            transform.position += Vector3.up * Time.deltaTime * upSpeed;

        if (stateInfo.normalizedTime >= 0.99f && stateInfo.IsName("Start"))
        {
            movetime = Mathf.PI / 2f;
            currentState = LastBossState.Move;
        }
    }

    private void IdleUpdate()
    {
        
    }

    private void MoveUpdate()
    {
        lr.enabled = false;
        if (attackTimer > attackTime)
        {
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 2);
            if (Vector2.Distance(transform.position, targetWorldPos) < 0.01f)
            {
                transform.position = targetWorldPos;
                switch (attackcount%3)
                {
                    case 0:
                        currentState = LastBossState.Attack1;
                        break;
                    case 1:
                        currentState = LastBossState.Attack2;
                        break;
                    case 2:
                        currentState = LastBossState.Attack3;
                        break;
                }
                attackcount++;
                movetime = Mathf.PI / 2f;
                attackTimer = 0;
                addSpeed = 2;
                ;
            }
        }
        else
        {
            movetime += Time.deltaTime * moveSpeed;

            // 무한대 모양 경로 공식 (Lemniscate 형태)
            float x = 3 * Mathf.Sin(movetime);
            float y = 3 * Mathf.Sin(movetime) * Mathf.Cos(movetime);

            Vector3 offset = new Vector3(x, y, 0);
            transform.position = centerPos + offset; 
        }
    }

    private void Attack1Update()
    {
        bodycol.enabled = true;
        anicol.enabled = false;
        if (lr.enabled == false)
            lr.enabled = true;
        addSpeed *= 1.1f;
        lerpT += Time.deltaTime * addSpeed;
        float t = lerpT / 5;
        lr.widthMultiplier = 0.05f * (1- t);
        if (t > 1) t = 1f;
        Vector3 startPos = new Vector3(line.transform.position.x, laserLeftPos.position.y, 0);
        Vector3 currentPos = Vector3.Lerp(startPos, laserLeftPos.position, t);
        lr.SetPosition(0, line.transform.position);
        lr.SetPosition(1, currentPos);
        // Raycast 방향과 거리
        Vector2 direction = (currentPos - line.transform.position).normalized;
        float distance = Vector2.Distance(line.transform.position, currentPos);

        if(!playerhit)
        {
            // 충돌 검사 (2D Raycast)
            RaycastHit2D hit = Physics2D.Raycast(line.transform.position, direction, distance, LayerMask.GetMask("Player"));
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                if (hit.collider.gameObject.GetComponent<IDamageable>() != null)
                {
                    hit.collider.gameObject.GetComponent<IDamageable>().TakeDamage(damage, gameObject);
                    playerhit = true;
                }
            }
        }

        if(Vector3.Distance(currentPos, laserLeftPos.position)<0.1f)
        {
            lr.enabled = false;
            playerhit = false;
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 2);
            if (Vector2.Distance(transform.position, targetWorldPos) < 0.01f)
            {
                transform.position = targetWorldPos;
                currentState = LastBossState.Move;
                movetime = Mathf.PI / 2f;
                lerpT = 0;
            }
        }
    }

    private void Attack2Update()
    {
        bodycol.enabled = false;
        anicol.enabled = true;
        if (stateInfo.normalizedTime >= 0.99f && stateInfo.IsName("Attack2"))
        {
            movetime = Mathf.PI / 2f;
            currentState = LastBossState.Move;
        }
    }

    private void Attack3Update()
    {
        bodycol.enabled = true;
        anicol.enabled = false;
        if (attack3count < 4)
        {
            bossani.Play("Attack3");
            Camera cam = Camera.main;
            float y = 0f; // Thunder의 Y 위치 (필요에 따라 플레이어 위치나 고정값 사용)

            for (int i = 0; i < 4; i++) 
            {
                float viewportX = (i + 0.5f) / 4f; // 0.125, 0.375, 0.625
                Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(viewportX, 0.5f, cam.nearClipPlane));
                worldPos.y = y;
                worldPos.z = 0f;

                PoolManager.Instance.Pop<Thunder>(PoolType.Thunder, worldPos);
                attack3count++;
            }
        }

        if (transform.position.y > -2.25f &&!isend)
            transform.position += Vector3.down * Time.deltaTime * moveSpeed / 2;
        else
        {
            if (attack3timer > 5)
            {
                bossani.Play("Move");
                isend = true;
                transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 2);
                if (Vector2.Distance(transform.position, targetWorldPos) < 0.01f)
                {
                    transform.position = targetWorldPos;
                    currentState = LastBossState.Move;
                    movetime = Mathf.PI / 2f;
                    lerpT = 0;
                    attack3timer = 0;
                    isend = false;
                }
            }
            else
            {
                transform.position = new Vector3(transform.position.x, -2.25f, transform.position.z);
                attack3timer += Time.deltaTime;
            }
        }
    }

    private void DeadUpdate()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<IDamageable>() != null)
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(damage, gameObject);
        }
    }
}
