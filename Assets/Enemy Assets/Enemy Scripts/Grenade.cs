using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    Rigidbody2D grenadeRB;
    Rigidbody2D boxRB;

    public GameObject explosion;
    GameObject newExplosion;
    bool explosionTimer = false;
    float explosionTime = 0.7f;
    float explosionRadius = 3;
    float explosionDamage = 30;

    float collisionDamage = 10;
    bool grenadeHitstopActive = false;
    float explosionBounceMaxSpeed = 6 ;
    public bool grenadeWasReflected = false;

    public bool remoteExplode = false;

    Color grenadeColor;
    float colorChangeSpeed = 20f;

    Vector2[] velocityList = new Vector2[3];
    void Start()
    {
        grenadeRB = transform.GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        //grenadeRB.velocity = Vector2.down * 100;
    }

    void Update()
    {
        if (grenadeRB.velocity.magnitude >= 30)
        {
            grenadeRB.velocity = grenadeRB.velocity.normalized * 30;
        }
        if (explosionTimer == true)
        {
            if (grenadeWasReflected == false)
            {
                if (grenadeRB.velocity.y >= explosionBounceMaxSpeed)
                {
                    grenadeRB.velocity = new Vector2(grenadeRB.velocity.x, explosionBounceMaxSpeed);
                }
                if (Mathf.Abs(grenadeRB.velocity.x) >= explosionBounceMaxSpeed)
                {
                    grenadeRB.velocity = new Vector2(Mathf.Sign(grenadeRB.velocity.x) * explosionBounceMaxSpeed, grenadeRB.velocity.y);
                }
            }
            else
            {
                if (grenadeRB.velocity.magnitude >= explosionBounceMaxSpeed * 3)
                {
                    grenadeRB.velocity = grenadeRB.velocity.normalized * explosionBounceMaxSpeed * 3;
                }
            }
        }

        if (remoteExplode)
        {
            StartCoroutine(DelayedExplode());
        }

        velocityList[2] = velocityList[1];
        velocityList[1] = velocityList[0];
        velocityList[0] = grenadeRB.velocity;

        Vector2 vectorToBox = (boxRB.position - grenadeRB.position).normalized;
        float distanceToBox = (boxRB.position - grenadeRB.position).magnitude;

        if (Box.pulseActive == true && distanceToBox <= Box.pulseRadius)
        {
            grenadeWasReflected = true;
            if (explosionTimer)
            {
                grenadeRB.velocity = -vectorToBox * 30;
            }
            else
            {
                grenadeRB.velocity = -vectorToBox * grenadeRB.velocity.magnitude;
            }
        }

        RaycastHit2D cast = Physics2D.CircleCast(grenadeRB.position, transform.localScale.x / 2, Vector2.zero, 0, LayerMask.GetMask("Box"));
        if (cast.collider != null)
        {
            if (explosionTimer == false)
            {
                Debug.DrawRay(grenadeRB.position, -grenadeRB.velocity.normalized, Color.green);
                Debug.DrawRay(grenadeRB.position, -vectorToBox.normalized, Color.red);
                grenadeRB.velocity = (-grenadeRB.velocity.normalized - vectorToBox.normalized).normalized * grenadeRB.velocity.magnitude;

                if (velocityList[2].magnitude >= 20)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = collisionDamage;
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - grenadeRB.position.x), 1).normalized;
                    StartCoroutine(GrenadeHitstop());
                }
                StartCoroutine(ExplosionTimer());
                StartCoroutine(GrenadeFlash());
                grenadeRB.gravityScale *= 0.2f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Hazards"))
        {
            Explosion();
        }

        if (explosionTimer == false && collision.contactCount > 0 && ((collision.contacts[0].normal.y >= 0.8f && 
            (1 << collision.gameObject.layer == LayerMask.GetMask("Obstacles") || 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")) && velocityList[2].y <= 0) || 
            1 << collision.gameObject.layer == LayerMask.GetMask("Box")))
        {
            StartCoroutine(ExplosionTimer());
            StartCoroutine(GrenadeFlash());
            grenadeRB.gravityScale *= 0.2f;
        }
        //if (1 << collision.gameObject.layer == LayerMask.GetMask("Box") && velocityList[2].magnitude >= 20)
        //{
        //    Box.activateDamage = true;
        //    Box.damageTaken = collisionDamage;
        //    Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - grenadeRB.position.x), 1).normalized;
        //    StartCoroutine(GrenadeHitstop());
        //}
        if (collision.transform.GetComponent<HitSwitch>() != null && velocityList[2].magnitude >= 20)
        {
            collision.transform.GetComponent<HitSwitch>().Hit();
        }
    }

    private void Explosion()
    {
        newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;
        newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        if (grenadeWasReflected)
        {
            newExplosion.GetComponent<Explosion>().damageEnemies = true;
        }
        explosionTimer = false;
        Destroy(gameObject);
    }

    IEnumerator ExplosionTimer()
    {
        explosionTimer = true;
        float timer = 0;
        while (timer <= explosionTime)
        {
            if (grenadeHitstopActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        Explosion();
    }
    IEnumerator GrenadeFlash()
    {
        grenadeColor = gameObject.GetComponent<Renderer>().material.color;
        while (explosionTimer == true)
        {
            while (grenadeColor.g <= 2)
            {
                grenadeColor.g += colorChangeSpeed * Time.deltaTime;
                gameObject.GetComponent<Renderer>().material.color = grenadeColor;
                yield return null;
            }
            while (grenadeColor.g >= 0.2f)
            {
                grenadeColor.g -= colorChangeSpeed * Time.deltaTime;
                gameObject.GetComponent<Renderer>().material.color = grenadeColor;
                yield return null;
            }
        }
    }
    IEnumerator GrenadeHitstop()
    {
        grenadeHitstopActive = true;
        Vector2 grenadeHitstopVelocity = grenadeRB.velocity;
        grenadeRB.velocity = new Vector2(0, 0);
        grenadeRB.isKinematic = true;
        if (Box.isCrouching)
        {
            yield return new WaitForSeconds(collisionDamage * Box.boxHitstopDelayMult * 0.6f);
        }
        else
        {
            yield return new WaitForSeconds(collisionDamage * Box.boxHitstopDelayMult);
        }
        grenadeRB.isKinematic = false;
        grenadeHitstopActive = false;
        grenadeRB.velocity = grenadeHitstopVelocity;
    }
    IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(0.02f);
        Explosion();
    }
}
