using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_GroundedVehicle : MonoBehaviour
{
    bool scriptEnabled = true;

    public int enemyDifficulty = 1;

    Transform[] enemyComponents;

    Rigidbody2D enemyRB; //rigidbody of the body of the vehicle
    Rigidbody2D boxRB; //rigidbody of the player
    Transform enemyBodyTransform; //transform of the body of the vehicle
    Transform enemyTransform; //enemy transform will remain locked to it's spawn position
    GameObject barrel;
    EnemyManager EM;

    Color InitialBarrelColor;

    RaycastHit2D enemyRC_AttackBox;
    RaycastHit2D enemyRC_MoveFromBox;
    RaycastHit2D walkFloorCheckLeftRC;
    RaycastHit2D walkFloorCheckRightRC;
    RaycastHit2D walkObstacleCheckLeftRC;
    RaycastHit2D walkObstacleCheckRightRC;
    RaycastHit2D[] enemyRC_RayToBox;
    RaycastHit2D enemyRC_DashAttack;

    public float moveToBoxRadius = 20;
    public float attackBoxRadius = 13;
    public float moveFromBoxRadius = 10;
    public float maxHorizSpeed = 15;
    public float moveForce = 50;
    float brakeSpeed = 200;

    int directionToBoxX;
    bool canSeeBox = false;
    [HideInInspector] public bool enemyIsGrounded = false;
    bool moveToBox = false;
    bool dontMove = false;
    bool moveFromBox = false;
    bool isCornered = false;
    float corneredTime = 0;
    float corneredTimeToDash = 0.4f;
    [HideInInspector] public float movingPlatformsExtraWalkSpeed = 0;

    float flippedTime = 0;
    float timeTilFlip = 0.7f;

    GameObject turret;
    public GameObject bullet;
    GameObject newBullet;
    public int bulletsPerVolley = 8;
    public float bulletSpreadMax = 5;
    public float bulletDamage = 40;
    public float bulletSpeed = 25;
    float bulletDespawnTime;
    public float shootTimeInterval = 0.4f;
    public float shootDelay = 0.25f;
    public float shootCoolDown = 2f;
    bool turretCRActive = false;
    bool turretCurrentlyShooting = false;
    bool turretCoolDown = false;

    public bool dashAttack = true;
    bool dashCR = false;
    bool isDashing = false;
    bool dashHitboxActive = false; //only activates when horizontal speed is above a certain amount
    public float dashAttackDistance = 7;
    public float maxDashAttackTime = 0.6f;
    public float dashAttackSpeed = 30;
    int dashDirection = 1;
    public float dashAttackDamage = 50;

    Vector2 bodyRotationVector; //normalized to point upwards
    float bodyRotationAngle;
    float bodyRotationAngleRad;
    float distanceToBox;

    Transform turretTransform;
    Vector2 turretCenterPosition;
    Vector2[] turretVectorLimits = new Vector2[2]; // when upright, Vector to left is 0 and Vector to right is 1.
    float[] turretAngleLimits = new float[2]; // angle versions of turretVectorLimits
    float turretAngularVelocity;
    public float shootingAngularVelocity = 100;
    public float regularAngularVelocity = 400;
    float turretAngleToBox; // always pointing directly to the player, rotates towards here when canseebox == true
    Vector2 turretVectorToBox; //always pointing directly to the player, rotates towards here when canseebox == true
    float realTurretAngle; // actual direction of the current vector, gets applied to turret.transform
    Vector2 realTurretVector; // actual direction of the current vector, gets applied to turret.transform

    bool enemyHitstopActive = false;

    public bool explodeUponDeath = true;
    bool explosionDeathActive = false;
    public GameObject deathExplosion;
    GameObject newExplosion;
    bool enemyExploded = false;
    public float explosionRadius = 5;
    public float explosionTimer = 2;
    public float explosionDamage = 40;

    public bool placeProximityMines = false;
    public GameObject proximityMine;
    GameObject newProximityMine;
    int maxMines = 1;
    bool setMineActive = false;
    List<GameObject> minesPlaced = new List<GameObject>();

    bool enraged = false;

    bool touchingThisEnemy = false;

    int boxLM;
    int groundLM;
    int obstacleAndBoxLM;

    bool aggroActive = false;
    float maxFallSpeed;

    public bool debugLinesEnabled = false;

    void Start()
    {
        if (enemyDifficulty > 3) { enemyDifficulty = 3; }
        if (enemyDifficulty < 1) { enemyDifficulty = 1; }

        enemyRB = gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>();
        enemyTransform = GetComponent<Transform>();
        enemyBodyTransform = gameObject.transform.GetChild(0).gameObject.GetComponent<Transform>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        barrel = transform.GetChild(1).transform.GetChild(0).gameObject;
        EM = GetComponent<EnemyManager>();

        InitialBarrelColor = barrel.GetComponent<SpriteRenderer>().color;

        boxLM = LayerMask.GetMask("Box");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");

        gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.SetActive(true);

        turret = gameObject.transform.GetChild(1).gameObject;
        turretTransform = turret.transform;

        if (explodeUponDeath == true)
        {
            EM.normalDeath = false;
        }

        enemyComponents = transform.GetComponentsInChildren<Transform>();

        maxFallSpeed = enemyRB.gravityScale * -5;
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





        //moveToBox
        if (moveToBox == true && dashCR == false)
        {
            if (Mathf.Abs(enemyRB.velocity.x - movingPlatformsExtraWalkSpeed) <= maxHorizSpeed)
            {
                enemyRB.AddForce(new Vector2(moveForce * directionToBoxX, 0));
            }
        }
        //moveFromBox
        else if (moveFromBox == true && dashCR == false)
        {
            if (Mathf.Abs(enemyRB.velocity.x - movingPlatformsExtraWalkSpeed) <= maxHorizSpeed * 3/4)
            {
                enemyRB.AddForce(new Vector2(-moveForce * directionToBoxX, 0));
            }
            if (placeProximityMines && enemyIsGrounded && setMineActive == false && minesPlaced.Count < maxMines)
            {
                StartCoroutine(PlaceMines());
            }
        }
        //isDashing
        else if (enemyIsGrounded == true && isDashing == true)
        {
            if (Mathf.Abs(enemyRB.velocity.x - movingPlatformsExtraWalkSpeed) <= dashAttackSpeed * 3/4)
            {
                enemyRB.AddForce(new Vector2(moveForce * 3 * dashDirection, 0));
            }
            else if (Mathf.Abs(enemyRB.velocity.x - movingPlatformsExtraWalkSpeed) <= dashAttackSpeed)
            {
                enemyRB.AddForce(new Vector2(moveForce * dashDirection, 0));
            }
        }
        //braking naturally
        else if (enemyIsGrounded == true && isDashing == false)
        {
            float realBrakeSpeed;
            if (enemyRB.velocity.x - movingPlatformsExtraWalkSpeed - brakeSpeed * Time.deltaTime > 0.01f)
            {
                if (walkFloorCheckRightRC.collider == null || walkObstacleCheckRightRC.collider != null || dontMove == true)
                {
                    realBrakeSpeed = brakeSpeed;
                }
                else
                {
                    realBrakeSpeed = brakeSpeed / 6;
                }
                enemyRB.velocity = new Vector2(enemyRB.velocity.x - realBrakeSpeed * Time.deltaTime, enemyRB.velocity.y);
            }
            else if (enemyRB.velocity.x - movingPlatformsExtraWalkSpeed + brakeSpeed * Time.deltaTime < -0.01f)
            {
                if (walkFloorCheckLeftRC.collider == null || walkObstacleCheckLeftRC.collider != null || dontMove == true)
                {
                    realBrakeSpeed = brakeSpeed;
                }
                else
                {
                    realBrakeSpeed = brakeSpeed / 6;
                }
                enemyRB.velocity = new Vector2(enemyRB.velocity.x + realBrakeSpeed * Time.deltaTime, enemyRB.velocity.y);
            }
            else
            {
                enemyRB.velocity = new Vector2(movingPlatformsExtraWalkSpeed, enemyRB.velocity.y);
            }
        }
        //braking when moving faster than max speed and dashing == false
        if (enemyIsGrounded && isDashing == false)
        {
            if (Mathf.Abs(enemyRB.velocity.x - movingPlatformsExtraWalkSpeed) >= maxHorizSpeed)
            {
                int horizMoveDirection = (int)new Vector2(enemyRB.velocity.x, 0).normalized.x;
                enemyRB.velocity = new Vector2(enemyRB.velocity.x - (brakeSpeed / 2) * Time.deltaTime * horizMoveDirection, enemyRB.velocity.y);
            }
        }

        //flip vehicle back upright
        if (flippedTime >= timeTilFlip)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x, 12);
            if (bodyRotationAngle >= 180)
            {
                enemyRB.angularVelocity = 180 + (260 - bodyRotationAngle) * 1.5f;
            }
            else
            {
                enemyRB.angularVelocity = -180 - (bodyRotationAngle - 100) * 1.5f;
            }
        }

        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            moveToBoxRadius *= EM.aggroIncreaseMult;
            attackBoxRadius *= EM.aggroIncreaseMult;
            moveFromBoxRadius *= EM.aggroIncreaseMult;

            maxHorizSpeed *= EM.aggroIncreaseMult;
            moveForce *= EM.aggroIncreaseMult;

            dashAttackSpeed *= EM.aggroIncreaseMult;
            dashAttackDamage *= EM.aggroIncreaseMult;
            corneredTimeToDash *= EM.aggroDecreaseMult;

            timeTilFlip *= EM.aggroDecreaseMult;

            bulletsPerVolley += 2;
            bulletDamage *= EM.aggroIncreaseMult;
            bulletSpeed *= EM.aggroIncreaseMult;
            bulletDespawnTime *= EM.aggroIncreaseMult;
            shootTimeInterval *= EM.aggroDecreaseMult;
            shootDelay *= EM.aggroDecreaseMult;
            shootCoolDown *= EM.aggroDecreaseMult;

            shootingAngularVelocity *= EM.aggroIncreaseMult;
            regularAngularVelocity *= EM.aggroIncreaseMult;

        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;

            moveToBoxRadius /= EM.aggroIncreaseMult;
            attackBoxRadius /= EM.aggroIncreaseMult;
            moveFromBoxRadius /= EM.aggroIncreaseMult;

            maxHorizSpeed /= EM.aggroIncreaseMult;
            moveForce /= EM.aggroIncreaseMult;

            dashAttackSpeed /= EM.aggroIncreaseMult;
            dashAttackDamage /= EM.aggroIncreaseMult;
            corneredTimeToDash /= EM.aggroDecreaseMult;

            timeTilFlip /= EM.aggroDecreaseMult;

            bulletsPerVolley -= 2;
            bulletDamage /= EM.aggroIncreaseMult;
            bulletSpeed /= EM.aggroIncreaseMult;
            bulletDespawnTime /= EM.aggroIncreaseMult;
            shootTimeInterval /= EM.aggroDecreaseMult;
            shootDelay /= EM.aggroDecreaseMult;
            shootCoolDown /= EM.aggroDecreaseMult;

            shootingAngularVelocity /= EM.aggroIncreaseMult;
            regularAngularVelocity /= EM.aggroIncreaseMult;
        }
    }

    private void Update()
    {
        scriptEnabled = transform.GetComponent<EnemyManager>().scriptsEnabled;
        if (scriptEnabled == false)
        {
            gameObject.GetComponent<EnemyBehavior_GroundedVehicle>().enabled = false;
        }

        //set transform and center position for turret
        turretTransform.position = enemyRB.position + bodyRotationVector * enemyBodyTransform.lossyScale.y * 1f;
        turretCenterPosition = turretTransform.position;

        //various angles / vectors used for calculations
        directionToBoxX = (int) new Vector2(boxRB.position.x - turretCenterPosition.x, 0).normalized.x;
        bodyRotationAngle = enemyBodyTransform.rotation.eulerAngles.z;
        bodyRotationAngleRad = (bodyRotationAngle + 90) * Mathf.PI / 180;
        bodyRotationVector = new Vector2(Mathf.Cos(bodyRotationAngleRad), Mathf.Sin(bodyRotationAngleRad)).normalized;
        turretVectorLimits[0] = (new Vector2(-bodyRotationVector.y, bodyRotationVector.x) * 2 + (bodyRotationVector)*-1).normalized;
        turretVectorLimits[1] = (new Vector2(bodyRotationVector.y, -bodyRotationVector.x) * 2 + (bodyRotationVector)*-1).normalized;
        turretAngleLimits[0] = -Mathf.Atan2(turretVectorLimits[0].x, turretVectorLimits[0].y) * Mathf.Rad2Deg;
        turretAngleLimits[1] = -Mathf.Atan2(turretVectorLimits[1].x, turretVectorLimits[1].y) * Mathf.Rad2Deg;

        //turretVectorToBox
        turretVectorToBox = (boxRB.position - turretCenterPosition).normalized;

        //turretAngleToBox
        turretAngleToBox = -Mathf.Atan2(turretVectorToBox.x, turretVectorToBox.y) * Mathf.Rad2Deg;

        enemyRC_RayToBox = Physics2D.RaycastAll(turretCenterPosition, turretVectorToBox, moveToBoxRadius, obstacleAndBoxLM);
        enemyRC_AttackBox = Physics2D.CircleCast(turretCenterPosition, attackBoxRadius, new Vector2(0, 0), 0f, boxLM);
        enemyRC_MoveFromBox = Physics2D.CircleCast(turretCenterPosition, moveFromBoxRadius, new Vector2(0, 0), 0f, boxLM);

        enemyRC_DashAttack = Physics2D.BoxCast(new Vector2(enemyRB.position.x + (dashAttackDistance / 2) * directionToBoxX, enemyRB.position.y + 1f),
            new Vector2(dashAttackDistance, enemyTransform.lossyScale.y * 3f), 0, Vector2.down, 0f, boxLM);
        if (enraged)
        {
            enemyRC_DashAttack = Physics2D.BoxCast(new Vector2(enemyRB.position.x + (dashAttackDistance / 2 - 1) * directionToBoxX, enemyRB.position.y + 2.5f),
            new Vector2(dashAttackDistance + 2, enemyTransform.lossyScale.y * 6f), 0, Vector2.down, 0f, boxLM);
        }

        walkFloorCheckLeftRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 2,
            enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), new Vector2(enemyBodyTransform.lossyScale.x / 3, 3f), 0, Vector2.down, 0f, groundLM);
        walkFloorCheckRightRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 2,
            enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), new Vector2(enemyBodyTransform.lossyScale.x / 3, 3f), 0, Vector2.down, 0f, groundLM);
        walkObstacleCheckLeftRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 1.5f,
            enemyRB.position.y), new Vector2(enemyBodyTransform.lossyScale.x / 4, enemyBodyTransform.lossyScale.y / 3), 0, Vector2.down, 0f, groundLM);
        walkObstacleCheckRightRC = Physics2D.BoxCast(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 1.5f,
            enemyRB.position.y), new Vector2(enemyBodyTransform.lossyScale.x / 4, enemyBodyTransform.lossyScale.y / 3), 0, Vector2.down, 0f, groundLM);


        float distToBox = 1000;
        float distToObstacle = 1000;
        bool boxInRadius = false;
        foreach (RaycastHit2D col in enemyRC_RayToBox)
        {
            if (col.collider != null && 1 << col.collider.gameObject.layer == boxLM)
            {
                distToBox = col.distance;
                boxInRadius = true;
            }
            if (col.collider != null && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles") && col.collider.gameObject.tag != "Fence")
            {
                distToObstacle = Mathf.Min(distToObstacle, col.distance);
            }
        }

        //inBoxLOS = true if within moveToBox distance, no obstacles in between, and not underneath box
        if (boxInRadius == true && distToBox < distToObstacle
            && Vector2.Dot(turretVectorToBox, bodyRotationVector) >= Vector2.Dot(turretVectorLimits[0], bodyRotationVector))
        {
            canSeeBox = true;
            EM.canSeeItem = true;
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }

        //turret rotation logic
        realTurretAngle = turretTransform.eulerAngles.z;
        realTurretVector = new Vector2(Mathf.Cos(realTurretAngle * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin(realTurretAngle * Mathf.Deg2Rad + Mathf.PI / 2));

        if (turretCurrentlyShooting)
        {
            turretAngularVelocity = shootingAngularVelocity;
        }
        else
        {
            turretAngularVelocity = regularAngularVelocity;
        }

        if (canSeeBox == true && explosionDeathActive == false && dashCR == false)
        {
            if (Vector2.Dot(realTurretVector, turretVectorToBox) >= 0)
            {
                realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleToBox, turretAngularVelocity * Time.deltaTime);
            }
            else
            {
                if (Vector2.Dot(realTurretVector, turretVectorLimits[0]) < 0 && Vector2.Dot(realTurretVector, turretVectorLimits[1]) < 0)
                {
                    realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleToBox, turretAngularVelocity * Time.deltaTime);
                }
                else if (Vector2.Dot(realTurretVector, turretVectorLimits[0]) >= 0)
                {
                    if (Vector2.Dot(turretVectorToBox, turretVectorLimits[1]) < 0)
                    {
                        realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleToBox, turretAngularVelocity * Time.deltaTime);
                    }
                    else
                    {
                        realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleLimits[0] + 180, turretAngularVelocity * Time.deltaTime);
                    }
                }
                else if (Vector2.Dot(realTurretVector, turretVectorLimits[1]) >= 0)
                {
                    if (Vector2.Dot(turretVectorToBox, turretVectorLimits[0]) < 0)
                    {
                        realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleToBox, turretAngularVelocity * Time.deltaTime);
                    }
                    else
                    {
                        realTurretAngle = Mathf.MoveTowardsAngle(turretTransform.eulerAngles.z, turretAngleLimits[1] + 180, turretAngularVelocity * Time.deltaTime);
                    }
                }
            }
        }
        else
        {
            if (Vector2.Dot(realTurretVector, bodyRotationVector) < Vector2.Dot(turretVectorLimits[0], bodyRotationVector))
            {
                if (Vector2.Dot(realTurretVector, turretVectorLimits[0]) >= Vector2.Dot(realTurretVector, turretVectorLimits[1]))
                {
                    realTurretAngle = turretAngleLimits[0];
                }
                else
                {
                    realTurretAngle = turretAngleLimits[1];
                }
            }
        }
        turretTransform.eulerAngles = Vector3.forward * (realTurretAngle);

        //condition for flipping back over
        if (enemyIsGrounded == false && Mathf.Abs(enemyRB.velocity.y) < 0.5f && explosionDeathActive == false)
        {
            flippedTime += Time.deltaTime;
        }
        else
        {
            flippedTime = 0;
        }

        //attacks
        if (canSeeBox == true && enemyRC_AttackBox.collider != null && explosionDeathActive == false && EM.initialDelay == false)
        {
            if (turretCRActive == false && dashCR == false && enemyIsGrounded)
            {
                StartCoroutine(ShootTurret());
            }
            if (placeProximityMines && enemyIsGrounded && setMineActive == false && minesPlaced.Count < maxMines)
            {
                StartCoroutine(PlaceMines());
            }
        }
        if (canSeeBox && enraged && turretCRActive == false && dashCR == false && enemyIsGrounded && EM.initialDelay == false)
        {
            StartCoroutine(ShootTurret());
        }

        //move if inside moveToBox radius but outside attack radius, attack if inside attack radius, move away if inside moveFromBox radius
        if (enemyIsGrounded == true && canSeeBox == true && explosionDeathActive == false && dashCR == false)
        {
            if (enemyRC_AttackBox.collider == null)
            {
                if ((directionToBoxX == 1 && walkFloorCheckRightRC.collider != null && walkObstacleCheckRightRC.collider == null) ||
                    (directionToBoxX == -1 && walkFloorCheckLeftRC.collider != null && walkObstacleCheckLeftRC.collider == null))
                { 
                    moveToBox = true;
                    dontMove = false;
                    moveFromBox = false;
                }
                else
                {
                    moveToBox = false;
                    dontMove = false;
                    moveFromBox = false;
                }
                isCornered = false;
            }
            else if (enemyRC_AttackBox.collider != null && enemyRC_MoveFromBox.collider == null)
            {
                moveToBox = false;
                dontMove = true;
                moveFromBox = false;

                if ((directionToBoxX == 1 && walkFloorCheckRightRC.collider != null && walkObstacleCheckRightRC.collider == null) ||
                    (directionToBoxX == -1 && walkFloorCheckLeftRC.collider != null && walkObstacleCheckLeftRC.collider == null))
                {
                    isCornered = true;
                }
                else
                {
                    isCornered = false;
                }
            }
            else if (enemyRC_MoveFromBox.collider != null)
            {
                if ((directionToBoxX == -1 && walkFloorCheckRightRC.collider != null && walkObstacleCheckRightRC.collider == null) ||
                    (directionToBoxX == 1 && walkFloorCheckLeftRC.collider != null && walkObstacleCheckLeftRC.collider == null))
                {
                    moveToBox = false;
                    dontMove = false;
                    moveFromBox = true;

                    isCornered = false;
                }
                else
                {
                    moveToBox = false;
                    dontMove = false;
                    moveFromBox = false;

                    isCornered = true;
                }
            }
            if (corneredTime >= corneredTimeToDash && (turretCoolDown == true || turretCRActive == false)
                && dashCR == false && enemyRC_DashAttack.collider != null && dashAttack == true && EM.initialDelay == false)
            {
                StartCoroutine(Dash());
                isCornered = false;
            }
        }
        else
        {
            moveToBox = false;
            dontMove = false;
            moveFromBox = false;

            isCornered = false;
        }
        //iscornered timer to activate dash attack
        if (isCornered == true)
        {
            corneredTime += Time.deltaTime;
        }
        else
        {
            corneredTime = 0;
        }
        //activate dashing hitbox based on the speed of the enemy OR if hitstop is active from dashing into the box
        if (isDashing == true && (Mathf.Abs(enemyRB.velocity.x) >= maxHorizSpeed || enemyHitstopActive == true))
        {
            dashHitboxActive = true;
        }
        else
        {
            dashHitboxActive = false;
        }


        //determine if touchingthisenemy == true
        bool thisEnemyFound = false;
        foreach (RaycastHit2D enemy in Box.attackRayCast)
        {
            if (enemy.collider != null && enemy.transform.root.gameObject == transform.gameObject)
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
        //if the box collides with the enemy...
        if (touchingThisEnemy == true && explosionDeathActive == false)
        {
            //...and if the enemy is not currently dashing...
            if (dashHitboxActive == false)
            {
                //...and if the box is currently attacking, damage the enemy.
                if (Box.boxHitboxActive)
                {
                    EM.enemyWasDamaged = true;
                    if (EM.enemyIsInvulnerable == false)
                    {
                        Box.activateHitstop = true;
                    }
                }
                //...and if the box is not attacking, push the box back.
                else
                {
                    Box.activatePushBack = true;
                    Box.pushBackMagnitude = 6;
                }
            }
            //...and if the enemy IS currently dashing, damage the box.
            else
            {
                if (Box.isInvulnerable == false)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = dashAttackDamage;
                    Box.boxDamageDirection = new Vector2(dashDirection, 1).normalized;
                    if (EM.shockActive)
                    {
                        Box.activateShock = true;
                    }
                    StartCoroutine(EnemyHitstop());
                    if (BoxPerks.spikesActive)
                    {
                        EM.enemyWasDamaged = true;
                        if (EM.enemyIsInvulnerable == false)
                        {
                            Box.activateHitstop = true;
                        }
                    }
                }
                //...and if the enemy IS currently dashing, but the box is invulnerable and is attacking, damage the enemy.
                else if (Box.isInvulnerable && Box.boxHitstopActive == false && Box.boxHitboxActive)
                {
                    EM.enemyWasDamaged = true;
                    if (EM.enemyIsInvulnerable == false)
                    {
                        Box.activateHitstop = true;
                    }
                }
            }
        }

        distanceToBox = (boxRB.position - enemyRB.position).magnitude;
        if (Box.pulseActive == true && distanceToBox <= Box.pulseRadius * 1.05 && isDashing == true && explosionDeathActive == false)
        {
            isDashing = false;
            enemyRB.velocity = new Vector2(12 * dashDirection, 12);
            enemyRB.angularVelocity = 400 * dashDirection;
        }

        if (EM.enemyWasKilled == true && explodeUponDeath == true &&
            EM.hitstopImpactActive == false && explosionDeathActive == false)
        {
            StartCoroutine(ExplosionDeath());
        }
        if (enemyExploded == true)
        {
            newExplosion = Instantiate(deathExplosion, enemyRB.position, Quaternion.identity);
            newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
            newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;
            newExplosion.GetComponent<Transform>().localScale = new Vector2(explosionRadius * 2, explosionRadius * 2);
            EM.scriptsEnabled = false;
        }

        if (placeProximityMines)
        {
            for (int i = 0; i < minesPlaced.Count; i++)
            {
                if (minesPlaced[i] == null)
                {
                    minesPlaced.RemoveAt(i);
                }
            }
        }

        if (EM.enemyHealth == 1 && enraged == false && enemyDifficulty == 3)
        {
            enraged = true;
            enemyRB.transform.GetComponent<Renderer>().material.color = new Color(5, 0, 0, 1);
            shootTimeInterval = 0.02f;
            shootCoolDown = 0.9f;
            bulletSpreadMax *= 0.6f;
            maxHorizSpeed *= 1.1f;
            brakeSpeed *= 1.2f;
            maxDashAttackTime *= 0.6f;
        }

        if (turretCRActive)



        //debug lines
        if (debugLinesEnabled == true && enemyExploded == false)
        {
            //line to box
            if (canSeeBox == true)
            {
                Debug.DrawRay(turretCenterPosition, moveToBoxRadius * (boxRB.position - turretCenterPosition).normalized, Color.white);
                Debug.DrawRay(turretCenterPosition, attackBoxRadius * (boxRB.position - turretCenterPosition).normalized, Color.red);
                Debug.DrawRay(turretCenterPosition, moveFromBoxRadius * (boxRB.position - turretCenterPosition).normalized, Color.green);
            }
            else
            {
                Debug.DrawRay(turretCenterPosition, moveToBoxRadius * (boxRB.position - turretCenterPosition).normalized, Color.gray);
            }
            //body colors
            if (isDashing)
            {
                gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
            else if (enemyIsGrounded)
            {
                gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.cyan;
            }
            else
            {
                gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = Color.white;
            }

            //dashAttack box
            if (enemyRC_DashAttack.collider != null)
            {

                if (enraged == false)
                {
                    Debug.DrawRay(new Vector2(enemyRB.position.x + (dashAttackDistance / 2) * directionToBoxX, (enemyRB.position.y + 1) - enemyTransform.lossyScale.y * 1.5f),
                        Vector2.up * enemyTransform.lossyScale.y * 3, Color.red);
                    Debug.DrawRay(new Vector2((enemyRB.position.x + (dashAttackDistance / 2) * directionToBoxX) - dashAttackDistance / 2,
                        enemyRB.position.y + 1f), Vector2.right * dashAttackDistance, Color.red);
                }
                else
                {
                    Debug.DrawRay(new Vector2(enemyRB.position.x + (dashAttackDistance / 2 - 1) * directionToBoxX, enemyRB.position.y + 2.5f - enemyTransform.lossyScale.y * 3),
                        Vector2.up * enemyTransform.lossyScale.y * 6, Color.red);
                    Debug.DrawRay(new Vector2((enemyRB.position.x + (dashAttackDistance / 2 - 1) * directionToBoxX) - dashAttackDistance / 2 - 1,
                        enemyRB.position.y + 2.5f), Vector2.right * (dashAttackDistance + 2), Color.red);
                }
            }
            else
            {
                if (enraged == false)
                {
                    Debug.DrawRay(new Vector2(enemyRB.position.x + (dashAttackDistance / 2) * directionToBoxX, (enemyRB.position.y + 1) - enemyTransform.lossyScale.y * 1.5f),
                        Vector2.up * enemyTransform.lossyScale.y * 3, Color.white);
                    Debug.DrawRay(new Vector2((enemyRB.position.x + (dashAttackDistance / 2) * directionToBoxX) - dashAttackDistance / 2,
                        enemyRB.position.y + 1f), Vector2.right * dashAttackDistance, Color.white);
                }
                else
                {
                    Debug.DrawRay(new Vector2(enemyRB.position.x + (dashAttackDistance / 2 - 1) * directionToBoxX, enemyRB.position.y + 2.5f - enemyTransform.lossyScale.y * 3),
                        Vector2.up * enemyTransform.lossyScale.y * 6, Color.white);
                    Debug.DrawRay(new Vector2((enemyRB.position.x + (dashAttackDistance / 2 - 1) * directionToBoxX) - dashAttackDistance / 2 - 1,
                        enemyRB.position.y + 2.5f), Vector2.right * (dashAttackDistance + 2), Color.white);
                }
            }
            //walkFloorCheckRightRC box
            if (walkFloorCheckRightRC.collider == null)
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 2,
                    (enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f) - 1.5f), Vector2.up * 3, Color.red);
                Debug.DrawRay(new Vector2((enemyRB.position.x + enemyBodyTransform.lossyScale.x * 2) - enemyBodyTransform.lossyScale.x / 3,
                    enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), Vector2.right * enemyBodyTransform.lossyScale.x * 2 / 3, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 2,
                    (enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f) - 1.5f), Vector2.up * 3, Color.white);
                Debug.DrawRay(new Vector2((enemyRB.position.x + enemyBodyTransform.lossyScale.x * 2) - enemyBodyTransform.lossyScale.x / 3,
                    enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), Vector2.right * enemyBodyTransform.lossyScale.x * 2 / 3, Color.white);
            }
            //walkFloorCheckLeftRC box
            if (walkFloorCheckLeftRC.collider == null)
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 2,
                    (enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f) - 1.5f), Vector2.up * 3, Color.red);
                Debug.DrawRay(new Vector2((enemyRB.position.x - enemyBodyTransform.lossyScale.x * 2) - enemyBodyTransform.lossyScale.x / 3,
                    enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), Vector2.right * enemyBodyTransform.lossyScale.x * 2 / 3, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 2,
                    (enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f) - 1.5f), Vector2.up * 3, Color.white);
                Debug.DrawRay(new Vector2((enemyRB.position.x - enemyBodyTransform.lossyScale.x * 2) - enemyBodyTransform.lossyScale.x / 3,
                    enemyRB.position.y - enemyBodyTransform.lossyScale.y - 1.5f), Vector2.right * enemyBodyTransform.lossyScale.x * 2 / 3, Color.white);
            }
            //walkObstacleCheckRightRC box
            if (walkObstacleCheckRightRC.collider != null)
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 1.5f,
                    (enemyRB.position.y) - enemyBodyTransform.lossyScale.y / 6), Vector2.up * enemyBodyTransform.lossyScale.y / 3, Color.red);
                Debug.DrawRay(new Vector2((enemyRB.position.x + enemyBodyTransform.lossyScale.x * 1.5f) - enemyBodyTransform.lossyScale.x / 8,
                    enemyRB.position.y), Vector2.right * enemyBodyTransform.lossyScale.x / 4, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x + enemyBodyTransform.lossyScale.x * 1.5f,
                    (enemyRB.position.y) - enemyBodyTransform.lossyScale.y / 6), Vector2.up * enemyBodyTransform.lossyScale.y / 3, Color.white);
                Debug.DrawRay(new Vector2((enemyRB.position.x + enemyBodyTransform.lossyScale.x * 1.5f) - enemyBodyTransform.lossyScale.x / 8,
                    enemyRB.position.y), Vector2.right * enemyBodyTransform.lossyScale.x / 4, Color.white);
            }
            //walkObstacleCheckLeftRC box
            if (walkObstacleCheckLeftRC.collider != null)
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 1.5f,
                    (enemyRB.position.y) - enemyBodyTransform.lossyScale.y / 6), Vector2.up * enemyBodyTransform.lossyScale.y / 3, Color.red);
                Debug.DrawRay(new Vector2((enemyRB.position.x - enemyBodyTransform.lossyScale.x * 1.5f) - enemyBodyTransform.lossyScale.x / 8,
                    enemyRB.position.y), Vector2.right * enemyBodyTransform.lossyScale.x / 4, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(enemyRB.position.x - enemyBodyTransform.lossyScale.x * 1.5f,
                    (enemyRB.position.y) - enemyBodyTransform.lossyScale.y / 6), Vector2.up * enemyBodyTransform.lossyScale.y / 3, Color.white);
                Debug.DrawRay(new Vector2((enemyRB.position.x - enemyBodyTransform.lossyScale.x * 1.5f) - enemyBodyTransform.lossyScale.x / 8,
                    enemyRB.position.y), Vector2.right * enemyBodyTransform.lossyScale.x / 4, Color.white);
            }

            //body vector + turret limits
            Debug.DrawRay(enemyRB.position + bodyRotationVector * enemyBodyTransform.localScale.y, bodyRotationVector);
            Debug.DrawRay(enemyRB.position + bodyRotationVector * enemyBodyTransform.localScale.y, turretVectorLimits[0]);
            Debug.DrawRay(enemyRB.position + bodyRotationVector * enemyBodyTransform.localScale.y, turretVectorLimits[1]);
        }
    }

    IEnumerator ShootTurret()
    {
        turretCRActive = true;
        StartCoroutine(BarrelColor());
        float window = shootDelay;
        float timer = 0;
        while (timer < window)
        {
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        if (EM.initialDelay)
        {
            yield return null;
        }
        turretCurrentlyShooting = true;
        int bulletsShot = 0;
        float bulletSpread;
        bulletDespawnTime = attackBoxRadius * 2.5f / bulletSpeed;
        while (bulletsShot < bulletsPerVolley && scriptEnabled == true && explosionDeathActive == false && EM.enemyWasKilled == false && EM.hitstopImpactActive == false)
        {
            bulletSpread = (-bulletSpreadMax / 2) + Random.value * bulletSpreadMax;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, realTurretAngle + bulletSpread);
            newBullet = Instantiate(bullet, turretCenterPosition + realTurretVector * 0.8f, bulletRotation);
            newBullet.GetComponent<Transform>().localScale = new Vector2(0.26f, 0.26f);
            newBullet.GetComponent<BulletScript>().bulletDespawnWindow = bulletDespawnTime;
            newBullet.GetComponent<BulletScript>().bulletDamage = bulletDamage;
            newBullet.GetComponent<Rigidbody2D>().velocity = (realTurretVector +
                Vector2.Perpendicular(realTurretVector) * Mathf.Sin(bulletSpread * Mathf.PI / 180)).normalized * bulletSpeed;
            if (aggroActive)
            {
                newBullet.GetComponent<BulletScript>().aggro = true;
            }

            if (shootTimeInterval > 0.2f)
            {
                enemyRB.AddForce(Vector2.up * 0.5f, ForceMode2D.Impulse);
                if (enemyIsGrounded == true)
                {
                    enemyRB.angularVelocity = (250 - 15 * Mathf.Abs(enemyRB.velocity.x)) * directionToBoxX;
                }
            }
            bulletsShot += 1;
            if (shootTimeInterval > 0)
            {
                float shootTimeIntervalTimer = 0;
                while (shootTimeIntervalTimer <= shootTimeInterval)
                {
                    shootTimeIntervalTimer += Time.deltaTime;
                    yield return null;
                }
            }
        }
        turretCurrentlyShooting = false;
        turretCoolDown = true;
        yield return new WaitForSeconds(shootCoolDown);
        turretCoolDown = false;
        turretCRActive = false;
    }
    IEnumerator BarrelColor()
    {
        EM.attackActive = true;
        Color barrelColor = new Color(InitialBarrelColor.r, InitialBarrelColor.g + 0.6f, InitialBarrelColor.b);
        while (turretCRActive && turretCoolDown == false && EM.enemyWasKilled == false)
        {
            if (EM.flashOn == false)
            {
                barrel.GetComponent<SpriteRenderer>().color = barrelColor;
            }
            yield return null;
        }
        EM.attackActive = false;
        barrel.GetComponent<SpriteRenderer>().color = InitialBarrelColor;
    }
    IEnumerator Dash()
    {
        dashCR = true;
        isDashing = true;
        EM.normalPulse = false;
        dashDirection = directionToBoxX;
        float dashTimer = 0;
        EM.physicalHitboxActive = true;
        //continue dashing as long as the timer isn't up and there's no reason to brake
        while (dashTimer <= maxDashAttackTime && isDashing && explosionDeathActive == false && EM.hitstopImpactActive == false && 
            ((dashDirection == 1 && walkFloorCheckRightRC.collider != null && walkObstacleCheckRightRC.collider == null) ||
            (dashDirection == -1 && walkFloorCheckLeftRC.collider != null && walkObstacleCheckLeftRC.collider == null)))
        {
            if (enemyHitstopActive == false)
            {
                dashTimer += Time.deltaTime;
            }
            yield return null;
        }
        EM.physicalHitboxActive = false;
        isDashing = false;
        if (EM.hitstopImpactActive == false)
        {
            yield return new WaitForSeconds(0.6f);
        }
        EM.normalPulse = true;
        dashCR = false;
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
        enemyHitstopActive = false;
        enemyRB.velocity = enemyHitstopVelocity;
    }
    IEnumerator ExplosionDeath()
    {
        explosionDeathActive = true;
        EM.reflectedBulletsWillDamage = false;
        yield return null;
        foreach (Transform transform in enemyComponents)
        {
            if (transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().material.color = Color.gray;
            }
        }
        EM.enemyIsInvulnerable = false;
        enemyRB.velocity = new Vector2(0, 12);
        enemyRB.angularVelocity = 400 * new Vector2(bodyRotationVector.x, 0).normalized.x;
        yield return new WaitForSeconds(explosionTimer);
        if (placeProximityMines)
        {
            foreach (GameObject mine in minesPlaced)
            {
                mine.GetComponent<ProximityMine>().remoteExplode = true;
            }
        }
        EM.startEnemyDeath = true;
        enemyExploded = true;
    }
    IEnumerator PlaceMines()
    {
        setMineActive = true;
        yield return new WaitForSeconds(2f);
        RaycastHit2D mineCheck = Physics2D.CircleCast(enemyRB.position, 5, Vector2.zero, 0, LayerMask.GetMask("Enemy Device"));
        if (enemyIsGrounded && mineCheck.collider == null && EM.enemyWasKilled == false)
        {
            newProximityMine = Instantiate(proximityMine, enemyRB.position, Quaternion.identity);
            minesPlaced.Add(newProximityMine);
        }
        setMineActive = false;
    }
}
