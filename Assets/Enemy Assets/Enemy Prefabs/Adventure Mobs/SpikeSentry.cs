using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeSentry : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D boxRB;
    Rigidbody2D enemyRB;

    [HideInInspector] public GameObject[] sentinels = new GameObject[2];
    [HideInInspector] public bool[] sentinelIdle = new bool[2];
    [HideInInspector] public bool[] sentinelsKilled = new bool[2];
    float[] sentinelAngles = new float[2];
    [HideInInspector] public Vector2[] sentinelPositions = new Vector2[2];

    float startDistance;
    float sentinelDistance;
    float sentinelMoveSpeed = 15;
    float sentinelRotateSpeed = 700;
    float initialSentinelOrbitSpeed = 200;
    float sentinelOrbitSpeed;

    Vector2 vectorToBox;
    [HideInInspector] public Vector2 visibleVectorToBox;
    int obstacleLM;
    int boxLM;
    int obstacleAndBoxLM;

    bool touchingThisEnemy = false;

    public float initialSightRadius = 15;
    float sightRadius;
    bool canSeeBox = false;

    [HideInInspector] public bool boomerangAttack = false;
    [HideInInspector] public bool[] sentinelBoomerang = new bool[2];
    [HideInInspector] public bool[] sentinelsReturned = new bool[2];
    float attackDelay = 0.6f;
    float attackCooldown = 2f;
    bool cooldownActive = false;

    [HideInInspector] public bool stopIdleMovement = false;
    Vector2 posRight;
    Vector2 posLeft;
    [HideInInspector] public int direction = 1;
    float moveSpeed = 3;
    float waitTime = 1.5f;

    bool scaredMovement = false;

    public bool debugEnabled = false;

    [HideInInspector] public bool aggroActive = false;

    void Start()
    {
        EM = GetComponent<EnemyManager>();
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        for (int i = 0; i < 2; i++)
        {
            sentinels[i] = transform.GetChild(0).gameObject;
            sentinelAngles[i] = sentinels[i].transform.eulerAngles.z;
            sentinelIdle[i] = true;
            sentinels[i].GetComponent<SpikeSentinel>().index = i;
            sentinels[i].transform.parent = null;
        }

        startDistance = (sentinels[0].transform.position - transform.position).magnitude;
        sentinelDistance = startDistance;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed;
        sightRadius = initialSightRadius;

        boxLM = LayerMask.GetMask("Box");
        obstacleLM = LayerMask.GetMask("Obstacles");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");

        EM.normalPulse = false;
        EM.keepAsKinematic = true;


        float maxHalfDist = 5;
        RaycastHit2D RCLeft = Physics2D.CircleCast(enemyRB.position, transform.lossyScale.y / 2, Vector2.left, maxHalfDist + transform.lossyScale.y, obstacleLM);
        RaycastHit2D RCRight = Physics2D.CircleCast(enemyRB.position, transform.lossyScale.y / 2, Vector2.right, maxHalfDist + transform.lossyScale.y, obstacleLM);
        posLeft = enemyRB.position + Vector2.left * maxHalfDist;
        posRight = enemyRB.position + Vector2.right * maxHalfDist;
        if (RCLeft.collider != null)
        {
            posLeft = enemyRB.position + Vector2.left * (RCLeft.distance - transform.lossyScale.y);
        }
        if (RCRight.collider != null)
        {
            posRight = enemyRB.position + Vector2.right * (RCRight.distance - transform.lossyScale.y);
        }

        direction = (Random.Range(0, 2) * 2) - 1;
        StartCoroutine(IdleMovement());

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 2; i++)
        {
            if ((sentinels[i] == null || sentinels[i].GetComponent<EnemyManager>().enemyWasKilled) && sentinelsKilled[i] == false)
            {
                sentinelsKilled[i] = true;
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

        if (touchingThisEnemy && Box.boxHitboxActive && EM.enemyIsInvulnerable == false && Box.damageActive == false)
        {
            EM.enemyWasDamaged = true;
            Box.activateHitstop = true;
        }

        vectorToBox = (boxRB.position - enemyRB.position);
        RaycastHit2D[] enemyRC_RayToBox = Physics2D.RaycastAll(enemyRB.position, vectorToBox.normalized, sightRadius, obstacleAndBoxLM);
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
        if (enemyRC_RayToBox.Length > 0 && distToBox < distToObstacle)
        {
            canSeeBox = true;
            EM.canSeeItem = true;
            visibleVectorToBox = vectorToBox;
            if (stopIdleMovement == false)
            {
                StartCoroutine(StopMovement());
            }
            if (stopIdleMovement && scaredMovement == false && sentinelsKilled[0] && sentinelsKilled[1])
            {
                StartCoroutine(ScaredMovement());
            }
        }
        else
        {
            canSeeBox = false;
            EM.canSeeItem = false;
        }

        if (debugEnabled)
        {
            Color color = Color.white;
            if (canSeeBox) { color = Color.red; }
            else
            {
                Debug.DrawRay(enemyRB.position, visibleVectorToBox.normalized * sightRadius, Color.green);
            }
            Debug.DrawRay(enemyRB.position, vectorToBox.normalized * sightRadius, color);
        }

        if (canSeeBox && boomerangAttack == false && cooldownActive == false && EM.initialDelay == false && 
            (sentinelsKilled[0] == false || sentinelsKilled[1] == false))
        {
            StartCoroutine(BoomerangAttack());
        }


        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            moveSpeed *= EM.aggroIncreaseMult;
            waitTime *= EM.aggroDecreaseMult;

            attackDelay *= EM.aggroDecreaseMult;
            attackCooldown *= EM.aggroDecreaseMult;

            initialSentinelOrbitSpeed *= EM.aggroIncreaseMult;
            sentinelOrbitSpeed *= EM.aggroIncreaseMult;

            initialSightRadius *= EM.aggroIncreaseMult;
            sightRadius *= EM.aggroIncreaseMult;

            for (int i = 0; i < 2; i++)
            {
                if (sentinelsKilled[i] == false)
                {
                    sentinels[i].GetComponent<SpikeSentinel>().damage *= EM.aggroIncreaseMult;
                    sentinels[i].GetComponent<SpikeSentinel>().maxSpeed *= EM.aggroIncreaseMult;
                    sentinels[i].GetComponent<SpikeSentinel>().minSpeed += 1;
                    sentinels[i].GetComponent<SpikeSentinel>().waitTime *= EM.aggroDecreaseMult;
                }
            }
        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;

            moveSpeed /= EM.aggroIncreaseMult;
            waitTime /= EM.aggroDecreaseMult;

            attackDelay /= EM.aggroDecreaseMult;
            attackCooldown /= EM.aggroDecreaseMult;

            initialSentinelOrbitSpeed /= EM.aggroIncreaseMult;
            sentinelOrbitSpeed /= EM.aggroIncreaseMult;

            initialSightRadius /= EM.aggroIncreaseMult;
            sightRadius /= EM.aggroIncreaseMult;

            for (int i = 0; i < 2; i++)
            {
                if (sentinelsKilled[i] == false)
                {
                    sentinels[i].GetComponent<SpikeSentinel>().damage /= EM.aggroIncreaseMult;
                    sentinels[i].GetComponent<SpikeSentinel>().maxSpeed /= EM.aggroIncreaseMult;
                    sentinels[i].GetComponent<SpikeSentinel>().minSpeed -= 1;
                    sentinels[i].GetComponent<SpikeSentinel>().waitTime /= EM.aggroDecreaseMult;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < 2; i++)
        {
            if (EM.hitstopImpactActive == false)
            {
                sentinelAngles[i] += Time.fixedDeltaTime * sentinelOrbitSpeed;

                sentinelPositions[i] = new Vector2(Mathf.Cos(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2)).normalized * sentinelDistance;
                if (sentinels[i] != null && sentinelIdle[i] && sentinelsKilled[i] == false)
                {
                    sentinels[i].GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(sentinels[i].GetComponent<Rigidbody2D>().position,
                        enemyRB.position + sentinelPositions[i], sentinelMoveSpeed * Time.fixedDeltaTime);

                    sentinels[i].GetComponent<Rigidbody2D>().rotation = Mathf.MoveTowardsAngle(sentinels[i].GetComponent<Rigidbody2D>().rotation,
                        sentinelAngles[i], sentinelRotateSpeed * Time.fixedDeltaTime);
                }
            }

            if (sentinels[i] != null && (sentinels[i].GetComponent<Rigidbody2D>().position - (enemyRB.position + sentinelPositions[i])).magnitude < 0.05f || sentinelsKilled[i])
            {
                sentinelsReturned[i] = true;
            }
            else
            {
                sentinelsReturned[i] = false;
            }
        }
    }

    IEnumerator BoomerangAttack()
    {
        boomerangAttack = true;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed * 0.3f;
        sightRadius = initialSightRadius * 10;
        float distMult = 0.7f;
        float window = attackDelay;
        float timer = 0;
        while (timer < window)
        {
            sentinelDistance = Mathf.MoveTowards(sentinelDistance, startDistance * 0.6f, (startDistance * (1 - distMult) / window) * Time.fixedDeltaTime);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        bool allThrown = false;
        bool[] boomerangLaunched = new bool[2];
        while (allThrown == false)
        {
            allThrown = true;
            for (int i = 0; i < 2; i++)
            {
                if (Vector2.Dot(sentinelPositions[i].normalized, visibleVectorToBox.normalized) > 0.99f && 
                    boomerangLaunched[i] == false && sentinelsKilled[i] == false && sentinelsReturned[i])
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
            sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.9f * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        sightRadius = initialSightRadius;
        sentinelDistance = startDistance;
        sentinelOrbitSpeed = initialSentinelOrbitSpeed;

        bool allReturned = false;
        while (allReturned == false)
        {
            allReturned = true;
            for (int i = 0; i < 2; i++)
            {
                if (sentinelBoomerang[i] == true && sentinelsKilled[i] == false)
                {
                    allReturned = false;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        boomerangAttack = false;
        cooldownActive = true;
        yield return new WaitForSeconds(attackCooldown);
        cooldownActive = false;
    }
    IEnumerator IdleMovement()
    {
        if ((posLeft - posRight).magnitude > 3)
        {
            while (true)
            {
                if (direction == -1)
                {
                    while ((enemyRB.position - posLeft).magnitude > 0.05)
                    {
                        if (stopIdleMovement == false)
                        {
                            enemyRB.position = Vector2.MoveTowards(enemyRB.position, posLeft, moveSpeed * Time.deltaTime);
                        }
                        yield return null;
                    }
                    float window = waitTime + Random.Range(-waitTime * 0.2f, waitTime * 0.2f);
                    float timer = 0;
                    while (timer < window)
                    {
                        if (stopIdleMovement == false)
                        {
                            timer += Time.deltaTime;
                        }
                        yield return null;
                    }
                    direction *= -1;
                }
                if (direction == 1)
                {
                    while ((enemyRB.position - posRight).magnitude > 0.05)
                    {
                        if (stopIdleMovement == false)
                        {
                            enemyRB.position = Vector2.MoveTowards(enemyRB.position, posRight, moveSpeed * Time.deltaTime);
                        }
                        yield return null;
                    }
                    float window = waitTime + Random.Range(-waitTime * 0.2f, waitTime * 0.2f);
                    float timer = 0;
                    while (timer < window)
                    {
                        if (stopIdleMovement == false)
                        {
                            timer += Time.deltaTime;
                        }
                        yield return null;
                    }
                    direction *= -1;
                }
            }
        }
    }
    IEnumerator ScaredMovement()
    {
        scaredMovement = true;
        while (stopIdleMovement)
        {
            if (boxRB.position.x > enemyRB.position.x && canSeeBox && GetComponentInChildren<BlginkrakEye>().damageBlink == false)
            {
                enemyRB.position = Vector2.MoveTowards(enemyRB.position, posLeft, moveSpeed * Time.deltaTime);
            }
            else if (canSeeBox && GetComponentInChildren<BlginkrakEye>().damageBlink == false)
            {
                enemyRB.position = Vector2.MoveTowards(enemyRB.position, posRight, moveSpeed * Time.deltaTime);
            }
            yield return null;
        }
        scaredMovement = false;
    }
    IEnumerator StopMovement()
    {
        stopIdleMovement = true;
        while (canSeeBox || boomerangAttack)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForSeconds(1.6f);
        stopIdleMovement = false;
    }
}
