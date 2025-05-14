using UnityEngine;
using UnityEngine.UI;
using static Player;

public class PlayerHpUI : MonoBehaviour
{
    // Inspector에서 연결할 Slider
    public Slider hpSlider;
    public Slider mpSlider;

    // PlayerStats 타입 스크립트에서 체력 값 가져옴
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        hpSlider.value = player.MaxHp;
        mpSlider.value = player.CurrentMp;
    }
    void LateUpdate()
    {
        // 현재 체력 / 최대 체력 비율을 슬라이더에 반영
        hpSlider.value = (float)player.CurrentHp / player.MaxHp;
        mpSlider.value = (float)player.CurrentMp / player.MaxMp;
    }
}