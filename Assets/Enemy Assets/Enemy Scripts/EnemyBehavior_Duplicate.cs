using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Duplicate : MonoBehaviour
{
    public Transform body;
    public Transform mask1;
    public Transform maskMid;
    public Transform mask2;
    public Transform fallingSand;

    LOSMeshGenerator meshGenerator;
    LineRenderer lr;

    public GameObject duplicate;
    GameObject newDuplicate;
    public GameObject energy;
    GameObject newEnergy;
    public float secondsBehind = 2f;
    bool duplicateSpawned = false;

    Rigidbody2D boxRB;
    EnemyManager EM;
    Rigidbody2D enemyRB;

    public float flipTime = 6;
    float flipTransitionTime = 1f;
    bool upright = true;
    bool flipTransition = false;
    float sandDeltaTime = 0;
    float heldRotation = 0;

    float maskStartPos = -0.65f;
    float maskEndPos = 0.14f;
    float maskIdlePos = -0.85f;

    float midStartPos = -0.225f;
    float midEndPos = -0.075f;
    float midStartScale = 0.45f;
    float midEndScale = 0.15f;

    float lineStartPos = -0.205f;
    float lineStartScale = 0.475f;
    float lineEndPos = -0.2f;
    float lineEndScale = 0.1f;

    public float damage = 25;
    public float radius = 3;

    Vector2 initialPosition;
    int forceDirectionY;
    int forceDirectionX;
    float floatVelocity = 3;
    float forceMagnitudeY = 10f;
    float forceMagnitudeX = 7f;

    int obstacleAndBoxLM;
    int obstacleLM;
    int boxLM;

    bool aggro = false;
    bool frozen = false;

    bool debugEnabled = false;


    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        enemyRB = GetComponent<Rigidbody2D>();
        EM = GetComponent<EnemyManager>();

        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        obstacleLM = LayerMask.GetMask("Obstacles");
        boxLM = LayerMask.GetMask("Box");

        flipTransition = true;

        initialPosition = enemyRB.position;
        enemyRB.velocity = Vector2.up * 3;

        EM.normalDeath = false;

        Color color = new Color(0, 0, 0);

        meshGenerator = GetComponent<LOSMeshGenerator>();
        meshGenerator.meshRenderer.material.color = new Color(color.r, color.g, color.b, 0.4f);
        meshGenerator.radius = radius;
        meshGenerator.meshRenderer.sortingLayerName = "Items";

        lr = GetComponent<LineRenderer>();
        lr.startColor = color;
        lr.endColor = color;
    }

    void FixedUpdate()
    {
        if (EM.enemyIsFrozen)
        {
            frozen = true;
            return;
        }
        if (EM.enemyIsFrozen == false && frozen)
        {
            frozen = false;
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
            enemyRB.velocity = new Vector2(enemyRB.velocity.x, -floatVelocity);
        }

        //floating logic X
        if (Mathf.Abs(enemyRB.position.x - initialPosition.x) <= 0.5f) { forceDirectionX = 0; }
        else { forceDirectionX = (int)-Mathf.Sign(enemyRB.position.x - initialPosition.x); }
        if (Mathf.Abs(enemyRB.position.x - initialPosition.x) >= 0.5f)
        {
            if ((forceDirectionX == -1 && enemyRB.velocity.x >= -floatVelocity) || (forceDirectionX == 1 && enemyRB.velocity.x <= floatVelocity))
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * forceMagnitudeX);
            }
        }

        if (debugEnabled)
        {
            Debug.DrawRay(initialPosition + Vector2.left, Vector2.right * 2);
        }
    }
    void Update()
    {
        //logic for player to physically kill enemy
        bool thisEnemyFound = false;
        bool touchingThisEnemy = false;
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
        if (touchingThisEnemy == true && Box.boxHitboxActive && EM.enemyIsInvulnerable == false)
        {
            EM.enemyWasDamaged = true;
            if (EM.enemyIsInvulnerable == false)
            {
                Box.activateHitstop = true;
            }
        }
        if (EM.enemyWasDamaged && duplicateSpawned && newDuplicate != null)
        {
            StartCoroutine(newDuplicate.GetComponent<Duplicate>().DamageFlicker());
        }

        if (EM.enemyIsFrozen)
        {
            sandDeltaTime = 0;
            if (duplicateSpawned == false)
            {
                meshGenerator.generate = false;
                lr.enabled = false;
            }
            else if (frozen == false)
            {
                heldRotation = enemyRB.rotation;
            }

            frozen = true;
            return;
        }
        else
        {
            sandDeltaTime = Time.deltaTime;
        }
        if (EM.enemyIsFrozen == false && frozen)
        {
            frozen = false;
            if (duplicateSpawned == false)
            {
                meshGenerator.generate = true;
                lr.enabled = true;
                enemyRB.rotation = 0;
            }
            else
            {
                enemyRB.rotation = heldRotation;
                StartCoroutine(SpawnDuplicate());
            }
        }

        if (duplicateSpawned == false)
        {
            Vector2 vectorToBox = (boxRB.position - enemyRB.position);
            RaycastHit2D circleCast = Physics2D.CircleCast(enemyRB.position, radius, Vector2.zero, 0, boxLM);
            if (Tools.LineOfSight(enemyRB.position, vectorToBox) && circleCast.collider != null)
            {
                EM.canSeeItem = true;
                initiateDuplicate();
            }
            else
            {
                EM.canSeeItem = false;
            }
        }

        if (EM.enemyWasKilled && EM.hitstopImpactActive == false)
        {
            for (int i = 0; i < 14; i++)
            {
                newEnergy = Instantiate(energy);
                newEnergy.transform.position = transform.position;
                newEnergy.transform.localScale *= 1.4f;
                newEnergy.GetComponent<DuplicateEnergy>().startPosition = Vector2.zero;
                newEnergy.GetComponent<DuplicateEnergy>().maxDist = 4;
                newEnergy.GetComponent<DuplicateEnergy>().inwards = false;
                newEnergy.GetComponent<DuplicateEnergy>().slow = true;
            }
            if (newDuplicate != null)
            {
                newDuplicate.GetComponent<Duplicate>().death();
            }
            Destroy(gameObject);
        }

        if (EM.aggroCurrentlyActive && aggro == false)
        {
            aggro = true;
            radius *= EM.aggroIncreaseMult;
            meshGenerator.radius = radius;
        }
        if (EM.aggroCurrentlyActive == false && aggro)
        {
            aggro = false;
            radius /= EM.aggroIncreaseMult;
            meshGenerator.radius = radius;
        }
    }
    private void LateUpdate()
    {
        meshGenerator.GenerateMesh();
    }
    void initiateDuplicate()
    {
        StartCoroutine(FlipGlass());
        StartCoroutine(SpawnDuplicate());
        StartCoroutine(MaskMovement1());
        StartCoroutine(MaskMovementMid());
        StartCoroutine(MaskMovement2());
        StartCoroutine(SandMovement());
        StartCoroutine(DisableArea());
    }
    void CreateEnergy()
    {
        newEnergy = Instantiate(energy);
        newEnergy.transform.position = transform.position;
        newEnergy.transform.localScale *= 1.2f;
        newEnergy.GetComponent<DuplicateEnergy>().parent = GetComponent<Rigidbody2D>();
        newEnergy.GetComponent<DuplicateEnergy>().startPosition = Vector2.zero;
        newEnergy.GetComponent<DuplicateEnergy>().maxDist = 4;
        if (aggro)
        {
            newEnergy.GetComponent<DuplicateEnergy>().maxDist = 5;
        }
        newEnergy.GetComponent<DuplicateEnergy>().inwards = false;
    }

    IEnumerator SpawnDuplicate()
    {
        duplicateSpawned = true;
        yield return new WaitForSeconds(0.2f);
        newDuplicate = Instantiate(duplicate, boxRB.position, Quaternion.identity);
        newDuplicate.GetComponent<Duplicate>().secondsBehind = secondsBehind;
        newDuplicate.GetComponent<Duplicate>().damage = damage;
        newDuplicate.GetComponent<Duplicate>().sourceEM = EM;
        float window = secondsBehind * 0.8f;
        float timer = 0;
        while (timer < window)
        {
            float window2 = 0.05f;
            float timer2 = 0;
            while (timer2 < window2)
            {
                timer += Time.deltaTime;
                timer2 += Time.deltaTime;
                yield return null;
            }
            CreateEnergy();

        }
        StartCoroutine(EnergyRelease());
    }
    IEnumerator EnergyRelease()
    {
        while (EM.initialDelay == false)
        {
            float window = 0.2f;
            if (aggro)
            {
                window = 0.15f;
            }
            float timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            CreateEnergy();
        }
    }

    IEnumerator MaskMovement1()
    {
        if (flipTransition)
        {
            float window = flipTransitionTime;
            float timer = 0;
            if (upright)
            {
                while (timer < window)
                {
                    if (EM.hitstopImpactActive == false)
                    {
                        timer += sandDeltaTime;
                    }
                    yield return null;
                }
                mask1.transform.localPosition = new Vector2(mask1.transform.localPosition.x, maskIdlePos);
            }
            else
            {
                while (timer < window)
                {
                    if (EM.hitstopImpactActive == false)
                    {
                        mask1.transform.localPosition = Vector2.MoveTowards(mask1.transform.localPosition, new Vector2(mask1.transform.localPosition.x, maskStartPos),
                        Mathf.Abs(maskIdlePos - maskStartPos) / window * sandDeltaTime);
                        timer += sandDeltaTime;
                    }
                    yield return null;
                }
            }
            flipTransition = false;
            upright = !upright;
        }
        else if (upright)
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                if (EM.hitstopImpactActive == false)
                {
                    mask1.transform.localPosition = Vector2.MoveTowards(mask1.transform.localPosition, new Vector2(mask1.transform.localPosition.x, maskEndPos),
                    Mathf.Abs(maskStartPos - maskEndPos) / window * sandDeltaTime);
                    timer += sandDeltaTime;
                }
                yield return null;
            }
            flipTransition = true;
            StartCoroutine(FlipGlass());
        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                if (EM.hitstopImpactActive == false)
                {
                    timer += sandDeltaTime;
                }
                yield return null;
            }
            flipTransition = true;
            StartCoroutine(FlipGlass());
        }
        yield return null;
        StartCoroutine(MaskMovement1());
        StartCoroutine(MaskMovement2());
        StartCoroutine(MaskMovementMid());
        StartCoroutine(SandMovement());
    }
    IEnumerator MaskMovementMid()
    {
        if (flipTransition)
        {
            float startpos = maskMid.localPosition.y;
            float targetPos = -midStartPos;
            if (upright == false)
            {
                targetPos = midStartPos;
            }


            while (flipTransition)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, targetPos),
                    Mathf.Abs(startpos - targetPos) / flipTransitionTime * sandDeltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midStartScale,
                    Mathf.Abs(midStartScale - midEndScale) / flipTransitionTime * sandDeltaTime));
                yield return null;
            }
        }
        else if (upright)
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, midEndPos),
                    Mathf.Abs(midStartPos - midEndPos) / window * sandDeltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midEndScale,
                    Mathf.Abs(midStartScale - midEndScale) / window * sandDeltaTime));
                timer += sandDeltaTime;
                yield return null;
            }
        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, -midEndPos),
                    Mathf.Abs(midStartPos - midEndPos) / window * sandDeltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midEndScale,
                    Mathf.Abs(midStartScale - midEndScale) / window * sandDeltaTime));
                timer += sandDeltaTime;
                yield return null;
            }
        }
    }
    IEnumerator MaskMovement2()
    {
        if (flipTransition)
        {
            float window = flipTransitionTime;
            float timer = 0;
            if (upright == false)
            {
                while (timer < window)
                {
                    timer += sandDeltaTime;
                    yield return null;
                }
                mask2.transform.localPosition = new Vector2(mask2.transform.localPosition.x, maskIdlePos);
            }
            else
            {
                while (timer < window)
                {
                    mask2.transform.localPosition = Vector2.MoveTowards(mask2.transform.localPosition, new Vector2(mask2.transform.localPosition.x, maskStartPos),
                        Mathf.Abs(maskIdlePos - maskStartPos) / window * sandDeltaTime);
                    timer += sandDeltaTime;
                    yield return null;
                }
            }
        }
        else if (upright)
        {

        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                mask2.transform.localPosition = Vector2.MoveTowards(mask2.transform.localPosition, new Vector2(mask2.transform.localPosition.x, maskEndPos),
                    Mathf.Abs(maskStartPos - maskEndPos) / window * sandDeltaTime);
                timer += sandDeltaTime;
                yield return null;
            }
        }
    }
    IEnumerator SandMovement()
    {
        float window = 0.1f;
        float timer = 0;
        if (flipTransition)
        {
            float startPos = fallingSand.transform.localPosition.y;
            if (upright == false)
            {
                float targetPos = -lineEndPos;
                while (timer < window)
                {
                    fallingSand.transform.localPosition = Vector2.MoveTowards(fallingSand.transform.localPosition, new Vector2(fallingSand.transform.localPosition.x, targetPos),
                        Mathf.Abs(startPos - targetPos) / window * sandDeltaTime);
                    fallingSand.localScale = new Vector2(fallingSand.localScale.x, Mathf.MoveTowards(fallingSand.localScale.y, lineEndScale,
                        Mathf.Abs(lineStartScale - lineEndScale) / window * sandDeltaTime));
                    timer += sandDeltaTime;
                    yield return null;
                }
            }
            else
            {
                float targetPos = lineEndPos;
                while (timer < window)
                {
                    fallingSand.transform.localPosition = Vector2.MoveTowards(fallingSand.transform.localPosition, new Vector2(fallingSand.transform.localPosition.x, targetPos),
                        Mathf.Abs(startPos - targetPos) / window * sandDeltaTime);
                    fallingSand.localScale = new Vector2(fallingSand.localScale.x, Mathf.MoveTowards(fallingSand.localScale.y, lineEndScale,
                        Mathf.Abs(lineStartScale - lineEndScale) / window * sandDeltaTime));
                    timer += sandDeltaTime;
                    yield return null;
                }
            }
        }
        else if (upright)
        {
            float targetPos = lineStartPos;
            float startPos = fallingSand.transform.localPosition.y;
            while (timer < window)
            {
                fallingSand.transform.localPosition = Vector2.MoveTowards(fallingSand.transform.localPosition, new Vector2(fallingSand.transform.localPosition.x, targetPos),
                    Mathf.Abs(startPos - targetPos) / window * sandDeltaTime);
                fallingSand.localScale = new Vector2(fallingSand.localScale.x, Mathf.MoveTowards(fallingSand.localScale.y, lineStartScale,
                    Mathf.Abs(lineStartScale - lineEndScale) / window * sandDeltaTime));
                timer += sandDeltaTime;
                yield return null;
            }
        }
        else
        {
            float targetPos = -lineStartPos;
            float startPos = fallingSand.transform.localPosition.y;
            while (timer < window)
            {
                fallingSand.transform.localPosition = Vector2.MoveTowards(fallingSand.transform.localPosition, new Vector2(fallingSand.transform.localPosition.x, targetPos),
                    Mathf.Abs(startPos - targetPos) / window * sandDeltaTime);
                fallingSand.localScale = new Vector2(fallingSand.localScale.x, Mathf.MoveTowards(fallingSand.localScale.y, lineStartScale,
                    Mathf.Abs(lineStartScale - lineEndScale) / window * sandDeltaTime));
                timer += sandDeltaTime;
                yield return null;
            }
        }
        yield return null;
    }
    IEnumerator FlipGlass()
    {
        float window = flipTransitionTime;
        float timer = 0;
        float target = 180;
        if (upright == false)
        {
            target = 360;
        }
        while (timer < window)
        {
            if (EM.hitstopImpactActive == false)
            {
                body.eulerAngles = new Vector3(body.eulerAngles.x, body.eulerAngles.y,
                    Mathf.MoveTowards(body.eulerAngles.z, target, 180 / flipTransitionTime * sandDeltaTime));
                timer += sandDeltaTime;
            }
            yield return null;
        }
        if (target == 360)
        {
            body.eulerAngles = new Vector3(body.eulerAngles.x, body.eulerAngles.y, 0);
        }
    }

    IEnumerator DisableArea()
    {
        Color areaColor = meshGenerator.meshRenderer.material.color;
        while (meshGenerator.meshRenderer.material.color.a > 0.01f)
        {
            areaColor.a -= Time.deltaTime / 3;
            meshGenerator.meshRenderer.material.color = areaColor;
            lr.startColor = areaColor;
            lr.endColor = areaColor;
            yield return null;
        }
        meshGenerator.generate = false;
    }
}
