using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// target 사망하면 애니메이션 실행하는 스크립트
/// </summary>
[RequireComponent(typeof(Animator))]
public class Door : MonoBehaviour
{
    [Tooltip("직접 트리거가 될 오브젝트 넣어두기")]
    public List<GameObject> targetObjects;
    private Animator animator;

    private int remainCount;
    private int RemainCount
    {
        get => remainCount;
        set
        {
            remainCount = value;
            if(!isAnimatoinPlay && remainCount == 0)
            {
                Debug.Log($"-- {gameObject.name} 문 열림 --");
                animator.Play("Open"); // TODO : 애니메이터에 애니메이션 클립 추가 확인
                isAnimatoinPlay = true;
            }
        }
    }

    private bool isAnimatoinPlay = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        remainCount = targetObjects.Count;
    }

    private void Start()
    {
        for (int i = 0; i < targetObjects.Count; i++)
        {
            IDamageable target = targetObjects[i].GetComponent<IDamageable>();
            target.OnDead += () => { RemainCount--; };
        }
    }
}
