using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Player;

public class PlayerHpUI : MonoBehaviour
{
    // Inspector���� ������ Slider
    public Slider hpSlider;
    public Slider mpSlider;
    public TextMeshProUGUI parryingCountText;

    // PlayerStats Ÿ�� ��ũ��Ʈ���� ü�� �� ������
    public Player player;
    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        hpSlider.value = 1;
        mpSlider.value = 0;
        parryingCountText.text = "";

        if (player == null) Debug.LogWarning($"{gameObject.name} ���� �÷��̾ ã�� �� ���� ");
    }
    void LateUpdate()
    {
        // ���� ü�� / �ִ� ü�� ������ �����̴��� �ݿ�
        if(player != null)
        {
            hpSlider.value = (float)player.CurrentHp / player.MaxHp;
            mpSlider.value = (float)player.CurrentMp / player.MaxMp;
            float count = player.CurParryCount > 3 ? 3 : player.CurParryCount;
            parryingCountText.text = player.CurParryCount == 0 ? "" : "X" + count;
        }
    }
}