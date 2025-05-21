using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class Scene3Item1 : MonoBehaviour, Interactable
{
    public PlayerSkillType type;
    public int price = 1000;

    public bool isTriggered = false;

    private void Start()
    {
        if (PlayerManager.Instance.IsSkillUnlock[(int)type])
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnInteraction()
    {
        if (isTriggered) return;

        isTriggered = true;

        if (PlayerManager.Instance.Coin < price)
        {
            StopAllCoroutines();
            GameManager.Instance.BottomMessagePanel.Close();
            GameManager.Instance.BottomMessagePanel.Show();
            GameManager.Instance.BottomMessagePanel.SetText("돈이 부족합니다.");
            StartCoroutine(MessageOutProcess());
            return;
        }
        else
        {
            PlayerManager.Instance.UnlockPlayerSkill(type);
            PlayerManager.Instance.UseCoin(price);
            this.gameObject.SetActive(false);
        }
    }

    private IEnumerator MessageOutProcess()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.BottomMessagePanel.FadeOutClose(2f);
    }
}
