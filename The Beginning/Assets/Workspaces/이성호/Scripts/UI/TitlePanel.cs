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

    //private bool isIntroStart = false;

    private float cutSceneDelay = 23f;

    private void Awake()
    {
        titleText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        guideText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

        titleText.text = "태초에 말씀이 있었다 ";
    }

    private void Start()
    {
        InitInput();

        cutSceneDelay = CutSceneManager.Instance.GetSequenceTime(0);
        this.gameObject.SetActive(true);

#if UNITY_EDITOR
        if (GameManager.Instance.isDebug)
        {
            //isIntroStart = true;
            gameObject.SetActive(false);
        }
#endif
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
    }

    private IEnumerator ProcessGameIntro()
    {
        CameraManager.Instance.HideTitleCamera(CameraType.TitleCamra);
        GameManager.Instance.State = GameState.CutScene;
        CameraManager.Instance.SetCameraBlendingSpeed(5f);

        float timeElapsed = 0.0f;
        float duration = 2f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;

            titleText.faceColor = new Color(titleText.faceColor.r, titleText.faceColor.g, titleText.faceColor.b, 1 - timeElapsed / duration); // 글자의 face 색상 조정
            //titleText.material.SetColor("Color", new Color(titleText.faceColor.r, titleText.faceColor.g, titleText.faceColor.b, 1 - timeElapsed / duration)); // glow 색상 조정
            guideText.faceColor = new Color(1f, 1f, 1f, 1 - timeElapsed / duration);

            yield return null;
        }

        CutSceneManager.Instance.ShowCutscene(0);
        CameraManager.Instance.SetCameraBlendingSpeed();

        yield return new WaitForSeconds(cutSceneDelay);
        GameManager.Instance.State = GameState.Play;
        //isIntroStart = true;
        this.gameObject.SetActive(false);
    }
}