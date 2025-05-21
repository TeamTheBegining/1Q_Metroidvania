using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class PlayerHpUI : MonoBehaviour
{
    // Inspector���� ������ Slider
    public  Slider           hpSlider;
    public  Slider           mpSlider;
    public  List<Sprite>     appleimage;
    private Image           a_image;

    // PlayerStats Ÿ�� ��ũ��Ʈ���� ü�� �� ������
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        a_image = transform.Find("apple").GetComponent<Image>();        
        hpSlider.value = 1;
        mpSlider.value = 0;

        if (player == null) Debug.LogWarning($"{gameObject.name} ���� �÷��̾ ã�� �� ���� ");
    }
    void FixedUpdate()
    {
        // ���� ü�� / �ִ� ü�� ������ �����̴��� �ݿ�
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