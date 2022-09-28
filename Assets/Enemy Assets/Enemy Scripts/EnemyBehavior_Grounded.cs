using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehavior_Grounded : MonoBehaviour
{
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    PolygonCollider2D enemyCollider;
    EnemyManager EM;
    RaycastHit2D enemyRC_MoveToBox;
    RaycastHit2D enemyRC_AttackBox;
    RaycastHit2D isGroundedRC;
    RaycastHit2D walkFloorCheckRC;
    RaycastHit2D walkObstacleCheckRC;
    RaycastHit2D enemyRC_RayToBox;

    [System.NonSerialized] public bool enemyIsGrounded = false;
    bool touchedGround = false;

    [HideInInspector] public bool touchingThisEnemy = false;

    public float moveToBoxRadius = 8;
    public float attackBoxRadius = 4;
    public float maxHorizSpeed = 4;
    float moveForce = 50;

    public bool willIdleWalk = true;
    bool idleCRActive = false;

    bool attackCRActive = false;
    bool onCoolDown = false;
    //bool attackAirborne = false;
    [System.NonSerialized] public bool enemyHitboxActive = false;
    public float attackDelay = 0.1f;
    float attackSpinDelay = 0.08f;
    public float attackCoolDown = 2f; //must be larger than attackSpinDelay
    public float attackJumpVelocity = 10;
    public float attackSpinSpeed = -1000;
    float platformSpeed = 0;
    [HideInInspector] public bool enemyWasReboundedDash = false;
    [HideInInspector] public bool enemyWasRebounded = false;

    [HideInInspector] public bool willDamageEnemies = false;

    bool enemyHitstopActive = false;
    bool extendHitstop = false;
    public float damage = 25;
    bool isInvulnerable = false;

    public bool lvl3 = false;
    public GameObject pulse;

    bool enemyIsPushedBack = false;
    bool enemyIsWalking = false;

    int directionToBoxX; //1 if box is to the right of the enemy, -1 if box is to the left
    bool canSeeBox = false;
    int lookingRight = 1; //same as lookingRight for Box. Will be set to = directionToBoxX if box is within range.

    Color initialColor;

    int boxLM;
    int groundLM;
    int obstacleAndBoxLM;
    int enemyLM;

    public bool debugLines = false;

    float[] YVelocities = new float[3];
    float avgYVelocity = 0;
    float groundpoundYVel = -8;
    public float groundpoundRadius = 7;
    public float groundpoundDamage = 30;
    bool groundpoundActive = false;

    bool aggroActive = false;

    float maxFallSpeed;

    private void Awake()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        EM = GetComponent<EnemyManager>();

        enemyCollider = GetComponent<PolygonCollider2D>();

        initialColor = GetComponent<SpriteRenderer>().color;

        isInvulnerable = false;
        onCoolDown = false;
        willDamageEnemies = false;
        attackCRActive = false;

        boxLM = LayerMask.GetMask("Box");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        enemyLM = LayerMask.GetMask("Enemies");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        maxFallSpeed = enemyRB.gravityScale * -5;
        if (lvl3)
        {
            maxFallSpeed *= 2;
        }
    }
    void FixedUpdate()
    {
        if (EM.scriptsEnabled == false)
        {
            this.enabled = false;
        }

        if (enemyRB.velocity.y < maxFallSpeed && EM.enemyWasKilled == false)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x, maxFallSpeed);
        }

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
            enemyIsPushedBack = false;
            touchingThisEnemy = false;
            EM.touchingThisEnemy = false;
        }
        //if the box collides with the enemy...
        if (touchingThisEnemy == true)
        {
            //...and if the spike perk is off...
            if (BoxPerks.spikesActive == false)
            {
                //...and if the box is currently attacking or is in hitstop...
                if (Box.boxHitboxActive && isInvulnerable == false)
                {
                    //...and if the enemy is not currently attacking, damage the enemy.
                    if (enemyHitboxActive == false || BoxPerks.starActive)
                    {
                        EM.enemyWasDamaged = true;
                        if (EM.enemyIsInvulnerable == false)
                        {
                            Box.activateHitstop = true;
                        }
                    }
                    //...and if the enemy is currently attacking, rebound box and enemy. Cause a stronger enemy rebound if it was a dash attack.
                    else
                    {
                        if (lvl3 == false)
                        {
                            if (Box.dashActive == true)
                            {
                                enemyWasReboundedDash = true;
                            }
                            enemyWasRebounded = true;
                        }
                        Box.activateRebound = true;
                        if (EM.shockActive)
                        {
                            Box.activateShock = true;
                            Box.activateDamage = true;
                            Box.damageTaken = Lightning.contactDamage;
                            Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - enemyRB.position.x), 1).normalized;
                            Box.activateRebound = false;
                            StartCoroutine(EnemyHitstop());
                        }
                    }
                }
                //...and if the box is NOT currently attacking, but the enemy is currently attacking, damage the box.
                else if (enemyHitboxActive == true && EM.enemyWasDamaged == false && Box.isInvulnerable == false
                    && enemyWasRebounded == false)
                {
                    if (EM.shockActive)
                    {
                        Box.activateShock = true;
                    }
                    Box.activateDamage = true;
                    Box.damageTaken = damage;
                    if (lvl3 && avgYVelocity <= groundpoundYVel && willDamageEnemies == false)
                    {
                        Box.damageTaken = damage * 2.5f;
                    }
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - enemyRB.position.x), 1).normalized;
                    StartCoroutine(EnemyHitstop());
                }
                //...and if neither the box or the enemy are attacking and the box is grounded, activate pushback.
                else if (Box.isGrounded == true && Box.isInvulnerable == false)
                {
                    Box.activatePushBack = true;
                    enemyIsPushedBack = true;
                }
            }
            //...and if the spike perk is active, damage the enemy no matter what.
            else
            {
                EM.enemyWasDamaged = true;
                if (EM.enemyIsInvulnerable == false)
                {
                    Box.activateHitstop = true;
                }
            }
        }

        //if the enemy was damaged...
        if (EM.enemyWasDamaged == true)
        {
            enemyHitboxActive = false;
        }

        if (EM.enemyIsFrozen)
        {
            EM.physicalHitboxActive = false;
            enemyHitboxActive = false;
            idleCRActive = false;
            groundpoundActive = false;
            return;
        }

        directionToBoxX = (int) Mathf.Sign(boxRB.position.x - enemyRB.position.x);

        enemyRC_MoveToBox = Physics2D.CircleCast(enemyRB.position, moveToBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_AttackBox = Physics2D.CircleCast(enemyRB.position, attackBoxRadius, new Vector2(0, 0), 0f, boxLM);
        walkFloorCheckRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x + transform.lossyScale.x * 1.5f * lookingRight,
            enemyRB.position.y - transform.lossyScale.y * 0.45f - 1.5f), new Vector2(transform.lossyScale.x / 4, 3f), 0, Vector2.down, 0f, groundLM);
        walkObstacleCheckRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x + transform.lossyScale.x * lookingRight,
            enemyRB.position.y), new Vector2(transform.lossyScale.x/4, transform.lossyScale.y / 3), 0, Vector2.down, 0f, groundLM);
        enemyRC_RayToBox = Physics2D.Raycast(enemyRB.position, (boxRB.position - enemyRB.position).normalized,
            moveToBoxRadius, obstacleAndBoxLM);

        //canseebox
        if (enemyRC_RayToBox.collider != null && 1 << enemyRC_RayToBox.collider.gameObject.layer == boxLM)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }

        //enemy isGrounded
        if (touchedGround)
        {
            enemyIsGrounded = true;
            enemyHitboxActive = false;
        }
        else
        {
            enemyIsGrounded = false;
        }

        //enemy walking
        if (enemyIsWalking == true)
        {
            float walkSpeed;
            if (idleCRActive == true)
            {
                walkSpeed = maxHorizSpeed / 2.5f;
            }
            else
            {
                walkSpeed = maxHorizSpeed;
            }
            if (Mathf.Abs(enemyRB.velocity.x - platformSpeed) <= walkSpeed 
                && walkFloorCheckRC.collider != null && walkObstacleCheckRC.collider == null && attackCRActive == false)
            {
                enemyRB.AddForce(new Vector2(moveForce * lookingRight, 0));
            }
        }

        //Start idle walking
        if (canSeeBox == false && idleCRActive == false && enemyIsGrounded == true)
        {
            StartCoroutine(IdleWalking());
        }

        //move if inside move radius but outside attack radius, attack if inside attack radius
        if (enemyRC_MoveToBox.collider != null && enemyIsGrounded == true && canSeeBox == true && EM.initialDelay == false)
        {
            lookingRight = directionToBoxX;
            idleCRActive = false;
            if (enemyRC_AttackBox.collider == null && attackCRActive == false && Mathf.Abs(enemyRB.velocity.x) <= maxHorizSpeed)
            {
                enemyIsWalking = true;
            }
            if (enemyRC_AttackBox.collider != null && attackCRActive == false)
            {
                enemyIsWalking = false;
                StartCoroutine(Attack());
            }
        }

        //if the enemy was rebounded...
        if (enemyWasRebounded == true)
        {
            enemyHitboxActive = false;
            StartCoroutine(EnemyRebound());
        }

        //if the enemy was pushed back...
        if (enemyIsPushedBack == true && Mathf.Abs(enemyRB.velocity.x) < 2)
        {
            int pushDirection = -(int)(Mathf.Abs(boxRB.position.x - enemyRB.position.x) / (boxRB.position.x - enemyRB.position.x));
            enemyRB.AddForce(new Vector2((float)20 * pushDirection, 0));
        }

        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;
            moveToBoxRadius *= EM.aggroIncreaseMult;
            attackBoxRadius *= EM.aggroIncreaseMult;
            maxHorizSpeed *= EM.aggroIncreaseMult;
            moveForce *= EM.aggroIncreaseMult;
            attackJumpVelocity *= EM.aggroIncreaseMult;
            enemyRB.gravityScale *= EM.aggroIncreaseMult;

            attackCoolDown *= EM.aggroDecreaseMult;
            damage *= EM.aggroIncreaseMult;

            groundpoundRadius *= EM.aggroIncreaseMult;
            groundpoundDamage *= EM.aggroIncreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;
            moveToBoxRadius /= EM.aggroIncreaseMult;
            attackBoxRadius /= EM.aggroIncreaseMult;
            maxHorizSpeed /= EM.aggroIncreaseMult;
            moveForce /= EM.aggroIncreaseMult;
            attackJumpVelocity /= EM.aggroIncreaseMult;
            enemyRB.gravityScale /= EM.aggroIncreaseMult;

            attackCoolDown /= EM.aggroDecreaseMult;
            damage /= EM.aggroIncreaseMult;

            groundpoundRadius /= EM.aggroIncreaseMult;
            groundpoundDamage /= EM.aggroIncreaseMult;
        }

        if (debugLines)
        {
            if (canSeeBox == true)
            {
                Debug.DrawRay(enemyRB.position, moveToBoxRadius * (boxRB.position - enemyRB.position).normalized, Color.white);
                Debug.DrawRay(enemyRB.position, attackBoxRadius * (boxRB.position - enemyRB.position).normalized, Color.red);
            }
            else
            {
                Debug.DrawRay(enemyRB.position, moveToBoxRadius * (boxRB.position - enemyRB.position).normalized, Color.gray);
            }

            Debug.DrawRay(enemyRB.position + (Vector2.down * enemyCollider.bounds.extents.y * 1.2f) + Vector2.left * 0.25f, Vector2.right * 0.5f);

            if (enemyIsGrounded == true && enemyRC_MoveToBox.collider != null)
            {
                Color color = initialColor * 4;
                gameObject.GetComponent<SpriteRenderer>().color = color;
            }
            else if (enemyHitboxActive == true)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().color = initialColor;
            }
        }

        YVelocities[2] = YVelocities[1];
        YVelocities[1] = YVelocities[0];
        YVelocities[0] = enemyRB.velocity.y;
        avgYVelocity = (YVelocities[0] + YVelocities[1] + YVelocities[2]) / 3;
    }

    private void Update()
    {
        if (groundpoundActive)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.9f, 0f, 0, 0.7f);
        }
        else if (enemyHitboxActive)
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0.9f, 0, 0, 0.3f);
        }
        else
        {
            transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
        }

        if (attackCRActive && onCoolDown == false && EM.enemyIsFrozen == false)
        {
            if (EM.enemyWasPulsed)
            {
                willDamageEnemies = true;
            }
        }
        else
        {
            willDamageEnemies = false;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (willDamageEnemies)
        {
            if (collision.transform.root.GetComponent<EnemyManager>() != null)
            {
                if (collision.transform.root.GetComponent<EnemyManager>().enemyIsInvulnerable == false)
                {
                    collision.transform.root.GetComponent<EnemyManager>().enemyWasDamaged = true;
                }
            }
            if (collision.transform.root.GetComponent<HitSwitch>() != null)
            {
                collision.transform.root.GetComponent<HitSwitch>().Hit();
            }
        }

        if (collision.transform.root.GetComponent<EnemyManager>() != null)
        {
            if (willDamageEnemies == false)
            {
                enemyHitboxActive = false;
            }
        }
        if (1 << collision.collider.gameObject.layer == LayerMask.GetMask("Platforms") || 1 << collision.collider.gameObject.layer == LayerMask.GetMask("Obstacles"))
        {
            platformSpeed = collision.collider.gameObject.GetComponent<Rigidbody2D>().velocity.x;

            touchedGround = false;
            RaycastHit2D groundCheck = Physics2D.Raycast(enemyRB.position + (Vector2.down * enemyCollider.bounds.extents.y * 1.2f) + Vector2.left * 0.25f,
                Vector2.right, 0.5f, groundLM);
            foreach (ContactPoint2D col in collision.contacts)
            {
                if (col.normal.y > 0.8f && (1 << collision.gameObject.layer == LayerMask.GetMask("Obstacles") ||
                    (avgYVelocity - collision.transform.GetComponent<Rigidbody2D>().velocity.y < 1f && groundCheck.collider != null)))
                {
                    touchedGround = true;
                    if (enemyHitboxActive)
                    {
                        enemyHitboxActive = false;
                        enemyRB.velocity = new Vector2(enemyRB.velocity.x / 3, enemyRB.velocity.y);
                        if (groundpoundActive)
                        {
                            enemyRB.velocity = Vector2.zero;
                            enemyRB.position = new Vector2(enemyRB.position.x, col.point.y + enemyCollider.bounds.extents.y * 0.65f);
                            enemyRB.rotation = 0;
                            enemyRB.angularVelocity = 0;
                            if (FindObjectOfType<CameraFollowBox>() != null)
                            {
                                FindObjectOfType<CameraFollowBox>().StartCameraShake(groundpoundDamage * 0.75f, (boxRB.position - enemyRB.position).magnitude);
                            }

                            GameObject newPulse = Instantiate(pulse, enemyRB.position, Quaternion.identity);
                            newPulse.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.05f);
                            newPulse.transform.localScale = Vector2.one * groundpoundRadius * 2;

                            RaycastHit2D groundpound = Physics2D.CircleCast(enemyRB.position, groundpoundRadius, Vector2.zero, 0, boxLM);
                            if (groundpound.collider != null && (Box.isGrounded || Box.canWallJump))
                            {
                                Box.activateDamage = true;
                                Box.damageTaken = groundpoundDamage;
                                if (Box.isGrounded)
                                {
                                    Box.boxDamageDirection = Vector2.up;
                                }
                                else
                                {
                                    Box.boxDamageDirection = new Vector2(Box.wallJumpDirection, 1).normalized;
                                }
                            }
                        }
                    }
                    break;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (willDamageEnemies)
        {
            if (collision.transform.root.GetComponent<EnemyManager>() != null)
            {
                if (collision.transform.root.GetComponent<EnemyManager>().enemyIsInvulnerable == false)
                {
                    collision.transform.root.GetComponent<EnemyManager>().enemyWasDamaged = true;
                    StartCoroutine(EnemyHitstop());
                }
            }
            if (collision.transform.root.GetComponent<HitSwitch>() != null)
            {
                collision.transform.root.GetComponent<HitSwitch>().Hit();
            }
        }

        if (collision.gameObject.tag == "Hazard")
        {
            EM.enemyWasDamaged = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        platformSpeed = 0;
        touchedGround = false;
    }
    IEnumerator Attack()
    {
        attackCRActive = true;
        float attackDelayTimer = 0;
        float attackSpinDelayTimer = 0;
        float attackCoolDownTimer = 0;
        float squatFactor = 0.7f;
        bool damaged = false;
        if (lvl3)
        {
            enemyRB.rotation = 0;
            transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y * squatFactor);
            enemyRB.position += Vector2.down * transform.localScale.y * (1 - squatFactor) / 2;
        }
        while (attackDelayTimer <= attackDelay && EM.enemyWasKilled == false && damaged == false && EM.enemyIsFrozen == false) // delay on ground before launching upwards
        {
            if (EM.hitstopImpactActive == false)
            {
                attackDelayTimer += Time.deltaTime;
            }
            if (lvl3)
            {
                enemyRB.rotation = 0;
                enemyRB.angularVelocity = 0;
            }
            if (EM.enemyWasDamaged)
            {
                damaged = true;
            }
            yield return null;
        }
        if (lvl3)
        {
            transform.localScale = new Vector2(transform.localScale.x, transform.localScale.y / squatFactor);
            enemyRB.position += Vector2.up * transform.localScale.y * (1 - squatFactor) / 2;
        }

        if (damaged == false)
        {
            float horizDistance = Mathf.Abs(enemyRB.position.x - boxRB.position.x) * directionToBoxX;
            float timeToLand = 2 * attackJumpVelocity / (9.81f * enemyRB.gravityScale);
            float adjustedAttackHorizSpeed = horizDistance / timeToLand;
            float adjustedJumpVelocity = attackJumpVelocity;
            if (lvl3)
            {
                adjustedJumpVelocity = Mathf.Max(Mathf.Sqrt(2 * 9.81f * enemyRB.gravityScale * (boxRB.position.y - enemyRB.position.y + 4)), attackJumpVelocity);
                adjustedAttackHorizSpeed *= 1.2f;
                adjustedAttackHorizSpeed *= 1 - Mathf.Abs(boxRB.position.y - enemyRB.position.y) / 30;
            }
            if (EM.enemyWasKilled == false && enemyIsGrounded && Mathf.Abs(enemyRB.velocity.x - platformSpeed) < maxHorizSpeed * 1.2f)
            {
                enemyRB.velocity = new Vector2(adjustedAttackHorizSpeed + platformSpeed, adjustedJumpVelocity);
            }
            while (attackSpinDelayTimer <= attackSpinDelay) // delay after launching before starting to spin activating hitbox
            {
                if (EM.hitstopImpactActive == false)
                {
                    attackSpinDelayTimer += Time.deltaTime;
                }
                yield return null;
            }
            bool activatedGroundPound = false;
            enemyHitboxActive = true;
            enemyRB.angularVelocity = attackSpinSpeed * directionToBoxX;
            while (enemyHitboxActive || enemyIsGrounded == false)
            {
                EM.physicalHitboxActive = true;
                if (enemyRB.velocity.y < -5f && lvl3 && enemyHitboxActive && willDamageEnemies == false && activatedGroundPound == false && EM.enemyWasKilled == false)
                {
                    activatedGroundPound = true;
                    groundpoundActive = true;
                    yield return null;
                    enemyRB.velocity = new Vector2(enemyRB.velocity.x, groundpoundYVel * 2f);
                }
                if (activatedGroundPound && groundpoundActive && avgYVelocity > groundpoundYVel && enemyRB.velocity.y > groundpoundYVel)
                {
                    groundpoundActive = false;
                }
                yield return null;
            }
        }

        groundpoundActive = false;
        EM.physicalHitboxActive = false;
        onCoolDown = true;
        while (attackCoolDownTimer <= attackCoolDown)
        {
            if (EM.hitstopImpactActive == false)
            {
                attackCoolDownTimer += Time.deltaTime;
            }
            yield return null;
        }
        attackCRActive = false;
        onCoolDown = false;
    }
    IEnumerator EnemyRebound()
    {
        while (enemyHitstopActive)
        {
            yield return null;
        }
        isInvulnerable = true;
        int reboundDirection;
        if (enemyWasReboundedDash == true)
        {
            reboundDirection = (int)new Vector2(boxRB.velocity.x, 0).normalized.x;
            enemyRB.velocity = new Vector2(7*reboundDirection, enemyRB.gravityScale * 3);
        }
        else
        {
            reboundDirection = -(int)new Vector2(boxRB.position.x - enemyRB.position.x, 0).normalized.x;
            enemyRB.velocity = new Vector2(4*reboundDirection, enemyRB.gravityScale * 2);
        }
        enemyWasReboundedDash = false;
        enemyWasRebounded = false;
        yield return new WaitForSeconds(0.2f);
        isInvulnerable = false;
    }
    IEnumerator IdleWalking()
    {
        enemyIsWalking = false;
        idleCRActive = true;
        float waitTime = 0.5f + Random.value * 3;
        float moveTime = 0.5f + Random.value * 5f;
        if (walkFloorCheckRC.collider == null || walkObstacleCheckRC.collider != null && willIdleWalk)
        {
            lookingRight *= -1;
        }
        else if (willIdleWalk)
        {
            lookingRight = Random.Range(0, 3) - 1;
        }
        else
        {
            lookingRight = 0;
        }
        yield return new WaitForSeconds(waitTime);
        if (idleCRActive)
        {
            enemyIsWalking = true;
            yield return new WaitForSeconds(moveTime);
        }
        if (idleCRActive)
        {
            StartCoroutine(IdleWalking());
        }
    }
    public IEnumerator EnemyHitstop()
    {
        if (enemyHitstopActive == false)
        {
            isInvulnerable = true;
            enemyHitstopActive = true;
            Vector2 enemyHitstopVelocity = enemyRB.velocity;
            float enemyHitstopRotationSlowDown = 10;
            enemyRB.velocity = new Vector2(0, 0);
            enemyRB.angularVelocity /= enemyHitstopRotationSlowDown;
            enemyRB.isKinematic = true;
            yield return null;
            float window = Box.boxHitstopDelayMult * damage;
            float timer = 0;
            while (Box.boxHitstopActive || timer < window)
            {
                timer += Time.deltaTime;
                if (extendHitstop)
                {
                    timer = 0;
                    extendHitstop = false;
                }
                yield return null;
            }
            enemyRB.isKinematic = false;
            enemyRB.angularVelocity *= enemyHitstopRotationSlowDown;
            enemyHitstopActive = false;
            enemyRB.velocity = enemyHitstopVelocity;
            yield return new WaitForSeconds(0.2f);
            isInvulnerable = false;
        }
        else
        {
            extendHitstop = true;
        }
    }
}
