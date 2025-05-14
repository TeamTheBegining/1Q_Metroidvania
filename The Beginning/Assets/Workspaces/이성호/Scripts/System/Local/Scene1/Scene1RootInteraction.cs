using System.Collections;
using UnityEngine;

public class Scene1RootInteraction : MonoBehaviour, Interactable
{
    private PlayerInputActions actions;
    public TextDataSO textData;

    [Tooltip("트리거 되었을 때 바뀔 반지름 값")]
    public float targetRadius;

    [Tooltip("트리거 되었을 때 바뀔 빛 강도")]
    public float targetIntensity;

    [Tooltip("트리거 되었을 때 빛의 크기가 변하는 시간")]
    public float targetDuration;

    public float DisableDelay = 3f;
    
    private void Awake()
    {
        actions = new PlayerInputActions();
    }
    public void OnInteraction()
    {
        // 임시
        GameManager.Instance.MessagePanel.FadeInShow();
        GameManager.Instance.MessagePanel.SetText(textData.text);

        actions.UI.Enable();
        actions.UI.Click.performed += Click_performed;        
    }

    private IEnumerator SpreadSpot()
    {
        GameManager.Instance.MessagePanel.FadeOutClose();

        yield return new WaitForSeconds(3f); // 빛 퍼짐 딜레이

        float timeElapsed = 0f;

        while (timeElapsed < targetDuration)
        {
            timeElapsed += Time.deltaTime;
            LightManager.Instance.PlayerSpotlihgt.SetSpotlight(targetRadius * (timeElapsed / targetDuration));
            yield return null;
        }

        LightManager.Instance.PlayerSpotlihgt.SetSpotlight(0f, 0f);
        LightManager.Instance.SetGlobalLight(Color.white);

        yield return new WaitForSeconds(1f);

        StartCoroutine(DisableProecess());
    }

    private void Click_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //Debug.Log("click");
        OnClick();
    }

    private void OnClick()
    {
        GameManager.Instance.MessagePanel.AddText(" ;");
        actions.UI.Click.performed -= Click_performed;
        actions.UI.Disable();
        StartCoroutine(SpreadSpot());
    }

    private IEnumerator DisableProecess()
    {
        float timeElapsed = 0f;
        while(timeElapsed < DisableDelay)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        //GameSceneManager.Instance.ChangeScene(1);

        //gameObject.SetActive(false);
    }
}
