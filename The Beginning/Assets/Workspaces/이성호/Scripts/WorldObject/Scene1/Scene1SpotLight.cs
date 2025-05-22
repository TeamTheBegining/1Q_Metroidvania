using UnityEngine;

public class Scene1SpotLight : MonoBehaviour
{
    private Player player;

    public void GetPlayer(Player player)
    {
        this.player = player;
    }

    private void Update()
    {
        if(player != null)
        {
            transform.position = player.transform.position;
        }
    }
}
