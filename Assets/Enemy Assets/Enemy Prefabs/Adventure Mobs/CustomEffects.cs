using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomEffects : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sprite;
    Color initialColor;
    Color newColor;

    float rotationXspeed;
    float rotationYspeed;
    float rotationZspeed;

    float rotationX = 0;
    float rotationY = 0;
    float rotationZ = 0;

    public float rotateSpeed = 1000f;
    public bool rotateXY = true;
    public bool rotateZ = true;
    public bool flash = true;

    public float aValue = 2;
    float initialAValue;
    public float fadeSpeed = 3f;
    float time;
    float initialVelocity;

    public bool slowDown = false;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        initialColor = sprite.color;
        newColor = Color.white;
        time = aValue / fadeSpeed;
        initialAValue = aValue;

        if (rb.velocity.magnitude < 0.5f)
        {
            rb.velocity = Random.insideUnitCircle * 6f;
        }
        initialVelocity = rb.velocity.magnitude;

        if (rotateXY)
        {
            rotationXspeed = Random.Range(rotateSpeed * 0.8f, rotateSpeed * 1.2f);
            rotationYspeed = Random.Range(rotateSpeed * 0.8f, rotateSpeed * 1.2f);
            rotationX = Random.Range(-90f, 90f);
            rotationY = Random.Range(-90f, 90f);
        }
        if (rotateZ)
        {
            rotationZspeed = Random.Range(rotateSpeed * 0.8f, rotateSpeed * 1.2f);
            rotationZ = Random.Range(-90f, 90f);
        }
        transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);

        StartCoroutine(LightFlash());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (rotateXY)
        {
            rotationX += rotationXspeed * Time.fixedDeltaTime;
            if (rotationX > 90)
            {
                rotationX -= 180;
            }
            rotationY += rotationYspeed * Time.fixedDeltaTime;
            if (rotationY > 90)
            {
                rotationY -= 180;
            }
            transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
        }
        if (rotateZ)
        {
            rotationZ += rotationZspeed * Time.fixedDeltaTime;
            if (rotationZ > 90)
            {
                rotationZ -= 180;
            }
            transform.eulerAngles = new Vector3(rotationX, rotationY, rotationZ);
        }

        if (slowDown)
        {
            rb.velocity = rb.velocity.normalized * (initialVelocity * aValue / initialAValue);
        }

        aValue -= fadeSpeed * Time.fixedDeltaTime;
    }

    IEnumerator LightFlash()
    {
        float initialWindow = 0.4f;
        float timer = 0;
        while (true)
        {
            float window = initialWindow + Random.Range(-0.2f, 0.2f);
            timer += Time.fixedDeltaTime;

            Color color = sprite.color;
            color.a = aValue * 0.7f;
            sprite.color = color;
            if (aValue <= 0)
            {
                Destroy(gameObject);
            }

            yield return new WaitForFixedUpdate();
            if (timer > window)
            {
                if (flash)
                {
                    sprite.color = new Color(newColor.r, newColor.g, newColor.b, aValue);
                }
                yield return new WaitForFixedUpdate();
                sprite.color = new Color(initialColor.r, initialColor.g, initialColor.b, aValue * 0.7f);
                timer = 0;
            }
        }
    }
}
