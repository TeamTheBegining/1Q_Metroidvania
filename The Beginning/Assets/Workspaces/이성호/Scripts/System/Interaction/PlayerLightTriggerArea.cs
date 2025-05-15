using System.Collections;
using UnityEngine;

/// <summary>
/// 트리거 내에 플레이어가 들어왔을 때 target값으로 빛 프로퍼티 값을 바꾸는 클래스
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerLightTriggerArea : MonoBehaviour
{
    [Tooltip("트리거 되었을 때 바뀔  값")]
    public float targetValue;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 속도")]
    public float speed = 1f;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isTriggered && collision.CompareTag("Player")) // 플레이어가 오면 발동
        {
            LightManager.Instance.SpreadPlayerLight(targetDuration, targetValue, speed);
            isTriggered = true;
        }
    }
}