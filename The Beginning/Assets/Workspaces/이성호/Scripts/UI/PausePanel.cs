using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    private PlayerInputActions actions;
    private CanvasGroup cg;
    private Button restartButton;
    private Button exitButton;


    private void Awake()
    {
        actions = new PlayerInputActions();

        cg = GetComponent<CanvasGroup>();
        restartButton = transform.GetChild(1).GetChild(1).GetComponent<Button>();
        restartButton.onClick.AddListener(() => { ClosePanel(); });
            
        exitButton = transform.GetChild(1).GetChild(2).GetComponent<Button>();
        exitButton.onClick.AddListener(() => GameManager.Instance.ExitGame());
    }

    private void OnEnable()
    {
        actions.UI.Pause.Enable();
        actions.UI.Pause.performed += Pause_performed;
    }

    private void OnDisable()
    {
        actions.UI.Pause.performed -= Pause_performed;
        actions.UI.Pause.Disable();
    }

    private void Start()
    {
        ClosePanel();
    }

    private void Pause_performed(InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.State != GameState.Play) return;

        if (cg.alpha != 1)
        {
            ShowPanel();
        }
        else
        {
            ClosePanel();
        }
    }

    private void ShowPanel()
    {
        Time.timeScale = 0f;
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void ClosePanel()
    {
        Time.timeScale = 1f;
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
