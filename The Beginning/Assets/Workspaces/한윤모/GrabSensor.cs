using UnityEngine;

public class GrabSensor : MonoBehaviour
{
    private int m_ColCount = 0;
    private Collider2D coll;
    //private Transform tr;

    private void OnEnable()
    {
        m_ColCount = 0;
    }

    public bool State()
    {
        return m_ColCount > 0;
    }

    public Bounds GetColliderBounds()
    {
        return coll.bounds;
    }
    /*public Transform GetTranform()
    {
        return tr;
    }*/
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Grab")
        {
            coll = other.GetComponent<Collider2D>();
            //tr = other.transform;
            m_ColCount++;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Grab")
            m_ColCount--;
    }
}
