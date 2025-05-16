using System.Collections;
using UnityEngine;

public class Scene1RootInteraction : MonoBehaviour, Interactable
{
    private PlayerInputActions actions;
    public TextDataSO textData;

    [Tooltip("트리거 되었을 때 바뀔 반지름 값")]
    public float targetRadius;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    public float DisableDelay = 3f;

    private bool isTrigger = false;
    
    private void Awake()
    {
        actions = new PlayerInputActions();
    }
    public void OnInteraction()
    {
        if (isTrigger) return;
        GameManager.Instance.MiddleMessagePanel.FadeInShow();
        GameManager.Instance.MiddleMessagePanel.SetGlowText(textData.text);

        actions.UI.Enable();
        actions.UI.PanelInteraction.performed += PanelInteraction_performed;        
    }

    private void PanelInteraction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("click");
        OnClick();
    }

    private void OnClick()
    {
        GameManager.Instance.MiddleMessagePanel.AddGlowText(" ;");
        actions.UI.PanelInteraction.performed -= PanelInteraction_performed;
        actions.UI.Disable();
        LightManager.Instance.SpreadPlayerLight(targetDuration, targetRadius, 1f);

        StartCoroutine(DisableProecess());
    }

    private IEnumerator DisableProecess()
    {
        GameManager.Instance.MiddleMessagePanel.GlowFadeInOpen(4f);
        GameSceneManager.Instance.ChangeScene(1, true);

        float timeElapsed = 0f;
        while(timeElapsed < DisableDelay)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.MiddleMessagePanel.FadeOutClose();
        LightManager.Instance.SetPlayerShadowActive(false);
        isTrigger = true;
    }
}
