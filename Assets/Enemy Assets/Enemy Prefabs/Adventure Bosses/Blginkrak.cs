using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blginkrak : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    GameObject[] sentinels = new GameObject[4];
    float[] startAngles = new float[4];
    float startDistance;

    float sentinelMoveSpeed = 20;
    float sentinelRotateSpeed = 500;
    float initialSentinelOrbitSpeed = 300;
    float sentinelOrbitSpeed = 300;
    public bool[] sentinelIdle = new bool[4];
    public Vector2[] sentinelPositions = new Vector2[4];
    float[] sentinelAngles = new float[4];

    bool idle = true;
    bool idleCR = false;
    float cycleTimer = 2f;

    Vector2 vectorToBox;
    bool touchingThisEnemy;

    public bool boomerang = false;
    public bool[] sentinelBoomerang = new bool[4];

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
        startDistance = (sentinels[0].transform.position - transform.position).magnitude;
        for (int i = 0; i < 4; i++)
        {
            sentinels[i].transform.parent = null;
        }

        sentinelAngles = startAngles;
        
    }

    // Update is called once per frame
    void Update()
    {
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

        if (touchingThisEnemy && Box.boxHitboxActive)
        {
            EM.enemyWasDamaged = true;
            if (EM.enemyIsInvulnerable == false)
            {
                Box.activateHitstop = true;
            }
        }

        if (idle && idleCR == false)
        {
            StartCoroutine(AttackSelect());
            Debug.Log("start attack cycle");
        }

        vectorToBox = (boxRB.position - enemyRB.position).normalized;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < 4; i++)
        {
            sentinelAngles[i] += Time.fixedDeltaTime * sentinelOrbitSpeed;
            sentinelPositions[i] = new Vector2(Mathf.Cos(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(sentinelAngles[i] * Mathf.Deg2Rad + Mathf.PI / 2)).normalized * startDistance;
            if (sentinelIdle[i])
            {
                sentinels[i].GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(sentinels[i].GetComponent<Rigidbody2D>().position, 
                    enemyRB.position + sentinelPositions[i], sentinelMoveSpeed * Time.fixedDeltaTime);
                sentinels[i].GetComponent<Rigidbody2D>().rotation = Mathf.MoveTowardsAngle(sentinels[i].GetComponent<Rigidbody2D>().rotation, 
                    sentinelAngles[i], sentinelRotateSpeed * Time.fixedDeltaTime);
            }
        }
    }

    IEnumerator AttackSelect()
    {
        idleCR = true;
        float window = cycleTimer;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        StartCoroutine(BoomerangAttack());
        Debug.Log("starting Boomerang attack");

        idle = false;
        idleCR = false;
    }
    IEnumerator BoomerangAttack()
    {
        sentinelOrbitSpeed *= 0.2f;
        yield return new WaitForSeconds(0.2f);
        bool allThrown = false;
        bool[] boomerangLaunched = new bool[4];
        while (allThrown == false)
        {
            allThrown = true;
            for (int i = 0; i < 4; i++)
            {
                if (Vector2.Dot(sentinelPositions[i].normalized, vectorToBox) > 0.95f && boomerangLaunched[i] == false)
                {
                    boomerangLaunched[i] = true;
                    sentinelBoomerang[i] = true;
                    sentinelIdle[i] = false;
                    Debug.Log("launched sentinel " + i);
                }
                if (boomerangLaunched[i] == false)
                {
                    allThrown = false;
                }

            }
            sentinelOrbitSpeed += initialSentinelOrbitSpeed * 0.7f * Time.fixedDeltaTime;
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
                if (sentinelBoomerang[i] == true)
                {
                    allReturned = false;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        Debug.Log("all sentinels returned");
        idle = true;
    }
}
