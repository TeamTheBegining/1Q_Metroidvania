using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.Universal;

public class Scene1RootInteraction : MonoBehaviour, Interactable
{
    private PlayerInputActions actions;
    private PlayerInput targetInput;

    public Scene1RootController rootController;
    public GameObject rightBlockObject;

    [Space(20f)]
    public TextDataSO textData;

    [Tooltip("트리거 되었을 때 바뀔 반지름 값")]
    public float targetRadius;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    public float enableDelay = 1f;
    public float disableDelay = 3f;
    private bool isTrigger = false;

    [Space(20)]
    public Light2D spotLight;

    private void Awake()
    {
        actions = new PlayerInputActions();
    }

    public void OnInteraction()
    {
        if (isTrigger) return;

        isTrigger = true;
        GameManager.Instance.MiddleMessagePanel.FadeInShow(enableDelay);
        GameManager.Instance.MiddleMessagePanel.SetGlowText(textData.text);

        targetInput = FindFirstObjectByType<PlayerInput>();
        if(targetInput != null)
        {
            targetInput.AllDisable();
        }

        StartCoroutine(ClickEnableProcess());
    }

    private void PanelInteraction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        Debug.Log("click");
        OnClick();
    }

    private void OnClick()
    {
        actions.UI.PanelInteraction.performed -= PanelInteraction_performed;
        actions.UI.Disable();


        GameManager.Instance.MiddleMessagePanel.AddGlowText(" ;");
        LightManager.Instance.SpreadPlayerLight(targetDuration, targetRadius, 1f);

        StartCoroutine(DisableProecess());
    }

    private IEnumerator ClickEnableProcess()
    {
        float timeElapsed = 0.0f;

        while(timeElapsed < enableDelay)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        targetInput.AllEnable();

        actions.UI.Enable();
        actions.UI.PanelInteraction.performed += PanelInteraction_performed;
    }

    private IEnumerator DisableProecess()
    {
        GameManager.Instance.MiddleMessagePanel.GlowFadeInOpen(4f);
        //GameSceneManager.Instance.RequestSceneChange(1, true); ->

        float timeElapsed = 0f;
        while(timeElapsed < disableDelay)
        {
            timeElapsed += Time.deltaTime;
            if(spotLight != null) spotLight.pointLightOuterRadius += timeElapsed * 2f;
            yield return null;
        }

        GameManager.Instance.MiddleMessagePanel.FadeOutClose();
        LightManager.Instance.SetPlayerShadowActive(false);
        rightBlockObject.SetActive(false);
        rootController.ColliderActive();

        if (spotLight != null) Destroy(spotLight);
    }
}
