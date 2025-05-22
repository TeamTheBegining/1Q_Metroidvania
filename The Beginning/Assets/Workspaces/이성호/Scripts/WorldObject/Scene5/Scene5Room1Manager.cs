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

                    // NOTE: 모든 적을 죽인뒤에 다시 돌아갔을 때 리스폰되는지 안나오는지 테스트 안함
                    foreach (var obj in enemies)
                    {
                        obj.SetActive(true);
                    }
                }
                else
                {
                    Debug.LogWarning($"{door} 스크립트가 Scene5Room1Manager에 존재하지 않습니다.");
                }
            }
        }
    }
}
