using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlginkrakSentinel : MonoBehaviour
{
    public Blginkrak bossScript;
    GameObject boss;
    EnemyManager EM;
    Rigidbody2D bossRB;
    Rigidbody2D sentinelRB;
    Rigidbody2D boxRB;
    public int index;

    bool boomerangCR = false;
    float damage = 30;
    bool sentinelHitstopActive = false;
    Vector2 posPrevFrame;

    bool explosionCR = false;

    GameObject hitboxGlow;

    bool enemySpawnCR = false;
    public GameObject spawnGlow;
    GameObject newSpawnGlow;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;
    public GameObject enemy5;
    public GameObject enemy6;
    public GameObject enemy7;
    public GameObject mine;
    public GameObject perk;
    GameObject[] spawns = new GameObject[9];
    GameObject newEnemy;
    float spawnMaxMoveSpeed = 20;
    float spawnMaxRotateSpeed = 600;
    public bool canSpawnPerk = false;

    bool laserCR = false;
    bool laserActive = false;
    public GameObject fire;
    GameObject newFire;

    [HideInInspector] public bool newSpawn = false;

    [HideInInspector] public bool sawBladeCR = false;
    [HideInInspector] public bool readyForSawBlade = false;
    public GameObject sawBlade;
    GameObject newSawBlade;
    float bladeSpawnTime = 2f;
    float bladeSpeed = 10;

    LineRenderer line;
    Color lineColor;
    Color lineLaserColor;

    public GameObject smoke;
    bool smokeCR;
    Color damageColor1 = new Color(0.6f, 0.6f, 0.6f);
    Color damageColor2 = new Color(0.45f, 0.45f, 0.45f);

    int groundAndEnemiesLM;

    void Start()
    {
        EM = GetComponent<EnemyManager>();
        sentinelRB = GetComponent<Rigidbody2D>();
        bossRB = bossScript.GetComponent<Rigidbody2D>();
        boss = bossScript.gameObject;
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        line.SetPosition(0, sentinelRB.position); line.SetPosition(1, sentinelRB.position);
        line.enabled = false;

        EM.blastzoneDeath = false;
        EM.explosionsWillPush = false;

        groundAndEnemiesLM = LayerMask.GetMask("Obstacles", "Platforms", "Enemies");
        spawns[0] = enemy1;
        spawns[1] = enemy2;
        spawns[2] = enemy3;
        spawns[3] = enemy4;
        spawns[4] = enemy5;
        spawns[5] = enemy6;
        spawns[6] = enemy7;
        spawns[7] = mine;
        spawns[8] = perk;

        lineColor = line.startColor;
        lineLaserColor = new Color(1, 0.2f, 0.2f);

        hitboxGlow = transform.GetChild(0).gameObject;

        if (newSpawn)
        {
            StartCoroutine(NewSpawn());
            if (bossScript.upset)
            {
                NextState();
            }
            if (bossScript.pissed)
            {
                NextState();
            }
        }
    }

    private void Update()
    {
        if (boss.GetComponent<EnemyManager>().enemyWasKilled && EM.enemyWasKilled == false)
        {
            EM.enemyHealth = 0;
            EM.enemyWasDamaged = true;
            if (newSawBlade != null)
            {
                newSawBlade.GetComponent<SawBlade>().death = true;
            }
            StartCoroutine(SentinelDeath());
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
        if (bossScript.enemySpawnAttack == true && enemySpawnCR == false)
        {
            StartCoroutine(SpawnEnemies());
        }
        if (bossScript.laserAttack == true && laserCR == false)
        {
            StartCoroutine(Laser());
        }

        if (boomerangCR && sentinelHitstopActive == false && bossScript.sentinelsReturned[index] == false)
        {
            sentinelRB.rotation += 3000 * Time.fixedDeltaTime;
        }


        if (EM.enemyHealth == 2 && sentinelRB.GetComponent<SpriteRenderer>().color != damageColor1)
        {
            sentinelRB.GetComponent<SpriteRenderer>().color = damageColor1;
        }
        if (EM.enemyHealth == 1 && smokeCR == false)
        {
            sentinelRB.GetComponent<SpriteRenderer>().color = damageColor2;
            StartCoroutine(LowHealth());
        }

        float radius = transform.localScale.y * 0.6f;
        bool hitboxActive = false;
        if (((bossScript.sentinelMoveSpeed <= bossScript.initialSentinelMoveSpeed && bossScript.sentinelIdle[index] && bossScript.sentinelsReturned[index] && bossScript.idle) ||
                bossScript.boomerangAttack || laserActive || bossScript.dashAttack || (bossScript.spawnSentinel && newSpawn == false) || bossScript.spawnSawBlade) &&
                EM.enemyIsInvulnerable == false && EM.enemyWasKilled == false)
        {
            hitboxActive = true;
            hitboxGlow.SetActive(true);
        }
        else
        {
            hitboxGlow.SetActive(false);
        }

        int casts = 3;
        for (int i = 0; i < casts; i++)
        {
            Vector2 difference = sentinelRB.position - posPrevFrame;
            Vector2 increment = difference / casts;
            Vector2 position = posPrevFrame + (increment * (i + 1));
            RaycastHit2D hitbox = Physics2D.CircleCast(position, radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
            if (hitbox.collider != null)
            {
                if (hitboxActive)
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
                        if (bossScript.dashAttack)
                        {
                            StartCoroutine(bossScript.EnemyHitstop());
                            if (bossScript.dashActive)
                            {
                                Box.damageTaken = bossScript.dashDamage;
                                Box.boxDamageDirection = new Vector2(bossScript.dashDirection, 1).normalized;
                            }
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

    public void NextState()
    {
        if (bossScript.pissed)
        {
            damage += 5;

            bladeSpawnTime *= 0.75f;
            bladeSpeed *= 1.3f;

            spawnMaxMoveSpeed *= 1.2f;
            spawnMaxRotateSpeed *= 1.2f;
        }
        else if (bossScript.upset)
        {
            damage += 5;

            bladeSpawnTime *= 0.75f;
            bladeSpeed *= 1.3f;

            spawnMaxMoveSpeed *= 1.2f;
            spawnMaxRotateSpeed *= 1.2f;
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
            sentinelRB.velocity = new Vector2(0, 0);
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
    IEnumerator LowHealth()
    {
        smokeCR = true;
        while (EM.enemyWasKilled == false)
        {
            float window = 0.25f;
            float timer = 0;
            while (timer < window)
            {
                if (EM.hitstopImpactActive == false && bossScript.idle == false && bossScript.dashAttack == false)
                {
                    timer += Time.fixedDeltaTime;
                }
                yield return new WaitForFixedUpdate();
            }
            Instantiate(smoke, sentinelRB.position, Quaternion.identity);
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


    IEnumerator Boomerang()
    {
        boomerangCR = true;
        float maxSpeed = 25;
        float minSpeed = 3;
        if (bossScript.upset) { maxSpeed += 6; }// minSpeed += 3; } 
        if (bossScript.pissed) { maxSpeed += 6; }// minSpeed += 2; }
        Vector2 vectorToBox = boxRB.position - sentinelRB.position;
        float distance = Mathf.Max(15, vectorToBox.magnitude + 8f);
        Vector2 target = sentinelRB.position + (vectorToBox.normalized * distance);
        while (Mathf.Abs((sentinelRB.position - target).magnitude) > 0.2f && EM.enemyWasKilled == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        float window = 0.8f;
        if (bossScript.upset) { window -= 0.2f; }
        if (bossScript.pissed) { window -= 0.3f; }
        Vector2 direction = sentinelRB.velocity.normalized;
        float timer = 0;
        while (timer < window && EM.enemyWasKilled == false)
        {
            sentinelRB.velocity -= direction * (minSpeed * 2 / window) * Time.deltaTime;
            //sentinelRB.velocity = Vector2.zero;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        while ((sentinelRB.position - bossRB.position).magnitude > 1.5f && EM.enemyWasKilled == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (bossRB.position - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        while ((sentinelRB.position - (bossRB.position + bossScript.sentinelPositions[index])).magnitude > 0.5f && EM.enemyWasKilled == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (bossRB.position + bossScript.sentinelPositions[index] - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        sentinelRB.velocity = Vector2.zero;
        bossScript.sentinelBoomerang[index] = false;
        bossScript.sentinelIdle[index] = true;
        while (bossScript.boomerangAttack)
        {
            yield return null;
        }
        boomerangCR = false;
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
        if (bossScript.upset) { timeBetweenRays *= 0.8f; raySpeed *= 1.3f; initialTimer *= 0.9f; timeToEvade *= 0.9f; }
        if (bossScript.pissed) { timeBetweenRays *= 0.8f; raySpeed *= 1.3f; initialTimer *= 0.9f; timeToEvade *= 0.8f; }
        line.positionCount = 2;
        line.startColor = lineColor;
        line.endColor = lineColor;
        while (timer < initialTimer + timeBetweenRays * 5 && bossScript.sentinelsLeft == 4)
        {
            sentinelRB.velocity = Vector2.zero;
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
            }
            if (index == 1 && timer > initialTimer + timeBetweenRays * 2)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.right * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
            }
            if (index == 2 && timer > initialTimer + timeBetweenRays * 3)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[index + 1].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.up * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
            }
            if (index == 3 && timer > initialTimer + timeBetweenRays * 4)
            {
                line.enabled = true;
                rayDistance = Mathf.MoveTowards(rayDistance, (bossScript.sentinels[0].GetComponent<Rigidbody2D>().position - sentinelRB.position).magnitude, raySpeed * Time.fixedDeltaTime);
                Vector2 rayPosition = sentinelRB.position + Vector2.left * rayDistance;
                line.SetPosition(0, sentinelRB.position);
                line.SetPosition(1, rayPosition);
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        bossScript.explosionBoundaryConnected = true;
        if (bossScript.sentinelsLeft != 4)
        {
            line.enabled = false;
        }
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
    IEnumerator SpawnEnemies()
    {
        enemySpawnCR = true;


        //assigning area quadrant to sentinel
        Vector2 center = bossScript.area.transform.position;
        Vector2 halfExtents = bossScript.area.transform.localScale / 2;
        float offset = 2f;
        float minX = center.x - halfExtents.x + offset;
        float maxX = center.x - offset;
        if (index == 2 || index == 3)
        {
            minX = center.x + offset;
            maxX = center.x + halfExtents.x - offset;
        }
        float minY = center.y + offset;
        float maxY = center.y + halfExtents.y - offset;
        if (index == 1 || index == 2)
        {
            minY = center.y - halfExtents.y + offset;
            maxY = center.y - offset;
        }
        Vector2 min = new Vector2(minX, minY);
        Vector2 max = new Vector2(maxX, maxY);


        //picking enemyPosition when it passes some criteria
        Vector2 enemyPosition = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        float obstacleDist = 1.2f;
        RaycastHit2D obstacleCheck = Physics2D.CircleCast(enemyPosition, obstacleDist, Vector2.zero, 0f, groundAndEnemiesLM);
        float distToTelePositions = 1000;
        float minDist = 2f;
        foreach (Vector2 position in bossScript.telePositions)
        {
            distToTelePositions = Mathf.Min(distToTelePositions, (enemyPosition - position).magnitude);
        }
        while (distToTelePositions <= minDist || obstacleCheck.collider != null)
        {
            enemyPosition = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
            obstacleCheck = Physics2D.CircleCast(enemyPosition, obstacleDist, Vector2.zero, 0f, groundAndEnemiesLM);
            distToTelePositions = 1000;
            foreach (Vector2 position in bossScript.telePositions)
            {
                distToTelePositions = Mathf.Min(distToTelePositions, (enemyPosition - position).magnitude);
            }

            minDist -= 0.05f;
            obstacleDist -= 0.03f;

            if (bossScript.debugEnabled)
            {
                Debug.DrawRay(enemyPosition + Vector2.up, Vector2.down * 2);
                Debug.DrawRay(enemyPosition + Vector2.right, Vector2.left * 2);
                Debug.DrawRay((min + max) / 2 + Vector2.up, Vector2.down * 2, Color.green);
                Debug.DrawRay((min + max) / 2 + Vector2.right, Vector2.left * 2, Color.green);
            }
        }


        //moving sentinel to a point above enemyPosition, telling boss script that it's reached its destination afterwards. 
        offset = 1.7f;
        float maxSpeed = spawnMaxMoveSpeed;
        float minSpeed = spawnMaxMoveSpeed / 4;
        Vector2 target = enemyPosition + Vector2.up * offset;
        float distance = (sentinelRB.position - target).magnitude;
        while ((sentinelRB.position - target).magnitude > 0.2f && EM.enemyWasKilled == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 2 * (sentinelRB.position - target).magnitude / distance);
            sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            sentinelRB.rotation = Mathf.MoveTowardsAngle(sentinelRB.rotation, 180f, 250 * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        sentinelRB.position = target;
        bossScript.sentinelAtSpawnPoint[index] = true;
        sentinelRB.velocity = Vector2.zero;


        //continue rotating towards downward direction while waiting for boss script to set startSpawns = true
        while (bossScript.startSpawns == false)
        {
            sentinelRB.rotation = Mathf.MoveTowardsAngle(sentinelRB.rotation, 180f, 250 * Time.fixedDeltaTime);
            if (bossScript.debugEnabled)
            {
                Debug.DrawRay(enemyPosition + Vector2.up, Vector2.down * 2);
                Debug.DrawRay(enemyPosition + Vector2.right, Vector2.left * 2);
            }
            yield return new WaitForFixedUpdate();
        }


        //rotate sentinel position and rotation around enemyPosition at a certain speed and offset. Enable and draw line renderer points.
        maxSpeed = spawnMaxRotateSpeed;
        minSpeed = spawnMaxRotateSpeed / 6;
        float angle = 0;
        Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
        line.enabled = true;
        line.positionCount = 1;
        line.startColor = lineColor;
        line.endColor = lineColor;
        float lineOffset = 0.4f;
        line.SetPosition(0, sentinelRB.position - vector * lineOffset);
        int i = 1;
        while (angle < 360 && EM.enemyWasKilled == false)
        {
            if (EM.hitstopImpactActive == false)
            {
                float angularVelocity = minSpeed + Mathf.Min(maxSpeed, maxSpeed * (360 - angle) / 360);
                angle += angularVelocity * Time.fixedDeltaTime;
                vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, enemyPosition + (vector * offset), 40 * Time.fixedDeltaTime);
                sentinelRB.rotation = Mathf.MoveTowardsAngle(sentinelRB.rotation, angle + 180, 2000 * Time.fixedDeltaTime);
                line.positionCount++;
                line.SetPosition(i, sentinelRB.position - vector * lineOffset);
                i++;

                if (bossScript.debugEnabled)
                {
                    Debug.DrawRay(enemyPosition + Vector2.up, Vector2.down * 2);
                    Debug.DrawRay(enemyPosition + Vector2.right, Vector2.left * 2);
                }
            }

            yield return new WaitForFixedUpdate();
        }


        //when loop is finished, instantiate a spawnGlow and wait 0.25 seconds.
        if (EM.enemyWasKilled == false)
        {
            line.loop = true;
            newSpawnGlow = Instantiate(spawnGlow, enemyPosition, Quaternion.identity);
            newSpawnGlow.transform.localScale = Vector2.one * (offset - lineOffset) * 2;
            yield return new WaitForSeconds(0.25f);
        }


        //spawn the enemy and fade out line color over time at the same time spawnGlow fades out on its own. Disable and reset line parameters after it's faded out.
        Color color = line.startColor;
        Color initialColor = line.startColor;
        if (EM.enemyWasKilled == false)
        {
            bossScript.sentinelSpawnedEnemy[index] = true;
            int num = Random.Range(0, spawns.Length - 1);
            if (canSpawnPerk)// && Random.Range(0,4) == 0)
            {
                num = spawns.Length - 1;
            }
            newEnemy = Instantiate(spawns[num], enemyPosition, Quaternion.identity);
            if (newEnemy.GetComponent<EnemyManager>() != null)
            {
                bossScript.spawnedEnemies.Add(newEnemy);
                if (newEnemy.GetComponent<SpikeSentry>() != null)
                {
                    newEnemy.GetComponent<EnemyManager>().enemyHealth = 1;
                }
            }
            if (newEnemy.GetComponent<ProximityMine>() != null)
            {
                bossScript.spawnedMines.Add(newEnemy);
            }
            while (color.a > 0 && EM.enemyWasKilled == false)
            {
                color.a -= 2 * Time.deltaTime;
                line.startColor = color;
                line.endColor = color;
                yield return null;
            }
        }
        canSpawnPerk = false;
        line.enabled = false;
        line.startColor = initialColor;
        line.endColor = initialColor;
        line.loop = false;


        //pause script while waiting for boss script to set enemySpawnAttack to true.
        while (bossScript.enemySpawnAttack == true)
        {
            yield return null;
        }
        bossScript.sentinelAtSpawnPoint[index] = false;
        bossScript.sentinelSpawnedEnemy[index] = false;
        enemySpawnCR = false;
    }
    IEnumerator Laser()
    {
        laserCR = true;
        float initialMult = line.widthMultiplier;
        line.startColor = lineLaserColor;
        line.endColor = lineLaserColor;
        line.widthMultiplier = initialMult * 0.15f;
        line.loop = false;
        line.positionCount = 2;
        line.enabled = true;

        while (bossScript.laserActive == false && EM.enemyWasKilled == false)
        {
            line.SetPosition(0, sentinelRB.position);
            Vector2 vector = new Vector2(Mathf.Cos(sentinelRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(sentinelRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
            RaycastHit2D raycast = Physics2D.Raycast(sentinelRB.position, vector, 100, LayerMask.GetMask("Obstacles", "Box"));
            if (Box.dodgeInvulActive)
            {
                raycast = Physics2D.Raycast(sentinelRB.position, vector, 100, LayerMask.GetMask("Obstacles"));
            }
            if (raycast.collider != null)
            {
                line.SetPosition(1, raycast.point + vector * 0.1f);
            }
            else
            {
                line.SetPosition(1, sentinelRB.position + vector * 100);
            }

            yield return new WaitForFixedUpdate();
        }

        line.widthMultiplier = initialMult;
        laserActive = true;
        float rotationPrevFrame = sentinelRB.rotation;
        while (bossScript.laserActive && EM.enemyWasKilled == false)
        {
            line.SetPosition(0, sentinelRB.position);

            Vector2 vector = new Vector2(Mathf.Cos(sentinelRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(sentinelRB.rotation * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
            RaycastHit2D raycast = Physics2D.Raycast(sentinelRB.position, vector, 100, LayerMask.GetMask("Obstacles", "Box"));
            if (Box.dodgeInvulActive)
            {
                raycast = Physics2D.Raycast(sentinelRB.position, vector, 100, LayerMask.GetMask("Obstacles"));
            }
            else
            {
                line.SetPosition(1, sentinelRB.position + vector * 100);
            }
            if (raycast.collider != null)
            {
                line.SetPosition(1, raycast.point + vector * 0.1f);
            }

            int casts = 4 + (4 - bossScript.sentinelsLeft) * 3;
            float increment = (sentinelRB.rotation - rotationPrevFrame) / (casts);
            for (int i = 0; i < casts; i++)
            {
                vector = new Vector2(Mathf.Cos((rotationPrevFrame + increment * (i + 1)) * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin((rotationPrevFrame + increment * (i + 1)) * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                raycast = Physics2D.Raycast(bossRB.position + vector * bossScript.sentinelDistance, vector, 100, LayerMask.GetMask("Obstacles", "Box"));
                if (raycast.collider != null)
                {
                    if (raycast.collider.GetComponent<Box>() != null && Box.damageActive == false)
                    {
                        Box.activateDamage = true;
                        Box.damageTaken = damage;
                        Box.boxDamageDirection = new Vector2(Mathf.Sign(vector.x), 1).normalized;
                    }
                    if (1 << raycast.collider.gameObject.layer == LayerMask.GetMask("Obstacles"))
                    {
                        Physics2D.queriesHitTriggers = true;
                        RaycastHit2D[] fireRC = Physics2D.CircleCastAll(raycast.point, 0.5f, Vector2.zero, 0, LayerMask.GetMask("Hazards"));
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
                            newFire = Instantiate(fire, raycast.point, Quaternion.identity);
                            newFire.GetComponent<Fire>().surfaceNormal = raycast.normal;
                            newFire.GetComponent<Fire>().hazardFire = true;
                            newFire.GetComponent<Fire>().fireWindow = 0.7f;
                        }
                    }
                }

                if (bossScript.debugEnabled)
                {
                    Debug.DrawRay(bossRB.position + vector * bossScript.sentinelDistance, vector * raycast.distance);
                }
            }

            rotationPrevFrame = sentinelRB.rotation;
            yield return new WaitForFixedUpdate();
        }
        line.enabled = false;
        laserActive = false;

        while (bossScript.laserAttack)
        {
            yield return null;
        }
        laserCR = false;
    }
    IEnumerator NewSpawn()
    {
        Vector2 vector = bossScript.sentinelPositions[index].normalized; //Vector2.down;
        float distance = 8;
        Vector2 target = sentinelRB.position + (vector * distance);
        float maxSpeed = 20;
        if (bossScript.upset) { maxSpeed *= 1.3f; }
        if (bossScript.pissed) { maxSpeed *= 1.3f; }
        sentinelRB.angularVelocity = 200f;
        while (Mathf.Abs((sentinelRB.position - target).magnitude) > 0.5f && EM.enemyWasKilled == false)
        {
            float speed = ((sentinelRB.position - target).magnitude / distance) * maxSpeed;
            if (sentinelHitstopActive == false)
            {
                sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        sentinelRB.angularVelocity = 0;
        bossScript.sentinelIdle[index] = true;
        while (bossScript.sentinelsReturned[index] == false)
        {
            yield return null;
        }
        newSpawn = false;
    }
    public IEnumerator SawBlade()
    {
        sawBladeCR = true;
        bossScript.sentinelIdle[index] = false;
        sentinelRB.angularVelocity = 0;
        float window = bladeSpawnTime * 0.3f;
        float speed = 5000 / window;
        Vector2 initialScale = transform.localScale;
        float moveSpeed = 15;
        float angleMoveSpeed = 90;
        if (bossScript.upset)
        {
            angleMoveSpeed += 30;
        }
        if (bossScript.pissed)
        {
            angleMoveSpeed += 30;
        }
        float angle = -Mathf.Atan2(bossScript.vectorToBox.x, bossScript.vectorToBox.y) * Mathf.Rad2Deg;
        Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;

        while (Mathf.Abs(sentinelRB.angularVelocity) < 5000)
        {
            float boxAngle = -Mathf.Atan2(bossScript.vectorToBox.x, bossScript.vectorToBox.y) * Mathf.Rad2Deg;
            angle = Mathf.MoveTowardsAngle(angle, boxAngle, angleMoveSpeed * Time.deltaTime);
            vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
            sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, bossRB.position + vector * bossScript.sentinelDistance * 1.3f, moveSpeed * Time.fixedDeltaTime);
            sentinelRB.angularVelocity += speed * Time.fixedDeltaTime;
            transform.localScale = Vector2.MoveTowards(transform.localScale, initialScale * 1.5f, 0.3f * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        window = bladeSpawnTime - window;
        float timer = 0;
        newSawBlade = Instantiate(sawBlade, sentinelRB.position, Quaternion.identity);
        newSawBlade.GetComponent<SawBlade>().fadeIn = true;
        newSawBlade.GetComponent<SawBlade>().fadeTime = window;

        while (timer < window)
        {
            float boxAngle = -Mathf.Atan2(bossScript.vectorToBox.x, bossScript.vectorToBox.y) * Mathf.Rad2Deg;
            angle = Mathf.MoveTowardsAngle(angle, boxAngle, angleMoveSpeed * Time.deltaTime);
            vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
            sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, bossRB.position + vector * bossScript.sentinelDistance * 1.3f, moveSpeed * Time.fixedDeltaTime);
            newSawBlade.transform.position = sentinelRB.position;
            foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                Color color = sprite.color;
                color.a -= (1 / window) * Time.fixedDeltaTime;
                sprite.color = color;
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        newSawBlade.GetComponent<SawBlade>().speed = bladeSpeed;
        newSawBlade.GetComponent<SawBlade>().direction = vector;
        newSawBlade.GetComponent<SawBlade>().damage = damage;
        newSawBlade.GetComponent<SawBlade>().Launch();
        bossScript.bladeThrown = true;
        bossScript.blade = newSawBlade;
        //Destroy(gameObject);
        bossScript.sentinelIdle[index] = true;
        sawBladeCR = false;
        sentinelRB.angularVelocity = 0;
        transform.localScale = initialScale;
        foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
        {
            Color color = sprite.color;
            color.a = 1;
            sprite.color = color;
        }


    }
}
