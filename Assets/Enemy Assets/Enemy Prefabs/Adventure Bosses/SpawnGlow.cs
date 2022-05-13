using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnGlow : MonoBehaviour
{
    SpriteRenderer sprite;
    public float fadeSpeed = 2;
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        yield return new WaitForSeconds(0.5f);
        Color color = sprite.color;
        while (color.a > 0)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            sprite.color = color;
            yield return null;
        }
        Destroy(gameObject);
    }
}
