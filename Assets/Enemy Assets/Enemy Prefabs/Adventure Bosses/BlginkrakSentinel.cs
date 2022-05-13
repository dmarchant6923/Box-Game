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

    bool enemySpawnCR = false;
    public GameObject spawnGlow;
    GameObject newSpawnGlow;
    public GameObject enemy1;
    public GameObject enemy2;
    public GameObject enemy3;
    public GameObject enemy4;
    public GameObject enemy5;
    public GameObject perk;
    GameObject[] enemies = new GameObject[6];
    GameObject newEnemy;



    LineRenderer line;

    int groundAndEnemiesLM;

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

        groundAndEnemiesLM = LayerMask.GetMask("Obstacles", "Platforms", "Enemies");
        enemies[0] = enemy1;
        enemies[1] = enemy2;
        enemies[2] = enemy3;
        enemies[3] = enemy4;
        enemies[4] = enemy5;
        enemies[5] = perk;
    }

    private void Update()
    {
        float radius = 0.4f;
        RaycastHit2D hitbox = Physics2D.CircleCast(sentinelRB.position, radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
        if (hitbox.collider != null)
        {
            if (bossScript.sentinelMoveSpeed <= bossScript.initialSentinelMoveSpeed &&
            bossScript.explosionAttack == false && bossScript.sentinelHitboxEnabled && bossScript.enemySpawnAttack == false)
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

        if (bossScript.sentinelMoveSpeed <= bossScript.initialSentinelMoveSpeed && bossScript.explosionAttack == false && bossScript.sentinelHitboxEnabled &&
            bossScript.enemySpawnAttack == false)
        {
            if (bossScript.debugEnabled)
            {

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
        if (bossScript.enemySpawnAttack == true && enemySpawnCR == false)
        {
            StartCoroutine(SpawnEnemies());
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
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
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
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 3 * (sentinelRB.position - target).magnitude / distance);
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
        line.positionCount = 2;
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
        float minSpeed = 5;
        float maxSpeed = 20;
        Vector2 target = enemyPosition + Vector2.up * offset;
        float distance = (sentinelRB.position - target).magnitude;
        while ((sentinelRB.position - target).magnitude > 0.05f && EM.enemyWasKilled == false)
        {
            float speed = minSpeed + Mathf.Min(maxSpeed, maxSpeed * 2 * (sentinelRB.position - target).magnitude / distance);
            sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            sentinelRB.rotation = Mathf.MoveTowardsAngle(sentinelRB.rotation, 180f, 250 * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
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
        minSpeed = 100;
        maxSpeed = 600;
        float angle = 0;
        Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
        line.enabled = true;
        line.positionCount = 1;
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
            int num = Random.Range(0, enemies.Length - 1);
            if (Random.Range(0, 10) == 9)
            {
                num = 5;
            }
            newEnemy = Instantiate(enemies[num], enemyPosition, Quaternion.identity);
            if (newEnemy.GetComponent<perks>() == null)
            {
                bossScript.spawnedEnemies.Add(newEnemy);
            }
            while (color.a > 0 && EM.enemyWasKilled == false)
            {
                color.a -= 2 * Time.deltaTime;
                line.startColor = color;
                line.endColor = color;
                yield return null;
            }
        }
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
}
