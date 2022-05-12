using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlginkrakSentinel : MonoBehaviour
{
    public Blginkrak bossScript;
    EnemyManager EM;
    Rigidbody2D bossRB;
    Rigidbody2D sentinelRB;
    Rigidbody2D boxRB;
    public int index;

    bool boomerangCR = false;
    float damage = 30;
    bool sentinelHitstopActive = false;

    bool explosionCR = false;

    LineRenderer line;

    void Start()
    {
        EM = GetComponent<EnemyManager>();
        sentinelRB = GetComponent<Rigidbody2D>();
        bossRB = bossScript.GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.SetPosition(0, sentinelRB.position); line.SetPosition(1, sentinelRB.position);
        line.enabled = false;

        EM.blastzoneDeath = false;
        EM.explosionsWillPush = false;
    }

    private void Update()
    {
        float radius = 0.4f;
        RaycastHit2D hitbox = Physics2D.CircleCast(sentinelRB.position, radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
        if (hitbox.collider != null)
        {
            if (bossScript.sentinelMoveSpeed <= bossScript.initialSentinelMoveSpeed &&
            bossScript.explosionAttack == false && bossScript.sentinelHitboxEnabled)
            {
                if (Box.isInvulnerable == false)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = damage;
                    if (boomerangCR)
                    {
                        Box.boxDamageDirection = new Vector2(Mathf.Sign(sentinelRB.velocity.x), 1).normalized;
                    }
                    else
                    {
                        Box.boxDamageDirection = Vector2.Perpendicular(sentinelRB.position - bossRB.position).normalized;
                    }
                    StartCoroutine(EnemyHitstop());
                }
            }
            else if (Box.boxHitboxActive)
            {
                EM.enemyWasDamaged = true;
                if (EM.enemyIsInvulnerable == false)
                {
                    Box.activateHitstop = true;
                }
            }
        }

        if (bossScript.sentinelMoveSpeed <= bossScript.initialSentinelMoveSpeed && bossScript.explosionAttack == false && bossScript.sentinelHitboxEnabled)
        {
            if (bossScript.debugEnabled)
            {
                Debug.DrawRay(sentinelRB.position + Vector2.up * radius, Vector2.down * radius * 2);
                Debug.DrawRay(sentinelRB.position + Vector2.right * radius, Vector2.left * radius * 2);
            }
        }
    }

    void FixedUpdate()
    {
        if (bossScript.sentinelBoomerang[index] == true && boomerangCR == false)
        {
            StartCoroutine(Boomerang());
        }
        if (bossScript.explosionAttack == true && explosionCR == false)
        {
            StartCoroutine(ExplosionBox());
        }

        if (boomerangCR && sentinelHitstopActive == false)
        {
            sentinelRB.rotation += 3000 * Time.fixedDeltaTime;
        }
    }

    IEnumerator EnemyHitstop()
    {
        sentinelHitstopActive = true;
        Vector2 enemyHitstopVelocity = sentinelRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        sentinelRB.velocity = new Vector2(0, 0);
        sentinelRB.angularVelocity /= enemyHitstopRotationSlowDown;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        sentinelRB.angularVelocity *= enemyHitstopRotationSlowDown;
        sentinelRB.velocity = enemyHitstopVelocity;
        sentinelHitstopActive = false;
    }
    IEnumerator Boomerang()
    {
        boomerangCR = true;
        float maxSpeed = 25;
        float minSpeed = 3;
        Vector2 vectorToBox = boxRB.position - sentinelRB.position;
        float distance = Mathf.Max(15, vectorToBox.magnitude + 8f);
        Vector2 target = sentinelRB.position + (vectorToBox.normalized * distance);
        while (Mathf.Abs((sentinelRB.position - target).magnitude) > 0.05f)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 2 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                //sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, target, speed * Time.fixedDeltaTime);
                sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        float window = 0.8f;
        float timer = 0;
        while (timer < window)
        {
            sentinelRB.velocity = Vector2.zero;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        while (Mathf.Abs((sentinelRB.position - (bossRB.position + bossScript.sentinelPositions[index])).magnitude) > 0.5f)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 2 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                //sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, bossRB.position + bossScript.sentinelPositions[index], speed * Time.fixedDeltaTime);
                sentinelRB.velocity = (bossRB.position + bossScript.sentinelPositions[index] - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        sentinelRB.velocity = Vector2.zero;
        boomerangCR = false;
        bossScript.sentinelBoomerang[index] = false;
        bossScript.sentinelIdle[index] = true;
    }
    IEnumerator ExplosionBox()
    {
        explosionCR = true;
        float distance = bossScript.distBetweenSentinels / 2;
        float raySpeed = 35f;
        float rayDistance = 0;
        float initialTimer = 0.8f;
        float timeBetweenRays = 0.3f;
        float timeToEvade = 0.3f;
        float timer = 0;
        while (bossScript.explosionBoundaryConnected == false && timer < initialTimer + timeBetweenRays * 5)
        {
            float rotation = -135 + 90 * index;
            float x = -1;
            if (index == 2 || index == 3) { x = 1; }
            float y = 1;
            if (index == 1 || index == 2) { y = -1; }
            Vector2 position = boxRB.position + new Vector2(x, y) * distance;
            if (timer < initialTimer + timeBetweenRays * 5 - timeToEvade)
            sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, position, Mathf.Min(50, 20 + 100 * timer) * Time.fixedDeltaTime);
            sentinelRB.rotation = Mathf.MoveTowardsAngle(sentinelRB.rotation, rotation, 250 * Time.fixedDeltaTime);

            if (index == 0 && timer > initialTimer + timeBetweenRays)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.down * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
                if ((rayPosition - bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position).magnitude < 0.05f)
                {
                    bossScript.boundaryConnected[index] = true;
                }
            }
            if (index == 1 && timer > initialTimer + timeBetweenRays * 2)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.right * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
                if ((rayPosition - bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position).magnitude < 0.05f)
                {
                    bossScript.boundaryConnected[index] = true;
                }
            }
            if (index == 2 && timer > initialTimer + timeBetweenRays * 3)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.up * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
                if ((rayPosition - bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position).magnitude < 0.05f)
                {
                    bossScript.boundaryConnected[index] = true;
                }
            }
            if (index == 3 && timer > initialTimer + timeBetweenRays * 4)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[0].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.left * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
                if ((rayPosition - bossScript.sentinels[0].GetComponent<Rigidbody2D>().position).magnitude < 0.05f)
                {
                    bossScript.boundaryConnected[index] = true;
                }
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        bossScript.explosionBoundaryConnected = true;
        yield return new WaitForSeconds(1f);
        Color color = line.startColor;
        Color initialColor = line.startColor;
        while (color.a > 0)
        {
            color.a -= 5 * Time.deltaTime;
            line.startColor = color;
            line.endColor = color;
            yield return null;
        }
        while (bossScript.explosionAttack)
        {
            yield return null;
        }
        explosionCR = false;
        line.enabled = false;
        line.startColor = initialColor;
        line.endColor = initialColor;

    }
}
