using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBehavior_Flying : MonoBehaviour
{
    bool scriptEnabled = true;

    public int enemyDifficulty = 1; //this will modify things in the script, but also public values will be manually changed in prefab

    int boxLM;
    int obstacleAndBoxLM;
    int obstacleLM;
    int enemyLM;
    int groundLM;

    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    EnemyManager EM;

    RaycastHit2D[] enemyRC_RayToBox;
    RaycastHit2D enemyRC_MoveToBox;
    RaycastHit2D enemyRC_ShootBox;
    RaycastHit2D enemyRC_ShootBoxTruePosition;
    RaycastHit2D enemyRC_MoveFromBox;

    RaycastHit2D enemyRC_ObstacleLeft;
    RaycastHit2D enemyRC_ObstacleRight;
    RaycastHit2D enemyRC_ObstacleLeftTP;
    RaycastHit2D enemyRC_ObstacleRightTP;

    RaycastHit2D enemyRC_Grounded;
    bool offGroundCR = false;

    RaycastHit2D enemyRC_ToBox_Pulse;

    public float moveToBoxRadius = 18;
    public float shootBoxRadius = 10;
    public float moveFromBoxRadius = 8;

    public bool isStationary = false;
    public bool moveWhileShooting = false;
    public bool heatSeekingBullets = false;
    public bool explodingBullets = false;

    //float bulletExplosionRadius = 2;

    public bool kamikaze = false;
    [HideInInspector] public bool kamikazeCR_Active = false;
    bool isDiving = false;
    float diveDelay = 1f;
    float maxDiveTime = 1.2f;
    float diveVelocity = 3;
    float diveAcceleration = 30;
    [HideInInspector] public bool kamikazeExplode = false;
    float kamikazeExplosionRadius = 5;
    float kamikazeDamage = 30;
    public GameObject explosion;
    GameObject newExplosion;
    Color kamikazeColor;
    float litColorChangespeed = 1.5f;
    float litColorChangeAcceleration = 7f;
    bool increaseColorG = true;

    bool pulseCanSeeBox = false;
    bool activateDiveReflect = false;
    [HideInInspector] public bool diveWasReflected = false;

    int lookingRight = 1;

    public float truePositionVelocity = 4; //horizontal velocity of trueposition
    float flySpeed; //maximum speed used to follow trueposition
    float flyForce; //force used to follow trueposition
    Vector2 truePosition;
    float randTruePositionY;

    Vector2 vectorToBox;
    float angleToBox;
    int horizDirectionToBox;
    bool canSeeBox = false;

    public GameObject bullet;
    GameObject newBullet;
    public float bulletSpeed = 10;
    public float bulletSpreadMax = 2;
    public int bulletsPerAttack = 8;
    public float delayBeforeShooting = 0;
    float bulletDespawnTime;
    public float shootTimeInterval = 0.1f;
    public float bulletRecoil = 1;
    public float bulletDamage = 20;
    float barrelAngle;
    float realBarrelAngle;
    Vector2 realBarrelVector;
    public float regularAngularVelocity = 400;
    public float shootAngularVelocity = 100;
    float angularVelocity;
    public float restTime = 2f;
    public int numberOfVolleys = 1;
    public float volleyDelay = 0.4f;
    bool isAttacking = false;
    bool isCurrentlyShooting = false;
    bool isWaitingToShoot = false;
    bool isOnCoolDown = false;
    bool attackFlapCR_Active = false;

    bool touchingThisEnemy = false;

    GameObject barrel;
    Color initialBarrelColor;

    public bool laserScopeEnabled = false;
    LineRenderer laserScope;

    bool aggroActive = false;

    public bool debugLines = false;

    void Start()
    {
        barrel = gameObject.transform.GetChild(0).gameObject;
        initialBarrelColor = barrel.GetComponent<SpriteRenderer>().color;
        enemyRB = gameObject.GetComponent<Rigidbody2D>();
        EM = gameObject.GetComponent<EnemyManager>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        truePosition = new Vector2(enemyRB.position.x, enemyRB.position.y);

        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");

        flySpeed = truePositionVelocity * 1.1f;

        if (isStationary == true)
        {
            moveFromBoxRadius = 0;
            moveToBoxRadius = shootBoxRadius;
        }

        diveVelocity = 3;

        bulletDespawnTime = shootBoxRadius * 3 / bulletSpeed;
        float horizDirection = Random.Range(0, 2);
        if (horizDirection == 0)
        {
            horizDirection = -1;
        }

        if (kamikaze == true)
        {
            kamikazeColor = gameObject.GetComponent<Renderer>().material.color;
        }

        if (enemyDifficulty < 1) { enemyDifficulty = 1; }
        if (enemyDifficulty > 3) { enemyDifficulty = 3; }
        if (enemyDifficulty == 3)
        {
            bulletDespawnTime *= 1.5f;
        }
        if (laserScopeEnabled)
        {
            laserScope = transform.GetChild(1).gameObject.GetComponent<LineRenderer>();
        }
    }

    void FixedUpdate()
    {
        //vertical and horizontal movement about the true position if not diving
        if (kamikazeCR_Active == false)
        {
            //vertical: flap when enemyRB.position goes below trueposition y value. Use different flap logic when attacking due to recoil.
            float flapVelocity = 4f + Random.value * 2;
            randTruePositionY = truePosition.y + -2f + Random.value * 2;
            if (enemyRB.position.y < randTruePositionY)
            {
                if (isAttacking == false)
                {
                    enemyRB.velocity = new Vector2(enemyRB.velocity.x, flapVelocity); 
                }
                else if (attackFlapCR_Active == false)
                {
                    StartCoroutine(AttackFlap());
                }
            }
            //horizontal: keep bird within a certain distance from trueposition. if outside, apply force towards trueposition.
            //if inside, but not attacking, apply force left and right to wander about true position.
            float wanderDistance;
            if (isStationary)
            {
                wanderDistance = 0.2f;
                flyForce = flySpeed * 1.5f;
            }
            else
            {
                if (enemyRC_MoveToBox.collider == null)
                {
                    wanderDistance = 2f;
                    flyForce = flySpeed * 1.5f;
                }
                else
                {
                    wanderDistance = 0.5f;
                    flyForce = flySpeed * 5; //3.5f
                }
            }
            if (Math.Abs(enemyRB.position.x - truePosition.x) >= wanderDistance)
            {
                int horizDirection = (int) Mathf.Sign(truePosition.x - enemyRB.position.x);
                if (Math.Abs(enemyRB.velocity.x) < flySpeed)
                {
                    enemyRB.AddForce(new Vector2(flyForce * horizDirection, 0));
                }
            }
            else if (isAttacking == false || moveWhileShooting == true)
            {
                int horizDirection = 1;
                if (enemyRB.velocity.x != 0)
                {
                    horizDirection = (int) Mathf.Sign(enemyRB.velocity.x);
                }
                enemyRB.AddForce(new Vector2(flyForce * (float)(Random.value) * horizDirection / 8, 0));
            }
        }

        //trueposition movement and attacking logic
        if (isStationary == false && canSeeBox == true)
        {
            //move trueposition towards box if inside visible range, not within shooting range,
            //can see the box, is set to move while shooting OR isn't attacking OR is on cooldown from attacking, and
            //there are no obstacles in the way of the direction it wants to move.
            if (enemyRC_ShootBox.collider == null && (moveWhileShooting || isAttacking == false || (isAttacking && isOnCoolDown)) &&
                ((horizDirectionToBox == 1 && enemyRC_ObstacleRightTP.collider == null) ||
                (horizDirectionToBox == -1 && enemyRC_ObstacleLeftTP.collider == null)))
            {
                truePosition = new Vector2(truePosition.x + truePositionVelocity * horizDirectionToBox * Time.deltaTime, truePosition.y);
            }
            //if inside shooting range but outside move away range, and is not attacking, and initial delay is over, start attacking.
            else if (enemyRC_ShootBox.collider != null && enemyRC_MoveFromBox.collider == null && 
                EM.initialDelay == false && kamikaze == false)
            {
                truePosition = new Vector2(enemyRB.position.x, truePosition.y);
                if (isAttacking == false)
                {
                    StartCoroutine(Attack());
                }
            }
            //move trueposition away from box if inside move away range,
            //can see the box, is set to move while shooting OR isn't attacking OR is on cooldown from attacking, and
            //there are no obstacles in the way of the direction it wants to move.
            else if (enemyRC_MoveFromBox.collider != null && (moveWhileShooting || isAttacking == false || (isAttacking && isOnCoolDown)) &&
                ((horizDirectionToBox == 1 && enemyRC_ObstacleLeftTP.collider == null) ||
                (horizDirectionToBox == -1 && enemyRC_ObstacleRightTP.collider == null)))
            {
                truePosition = new Vector2(truePosition.x + truePositionVelocity * -horizDirectionToBox * Time.deltaTime, truePosition.y);
            }
            //if inside move away range but an obstacle is preventing movement, and is not attacking, and initial delay is over, start attacking.
            else if (enemyRC_MoveFromBox.collider != null && isAttacking == false && EM.initialDelay == false && kamikaze == false && 
                ((horizDirectionToBox == 1 && enemyRC_ObstacleLeft.collider != null) ||
                (horizDirectionToBox == -1 && enemyRC_ObstacleRight.collider != null)))
            {
                StartCoroutine(Attack());
            }
        }

        else if (isStationary == true && kamikaze == false)
        {
            //if the enemy is stationary, shoot whenever the enemy is within shooting range
            if (enemyRC_ShootBox.collider != null && canSeeBox && isAttacking == false && EM.initialDelay == false)
            {
                StartCoroutine(Attack());
            }
        }

        if (kamikaze == true)
        {
            if (enemyRC_ShootBox.collider != null && canSeeBox == true && kamikazeCR_Active == false)
            {
                StartCoroutine(KamikazeDive());
            }
            if (activateDiveReflect == true)
            {
                if (enemyRB.velocity.magnitude < 10)
                {
                    while (enemyRB.velocity.magnitude < 10)
                    {
                        enemyRB.AddForce(-vectorToBox, ForceMode2D.Impulse);
                    }
                }
                else
                {
                    enemyRB.velocity = -vectorToBox * enemyRB.velocity.magnitude;
                }

                activateDiveReflect = false;
                diveWasReflected = true;
               
            }
            if (isDiving == true && diveWasReflected == false)
            {
                diveVelocity += diveAcceleration * Time.deltaTime;
                enemyRB.velocity = realBarrelVector * diveVelocity;
            }
            if (diveWasReflected == true)
            {
                enemyRB.gravityScale = 2;
                enemyRB.drag = 0;
                enemyRB.freezeRotation = false;
                enemyRB.angularVelocity += -1500*Time.deltaTime;
                if (isDiving == true)
                {
                    diveVelocity = enemyRB.velocity.magnitude + diveAcceleration * Time.deltaTime;
                    enemyRB.velocity = (enemyRB.velocity.normalized * diveVelocity);
                }
            }
            if (diveWasReflected || isDiving)
            {
                EM.normalExplosionsWillDamage = true;
                EM.normalDeath = false;
                if (EM.hitstopImpactActive == true)
                {
                    kamikazeExplode = true;
                }
                RaycastHit2D detectBox = Physics2D.CircleCast(enemyRB.position, transform.lossyScale.x / 2,
                    new Vector2(0, 0), 0f, boxLM);
                if (detectBox.collider != null)
                {
                    kamikazeExplode = true;
                }


                if (increaseColorG == true)
                {
                    kamikazeColor.g += litColorChangespeed * Time.deltaTime;
                }
                if (increaseColorG == false)
                {
                    kamikazeColor.g -= litColorChangespeed * Time.deltaTime;
                }
                if (kamikazeColor.g >= 2)
                {
                    kamikazeColor.g = 2;
                    increaseColorG = false;
                }
                if (kamikazeColor.g <= 1)
                {
                    kamikazeColor.g = 1;
                    increaseColorG = true;
                }
                gameObject.GetComponent<Renderer>().material.color = kamikazeColor;
                litColorChangespeed += litColorChangeAcceleration * Time.deltaTime;

            }
            if (kamikazeExplode == true)
            {
                newExplosion = Instantiate(explosion, enemyRB.position, Quaternion.identity);
                if (diveWasReflected == true)
                {
                    newExplosion.GetComponent<Explosion>().damageEnemies = true;
                }
                newExplosion.GetComponent<Explosion>().explosionRadius = kamikazeExplosionRadius;
                newExplosion.GetComponent<Explosion>().explosionDamage = kamikazeDamage;
                newExplosion.GetComponent<Transform>().localScale = new Vector2(kamikazeExplosionRadius * 2, kamikazeExplosionRadius * 2);
                Destroy(gameObject);
            }
        }

        //which direction the enemy is facing
        if (enemyRB.velocity.x > 0.1f)
        {
            lookingRight = 1;
        }
        if (enemyRB.velocity.x < -0.1f)
        {
            lookingRight = -1;
        }

        //idle barrel angle
        if (canSeeBox == false && isAttacking == false && kamikazeCR_Active == false)
        {
            barrelAngle = (180 + lookingRight * 60);
        }

        //determine the speed of barrel swivel
        if (isCurrentlyShooting == true && isWaitingToShoot == false)
        {
            angularVelocity = shootAngularVelocity;
        }
        else if (isDiving == true)
        {
            angularVelocity = shootAngularVelocity / 2.4f;
        }
        else
        {
            angularVelocity = regularAngularVelocity;
        }

        if (EM.hitstopImpactActive == false)
        {
            realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngle, angularVelocity * Time.deltaTime);
            realBarrelVector = new Vector2(Mathf.Cos(realBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(realBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
            transform.eulerAngles = Vector3.forward * (realBarrelAngle);
        }

        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;
            truePositionVelocity *= EM.aggroIncreaseMult;
            flySpeed *= EM.aggroIncreaseMult;

            shootBoxRadius *= EM.aggroIncreaseMult;
            moveToBoxRadius *= EM.aggroIncreaseMult;
            moveFromBoxRadius *= EM.aggroIncreaseMult;

            bulletSpeed *= EM.aggroIncreaseMult;
            bulletDamage *= EM.aggroIncreaseMult;
            bulletsPerAttack = Mathf.FloorToInt(bulletsPerAttack * EM.aggroIncreaseMult);
            bulletDespawnTime *= EM.aggroIncreaseMult;

            shootTimeInterval *= EM.aggroDecreaseMult;
            delayBeforeShooting *= EM.aggroDecreaseMult;
            restTime *= EM.aggroDecreaseMult;

            diveDelay *= EM.aggroDecreaseMult;
            diveAcceleration *= EM.aggroIncreaseMult;
            kamikazeExplosionRadius *= EM.aggroIncreaseMult;
            kamikazeDamage *= EM.aggroIncreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;
            truePositionVelocity /= EM.aggroIncreaseMult;
            flySpeed /= EM.aggroIncreaseMult;

            shootBoxRadius /= EM.aggroIncreaseMult;
            moveToBoxRadius /= EM.aggroIncreaseMult;
            moveFromBoxRadius /= EM.aggroIncreaseMult;

            bulletSpeed /= EM.aggroIncreaseMult;
            bulletDamage /= EM.aggroIncreaseMult;
            bulletsPerAttack = Mathf.CeilToInt(bulletsPerAttack / EM.aggroIncreaseMult);
            bulletDespawnTime /= EM.aggroIncreaseMult;

            shootTimeInterval /= EM.aggroDecreaseMult;
            delayBeforeShooting /= EM.aggroDecreaseMult;
            restTime /= EM.aggroDecreaseMult;

            diveDelay /= EM.aggroDecreaseMult;
            diveAcceleration /= EM.aggroIncreaseMult;
            kamikazeExplosionRadius /= EM.aggroIncreaseMult;
            kamikazeDamage /= EM.aggroIncreaseMult;
        }
    }

    private void Update()
    {
        //disable script when enemy is killed (not currently working properly)
        scriptEnabled = EM.scriptsEnabled;
        if (scriptEnabled == false)
        {
            StopAllCoroutines();
            if (laserScopeEnabled)
            {
                laserScope.enabled = false;
            }
            gameObject.GetComponent<EnemyBehavior_Flying>().enabled = false;
        }

        //turretVectorToBox
        vectorToBox = (boxRB.position - enemyRB.position).normalized;
        //turretAngleToBox
        angleToBox = -Mathf.Atan2(vectorToBox.x, vectorToBox.y) * Mathf.Rad2Deg;
        //horizDirectionToBox
        horizDirectionToBox = (int)new Vector2(vectorToBox.x, 0).normalized.x;

        enemyRC_MoveToBox = Physics2D.CircleCast(enemyRB.position, moveToBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_ShootBox = Physics2D.CircleCast(enemyRB.position, shootBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_ShootBoxTruePosition = Physics2D.CircleCast(truePosition, shootBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_MoveFromBox = Physics2D.CircleCast(enemyRB.position, moveFromBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_RayToBox = Physics2D.RaycastAll(enemyRB.position, vectorToBox, moveToBoxRadius, obstacleAndBoxLM);


        enemyRC_ObstacleLeft = Physics2D.BoxCast(new Vector2(enemyRB.position.x - 1f, truePosition.y + 0.5f),
            new Vector2(1, 1), 0, Vector2.down, 0f, obstacleLM);
        enemyRC_ObstacleRight = Physics2D.BoxCast(new Vector2(enemyRB.position.x + 1, truePosition.y + 0.5f),
            new Vector2(1, 1), 0, Vector2.down, 0f, obstacleLM);

        enemyRC_ObstacleLeftTP = Physics2D.BoxCast(new Vector2(truePosition.x - 1f, truePosition.y + 0.5f),
            new Vector2(1, 1), 0, Vector2.down, 0f, obstacleLM);
        enemyRC_ObstacleRightTP = Physics2D.BoxCast(new Vector2(truePosition.x + 1f, truePosition.y + 0.5f),
            new Vector2(1, 1), 0, Vector2.down, 0f, obstacleLM);

        enemyRC_Grounded = Physics2D.BoxCast(new Vector2(enemyRB.position.x, enemyRB.position.y - transform.lossyScale.y / 2),
            new Vector2(transform.lossyScale.x / 3, 0.1f), 0, Vector2.down, 0f, groundLM);

        if (enemyRC_Grounded.collider != null && offGroundCR == false)
        {
            StartCoroutine(GetOffGround());
        }

        //canSeeItem, and keeping the barrel angle still once canSeeItem = false
        float distToBox = 1000;
        float distToObstacle = 1000;
        foreach (RaycastHit2D col in enemyRC_RayToBox)
        {
            if (col.collider != null && 1 << col.collider.gameObject.layer == boxLM && enemyRC_MoveToBox.collider != null)
            {
                distToBox = col.distance;
            }
            if (col.collider != null && 1 << col.collider.gameObject.layer == obstacleLM && col.collider.gameObject.tag != "Fence")
            {
                distToObstacle = Mathf.Min(col.distance, distToObstacle);
            }
        }
        if (enemyRC_MoveToBox.collider != null && distToBox < distToObstacle)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            barrelAngle = angleToBox;
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }


        if (Box.pulseActive == true)
        {
            enemyRC_ToBox_Pulse = Physics2D.Raycast(enemyRB.position, vectorToBox, Box.pulseRadius, obstacleAndBoxLM);

            //inBoxLOS within pulse radius
            if (enemyRC_ToBox_Pulse.collider != null && 1 << enemyRC_ToBox_Pulse.collider.gameObject.layer == boxLM)
            {
                pulseCanSeeBox = true;
            }
            else
            {
                pulseCanSeeBox = false;
            }

            //activate dive reflect when pulsed during kamikaze
            if (pulseCanSeeBox == true && EM.enemyWasDamaged == false && kamikazeCR_Active == true)
            {
                activateDiveReflect = true;
            }
        }


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
        if (touchingThisEnemy == true && Box.boxHitboxActive)
        {
            EM.enemyWasDamaged = true;
            if (EM.enemyIsInvulnerable == false)
            {
                Box.activateHitstop = true;
            }
        }

        //debug lines
        if (debugLines)
        {
            Debug.DrawLine(truePosition + Vector2.left / 2, truePosition + Vector2.right / 2);
            if (canSeeBox == true)
            {
                if (isStationary == false)
                {
                    Debug.DrawRay(enemyRB.position, moveToBoxRadius * vectorToBox, Color.white);
                    Debug.DrawRay(enemyRB.position, shootBoxRadius * vectorToBox, Color.red);
                    if (kamikaze == false)
                    {
                        Debug.DrawRay(enemyRB.position, moveFromBoxRadius * vectorToBox, Color.green);
                    }
                }
                else
                {
                    Debug.DrawRay(enemyRB.position, shootBoxRadius * vectorToBox, Color.blue);
                }
            }
            else
            {
                if (isStationary == false)
                {
                    Debug.DrawRay(enemyRB.position, moveToBoxRadius * vectorToBox, Color.gray);
                }
                else
                {
                    Debug.DrawRay(enemyRB.position, shootBoxRadius * vectorToBox, Color.gray);
                }
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDiving == true || diveWasReflected == true)
        {
            kamikazeExplode = true;
        }
        if (collision.collider.gameObject.tag == "Hazard" && isDiving == false)
        {
            EM.enemyWasDamaged = true;
        }
    }

    IEnumerator Attack()
    {
        float holdGravityScale = enemyRB.gravityScale;
        if (moveWhileShooting == false)
        {
            enemyRB.gravityScale = 0.2f;
            enemyRB.velocity = new Vector2(enemyRB.velocity.x / 4, enemyRB.velocity.y / 4);
        }
        isAttacking = true;
        int bulletsShot = 0;
        float bulletSpread;
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayBeforeShootingTimer = 0;
        float shootTimeIntervalTimer = 0;
        StartCoroutine(BarrelColor());
        if (laserScopeEnabled)
        {
            laserScope.gameObject.SetActive(true);
        }
        while (delayBeforeShootingTimer <= delayBeforeShooting && EM.enemyWasKilled == false)
        {
            if (laserScopeEnabled)
            {
                laserScope.SetPosition(0, enemyRB.position + realBarrelVector * transform.localScale.x * 2 / 3);
                RaycastHit2D[] laserRC = Physics2D.RaycastAll(enemyRB.position + realBarrelVector * transform.localScale.x * 2 / 3, realBarrelVector, 100, obstacleAndBoxLM);
                float targetDist = 100;

                foreach (RaycastHit2D col in laserRC)
                {
                    if (col.collider.tag != "Fence")
                    {
                        targetDist = Mathf.Min(targetDist, col.distance);
                    }
                }
                laserScope.SetPosition(1, enemyRB.position + realBarrelVector * (transform.localScale.x * 2/3 + targetDist));
                //if (laserRC.collider != null)
                //{
                //    laserScope.SetPosition(1, laserRC.point);
                //}
                //else
                //{
                //    laserScope.SetPosition(1, enemyRB.position + realBarrelVector * 100);
                //}
            }
            if (EM.hitstopImpactActive == false)
            {
                delayBeforeShootingTimer += Time.deltaTime;
            }
            yield return null;
        }
        if (laserScopeEnabled)
        {
            laserScope.gameObject.SetActive(false);
        }
        isWaitingToShoot = false;
        int volleysFired = 0;
        while (volleysFired < numberOfVolleys && EM.enemyWasKilled == false)
        {
            while (bulletsShot < bulletsPerAttack && scriptEnabled == true)
            {
                bulletSpread = (-bulletSpreadMax / 2) + Random.value * bulletSpreadMax;
                Quaternion bulletRotation = Quaternion.Euler(0, 0, realBarrelAngle + bulletSpread);
                newBullet = Instantiate(bullet, enemyRB.position + realBarrelVector * 0.8f, bulletRotation);
                newBullet.GetComponent<BulletScript>().bulletDespawnWindow = bulletDespawnTime;
                newBullet.GetComponent<BulletScript>().bulletDamage = bulletDamage;
                if (heatSeekingBullets)
                {
                    newBullet.GetComponent<BulletScript>().heatSeeking = true;
                }
                if (explodingBullets)
                {
                    newBullet.GetComponent<BulletScript>().explodingBullets = true;
                }
                if (aggroActive)
                {
                    newBullet.GetComponent<BulletScript>().aggro = true;
                }
                newBullet.GetComponent<Rigidbody2D>().velocity = (realBarrelVector +
                    Vector2.Perpendicular(realBarrelVector) * Mathf.Sin(bulletSpread * Mathf.PI / 180)).normalized * bulletSpeed;

                bulletsShot++;
                enemyRB.AddForce(bulletRecoil * -realBarrelVector, ForceMode2D.Impulse);

                while (shootTimeIntervalTimer <= shootTimeInterval && shootTimeInterval != 0)
                {
                    if (EM.hitstopImpactActive == false)
                    {
                        shootTimeIntervalTimer += Time.deltaTime;
                    }
                    yield return null;
                }
                shootTimeIntervalTimer = 0;
            }
            volleysFired++;
            if (volleysFired < numberOfVolleys)
            {
                bulletsShot = 0;
                float volleyDelayTime = 0;
                while (volleyDelayTime <= volleyDelay)
                {
                    if (EM.hitstopImpactActive == false)
                    {
                        volleyDelayTime += Time.deltaTime;
                    }
                    yield return null;
                }
            }
        }

        isCurrentlyShooting = false;
        isOnCoolDown = true;
        if (moveWhileShooting == false)
        {
            enemyRB.gravityScale = holdGravityScale;
        }
        yield return new WaitForSeconds(restTime);
        isOnCoolDown = false;
        isAttacking = false;
    }
    IEnumerator BarrelColor()
    {
        EM.attackActive = true;
        Color barrelColor = new Color(1, 0.5f, 0);
        if (heatSeekingBullets)
        {
            barrelColor = new Color(0.8f, 0.8f, 0);
        }
        while (isAttacking && isOnCoolDown == false)
        {
            if (EM.flashOn == false)
            {
                barrel.GetComponent<SpriteRenderer>().color = barrelColor;
            }
            yield return null;
        }
        EM.attackActive = false;
        barrel.GetComponent<SpriteRenderer>().color = initialBarrelColor;
    }
    IEnumerator AttackFlap()
    {
        attackFlapCR_Active = true;
        while (enemyRB.position.y < truePosition.y && isAttacking == true && scriptEnabled == true)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x,
                (enemyRB.velocity.y + 5 + 2* (truePosition.y - enemyRB.position.y + 1)) * (0.5f + enemyRB.gravityScale) / 2);
            yield return new WaitForSeconds(0.4f);
        }
        attackFlapCR_Active = false;
    }
    IEnumerator KamikazeDive()
    {
        EM.normalPulse = false;
        kamikazeCR_Active = true;
        enemyRB.gravityScale = 0;
        enemyRB.velocity = new Vector2(enemyRB.velocity.x, enemyRB.velocity.y / 4);
        float diveDelayTimer = 0;
        while (diveDelayTimer <= diveDelay)
        {
            if (EM.hitstopImpactActive == false)
            {
                diveDelayTimer += Time.deltaTime;
            }
            yield return null;
        }
        if (diveWasReflected == false)
        {
            isDiving = true;
        }
        yield return new WaitForSeconds(maxDiveTime);
        kamikazeExplode = true;
    }
    IEnumerator GetOffGround()
    {
        offGroundCR = true;
        float window = 0.5f;
        float timer = 0;
        bool getOffGround = false;
        while (enemyRC_Grounded.collider != null && getOffGround == false)
        {
            if (timer >= window)
            {
                getOffGround = true;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (getOffGround == true)
        {
            float moveWindow = 0.35f;
            float moveTimer = 0;
            while (moveTimer <= moveWindow)
            {
                if (enemyRC_Grounded.collider != null)
                {
                    moveTimer = 0;
                }
                truePosition = new Vector2(truePosition.x, truePosition.y + truePositionVelocity / 2 * Time.deltaTime);
                moveTimer += Time.deltaTime;
                yield return null;
            }
        }
        offGroundCR = false;
    }
}
