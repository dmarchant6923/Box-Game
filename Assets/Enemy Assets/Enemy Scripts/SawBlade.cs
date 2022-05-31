using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawBlade : MonoBehaviour
{
    public float speed = 15f;
    public Vector2 direction;

    float despawnTime = 18f;
    float timer = 0;
    bool flickerCR = false;

    public float damage = 30;
    Vector2 posLastFrame;
    bool bladeHitstopActive = false;

    public bool fadeIn = false;
    public float fadeTime = 2;
    SpriteRenderer[] sprites;

    Rigidbody2D bladeRB;

    bool spawnHitboxCR = false;
    public GameObject hitbox;
    GameObject newHitbox;

    public bool death = false;

    void Start()
    {
        bladeRB = GetComponent<Rigidbody2D>();
        sprites = GetComponentsInChildren<SpriteRenderer>();
        posLastFrame = bladeRB.position;

        bladeRB.angularVelocity = -700;
        if (fadeIn)
        {
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.enabled = false;
            }
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(Fade());
        }
        else
        {
            bladeRB.velocity = direction.normalized * speed;
        }
    }

    private void FixedUpdate()
    {
        RaycastHit2D[] enemyCast = Physics2D.CircleCastAll(bladeRB.position, 10, Vector2.zero, 0, LayerMask.GetMask("Enemies"));
        foreach (RaycastHit2D enemy in enemyCast)
        {
            if (Physics2D.GetIgnoreCollision(enemy.collider, GetComponent<Collider2D>()) == false)
            {
                Physics2D.IgnoreCollision(enemy.collider, GetComponent<Collider2D>(), true);
            }
        }

        if (fadeIn == false)
        {
            int casts = 3;
            Vector2 difference = bladeRB.position - posLastFrame;
            Vector2 increment = difference / casts;
            for (int i = 0; i < casts; i++)
            {
                RaycastHit2D boxCast = Physics2D.CircleCast(posLastFrame + increment * (i + 1) , GetComponent<Collider2D>().bounds.extents.x, Vector2.zero, 0, LayerMask.GetMask("Box"));
                if (boxCast.collider != null && Box.isInvulnerable == false)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = damage;
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(bladeRB.velocity.x), 1).normalized;
                    StartCoroutine(BladeHitstop());
                    break;
                }
            }

            if (spawnHitboxCR == false)
            {
                StartCoroutine(SpawnHitbox());
            }

            bladeRB.velocity = bladeRB.velocity.normalized * speed;
        }

        timer += Time.fixedDeltaTime;
        if (timer > despawnTime - 2.5f && flickerCR == false)
        {
            StartCoroutine(Flicker());
        }
        if (timer > despawnTime && death == false)
        {
            death = true;
        }
        if (death)
        {
            StartCoroutine(Death());
        }

        posLastFrame = bladeRB.position;
    }

    public void Launch()
    {
        bladeRB.velocity = direction.normalized * speed;
        GetComponent<Collider2D>().enabled = true;
    }

    IEnumerator Fade()
    {
        Color[] initialSpriteColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            initialSpriteColors[i] = sprites[i].color;
        }
        float initialWait = 0.1f;
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < sprites.Length; i++)
        {
            Color color = sprites[i].color;
            color.a = 0;
            sprites[i].color = color;
            sprites[i].enabled = true;
        }

        float time = fadeTime - initialWait;
        while (sprites[0].color.a < initialSpriteColors[0].a)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                float changeSpeed = initialSpriteColors[i].a / time;
                Color color = sprites[i].color;
                color.a += changeSpeed * Time.fixedDeltaTime;
                sprites[i].color = color;
            }
            yield return new WaitForFixedUpdate();
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            sprites[i].color = initialSpriteColors[i];
        }

        fadeIn = false;
    }
    IEnumerator BladeHitstop()
    {
        bladeHitstopActive = true;
        Vector2 enemyHitstopVelocity = bladeRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        bladeRB.velocity = new Vector2(0, 0);
        bladeRB.angularVelocity /= enemyHitstopRotationSlowDown;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        bladeRB.angularVelocity *= enemyHitstopRotationSlowDown;
        bladeRB.velocity = enemyHitstopVelocity;
        bladeHitstopActive = false;
    }
    IEnumerator Flicker()
    {
        flickerCR = true;
        while (true)
        {
            float waitTime = 0.2f;
            if (timer > despawnTime - 1)
            {
                waitTime = 0.12f;
            }
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.enabled = false;
            }
            yield return new WaitForSeconds(waitTime * 0.25f);
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.enabled = true;
            }
            yield return new WaitForSeconds(waitTime * 0.75f);
        }
    }
    IEnumerator SpawnHitbox()
    {
        spawnHitboxCR = true;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            while (bladeHitstopActive)
            {
                yield return null;
            }
            newHitbox = Instantiate(hitbox, bladeRB.position, Quaternion.identity);
            newHitbox.transform.localScale = transform.localScale * 1.1f;
            newHitbox.transform.eulerAngles = new Vector3(0, 0, bladeRB.rotation);
            StartCoroutine(HitboxFade(newHitbox));
        }
    }
    IEnumerator HitboxFade(GameObject hitbox)
    {
        SpriteRenderer sprite = hitbox.GetComponent<SpriteRenderer>();
        sprite.enabled = true;
        Color color = sprite.color;
        color.a = 0.8f;
        sprite.color = color;

        float window = 0.4f;
        float speed = color.a / window;
        while (hitbox.GetComponent<SpriteRenderer>().color.a > 0 && death == false)
        {
            if (bladeHitstopActive == false)
            {
                color = sprite.color;
                color.a -= speed * Time.deltaTime;
                sprite.color = color;

                sprite.transform.localScale -= Vector3.one * speed * Time.deltaTime;
            }
            yield return null;
        }
        Destroy(hitbox);
    }
    IEnumerator Death()
    {
        yield return null;
        Destroy(gameObject);
    }
}
