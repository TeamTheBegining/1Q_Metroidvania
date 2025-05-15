using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 타이틀 패널 관리 클래스
/// </summary>
public class TitlePanel : MonoBehaviour
{
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI guideText;

    PlayerInputActions actions;

    private bool isIntroStart = false;

    private void Awake()
    {
        titleText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        guideText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        titleText.text = "There be light ";
    }

    private void Start()
    {
        InitInput();

#if UNITY_EDITOR
        if(GameManager.Instance.isDebug)
        {
            isIntroStart = true;
            gameObject.SetActive(false);
        }
#endif
    }

    private void Update()
    {
        if(GameManager.Instance.State == GameState.CutScene && isIntroStart)
        {
            if(!CutSceneManager.Instance.isPlay)
            {
                GameManager.Instance.State = GameState.Play;
            }
        }
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
        StartCoroutine(ProcessGameIntro());

        CameraManager.Instance.HideTitleCamera(CameraType.TitleCamra);
        GameManager.Instance.State = GameState.CutScene;
    }

    private IEnumerator ProcessGameIntro()
    {
        CameraManager.Instance.SetCameraBlendingSpeed(5f);

        float timeElapsed = 0.0f;
        float duration = 2f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            titleText.color = new Color(1f, 1f, 1f, 1 - timeElapsed / duration);
            guideText.color = new Color(1f, 1f, 1f, 1 - timeElapsed / duration);
            yield return null;
        }

        CutSceneManager.Instance.ShowCutscene(0);
        CameraManager.Instance.SetCameraBlendingSpeed();
        isIntroStart = true;
    }
}