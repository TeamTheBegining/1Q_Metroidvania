using UnityEngine;

public class WallSensor : MonoBehaviour
{
    private int m_ColCount = 0;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        return m_ColCount > 0;
    }

    public Transform GetTransform()
    {
        return this.transform;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Wall")
            m_ColCount++;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Wall")
            m_ColCount--;
    }
}
