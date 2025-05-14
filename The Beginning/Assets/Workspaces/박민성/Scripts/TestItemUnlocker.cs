using UnityEngine;
using UnityEngine.UI;

public class TestItemUnlocker : MonoBehaviour
{
    public Image slot1Icon;
    public Image slot2Icon;

    public Sprite newItem1;
    public Sprite newItem2;

    private bool hasSlot1 = false;
    private bool hasSlot2 = false;

    void Start()
    {
        if (slot1Icon != null)
        {
            slot1Icon.enabled = false;
        }
        if (slot2Icon != null)
        {
            slot2Icon.enabled = false;
        }
    }

    void Update()
    {
        if (!hasSlot1 && Input.GetKeyDown(KeyCode.K))
        {
            AcquireItem();
        }
    }

    void AcquireItem()
    {
        if (slot1Icon)
        {
            
        }
    }

}
