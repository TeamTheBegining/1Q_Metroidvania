using System.Collections;
using UnityEngine;

public class Scene2Manager : LocalSceneManager
{
    private CanvasGroup cg;
    private TextScroll textScroll;
    public float cutSceneTime;

    private PlayerInput input;

    private void Awake()
    {
        cg = transform.GetChild(0).GetComponent<CanvasGroup>();
        textScroll = transform.GetChild(0).GetChild(0).GetComponent<TextScroll>();
    }

    public override void Init()
    {
        input = FindFirstObjectByType<PlayerInput>();

        if (!MapStateManager.Instance.IsScene2FirstEnter)
        {
            MapStateManager.Instance.SetIsScene2FirstEnterTrue();
            //cutSceneTime = CutSceneManager.Instance.GetSequenceTime(1);
            input.AllDisable();
            StartCoroutine(CameraSetProcess());
        }
        else
        {
            GameObject playerObject = FindFirstObjectByType<Player>().gameObject;
            CameraManager.Instance.SetTarget(CameraType.Scene2PlayerCamera, playerObject.transform);
            CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 20);
            CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2CutSceneCamera, 0);

            SoundManager.Instance.PlayBGM(BGMType.bgm_Credit_01);
        }
        PlayerHpMpUI hp = FindFirstObjectByType<PlayerHpMpUI>();
        hp.GetPlayer();
    }

    public void PlayText()
    {
        textScroll.PlayScroll();
    }

    public void DisableText()
    {
        StartCoroutine(CloseProcess(0.5f));
    }

    private IEnumerator CloseProcess(float duration)
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = 1 - timeElapsed / duration;
            yield return null;
        }
    }

    private IEnumerator CameraSetProcess()
    {
        float elapsedTime = 0.0f;
        float duration = CutSceneManager.Instance.GetSequenceTime(1);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GameObject playerObject = FindFirstObjectByType<Player>().gameObject;

        GameManager.Instance.State = GameState.CutScene;

        // 처음 시작 카메라 설정
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 20);
        CameraManager.Instance.SetTarget(CameraType.Scene2PlayerCamera, playerObject.transform);

        // 보여 주기 위한 전체 씬 로드 하기
        GameSceneManager.Instance.LoadSceneAddive(3);

        yield return new WaitForSeconds(1f);

        // 컷씬 카메라로 변경
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 0);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2CutSceneCamera, 20);

        yield return new WaitForSeconds(3f);
        PlayText();

        yield return new WaitForSeconds(8f);
        DisableText();

        // 컷씬 종료 후 다시 시작 카메라로 변경
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 20);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2CutSceneCamera, 0);

        yield return new WaitForSeconds(3f);
        // 현재 씬 이외 씬들 모두 로드 해제

        GameSceneManager.Instance.UnloadScene(3);
        GameManager.Instance.State = GameState.Play;
        input.AllEnable();
        gameObject.SetActive(false);
    }
}
