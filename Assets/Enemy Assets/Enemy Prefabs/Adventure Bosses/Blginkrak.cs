using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blginkrak : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    [HideInInspector] public GameObject[] sentinels = new GameObject[4];
    bool[] sentinelsKilled = new bool[4];
    [HideInInspector] public int sentinelsLeft = 4;
    public GameObject area;
    [HideInInspector] public List<Vector2> telePositions = new List<Vector2>();
    float distanceTriggersTeleport = 8f;
    Vector2 currentPosition;
    float[] startAngles = new float[4];
    float startDistance;
    [HideInInspector] public float sentinelDistance;

    [HideInInspector] public float sentinelMoveSpeed = 20;
    [HideInInspector] public float initialSentinelMoveSpeed = 20;

    float sentinelRotateSpeed = 600;

    float initialSentinelOrbitSpeed = 400;
    [HideInInspector] public float sentinelOrbitSpeed = 400;

    [HideInInspector] public bool[] sentinelIdle = new bool[4];
    [HideInInspector] public Vector2[] sentinelPositions = new Vector2[4];
    float[] sentinelAngles = new float[4];

    [HideInInspector] public bool idle = true;
    bool canTeleport = true;
    bool idleTeleportCR = false;
    bool attackCycleCR = false;
    float cycleTimer = 4f; //2f
    bool teleportCR = false;
    int lastAttackNum = 0;
    int attacksStarted = 0;

    [HideInInspector] public Vector2 vectorToBox;
    bool touchingThisEnemy;

    float initialHealth;
    [HideInInspector] public bool upset = false;
    [HideInInspector] public bool pissed = false;

    [HideInInspector] public bool boomerangAttack = false;
    [HideInInspector] public bool[] sentinelBoomerang = new bool[4];

    [HideInInspector] public bool explosionAttack = false;
    [HideInInspector] public bool explosionBoundaryConnected = false;
    [System.NonSerialized] public float distBetweenSentinels = 6;
    public GameObject squareExplosion;
    GameObject newSquareExplosion;

    [HideInInspector] public bool enemySpawnAttack = false;
    [HideInInspector] public bool enemiesSpawned = false;
    [HideInInspector] public bool[] sentinelAtSpawnPoint = new bool[4];
    [HideInInspector] public bool[] sentinelSpawnedEnemy = new bool[4];
    [HideInInspector] public bool startSpawns = false;
    [HideInInspector] public List<GameObject> spawnedEnemies = new List<GameObject>();

    [HideInInspector] public bool laserAttack = false;
    [HideInInspector] public bool laserActive = false;
    [System.NonSerialized] public float laserTime;
    [System.NonSerialized] public float initialLaserTime = 9f;

    [HideInInspector] public bool dashAttack = false;
    [HideInInspector] public bool dashActive = false;
    [HideInInspector] public int dashDirection = 1;
    [HideInInspector] public float dashDamage = 50;
    [HideInInspector] public bool bossHitstopActive = false;

    [HideInInspector] public bool spawnSawBlade = false;
    [HideInInspector] public bool bladeThrown = false;
    [HideInInspector] public GameObject blade;

    [HideInInspector] public bool spawnSentinel = false;
    public GameObject sentinelClone;
    GameObject newSentinel;
    BlginkrakEye eyeScript;

    [HideInInspector] public bool sentinelHitboxEnabled = true;
    bool allSentinelsReturned = true;
    [HideInInspector] public bool[] sentinelsReturned = new bool[4];

    public bool debugEnabled = false;

    void Start()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        eyeScript = GetComponentInChildren<BlginkrakEye>();
        EM = GetComponent<EnemyManager>();
        EM.normalPulse = false;
        //EM.normalDeath = false;
        EM.keepAsKinematic = true;
        initialHealth = EM.enemyHealth;

        for (int i = 0; i < 4; i++)
        {
            sentinels[i] = transform.GetChild(i).gameObject;
            sentinels[i].GetComponent<BlginkrakSentinel>().index = i;

            startAngles[i] = sentinels[i].transform.eulerAngles.z;

            sentinelIdle[i] = true;
        }
        sentinelAngles = startAngles;
        startDistance = (sentinels[0].transform.position - transform.position).magnitude;
        sentinelDistance = startDistance;
        for (int i = 0; i < 4; i++)
        {
            sentinels[i].transform.parent = null;
        }

        foreach (Transform child in area.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject != area.gameObject)
            {
                telePositions.Add(child.position);
                if (debugEnabled == false)
                {
                    child.GetComponent<SpriteRenderer>().enabled = false;
                }
            }
        }

        laserTime = initialLaserTime;


        if (debugEnabled == false)
        {
            area.GetComponent<SpriteRenderer>().enabled = false;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (EM.enemyWasKilled)
        {
            foreach (GameObject enemy in spawnedEnemies)
            {
                enemy.GetComponent<EnemyManager>().enemyHealth = 0;
                enemy.GetComponent<EnemyManager>().enemyWasDamaged = true;
            }
            Destroy(this);
        }

        for (int i = 0; i < 4; i++)
        {
            if ((sentinels[i] == null || sentinels[i].GetComponent<EnemyManager>().enemyWasKilled) && sentinelsKilled[i] == false)
            {
                sentinelsKilled[i] = true;
                sentinelsLeft -= 1;
                laserTime = initialLaserTime - (4 - sentinelsLeft) * 1.3f;
            }
        }

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] == null || spawnedEnemies[i].GetComponent<EnemyManager>().enemyWasKilled)
            {
                spawnedEnemies.RemoveAt(i);
                i--;
            }
        }

        if (bladeThrown && blade == null)
        {
            bladeThrown = false;
        }

        if (upset == false && EM.enemyHealth <= (initialHealth / 2) + 1)
        {
            upset = true;
            Color color = GetComponent<SpriteRenderer>().color;
            color.r = 0.8f;
            GetComponent<SpriteRenderer>().color = color;

            dashDamage += 5;
            for (int i = 0; i < 4; i++)
            {
                if (sentinels[i] != null)
                {
                    sentinels[i].GetComponent<BlginkrakSentinel>().NextState();
                }
            }
        }
        if (pissed == false && EM.enemyHealth <= (initialHealth / 4) + 1)
        {
            pissed = true;
            Color color = GetComponent<SpriteRenderer>().color;
            color.r = 1f;
            color.g = 0;
            color.b = 0;
            GetComponent<SpriteRenderer>().color = color;

            dashDamage += 5;
            for (int i = 0; i < 4; i++)
            {
                if (sentinels[i] != null)
                {
                    sentinels[i].GetComponent<BlginkrakSentinel>().NextState();
                }
            }
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
            touchingThisEnemy = false;
            EM.touchingThisEnemy = false;
        }

        if (touchingThisEnemy && Box.boxHitboxActive && EM.enemyIsInvulnerable == false && dashActive == false && Box.damageActive == false)
        {
            EM.enemyWasDamaged = true;
            Box.activateHitstop = true;
        }
        if (touchingThisEnemy && Box.isInvulnerable == false && dashActive)
        {
            Box.activateDamage = true;
            Box.damageTaken = dashDamage;
            Box.boxDamageDirection = new Vector2(dashDirection, 1).normalized;
            StartCoroutine(EnemyHitstop());
        }

        if (idle || explosionAttack || enemySpawnAttack)
        {
            canTeleport = true;
        }
        else
        {
            canTeleport = false;
        }
        if (idle && attackCycleCR == false)
        {
            StartCoroutine(AttackSelect());
        }
        if (canTeleport && idleTeleportCR == false)
        {
            StartCoroutine(IdleTeleport());
        }

        vectorToBox = (boxRB.position - enemyRB.position).normalized;
    }

    private void FixedUpdate()
    {
        allSentinelsReturned = true;
        for (int i = 0; i < 4; i++)
        {
            if (dashAttack && bossHitstopActive)
            {
                sentinelAngles[i] += Time.fixedDeltaTime * sentinelOrbitSpeed / 20;
            }
            else
            {
                sentinelAngles[i] += Time.fixedDeltaTime * sentinelOrbitSpeed;
            }
            sentinelPositions[i] = new Vector2(Mathf.Cos(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2)).normalized * sentinelDistance;
            if (sentinels[i] != null && sentinelIdle[i] && sentinelsKilled[i] == false)
            {
                sentinels[i].GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(sentinels[i].GetComponent<Rigidbody2D>().position,
                    enemyRB.position + sentinelPositions[i], sentinelMoveSpeed * Time.fixedDeltaTime);

                sentinels[i].GetComponent<Rigidbody2D>().rotation = Mathf.MoveTowardsAngle(sentinels[i].GetComponent<Rigidbody2D>().rotation,
                    sentinelAngles[i], sentinelRotateSpeed * Time.fixedDeltaTime);
            }


            if ((sentinels[i] != null && (sentinels[i].GetComponent<Rigidbody2D>().position - (enemyRB.position + sentinelPositions[i])).magnitude < 0.05f && 
                enemySpawnAttack == false && explosionAttack == false) || sentinelsKilled[i])
            {
                sentinelsReturned[i] = true;
            }
            else
            {
                sentinelsReturned[i] = false;
                allSentinelsReturned = false;
            }
        }

        enemyRB.velocity = Vector2.zero;
        enemyRB.rotation = 0;

        if (debugEnabled)
        {
            Debug.DrawRay(enemyRB.position + Vector2.up * distanceTriggersTeleport, Vector2.down * 2 * distanceTriggersTeleport, Color.gray);
            Debug.DrawRay(enemyRB.position + Vector2.right * distanceTriggersTeleport, Vector2.left * 2 * distanceTriggersTeleport, Color.gray);
        }
    }

    private void explosion(Vector2 position)
    {
        newSquareExplosion = Instantiate(squareExplosion, position, Quaternion.identity);
        newSquareExplosion.GetComponent<Explosion>().explosionRadius = distBetweenSentinels / 2;
        newSquareExplosion.GetComponent<Explosion>().explosionDamage = 40;
        newSquareExplosion.GetComponent<Explosion>().circleExplosion = false;
    }
    IEnumerator IdleTeleport()
    {
        idleTeleportCR = true;
        RaycastHit2D boxDetect = Physics2D.CircleCast(enemyRB.position, distanceTriggersTeleport, Vector2.zero, 0, LayerMask.GetMask("Box"));
        while (boxDetect.collider == null && canTeleport)
        {
            boxDetect = Physics2D.CircleCast(enemyRB.position, distanceTriggersTeleport, Vector2.zero, 0, LayerMask.GetMask("Box"));
            yield return new WaitForFixedUpdate();
        }
        if (canTeleport && teleportCR == false)
        {
            StartCoroutine(Teleport());
        }
        float time = 2.5f;
        if (upset) { time = 2f; } if (pissed) { time = 1.5f; }
        yield return new WaitForSeconds(time);
        idleTeleportCR = false;

    }
    IEnumerator Teleport()
    {
        teleportCR = true;
        yield return new WaitForSeconds(0.3f);
        if (canTeleport)
        {
            float minDist = 15;
            Vector2 newPosition = telePositions[Random.Range(0, telePositions.Count - 1)];
            while ((newPosition - boxRB.position).magnitude < minDist || (newPosition - currentPosition).magnitude < 0.4f)
            {
                newPosition = telePositions[Random.Range(0, telePositions.Count - 1)];
                minDist -= 0.1f;
            }
            currentPosition = newPosition;
            enemyRB.position = newPosition;
            sentinelMoveSpeed = initialSentinelMoveSpeed * 2;
            teleportCR = false;
            yield return new WaitForFixedUpdate();
            while (allSentinelsReturned == false)
            {
                yield return null;
            }
            sentinelMoveSpeed = initialSentinelMoveSpeed;
        }
        teleportCR = false;
    }
    public IEnumerator EnemyHitstop()
    {
        bossHitstopActive = true;
        Vector2 enemyHitstopVelocity = enemyRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        enemyRB.velocity = new Vector2(0, 0);
        enemyRB.angularVelocity /= enemyHitstopRotationSlowDown;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        enemyRB.angularVelocity *= enemyHitstopRotationSlowDown;
        enemyRB.velocity = enemyHitstopVelocity;
        bossHitstopActive = false;
    }
    IEnumerator AttackSelect()
    {
        attackCycleCR = true;
        float window = cycleTimer;
        if (upset) { window = cycleTimer * 0.7f; }
        if (pissed) { window = cycleTimer * 0.5f; }
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        bool attackSelected = false;
        int num = 100;
        //if (sentinelsLeft > 0)
        //{
        //    while (bladeThrown)
        //    {
        //        yield return null;
        //    }
        //    attackSelected = true;
        //    StartCoroutine(SawBlade());
        //    Debug.Log("blade selected");
        //}
        int i = 0;
        while (attackSelected == false)
        {
            int numAttacks = 7;
            num = Random.Range(0, numAttacks);

            // 0          1            2          3      4     5          6          7         8     9            10         11     12    13           14
            //if all 4 sentinels:
            //

            //if missing sentinels:
            //

            //if <4 sentinels and >=7 enemies:
            //
            if ((attacksStarted % 9 == 0 && sentinelsLeft < 4) || (attacksStarted % 2 == 0 && sentinelsLeft == 0))
            {
                StartCoroutine(SpawnSentinel());
                attackSelected = true;
                num = 0;
                Debug.Log("manually selected sentinel spawn");
            }
            else if (attacksStarted % 5 == 1 && spawnedEnemies.Count < 7 && sentinelsLeft > 0 && lastAttackNum != num)
            {
                StartCoroutine(EnemySpawnAttack());
                attackSelected = true;
                num = 1;
                Debug.Log("manually selected enemy spawn");
            }
            else if (sentinelsLeft > 0 && attacksStarted % 6 == 0 && lastAttackNum != num)
            {
                StartCoroutine(BoomerangAttack());
                attackSelected = true;
                num = 2;
                Debug.Log("manually selected boomerang");
            }
            else if (attacksStarted % 5 == 0 || sentinelsLeft == 0 && lastAttackNum != num)
            {
                StartCoroutine(DashAttack());
                attackSelected = true;
                num = 3;
                Debug.Log("manually selected dash attack");
            }
            else if (attacksStarted % 5 == 2 && sentinelsLeft == 4 && lastAttackNum != num)
            {
                StartCoroutine(ExplosionAttack());
                attackSelected = true;
                num = 4;
                Debug.Log("manually selected explosion");
            }
            else if (attacksStarted % 5 == 3 && pissed == false && sentinelsLeft > 0 && lastAttackNum != num)
            {
                StartCoroutine(LaserAttack());
                attackSelected = true;
                num = 5;
                Debug.Log("manually selected laser");
            }
            else if (attacksStarted % 5 == 4 && sentinelsLeft > 2 && bladeThrown == false && lastAttackNum != num)
            {
                StartCoroutine(SawBlade());
                attackSelected = true;
                num = 6;
                Debug.Log("manually selected saw blade");
            }


            else if (num == 1 && spawnedEnemies.Count < 7 && sentinelsLeft > 0 && lastAttackNum != num && (attacksStarted + 1) % 5 != 1)
            {
                StartCoroutine(EnemySpawnAttack());
                attackSelected = true;
                Debug.Log("randomly selected enemy spawn");
            }
            else if (num == 2 && sentinelsLeft > 0 && lastAttackNum != num && (attacksStarted + 1) % 6 != 0)
            {
                StartCoroutine(BoomerangAttack());
                attackSelected = true;
                Debug.Log("randomly selected boomerang");
            }
            else if (num == 5 && sentinelsLeft > 0 && pissed == false && lastAttackNum != num && (attacksStarted + 1) % 5 != 3)
            {
                StartCoroutine(LaserAttack());
                attackSelected = true;
                Debug.Log("randomly selected laser");
            }
            else if (num == 6 && sentinelsLeft > 2 && bladeThrown == false && lastAttackNum != num && (attacksStarted + 1) % 5 != 4)
            {
                StartCoroutine(SawBlade());
                attackSelected = true;
                Debug.Log("randomly selected saw blade");
            }
            else if ((num == 3 && lastAttackNum != num && (attacksStarted + 1) % 5 != 0) || i >= 20)
            {
                StartCoroutine(DashAttack());
                attackSelected = true;
                Debug.Log("randomly selected dash attack");
                if (i >= 20)
                {
                    Debug.Log("max iterations reached");
                }
            }

            i++;
        }
        Debug.Log(i);
        attacksStarted++;
        lastAttackNum = num;

        idle = false;
        attackCycleCR = false;
    }


    IEnumerator BoomerangAttack()
    {
        while (allSentinelsReturned == false)
        {
            yield return null;
        }
        boomerangAttack = true;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed * 0.1f;
        if (upset) { sentinelOrbitSpeed = initialSentinelOrbitSpeed * 0.2f; }
        if (pissed) { sentinelOrbitSpeed = initialSentinelOrbitSpeed * 0.4f; }
        float window = 0.7f;
        if (upset) { window = 0.5f; } if (pissed) { window = 0.25f; }
        float timer = 0;
        while (timer < window)
        {
            sentinelDistance = Mathf.MoveTowards(sentinelDistance, startDistance * 0.6f, startDistance * Time.fixedDeltaTime * 2);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        bool allThrown = false;
        bool[] boomerangLaunched = new bool[4];
        while (allThrown == false)
        {
            allThrown = true;
            for (int i = 0; i < 4; i++)
            {
                if (Vector2.Dot(sentinelPositions[i].normalized, vectorToBox) > 0.99f && boomerangLaunched[i] == false && sentinelsKilled[i] == false && sentinelsReturned[i])
                {
                    boomerangLaunched[i] = true;
                    sentinelBoomerang[i] = true;
                    sentinelIdle[i] = false;
                }
                if (boomerangLaunched[i] == false && sentinelsKilled[i] == false)
                {
                    allThrown = false;
                }

            }
            sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.4f * Time.fixedDeltaTime;
            if (upset) { sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.3f * Time.fixedDeltaTime; }
            if (pissed) { sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.5f * Time.fixedDeltaTime; }
            yield return new WaitForFixedUpdate();
        }
        sentinelDistance = startDistance;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed;

        bool allReturned = false;
        while (allReturned == false)
        {
            allReturned = true;
            for (int i = 0; i < 4; i++)
            {
                if (sentinelBoomerang[i] == true && sentinelsKilled[i] == false)
                {
                    allReturned = false;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        idle = true;
        boomerangAttack = false;
    }
    IEnumerator ExplosionAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = false;
        }
        explosionAttack = true;
        while (explosionBoundaryConnected == false && sentinelsLeft == 4)
        {
            yield return null;
        }
        explosionBoundaryConnected = false;
        if (sentinelsLeft == 4)
        {
            Vector2 explosionPosition = (sentinels[0].transform.position + sentinels[1].transform.position + sentinels[2].transform.position + sentinels[3].transform.position) / 4;
            for (int i = 0; i < 4; i++)
            {
                explosion(explosionPosition);
                yield return new WaitForSeconds(0.2f);
            }
            yield return new WaitForSeconds(1);
        }

        explosionAttack = false;
        idle = true;
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = true;
        }
        sentinelHitboxEnabled = false;
        while (allSentinelsReturned == false)
        {
            yield return new WaitForFixedUpdate();
        }
        sentinelHitboxEnabled = true;
    }
    IEnumerator EnemySpawnAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = false;
        }
        enemySpawnAttack = true;

        int num = Random.Range(0, 4);
        if (sentinelsKilled[num] == false)
        {
            sentinels[num].GetComponent<BlginkrakSentinel>().canSpawnPerk = true;
        }

        //wait for all 4 sentinels to arrive, then set startSpawns = true to trigger sentinels to begin spinning
        bool sentinelsArrived = false;
        while (sentinelsArrived == false)
        {
            sentinelsArrived = true;
            for (int i = 0; i < 4; i++)
            {
                if (sentinelAtSpawnPoint[i] == false && sentinelsKilled[i] == false)
                {
                    sentinelsArrived = false;
                    break;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        float waitTime = 0.25f;
        if (upset) { waitTime -= 0.05f; }
        if (pissed) { waitTime -= 0.05f; }
        yield return new WaitForSeconds(waitTime);
        startSpawns = true;

        //wait for all 4 sentinels to finish spawning the enemy, then pause for a small amount of time then finish attack
        while (enemiesSpawned == false)
        {
            enemiesSpawned = true;
            for (int i = 0; i < 4; i++)
            {
                if (sentinelSpawnedEnemy[i] == false && sentinelsKilled[i] == false)
                {
                    enemiesSpawned = false;
                    break;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        startSpawns = false;
        waitTime = 0.5f;
        if (upset) { waitTime -= 0.1f; }
        if (pissed) { waitTime -= 0.1f; }
        yield return new WaitForSeconds(waitTime);
        idle = true;
        enemySpawnAttack = false;
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = true;
        }
        enemiesSpawned = false;
    }
    IEnumerator LaserAttack()
    {
        while (allSentinelsReturned == false)
        {
            yield return null;
        }
        laserAttack = true;
        sentinelOrbitSpeed *= 0.1f + (4 - sentinelsLeft) * 0.07f;
        if (sentinelsLeft == 1)
        {
            sentinelOrbitSpeed += 60;
        }
        float window = laserTime * 0.15f;
        float timer = 0;
        while (timer < window)
        {
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }

        laserActive = true;
        window = laserTime * 0.7f;
        timer = 0;
        while (timer < window && EM.hitstopImpactActive == false)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        laserActive = false;

        yield return new WaitForSeconds(laserTime * 0.15f);
        sentinelOrbitSpeed = initialSentinelOrbitSpeed;
        laserAttack = false;
        idle = true;
    }
    IEnumerator DashAttack()
    {
        //begin dash attack, move boss upwards from position
        while (allSentinelsReturned == false)
        {
            yield return null;
        }
        yield return null;
        dashAttack = true;
        sentinelMoveSpeed = initialSentinelMoveSpeed * 10;
        Vector2 target = enemyRB.position + Vector2.up * 40;
        float speed = 0f;
        float window = 1.5f;
        float timer = 0;
        while (timer < window)
        {
            sentinelMoveSpeed = initialSentinelMoveSpeed * 10;
            if (bossHitstopActive == false)
            {
                enemyRB.position = Vector2.MoveTowards(enemyRB.position, target, speed * Time.fixedDeltaTime);
                speed += 50 * Time.fixedDeltaTime;
                timer += Time.fixedDeltaTime;
            }
            yield return new WaitForFixedUpdate();
        }

        //make boss and sentinels larger, spin faster. Decide dash direction and height, and teleport off screen at the chosen height
        Vector2 bossInitialScale = transform.localScale;
        Vector2 sentinelInitialScale = Vector2.one;
        float scale = 2.2f;
        transform.localScale = bossInitialScale * scale;
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i] == false)
            {
                sentinelInitialScale = sentinels[i].transform.localScale;
                sentinels[i].transform.localScale = sentinelInitialScale * scale;
            }
        }
        sentinelDistance = startDistance * (1 + scale / 4); //1.4f;
        sentinelOrbitSpeed *= 4f;
        sentinelRotateSpeed *= 10f;
        dashDirection = (Random.Range(0, 2) * 2) - 1;
        float heightMult = 0.3f;
        float height = area.transform.position.y + Random.Range(-1f, 1f) * area.transform.localScale.y * heightMult - 1;
        enemyRB.position = new Vector2(area.transform.position.x - dashDirection * 60, height);
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i] == false)
            {
                sentinels[i].transform.position = enemyRB.position + sentinelPositions[i];
            }
        }

        //if not pissed, move to the edge of the area to prepare to dash. if pissed, skip directly to the dash.
        float minSpeed = 1;
        if (pissed == false)
        {
            target = new Vector2(area.transform.position.x + ((area.transform.localScale.x / 2) + 10) * -dashDirection, height);
            while ((enemyRB.position - target).magnitude > 0.2f)
            {
                
                speed = minSpeed + ((enemyRB.position - target).magnitude) * 4;
                enemyRB.position = Vector2.MoveTowards(enemyRB.position, target, speed * Time.fixedDeltaTime);
                timer += Time.fixedDeltaTime;

                if (debugEnabled)
                {
                    Debug.DrawRay(new Vector2(target.x, area.transform.position.y + area.transform.localScale.y * heightMult - 1), Vector2.right * dashDirection * 5, Color.green);
                    Debug.DrawRay(new Vector2(target.x, area.transform.position.y - area.transform.localScale.y * heightMult - 1), Vector2.right * dashDirection * 5, Color.green);
                }
                yield return new WaitForFixedUpdate();
            }

            //chill there for a bit, then pull back before launching
            float time = 0.7f;
            if (upset) { time = 0.2f; }
            yield return new WaitForSeconds(time);
            window = 0.3f;
            timer = 0;
            speed = 5f;
            while (timer < window)
            {
                enemyRB.position += Vector2.right * speed * Time.fixedDeltaTime * -dashDirection;
                speed -= 20 * Time.fixedDeltaTime;
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }

        //dash is now active, move in dash direction for a certain length of time, then deactivate dash and change size / speed back to normal afterwards
        dashActive = true;
        speed = 40f;
        if (upset) { speed = 50f; }
        window = 2f;
        timer = 0;
        while (timer < window)
        {
            if (bossHitstopActive == false)
            {
                enemyRB.position += Vector2.right * speed * Time.fixedDeltaTime * dashDirection;
            }
            else
            {
                enemyRB.position += Vector2.right * speed * Time.fixedDeltaTime * dashDirection / 15;
            }
            speed += 20 * Time.fixedDeltaTime;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        dashActive = false;
        transform.localScale = bossInitialScale;
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i] == false)
            {
                sentinels[i].transform.localScale = sentinelInitialScale;
            }
        }
        sentinelDistance = startDistance;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed;


        //pick a new position using same teleport logic, set boss position to be above the new position, then descend into new position
        float minDist = 15;
        Vector2 newPosition = telePositions[Random.Range(0, telePositions.Count - 1)];
        while ((newPosition - boxRB.position).magnitude < minDist || (newPosition - currentPosition).magnitude < 0.4f)
        {
            newPosition = telePositions[Random.Range(0, telePositions.Count - 1)];
            minDist -= 0.1f;
        }
        enemyRB.position = newPosition + Vector2.up * 25;
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i] == false)
            {
                sentinels[i].transform.position = enemyRB.position + sentinelPositions[i];
            }
        }
        minSpeed = 1;
        while ((enemyRB.position - newPosition).magnitude > 0.05f)
        {
            speed = minSpeed + (enemyRB.position - newPosition).magnitude * 4;
            enemyRB.position = Vector2.MoveTowards(enemyRB.position, newPosition, speed * Time.fixedDeltaTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        enemyRB.position = newPosition;
        sentinelMoveSpeed = initialSentinelMoveSpeed;
        yield return new WaitForSeconds(0.4f);
        dashAttack = false;
        idle = true;



    }
    IEnumerator SpawnSentinel()
    {
        spawnSentinel = true;
        float window = 2f;
        if (upset) { window = 1.4f; }
        if (pissed) { window = 0.7f; }
        float timer = 0f;
        StartCoroutine(eyeScript.ForceBlink(window));
        while (timer < window)
        {
            Vector2 shuffle = Random.insideUnitCircle.normalized * 0.3f * (timer / window);
            enemyRB.position += shuffle;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();

            enemyRB.position -= shuffle;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        int index = 0;
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i])
            {
                index = i;
                break;
            }        
        }
        newSentinel = Instantiate(sentinelClone, transform);
        newSentinel.transform.position = transform.position;
        newSentinel.transform.parent = null;
        newSentinel.GetComponent<BlginkrakSentinel>().index = index;
        newSentinel.GetComponent<BlginkrakSentinel>().bossScript = this;
        newSentinel.GetComponent<BlginkrakSentinel>().newSpawn = true;
        newSentinel.GetComponent<EnemyManager>().enemyHealth = 2;
        sentinelIdle[index] = false;
        sentinels[index] = newSentinel;
        sentinelsKilled[index] = false;
        sentinelsLeft++;
        yield return new WaitForFixedUpdate();
        while (allSentinelsReturned == false)
        {
            yield return null;
        }
        spawnSentinel = false;
        idle = true;
    }
    IEnumerator SawBlade()
    {
        while (allSentinelsReturned == false)
        {
            yield return null;
        }
        spawnSawBlade = true;
        for (int i = 0; i < 4; i++)
        {
            if (sentinelsKilled[i] == false)
            {
                StartCoroutine(sentinels[i].GetComponent<BlginkrakSentinel>().SawBlade());
                break;
            }
        }

        while (bladeThrown == false)
        {
            yield return null;
        }
        float waitTime = 0.5f;
        if (upset) { waitTime -= 0.1f; }
        if (pissed) { waitTime -= 0.1f; }
        yield return new WaitForSeconds(waitTime);
        spawnSawBlade = false;
        idle = true;
    }
}
