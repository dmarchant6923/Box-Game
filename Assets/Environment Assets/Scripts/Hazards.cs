using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazards : MonoBehaviour
{
    Color initialColor;
    SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        initialColor = sprite.color;
        initialColor.r = 1;
        initialColor.g = 0.15f;
        sprite.color = initialColor;
        StartCoroutine(ColorChange());
        gameObject.tag = "Hazard";
    }

    IEnumerator ColorChange()
    {
        float timer = 0;
        Color color = initialColor;
        float periodTime = 0.8f;
        while (true)
        {
            color.g = initialColor.g + 0.3f * Mathf.Sin(timer * Mathf.PI * 2 / periodTime);
            sprite.color = color;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }
}
