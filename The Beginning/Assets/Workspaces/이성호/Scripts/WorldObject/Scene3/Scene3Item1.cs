using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class Scene3Item1 : MonoBehaviour, Interactable
{
    public PlayerSkillType type;
    public float price = 1000;

    private void Start()
    {
        if (PlayerManager.Instance.IsSkillUnlock[(int)type])
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OnInteraction()
    {
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
            PlayerManager.Instance.UseCoin(1000);
            this.gameObject.SetActive(false);
        }
    }

    private IEnumerator MessageOutProcess()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.BottomMessagePanel.FadeOutClose(2f);
    }
}
