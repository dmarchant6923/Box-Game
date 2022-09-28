using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Turret : MonoBehaviour
{
    int obstacleLM;
    int obstacleAndBoxLM;
    int boxLM;

    public float offsetFromWallMult = 0.15f;
    public bool canAttachToGround = true;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    GameObject barrel;
    Color initialBarrelColor;
    EnemyManager EM;
    bool touchingThisEnemy = false;

    Vector2 initialPosition;

    Vector2 vectorToBox;
    Vector2 vectorToBoxNormalized;
    float angleToBox;
    bool canSeeBox = false;

    float targetAngle;
    Vector2 targetVector;
    float barrelAngle;
    Vector2 barrelVector;
    Vector2[] barrelVectorLimits = new Vector2[2];
    float[] barrelAngleLimits = new float[2];
    Vector2 halfwayVector;
    float halfwayAngle;
    float minDotProduct;

    float angularVelocity;
    public float shootAngularVelocity = 60;
    public float normalAngularVelocity = 200;

    public bool shootBullets = true;
    public bool shootGrenades = false;
    public bool fireLaserBeam = false;

    public float shootBoxRadius = 25;
    public float delayBeforeShooting = 0.5f;
    public float restTime = 3;
    bool isCurrentlyShooting = false;
    bool isWaitingToShoot = false;
    bool isOnCooldown = false;

    public GameObject bullet;
    GameObject newBullet;
    public int bulletsPerAttack = 6;
    public float bulletSpreadMax = 7;
    public float bulletSpeed = 20;
    public float bulletInterval = 0.5f;
    public float bulletDespawnTime;
    public float bulletDamage = 20;

    public GameObject grenade;
    GameObject newGrenade;
    float grenadeGravity;
    public int grenadesPerAttack = 4;
    public float grenadeSpreadMax = 30;
    public float grenadeSpeed = 25;
    public float grenadeInterval = 0.6f;
    bool higherAngle = false;

    LineRenderer laserBeam;
    LineRenderer laserCenter;
    float laserDuration = 2.5f;
    float laserDOT = 15;
    bool laserHittingBox = false;
    GameObject fuel;
    GameObject fire;
    GameObject newFire;

    bool aggroActive = false;

    public bool debugLines = false;

    bool scriptEnabled = true;

    void Start()
    {
        initialPosition = transform.position;
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        barrel = transform.GetChild(0).gameObject;
        initialBarrelColor = barrel.GetComponent<SpriteRenderer>().color;
        EM = GetComponent<EnemyManager>();
        if (fireLaserBeam)
        {
            fuel = transform.GetChild(5).gameObject;
        }

        obstacleLM = LayerMask.GetMask("Obstacles");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        boxLM = LayerMask.GetMask("Box");

        RaycastHit2D[] rays = new RaycastHit2D[4];
        rays[0] = Physics2D.Raycast(transform.position, Vector2.left, transform.lossyScale.y * 2, obstacleLM);
        rays[1] = Physics2D.Raycast(transform.position, Vector2.right, transform.lossyScale.y * 2, obstacleLM);
        rays[2] = Physics2D.Raycast(transform.position, Vector2.up, transform.lossyScale.y * 2, obstacleLM);
        rays[3] = Physics2D.Raycast(transform.position, Vector2.down, transform.lossyScale.y * 2, obstacleLM);
        if (canAttachToGround == false)
        {
            rays[3] = Physics2D.Raycast(transform.position, Vector2.up, transform.lossyScale.y * 2, obstacleLM);
        }

        bool foundWall = false;
        float distance = 1000;
        int rayint = 0;
        for (int i = 0; i < rays.Length; i++)
        {
            if (rays[i].collider != null)
            {
                if (rays[i].distance < distance)
                {
                    distance = rays[i].distance;
                    rayint = i;
                    foundWall = true;
                }
            }
        }
        if (foundWall)
        {
            enemyRB.position = rays[rayint].point + rays[rayint].normal * transform.lossyScale.x * offsetFromWallMult;
            targetAngle = Tools.VectorToAngle(rays[rayint].normal);
            transform.eulerAngles = Vector3.forward * targetAngle;
        }

        bool barrelAngleCheck = false;
        float currentBarrelAngle = targetAngle;
        halfwayAngle = targetAngle;
        Vector2 currentBarrelVector = Tools.AngleToVector(currentBarrelAngle);
        bool foundOne = false;
        float angleTraveled = 0;
        while (barrelAngleCheck == false && foundWall == true)
        {
            RaycastHit2D checkRay = Physics2D.Raycast(enemyRB.position, currentBarrelVector, transform.lossyScale.x * 0.75f, obstacleLM);
            if (checkRay.collider == null && foundOne == false)
            {
                currentBarrelAngle += 2;
                halfwayAngle += 2;
                currentBarrelVector = Tools.AngleToVector(currentBarrelAngle);
                angleTraveled += 2;
                continue;
            }
            if (checkRay.collider != null && foundOne == false)
            {
                currentBarrelAngle -= 2;
                halfwayAngle -= 2;
                currentBarrelVector = Tools.AngleToVector(currentBarrelAngle);
                barrelAngleLimits[0] = currentBarrelAngle;
                barrelVectorLimits[0] = currentBarrelVector;
                foundOne = true;
                currentBarrelAngle -= (angleTraveled - 2);
                halfwayAngle -= (angleTraveled - 2) / 2;
                continue;
            }
            if (checkRay.collider == null && foundOne == true)
            {
                currentBarrelAngle -= 2;
                halfwayAngle -= 1;
                currentBarrelVector = Tools.AngleToVector(currentBarrelAngle);
                continue;
            }
            if (checkRay.collider != null && foundOne == true)
            {
                currentBarrelAngle += 2;
                halfwayAngle += 1;
                currentBarrelVector = Tools.AngleToVector(currentBarrelAngle);
                barrelAngleLimits[1] = currentBarrelAngle;
                barrelVectorLimits[1] = currentBarrelVector;
                halfwayVector = Tools.AngleToVector(halfwayAngle);
                targetAngle = halfwayAngle;
                minDotProduct = Vector2.Dot(halfwayVector, barrelVectorLimits[0]);
                barrelAngleCheck = true;
            }
        }

        barrelAngle = targetAngle;
        targetVector = Tools.AngleToVector(targetAngle);

        bulletDespawnTime = shootBoxRadius * 2 / bulletSpeed;
        EM.normalPulse = false;
        EM.reflectedBulletsWillDamage = false;
        EM.keepAsKinematic = true;

        if (shootBullets) { shootGrenades = false; fireLaserBeam = false; }
        if (shootGrenades) { fireLaserBeam = false; }

        if (shootGrenades)
        {
            grenadeGravity = grenade.GetComponent<Rigidbody2D>().gravityScale;
        }
        if (fireLaserBeam)
        {
            laserBeam = transform.GetChild(2).gameObject.GetComponent<LineRenderer>();
            laserCenter = transform.GetChild(3).gameObject.GetComponent<LineRenderer>();
        }

        StartCoroutine(InitialDelay());

        fire = EM.fire;
    }

    void Update()
    {
        //disable script when enemy is killed (not currently working properly)
        scriptEnabled = EM.scriptsEnabled;
        if (EM.enemyWasKilled)
        {
            StopAllCoroutines();
            if (fireLaserBeam)
            {
                laserBeam.enabled = false;
                laserCenter.enabled = false;
            }
        }

        //logic for player to physically kill enemy
        bool thisEnemyFound = false;
        if (Box.attackRayCast.Length > 0)
        {
            foreach (RaycastHit2D enemy in Box.attackRayCast)
            {
                if (enemy.collider != null && enemy.collider.gameObject == this.gameObject)
                {
                    thisEnemyFound = true;
                }
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

        if (EM.enemyIsFrozen)
        {
            return;
        }

        //barrelVector
        vectorToBox = boxRB.position - enemyRB.position;
        vectorToBoxNormalized = vectorToBox.normalized;
        //barrelAngle
        angleToBox = Tools.VectorToAngle(vectorToBoxNormalized);

        //inBoxLOS, and keeping the barrel angle still once inBoxLOS = false
        if (vectorToBox.magnitude < shootBoxRadius && Tools.LineOfSight(enemyRB.position, vectorToBox) && Vector2.Dot(vectorToBox.normalized, halfwayVector) > minDotProduct)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            if (shootBullets || fireLaserBeam)
            {
                targetAngle = angleToBox;
            }
            else if (shootGrenades)
            {
                Vector2 distanceVector = boxRB.position - (enemyRB.position + barrelVector * 1.5f * transform.lossyScale.x / 2);
                float g = -Physics.gravity.y * grenadeGravity;
                float deltaX = distanceVector.x;
                float deltaY = distanceVector.y;

                float v = grenade.GetComponent<Snowball>() ? grenadeSpeed : grenadeSpeed * 1.2f;
                float b = v * v - deltaY * g;
                float discriminant = b * b - g * g * (deltaX * deltaX + deltaY * deltaY);
                float discRoot;
                float T;
                float vx;
                float vy;
                if (discriminant >= 0)
                {
                    discRoot = Mathf.Sqrt(discriminant);
                    if (higherAngle)
                    {
                        T = Mathf.Sqrt((b + discRoot) * 2 / (g * g));
                    }
                    else
                    {
                        T = Mathf.Sqrt((b - discRoot) * 2 / (g * g));
                    }
                    vx = deltaX / T;
                    vy = deltaY / T + T * g / 2;

                    targetAngle = -Mathf.Atan2(vx, vy) * Mathf.Rad2Deg;
                    targetVector = Tools.AngleToVector(targetAngle);
                    RaycastHit2D detectObstacle = Physics2D.Raycast(transform.position, targetVector, 5, obstacleLM);

                    if (detectObstacle.collider != null)
                    {
                        targetAngle = angleToBox;
                    }
                }
                else
                {
                    if (Mathf.Abs(targetAngle) >= 45)
                    {
                        targetAngle = 45 * -Mathf.Sign(deltaX);
                    }
                    else
                    {
                        targetAngle = angleToBox;
                    }
                }
            }
            targetVector = Tools.AngleToVector(targetAngle);
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }

        //determine the speed of barrel swivel
        if (isCurrentlyShooting == true && isWaitingToShoot == false && isOnCooldown == false)
        {
            angularVelocity = shootAngularVelocity;
        }
        else
        {
            angularVelocity = normalAngularVelocity;
        }

        //making sure it doesn't turn into the wall it's attached to
        if (EM.hitstopImpactActive == false)
        {
            if (Vector2.Dot(barrelVector, halfwayVector) < minDotProduct)
            {
                if (Vector2.Dot(barrelVector, barrelVectorLimits[0]) > Vector2.Dot(barrelVector, barrelVectorLimits[1]))
                {
                    //barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[0], angularVelocity * Time.deltaTime);
                    float fixedAngle = Tools.VectorToAngle((barrelVectorLimits[0] + halfwayVector * 0.01f).normalized);
                    barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, fixedAngle, angularVelocity * Time.deltaTime);
                }
                else
                {
                    //barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[1], angularVelocity * Time.deltaTime);
                    float fixedAngle = Tools.VectorToAngle((barrelVectorLimits[1] + halfwayVector * 0.01f).normalized);
                    barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, fixedAngle, angularVelocity * Time.deltaTime);
                }
            }
            else
            {
                if (Vector2.Dot(barrelVector, targetVector) >= 0)
                {
                    barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, angularVelocity * Time.deltaTime);
                }
                else
                {
                    if (Vector2.Dot(barrelVector, barrelVectorLimits[0]) < 0 && Vector2.Dot(barrelVector, barrelVectorLimits[1]) < 0)
                    {
                        barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, angularVelocity * Time.deltaTime);
                    }
                    else if (Vector2.Dot(barrelVector, barrelVectorLimits[0]) >= 0)
                    {
                        if (Vector2.Dot(targetVector, barrelVectorLimits[1]) < 0)
                        {
                            barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, angularVelocity * Time.deltaTime);
                        }
                        else
                        {
                            barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[0] + 180, angularVelocity * Time.deltaTime);
                        }
                    }
                    else if (Vector2.Dot(barrelVector, barrelVectorLimits[1]) >= 0)
                    {
                        if (Vector2.Dot(targetVector, barrelVectorLimits[0]) < 0)
                        {
                            barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, angularVelocity * Time.deltaTime);
                        }
                        else
                        {
                            barrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[1] + 180, angularVelocity * Time.deltaTime);
                        }
                    }
                }
            }
        }

        barrelVector = Tools.AngleToVector(barrelAngle);

        transform.eulerAngles = Vector3.forward * (barrelAngle);


        if (canSeeBox == true && isCurrentlyShooting == false && EM.initialDelay == false)
        {
            if (shootBullets)
            {
                StartCoroutine(ShootBullets());
            }
            if (shootGrenades)
            {
                StartCoroutine(ShootGrenades());
            }
            if (fireLaserBeam)
            {
                StartCoroutine(LaserBeam());
            }
        }



        //debug lines
        if (debugLines)
        {
            //Debug.DrawRay(initialPosition, Vector2.up * transform.lossyScale.y * 2);
            //Debug.DrawRay(initialPosition, Vector2.down * transform.lossyScale.y * 2);
            //Debug.DrawRay(initialPosition, Vector2.left * transform.lossyScale.y * 2);
            //Debug.DrawRay(initialPosition, Vector2.right * transform.lossyScale.y * 2);

            //Debug.DrawRay(enemyRB.position, vectorToBoxNormalized * shootBoxRadius, Color.red);

            Debug.DrawRay(transform.position, barrelVectorLimits[0] * transform.lossyScale.x * 0.75f);
            Debug.DrawRay(transform.position, barrelVectorLimits[1] * transform.lossyScale.x * 0.75f);
            Debug.DrawRay(transform.position, halfwayVector * transform.lossyScale.x * 0.75f, Color.blue);
            Debug.DrawRay(transform.position, targetVector * transform.lossyScale.x * 0.75f, Color.red);
        }
    }

    void FixedUpdate()
    {
        if (EM.enemyIsFrozen)
        {
            return;
        }

        if (laserHittingBox == true && Box.isInvulnerable == false)
        {
            Box.activateDamage = true;
            if (Box.onFire)
            {
                Box.damageTaken += (laserDOT - Box.fireDOT) * Time.deltaTime;
            }
            else
            {
                Box.damageTaken += laserDOT * Time.deltaTime;
            }

            if (FindObjectOfType<CameraFollowBox>() != null)
            {
                FindObjectOfType<CameraFollowBox>().StartCameraShake(4, 30);
                //FindObjectOfType<CameraFollowBox>().shakeInfo = new Vector2(10, 30);
            }
        }

        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            shootAngularVelocity *= EM.aggroIncreaseMult;
            normalAngularVelocity *= EM.aggroIncreaseMult;

            shootBoxRadius *= EM.aggroIncreaseMult;
            delayBeforeShooting *= EM.aggroDecreaseMult;
            restTime *= EM.aggroDecreaseMult;

            Mathf.FloorToInt(bulletsPerAttack * EM.aggroIncreaseMult);
            bulletDamage *= EM.aggroIncreaseMult;
            bulletInterval *= EM.aggroDecreaseMult;
            bulletSpeed *= EM.aggroIncreaseMult;
            bulletDespawnTime *= EM.aggroIncreaseMult;

            if (grenadesPerAttack > 1)
            {
                grenadesPerAttack += 1;
            }
            grenadeSpeed *= EM.aggroIncreaseMult;
            grenadeInterval *= EM.aggroDecreaseMult;

            laserDuration *= EM.aggroIncreaseMult;
            laserDOT *= EM.aggroIncreaseMult;
            if (fireLaserBeam)
            {
                Color centerColor = laserCenter.startColor;
                centerColor.g *= 0.8f; centerColor.b *= 0.8f;
                laserCenter.startColor = centerColor; laserCenter.endColor = centerColor;

                Color beamColor = laserBeam.startColor;
                beamColor.b *= 0.5f;
                laserBeam.startColor = beamColor; laserBeam.endColor = beamColor;
            }

        }
        if (EM.aggroCurrentlyActive == false && aggroActive == true)
        {
            aggroActive = false;

            shootAngularVelocity /= EM.aggroIncreaseMult;
            normalAngularVelocity /= EM.aggroIncreaseMult;

            shootBoxRadius /= EM.aggroIncreaseMult;
            delayBeforeShooting /= EM.aggroDecreaseMult;
            restTime /= EM.aggroDecreaseMult;

            Mathf.CeilToInt(bulletsPerAttack / EM.aggroIncreaseMult);
            bulletDamage /= EM.aggroIncreaseMult;
            bulletInterval /= EM.aggroDecreaseMult;
            bulletSpeed /= EM.aggroIncreaseMult;
            bulletDespawnTime /= EM.aggroIncreaseMult;

            if (grenadesPerAttack > 1)
            {
                grenadesPerAttack -= 1;
            }
            grenadeSpeed /= EM.aggroIncreaseMult;
            grenadeInterval /= EM.aggroDecreaseMult;

            laserDuration /= EM.aggroIncreaseMult;
            laserDOT /= EM.aggroIncreaseMult;
            if (fireLaserBeam)
            {
                Color centerColor = laserCenter.startColor;
                centerColor.g /= 0.8f; centerColor.b /= 0.8f;
                laserCenter.startColor = centerColor; laserCenter.endColor = centerColor;

                Color beamColor = laserBeam.startColor;
                beamColor.b /= 0.5f;
                laserBeam.startColor = beamColor; laserBeam.endColor = beamColor;
            }
        }
    }

    IEnumerator ShootBullets()
    {
        int bulletsShot = 0;
        float bulletSpread;
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayBeforeShootingTimer = 0;
        float shootTimeIntervalTimer = 0;
        StartCoroutine(BarrelColor(1, 0.5f, 0f));
        while (delayBeforeShootingTimer <= delayBeforeShooting)
        {
            if (EM.hitstopImpactActive == false)
            {
                delayBeforeShootingTimer += Time.deltaTime;
            }
            yield return null;
        }
        isWaitingToShoot = false;
        while (bulletsShot < bulletsPerAttack && scriptEnabled == true && EM.enemyIsFrozen == false)
        {
            bulletSpread = (-bulletSpreadMax / 2) + Random.value * bulletSpreadMax;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, barrelAngle + bulletSpread);
            newBullet = Instantiate(bullet, enemyRB.position + barrelVector * transform.lossyScale.x * 0.8f, bulletRotation);
            newBullet.GetComponent<BulletScript>().bulletDespawnWindow = bulletDespawnTime;
            newBullet.GetComponent<BulletScript>().bulletDamage = bulletDamage;
            newBullet.GetComponent<Rigidbody2D>().velocity = (barrelVector +
                Vector2.Perpendicular(barrelVector) * Mathf.Sin(bulletSpread * Mathf.PI / 180)).normalized * bulletSpeed;
            if (aggroActive)
            {
                newBullet.GetComponent<BulletScript>().aggro = true;
            }


            bulletsShot++;

            while (shootTimeIntervalTimer <= bulletInterval)
            {
                if (EM.hitstopImpactActive == false)
                {
                    shootTimeIntervalTimer += Time.deltaTime;
                }
                yield return null;
            }
            shootTimeIntervalTimer = 0;
        }
        isOnCooldown = true;
        yield return new WaitForSeconds(restTime);
        isOnCooldown = false;
        isCurrentlyShooting = false;
    }
    IEnumerator ShootGrenades()
    {
        int grenadesShot = 0;
        float grenadeSpread;
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayBeforeShootingTimer = 0;
        float shootTimeIntervalTimer = 0;
        StartCoroutine(BarrelColor(1, 0.6f, 0.2f));
        while (delayBeforeShootingTimer <= delayBeforeShooting)
        {
            if (EM.hitstopImpactActive == false)
            {
                delayBeforeShootingTimer += Time.deltaTime;
            }
            yield return null;
        }
        isWaitingToShoot = false;
        while (grenadesShot < grenadesPerAttack && scriptEnabled == true && EM.enemyIsFrozen == false)
        {
            grenadeSpread = (-grenadeSpreadMax / 2) + Random.value * grenadeSpreadMax;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, barrelAngle + grenadeSpread);
            newGrenade = Instantiate(grenade, enemyRB.position + barrelVector * 1.5f * transform.lossyScale.x /2, bulletRotation);
            newGrenade.GetComponent<Rigidbody2D>().velocity = (barrelVector +
                Vector2.Perpendicular(barrelVector) * Mathf.Sin(grenadeSpread * Mathf.PI / 180)).normalized * grenadeSpeed;
            if (newGrenade.GetComponent<Snowball>() != null)
            {
                newGrenade.GetComponent<Snowball>().aggro = aggroActive;
            }

            grenadesShot++;

            while (shootTimeIntervalTimer <= grenadeInterval && grenadesShot < grenadesPerAttack)
            {
                if (EM.hitstopImpactActive == false)
                {
                    shootTimeIntervalTimer += Time.deltaTime;
                }
                yield return null;
            }
            shootTimeIntervalTimer = 0;
        }
        isOnCooldown = true;
        yield return new WaitForSeconds(restTime);
        isOnCooldown = false;
        isCurrentlyShooting = false;
    }
    IEnumerator LaserBeam()
    {
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayTimer = 0;
        StartCoroutine(BarrelColor(1, 0, 0.5f));
        while (delayTimer <= delayBeforeShooting)
        {
            if (EM.hitstopImpactActive == false)
            {
                delayTimer += Time.deltaTime;
            }
            yield return null;
        }
        isWaitingToShoot = false;

        float laserTimer = 0;
        laserBeam.gameObject.SetActive(true);
        laserCenter.gameObject.SetActive(true);
        while (laserTimer <= laserDuration && EM.hitstopImpactActive == false && EM.enemyIsFrozen == false)
        {
            laserBeam.SetPosition(0, enemyRB.position + barrelVector * transform.localScale.x * 2 / 3);
            laserCenter.SetPosition(0, enemyRB.position + barrelVector * transform.localScale.x * 2 / 3);
            RaycastHit2D laserRC = Physics2D.Raycast(enemyRB.position + barrelVector * transform.localScale.x * 2 / 3, barrelVector, 100,
                LayerMask.GetMask("Obstacles", "Box", "Pulse"));
            if (Box.dodgeInvulActive)
            {
                laserRC = Physics2D.Raycast(enemyRB.position + barrelVector * transform.localScale.x * 2 / 3, barrelVector, 100,
                    LayerMask.GetMask("Obstacles", "Pulse"));
            }
            if (laserRC.collider != null)
            {
                laserBeam.SetPosition(1, laserRC.point + barrelVector * 0.25f);
                if (1 << laserRC.collider.gameObject.layer == boxLM)
                {
                    laserHittingBox = true;
                }
                else
                {
                    laserHittingBox = false;
                }
            }
            else
            {
                laserHittingBox = false;
                laserBeam.SetPosition(1, enemyRB.position + barrelVector * 100);
            }
            if (EM.hitstopImpactActive == false)
            {
                fuel.transform.localRotation = Quaternion.Euler(0, 0, 
                    Mathf.MoveTowardsAngle(fuel.transform.localEulerAngles.z, 130, (130 - 49) / laserDuration * Time.deltaTime));
                laserTimer += Time.deltaTime;
            }
            laserCenter.SetPosition(1, laserBeam.GetPosition(1));
            RaycastHit2D enemyRC = Physics2D.Raycast(laserBeam.GetPosition(0), (laserBeam.GetPosition(1) - laserBeam.GetPosition(0)).normalized,
                (laserBeam.GetPosition(1) - laserBeam.GetPosition(0)).magnitude, LayerMask.GetMask("Enemies", "Enemy Device", "Obstacles", "Box"));
            if (enemyRC.collider != null)
            {
                if (enemyRC.collider.GetComponent<EnemyManager>() != null && enemyRC.collider.GetComponent<EnemyManager>().normalExplosionsWillDamage == true)
                {
                    enemyRC.collider.GetComponent<EnemyManager>().enemyWasDamaged = true;
                }
                if (1 << enemyRC.collider.gameObject.layer == LayerMask.GetMask("Enemy Device"))
                {
                    if (enemyRC.collider.GetComponent<ProximityMine>() != null)
                    {
                        enemyRC.collider.GetComponent<ProximityMine>().remoteExplode = true;
                    }
                    if (enemyRC.collider.GetComponent<Grenade>() != null)
                    {
                        enemyRC.collider.GetComponent<Grenade>().remoteExplode = true;
                    }
                }
            }

            if (laserRC.collider != null && 1 << laserRC.collider.gameObject.layer == obstacleLM)
            {
                Physics2D.queriesHitTriggers = true;
                RaycastHit2D[] fireRC = Physics2D.CircleCastAll(laserRC.point, 0.3f, Vector2.zero, 0, LayerMask.GetMask("Hazards"));
                Physics2D.queriesHitTriggers = false;
                bool spawnFire = true;
                foreach (RaycastHit2D item in fireRC)
                {
                    if (item.collider.GetComponent<Fire>() != null && item.collider.GetComponent<Fire>().hazardFire == true)
                    {
                        spawnFire = false;
                        item.collider.GetComponent<Fire>().fireTime = 0;
                    }
                }
                if (spawnFire)
                {
                    newFire = Instantiate(fire, laserRC.point, Quaternion.identity);
                    newFire.GetComponent<Fire>().surfaceNormal = laserRC.normal;
                    newFire.GetComponent<Fire>().hazardFire = true;
                }
            }

            yield return null;
        }
        float fuelUsed = laserTimer / laserDuration;
        laserHittingBox = false;
        laserBeam.gameObject.SetActive(false);
        laserCenter.gameObject.SetActive(false);

        while (EM.enemyIsFrozen)
        {
            yield return new WaitForFixedUpdate();
        }
        isOnCooldown = true;
        float coolDownTimer = 0;
        while (coolDownTimer <= restTime * fuelUsed)
        {
            if (EM.hitstopImpactActive == false)
            {
                fuel.transform.localRotation = Quaternion.Euler(0, 0,
                    Mathf.MoveTowardsAngle(fuel.transform.localEulerAngles.z, 49, (130 - 49) / restTime * Time.deltaTime));
                coolDownTimer += Time.deltaTime;
            }
            yield return null;
        }
        isOnCooldown = false;
        isCurrentlyShooting = false;
    }
    IEnumerator BarrelColor(float r, float g, float b)
    {
        Color barrelColor = new Color(r, g, b);
        EM.attackActive = true;
        while (isCurrentlyShooting && isOnCooldown == false)
        {
            if (EM.flashOn == false)
            {
                barrel.GetComponent<SpriteRenderer>().color = barrelColor;
            }
            yield return null;
        }
        barrel.GetComponent<SpriteRenderer>().color = initialBarrelColor;
        EM.attackActive = false;
    }
    IEnumerator InitialDelay()
    {
        yield return null;
        SpriteRenderer[] sprites = transform.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer item in sprites)
        {
            item.enabled = true;
        }
    }
}
