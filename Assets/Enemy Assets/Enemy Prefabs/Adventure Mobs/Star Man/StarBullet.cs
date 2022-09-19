using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBullet : MonoBehaviour
{
    Rigidbody2D bulletRB;
    TrailRenderer trail;
    BulletScript bulletScript;

    public bool aestheticBullet = false;

    void Start()
    {
        bulletRB = GetComponent<Rigidbody2D>();
        bulletScript = GetComponent<BulletScript>();
        Color newColor = new Color(Random.Range(0.5f, 1), Random.Range(0.5f, 1), Random.Range(0.5f, 1));
        if (bulletScript.aggro)
        {
            newColor = new Color(newColor.r + 0.3f, newColor.g - 0.3f, newColor.b - 0.3f);
        }
        GetComponent<SpriteRenderer>().color = newColor;

        trail = GetComponent<TrailRenderer>();
        trail.startColor = newColor;
        trail.endColor = new Color(newColor.r, newColor.g, newColor.b, 0);

        if (aestheticBullet)
        {
            bulletScript.bulletCanBeReflected = false;
            //GetComponent<SpriteRenderer>().sortingLayerName = "Items";
            newColor = GetComponent<SpriteRenderer>().color;
            newColor.a = 0.5f;
            GetComponent<SpriteRenderer>().color = newColor;
            //trail.sortingLayerName = "Items";
            trail.startColor = newColor;
            transform.localScale *= 0.6f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        bulletRB.rotation += 500 * Time.deltaTime;
    }
}
