using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;
using static System.Net.Mime.MediaTypeNames;

public class PlayerHpMpUI : MonoBehaviour
{
    // Inspector���� ������ Slider
    public Slider hpSlider;
    public Slider mpSlider;
    public TextMeshProUGUI text;

    // PlayerStats Ÿ�� ��ũ��Ʈ���� ü�� �� ������
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        text = transform.Find("ParryingCount").GetComponent<TextMeshProUGUI>();
        hpSlider.value = 1;
        mpSlider.value = 0;

        if (player == null) Debug.LogWarning($"{gameObject.name} ���� �÷��̾ ã�� �� ���� ");
    }
    void FixedUpdate()
    {
        // ���� ü�� / �ִ� ü�� ������ �����̴��� �ݿ�
        if (player != null)
        {
            hpSlider.value = (float)player.CurrentHp / player.MaxHp;
            mpSlider.value = (float)player.CurrentMp / player.MaxMp;
            int count = player.CurParryCount > 3 ? 3 : player.CurParryCount;  // switch
            text.text = player.CurParryCount == 0 ? "" : $"X{count}";

            switch (count)
            {
                case 0:
                    break;
                case 1:
                    text.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    break;
                case 2:
                    text.color = new Color(255, 200, 0, 255);
                    break;
                case 3:                                                     // parrying 3 �ܰ� ������ �󸶳� ��?
                    text.color = new Color(1.0f, 0f, 0f, 1.0f);
                    break;
            }
        }

    }
}