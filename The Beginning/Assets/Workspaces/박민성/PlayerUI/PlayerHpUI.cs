using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class PlayerHpUI : MonoBehaviour
{
    // Inspector에서 연결할 Slider
    public  Slider           hpSlider;
    public  Slider           mpSlider;
    public  List<Sprite>     appleimage;
    private Image           a_image;

    // PlayerStats 타입 스크립트에서 체력 값 가져옴
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        a_image = transform.Find("apple").GetComponent<Image>();        
        hpSlider.value = 1;
        mpSlider.value = 0;

        if (player == null) Debug.LogWarning($"{gameObject.name} 에서 플레이어를 찾을 수 없음 ");
    }
    void FixedUpdate()
    {
        // 현재 체력 / 최대 체력 비율을 슬라이더에 반영
        if(player != null)
        {
            hpSlider.value = (float)player.CurrentHp / player.MaxHp;
            mpSlider.value = (float)player.CurrentMp / player.MaxMp;
            int count = player.CurParryCount > 3 ? 3 : player.CurParryCount;  // switch

            if (count == 0)
            {
                a_image.enabled = false;
            }
            else
            {
                a_image.enabled = true;
                a_image.sprite = appleimage[count - 1];
            }               
        }

        
    }
}