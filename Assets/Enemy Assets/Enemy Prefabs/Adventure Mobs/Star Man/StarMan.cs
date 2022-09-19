using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarMan : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;

    Transform starman;
    Transform mouth;
    Vector2 initialMouthScale;

    Vector2 initialPosition;
    int forceDirectionY;
    int forceDirectionX;
    float floatVelocity = 4;
    float forceMagnitudeY = 5f;
    float forceMagnitudeX = 7f;
    float spinMultiplier = 100;
    float truePositionX;
    int truePositionXDirection;
    float truePositionXDisplacement = 20;
    float truePositionXVelocity = 1.5f;
    float trueAngularVelocity;
    bool isStationary = false;

    public float sightRadius = 16;

    bool touchingThisEnemy = false;
    Vector2 vectorToBox;
    Vector2 lastVectorToBox = Vector2.up;
    Vector2 realVector;
    float realAngle;
    float initialMouthRotateSpeed = 400;
    float mouthRotateSpeed = 400;
    bool canSeeBox = false;
    bool idleMouthCRActive = false;

    bool shootCRActive = false;
    public GameObject starBullet;
    GameObject newStarBullet;
    public float bulletSpeed = 15;
    public float bulletSpreadMax = 17;
    public int bulletsPerAttack = 10;
    public float delayBeforeShooting = 0.7f;
    float bulletDespawnTime;
    public float shootTimeInterval = 0.04f;
    public float bulletRecoil = 5;
    float initialbulletRecoil;
    public float bulletDamage = 20;
    public float restTime = 3f;
    bool isAttacking = false;
    bool isCurrentlyShooting = false;
    bool isWaitingToShoot = false;
    bool isOnCoolDown = false;

    public float totalBirthTime = 12;
    float timeToReproduce = 8;
    float birthDelay = 6;
    Vector2 minSize;
    float sizeMult = 2.8f;
    float growthTimer = 0;

    bool birthCRActive = false;
    Color initialColor;
    Color currentColor;
    bool givingBirth = false;
    public GameObject starManProjectile;
    GameObject newStarManProjectile;
    bool gaveBirth = false;
    float projectileExplosionRadius = 2.5f;
    float projectileVelocity = 25;
    Color postBirthColor;

    public bool debugEnabled = false;

    bool aggroActive = false;

    int obstacleAndBoxLM;
    int boxLM;
    int obstacleLM;

    [HideInInspector] public int iteration = 1;
    public int maxIterations = 4;
    
    void Start()
    {
        EM = GetComponent<EnemyManager>();
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();


        EM.pulseMultiplier = 0.55f;
        mouth = transform.GetChild(0);
        bulletDespawnTime = sightRadius * 3 / bulletSpeed;
        initialbulletRecoil = bulletRecoil;

        initialMouthScale = mouth.localScale;

        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        boxLM = LayerMask.GetMask("Box");
        obstacleLM = LayerMask.GetMask("Obstacles");

        initialPosition = enemyRB.position;
        float castRadius = 1.5f;
        float castDistance = 1f;
        RaycastHit2D circleCast = Physics2D.CircleCast(enemyRB.position, castRadius, Vector2.zero, 0, obstacleLM);
        if (circleCast.collider != null) 
        {
            bool foundPosition = false;
            for (float angle = 0; angle < 360; angle += 90)
            {
                Vector2 vector = Tools.AngleToVector(angle) * castDistance;
                circleCast = Physics2D.CircleCast(enemyRB.position + vector, castRadius, Vector2.zero, 0, obstacleLM);
                if (circleCast.collider == null)
                {
                    initialPosition = enemyRB.position + vector;
                    foundPosition = true;
                    break;
                }
            }
            if (foundPosition == false)
            {
                for (float angle = 0; angle < 360; angle += 30)
                {
                    Vector2 vector = Tools.AngleToVector(angle) * castDistance;
                    circleCast = Physics2D.CircleCast(enemyRB.position + vector, castRadius, Vector2.zero, 0, obstacleLM);
                    if (circleCast.collider == null)
                    {
                        initialPosition = enemyRB.position + vector;
                        break;
                    }
                }
            }
        }
        RaycastHit2D boxCastLeft = Physics2D.BoxCast(initialPosition, new Vector2(0.25f, 0.75f), 0, Vector2.left, 4f, obstacleLM);
        RaycastHit2D boxCastRight = Physics2D.BoxCast(initialPosition, new Vector2(0.25f, 0.75f), 0, Vector2.right, 4f, obstacleLM);
        if (boxCastLeft.collider != null && boxCastRight.collider != null)
        {
            isStationary = true;
            initialPosition = new Vector2(initialPosition.x + (boxCastRight.distance - boxCastLeft.distance) / 2, initialPosition.y);
        }

        truePositionX = initialPosition.x;

        enemyRB.velocity = Vector2.up * 3;
        truePositionXDirection = Random.Range(0, 2);
        if (truePositionXDirection == 0) { truePositionXDirection = -1; }
        realVector = new Vector2(truePositionXDirection, -0.25f);
        lastVectorToBox = realVector;

        minSize = transform.localScale;
        initialColor = GetComponent<SpriteRenderer>().color;
        postBirthColor = new Color(1, 1, 0.6f);

        timeToReproduce = totalBirthTime * 0.6f;
        birthDelay = totalBirthTime * 0.4f;

        if (iteration >= maxIterations)
        {
            gaveBirth = true;
            transform.localScale = minSize * (1 + (sizeMult - 1) * 0.3f);
            GetComponent<SpriteRenderer>().color = postBirthColor;
            for (int i = 0; i < EM.enemyObjects.Count; i++)
            {
                if (EM.enemyObjects[i] == GetComponent<SpriteRenderer>())
                {
                    EM.enemyColors[i] = postBirthColor;
                }
            }
        }

        StartCoroutine(StarParticles());
        StartCoroutine(InitialDrag());
    }
    private void FixedUpdate()
    {
        if (EM.enemyIsFrozen)
        {
            return;
        }

        //floating logic Y
        forceDirectionY = (int)-Mathf.Sign(enemyRB.position.y - initialPosition.y);
        if ((forceDirectionY == -1 && enemyRB.velocity.y >= -floatVelocity) || (forceDirectionY == 1 && enemyRB.velocity.y <= floatVelocity))
        {
            enemyRB.AddForce(Vector2.up * forceDirectionY * forceMagnitudeY);
        }
        if (enemyRB.position.y > initialPosition.y && enemyRB.position.y + enemyRB.velocity.y * Time.deltaTime * 2 < initialPosition.y
            && enemyRB.velocity.y > -2.5f)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x, -floatVelocity / 2);
        }

        //floating logic X
        if (Mathf.Abs(enemyRB.position.x - truePositionX) <= 1) { forceDirectionX = 0; }
        else { forceDirectionX = (int)-Mathf.Sign(enemyRB.position.x - truePositionX); }

        float distThreshold = (isStationary) ? 0.1f : 3;
        if (Mathf.Abs(enemyRB.position.x - truePositionX) >= distThreshold)
        {
            if ((forceDirectionX == -1 && enemyRB.velocity.x >= -floatVelocity) || (forceDirectionX == 1 && enemyRB.velocity.x <= floatVelocity))
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * forceMagnitudeX);
            }
        }
        else
        {
            if ((forceDirectionX == -1 && enemyRB.velocity.x >= truePositionXVelocity) || (forceDirectionX == 1 && enemyRB.velocity.x <= truePositionXVelocity))
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * forceMagnitudeX / 4);
            }
        }

        //moving truePositionX
        if (isStationary == false)
        {
            truePositionX += truePositionXVelocity * truePositionXDirection * Time.fixedDeltaTime;
            if (truePositionX >= initialPosition.x + truePositionXDisplacement && truePositionXDirection == 1)
            {
                truePositionXDirection = -1;
            }
            if (truePositionX <= initialPosition.x - truePositionXDisplacement && truePositionXDirection == -1)
            {
                truePositionXDirection = 1;
            }

            //changing truePositionXDirection if an obstacle is detected, turning around if so
            RaycastHit2D obstacleCheckRC = Physics2D.BoxCast(new Vector2(truePositionX + 2f * truePositionXDirection,
                initialPosition.y), new Vector2(0.25f, 1), 0, Vector2.down, 0f, obstacleLM);
            if (obstacleCheckRC.collider != null)
            {
                truePositionXDirection *= -1;
            }
        }

        //magical spinning
        float baseSpin = (givingBirth || (shootCRActive && isOnCoolDown == false)) ? 0 : 100;
        trueAngularVelocity = Mathf.Sign(-enemyRB.velocity.x) * baseSpin + (-Mathf.Sqrt(Mathf.Abs(enemyRB.velocity.x)) * Mathf.Sign(enemyRB.velocity.x) * spinMultiplier);
        float spinAccel = 1000;
        enemyRB.angularVelocity = Mathf.MoveTowards(enemyRB.angularVelocity, trueAngularVelocity, spinAccel * Time.fixedDeltaTime);

        //mouth movement
        if (canSeeBox && EM.initialDelay == false)
        {
            lastVectorToBox = vectorToBox;
            if (givingBirth == false)
            {
                realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(lastVectorToBox), mouthRotateSpeed * Time.fixedDeltaTime);
            }
        }
        else if (idleMouthCRActive == false && shootCRActive == false && givingBirth == false)
        {
            StartCoroutine(IdleMouth());
        }
        realVector = Tools.AngleToVector(realAngle);
        mouth.eulerAngles = new Vector3(0, 0, realAngle);

        if (EM.initialDelay == false && gaveBirth == false)
        {
            growthTimer += Time.fixedDeltaTime;
            float currentScale = Mathf.Min(1 + (growthTimer / timeToReproduce) * (sizeMult - 1), sizeMult);
            transform.localScale = minSize * currentScale;
            if (growthTimer > timeToReproduce && birthCRActive == false)
            {
                StartCoroutine(Birth());
            }
        }

        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            sightRadius *= EM.aggroIncreaseMult;

            bulletSpeed *= EM.aggroIncreaseMult;
            bulletDamage *= EM.aggroIncreaseMult;
            bulletsPerAttack = Mathf.FloorToInt(bulletsPerAttack * EM.aggroIncreaseMult);
            bulletDespawnTime *= EM.aggroIncreaseMult;

            shootTimeInterval *= EM.aggroDecreaseMult;
            delayBeforeShooting *= EM.aggroDecreaseMult;
            restTime *= EM.aggroDecreaseMult;

            projectileExplosionRadius *= EM.aggroIncreaseMult;
            projectileVelocity *= EM.aggroIncreaseMult;
            timeToReproduce *= EM.aggroDecreaseMult;
            birthDelay *= EM.aggroDecreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive == true)
        {
            aggroActive = false;

            sightRadius /= EM.aggroIncreaseMult;

            bulletSpeed /= EM.aggroIncreaseMult;
            bulletDamage /= EM.aggroIncreaseMult;
            bulletsPerAttack = Mathf.CeilToInt(bulletsPerAttack / EM.aggroIncreaseMult);
            bulletDespawnTime /= EM.aggroIncreaseMult;

            shootTimeInterval /= EM.aggroDecreaseMult;
            delayBeforeShooting /= EM.aggroDecreaseMult;
            restTime /= EM.aggroDecreaseMult;

            projectileExplosionRadius /= EM.aggroIncreaseMult;
            projectileVelocity /= EM.aggroIncreaseMult;
            timeToReproduce /= EM.aggroDecreaseMult;
            birthDelay /= EM.aggroDecreaseMult;

        }


        //debug lines
        if (debugEnabled)
        {
            Debug.DrawRay(new Vector2(truePositionX - 1, initialPosition.y), Vector2.right * 2, Color.green);
        }
    }
    void Update()
    {
        //logic for player to physically hit enemy
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

        if (touchingThisEnemy && EM.enemyIsInvulnerable == false && Box.boxHitboxActive)
        {
            EM.enemyWasDamaged = true;
            Box.activateHitstop = true;
        }

        if (EM.enemyIsFrozen)
        {
            return;
        }

        vectorToBox = boxRB.position - enemyRB.position;
        if (Tools.LineOfSight(enemyRB.position, vectorToBox))
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            if (shootCRActive == false && EM.initialDelay == false && givingBirth == false)
            {
                StartCoroutine(ShootBullets());
            }
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }
    }

    IEnumerator ShootBullets()
    {
        shootCRActive = true;

        forceMagnitudeY /= 4;
        truePositionXVelocity /= 4;
        forceMagnitudeX /= 4;
        enemyRB.velocity /= 4;
        enemyRB.angularVelocity /= 10;
        floatVelocity /= 4;
        enemyRB.drag = 1;

        float gValue = 0.8f;
        foreach (SpriteRenderer sprite in mouth.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sprite.color;
            color.g += gValue;
            sprite.color = color;
        }

        mouthRotateSpeed = initialMouthRotateSpeed * 2;
        int bulletsShot = 0;
        float bulletSpread;
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayBeforeShootingTimer = 0;
        while (delayBeforeShootingTimer <= delayBeforeShooting && EM.enemyWasKilled == false && EM.initialDelay == false)
        {
            if (EM.hitstopImpactActive == false)
            {
                delayBeforeShootingTimer += Time.fixedDeltaTime;
            }
            if (delayBeforeShootingTimer > delayBeforeShooting * 0.75f)
            {
                mouthRotateSpeed = 0;
            }

            mouth.localScale = Vector2.MoveTowards(mouth.localScale, new Vector2(initialMouthScale.x, initialMouthScale.y * 0.6f), Time.fixedDeltaTime);

            yield return new WaitForFixedUpdate();
        }
        isWaitingToShoot = false;
        float shootTimeIntervalTimer = 0;
        while (bulletsShot < bulletsPerAttack && EM.enemyWasKilled == false && EM.initialDelay == false)
        {
            mouth.localScale = new Vector2(mouth.localScale.x, initialMouthScale.y * 1.7f);

            if (bulletsShot == 1)
            {
                bulletRecoil = initialbulletRecoil / 100;
                enemyRB.velocity = new Vector2(enemyRB.velocity.x / 2, enemyRB.velocity.y);
            }

            if (shootTimeInterval == 0 && bulletsPerAttack > 1) //only for shotguns
            {
                bulletSpread = (-bulletSpreadMax / 2) + (bulletSpreadMax * ((float)bulletsShot / (bulletsPerAttack - 1)));
                bulletSpread += (-0.5f + Random.value) * (bulletSpreadMax * (1f / (bulletsPerAttack - 1)));
            }
            else
            {
                bulletSpread = (-bulletSpreadMax / 2) + Random.value * bulletSpreadMax;
            }
            Vector2 bulletDirection = (realVector + Vector2.Perpendicular(realVector) * Mathf.Sin(bulletSpread * Mathf.PI / 180)).normalized;
            float distTraveled = bulletSpeed * shootTimeIntervalTimer;
            newStarBullet = Instantiate(starBullet, enemyRB.position + (bulletDirection * transform.localScale.x * 0.6f) + (bulletDirection * distTraveled), Quaternion.identity);
            newStarBullet.GetComponent<BulletScript>().bulletDespawnWindow = bulletDespawnTime;
            newStarBullet.GetComponent<BulletScript>().bulletDamage = bulletDamage;
            newStarBullet.GetComponent<BulletScript>().bulletCosmetic = true;
            newStarBullet.GetComponent<BulletScript>().aggro = aggroActive;
            newStarBullet.GetComponent<Rigidbody2D>().velocity = bulletDirection * bulletSpeed;

            bulletsShot++;
            enemyRB.AddForce(bulletRecoil * -realVector, ForceMode2D.Impulse);
            while (shootTimeIntervalTimer <= shootTimeInterval && shootTimeInterval != 0)
            {
                if (EM.hitstopImpactActive == false)
                {
                    shootTimeIntervalTimer += Time.fixedDeltaTime;
                }

                yield return new WaitForFixedUpdate();
                mouth.localScale = Vector2.MoveTowards(mouth.localScale, new Vector2(mouth.localScale.x, initialMouthScale.y * 0.8f), Time.deltaTime * 20);
            }
            shootTimeIntervalTimer -= shootTimeInterval;
        }

        foreach (SpriteRenderer sprite in mouth.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sprite.color;
            color.g -= gValue;
            sprite.color = color;
        }

        yield return new WaitForSeconds(restTime * 0.2f);
        isCurrentlyShooting = false;
        isOnCoolDown = true;

        bulletRecoil = initialbulletRecoil;
        forceMagnitudeY *= 4;
        truePositionXVelocity *= 4;
        forceMagnitudeX *= 4;
        floatVelocity *= 4;
        enemyRB.drag = 0;
        mouthRotateSpeed = initialMouthRotateSpeed;

        float window = restTime * 0.8f;
        float timer = 0;
        while (timer < window && EM.initialDelay == false)
        {
            timer += Time.fixedDeltaTime;
            mouth.localScale = Vector2.MoveTowards(mouth.localScale, new Vector2(initialMouthScale.x, initialMouthScale.y), Time.deltaTime * 3);
            yield return new WaitForFixedUpdate();
        }
        mouth.localScale = initialMouthScale;
        isOnCoolDown = false;
        shootCRActive = false;
    }
    IEnumerator StarParticles()
    {
        float interval = 0.2f;

        while (gaveBirth == false)
        {
            float randInterval = interval * 0.5f + interval * Random.Range(0f, 1f);
            yield return new WaitForSeconds(randInterval);
            while (EM.initialDelay)
            {
                yield return new WaitForFixedUpdate();
            }
            newStarBullet = Instantiate(starBullet, enemyRB.position + Random.insideUnitCircle * transform.localScale.x / 2, Quaternion.identity);
            newStarBullet.GetComponent<BulletScript>().bulletDespawnWindow = 5;
            newStarBullet.GetComponent<BulletScript>().bulletDamage = 0;
            newStarBullet.GetComponent<Rigidbody2D>().velocity = Vector2.down * 5;
            newStarBullet.GetComponent<StarBullet>().aestheticBullet = true;
        }
    }
    IEnumerator IdleMouth()
    {
        idleMouthCRActive = true;
        float delay = 1.5f;
        float timer = 0;
        while (timer < delay && shootCRActive == false && givingBirth == false)
        {
            realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(lastVectorToBox), mouthRotateSpeed * Time.deltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Vector2 vector = new Vector2(Mathf.Sign(enemyRB.velocity.x), -0.25f).normalized;
        while (shootCRActive == false && givingBirth == false && EM.enemyIsFrozen == false)
        {
            if (Mathf.Abs(enemyRB.velocity.x) > 0.2f)
            {
                vector = new Vector2(Mathf.Sign(enemyRB.velocity.x), -0.25f).normalized;
            }
            float angle = Tools.VectorToAngle(vector);
            realAngle = Mathf.MoveTowardsAngle(realAngle, angle, mouthRotateSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }
        idleMouthCRActive = false;

    }
    IEnumerator Birth()
    {
        birthCRActive = true;
        float timer = 0;
        float initialG = initialColor.g;
        float targetG = 0.4f;
        float period = 0.5f;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color color = initialColor;
        while (timer < birthDelay || (shootCRActive && isOnCoolDown == false))
        {
            if (timer > birthDelay * 0.6f)
            {
                period = 0.2f;
            }

            while ((timer < birthDelay || (shootCRActive && isOnCoolDown == false)) && color.g < initialG)
            {
                float deltaTime = Time.fixedDeltaTime;
                if (EM.enemyIsFrozen)
                {
                    deltaTime = 0;
                }
                color.g = Mathf.MoveTowards(color.g, initialG, (initialColor.g - targetG) / period * 2 * deltaTime);
                if (EM.flashOn == false)
                {
                    sprite.color = color;
                }
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            color.g = initialG;
            sprite.color = color;

            while ((timer < birthDelay || (shootCRActive && isOnCoolDown == false)) && color.g > targetG)
            {
                float deltaTime = Time.fixedDeltaTime;
                if (EM.enemyIsFrozen)
                {
                    deltaTime = 0;
                }
                color.g = Mathf.MoveTowards(color.g, targetG, (initialColor.g - targetG) / period * 2 * deltaTime);
                if (EM.flashOn == false)
                {
                    sprite.color = color;
                }
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            color.g = targetG;
            sprite.color = color;
        }
        color.g = targetG;
        sprite.color = color;

        yield return new WaitForFixedUpdate();

        givingBirth = true;
        StartCoroutine(BirthShake());

        forceMagnitudeY /= 20;
        truePositionXVelocity /= 20;
        forceMagnitudeX /= 20;
        enemyRB.velocity /= 20;
        floatVelocity /= 20;
        enemyRB.drag = 5;

        float delay = 1.5f;
        timer = 0;
        float distance = 35;
        float maxDistance = 0;
        Vector2 chosenVector = Vector2.up;
        int groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        //int rand = Random.Range(0, 4);
        //for (int angle = rand * 90; angle < (rand + 1) * 90; angle += 5)
        for (int angle = 0; angle < 360; angle += 5)
        {
            Vector2 vector = Tools.AngleToVector(angle);
            RaycastHit2D ray = Physics2D.CircleCast(enemyRB.position, 0.5f, vector, distance, obstacleLM);
            if (vector.y < 0)
            {
                ray = Physics2D.CircleCast(enemyRB.position, 0.5f, vector, distance, groundLM);
            }
            if (ray.collider != null && ray.distance > maxDistance)
            {
                int rand = Random.Range(0, 7);
                if (rand == 0)
                {
                    maxDistance = ray.distance;
                    chosenVector = vector;
                }
            }
        }
        while (timer < delay)
        {
            if (canSeeBox == false)
            {
                realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(chosenVector), mouthRotateSpeed * Time.fixedDeltaTime);
            }
            else
            {
                realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(lastVectorToBox), mouthRotateSpeed * Time.fixedDeltaTime);
                chosenVector = lastVectorToBox;
            }
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.fixedDeltaTime;
            }
            if (EM.initialDelay)
            {
                timer = 0;
            }
            if (EM.flashOn == false)
            {
                sprite.color = color;
            }
            yield return new WaitForFixedUpdate();
        }
        if (EM.enemyWasKilled)
        {
            yield return new WaitForSeconds(1000);
        }

        newStarManProjectile = Instantiate(starManProjectile, enemyRB.position + realVector * transform.localScale.x * 0.4f, Quaternion.identity);
        newStarManProjectile.GetComponent<StarManProjectile>().debugEnabled = debugEnabled;
        newStarManProjectile.GetComponent<StarManProjectile>().explosionRadius = projectileExplosionRadius;
        newStarManProjectile.GetComponent<StarManProjectile>().iteration = iteration + 1;
        newStarManProjectile.GetComponent<Rigidbody2D>().velocity = realVector.normalized * projectileVelocity;
        if (aggroActive)
        {
            Color projectileColor = newStarManProjectile.GetComponent<SpriteRenderer>().color;
            projectileColor.g -= 0.5f;
            newStarManProjectile.GetComponent<SpriteRenderer>().color = projectileColor;
        }
        enemyRB.drag = 1;
        enemyRB.AddForce(20 * -realVector, ForceMode2D.Impulse);
        gaveBirth = true;

        float currentScale = sizeMult;
        while (currentScale > 1 + (sizeMult - 1) * 0.3f)
        {
            if (EM.enemyIsFrozen == false)
            {
                currentScale -= Time.fixedDeltaTime;
            }
            transform.localScale = minSize * currentScale;

            color = Color.Lerp(color, postBirthColor, Time.fixedDeltaTime * 1.7f);
            if (EM.flashOn == false)
            {
                sprite.color = color;
            }

            yield return new WaitForFixedUpdate();
        }
        sprite.color = postBirthColor;
        for (int i = 0; i < EM.enemyObjects.Count; i++)
        {
            if (EM.enemyObjects[i] == GetComponent<SpriteRenderer>())
            {
                EM.enemyColors[i] = postBirthColor;
            }
        }
        birthCRActive = false;
        givingBirth = false;

        transform.localScale = minSize * (1 + (sizeMult - 1) * 0.3f);

        forceMagnitudeY *= 20;
        truePositionXVelocity *= 20;
        forceMagnitudeX *= 20;
        floatVelocity *= 20;
        enemyRB.drag = 0;
    }
    IEnumerator BirthShake()
    {
        float distance = 0.02f;
        while (gaveBirth == false)
        {
            while (EM.initialDelay)
            {
                distance = 0.02f;
                yield return new WaitForFixedUpdate();
            }
            mouth.localScale = Vector2.MoveTowards(mouth.localScale, new Vector2(initialMouthScale.x * 1.5f, initialMouthScale.y * 0.7f), Time.fixedDeltaTime * 0.8f);

            Vector2 rand = Random.insideUnitCircle.normalized * distance;
            enemyRB.position += rand;
            yield return new WaitForFixedUpdate();
            enemyRB.position -= rand;
            yield return new WaitForFixedUpdate();
            distance += 0.003f;
        }
        mouth.localScale = new Vector2(initialMouthScale.x * 1.5f, initialMouthScale.y * 1.5f);
        while (mouth.localScale.magnitude > initialMouthScale.magnitude)
        {
            while (EM.enemyIsFrozen)
            {
                yield return new WaitForFixedUpdate();
            }
            mouth.localScale = Vector2.MoveTowards(mouth.localScale, initialMouthScale, Time.fixedDeltaTime * 0.8f);
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator InitialDrag()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        enemyRB.drag = 3;
        yield return new WaitForSeconds(0.5f);
        enemyRB.drag = 0;
    }
}
