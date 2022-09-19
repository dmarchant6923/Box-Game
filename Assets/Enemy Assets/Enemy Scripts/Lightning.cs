using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    // Start is called before the first frame update
    Vector2[] basePoints;
    Vector3[] points;
    [HideInInspector] public Vector2 pointA;
    [HideInInspector] public Vector2 pointB;
    [HideInInspector] public bool movingTarget = false;
    [HideInInspector] public Vector2 movingTargetOffset;
    [HideInInspector] public Rigidbody2D targetRB;
    Rigidbody2D boxRB;
    [System.NonSerialized] public LineRenderer line;
    [System.NonSerialized] public LineRenderer smallLine;

    public GameObject lightning;
    GameObject newLightning;

    [HideInInspector] public static float baseThunderDamage = 40;
    [HideInInspector] public float thunderDamage = 40;
    public bool aestheticElectricity = false;
    float chainRadius = 5;
    float chainDelay = 0.4f;
    bool chainFinished = false;

    [System.NonSerialized] public float pointsPerUnit = 1.656f;
    [System.NonSerialized] public float pointRadius = 0.3f;
    int numPoints;

    [System.NonSerialized] public float fadeSpeed = 1.5f;
    [System.NonSerialized] public float width = 0.12f;

    [HideInInspector] public static float contactDamage = 20;

    int boxLM;
    int platformLM;
    int enemyLM;
    int obstacleLM;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        smallLine = transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        if (aestheticElectricity)
        {
            width = 0.08f;
            if (pointsPerUnit == 1.656f)
            {
                pointsPerUnit = 5f;
            }
            fadeSpeed = 20;
            line.sortingOrder = 1;
            smallLine.sortingOrder = 2;
            movingTarget = false;
        }

        if (movingTarget)
        {
            pointB = targetRB.position + movingTargetOffset;
        }
        float dist = (pointA - pointB).magnitude;
        numPoints = (int)Mathf.Max(Mathf.Floor(dist * pointsPerUnit), 2);
        basePoints = new Vector2[numPoints];
        points = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            basePoints[i] = pointA + (pointB - pointA) * i / numPoints;
            if (i != 0 && i != points.Length - 1)
            {
                points[i] = basePoints[i] + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * pointRadius;
            }
            else if (i == 0)
            {
                points[i] = pointA;
            }
            else if (i == points.Length - 1)
            {
                points[i] = pointB;
            }
        }
        StartCoroutine(DrawLine());
        transform.position = Vector3.zero;

        line.startWidth = width;
        smallLine.startWidth = width * 0.4f;

        boxLM = LayerMask.GetMask("Box");
        platformLM = LayerMask.GetMask("Platforms");
        enemyLM = LayerMask.GetMask("Enemies");
        obstacleLM = LayerMask.GetMask("Obstacles");
    }

    // Update is called once per frame
    void Update()
    {

    }

    //process:
    //Lightning is initialized and given points A and B by outside source (thunder guy, or another lightning).
    //DrawLine() draws the thunder from point A to point B.
    //once point B is drawn, StrikeCheck() is called to determine if there is an object within a small radius at point B eligible to be shocked.
    //for every object eligible to be shocked, the object is shocked in its own script and LightningStrike(...) is called.
    //LightningStrike(...) takes the shocked object's position and determines if there are other objects in a radius that are eligible to be shocked.
    //For each eligible object it finds, it instantiates a new lightning bolt with this point B as its point A, and the other object's position as its point B.

    IEnumerator DrawLine()
    {
        int maxBranches = (int)Mathf.Floor(numPoints / 5);
        int branches = 0;
        bool touchedWall = false;
        for (int i = 0; i < numPoints; i++)
        {
            if (i == points.Length - 1 && movingTarget)
            {
                points[i] = targetRB.position + movingTargetOffset + targetRB.velocity * Time.fixedDeltaTime;
                pointB = points[i];
            }
            if (i > 0 && aestheticElectricity == false)
            {
                Physics2D.queriesStartInColliders = false;
                RaycastHit2D wallCheck = Physics2D.Linecast(points[i - 1], points[i], obstacleLM);
                Physics2D.queriesStartInColliders = true;
                int attempts = 0;
                if (i != points.Length - 1)
                {
                    while (wallCheck.collider != null && attempts < 4)
                    {
                        points[i] = basePoints[i] + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * pointRadius * 2;
                        wallCheck = Physics2D.Linecast(points[i - 1], points[i], obstacleLM);
                        attempts++;
                    }
                }
                if (wallCheck.collider != null)
                {
                    touchedWall = true;
                    points[i] = wallCheck.point;
                    pointB = points[i];
                    if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
                    {
                        CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
                        camScript.startCamShake = true;
                        camScript.shakeInfo = new Vector2(thunderDamage,
                            new Vector2(pointB.x - boxRB.position.x, pointB.y - boxRB.position.y).magnitude);
                    }
                }
            }

            line.positionCount = i + 1;
            line.SetPosition(i, points[i]);
            smallLine.positionCount = i + 1;
            smallLine.SetPosition(i, points[i]);
            if (touchedWall)
            {
                break;
            }

            if (i % 2 == 0)
            {
                yield return new WaitForFixedUpdate();
            }
            if (branches < maxBranches && i !=0 && i < points.Length - 3 && aestheticElectricity == false)
            {
                int rand = Random.Range(0, numPoints / maxBranches);
                if (rand == 0)
                {
                    newLightning = Instantiate(lightning);
                    newLightning.GetComponent<Lightning>().pointA = line.GetPosition(i);
                    newLightning.GetComponent<Lightning>().movingTarget = false;
                    Vector3 v3 = pointB;
                    newLightning.GetComponent<Lightning>().pointB = line.GetPosition(i) + ((v3 - line.GetPosition(i)).normalized + 
                        new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f))).normalized / 2 * (numPoints - i) * 0.6f / pointsPerUnit;
                    branches++;
                }
            }
        }
        if (aestheticElectricity == false && movingTarget)
        {
            StrikeCheck();
        }
        else
        {
            chainFinished = true;
        }
        Color color = line.startColor;
        Color smallColor = smallLine.startColor;
        color.a = 1;
        smallColor.a = 1;
        while (color.a > 0)
        {
            color.a -= fadeSpeed * Time.deltaTime;
            smallColor.a -= fadeSpeed * Time.deltaTime;
            line.startColor = color;
            line.endColor = color;
            smallLine.startColor = smallColor;
            smallLine.endColor = smallColor;
            yield return null;
        }
        while (chainFinished == false)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
    void StrikeCheck()
    {
        float strikeRadius = 0.1f; //0.1f
        bool strike = false;
        RaycastHit2D boxCheck = Physics2D.CircleCast(pointB, strikeRadius, Vector2.zero, 0, boxLM);
        RaycastHit2D[] enemyCheck = Physics2D.CircleCastAll(pointB, strikeRadius, Vector2.zero, 0, enemyLM);
        RaycastHit2D platformCheck = Physics2D.CircleCast(pointB, strikeRadius, Vector2.zero, 0, platformLM);
        RaycastHit2D fenceCheck = Physics2D.CircleCast(pointB, strikeRadius, Vector2.zero, 0, obstacleLM);

        //Debug.DrawRay(pointB, Vector2.up * strikeRadius);
        //Debug.DrawRay(pointB, Vector2.right * strikeRadius);

        if (boxCheck.collider != null 
            && Box.shockActive == false && Box.isInvulnerable == false)
        {
            Box.damageTaken = thunderDamage;
            Box.boxDamageDirection = new Vector2(Mathf.Sign((pointB - pointA).normalized.x), 1).normalized;
            Box.activateDamage = true;
            Box.activateShock = true;
            StartCoroutine(LightningStrike(chainRadius, chainDelay, thunderDamage, pointB, false, boxCheck.collider.gameObject));
            strike = true;
        }
        foreach (RaycastHit2D item in enemyCheck)
        {
            if (item.collider != null
                    && item.transform.root.gameObject.GetComponent<EnemyManager>() != null
                    && item.collider.gameObject.GetComponent<Rigidbody2D>() != null
                    && item.transform.root.gameObject.GetComponent<EnemyManager>().shockActive == false
                    && item.transform.root.gameObject.GetComponent<EnemyManager>().activateShock == false
                    && item.transform.root.gameObject.GetComponent<EnemyManager>().canBeShocked == true
                    && item.transform.root.gameObject.GetComponent<EnemyManager>().shockCoolDown == false
                    && item.transform.root.gameObject.GetComponent<EnemyManager>().enemyWasKilled == false)
            {
                item.transform.root.gameObject.GetComponent<EnemyManager>().activateShock = true;
                StartCoroutine(LightningStrike(chainRadius, chainDelay, thunderDamage, pointB, false, item.collider.gameObject));
                strike = true;
            }
            else if (item.collider != null && item.transform.root.gameObject.GetComponent<HitSwitch>() != null
                && item.transform.root.gameObject.GetComponent<HitSwitch>().canBeShocked
                && item.transform.root.gameObject.GetComponent<HitSwitch>().activateShock == false)
            {
                item.transform.root.gameObject.GetComponent<HitSwitch>().activateShock = true;
                strike = true;
            }
        }
        if (platformCheck.collider != null
            && platformCheck.collider.gameObject.GetComponent<PlatformDrop>() != null
            && platformCheck.collider.gameObject.GetComponent<PlatformDrop>().shockActive == false)
        {
            platformCheck.collider.gameObject.GetComponent<PlatformDrop>().activateShock = true;
            StartCoroutine(LightningStrike(chainRadius, chainDelay, thunderDamage, pointB, false, platformCheck.collider.gameObject));
            strike = true;
        }
        if (fenceCheck.collider != null
            && fenceCheck.collider.gameObject.GetComponent<Fence>() != null
            && fenceCheck.collider.gameObject.GetComponent<Fence>().shockActive == false)
        {
            fenceCheck.collider.gameObject.GetComponent<Fence>().activateShock = true;
            StartCoroutine(LightningStrike(chainRadius, chainDelay, thunderDamage, pointB, false, fenceCheck.collider.gameObject));
            strike = true;
        }

        float distToBox = new Vector2(pointB.x - boxRB.position.x, pointB.y - boxRB.position.y).magnitude;
        if (strike && GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null && distToBox < 30)
        {
            CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
            camScript.startCamShake = true;
            camScript.shakeInfo = new Vector2(thunderDamage * 0.4f,
                new Vector2(pointB.x - boxRB.position.x, pointB.y - boxRB.position.y).magnitude);
        }
        if (strike == false)
        {
            chainFinished = true;
        }
    }
    public IEnumerator LightningStrike(float radius, float delay, float damage, Vector2 startPoint, bool firstStrike, GameObject sourceObject)
    {
        delay += Random.Range(-0.05f, 0.05f);
        Rigidbody2D boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        yield return new WaitForSeconds(delay);
        if (sourceObject != null && (
            sourceObject.transform.root.GetComponent<EnemyBehavior_Thunder>() != null || sourceObject.transform.root.GetComponent<TeslaCoil>() != null ||
            (sourceObject.transform.root.GetComponent<EnemyManager>() != null && sourceObject.transform.root.GetComponent<EnemyManager>().shockActive) ||
            (sourceObject.transform.root.GetComponent<PlatformDrop>() != null && sourceObject.transform.root.GetComponent<PlatformDrop>().shockActive) ||
            (sourceObject.transform.root.GetComponent<Fence>() != null && sourceObject.transform.root.GetComponent<Fence>().shockActive) ||
            (sourceObject.transform.root.GetComponent<Box>() != null && Box.shockActive)))
        {
            //startPoint = sourceObject.transform.position;
            RaycastHit2D[] lightningCheck = Physics2D.CircleCastAll(startPoint, radius, Vector2.zero, 0,
                LayerMask.GetMask("Box", "Platforms", "Enemies", "Obstacles"));
            if (sourceObject.transform.root.GetComponent<PlatformDrop>() != null)
            {
                lightningCheck = Physics2D.CircleCastAll(startPoint + Vector2.left * sourceObject.transform.root.localScale.x / 2, radius, Vector2.right,
                    sourceObject.transform.root.localScale.x, LayerMask.GetMask("Box", "Platforms", "Enemies", "Obstacles"));
            }

            int strikes = 0;
            int boxLM = LayerMask.GetMask("Box");
            int enemyLM = LayerMask.GetMask("Enemies");
            int platformLM = LayerMask.GetMask("Platforms");
            int obstacleLM = LayerMask.GetMask("Obstacles");
            float strikeDelay = 0.05f;

            foreach (RaycastHit2D item in lightningCheck)
            {
                if (1 << item.collider.gameObject.layer == boxLM && Box.shockActive == false && Box.isInvulnerable == false)
                {
                    newLightning = Instantiate(lightning);
                    newLightning.GetComponent<Lightning>().pointA = sourceObject.transform.position;
                    newLightning.GetComponent<Lightning>().targetRB = item.collider.GetComponent<Rigidbody2D>();
                    newLightning.GetComponent<Lightning>().pointB = item.collider.GetComponent<Rigidbody2D>().position;
                    newLightning.GetComponent<Lightning>().movingTargetOffset = Vector2.zero;
                    newLightning.GetComponent<Lightning>().movingTarget = true;
                    newLightning.GetComponent<Lightning>().thunderDamage = damage;
                    strikes++;
                    yield return new WaitForSeconds(strikeDelay);
                    break;
                }
            }


            if (lightningCheck.Length > 1)
            {
                foreach (RaycastHit2D item in lightningCheck)
                {
                    Vector2 transformPosition = item.transform.position;
                    bool success = false;
                    Rigidbody2D targetRB = null;
                    if (sourceObject != null && item.collider.gameObject == sourceObject)
                    {

                    }
                    else if (1 << item.collider.gameObject.layer == enemyLM && item.transform.root.gameObject.GetComponent<EnemyManager>() != null
                        && item.collider.gameObject.GetComponent<Rigidbody2D>() != null
                        && item.transform.root.gameObject.GetComponent<EnemyManager>().shockActive == false
                        && item.transform.root.gameObject.GetComponent<EnemyManager>().canBeShocked == true
                        && item.transform.root.gameObject.GetComponent<EnemyManager>().shockCoolDown == false
                        && item.transform.root.gameObject.GetComponent<EnemyManager>().enemyWasKilled == false)
                    {
                        success = true;
                        targetRB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                    }
                    else if (item.transform.root.GetComponent<HitSwitch>() != null && item.transform.root.GetComponent<Rigidbody2D>() != null
                        && item.transform.root.GetComponent<HitSwitch>().canBeShocked == true)
                    {
                        success = true;
                        targetRB = item.transform.root.GetComponent<Rigidbody2D>();
                    }
                    else if (1 << item.collider.gameObject.layer == platformLM && item.collider.gameObject.GetComponent<PlatformDrop>() != null
                        && item.collider.gameObject.GetComponent<PlatformDrop>().shockActive == false)
                    {
                        success = true;
                        targetRB = item.collider.GetComponent<Rigidbody2D>();
                    }
                    else if (1 << item.collider.gameObject.layer == obstacleLM && item.collider.gameObject.GetComponent<Fence>() != null
                        && item.collider.gameObject.GetComponent<Fence>().shockActive == false)
                    {
                        success = true;
                        targetRB = item.collider.GetComponent<Rigidbody2D>();
                    }

                    if (success)
                    {
                        newLightning = Instantiate(lightning);
                        Vector2 newPointA = sourceObject.transform.position;
                        if (sourceObject.transform.root.GetComponent<PlatformDrop>() != null)
                        {
                            if (targetRB.position.x > sourceObject.transform.position.x + sourceObject.transform.localScale.x / 2)
                            {
                                newPointA = sourceObject.transform.position + Vector3.right * sourceObject.transform.localScale.x / 2;
                            }
                            if (targetRB.position.x < sourceObject.transform.position.x - sourceObject.transform.localScale.x / 2)
                            {
                                newPointA = sourceObject.transform.position - Vector3.right * sourceObject.transform.localScale.x / 2;
                            }
                        }
                        newLightning.GetComponent<Lightning>().pointA = newPointA;
                        newLightning.GetComponent<Lightning>().pointB = targetRB.position;
                        if (targetRB.transform.root.GetComponent<PlatformDrop>() != null)
                        {
                            if (targetRB.position.x - targetRB.transform.localScale.x / 2 > newPointA.x)
                            {
                                newLightning.GetComponent<Lightning>().movingTargetOffset = Vector2.left * targetRB.transform.localScale.x * 0.45f;
                            }
                            if (targetRB.position.x + targetRB.transform.localScale.x / 2 < newPointA.x)
                            {
                                newLightning.GetComponent<Lightning>().movingTargetOffset = Vector2.right * targetRB.transform.localScale.x * 0.45f;
                            }
                        }
                        else
                        {
                            newLightning.GetComponent<Lightning>().movingTargetOffset = Vector2.zero;
                        }

                        newLightning.GetComponent<Lightning>().movingTarget = true;
                        newLightning.GetComponent<Lightning>().targetRB = targetRB;
                        newLightning.GetComponent<Lightning>().thunderDamage = damage;
                        yield return new WaitForSeconds(strikeDelay);
                        strikes++;
                    }
                }
            }
            if (firstStrike && strikes == 0)
            {
                RaycastHit2D floorCheck = Physics2D.Raycast(startPoint, Vector2.down, radius,
                    LayerMask.GetMask("Obstacles"));
                if (floorCheck.collider != null)
                {
                    newLightning = Instantiate(lightning);
                    newLightning.GetComponent<Lightning>().movingTarget = false;
                    newLightning.GetComponent<Lightning>().pointA = startPoint;
                    newLightning.GetComponent<Lightning>().pointB = floorCheck.point;

                    float distToBox = new Vector2(floorCheck.point.x - boxRB.position.x, floorCheck.point.y - boxRB.position.y).magnitude;
                    if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null && distToBox < 30)
                    {
                        CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
                        camScript.startCamShake = true;
                        camScript.shakeInfo = new Vector2(thunderDamage,
                            new Vector2(pointB.x - boxRB.position.x, pointB.y - boxRB.position.y).magnitude);
                    }
                }
                else
                {
                    int numBolts = 15;
                    float releaseRadius = chainRadius * 0.8f;
                    int boltsFired = 0;
                    float distToBox = (sourceObject.transform.root.GetComponentInChildren<Rigidbody2D>().position - boxRB.position).magnitude;
                    int quadrant = 1;
                    if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null && distToBox < 30)
                    {
                        CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
                        camScript.startCamShake = true;
                        camScript.shakeInfo = new Vector2(thunderDamage * 0.4f, (pointB - boxRB.position).magnitude);
                    }
                    while (boltsFired < numBolts)
                    {
                        Vector2 randVector;
                        if (quadrant == 1) { randVector = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)).normalized; }
                        else if (quadrant == 2) { randVector = new Vector2(Random.Range(-1f, 0f), Random.Range(0f, 1f)).normalized; }
                        else if (quadrant == 3) { randVector = new Vector2(Random.Range(-1f, 0f), Random.Range(-1f, 0f)).normalized; }
                        else { randVector = new Vector2(Random.Range(0, 1f), Random.Range(-1f, 0f)).normalized; }

                        Vector2 randPoint = startPoint + randVector * releaseRadius;
                        newLightning = Instantiate(lightning);
                        newLightning.GetComponent<Lightning>().movingTarget = false;
                        newLightning.GetComponent<Lightning>().pointRadius *= 1.5f;
                        newLightning.GetComponent<Lightning>().pointsPerUnit *= 1.5f;
                        newLightning.GetComponent<Lightning>().pointA = startPoint;
                        newLightning.GetComponent<Lightning>().pointB = randPoint;
                        yield return new WaitForFixedUpdate();
                        boltsFired++;
                        quadrant++; if (quadrant > 4) { quadrant = 1; }
                    }
                }
            }
        }
        chainFinished = true;
    }
}
