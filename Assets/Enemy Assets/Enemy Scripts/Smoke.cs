using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    SpriteRenderer sprite;
    float fadeSpeed = 0.8f;
    float riseSpeed = 4f;
    float sizeSpeed = 1;
    public float horizVelocity;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        fadeSpeed = (fadeSpeed * 0.85f) + Random.Range(0, 1f) * fadeSpeed * 0.3f;
        riseSpeed = (riseSpeed * 0.85f) + Random.Range(0, 1f) * riseSpeed * 0.3f;
        sizeSpeed = (sizeSpeed * 0.85f) + Random.Range(0, 1f) * sizeSpeed * 0.3f;
        horizVelocity += Random.Range(-1f, 1f) * 2;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Color color = sprite.color;
        color.a -= fadeSpeed * Time.deltaTime;
        sprite.color = color;

        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        transform.localScale = new Vector2(transform.localScale.x + sizeSpeed * Time.deltaTime, transform.localScale.y + sizeSpeed * Time.deltaTime);

        transform.position += Vector3.right * horizVelocity * Time.deltaTime;
        horizVelocity = Mathf.MoveTowards(horizVelocity, 0, 3 * Time.deltaTime);

        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
