using UnityEngine;
using UnityEngine.UI;
using static Player;

public class PlayerHpUI : MonoBehaviour
{
    // Inspector���� ������ Slider
    public Slider hpSlider;
    public Slider mpSlider;

    // PlayerStats Ÿ�� ��ũ��Ʈ���� ü�� �� ������
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        hpSlider.value = player.MaxHp;
        mpSlider.value = player.CurrentMp;
    }
    void LateUpdate()
    {
        // ���� ü�� / �ִ� ü�� ������ �����̴��� �ݿ�
        hpSlider.value = (float)player.CurrentHp / player.MaxHp;
        mpSlider.value = (float)player.CurrentMp / player.MaxMp;
    }
}