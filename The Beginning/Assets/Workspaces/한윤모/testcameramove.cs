using Unity.VisualScripting;
using UnityEngine;

public class testcameramove : MonoBehaviour
{
    public Transform playerpos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(playerpos.position.x, playerpos.position.y,-10);
    }
}
