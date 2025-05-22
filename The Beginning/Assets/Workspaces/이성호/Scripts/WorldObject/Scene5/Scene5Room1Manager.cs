using UnityEngine;

public class Scene5Room1Manager : MonoBehaviour
{
    public Scene5Room1Door door;
    private int interactionCount = 0;
    private int maxInteractionCount = 3;
    public GameObject[] enemies;

    private void Start()
    {
        foreach(var obj in enemies)
        {
            obj.SetActive(false);
        }

        if(MapStateManager.Instance.IsScene5DoorOpened)
        {
            door.Open();
        }
    }

    public int InteractionCount
    {
        get => interactionCount;
        set
        {
            interactionCount = value;
            if(interactionCount >= maxInteractionCount)
            {
                // 문열림
                if(door != null)
                {
                    door.Open();
                    MapStateManager.Instance.SetIsScene5DoorOpened();
                }
                else
                {
                    Debug.LogWarning($"{door} 스크립트가 Scene5Room1Manager에 존재하지 않습니다.");
                }
            }
        }
    }
}
