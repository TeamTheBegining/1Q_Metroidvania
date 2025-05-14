using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Scene1LookTreeTrigger : MonoBehaviour
{
    // 임시
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            CameraManager.Instance.SetVirtualCameraPriority(CameraType.Scene2Camera, 25); // 임시
        }
    }
}
