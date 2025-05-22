using System;
using System.Collections;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

public class LastBoss1 : MonoBehaviour, IDamageable
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
    private bool isLeft = false;
    private Vector3 randomSpecialPos;

    private Vector3 targetWorldPos;

    AnimatorStateInfo stateInfo;
    GameObject line;


    float changeColorTimer = 0f;
    float attackTimer = 0f;
    int attack3count;
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
        player = GameObject.FindWithTag("Player");// 추후 삭제
    }

    void FixedUpdate()
    {
        attackTimer += Time.deltaTime;
        stateInfo = bossani.GetCurrentAnimatorStateInfo(0);
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
                bossani.Play("Attack3");
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
            currentState = LastBossState.Move;
            SetRandomSidePosition();
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
            Vector3 destination;

            if (currentState == LastBossState.Attack1 || currentState == LastBossState.Attack2)
            {
                destination = targetWorldPos;
            }
            else if (currentState == LastBossState.Attack3)
            {
                destination = randomSpecialPos;
            }
            else
            {
                return; // 다른 상태면 이동 안함
            }

            transform.position = Vector3.Lerp(transform.position, destination, Time.deltaTime * 2f);

            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                transform.position = destination;

                // 방향에 따른 회전 (좌우만 필요하면 Attack1,2만)
                if (currentState == LastBossState.Attack1 || currentState == LastBossState.Attack2)
                {
                    Vector3 dir = targetWorldPos - transform.position;
                    if (dir.x < 0)
                        transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    else
                        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                }

                attackTimer = 0;
                movetime = Mathf.PI / 2f;
                addSpeed = 2;

                // 다음 상태 정하는 로직 (예시)
                if (currentState == LastBossState.Attack1 || currentState == LastBossState.Attack2)
                {
                    SetRandomSidePosition(); // 좌우 위치 다시 설정
                }
                else if (currentState == LastBossState.Attack3)
                {
                    SetRandomSpecialPosition(); // 특수 위치 새로 설정
                }

                // 상태 전환 (필요 시)
                // currentState = 다음 상태;
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
            
            Debug.DrawLine(centerPos, centerPos + offset, Color.red);
        }
    }

    void SetRandomSidePosition()
    {
        Vector3 camPos = Camera.main.transform.position;
        float y = -0.27f;
        bool isLeft = Random.value < 0.5f;

        if (isLeft)
        {
            targetWorldPos = camPos + new Vector3(-2.5f, y, 0f);
            centerPos = targetWorldPos + new Vector3(3f, 0f, 0f);
        }
        else
        {
            targetWorldPos = camPos + new Vector3(2.5f, y, 0f);
            centerPos = targetWorldPos - new Vector3(3f, 0f, 0f);
        }

        targetWorldPos.z = 0;
    }

    // Attack3용 특수 위치 랜덤 설정 예시
    void SetRandomSpecialPosition()
    {
        // 예: 화면 좌우 범위 내 랜덤 위치, y는 고정
        Vector3 camPos = Camera.main.transform.position;
        float xRange = 3f;
        float y = 1.0f; // 원하는 y값

        float randomX = camPos.x + Random.Range(-xRange, xRange);
        randomSpecialPos = new Vector3(randomX, y, 0f);
    }

    private void Attack1Update()
    {
        if (lr.enabled == false)
            lr.enabled = true;
        addSpeed *= 1.1f;
        lerpT += Time.deltaTime * addSpeed;
        float t = lerpT / 5;
        lr.widthMultiplier = 0.05f * (1- t);
        if (t > 1) t = 1f;
        Vector3 startPos = new Vector3(line.transform.position.x, !isLeft?laserLeftPos.position.y : laserRightPos.position.y, 0);
        Vector3 currentPos = Vector3.Lerp(startPos, !isLeft ? laserLeftPos.position : laserRightPos.position, t);
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

        
        if (Vector3.Distance(currentPos, !isLeft ? laserLeftPos.position : laserRightPos.position) <0.1f)
        {
            lr.enabled = false;
            playerhit = false;
            transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * 2);
            if (Vector2.Distance(transform.position, targetWorldPos) < 0.01f)
            {
                transform.position = targetWorldPos;
                currentState = LastBossState.Move;
                SetRandomSidePosition();
                movetime = Mathf.PI / 2f;
                lerpT = 0;
            }
        }
    }

    private void Attack2Update()
    {
        if (stateInfo.normalizedTime >= 0.95f && stateInfo.IsName("Attack2"))
        {
            currentState = LastBossState.Move;
            SetRandomSidePosition();
        }

    }

    private void Attack3Update()
    {
        if (attack3count < 4)
        {
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
