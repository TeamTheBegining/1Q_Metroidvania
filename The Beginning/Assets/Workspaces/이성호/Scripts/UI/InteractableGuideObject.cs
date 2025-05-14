using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 트리거에 진입 시 guidImage를 보여주는 클래스
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class InteractableGuideObject : MonoBehaviour
{
    public SpriteRenderer guideImage;

    private void Start()
    {
        guideImage = GetComponentInChildren<SpriteRenderer>();

        HideGuideImage();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ShowGuideImage();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HideGuideImage();
        }
    }

    protected void ShowGuideImage()
    {
        guideImage.color = new Color(1f, 1f, 1f, 1f);
    }

    protected void HideGuideImage()
    {
        guideImage.color = new Color(1f, 1f, 1f, 0f);
    }
}
