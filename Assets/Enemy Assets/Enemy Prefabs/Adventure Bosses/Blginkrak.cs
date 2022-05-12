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
    int sentinelsLeft = 4;
    public GameObject area;
    List<Vector2> telePositions = new List<Vector2>();
    float distanceTriggersTeleport = 8f;
    Vector2 currentPosition;
    float[] startAngles = new float[4];
    float startDistance;

    [HideInInspector] public float sentinelMoveSpeed = 20;
    [HideInInspector] public float initialSentinelMoveSpeed = 20;

    float sentinelRotateSpeed = 600;

    float initialSentinelOrbitSpeed = 400;
    float sentinelOrbitSpeed = 400;

    [HideInInspector] public bool[] sentinelIdle = new bool[4];
    [HideInInspector] public Vector2[] sentinelPositions = new Vector2[4];
    float[] sentinelAngles = new float[4];

    bool idle = true;
    bool canTeleport = true;
    bool idleTeleportCR = false;
    bool attackCycleCR = false;
    float cycleTimer = 4f; //2f
    bool teleportCR = false;

    Vector2 vectorToBox;
    bool touchingThisEnemy;

    [HideInInspector] public bool[] sentinelBoomerang = new bool[4];

    [HideInInspector] public bool explosionAttack = false;
    [HideInInspector] public bool explosionBoundaryConnected = false;
    [HideInInspector] public bool[] boundaryConnected = new bool[4];
    [System.NonSerialized] public float distBetweenSentinels = 6;
    public GameObject squareExplosion;
    GameObject newSquareExplosion;

    [HideInInspector] public bool sentinelHitboxEnabled = true;
    bool sentinelsReturned = true;

    public bool debugEnabled = false;

    void Start()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        EM = GetComponent<EnemyManager>();
        EM.normalPulse = false;
        EM.normalDeath = false;

        for (int i = 0; i < 4; i++)
        {
            sentinels[i] = transform.GetChild(i).gameObject;
            sentinels[i].GetComponent<BlginkrakSentinel>().index = i;

            startAngles[i] = sentinels[i].transform.eulerAngles.z;

            sentinelIdle[i] = true;
        }
        sentinelAngles = startAngles;
        startDistance = (sentinels[0].transform.position - transform.position).magnitude;
        for (int i = 0; i < 4; i++)
        {
            sentinels[i].transform.parent = null;
        }

        foreach (Transform child in area.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject != area.gameObject)
            {
                telePositions.Add(child.position);
                child.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        area.GetComponent<SpriteRenderer>().enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            if (sentinels[i] == null || sentinels[i].GetComponent<EnemyManager>().enemyWasKilled && sentinelsKilled[i] == false)
            {
                sentinelsKilled[i] = true;
                sentinelsLeft -= 1;
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

        if (touchingThisEnemy && Box.boxHitboxActive && EM.enemyIsInvulnerable == false)
        {
            EM.enemyWasDamaged = true;
            Box.activateHitstop = true;
            StartCoroutine(Teleport(true));
        }

        if (idle || explosionAttack)
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
        for (int i = 0; i < 4; i++)
        {
            sentinelsReturned = true;
            sentinelAngles[i] += Time.fixedDeltaTime * sentinelOrbitSpeed;
            sentinelPositions[i] = new Vector2(Mathf.Cos(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2)).normalized * startDistance;
            if (sentinelIdle[i] && sentinelsKilled[i] == false)
            {
                sentinels[i].GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(sentinels[i].GetComponent<Rigidbody2D>().position,
                    enemyRB.position + sentinelPositions[i], sentinelMoveSpeed * Time.fixedDeltaTime);

                sentinels[i].GetComponent<Rigidbody2D>().rotation = Mathf.MoveTowardsAngle(sentinels[i].GetComponent<Rigidbody2D>().rotation,
                    sentinelAngles[i], sentinelRotateSpeed * Time.fixedDeltaTime);
            }

            if (sentinels[i] != null && (sentinels[i].GetComponent<Rigidbody2D>().position - (enemyRB.position + sentinelPositions[i])).magnitude > 0.2f)
            {
                sentinelsReturned = false;
            }
        }

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
            StartCoroutine(Teleport(false));
        }
        yield return new WaitForSeconds(2.5f);
        idleTeleportCR = false;

    }
    IEnumerator Teleport(bool damaged)
    {
        teleportCR = true;
        while (canTeleport == false && damaged == true)
        {
            yield return null;
        }
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
            sentinelMoveSpeed *= 2;
            yield return new WaitForSeconds(0.4f);
            sentinelMoveSpeed = initialSentinelMoveSpeed;
        }
        teleportCR = false;
    }
    IEnumerator AttackSelect()
    {
        attackCycleCR = true;
        float window = cycleTimer;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        int num = Random.Range(0, 2);
        bool attackSelected = false;
        while (attackSelected == false)
        {
            if (num == 0)
            {
                StartCoroutine(BoomerangAttack());
                attackSelected = true;
            }
            if (num == 1 && sentinelsLeft == 4)
            {
                StartCoroutine(ExplosionAttack());
                attackSelected = true;
            }
            num = Random.Range(0, 2);
        }
        Debug.Log("starting attack");

        idle = false;
        attackCycleCR = false;
    }
    IEnumerator BoomerangAttack()
    {
        sentinelOrbitSpeed *= 0.1f;
        yield return new WaitForSeconds(0.2f);
        bool allThrown = false;
        bool[] boomerangLaunched = new bool[4];
        while (allThrown == false)
        {
            allThrown = true;
            for (int i = 0; i < 4; i++)
            {
                if (Vector2.Dot(sentinelPositions[i].normalized, vectorToBox) > 0.95f && boomerangLaunched[i] == false && sentinelsKilled[i] == false)
                {
                    boomerangLaunched[i] = true;
                    sentinelBoomerang[i] = true;
                    sentinelIdle[i] = false;
                    Debug.Log("launched sentinel " + i);
                }
                if (boomerangLaunched[i] == false && sentinelsKilled[i] == false)
                {
                    allThrown = false;
                }

            }
            sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.4f * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("all sentinels thrown");
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
        Debug.Log("all sentinels returned");
        idle = true;
    }
    IEnumerator ExplosionAttack()
    {
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = false;
        }
        explosionAttack = true;
        while (explosionBoundaryConnected == false)
        {
            yield return null;
        }
        explosionBoundaryConnected = false;
        Vector2 explosionPosition = (sentinels[0].transform.position + sentinels[1].transform.position + sentinels[2].transform.position + sentinels[3].transform.position) / 4;
        for (int i = 0; i < 4; i++)
        {
            explosion(explosionPosition);
            yield return new WaitForSeconds(0.2f);
        }
        yield return new WaitForSeconds(1);

        explosionAttack = false;
        idle = true;
        for (int i = 0; i < 4; i++)
        {
            sentinelIdle[i] = true;
        }
        sentinelHitboxEnabled = false;
        while (sentinelsReturned == false)
        {
            yield return new WaitForFixedUpdate();
        }
        sentinelHitboxEnabled = true;
    }
}
