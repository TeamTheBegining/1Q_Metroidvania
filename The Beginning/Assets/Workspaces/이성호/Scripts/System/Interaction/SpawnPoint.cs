using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class SpawnPoint : MonoBehaviour, Interactable
{
    public void OnInteraction()
    {
        PlayerManager.Instance.SetSpawn(SceneManager.GetActiveScene().buildIndex, this.transform.position);
        EnemyStateManager.Instance.ResetAllEnemies();
        Debug.Log("respawnInteraction ------------------------");
    }
}
