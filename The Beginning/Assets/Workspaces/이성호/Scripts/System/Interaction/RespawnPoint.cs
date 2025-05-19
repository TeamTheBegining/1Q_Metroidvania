using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class RespawnPoint : MonoBehaviour, Interactable
{
    public SpawnPointDataSO data;

    public void OnInteraction()
    {        
        if(data == null)
        {
            Debug.LogWarning($"{this.gameObject.name}의 SpawnPointData 데이터가 비어있습니다.");
        }

        PlayerManager.Instance.SetSpawn(SceneManager.GetActiveScene().name, data);
        Player player = FindFirstObjectByType<Player>();
        if(player != null)
        {
            player.CurrentHp = player.MaxHp;
        }

        EnemyStateManager.Instance.ResetAllEnemies();
        Debug.Log("respawnInteraction ------------------------");

        StartCoroutine(InteractionProcess());
    }

    private IEnumerator InteractionProcess()
    {
        float timeElapsed = 0.0f;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = Color.blue;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;  
            sprite.color = new Color(timeElapsed, timeElapsed, 1f, 1f);
            yield return null;
        }
        sprite.color = Color.white;
    }
}
