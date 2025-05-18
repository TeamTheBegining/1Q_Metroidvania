using UnityEngine;

public class MapStateManager : Singleton<MapStateManager>
{
    private bool isScene2FirstEnter = false;
    public bool IsScene2FirstEnter => isScene2FirstEnter;

    #region Change Functions
    public void SetIsScene2FirstEnterTrue() // юс╫ц
    {
        isScene2FirstEnter = true;
    }
    #endregion
}
