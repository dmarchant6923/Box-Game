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

    public float sightRadius = 12;

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
    public float bulletSpeed = 10;
    public float bulletSpreadMax = 40;
    public int bulletsPerAttack = 10;
    public float delayBeforeShooting = 0.7f;
    float bulletDespawnTime;
    public float shootTimeInterval = 0.1f;
    public float bulletRecoil = 1;
    public float bulletDamage = 25;
    public float restTime = 2f;
    bool isAttacking = false;
    bool isCurrentlyShooting = false;
    bool isWaitingToShoot = false;
    bool isOnCoolDown = false;

    float timeToReproduce = 8;
    Vector2 minSize;
    Vector2 maxSize;
    float sizeMult = 2f;
    float growthTimer = 0;

    bool birthCRActive = false;
    Color initialColor;
    bool givingBirth = false;
    public GameObject starManProjectile;
    GameObject newStarManProjectile;
    bool gaveBirth = false;

    public bool debugEnabled = false;

    bool aggroActive = false;

    int obstacleAndBoxLM;
    int boxLM;
    int obstacleLM;
    
    void Start()
    {
        EM = GetComponent<EnemyManager>();
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();


        EM.pulseMultiplier = 0.55f;
        mouth = transform.GetChild(0);
        bulletDespawnTime = sightRadius * 3 / bulletSpeed;

        realVector = lastVectorToBox;
        initialMouthScale = mouth.localScale;

        RaycastHit2D circleCast = Physics2D.CircleCast(enemyRB.position, 1.5f, Vector2.zero, 0, obstacleLM);
        if (circleCast.collider == null)
        {
            initialPosition = enemyRB.position;
        }
        else
        {
            for (float angle = 0; angle < 360; angle += 30)
            {
                Vector2 vector = Tools.AngleToVector(angle);
                circleCast = Physics2D.CircleCast(enemyRB.position + vector * 1.5f, 1.5f, Vector2.zero, 0, obstacleLM);
                if (circleCast.collider == null)
                {
                    initialPosition = enemyRB.position + vector;
                    break;
                }

            }
        }

        truePositionX = initialPosition.x;
        enemyRB.velocity = Vector2.up * 3;
        truePositionXDirection = Random.Range(0, 2);
        if (truePositionXDirection == 0) { truePositionXDirection = -1; }

        minSize = transform.localScale;
        maxSize = minSize * sizeMult;
        initialColor = GetComponent<SpriteRenderer>().color;

        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        boxLM = LayerMask.GetMask("Box");
        obstacleLM = LayerMask.GetMask("Obstacles");

        StartCoroutine(StarParticles());
    }
    private void FixedUpdate()
    {
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
        if (Mathf.Abs(enemyRB.position.x - truePositionX) >= 3)
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
        RaycastHit2D obstacleCheckRC = Physics2D.BoxCast(new Vector2(truePositionX + transform.lossyScale.x * 2f * truePositionXDirection,
            initialPosition.y), new Vector2(transform.lossyScale.x / 4, transform.lossyScale.y), 0, Vector2.down, 0f, obstacleLM);
        if (obstacleCheckRC.collider != null)
        {
            truePositionXDirection *= -1;
        }

        //magical spinning
        trueAngularVelocity = -Mathf.Sqrt(Mathf.Abs(enemyRB.velocity.x)) * Mathf.Sign(enemyRB.velocity.x) * spinMultiplier;
        float spinAccel = 2000;
        enemyRB.angularVelocity = Mathf.MoveTowards(enemyRB.angularVelocity, trueAngularVelocity, spinAccel * Time.fixedDeltaTime);

        //mouth movement
        if (canSeeBox)
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

        if (shootCRActive == false && canSeeBox == false && EM.initialDelay == false && gaveBirth == false)
        {
            growthTimer += Time.fixedDeltaTime;
            float currentScale = Mathf.Min(1 + (growthTimer / timeToReproduce) * (sizeMult - 1), sizeMult);
            transform.localScale = minSize * currentScale;
            if (growthTimer > timeToReproduce && birthCRActive == false)
            {
                StartCoroutine(Birth());
            }
        }


        //debug lines
        if (debugEnabled)
        {
            Debug.DrawRay(new Vector2(truePositionX - 1, initialPosition.y), Vector2.right * 2);

            if (obstacleCheckRC.collider != null)
            {
                Debug.DrawRay(new Vector2(truePositionX + transform.lossyScale.x * 4 * truePositionXDirection,
                    (initialPosition.y) - transform.lossyScale.y / 2), Vector2.up * transform.lossyScale.y, Color.red);
                Debug.DrawRay(new Vector2((truePositionX + transform.lossyScale.x * 4 * truePositionXDirection) - transform.lossyScale.x / 8,
                    initialPosition.y), Vector2.right * transform.lossyScale.x / 4, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(truePositionX + transform.lossyScale.x * 4 * truePositionXDirection,
                    (initialPosition.y) - transform.lossyScale.y / 2), Vector2.up * transform.lossyScale.y, Color.white);
                Debug.DrawRay(new Vector2((truePositionX + transform.lossyScale.x * 4 * truePositionXDirection) - transform.lossyScale.x / 8,
                    initialPosition.y), Vector2.right * transform.lossyScale.x / 4, Color.white);
            }
        }
    }
    void Update()
    {
        vectorToBox = (boxRB.position - enemyRB.position).normalized;
        RaycastHit2D[] enemyRC_RayToBox = Physics2D.RaycastAll(enemyRB.position, vectorToBox, sightRadius, obstacleAndBoxLM);
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
        if (distToBox < distToObstacle)
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
    }

    IEnumerator ShootBullets()
    {
        shootCRActive = true;

        forceMagnitudeY /= 4;
        truePositionXVelocity /= 4;
        forceMagnitudeX /= 4;
        enemyRB.velocity /= 4;
        floatVelocity /= 4;
        enemyRB.drag = 1;

        foreach (SpriteRenderer sprite in mouth.GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sprite.color;
            color.g += 0.25f;
            sprite.color = color;
        }

        int bulletsShot = 0;
        float bulletSpread;
        isCurrentlyShooting = true;
        isWaitingToShoot = true;
        float delayBeforeShootingTimer = 0;
        while (delayBeforeShootingTimer <= delayBeforeShooting && EM.enemyWasKilled == false)
        {
            if (EM.hitstopImpactActive == false)
            {
                delayBeforeShootingTimer += Time.fixedDeltaTime;
            }
            if (delayBeforeShootingTimer > delayBeforeShooting / 2)
            {
                mouthRotateSpeed = 0;
            }

            mouth.localScale = Vector2.MoveTowards(mouth.localScale, new Vector2(initialMouthScale.x * 1.3f, initialMouthScale.y * 0.8f), Time.deltaTime * 0.6f);

            yield return new WaitForFixedUpdate();
        }
        isWaitingToShoot = false;
        float shootTimeIntervalTimer = 0;
        while (bulletsShot < bulletsPerAttack && EM.enemyWasKilled == false)
        {
            mouth.localScale = new Vector2(mouth.localScale.x, initialMouthScale.y * 1.7f);

            if (bulletsShot == 1)
            {
                bulletRecoil /= 100;
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
            if (aggroActive)
            {
                newStarBullet.GetComponent<BulletScript>().aggro = true;
            }
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
            color.g -= 0.25f;
            sprite.color = color;
        }

        yield return new WaitForSeconds(restTime * 0.2f);
        isCurrentlyShooting = false;
        isOnCoolDown = true;

        bulletRecoil *= 100;
        forceMagnitudeY *= 4;
        truePositionXVelocity *= 4;
        forceMagnitudeX *= 4;
        floatVelocity *= 4;
        enemyRB.drag = 0;
        mouthRotateSpeed = initialMouthRotateSpeed;

        float window = restTime * 0.8f;
        float timer = 0;
        while (timer < window)
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
        while (timer < delay && shootCRActive == false)
        {
            realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(lastVectorToBox), mouthRotateSpeed * Time.deltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        while (shootCRActive == false)
        {
            Vector2 vector = new Vector2(truePositionXDirection, -0.25f).normalized;
            float angle = Tools.VectorToAngle(vector);
            realAngle = Mathf.MoveTowardsAngle(realAngle, angle, mouthRotateSpeed * Time.deltaTime);

            Debug.DrawRay(enemyRB.position, vector);

            yield return new WaitForFixedUpdate();
        }
        idleMouthCRActive = false;

    }
    IEnumerator Birth()
    {
        birthCRActive = true;
        float delay = 7;
        float timer = 0;
        float initialG = initialColor.g;
        float targetG = 0.2f;
        float period = 0.5f;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        while (timer < delay)
        {
            if (timer > delay * 0.6f)
            {
                period = 0.2f;
            }
            Color color = sprite.color;

            while (timer < delay && color.g < initialG)
            {
                if (EM.flashOn == false)
                {
                    color.g = Mathf.MoveTowards(color.g, initialG, (initialColor.g - targetG) / period * 2 * Time.fixedDeltaTime);
                    sprite.color = color;
                }
                if (canSeeBox == false && shootCRActive == false)
                {
                    timer += Time.fixedDeltaTime;
                }
                yield return new WaitForFixedUpdate();
            }
            color.g = initialG;
            sprite.color = color;

            while (timer < delay && color.g > targetG)
            {
                if (EM.flashOn == false)
                {
                    color.g = Mathf.MoveTowards(color.g, targetG, (initialColor.g - targetG) / period * 2 * Time.fixedDeltaTime);
                    sprite.color = color;
                }
                if (canSeeBox == false && shootCRActive == false)
                {
                    timer += Time.fixedDeltaTime;
                }
                yield return new WaitForFixedUpdate();
            }
            color.g = targetG;
            sprite.color = color;
        }

        while (canSeeBox || shootCRActive)
        {
            yield return null;
        }

        givingBirth = true;
        StartCoroutine(BirthShake());

        forceMagnitudeY /= 20;
        truePositionXVelocity /= 20;
        forceMagnitudeX /= 20;
        enemyRB.velocity /= 20;
        floatVelocity /= 20;
        enemyRB.drag = 5;

        delay = 1.5f;
        timer = 0;
        float distance = 15;
        float maxDistance = 0;
        Vector2 chosenVector = Vector2.up;
        for (int angle = 0; angle < 360; angle += 10)
        {
            Vector2 vector = Tools.AngleToVector(angle);
            RaycastHit2D ray = Physics2D.Raycast(enemyRB.position, vector, distance, obstacleLM);
            if (ray.collider != null && ray.distance > maxDistance)
            {
                maxDistance = ray.distance;
                chosenVector = vector;
            }
        }
        while (canSeeBox == false && timer < delay)
        {
            realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(chosenVector), mouthRotateSpeed * Time.deltaTime);
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        while (canSeeBox && timer < delay)
        {
            realAngle = Mathf.MoveTowardsAngle(realAngle, Tools.VectorToAngle(lastVectorToBox), mouthRotateSpeed * Time.deltaTime);
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }
        while (timer < delay)
        {
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }

        newStarManProjectile = Instantiate(starManProjectile, enemyRB.position + realVector * transform.localScale.x * 0.7f, Quaternion.identity);
        newStarManProjectile.GetComponent<Rigidbody2D>().velocity = realVector.normalized * 20;
        enemyRB.AddForce(5 * -realVector, ForceMode2D.Impulse);

        gaveBirth = true;
        birthCRActive = false;

        float currentScale = sizeMult;
        while (currentScale > 1)
        {
            currentScale -= Time.fixedDeltaTime;
            transform.localScale = minSize * currentScale;

            if (EM.flashOn == false)
            {
                Color color = sprite.color;
                color.g = Mathf.MoveTowards(color.g, initialG, Time.fixedDeltaTime);
                sprite.color = color;
            }

            yield return new WaitForFixedUpdate();
        }

        forceMagnitudeY *= 20;
        truePositionXVelocity *= 20;
        forceMagnitudeX *= 20;
        enemyRB.velocity *= 20;
        floatVelocity *= 20;
        enemyRB.drag = 0;
    }
    IEnumerator BirthShake()
    {
        while (givingBirth)
        {
            yield return new WaitForFixedUpdate();
        }
    }
}
