using System.Collections;
using TMPro;
using UnityEngine;

public class TitlePanel : MonoBehaviour
{
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI guideText;

    PlayerInputActions actions;

    private void Awake()
    {
        titleText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        guideText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        titleText.text = "There be light ";
    }

    private void Start()
    {
        InitInput();
    }

    public void InitInput()
    {
        actions = new PlayerInputActions();
        actions.UI.GameStart.Enable();
        actions.UI.GameStart.performed += GameStart_performed;
    }

    private void GameStart_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {       
        actions.UI.GameStart.Disable();
        titleText.text += ";";

        StartCoroutine(TextFadeOut());

        // TODO : 게임 시작하게 만들기
        GameManager.Instance.HideTitleCamera();
        GameManager.Instance.State = GameState.CutScene;
    }

    private IEnumerator TextFadeOut()
    {
        float timeElapsed = 0.0f;
        float duration = 2f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            titleText.color = new Color(1f, 1f, 1f, 1 - timeElapsed / duration);
            guideText.color = new Color(1f, 1f, 1f, 1 - timeElapsed / duration);
            yield return null;
        }
    }
}