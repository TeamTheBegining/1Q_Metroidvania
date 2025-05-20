using UnityEngine;
using UnityEngine.UI;

public class DissolveUIController : MonoBehaviour
{
    Material mat;

    private void Awake()
    {
        mat = GetComponent<Image>().material;
    }

    public float GetThredshold()
    {
        return mat.GetFloat("_DissolveThredshold");
    }

    public void SetThredhold(float range = 1f)
    {
        mat.SetFloat("_DissolveThredshold", range);
    }
}