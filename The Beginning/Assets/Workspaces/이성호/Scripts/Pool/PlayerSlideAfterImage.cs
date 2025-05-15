using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public enum PlayerSlideImageType
{
    Slide1 = 0,
    Slide2,
    Slide3,
    SlideImageCount
}

public class PlayerSlideAfterImage : MonoBehaviour, IPoolable
{
    private Sprite[] images;
    private SpriteRenderer spriteRenderer;
    public Action ReturnAction { get; set; }

    public float duration = 0.25f;
    [ColorUsage(true)]
    public Color color;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDisable()
    {
        ReturnAction?.Invoke();
    }

    public void Init(int index)
    {
        spriteRenderer.sprite = images[index];
        spriteRenderer.color = color;
    }

    public void Init(PlayerSlideImageType type)
    {
        Init((type));
    }

    public void OnDespawn()
    {
        
    }

    public void OnSpawn()
    {
        if(images == null)
        {
            images = new Sprite[(int)PlayerSlideImageType.SlideImageCount];

            images[0] = Resources.Load<Sprite>("Art/Player/main_crouch_1");
            images[1] = Resources.Load<Sprite>("Art/Player/Slide/main_slide_1");
            images[2] = Resources.Load<Sprite>("Art/Player/Slide/main_slide_2");
        }


        StartCoroutine(DisableProcess());
    }

    private IEnumerator DisableProcess()
    {
        float timeElapsed = 0.0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            Debug.Log($"{timeElapsed}");
            yield return null;
        }

        this.gameObject.SetActive(false);
    }
}
