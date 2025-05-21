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

    [SerializeField] private int remainCount;
    private int RemainCount
    {
        get => remainCount;
        set
        {
            remainCount = value;
            if(!isAnimatoinPlay && remainCount == 0)
            {
                Debug.Log($"-- {gameObject.name} 문 열림 --");
                animator.Play("Open");
                MapStateManager.Instance.SetIsScene3DoorOpened();
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
        if(MapStateManager.Instance.IsScene3DoorOpened)
        {
            this.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < targetObjects.Count; i++)
        {
            IDamageable target = targetObjects[i].GetComponentInChildren<IDamageable>();
            target.OnDead += () => { RemainCount--; };
        }
#if UNITY_EDITOR
        TestInit();
#endif
    }

#if UNITY_EDITOR

    [Space(20f)]
    [Header("Test Section")]
    public bool isDebug = false;
    private TestInputActions testActions;

    private void TestInit()
    {
        testActions = new TestInputActions();
        testActions.Enable();
        testActions.Test.Test1.performed += Test1_performed;
    }

    private void Test1_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        animator.Play("Open");
    }
#endif
}
