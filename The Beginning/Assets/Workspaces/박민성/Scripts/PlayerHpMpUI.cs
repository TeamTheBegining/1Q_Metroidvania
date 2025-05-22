using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;
using static System.Net.Mime.MediaTypeNames;

public class PlayerHpMpUI : MonoBehaviour
{
    // Inspector에서 연결할 Slider
    public Slider hpSlider;
    public Slider mpSlider;
    public TextMeshProUGUI text;

    // PlayerStats 타입 스크립트에서 체력 값 가져옴
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        text = transform.Find("ParryingCount").GetComponent<TextMeshProUGUI>();
        hpSlider.value = 1;
        mpSlider.value = 0;

        if (player == null) Debug.LogWarning($"{gameObject.name} 에서 플레이어를 찾을 수 없음 ");
    }
    void FixedUpdate()
    {
        // 현재 체력 / 최대 체력 비율을 슬라이더에 반영
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
                case 3:
                    text.color = new Color(1.0f, 0f, 0f, 1.0f);
                    break;
            }
        }
        else
        {
            player = GameObject.FindWithTag("Player").GetComponent<Player>();
        }
    }
}