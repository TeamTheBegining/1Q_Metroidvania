using TMPro;
using UnityEngine;

public class KeyGuidePanel : MonoBehaviour
{
    public TextDataSO data;
    public PlayerHpMpUI playerUI;

    private TextMeshProUGUI text;
    private CheatInputActions inputAction;
    private CanvasGroup cg;

    private void Awake()
    {
        inputAction = new CheatInputActions();

        text = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        playerUI = FindFirstObjectByType<PlayerHpMpUI>();

        cg = GetComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        text.text = data.text;

        Close();
    }

    private void OnEnable()
    {
        inputAction.Cheat.K.Enable();
        inputAction.Cheat.K.performed += K_performed;
    }

    private void OnDisable()
    {
        inputAction.Cheat.K.performed -= K_performed;
        inputAction.Cheat.K.Disable();
    }

    private void K_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(cg.alpha > 0)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    private void Open()
    {
        cg.alpha = 1f;
        playerUI.gameObject.SetActive(false);
    }

    private void Close()
    {
        cg.alpha = 0f;
        playerUI.gameObject.SetActive(true);
    }
}
