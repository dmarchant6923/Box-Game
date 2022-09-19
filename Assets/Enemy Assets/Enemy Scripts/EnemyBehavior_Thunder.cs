using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Thunder : MonoBehaviour
{
    EnemyManager EM;
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;

    Color initialColor;

    int obstacleLM;
    int boxLM;
    int enemyLM;

    float thunderRadius = 8;
    float thunderDamage = 40;

    public bool isStationary = false;
    float initialDistAtoB = 8;
    float distAtoB;
    public float timeToMove = 4;
    float maxMoveForce = 25;
    int forceDirectionX = 0;
    int forceDirectionY = 0;
    float regAngVel = 200;
    float shockAngVel = 700;
    float releaseAngVel = 3000;
    float currentAngVel;
    bool addForce = true;
    Vector2 pointA;
    Vector2 pointB;
    Vector2 currentPoint;
    int stage = 1;

    bool touchingThisEnemy = false;
    [System.NonSerialized] public bool shockPrepared = false;
    bool shockCRActive = false;
    bool enemyHitstopActive = false;

    public GameObject lightning;
    GameObject newLightning;

    public GameObject spikeN;
    public GameObject spikeE;
    public GameObject spikeS;
    public GameObject spikeW;
    GameObject[] spikes;

    List<SpriteRenderer> enemyObjects;
    List<Color> enemyColors;
    bool releaseFlashCR = false;

    bool aggroActive = false;

    public bool debugLines = false;


    void Start()
    {
        obstacleLM = LayerMask.GetMask("Obstacles");
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");

        initialColor = GetComponent<SpriteRenderer>().color;

        EM = GetComponent<EnemyManager>();
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        spikes = new GameObject[] { spikeN, spikeE, spikeS, spikeW };
        enemyObjects = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        enemyColors = new List<Color>();
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            if (enemyObjects[i].gameObject.tag == "Effect")
            {
                enemyObjects.RemoveAt(i);
                i -= 1;
            }
            enemyColors.Add(enemyObjects[i].color);
        }

        currentAngVel = regAngVel;

        distAtoB = initialDistAtoB;
        pointA = enemyRB.position; //pointA will always be on the left
        RaycastHit2D rayLeft = Physics2D.Raycast(pointA, Vector2.left, distAtoB, obstacleLM);
        RaycastHit2D rayRight = Physics2D.Raycast(pointA, Vector2.right, distAtoB, obstacleLM);
        if (isStationary == false)
        {
            if (rayLeft.collider == null && rayRight.collider == null)
            {
                int rand = Random.Range(0, 3);
                if (rand == 0)
                {
                    pointB = pointA + Vector2.right * distAtoB;
                }
                else if (rand == 1)
                {
                    pointB = pointA;
                    pointA += Vector2.left * distAtoB;
                }
                else
                {
                    pointA += Vector2.left * distAtoB / 2;
                    pointB = pointA + Vector2.right * distAtoB;
                }
            }
            else if (rayLeft.collider != null && rayRight.collider == null)
            {
                pointB = pointA + Vector2.right * distAtoB;
            }
            else if (rayLeft.collider == null && rayRight.collider != null)
            {
                pointB = pointA;
                pointA += Vector2.left * distAtoB;
            }
            else
            {
                pointA = rayLeft.point + Vector2.right * transform.localScale.x * 2;
                pointB = rayRight.point + Vector2.left * transform.lossyScale.x * 2;
                distAtoB = Mathf.Abs(pointA.x - pointB.x);
                if ((pointA - pointB).magnitude < 5)
                {
                    pointA = enemyRB.position;
                    pointB = pointA;
                    isStationary = true;
                }
            }
        }
        else
        {
            pointA = enemyRB.position;
            pointB = pointA;
        }

        currentPoint = pointA;
        enemyRB.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * 2, ForceMode2D.Impulse);
        enemyRB.angularVelocity = 200;

        StartCoroutine(SwitchPoints());
    }

    private void Update()
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
        if (touchingThisEnemy == true)
        {
            if (Box.boxHitboxActive && (shockPrepared == false || Box.isInvulnerable))
            {
                gameObject.GetComponent<EnemyManager>().enemyWasDamaged = true;
                if (gameObject.GetComponent<EnemyManager>().enemyIsInvulnerable == false)
                {
                    Box.activateHitstop = true;
                }
            }
            if (shockPrepared && enemyHitstopActive == false)
            {
                Box.damageTaken = Lightning.contactDamage;
                Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - enemyRB.position.x), 1).normalized;
                Box.activateDamage = true;
                Box.activateShock = true;
                StartCoroutine(EnemyHitstop());
            }
        }

        Vector3 colorValues = new Vector3(GetComponent<SpriteRenderer>().color.r, GetComponent<SpriteRenderer>().color.g, GetComponent<SpriteRenderer>().color.b);
        if (releaseFlashCR == false)
        {
            if (stage == 4 && shockPrepared)
            {
                colorValues = Vector3.MoveTowards(colorValues, new Vector3(1f, 0f, 0f), Time.deltaTime * 1.2f);
                GetComponent<SpriteRenderer>().color = new Color(colorValues.x, colorValues.y, colorValues.z);
            }
            else
            {
                colorValues = Vector3.MoveTowards(colorValues, new Vector3(initialColor.r, initialColor.g, initialColor.b), Time.deltaTime * 10);
                GetComponent<SpriteRenderer>().color = new Color(colorValues.x, colorValues.y, colorValues.z);
            }
        }

        if (EM.enemyIsFrozen)
        {
            shockPrepared = false;
            stage = 1;
            currentAngVel = regAngVel;
            return;
        }

        RaycastHit2D[] boxCheck = Physics2D.CircleCastAll(boxRB.position, thunderRadius, Vector2.zero, 0, enemyLM);
        bool thunderGuyInRadius = false;
        foreach (RaycastHit2D enemy in boxCheck)
        {
            if (enemy.transform.GetComponent<EnemyBehavior_Thunder>() != null && enemy.transform.GetComponent<EnemyBehavior_Thunder>().shockPrepared)
            {
                thunderGuyInRadius = true;
            }
        }
        if (thunderGuyInRadius && EM.enemyWasKilled == false) { Box.inShockRadius = true; }
        else { Box.inShockRadius = false; }

        if (shockPrepared && shockCRActive == false)
        {
            StartCoroutine(Shock());
        }

        if (Box.pulseActive && EM.distanceToBox <= Box.pulseRadius + 0.5f && EM.inBoxLOS && shockPrepared)
        {
            shockPrepared = false;
            stage = 1;
            currentAngVel = regAngVel;
        }

        if (enemyHitstopActive == false && EM.enemyWasKilled == false)
        {
            enemyRB.angularVelocity = Mathf.MoveTowards(enemyRB.angularVelocity, Mathf.Sign(enemyRB.angularVelocity) * currentAngVel, 1500 * Time.deltaTime);
        }



        if (debugLines)
        {
            Debug.DrawLine(enemyRB.position + Vector2.up * thunderRadius, enemyRB.position + Vector2.down * thunderRadius);
            Debug.DrawLine(enemyRB.position + Vector2.left * thunderRadius, enemyRB.position + Vector2.right * thunderRadius);
        }
    }

    void FixedUpdate()
    {
        if (EM.enemyIsFrozen)
        {
            return;
        }

        float force = (enemyRB.position - currentPoint).magnitude * maxMoveForce / distAtoB;
        force = Mathf.Min(force, maxMoveForce);
        force = Mathf.Max(force, 5);
        if (Mathf.Abs(enemyRB.position.x - currentPoint.x) > 0.08f)
        {
            forceDirectionX = (int) Mathf.Sign(currentPoint.x - enemyRB.position.x);
        }
        if (Mathf.Abs(enemyRB.position.y - currentPoint.y) > 0.08f)
        {
            forceDirectionY = (int)Mathf.Sign(currentPoint.y - enemyRB.position.y);
        }
        if (addForce)
        {
            if ((enemyRB.position.x > pointB.x || enemyRB.position.x < pointA.x) && isStationary == false)
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * force * 1.5f);
            }
            else
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * force);
            }
            enemyRB.AddForce(Vector2.up * forceDirectionY * force);
        }

        //aggro
        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            thunderRadius *= EM.aggroIncreaseMult;
            thunderDamage *= EM.aggroIncreaseMult;
            timeToMove *= EM.aggroDecreaseMult;
            maxMoveForce *= EM.aggroIncreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive)
        {
            aggroActive = false;

            thunderRadius /= EM.aggroIncreaseMult;
            thunderDamage /= EM.aggroIncreaseMult;
            timeToMove /= EM.aggroDecreaseMult;
            maxMoveForce /= EM.aggroIncreaseMult;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Hazard")
        {
            EM.enemyWasDamaged = true;
        }
    }

    IEnumerator SwitchPoints()
    {
        addForce = false;
        float window = timeToMove * 0.94f + Random.Range(0, 0.12f);
        float timer = 0;
        while (timer < window)
        {
            if (EM.hitstopImpactActive == false && enemyHitstopActive == false && EM.enemyIsFrozen == false)
            {
                timer += Time.fixedDeltaTime;
            }
            if (timer > 0.25f)
            {
                addForce = true;
            }
            yield return new WaitForFixedUpdate();
        }
        if (isStationary == false && EM.enemyWasKilled == false)
        {
            if (currentPoint == pointA)
            {
                currentPoint = pointB;
            }
            else
            {
                currentPoint = pointA;
            }
            enemyRB.AddForce((currentPoint - enemyRB.position).normalized * 13 * distAtoB / initialDistAtoB, ForceMode2D.Impulse);
            enemyRB.angularVelocity *= -1;
        }
        if (EM.enemyWasKilled == false)
        {
            StartCoroutine(SwitchPoints());
        }

        //the event at each stage happens at timeToMove - 1f
        stage++;
        if (stage > 3) { stage = 1; }
        if (EM.enemyWasKilled == false)
        {
            if (stage == 1)
            {

            }
            else if (stage == 2)
            {
                yield return new WaitForSeconds(timeToMove - 1.5f);
                currentAngVel = shockAngVel;
                yield return new WaitForSeconds(0.5f);
                shockPrepared = true;
            }
            else if (stage == 3)
            {
                yield return new WaitForSeconds(timeToMove - 2f);
                if (stage == 3)
                {
                    currentAngVel = releaseAngVel;
                    stage++;
                }
                yield return new WaitForSeconds(1f);
                if (stage == 4 && EM.enemyWasKilled == false)
                {
                    if (shockPrepared)
                    {
                        StartCoroutine(lightning.GetComponent<Lightning>().LightningStrike(thunderRadius, 0, thunderDamage, enemyRB.position, true, gameObject));
                        StartCoroutine(ReleaseFlash());
                    }
                    enemyRB.angularVelocity *= 0.5f;
                    shockPrepared = false;
                    currentAngVel = regAngVel;
                }
            }
        }
    }
    IEnumerator EnemyHitstop()
    {
        enemyHitstopActive = true;
        Vector2 enemyHitstopVelocity = enemyRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        enemyRB.velocity = new Vector2(0, 0);
        enemyRB.angularVelocity /= enemyHitstopRotationSlowDown;
        enemyRB.isKinematic = true;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        enemyRB.isKinematic = false;
        enemyRB.angularVelocity *= enemyHitstopRotationSlowDown;
        enemyHitstopActive = false;
        enemyRB.velocity = enemyHitstopVelocity;
    }
    IEnumerator Shock()
    {
        shockCRActive = true;
        float window1 = 0.03f;
        StartCoroutine(ShockFlash());
        while (shockPrepared)
        {
            int i = Random.Range(0, 4);
            GameObject objectA = spikes[i];
            i++;
            if (i > 3) { i = 0; }
            GameObject objectB = spikes[i];

            Vector2 pointA = transform.position + (objectA.transform.position - transform.position) * 1.8f;
            Vector2 pointB = transform.position + (objectB.transform.position - transform.position) * 1.8f;

            newLightning = Instantiate(lightning);
            newLightning.GetComponent<Lightning>().pointA = pointA;
            newLightning.GetComponent<Lightning>().pointB = pointB;
            newLightning.GetComponent<Lightning>().pointRadius /= 2;
            newLightning.GetComponent<Lightning>().aestheticElectricity = true;

            yield return new WaitForSeconds(window1 + Random.Range(0, window1 * 10));
        }
        shockCRActive = false;
    }
    IEnumerator ShockFlash()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockPrepared && stage != 4)
        {
            foreach (SpriteRenderer item in enemyObjects)
            {
                item.color = Color.white;
            }
            yield return new WaitForSeconds(window2 + Random.Range(0f, window2));
            for (int i = 0; i < enemyObjects.Count; i++)
            {
                enemyObjects[i].color = enemyColors[i];
            }
            yield return new WaitForSeconds(window1 + Random.Range(0f, window1));
        }


        for (int i = 0; i < enemyObjects.Count; i++)
        {
            enemyObjects[i].color = enemyColors[i];
        }
    }
    IEnumerator ReleaseFlash()
    {
        releaseFlashCR = true;
        float window = 0.15f;
        foreach (SpriteRenderer item in enemyObjects)
        {
            item.color = Color.white;
        }
        yield return new WaitForSeconds(window + Random.Range(0f, window));
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            enemyObjects[i].color = enemyColors[i];
        }
        releaseFlashCR = false;
    }
}
