using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarManProjectile : MonoBehaviour
{
    Rigidbody2D rb;
    Rigidbody2D boxRB;

    public GameObject starMan;
    GameObject newStarMan;

    public GameObject starBullet;
    GameObject newStarBullet;

    public GameObject explosion;
    GameObject newExplosion;
    public float explosionRadius = 2.5f;
    float explosionDamage = 30;

    float timer = 0;
    float maxDist = 40;
    bool spawnStarted = false;

    bool projectileWasReflected = false;

    public int iteration = 0;

    public bool aggroActive = false;

    public bool debugEnabled = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        rb.angularVelocity = 1000;
        Color color = GetComponent<SpriteRenderer>().color;
        GetComponent<TrailRenderer>().startColor = color;
        color.a = 0;
        GetComponent<TrailRenderer>().endColor = color;

        StartCoroutine(StarParticles());
    }

    private void Update()
    {
        float distanceToBox = (boxRB.position - rb.position).magnitude;
        Vector2 directionToBox = (boxRB.position - rb.position).normalized;
        if (Box.pulseActive == true && distanceToBox <= Box.pulseRadius)
        {
            projectileWasReflected = true;
            rb.velocity = -directionToBox * rb.velocity.magnitude * Box.projectilePulseMagnitude;
            explosionDamage *= 1.2f;
            timer = 0;
        }
    }

    void FixedUpdate()
    {
        if (spawnStarted == false)
        {
            RaycastHit2D circleCast = Physics2D.CircleCast(rb.position, GetComponent<CircleCollider2D>().radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
            if (circleCast.collider != null)
            {
                Explode();
            }

            timer += Time.fixedDeltaTime;

            if (timer > maxDist / rb.velocity.magnitude)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        spawnStarted = true;
        Vector2 position = rb.position;
        Vector2 explodePosition = rb.position;
        Vector2 posPrevF = position - (rb.velocity) * Time.fixedDeltaTime;
        float distance = (rb.velocity * Time.fixedDeltaTime * 2).magnitude;
        RaycastHit2D cast = Physics2D.CircleCast(posPrevF, GetComponent<CircleCollider2D>().radius, rb.velocity, distance, LayerMask.GetMask("Obstacles", "Platforms", "Box"));
        if (cast.collider != null)
        {
            position = cast.point + cast.normal * 1f;
            explodePosition = cast.point;
        }
        newStarMan = Instantiate(starMan, position, Quaternion.identity);
        newStarMan.GetComponent<StarMan>().debugEnabled = debugEnabled;
        newStarMan.GetComponent<StarMan>().iteration = iteration;

        if (FindObjectOfType<BattlegroundManager>() != null)
        {
            FindObjectOfType<BattlegroundManager>().spawnedEnemies.Add(newStarMan);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        rb.velocity = Vector2.zero;
        StartCoroutine(DelayedDestroy(explodePosition));
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlatformDrop>() != null && rb.velocity.y > 0)
        {

        }
        else if (collision.GetComponent<MovingObjects>() != null && collision.GetComponent<MovingObjects>().interactableObstacle && spawnStarted == false)
        {
            Explode();
        }

    }
    IEnumerator StarParticles()
    {
        float interval = 0.1f;

        while (GetComponent<SpriteRenderer>().enabled)
        {
            float randInterval = interval * 0.5f + interval * Random.Range(0f, 1f);
            yield return new WaitForSeconds(randInterval);
            newStarBullet = Instantiate(starBullet, rb.position + Random.insideUnitCircle * transform.localScale.x / 2, Quaternion.identity);
            newStarBullet.GetComponent<BulletScript>().bulletDespawnWindow = 5;
            newStarBullet.GetComponent<BulletScript>().bulletDamage = 0;
            newStarBullet.GetComponent<Rigidbody2D>().velocity = Vector2.down * 5;
            newStarBullet.GetComponent<StarBullet>().aestheticBullet = true;
        }
    }
    IEnumerator DelayedDestroy(Vector2 explodePosition)
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        newExplosion = Instantiate(explosion, explodePosition, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;
        if (projectileWasReflected)
        {
            newExplosion.GetComponent<Explosion>().damageEnemies = true;
        }

        yield return new WaitForSeconds(0.35f);
        Destroy(gameObject);
    }
}
