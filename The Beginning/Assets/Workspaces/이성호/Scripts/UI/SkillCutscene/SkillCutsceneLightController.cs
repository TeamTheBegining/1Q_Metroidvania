using UnityEngine;
using UnityEngine.UI;

public class SkillCutsceneLightController : MonoBehaviour
{
    Material material;
    public Vector2 offset = Vector2.zero;

    private void Awake()
    {
        material = GetComponent<Image>().material;
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
