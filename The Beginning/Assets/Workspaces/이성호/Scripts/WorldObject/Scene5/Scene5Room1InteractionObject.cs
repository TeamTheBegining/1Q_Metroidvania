using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene5Room1InteractionObject : MonoBehaviour, Interactable
{
    private Scene5Room1Manager manager;
    private bool isTrigger = false;

    private void Awake()
    {
        manager = FindFirstObjectByType<Scene5Room1Manager>();
    }

    public void OnInteraction()
    {
        if(!isTrigger)
        {
            isTrigger = true;
            manager.InteractionCount++;
        }
    }
}