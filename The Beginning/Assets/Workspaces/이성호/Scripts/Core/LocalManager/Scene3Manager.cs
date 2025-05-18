using UnityEngine;

public class Scene3Manager : MonoBehaviour
{
    public Transform spawnPoint;

    void Start()
    {
        PlayerManager.Instance.SpawnPlayer(spawnPoint.position);
    }
}
