using UnityEngine;

public class MapStateManager : Singleton<MapStateManager>
{
    private bool isScene2FirstEnter = false;
    public bool IsScene2FirstEnter => isScene2FirstEnter;

    private bool isScene3DoorOpened = false;
    public bool IsScene3DoorOpened => isScene3DoorOpened;

    private bool isScene5DoorOpend = false;
    public bool IsScene5DoorOpened => isScene5DoorOpend;

    #region Change Functions
    public void SetIsScene2FirstEnterTrue()
    {
        isScene2FirstEnter = true;
    }

    public void SetIsScene3DoorOpened()
    {
        isScene3DoorOpened = true;
        Debug.Log("Scene3 door 열림");
    }

    public void SetIsScene5DoorOpened()
    {
        isScene5DoorOpend = true;
        Debug.Log("Scene5 door 열림");
    }
    #endregion
}
