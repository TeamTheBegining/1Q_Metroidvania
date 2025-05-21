using UnityEngine;

public class MapStateManager : Singleton<MapStateManager>
{
    private bool isScene2FirstEnter = false;
    public bool IsScene2FirstEnter => isScene2FirstEnter;

    private bool isScene3DoorOpened = false;
    public bool IsScene3DoorOpened => isScene3DoorOpened;

    #region Change Functions
    public void SetIsScene2FirstEnterTrue()
    {
        isScene2FirstEnter = true;
    }

    public void SetIsScene3DoorOpened()
    {
        isScene3DoorOpened = true;
    }
    #endregion
}
