using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test_03_PlayerSpotLight : TestBase
{
    public PlayerSpotLight playerSpotLight;

    private void Start()
    {
        LightManager.Instance.SetGlobalLight(Color.black);
    }

    protected override void OnTest1(InputAction.CallbackContext context)
    {
        StartCoroutine(SpreadSpot());
    }

    protected override void OnTest2(InputAction.CallbackContext context)
    {
        playerSpotLight.SetSpotlight(1f);
    }

    protected override void OnTest3(InputAction.CallbackContext context)
    {
        LightManager.Instance.SetGlobalLight(Color.white);
    }

    private IEnumerator SpreadSpot()
    {
        float timeElapsed = 0f;
        float duration = 5f;
        float targetRadius = 100f;

        while(timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            playerSpotLight.SetSpotlight(targetRadius * (timeElapsed / duration));
            yield return null;
        }    
    }
}
