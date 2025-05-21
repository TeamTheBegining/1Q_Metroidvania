using UnityEngine;

public class CameraSetTrigger : MonoBehaviour
{
    Scene5Manager localManager;
    public CameraType targetType;
    private void Awake()
    {
        localManager = FindFirstObjectByType<Scene5Manager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            localManager.SetScene5Priority(targetType);
            //Debug.Log($"stay {this.gameObject.name}");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {        
  
    }
}
