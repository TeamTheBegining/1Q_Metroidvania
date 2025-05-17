using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 플레이어를 따라 다니는 쉐이더 빛 
/// </summary>
public class PlayerShaderLight : MonoBehaviour
{
    GameObject playerObject;
    Material material;
    public Vector2 offset = Vector2.zero;

    private void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    private void Start()
    {
        playerObject = GameObject.FindWithTag("Player");
    }

    private void LateUpdate()
    {
        if(playerObject != null)
        {
            transform.position = playerObject.transform.position + (Vector3)offset;
        }
    }

    public void SetPlayer(GameObject obj)
    {
        playerObject = obj;
    }

    /// <summary>
    /// 머터리얼의 DarknessStrength 값을 바꾸는 함수 ( 적을 수록 더 많은 빛 -> 머터리얼이 사라짐 ) 
    /// </summary>
    /// <param name="range"></param>
    public void SetRange(float range = 1f)
    {
        material.SetFloat("_DarknessStrength", range);
    }

    public float GetCurrentRange()
    {
        return material.GetFloat("_DarknessStrength");
    }

    public void SetShadowActive(bool value)
    {
        this.gameObject.SetActive(value);
    }
}