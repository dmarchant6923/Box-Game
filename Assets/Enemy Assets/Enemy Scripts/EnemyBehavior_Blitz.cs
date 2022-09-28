using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Blitz : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D enemyRB;

    Rigidbody2D boxRB;

    public float initialRadius = 16;
    float radius;
    RaycastHit2D[] enemyRC_RayToBox;
    bool canSeeBox = false;
    Vector2 vectorToBox;
    Vector2 visibleVectorToBox;
    float angleToBox;

    bool touchingThisEnemy = false;

    public float attackDelay = 1.2f;
    float maxDistance = 30f;
    public float attackCooldownTime = 2f;
    Vector2 vectorFacing;
    float angularVelocity;
    public float normalAngularVelocity = 400;
    public float attackAngularVelocity = 40;
    public float launchSpeed = 25f;
    public float attackAccel = 40f;
    bool blitzActive = false;
    bool delayActive = false;
    bool attackActive = false;
    bool blitzCollision = false;
    Vector2 collisionDirection;
    bool attackCooldown = false;
    bool knockbackActive = false;
    public int numAttacks = 1;
    int attacksLeft;


    Transform fireTransform;
    SpriteRenderer fireSprite;
    float initialFireScale;
    float activeFireScale = 1.4f;

    public GameObject smoke;

    public float damage = 40;
    bool enemyHitstopActive = false;
    bool ignoreBoxLaunchDamage = false;

    bool attackWasPulsed = false;

    float initialDrag;

    bool aggroActive = false;

    bool idle = true;
    bool idleCR = false;
    bool idleMoveActive = false;
    public float idleWaitTime = 4;
    float idleRadius = 7;
    float idleOffset = 2;

    Vector2[] positionList = new Vector2[3];


    int obstacleAndBoxLM;
    int obstacleLM;
    int boxLM;

    public bool debugEnabled = false;

    void Start()
    {
        EM = GetComponent<EnemyManager>();
        enemyRB = GetComponent<Rigidbody2D>();
        fireTransform = transform.GetChild(0);
        fireSprite = fireTransform.GetComponent<SpriteRenderer>();
        initialFireScale = fireTransform.localScale.y;

        initialDrag = enemyRB.drag;

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        radius = initialRadius;
        canSeeBox = false;
        touchingThisEnemy = false;
        angularVelocity = normalAngularVelocity;
        blitzActive = false;
        attackActive = false;
        enemyHitstopActive = false;
        ignoreBoxLaunchDamage = false;
        attacksLeft = numAttacks;
        visibleVectorToBox = Vector2.up;

        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        obstacleLM = LayerMask.GetMask("Obstacles");
        boxLM = LayerMask.GetMask("Box");
    }

    void Update()
    {
        //logic for player to physically kill enemy
        bool thisEnemyFound = false;
        foreach (RaycastHit2D enemy in Box.attackRayCast)
        {
            if (enemy.collider != null && enemy.collider.gameObject == this.gameObject)
            {
                thisEnemyFound = true;
            }
        }
        if (thisEnemyFound)
        {
            touchingThisEnemy = true;
            EM.touchingThisEnemy = true;
        }
        else
        {
            touchingThisEnemy = false;
            EM.touchingThisEnemy = false;
        }
        if (touchingThisEnemy == true && enemyHitstopActive == false)
        {
            if (attackActive && Box.isInvulnerable == false && ignoreBoxLaunchDamage == false && BoxPerks.spikesActive == false)
            {
                Box.activateDamage = true;
                Box.damageTaken = damage;
                Box.boxDamageDirection = new Vector2(Mathf.Sign(enemyRB.velocity.x), 1).normalized;
                if (EM.shockActive)
                {
                    Box.activateShock = true;
                }
                StartCoroutine(EnemyHitstop());
            }
            else if (Box.boxHitboxActive && (attackActive == false || Box.isInvulnerable) && ignoreBoxLaunchDamage == false)
            {
                EM.enemyWasDamaged = true;
                if (EM.enemyIsInvulnerable == false)
                {
                    Box.activateHitstop = true;
                }
            }
        }

        if (EM.enemyIsFrozen)
        {
            attackActive = false;
            idle = false;
            return;
        }

        //detecting box
        vectorToBox = (boxRB.position - enemyRB.position).normalized;
        enemyRC_RayToBox = Physics2D.RaycastAll(enemyRB.position, vectorToBox, radius, obstacleAndBoxLM);
        //canSeeItem, and keeping the barrel angle still once canSeeItem = false
        float distToBox = 1000;
        float distToObstacle = 1000;
        foreach (RaycastHit2D col in enemyRC_RayToBox)
        {
            if (col.collider != null && 1 << col.collider.gameObject.layer == boxLM)
            {
                distToBox = col.distance;
            }
            if (col.collider != null && 1 << col.collider.gameObject.layer == obstacleLM && col.collider.gameObject.tag != "Fence")
            {
                distToObstacle = Mathf.Min(col.distance, distToObstacle);
            }
        }
        if (distToBox < 1000 && distToBox < distToObstacle)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            visibleVectorToBox = vectorToBox;
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }



        angleToBox = -Mathf.Atan2(visibleVectorToBox.x, visibleVectorToBox.y) * Mathf.Rad2Deg;
        if (delayActive || attackActive)
        {
            enemyRB.rotation = Mathf.MoveTowardsAngle(enemyRB.rotation, angleToBox, angularVelocity * Time.deltaTime);
        }
        if (canSeeBox && EM.hitstopImpactActive == false && attackCooldown == false && enemyHitstopActive == false && EM.initialDelay == false)
        {
            if (blitzActive == false)
            {
                StartCoroutine(BlitzAttack());
            }
        }
        vectorFacing = new Vector2(Mathf.Cos(enemyRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(enemyRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;

        if (Box.pulseActive && (boxRB.position - enemyRB.position).magnitude <= Box.pulseRadius + 0.5f && attackActive && attackWasPulsed == false)
        {
            attackWasPulsed = true;
            collisionDirection = (enemyRB.position - boxRB.position).normalized;
        }



        if (debugEnabled)
        {
            Color color = Color.white;
            if (canSeeBox) { color = Color.red; }
            Debug.DrawRay(enemyRB.position + Vector2.down * radius, Vector2.up * radius * 2, color);
            Debug.DrawRay(enemyRB.position + Vector2.left * radius, Vector2.right * radius * 2, color);

            Debug.DrawRay(enemyRB.position, vectorFacing * radius);
        }
    }

    private void FixedUpdate()
    {
        if (EM.enemyIsFrozen)
        {
            return;
        }

        RaycastHit2D circleCast = Physics2D.CircleCast(enemyRB.position, idleOffset, Vector2.zero, 0, LayerMask.GetMask("Obstacles", "Platforms", "Enemies"));
        if ((knockbackActive == false || (knockbackActive && attacksLeft > 0)) && delayActive == false && attackActive == false && circleCast.collider != null)
        {
            for (int i = 0; i < 17; i++)
            {
                float angle = i * 22.5f;
                float offset = 0.9f;
                Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                RaycastHit2D cast = Physics2D.Raycast(enemyRB.position + vector * offset, vector, idleOffset - vector.magnitude * offset, 
                    LayerMask.GetMask("Obstacles", "Platforms", "Enemies"));
                Color color = Color.white;
                if (cast.collider != null)
                {
                    enemyRB.velocity -= vector * 0.1f;
                    color = Color.blue;
                }
                if (debugEnabled)
                {
                    Debug.DrawRay(enemyRB.position + vector * offset, vector * (idleOffset - vector.magnitude * offset), color);
                }
            }
        }

        if (idle && idleCR == false)
        {
            StartCoroutine(IdleMovement());
        }

        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            damage *= EM.aggroIncreaseMult;
            attackAccel *= EM.aggroIncreaseMult;
            attackDelay *= EM.aggroDecreaseMult;
            attackCooldownTime *= EM.aggroDecreaseMult;
            attackAngularVelocity *= EM.aggroIncreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;

            damage /= EM.aggroIncreaseMult;
            attackAccel /= EM.aggroIncreaseMult;
            attackDelay /= EM.aggroDecreaseMult;
            attackCooldownTime /= EM.aggroDecreaseMult;
            attackAngularVelocity /= EM.aggroIncreaseMult;
        }

        if (debugEnabled)
        {
            if (idle)
            {
                transform.GetChild(2).GetComponent<SpriteRenderer>().color = Color.green;
            }
            else
            {
                transform.GetChild(2).GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        if (enemyHitstopActive == false)
        {
            positionList[2] = positionList[1];
            positionList[1] = positionList[0];
            positionList[0] = enemyRB.position;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (attackActive && collision.transform.root.GetComponent<EnemyManager>() == null)
        {
            if ((collision.gameObject.GetComponent<PlatformDrop>() != null && enemyRB.velocity.y >= collision.gameObject.GetComponent<Rigidbody2D>().velocity.y + 0.05f) ||
                enemyHitstopActive)
            {

            }
            else
            {
                blitzCollision = true;
                collisionDirection = collision.contacts[0].normal;
                if (Mathf.Abs(collisionDirection.x) > 0.7f)
                {
                    collisionDirection = new Vector2(Mathf.Sign(collisionDirection.x), 1).normalized;
                }
                else
                {
                    Vector2 dot = Vector2.Dot(Vector2.Perpendicular(collision.contacts[0].normal), enemyRB.velocity.normalized) * 
                        Vector2.Perpendicular(collision.contacts[0].normal);
                    collisionDirection = (collisionDirection + dot).normalized;
                }
                float radius = 0.7f;
                RaycastHit2D cast = Physics2D.CircleCast(enemyRB.position + vectorFacing * 0.5f, radius, Vector2.zero, 0, boxLM);
                //Debug.DrawRay(enemyRB.position + vectorFacing * 0.5f + Vector2.up * radius, Vector2.down * radius * 2);
                //Debug.DrawRay(enemyRB.position + vectorFacing * 0.5f + Vector2.right * radius, Vector2.left * radius * 2);
                if (cast.collider != null)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = damage;
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(enemyRB.velocity.x), 1).normalized;
                    if (EM.shockActive)
                    {
                        Box.activateShock = true;
                    }
                    StartCoroutine(EnemyHitstop());
                }
            }
        }
    }

    IEnumerator BlitzAttack()
    {
        blitzActive = true;
        idle = false;
        while (attacksLeft > 0 && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
        {
            delayActive = true;
            attacksLeft -= 1;
            enemyRB.angularVelocity = 0;
            radius = 100;
            StartCoroutine(Shake());
            float window = attackDelay;
            if (attacksLeft != numAttacks - 1)
            {
                window /= 2;
            }
            float timer = 0;
            bool stopAttack = false;
            while (timer < window && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
            {
                if (EM.hitstopImpactActive)
                {
                    stopAttack = true;
                    attacksLeft = numAttacks;
                    break;
                }
                enemyRB.velocity -= vectorFacing * 15 * (0.5f / attackDelay) * timer * Time.deltaTime;
                timer += Time.deltaTime;
                yield return null;
            }
            delayActive = false;

            if (stopAttack == false)
            {
                attackActive = true;
                EM.physicalHitboxActive = true;
                StartCoroutine(Fire(true));
                StartCoroutine(SpawnSmoke());
                angularVelocity = attackAngularVelocity;
                enemyRB.velocity += vectorFacing * launchSpeed;
                float velMagnitude = enemyRB.velocity.magnitude;
                enemyRB.drag = 0;
                enemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;
                bool knockback = false;
                Vector2 initialPosition = enemyRB.position;
                float distance = 0;
                timer = 0;
                while (attackActive && distance < maxDistance && timer < 2 && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
                {
                    if (enemyHitstopActive == false)
                    {
                        if (blitzCollision == true || attackWasPulsed)
                        {
                            attackActive = false;
                            knockback = true;
                        }
                        blitzCollision = false;
                        velMagnitude = Mathf.Min(velMagnitude + (attackAccel * Time.deltaTime), launchSpeed * 1.5f);
                        enemyRB.velocity = velMagnitude * vectorFacing;

                        distance = (enemyRB.position - initialPosition).magnitude;
                        if (debugEnabled)
                        {
                            Debug.DrawLine(initialPosition, enemyRB.position, Color.green);
                        }

                        timer += Time.deltaTime;
                    }
                    if (EM.hitstopImpactActive)
                    {
                        attacksLeft = 0;
                    }

                    if ((positionList[2] - positionList[0]).magnitude < enemyRB.velocity.magnitude * Time.fixedDeltaTime && timer > 0.1f)
                    {
                        break;
                    }
                    yield return null;
                }
                enemyRB.constraints = RigidbodyConstraints2D.None;
                EM.physicalHitboxActive = false;
                attackActive = false;
                if (knockback)
                {
                    if (attackWasPulsed)
                    {
                        attacksLeft = 0;
                        StartCoroutine(Knockback(true));
                    }
                    else
                    {
                        StartCoroutine(Knockback(false));
                    }
                }
                else
                {
                    attacksLeft = 0;
                    enemyRB.drag = initialDrag;
                }
            }

            yield return new WaitForFixedUpdate();
            EM.normalPulse = true;
            angularVelocity = normalAngularVelocity;

            if (attacksLeft > 0 && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
            {
                window = attackCooldownTime / 8;
                timer = 0;
                while (timer < window && EM.enemyWasKilled == false)
                {
                    if (EM.hitstopImpactActive == false)
                    {
                        timer += Time.deltaTime;
                    }
                    yield return null;
                }
            }
            if (canSeeBox == false)
            {
                attacksLeft = 0;
            }
        }

        attackCooldown = true;
        radius = initialRadius;
        float window2 = attackCooldownTime;
        float timer2 = 0;
        while (timer2 < window2 && EM.enemyWasKilled == false)
        {
            timer2 += Time.deltaTime;
            yield return null;
        }
        angularVelocity = normalAngularVelocity;
        attackCooldown = false;
        idle = true;
        blitzActive = false;
        attacksLeft = numAttacks;
    }
    IEnumerator Shake()
    {
        float timer = 0;
        while (timer < attackDelay / 2 && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        EM.normalPulse = false;
        float shiftMult = 0.02f;
        while (delayActive && EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
        {
            Vector2 shift = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            enemyRB.position += shift * shiftMult;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            enemyRB.position -= shift * shiftMult;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            shiftMult += 0.02f * 1.5f / attackDelay;
        }
    }
    IEnumerator Fire(bool attack)
    {
        float initialPosY = fireTransform.localPosition.y;
        float activeScale = (attack) ? activeFireScale : activeFireScale * 0.6f;
        while (((attackActive && attack) || (idleMoveActive && attack == false)) && EM.enemyWasKilled == false)
        {
            float window = 0.05f;
            float timer = 0;
            fireTransform.localScale = new Vector2(fireTransform.localScale.x, activeScale);
            fireTransform.localPosition = new Vector2(fireTransform.localPosition.x, initialPosY - 0.2f);
            while (timer < window && ((attackActive && attack) || (idleMoveActive && attack == false)) && EM.enemyWasKilled == false)
            {
                if (enemyHitstopActive == false)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }

            timer = 0;
            fireTransform.localScale = new Vector2(fireTransform.localScale.x, (initialFireScale + activeScale) / 2);
            fireTransform.localPosition = new Vector2(fireTransform.localPosition.x, initialPosY - 0.1f);
            while (timer < window && ((attackActive && attack) || (idleMoveActive && attack == false)) && EM.enemyWasKilled == false)
            {
                if (enemyHitstopActive == false)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }
        fireTransform.localScale = new Vector2(fireTransform.localScale.x, initialFireScale);
        fireTransform.localPosition = new Vector2(fireTransform.localPosition.x, initialPosY);
    }
    IEnumerator SpawnSmoke()
    {
        while (attackActive && EM.enemyWasKilled == false)
        {
            Instantiate(smoke, enemyRB.position - vectorFacing * 0.5f + Vector2.Perpendicular(vectorFacing) * Random.Range(-0.2f, 0.2f), Quaternion.identity);
            float window = 0.05f;
            float timer = 0;
            while (timer < window && EM.enemyWasKilled == false)
            {
                if (enemyHitstopActive == false)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer = 0;
                }
                yield return null;
            }
        }
    }
    IEnumerator Knockback(bool pulsed)
    {
        knockbackActive = true;
        yield return null;
        if (FindObjectOfType<CameraFollowBox>() != null)
        {
            FindObjectOfType<CameraFollowBox>().StartCameraShake(6, (boxRB.position - enemyRB.position).magnitude);
        }
        while (EM.hitstopImpactActive)
        {
            yield return null;
        }
        //if (pulsed)
        //{
        //    yield return null;
        //}
        enemyRB.gravityScale = 2f;
        enemyRB.velocity = collisionDirection * 8;
        if (pulsed)
        {
            enemyRB.velocity *= 2;
        }
        float sign = Mathf.Sign(enemyRB.velocity.x);
        enemyRB.angularVelocity = sign * -800;
        if (pulsed)
        {
            enemyRB.angularVelocity *= 2;
        }
        float maxFallSpeed = -10;
        float window = attackCooldownTime / 2;
        if (attacksLeft > 0)
        {
            window = attackCooldownTime / 10;
        }
        float timer = 0;
        while (timer < window && EM.enemyWasKilled == false)
        {
            if (enemyRB.velocity.y < maxFallSpeed)
            {
                enemyRB.velocity = new Vector2(enemyRB.velocity.x, maxFallSpeed);
            }
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        enemyRB.gravityScale = 0;
        enemyRB.drag = initialDrag;
        attackWasPulsed = false;
        knockbackActive = false;
    }
    IEnumerator EnemyHitstop()
    {
        enemyHitstopActive = true;
        Vector2 enemyHitstopVelocity = enemyRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        enemyRB.velocity = new Vector2(0, 0);
        enemyRB.angularVelocity /= enemyHitstopRotationSlowDown;
        enemyRB.isKinematic = true;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        enemyRB.isKinematic = false;
        enemyRB.angularVelocity *= enemyHitstopRotationSlowDown;
        enemyRB.velocity = enemyHitstopVelocity;
        enemyHitstopActive = false;
        ignoreBoxLaunchDamage = true;
        yield return new WaitForFixedUpdate();
        while (Box.damageActive)
        {
            yield return null;
        }
        ignoreBoxLaunchDamage = false;
    }
    IEnumerator IdleMovement()
    {
        idleCR = true;
        angularVelocity = normalAngularVelocity / 3;
        while (idle)
        {
            float waitTime = idleWaitTime * Random.Range(0.7f, 1.3f);
            yield return new WaitForSeconds(waitTime);

            if (idle)
            {
                float minDist = 3;
                float angle = Random.Range(0f, 360f);
                Vector2 vector = Tools.AngleToVector(angle);
                float distance = Random.Range(minDist, idleRadius);
                int groundLM = LayerMask.GetMask("Obstacles", "Platforms");
                int avoidLM = LayerMask.GetMask("Obstacles", "Platforms", "Enemies");

                RaycastHit2D ray = Physics2D.Raycast(enemyRB.position, vector, distance, groundLM);
                Vector2 point = (ray.collider != null) ? ray.point - (vector * 2) : enemyRB.position + vector * distance;

                RaycastHit2D largeCircle = Physics2D.CircleCast(point, 10, Vector2.zero, 0, groundLM);
                RaycastHit2D smallCircle = Physics2D.CircleCast(point, idleOffset * 0.6f, Vector2.zero, 0, avoidLM);

                int i = 0;
                bool pointFound = true;
                while ((ray.collider != null && ray.distance - 2 < minDist) || largeCircle.collider == null || smallCircle.collider != null)
                {
                    minDist -= 0.1f;
                    angle = Random.Range(0f, 360f);
                    vector = Tools.AngleToVector(angle);
                    distance = Random.Range(minDist, idleRadius);

                    ray = Physics2D.Raycast(enemyRB.position, vector, distance, groundLM);
                    point = (ray.collider != null) ? ray.point - (vector * 2) : enemyRB.position + vector * distance;

                    largeCircle = Physics2D.CircleCast(point, 10, Vector2.zero, 0, groundLM);
                    smallCircle = Physics2D.CircleCast(point, idleOffset * 0.6f, Vector2.zero, 0, avoidLM);
                    i++;
                    if (i > 15)
                    {
                        pointFound = false;
                        break;
                    }
                }
                if (pointFound)
                {
                    float window = 0.5f;
                    float timer = 0;
                    while (timer < window && idle)
                    {
                        enemyRB.rotation = Mathf.MoveTowardsAngle(enemyRB.rotation, angle, angularVelocity * Time.deltaTime);
                        timer += Time.deltaTime;
                        yield return null;
                    }

                    idleMoveActive = true;
                    StartCoroutine(Fire(false));
                    float speed = launchSpeed * 0.25f;
                    window = (point - enemyRB.position).magnitude * 0.85f / speed;
                    timer = 0;
                    while (timer < window && idle)
                    {
                        if (enemyRB.velocity.magnitude < speed)
                        {
                            enemyRB.AddForce(vector * 50, ForceMode2D.Force);
                        }
                        float bodyAngle = Tools.VectorToAngle(enemyRB.velocity.normalized);
                        enemyRB.rotation = Mathf.MoveTowardsAngle(enemyRB.rotation, bodyAngle, angularVelocity * Time.deltaTime);
                        if (debugEnabled)
                        {
                            Debug.DrawRay(point, Vector2.up); Debug.DrawRay(point, Vector2.right);
                        }
                        timer += Time.fixedDeltaTime;
                        yield return new WaitForFixedUpdate();
                    }
                    idleMoveActive = false;
                }
                else
                {
                    yield return new WaitForSeconds(2f);
                }
            }
        }
        idleCR = false;
    }
}
