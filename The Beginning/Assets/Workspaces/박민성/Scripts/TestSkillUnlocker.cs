using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSkillUnlocker : MonoBehaviour
{
    public Image skillIcon;     // 스킬 슬롯 이미지

    public Sprite newSkillSprite;      // 습득할 스킬 아이콘

    private bool hasSkill = false;

    void Start()
    {
        // 처음에는 스킬 아이콘 비활성화 (투명 처리)
        if (skillIcon != null)
            skillIcon.sprite = null;
            skillIcon.color = new Color(1, 1, 1, 0.1f);
    }

    void Update()
    {
        if (!hasSkill && Input.GetKeyDown(KeyCode.K))
        {
            AcquireSkill();
        }
    }

    void AcquireSkill()
    {
        if (skillIcon != null && newSkillSprite != null)
        {
            skillIcon.sprite = newSkillSprite;
            skillIcon.enabled = true;
            hasSkill = true;
            Debug.Log("스킬 습득 완료!");
            skillIcon.color = Color.white;
        }
    }
}