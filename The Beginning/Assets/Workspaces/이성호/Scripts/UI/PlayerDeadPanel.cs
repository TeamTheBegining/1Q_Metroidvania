using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이어 사망 시 등장하는 패널 ( Setactive로 컨트롤 )
/// </summary>
public class PlayerDeadPanel : MonoBehaviour
{
    PlayerInputActions actions;
    CanvasGroup cg;
    private float duration = 0.5f;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        cg.alpha = 0.0f;
        Init();


        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        actions = new PlayerInputActions();
        actions.UI.Space.Enable();
        actions.UI.Space.started += Space_started;
    }

    private void OnDisable()
    {
        actions.UI.Space.started -= Space_started;        
        actions.UI.Space.Disable();
    }

    private void Space_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        this.gameObject.SetActive(false);
        PlayerManager.Instance.Respawn();
    }

    public void ShowPanel()
    {
        if (cg.alpha > 0.0f) return; // 중복 호출 방지

        this.gameObject.SetActive(true);
        StartCoroutine(ShowPanelProcess());
    }

    private IEnumerator ShowPanelProcess()
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            cg.alpha = timeElapsed / duration;
            yield return null;
        }
    }

    public void Init()
    {
        this.gameObject.SetActive(false);

        Player player = FindFirstObjectByType<Player>();
        // 처음 시작 시 플레이어 찾기
        if (player != null)
        {
            player.OnDead += () =>
            {
                if (GameManager.Instance.State == GameState.Play)
                {
                    GameManager.Instance.State = GameState.PlayEnd;
                    ShowPanel();
                }
            };
        }

    }
}