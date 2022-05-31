using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeSentinel : MonoBehaviour
{
    public SpikeSentry bossScript;
    GameObject boss;
    EnemyManager EM;
    Rigidbody2D bossRB;
    Rigidbody2D sentinelRB;
    Rigidbody2D boxRB;

    public GameObject hitboxGlow;

    public int index;

    bool boomerangCR = false;
    public float damage = 25;
    public float maxSpeed = 20;
    public float minSpeed = 1;
    [System.NonSerialized] public float waitTime = 0.4f;
    public bool hitboxActive = false;
    bool sentinelHitstopActive = false;

    Vector2 posPrevFrame;

    public bool reflectActive = false;
    public float reflectTimer = 0;
    float reflectWindow = 1.5f;
    public bool willDamageEnemies = false;

    // Start is called before the first frame update
    void Start()
    {
        EM = GetComponent<EnemyManager>();
        sentinelRB = GetComponent<Rigidbody2D>();
        bossRB = bossScript.GetComponent<Rigidbody2D>();
        boss = bossScript.gameObject;
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        EM.blastzoneDeath = false;
        EM.explosionsWillPush = false;

        hitboxGlow = transform.GetChild(0).gameObject;
        hitboxGlow.SetActive(false);

        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (EM.enemyWasKilled)
        {
            Destroy(this);
        }

        if (reflectActive == false && boss.GetComponent<EnemyManager>().enemyWasKilled && EM.enemyWasKilled == false)
        {
            EM.enemyHealth = 0;
            EM.enemyWasDamaged = true;
            StartCoroutine(SentinelDeath());
        }

        if (bossScript.sentinelBoomerang[index] == true && boomerangCR == false && reflectActive == false)
        {
            StartCoroutine(Boomerang());
        }

        if ((boomerangCR || reflectActive) && sentinelHitstopActive == false && bossScript.sentinelsReturned[index] == false && EM.hitstopImpactActive == false)
        {
            sentinelRB.angularVelocity = 2000;
        }
        else
        {
            sentinelRB.angularVelocity = 0;
        }

        if (Box.pulseActive == true && reflectActive == false)
        {
            Vector2 vectorToBox = boxRB.position - sentinelRB.position;
            RaycastHit2D[] pulseRC = Physics2D.RaycastAll(sentinelRB.position, vectorToBox.normalized, Box.pulseRadius, LayerMask.GetMask("Obstacles", "Box"));
            float distToBox = 1000;
            float distToObstacle = 1000;
            foreach (RaycastHit2D col in pulseRC)
            {
                if (col.collider != null && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Box"))
                {
                    distToBox = col.distance;
                }
                if (col.collider != null && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles") && col.collider.gameObject.tag != "Fence")
                {
                    distToObstacle = Mathf.Min(col.distance, distToObstacle);
                }
            }
            if (pulseRC.Length > 0 && distToBox < distToObstacle && boomerangCR)
            {
                if (reflectActive == false)
                {
                    StartCoroutine(ReflectBoomerang());
                }
                else if (reflectActive && willDamageEnemies == false)
                {
                    reflectTimer = 0;
                    willDamageEnemies = true;
                    sentinelRB.velocity = -vectorToBox.normalized * sentinelRB.velocity.magnitude;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        float radius = transform.localScale.y * 0.6f;

        int casts = 2;
        for (int i = 0; i < casts; i++)
        {
            bool connected = false;
            Vector2 difference = sentinelRB.position - posPrevFrame;
            Vector2 increment = difference / casts;
            Vector2 position = posPrevFrame + (increment * (i + 1));
            if (reflectActive == false || (reflectActive && willDamageEnemies == false))
            {
                RaycastHit2D hitbox = Physics2D.CircleCast(position, radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
                if (hitbox.collider != null)
                {
                    if (hitboxActive && EM.enemyWasKilled == false && BoxPerks.starActive == false)
                    {
                        if (Box.isInvulnerable == false)
                        {
                            Box.activateDamage = true;
                            Box.damageTaken = damage;
                            Box.boxDamageDirection = new Vector2(Mathf.Sign(sentinelRB.velocity.x), 1).normalized;
                            if (bossScript.sentinelIdle[index])
                            {
                                Box.boxDamageDirection = new Vector2(Mathf.Sign(bossScript.sentinelPositions[index].x), 1).normalized;
                            }
                            StartCoroutine(EnemyHitstop(true));
                            connected = true;
                        }
                    }
                    else if (Box.boxHitboxActive || BoxPerks.starActive)
                    {
                        EM.enemyWasDamaged = true;
                        if (EM.enemyIsInvulnerable == false)
                        {
                            Box.activateHitstop = true;
                        }
                        connected = true;
                    }
                }
            }
            else if (willDamageEnemies)
            {
                RaycastHit2D[] hitbox = Physics2D.CircleCastAll(position, radius, Vector2.zero, 0, LayerMask.GetMask("Enemies"));
                foreach (RaycastHit2D enemy in hitbox)
                {
                    if (enemy.collider.GetComponent<EnemyManager>() != null && enemy.collider.GetComponent<EnemyManager>().enemyIsInvulnerable == false)
                    {
                        enemy.collider.GetComponent<EnemyManager>().enemyWasDamaged = true;
                        StartCoroutine(EnemyHitstop(false));
                        connected = true;
                        break;
                    }
                }
            }

            if (connected)
            {
                break;
            }

            if (bossScript.debugEnabled)
            {
                Debug.DrawRay(position + Vector2.up * radius, Vector2.down * radius * 2);
                Debug.DrawRay(position + Vector2.right * radius, Vector2.left * radius * 2);
            }
        }

        posPrevFrame = sentinelRB.position;
    }

    IEnumerator DelayedStart()
    {
        yield return null;
        for (int i = 0; i < boss.GetComponent<EnemyManager>().enemyChildren.Length; i++)
        {
            if (boss.GetComponent<EnemyManager>().enemyChildren[i] == transform)
            {
                boss.GetComponent<EnemyManager>().enemyChildren[i] = null;
            }
        }
        for (int i = 0; i < boss.GetComponent<EnemyManager>().enemyObjects.Count; i++)
        {
            if (boss.GetComponent<EnemyManager>().enemyObjects[i] == GetComponent<SpriteRenderer>())
            {
                boss.GetComponent<EnemyManager>().enemyObjects.RemoveAt(i);
                boss.GetComponent<EnemyManager>().enemyColors.RemoveAt(i);
            }
        }
    }
    IEnumerator SentinelDeath()
    {
        yield return null;
        while (EM.hitstopImpactActive)
        {
            yield return null;
        }
        sentinelRB.velocity += new Vector2(Mathf.Sign(sentinelRB.position.x - bossRB.position.x) * Random.Range(3, 10f), Random.Range(0, 5f));
        Destroy(this);
    }
    IEnumerator EnemyHitstop(bool damagedBox)
    {
        sentinelHitstopActive = true;
        Vector2 enemyHitstopVelocity = sentinelRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        sentinelRB.velocity = new Vector2(0, 0);
        sentinelRB.angularVelocity /= enemyHitstopRotationSlowDown;
        yield return null;
        if (damagedBox)
        {
            while (Box.boxHitstopActive)
            {
                sentinelRB.velocity = new Vector2(0, 0);
                yield return null;
            }
        }
        else
        {
            float window = Box.enemyHitstopDelay;
            float timer = 0;
            while (timer < window)
            {
                sentinelRB.velocity = new Vector2(0, 0);
                timer += Time.deltaTime;
                yield return null;
            }
        }
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        sentinelRB.angularVelocity *= enemyHitstopRotationSlowDown;
        sentinelRB.velocity = enemyHitstopVelocity;
        Debug.Log("you are here");
        yield return null;
        sentinelHitstopActive = false;
    }
    IEnumerator Boomerang()
    {
        boomerangCR = true;
        hitboxActive = true;
        hitboxGlow.SetActive(true);
        Vector2 vectorToBox = bossScript.visibleVectorToBox;
        float distance = Mathf.Max(8, vectorToBox.magnitude + 5f);
        distance = Mathf.Min(distance, bossScript.initialSightRadius * 1.5f);
        Vector2 target = sentinelRB.position + (vectorToBox.normalized * distance);
        while (Mathf.Abs((sentinelRB.position - target).magnitude) > 0.1f && EM.enemyWasKilled == false && reflectActive == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        float timer = 0;
        Vector2 direction = sentinelRB.velocity.normalized;
        while (timer < waitTime && EM.enemyWasKilled == false && reflectActive == false)
        {
            sentinelRB.velocity -= direction * (minSpeed * 2 / waitTime) * Time.deltaTime;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        while ((sentinelRB.position - bossRB.position).magnitude > 1.5f && EM.enemyWasKilled == false && reflectActive == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (bossRB.position - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        while ((sentinelRB.position - (bossRB.position + bossScript.sentinelPositions[index])).magnitude > 0.5f && EM.enemyWasKilled == false && reflectActive == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (bossRB.position + bossScript.sentinelPositions[index] - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        if (reflectActive == false)
        {
            hitboxActive = false;
            hitboxGlow.SetActive(false);
            sentinelRB.velocity = Vector2.zero;
            bossScript.sentinelBoomerang[index] = false;
            bossScript.sentinelIdle[index] = true;
            while (bossScript.boomerangAttack)
            {
                yield return null;
            }
        }
        boomerangCR = false;
    }
    IEnumerator ReflectBoomerang()
    {
        reflectActive = true;
        hitboxActive = true;
        willDamageEnemies = true;
        hitboxGlow.SetActive(true);
        bossScript.sentinelsKilled[index] = true;
        float curveSpeed = 70;
        Vector2 newDirection = (sentinelRB.position - boxRB.position).normalized;
        Vector2 initialDirection = sentinelRB.velocity.normalized;
        Vector2 perpInitialDirection = Vector2.Perpendicular(initialDirection);
        int curveDirection = 1;
        if (Vector2.Dot(newDirection, perpInitialDirection) > 0)
        {
            curveDirection = -1;
        }

        //int curveDirection = (Random.Range(0, 2) * 2) - 1;
        float angle = -Mathf.Atan2(newDirection.x, newDirection.y) * Mathf.Rad2Deg;
        float speed = 20f;
        float acceleration = 8f;
        sentinelRB.velocity = newDirection * speed;

        reflectTimer = 0;
        while (reflectTimer < reflectWindow && EM.enemyWasKilled == false)
        {
            if (sentinelHitstopActive == false)
            {
                angle = -Mathf.Atan2(sentinelRB.velocity.x, sentinelRB.velocity.y) * Mathf.Rad2Deg;
                angle += curveSpeed * curveDirection * Time.deltaTime;
                speed += acceleration * Time.deltaTime;
                newDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                sentinelRB.velocity = newDirection * speed;
            }
            bossScript.sentinelsReturned[index] = false;

            //if ((sentinelRB.position - boxRB.position).magnitude > 30)
            //{
            //    break;
            //}

            reflectTimer += Time.deltaTime;
            yield return null;
        }
        if (EM.enemyWasKilled == false)
        {
            Destroy(gameObject);
        }
    }
}
