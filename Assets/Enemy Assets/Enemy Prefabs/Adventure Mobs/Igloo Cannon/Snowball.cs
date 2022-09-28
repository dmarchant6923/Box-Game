using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    Rigidbody2D rb;
    Rigidbody2D boxRB;

    public float radius = 4;
    public float freezeTime = 4;
    public float damage = 10;
    public float speed = 15;

    public GameObject snowflake;
    GameObject newSnowflake;

    public GameObject iceBlock;
    GameObject newIceBlock;

    public GameObject explosion;
    GameObject newExplosion;

    float maxTime = 4f;
    float timer = 0;

    bool snowballWasReflected = false;
    public bool aggro = false;

    int obstacleLM;
    int platformLM;
    int groundLM;
    int boxLM;
    int enemyLM;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        rb.angularVelocity = -Mathf.Sign(rb.velocity.x) * 1200;

        obstacleLM = LayerMask.GetMask("Obstacles");
        platformLM = LayerMask.GetMask("Platforms");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");

        if (aggro)
        {
            radius *= 1.3f;
            freezeTime *= 1.3f;
            damage *= 1.3f;
        }
    }

    private void Update()
    {
        if (Box.pulseActive == true && (boxRB.position - rb.position).magnitude <= Box.pulseRadius)
        {
            if (Tools.LineOfSight(boxRB.position, rb.position - boxRB.position) == true)
            {
                snowballWasReflected = true;
                rb.velocity = (rb.position - boxRB.position).normalized * rb.velocity.magnitude * 1.2f;
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit2D circleCast = Physics2D.CircleCast(rb.position, transform.localScale.x / 2, Vector2.zero, 0, boxLM);
        if (circleCast.collider != null && Box.dodgeInvulActive == false)
        {
            explode();
        }

        timer += Time.fixedDeltaTime;
        if (timer > maxTime)
        {
            Destroy(gameObject);
        }
    }

    void explode()
    {
        Vector2 explodePosition = rb.position - rb.velocity * Time.fixedDeltaTime;
        RaycastHit2D[] circleCast = Physics2D.CircleCastAll(explodePosition, radius, Vector2.zero, 0, LayerMask.GetMask("Enemies", "Box", "Enemy Ice Block", "Box Ice Block"));
        foreach (RaycastHit2D item in circleCast)
        {
            Vector2 vector = (item.collider.attachedRigidbody.position - explodePosition);
            if (Tools.LineOfSight(explodePosition, vector) == false)
            {
                continue;
            }

            if (item.collider.GetComponent<Box>() != null)
            {
                if (Box.isInvulnerable == false && Box.canBeFrozen == true && Box.boxWasFrozen == false && Box.boxHealth > 0)
                {
                    if (BoxPerks.shieldActive == false)
                    {
                        float velocity = 15;
                        Box.boxWasFrozen = true;
                        newIceBlock = Instantiate(iceBlock, boxRB.position, Quaternion.identity);
                        newIceBlock.GetComponent<Rigidbody2D>().rotation = boxRB.rotation;
                        if (Box.isCrouching)
                        {
                            velocity *= 0.6f;
                            freezeTime *= 0.6f;
                            damage *= 0.6f;
                        }
                        if (BoxPerks.heavyActive)
                        {
                            damage *= BoxPerks.heavyDamageMult;
                            velocity *= 0.92f;
                            freezeTime *= BoxPerks.heavyDamageMult;
                            newIceBlock.GetComponent<Rigidbody2D>().gravityScale *= 1.2f;
                        }
                        if (BoxPerks.speedActive)
                        {
                            velocity *= 1.2f;
                        }
                        newIceBlock.GetComponent<IceBlock>().freezeTime = freezeTime;
                        Box.boxHealth -= damage;
                        newIceBlock.GetComponent<IceBlock>().velocity = new Vector2(Mathf.Sign(boxRB.position.x - explodePosition.x), 1.2f).normalized * velocity;
                        newIceBlock.GetComponent<IceBlock>().angularVelocity = -Mathf.Sign(boxRB.position.x - explodePosition.x) * velocity * 40;
                        newIceBlock.GetComponent<IceBlock>().frozenRB = boxRB;
                        newIceBlock.layer = LayerMask.NameToLayer("Box Ice Block");
                        if (Box.boxHealth <= 0)
                        {
                            newIceBlock.GetComponent<IceBlock>().freezeTime = 0;
                        }
                    }
                    else
                    {
                        //BoxPerks.buffActive = false;
                        Box.activateDamage = true;
                        Box.damageTaken = damage;
                    }
                }
                else if (Box.frozen)
                {
                    if (BoxPerks.heavyActive)
                    {
                        damage *= BoxPerks.heavyDamageMult;
                    }
                    if (BoxPerks.shieldActive == false)
                    {
                        Box.boxHealth -= damage;
                    }
                }
            }

            if (item.transform.root.GetComponentInChildren<EnemyManager>() != null && item.transform.root.GetComponentInChildren<EnemyManager>().enemyIsFrozen == false
                && item.transform.root.GetComponentInChildren<EnemyManager>().enemyCanBeFrozen && item.transform.root.GetComponentInChildren<EnemyManager>().enemyIsInvulnerable == false)
            {
                if (snowballWasReflected)
                {
                    Rigidbody2D enemyRB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                    item.transform.root.GetComponentInChildren<EnemyManager>().enemyIsFrozen = true;
                    newIceBlock = Instantiate(iceBlock, enemyRB.position, Quaternion.identity);
                    //set freezetime for enemies in the ice block script
                    if (enemyRB.GetComponent<EnemyBehavior_Turret>() != null)
                    {
                        newIceBlock.GetComponent<Rigidbody2D>().isKinematic = true;
                        newIceBlock.GetComponent<Collider2D>().enabled = false;
                    }
                    else
                    {
                        newIceBlock.GetComponent<IceBlock>().velocity = enemyRB.velocity;
                        newIceBlock.GetComponent<IceBlock>().angularVelocity = -Mathf.Min(enemyRB.velocity.x * 100, 500);
                        newIceBlock.GetComponent<Rigidbody2D>().gravityScale = Mathf.Max(2, enemyRB.gravityScale);
                    }
                    newIceBlock.GetComponent<IceBlock>().frozenRB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                    newIceBlock.GetComponent<IceBlock>().EM = item.transform.root.GetComponentInChildren<EnemyManager>();

                    newIceBlock.transform.localScale *= enemyRB.transform.localScale.x * 1.2f;
                }
                else
                {
                    Rigidbody2D enemyRB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                    Vector2 forceVector = (enemyRB.position - rb.position).normalized;
                    enemyRB.AddForce(forceVector * 10, ForceMode2D.Impulse);
                }
            }

            if (item.transform.GetComponent<IceBlock>() != null)
            {
                Debug.Log("you are here");
                Rigidbody2D blockRB = item.transform.GetComponent<Rigidbody2D>();
                Vector2 forceVector = ((blockRB.position - rb.position).normalized + Vector2.up * 0.2f).normalized;
                blockRB.AddForce(forceVector * 10, ForceMode2D.Impulse);
            }
        }

        newExplosion = Instantiate(explosion, explodePosition, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionRadius = radius;
        newExplosion.GetComponent<Explosion>().aestheticExplosion = true;
        newExplosion.GetComponent<SpriteRenderer>().color = new Color(0.6f, 1, 1);
        if (FindObjectOfType<CameraFollowBox>() != null)
        {
            FindObjectOfType<CameraFollowBox>().StartCameraShake(7, (boxRB.position - explodePosition).magnitude);
        }

        int snowflakes = 12;
        for (int i = 0; i < snowflakes; i++)
        {
            float angle = i * 360f / snowflakes;
            Vector2 vector = Tools.AngleToVector(angle);
            newSnowflake = Instantiate(snowflake, explodePosition + vector * 0.5f, Quaternion.identity);
            newSnowflake.GetComponent<Rigidbody2D>().velocity = vector * 8;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == obstacleLM || (1 << collision.gameObject.layer == platformLM && rb.velocity.y < 0) ||
            collision.gameObject.GetComponent<EnemyManager>() != null)
        {
            if (collision.isTrigger)
            {
                return;
            }
            explode();
        }
    }
}
