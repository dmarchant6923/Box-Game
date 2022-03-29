using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountedTurret : MonoBehaviour
{
    int obstacleLM;
    int obstacleAndBoxLM;
    int boxLM;

    float offsetFromWallMult = 0.15f;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    GameObject barrel;
    Color initialBarrelColor;
    EnemyManager EM;
    bool touchingThisEnemy = false;

    Vector2 initialPosition;

    Vector2 vectorToBox;
    float angleToBox;
    RaycastHit2D enemyRC_RayToBox;
    bool canSeeBox = false;

    float barrelAngle;
    Vector2 barrelVector;
    float realBarrelAngle;
    Vector2 realBarrelVector;
    Vector2[] barrelVectorLimits = new Vector2[2];
    float[] barrelAngleLimits = new float[2];

    float angularVelocity;
    public float shootAngularVelocity = 60;
    public float normalAngularVelocity = 200;

    public bool shootBullets = true;
    public bool shootGrenades = false;
    public bool fireLaserBeam = false;

    RaycastHit2D enemyRC_ShootBox;
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

    bool initialShootDelay = true;

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

        RaycastHit2D rayUp = Physics2D.Raycast(transform.position, Vector2.up, transform.lossyScale.y * 2, obstacleLM);
        RaycastHit2D rayDown = Physics2D.Raycast(transform.position, Vector2.down, transform.lossyScale.y * 2, obstacleLM);
        RaycastHit2D rayLeft = Physics2D.Raycast(transform.position, Vector2.left, transform.lossyScale.y * 2, obstacleLM);
        RaycastHit2D rayRight = Physics2D.Raycast(transform.position, Vector2.right, transform.lossyScale.y * 2, obstacleLM);

        bool foundWall = false;
        if (rayLeft.collider != null)
        {
            enemyRB.position = rayLeft.point + Vector2.right * transform.lossyScale.x * offsetFromWallMult;
            transform.eulerAngles = Vector3.forward * (-90);
            barrelAngle = -90;
            foundWall = true;
        }
        else if (rayRight.collider != null)
        {
            enemyRB.position = rayRight.point + Vector2.left * transform.lossyScale.x * offsetFromWallMult;
            transform.eulerAngles = Vector3.forward * (90);
            barrelAngle = 90;
            foundWall = true;
        }
        else if (rayUp.collider != null)
        {
            enemyRB.position = rayUp.point + Vector2.down * transform.lossyScale.y * offsetFromWallMult;
            transform.eulerAngles = Vector3.forward * (180);
            barrelAngle = 180;
            foundWall = true;
        }
        else if (rayDown.collider != null)
        {
            enemyRB.position = rayDown.point + Vector2.up * transform.lossyScale.y * offsetFromWallMult;
            foundWall = true;
        }
        realBarrelAngle = barrelAngle;

        bool barrelAngleCheck = false;
        float currentBarrelAngle = barrelAngle;
        Vector2 currentBarrelVector = new Vector2(Mathf.Cos(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
        bool foundOne = false;
        float angleTraveled = 0;
        while (barrelAngleCheck == false && foundWall == true)
        {
            RaycastHit2D checkRay = Physics2D.Raycast(enemyRB.position, currentBarrelVector, transform.lossyScale.x, obstacleLM);
            if (checkRay.collider == null && foundOne == false)
            {
                currentBarrelAngle += 2;
                currentBarrelVector = new Vector2(Mathf.Cos(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                angleTraveled += 2;
                continue;
            }
            if (checkRay.collider != null && foundOne == false)
            {
                currentBarrelAngle -= 2;
                currentBarrelVector = new Vector2(Mathf.Cos(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                barrelAngleLimits[0] = currentBarrelAngle;
                barrelVectorLimits[0] = currentBarrelVector;
                foundOne = true;
                currentBarrelAngle -= (angleTraveled - 2);
                continue;
            }
            if (checkRay.collider == null && foundOne == true)
            {
                currentBarrelAngle -= 2;
                currentBarrelVector = new Vector2(Mathf.Cos(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                continue;
            }
            if (checkRay.collider != null && foundOne == true)
            {
                currentBarrelAngle += 2;
                currentBarrelVector = new Vector2(Mathf.Cos(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(currentBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                barrelAngleLimits[1] = currentBarrelAngle;
                barrelVectorLimits[1] = currentBarrelVector;
                barrelAngleCheck = true;
            }
        }

        bulletDespawnTime = shootBoxRadius * 2 / bulletSpeed;
        EM.normalPulse = false;
        EM.reflectedBulletsWillDamage = false;
        EM.keepAsKinematic = true;

        if (shootBullets) { shootGrenades = false; fireLaserBeam = false; }
        if (shootGrenades) { fireLaserBeam = false; }

        if (fireLaserBeam)
        {
            laserBeam = transform.GetChild(2).gameObject.GetComponent<LineRenderer>();
            laserCenter = transform.GetChild(3).gameObject.GetComponent<LineRenderer>();
        }

        StartCoroutine(InitialDelay());
    }

    void Update()
    {
        //disable script when enemy is killed (not currently working properly)
        scriptEnabled = EM.scriptsEnabled;
        if (scriptEnabled == false)
        {
            StopAllCoroutines();
            if (fireLaserBeam)
            {
                laserBeam.enabled = false;
                laserCenter.enabled = false;
            }
            gameObject.GetComponent<MountedTurret>().enabled = false;
        }

        //barrelVector
        vectorToBox = (boxRB.position - enemyRB.position).normalized;
        //barrelAngle
        angleToBox = -Mathf.Atan2(vectorToBox.x, vectorToBox.y) * Mathf.Rad2Deg;
        enemyRC_RayToBox = Physics2D.Raycast(enemyRB.position, vectorToBox, shootBoxRadius, obstacleAndBoxLM);
        enemyRC_ShootBox = Physics2D.CircleCast(enemyRB.position, shootBoxRadius * 1.05f, new Vector2(0, 0), 0f, boxLM);

        //inBoxLOS, and keeping the barrel angle still once inBoxLOS = false
        if (enemyRC_RayToBox.collider != null && 1 << enemyRC_RayToBox.collider.gameObject.layer == boxLM)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            if (shootBullets || fireLaserBeam)
            {
                barrelAngle = angleToBox;
            }
            else if (shootGrenades)
            {
                grenadeGravity = grenade.GetComponent<Rigidbody2D>().gravityScale;
                Vector2 distanceVector = boxRB.position - (enemyRB.position + realBarrelVector * 1.5f * transform.lossyScale.x / 2);
                float g = -Physics.gravity.y * grenadeGravity;
                float deltaX = distanceVector.x;
                float deltaY = distanceVector.y;

                float v = grenadeSpeed * 1.2f;
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

                    barrelAngle = -Mathf.Atan2(vx, vy) * Mathf.Rad2Deg;
                    barrelVector = new Vector2(Mathf.Cos(barrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                        Mathf.Sin(barrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
                    RaycastHit2D detectObstacle = Physics2D.Raycast(transform.position, barrelVector, 5, obstacleLM);

                    if (detectObstacle.collider != null)
                    {
                        barrelAngle = angleToBox;
                    }
                }
                else
                {
                    if (Mathf.Abs(barrelAngle) >= 45)
                    {
                        barrelAngle = 45 * -Mathf.Sign(deltaX);
                    }
                    else
                    {
                        barrelAngle = angleToBox;
                    }
                }
            }
            barrelVector = new Vector2(Mathf.Cos(barrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(barrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));
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

        //making sure it doesn't turn into the wall it's attached to (not currently working properly)
        if (EM.hitstopImpactActive == false)
        {
            if (Vector2.Dot(realBarrelVector, barrelVector) >= 0)
            {
                realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngle, angularVelocity * Time.deltaTime);
            }
            else
            {
                if (Vector2.Dot(realBarrelVector, barrelVectorLimits[0]) < 0 && Vector2.Dot(realBarrelVector, barrelVectorLimits[1]) < 0)
                {
                    realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngle, angularVelocity * Time.deltaTime);
                }
                else if (Vector2.Dot(realBarrelVector, barrelVectorLimits[0]) >= 0)
                {
                    if (Vector2.Dot(barrelVector, barrelVectorLimits[1]) < 0)
                    {
                        realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngle, angularVelocity * Time.deltaTime);
                    }
                    else
                    {
                        realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[0] + 180, angularVelocity * Time.deltaTime);
                    }
                }
                else if (Vector2.Dot(realBarrelVector, barrelVectorLimits[1]) >= 0)
                {
                    if (Vector2.Dot(barrelVector, barrelVectorLimits[0]) < 0)
                    {
                        realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngle, angularVelocity * Time.deltaTime);
                    }
                    else
                    {
                        realBarrelAngle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, barrelAngleLimits[1] + 180, angularVelocity * Time.deltaTime);
                    }
                }
            }
        }

        realBarrelVector = new Vector2(Mathf.Cos(realBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin(realBarrelAngle * Mathf.Deg2Rad + Mathf.PI / 2));

        transform.eulerAngles = Vector3.forward * (realBarrelAngle);


        if (enemyRC_ShootBox.collider != null && canSeeBox == true && isCurrentlyShooting == false && initialShootDelay == false)
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

        //debug lines
        if (debugLines)
        {
            Debug.DrawRay(initialPosition, Vector2.up * transform.lossyScale.y * 2);
            Debug.DrawRay(initialPosition, Vector2.down * transform.lossyScale.y * 2);
            Debug.DrawRay(initialPosition, Vector2.left * transform.lossyScale.y * 2);
            Debug.DrawRay(initialPosition, Vector2.right * transform.lossyScale.y * 2);

            Debug.DrawRay(enemyRB.position, vectorToBox * shootBoxRadius, Color.red);

            Debug.DrawRay(transform.position, barrelVectorLimits[0] * 1.5f);
            Debug.DrawRay(transform.position, barrelVectorLimits[1] * 1.5f);
        }
    }

    void FixedUpdate()
    {
        if (laserHittingBox == true)
        {
            Box.activateDamage = true;
            Box.damageTaken += laserDOT * Time.deltaTime;
        }

        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            shootAngularVelocity *= EM.aggroIncreaseMult;
            normalAngularVelocity *= EM.aggroIncreaseMult;

            shootBoxRadius *= EM.aggroIncreaseMult;
            delayBeforeShooting *= EM.aggroDecreaseMult;
            restTime *= EM.aggroDecreaseMult;

            bulletsPerAttack += 2;
            bulletDamage *= EM.aggroIncreaseMult;
            bulletInterval *= EM.aggroDecreaseMult;
            bulletSpeed *= EM.aggroIncreaseMult;
            bulletDespawnTime *= EM.aggroIncreaseMult;

            grenadesPerAttack += 1;
            grenadeSpeed *= EM.aggroIncreaseMult;
            grenadeInterval *= EM.aggroDecreaseMult;

            laserDuration *= EM.aggroIncreaseMult;
            laserDOT *= EM.aggroIncreaseMult;

        }
        if (EM.aggroCurrentlyActive == false && aggroActive == true)
        {
            aggroActive = false;

            shootAngularVelocity /= EM.aggroIncreaseMult;
            normalAngularVelocity /= EM.aggroIncreaseMult;

            shootBoxRadius /= EM.aggroIncreaseMult;
            delayBeforeShooting /= EM.aggroDecreaseMult;
            restTime /= EM.aggroDecreaseMult;

            bulletsPerAttack -= 2;
            bulletDamage /= EM.aggroIncreaseMult;
            bulletInterval /= EM.aggroDecreaseMult;
            bulletSpeed /= EM.aggroIncreaseMult;
            bulletDespawnTime /= EM.aggroIncreaseMult;

            grenadesPerAttack -= 1;
            grenadeSpeed /= EM.aggroIncreaseMult;
            grenadeInterval /= EM.aggroDecreaseMult;

            laserDuration /= EM.aggroIncreaseMult;
            laserDOT /= EM.aggroIncreaseMult;
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
        while (bulletsShot < bulletsPerAttack && scriptEnabled == true)
        {
            bulletSpread = (-bulletSpreadMax / 2) + Random.value * bulletSpreadMax;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, realBarrelAngle + bulletSpread);
            newBullet = Instantiate(bullet, enemyRB.position + realBarrelVector * 0.8f, bulletRotation);
            newBullet.GetComponent<BulletScript>().bulletDespawnWindow = bulletDespawnTime;
            newBullet.GetComponent<BulletScript>().bulletDamage = bulletDamage;
            newBullet.GetComponent<Rigidbody2D>().velocity = (realBarrelVector +
                Vector2.Perpendicular(realBarrelVector) * Mathf.Sin(bulletSpread * Mathf.PI / 180)).normalized * bulletSpeed;
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
        while (grenadesShot < grenadesPerAttack && scriptEnabled == true)
        {
            grenadeSpread = (-grenadeSpreadMax / 2) + Random.value * grenadeSpreadMax;
            Quaternion bulletRotation = Quaternion.Euler(0, 0, realBarrelAngle + grenadeSpread);
            newGrenade = Instantiate(grenade, enemyRB.position + realBarrelVector * 1.5f * transform.lossyScale.x /2, bulletRotation);
            newGrenade.GetComponent<Rigidbody2D>().velocity = (realBarrelVector +
                Vector2.Perpendicular(realBarrelVector) * Mathf.Sin(grenadeSpread * Mathf.PI / 180)).normalized * grenadeSpeed;

            grenadesShot++;

            while (shootTimeIntervalTimer <= grenadeInterval)
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
        while (laserTimer <= laserDuration && EM.enemyWasDamaged == false)
        {
            laserBeam.SetPosition(0, enemyRB.position + realBarrelVector * transform.localScale.x * 2 / 3);
            laserCenter.SetPosition(0, enemyRB.position + realBarrelVector * transform.localScale.x * 2 / 3);
            RaycastHit2D laserRC = Physics2D.Raycast(enemyRB.position + realBarrelVector * transform.localScale.x * 2 / 3, realBarrelVector, 100,
                LayerMask.GetMask("Obstacles", "Box", "Pulse"));
            if (laserRC.collider != null)
            {
                if (1 << laserRC.collider.gameObject.layer == LayerMask.GetMask("Pulse"))
                {
                    Vector2 vectorToCenter = new Vector2(laserRC.collider.transform.position.x - laserRC.point.x, laserRC.collider.transform.position.y - laserRC.point.y);
                    RaycastHit2D pulseToRay = Physics2D.Raycast(laserRC.point, vectorToCenter.normalized, vectorToCenter.magnitude, obstacleLM);
                    if (pulseToRay.collider == null)
                    {
                        laserBeam.SetPosition(1, laserRC.point + realBarrelVector * 0.25f);
                    }
                    else
                    {
                        RaycastHit2D continueLaser = Physics2D.Raycast(laserRC.point, realBarrelVector, 100, LayerMask.GetMask("Obstacles", "Box", "Hazards"));
                        if (continueLaser.collider != null)
                        {
                            laserBeam.SetPosition(1, continueLaser.point + realBarrelVector * 0.25f);
                        }
                        else
                        {
                            laserBeam.SetPosition(1, enemyRB.position + realBarrelVector * 100);
                        }
                    }
                    laserHittingBox = false;
                }
                else
                {
                    laserBeam.SetPosition(1, laserRC.point + realBarrelVector * 0.25f);
                    if (1 << laserRC.collider.gameObject.layer == boxLM)
                    {
                        laserHittingBox = true;
                    }
                    else
                    {
                        laserHittingBox = false;
                    }
                }
            }
            else
            {
                laserHittingBox = false;
                laserBeam.SetPosition(1, enemyRB.position + realBarrelVector * 100);
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

            yield return null;
        }
        laserHittingBox = false;
        laserBeam.gameObject.SetActive(false);
        laserCenter.gameObject.SetActive(false);

        isOnCooldown = true;
        float coolDownTimer = 0;
        while (coolDownTimer <= restTime)
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
        yield return new WaitForSeconds(1f);
        initialShootDelay = false;
    }
}
