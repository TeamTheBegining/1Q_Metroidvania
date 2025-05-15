using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_03_PlayerSpotLight : TestBase
{
#if UNITY_EDITOR
    public PlayerShaderLight playerSpotLight;

    private void Start()
    {
        LightManager.Instance.SetPlayerLightValue(0f);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        StartCoroutine(SpreadSpot());
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        playerSpotLight.SetRange(1f);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
    }

    private IEnumerator SpreadSpot()
    {
        float timeElapsed = 0f;
        float duration = 5f;
        float targetRadius = 100f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            playerSpotLight.SetRange(targetRadius * (timeElapsed / duration));
            yield return null;
        }    
    }
#endif
}
