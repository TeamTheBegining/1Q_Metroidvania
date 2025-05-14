using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestSkillUnlocker : MonoBehaviour
{
    public Image skillIcon;     // ��ų ���� �̹���

    public Sprite newSkillSprite;      // ������ ��ų ������

    private bool hasSkill = false;

    void Start()
    {
        // ó������ ��ų ������ ��Ȱ��ȭ (���� ó��)
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
            Debug.Log("��ų ���� �Ϸ�!");
            skillIcon.color = Color.white;
        }
    }
}