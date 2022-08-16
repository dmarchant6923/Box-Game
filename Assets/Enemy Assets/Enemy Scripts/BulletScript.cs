using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    LayerMask boxLM;
    LayerMask enemyLM;
    float distanceToBox;
    Vector2 directionToBox;
    Rigidbody2D boxRB;
    Rigidbody2D bulletRB;
    public float bulletDespawnWindow;
    float bulletTimer = 0;
    float bulletRotation;
    public float bulletDamage = 1;
    public bool bulletWasReflected;
    public bool bulletCanBeReflected = true;

    public bool heatSeeking = false;
    float angularVelocity = 60;
    RaycastHit2D rayToBox;
    int obstacleAndBoxLM;
    bool canSeeBox;

    public bool aggro = false;

    public bool explodingBullets = false;
    float explosionRadius = 2.6f;
    float explosionDamage = 25;
    public float explosionContactDamage;
    public GameObject explosion;
    GameObject newExplosion;
    bool explodeAtPosition = false;
    Vector2 explodePosition;

    bool flickerActive = false;
    float flickerMult = 0.85f;

    bool touchedWall = false;
    bool touchedReflect = false;

    public bool bulletCosmetic = false;

    private void Start()
    {
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        bulletRB = gameObject.GetComponent<Rigidbody2D>();
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        if (explodingBullets)
        {
            Color color = transform.GetComponent<SpriteRenderer>().color;
            color.g /= 2;
            color.b /= 2;
            transform.GetComponent<SpriteRenderer>().color = color;
        }
        if (heatSeeking)
        {
            Color color = transform.GetComponent<SpriteRenderer>().color;
            color = new Color(Mathf.Min(color.r, 1), Mathf.Min(color.g, 1), Mathf.Min(color.b, 1));
            color.r /= 3f;
            color.g *= 2;
            color.b += 0.5f;
            transform.GetComponent<SpriteRenderer>().color = color;
            bulletDespawnWindow *= 2;
        }
        if (aggro)
        {
            Color color = transform.GetComponent<SpriteRenderer>().color;
            color = new Color(Mathf.Min(color.r, 1), Mathf.Min(color.g, 1), Mathf.Min(color.b, 1));
            color.r += 0.25f;
            color.g /= 1.5f;
            color.b /= 1.5f;
            transform.GetComponent<SpriteRenderer>().color = color;
        }

    }
    void Update()
    {
        if (touchedWall && touchedReflect == false)
        {
            if ((bulletRB.position - boxRB.position).magnitude < 0.1f)
            {
                bulletHitPlayer();
            }
            else
            {
                DestroyBullet(false);
            }
        }
        touchedWall = false;
        touchedReflect = false;
        explodeAtPosition = false;

        if (bulletRB.velocity.magnitude >= 35 && bulletCosmetic == false)
        {
            transform.localScale = new Vector2(transform.localScale.x, 0.4f);
        }

        if (bulletCosmetic == false)
        {
            bulletRotation = -Mathf.Atan2(bulletRB.velocity.x, bulletRB.velocity.y) * Mathf.Rad2Deg;
            transform.eulerAngles = Vector3.forward * (bulletRotation);
        }
        distanceToBox = (boxRB.position - bulletRB.position).magnitude;
        directionToBox = (boxRB.position - bulletRB.position).normalized;

        if (Box.pulseActive == true && distanceToBox <= Box.pulseRadius && bulletCanBeReflected)
        {
            bulletWasReflected = true;
            bulletRB.velocity = -directionToBox * bulletRB.velocity.magnitude * Box.projectilePulseMagnitude;
            bulletDamage *= 1.2f;
            bulletTimer = 0;
        }

        rayToBox = Physics2D.Raycast(bulletRB.position, directionToBox, 100, obstacleAndBoxLM);
        if (rayToBox.collider != null && 1 << rayToBox.collider.gameObject.layer == boxLM)
        {
            canSeeBox = true;
        }

        if (heatSeeking && canSeeBox && bulletWasReflected == false)
        {
            float angleToBox = -Mathf.Atan2(directionToBox.x, directionToBox.y) * Mathf.Rad2Deg;
            angleToBox = Mathf.MoveTowardsAngle(bulletRotation, angleToBox, angularVelocity * Time.deltaTime);
            Vector2 velocityVector = new Vector2(Mathf.Cos(angleToBox * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angleToBox * Mathf.Deg2Rad + Mathf.PI / 2));
            bulletRB.velocity = velocityVector.normalized * bulletRB.velocity.magnitude;
        }

        bulletTimer += Time.deltaTime;
        if (bulletTimer >= bulletDespawnWindow)
        {
            DestroyBullet(false);
        }
        if (bulletTimer >= bulletDespawnWindow * flickerMult && flickerActive == false)
        {
            flickerActive = true;
            StartCoroutine(Flicker());
        }
    }

    private void FixedUpdate()
    {
        Vector2 vel = bulletRB.velocity.normalized;
        float frameDistance = bulletRB.velocity.magnitude * Time.deltaTime;
        RaycastHit2D raycast = Physics2D.Raycast(bulletRB.position, vel, frameDistance, LayerMask.GetMask("Obstacles"));


        if (raycast.collider != null && raycast.collider.gameObject.tag == "Reflect")
        {
            Vector2 newVel = vel;
            Vector2 newPosition = bulletRB.position;
            int i = 0;
            while (raycast.collider != null && raycast.collider.gameObject.tag == "Reflect" && i <= 3)
            {
                frameDistance -= raycast.distance;
                i++;

                Vector2 perp = raycast.normal;
                Vector2 parallel = Vector2.Perpendicular(perp);
                Vector2 perpComponent = Vector2.Dot(newVel, perp) * perp;
                Vector2 parallelComponent = Vector2.Dot(newVel, parallel) * parallel;

                perpComponent *= -1;
                newVel = (perpComponent + parallelComponent).normalized;
                //Debug.DrawRay(raycast.point, newVel * frameDistance, Color.yellow);
                //Debug.DrawRay(raycast.point, raycast.normal, Color.green);

                Vector2 raycastPoint = raycast.point;
                raycast = Physics2D.Raycast(raycast.point + newVel * frameDistance * 0.05f, newVel, frameDistance, LayerMask.GetMask("Obstacles"));
                if (raycast.collider == null || frameDistance <= 0)
                {
                    newPosition = raycastPoint;
                    break;
                }
                //if (i == 3)
                //{
                //    Debug.Log("Max Iterations");
                //}
            }
            bulletRB.velocity = newVel * bulletRB.velocity.magnitude;
            bulletRB.position = newPosition + newVel * (bulletRB.velocity.magnitude * -Time.deltaTime * 3/4);
        }
        //else if(raycast.collider != null && raycast.collider.tag != "Reflect" && raycast.collider.tag != "Fence")
        //{
        //    DestroyBullet(false);
        //    Debug.Log("destroyed by raycast");
        //}
    }

    public void bulletHitPlayer()
    {
        Box.damageTaken = bulletDamage;
        Box.boxDamageDirection = new Vector2(Mathf.Sign(bulletRB.velocity.x), 1).normalized;
        Box.activateDamage = true;
        explosionDamage = bulletDamage / 1.6f;
        if (explodingBullets)
        {
            Box.boxWasBurned = true;
        }
        DestroyBullet(true);
    }

    public void DestroyBullet(bool aestheticExplosion)
    {
        if (explodingBullets == true)
        {
            if (aestheticExplosion)
            {
                Explosion(explosionDamage, true);
            }
            else
            {
                Explosion(explosionDamage, false);
            }
        }
        if (bulletCosmetic == false)
        {
            Destroy(gameObject);
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
            GetComponent<SpriteRenderer>().enabled = false;
            bulletRB.position += bulletRB.velocity * Time.fixedDeltaTime;
            bulletRB.velocity = Vector2.zero;
            bulletTimer = 0;
            StartCoroutine(DelayDestroy());
        }
    }

    private void Explosion(float explosionDamage, bool aesthetic)
    {
        Vector2 position = (explodeAtPosition) ? explodePosition : bulletRB.position - bulletRB.velocity * Time.deltaTime / 2;
        newExplosion = Instantiate(explosion, position, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;
        if (bulletWasReflected)
        {
            newExplosion.GetComponent<Explosion>().damageEnemies = true;
        }
        if (aesthetic)
        {
            newExplosion.GetComponent<Explosion>().aestheticExplosion = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RaycastHit2D boxRC = Physics2D.Raycast(bulletRB.position - bulletRB.velocity.normalized * transform.lossyScale.y,
            bulletRB.velocity.normalized, transform.lossyScale.y * 2, boxLM);
        if ((1 << collision.gameObject.layer == boxLM || boxRC.collider != null) && bulletWasReflected == false && Box.dodgeInvulActive == false)
        {
            bulletHitPlayer();
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        RaycastHit2D boxRC = Physics2D.Raycast(bulletRB.position - bulletRB.velocity.normalized * transform.lossyScale.y, 
            bulletRB.velocity.normalized, transform.lossyScale.y * 2, boxLM);
        if ((1 << collision.gameObject.layer == boxLM || boxRC.collider != null) && bulletWasReflected == false && Box.dodgeInvulActive == false)
        {
            bulletHitPlayer();
        }
        else if (1 << collision.gameObject.layer == boxLM && bulletWasReflected == true)
        {

        }
        else if (1 << collision.gameObject.layer == enemyLM && bulletWasReflected == true)
        {
            if (collision.GetComponent<EnemyManager>() != null && collision.GetComponent<EnemyManager>().reflectedBulletsWillDamage == true)
            {
                collision.GetComponent<EnemyManager>().enemyWasDamaged = true;
            }
            else if (collision.GetComponentInParent<EnemyManager>() != null && collision.GetComponentInParent<EnemyManager>().reflectedBulletsWillDamage == true)
            {
                collision.GetComponentInParent<EnemyManager>().enemyWasDamaged = true;
            }
            DestroyBullet(false);
        }
        else if (1 << collision.gameObject.layer == enemyLM && bulletWasReflected == false)
        {

        }
        else if (1 << collision.gameObject.layer != LayerMask.GetMask("Pulse") && collision.isTrigger == false && collision.tag != "Fence")
        {
            if (1 << collision.gameObject.layer == LayerMask.GetMask("Obstacles"))
            {
                touchedWall = true;
                RaycastHit2D wallCast = Physics2D.Raycast(bulletRB.position - bulletRB.velocity * Time.deltaTime * 2, bulletRB.velocity, bulletRB.velocity.magnitude * Time.deltaTime * 4,
                    LayerMask.GetMask("Obstacles"));
                if (wallCast.collider != null)
                {
                    explodeAtPosition = true;
                    explodePosition = wallCast.point - bulletRB.velocity.normalized * 0.05f;
                }

                if (collision.gameObject.tag == "Reflect")
                {
                    touchedReflect = true;
                }
            }
            else
            {
                if ((bulletRB.position - boxRB.position).magnitude < 0.1f)
                {
                    bulletHitPlayer();
                }
                else
                {
                    DestroyBullet(false);
                }
            }
        }

        if (collision.transform.root.GetComponent<HitSwitch>() != null)
        {
            collision.transform.root.GetComponent<HitSwitch>().Hit();
            DestroyBullet(false);
        }
    }

    IEnumerator Flicker()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        while (true)
        {
            if (sprite != null)
            {
                sprite.enabled = false;
                yield return new WaitForSeconds(0.04f);
            }
            if (sprite != null)
            {
                sprite.enabled = true;
                yield return new WaitForSeconds(0.06f);
            }
        }
    }
    IEnumerator DelayDestroy()
    {
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }

}
