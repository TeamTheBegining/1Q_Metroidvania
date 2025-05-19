using UnityEngine;
using UnityEngine.Rendering;

public class SkillEffectMove : MonoBehaviour//, IPoolable
{
    /*public void OnSpawn();
    public void OnDespawn();
    Action ReturnAction { get; set; }*/

    [SerializeField]float MoveSpeed;
    [SerializeField]AnimationCurve anicurv;
    AnimatorStateInfo stateInfo;
    Animator ani;
    private void Start()
    {
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        stateInfo = ani.GetCurrentAnimatorStateInfo(0);
        MoveSpeed = anicurv.Evaluate(stateInfo.normalizedTime);
        transform.position += Vector3.right * MoveSpeed * Time.deltaTime;
    }
}
