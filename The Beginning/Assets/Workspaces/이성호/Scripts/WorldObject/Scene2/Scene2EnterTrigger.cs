using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene2EnterTrigger : MonoBehaviour
{
    private Scene2Manager manager;
    public Transform spawnPosition;
    public Collider2D groundCollider;

    public float cutSceneTime = 10f;

    private bool isTrigger = false;

    private void Awake()
    {
        manager = FindFirstObjectByType<Scene2Manager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!isTrigger && collision.CompareTag("Player"))
        {
            groundCollider.enabled = true;
            isTrigger = true;

            GameManager.Instance.State = GameState.CutScene;
            CutSceneManager.Instance.ShowCutscene(1);
            StartCoroutine(CameraSetProcess());
        }
    }

    private IEnumerator CameraSetProcess()
    {
        float elapsedTime = 0.0f;
        float duration = CutSceneManager.Instance.GetSequenceTime(1);
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        GameSceneManager.Instance.UnloadScene(0); // 튜토리얼 씬 로드 해제

        GameObject obj = PlayerManager.Instance.SpawnPlayer(spawnPosition.position).gameObject;
        Debug.Log("PlayerSpawn");

        // 처음 시작 카메라 설정
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 20);
        CameraManager.Instance.SetTarget(CameraType.Scene2PlayerCamera, obj.transform);

        // 보여 주기 위한 전체 씬 로드 하기
        GameSceneManager.Instance.ChangeScene(2, true);

        yield return new WaitForSeconds(1f);

        // 컷씬 카메라로 변경
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 0);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2CutSceneCamera, 20);

        yield return new WaitForSeconds(3f);
        manager.PlayText();

        yield return new WaitForSeconds(cutSceneTime);
        manager.DisableText();

        // 컷씬 종료 후 다시 시작 카메라로 변경
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2PlayerCamera, 20);
        CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2CutSceneCamera, 0);

        yield return new WaitForSeconds(3f);
        // 현재 씬 이외 씬들 모두 로드 해제

        GameSceneManager.Instance.UnloadScene(2);
        GameManager.Instance.State = GameState.Play;
        gameObject.SetActive(false);
    }
}
