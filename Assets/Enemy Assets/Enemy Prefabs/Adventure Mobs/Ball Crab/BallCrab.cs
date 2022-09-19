using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCrab : MonoBehaviour
{
    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    EnemyManager EM;

    Transform leftFront;
    Transform rightFront;
    Transform leftBack;
    Transform rightBack;

    Transform balledFront;
    Transform balledBack;

    Transform[] legs = new Transform[4];
    Transform[] balledLegs = new Transform[2];

    float moveSpeed = 1.5f;
    float stepDistance = 0.25f;
    float stepHeight = 0.07f;
    float platformMoveSpeed = 0;

    bool isGrounded = false;
    float notGroundedTimer = 0;
    float notGroundedWindow = 1f;

    bool idleCR = false;
    bool walking = true;
    bool walkCR = false;
    bool walkingRight = true;

    bool touchingThisEnemy = false;
    float launchSpeed = 20;
    bool justLaunched = false;
    bool hitboxActive = false;
    bool enemyHitstopActive = false;
    bool extendHitstop = false;

    bool balled = false;
    bool balledCR = false;

    int obstacleLM;
    int groundLM;
    int enemyLM;

    public bool debugEnabled = false;

    private void Start()
    {
        enemyRB = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        EM = GetComponent<EnemyManager>();
        EM.invulnerabilityPeriod = 0.4f;
        EM.enemyWillFlash = false;

        leftFront = transform.GetChild(0);
        rightFront = transform.GetChild(1);
        leftBack = transform.GetChild(2);
        rightBack = transform.GetChild(3);

        balledFront = transform.GetChild(4);
        balledBack = transform.GetChild(5);

        legs = new Transform[4] { leftFront, rightFront, leftBack, rightBack };
        balledLegs = new Transform[2] { balledFront, balledBack };

        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Platforms", "Obstacles");
        enemyLM = LayerMask.GetMask("Enemies");
    }

    void GroundCheck()
    {
        RaycastHit2D groundCast = Physics2D.BoxCast(enemyRB.position + Vector2.down * 0.35f * transform.localScale.x, new Vector2(0.25f, 0.05f), 0, Vector2.zero, 0, groundLM);
        if (groundCast.collider != null && Tools.AngleToVector(transform.eulerAngles.z).y > 0.8f && balled == false)
        {
            isGrounded = true;
            notGroundedTimer = 0;
            if (idleCR == false)
            {
                StartCoroutine(IdleCR());
            }
        }
        else
        {
            notGroundedTimer += Time.deltaTime;
        }

        if (notGroundedTimer > notGroundedWindow)
        {
            isGrounded = false;
            balled = true;
            notGroundedTimer = 0;
        }
    }

    private void Update()
    {
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
        if (touchingThisEnemy == true && Box.boxHitboxActive && EM.enemyIsInvulnerable == false)
        {
            EM.enemyWasDamaged = true;
            StartCoroutine(Damaged(boxRB.velocity, Box.dashActive));
            balled = true;
            Box.activateHitstop = true;
        }

        if (notGroundedTimer == 0 && isGrounded && balled == false)
        {
            enemyRB.rotation = 0;
            enemyRB.freezeRotation = true;
        }
        else
        {
            enemyRB.freezeRotation = false;
        }

        if (debugEnabled)
        {
            if (isGrounded)
            {
                GetComponent<SpriteRenderer>().color = Color.green;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.gray;
            }
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
        if (balled)
        {
            isGrounded = false;
            if (balledCR == false)
            {
                StartCoroutine(Balled());
            }
        }

        if (walkCR && notGroundedTimer == 0 && balled == false)
        {
            if (walkingRight && enemyRB.velocity.x < moveSpeed + platformMoveSpeed)
            {
                enemyRB.AddForce(Vector2.right * 40);
                RaycastHit2D rightWallCast = Physics2D.BoxCast(enemyRB.position + new Vector2(1, 0.5f), Vector2.one * 0.5f, 0, Vector2.zero, 0, obstacleLM);
                RaycastHit2D rightFloorCast = Physics2D.BoxCast(enemyRB.position + new Vector2(1.5f, -0.5f), Vector2.one * 0.25f, 0, Vector2.zero, 0, obstacleLM);
                if (rightWallCast.collider != null || rightFloorCast.collider == null)
                {
                    walking = false;
                }
            }
            if (walkingRight == false && enemyRB.velocity.x > -moveSpeed + platformMoveSpeed)
            {
                enemyRB.AddForce(Vector2.left * 40);
                RaycastHit2D leftWallCast = Physics2D.BoxCast(enemyRB.position + new Vector2(-1, 0.5f), Vector2.one * 0.5f, 0, Vector2.zero, 0, obstacleLM);
                RaycastHit2D leftFloorCast = Physics2D.BoxCast(enemyRB.position + new Vector2(-1.5f, -0.5f), Vector2.one * 0.25f, 0, Vector2.zero, 0, obstacleLM);
                if (leftWallCast.collider != null || leftFloorCast.collider == null)
                {
                    walking = false;
                }
            }
        }

        if (balled && (enemyRB.velocity.magnitude > launchSpeed / 2 || enemyHitstopActive))
        {
            hitboxActive = true;
            //RaycastHit2D[] enemyCast = Physics2D.CircleCastAll(enemyRB.position, transform.localScale.x * 0.4f, Vector2.zero, 0, enemyLM);
            //foreach (RaycastHit2D enemy in enemyCast)
            //{
            //    EnemyManager em = enemy.collider.GetComponent<EnemyManager>();
            //    if (em != null && em.enemyIsInvulnerable == false && em.gameObject != gameObject)
            //    {
            //        em.enemyWasDamaged = true;
            //        if (enemyHitstopActive == false)
            //        {
            //            StartCoroutine(EnemyHitstop());
            //        }
            //        else
            //        {
            //            extendHitstop = true;
            //        }
            //    }
            //}
        }
        else
        {
            hitboxActive = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<MovingObjects>() != null && collision.transform.GetComponent<MovingObjects>().interactableObstacle && 
            balled && collision.GetContact(0).normal.y > 0.8f && justLaunched == false)
        {
            enemyRB.velocity = new Vector2(enemyRB.velocity.x * 0.8f, enemyRB.velocity.y);
        }

        if (hitboxActive && collision.gameObject.GetComponent<EnemyManager>() != null && collision.gameObject.GetComponent<EnemyManager>().enemyIsInvulnerable == false)
        {
            collision.gameObject.GetComponent<EnemyManager>().enemyWasDamaged = true;
            if (enemyHitstopActive == false)
            {
                StartCoroutine(EnemyHitstop());
            }
            else
            {
                extendHitstop = true;
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.transform.GetComponent<MovingObjects>() != null && collision.transform.GetComponent<MovingObjects>().interactableObstacle && 
            walkCR && collision.GetContact(0).normal.y > 0.8f)
        {
            platformMoveSpeed = collision.transform.GetComponent<Rigidbody2D>().velocity.x;
        }
    }

    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    if (hitboxActive && collision.GetComponent<EnemyManager>() != null)
    //    {
    //        foreach(Collider2D collider in GetComponentsInChildren<Collider2D>())
    //        {
    //            if (collider.isTrigger == false)
    //            {
    //                Physics2D.IgnoreCollision(collider, collision, true);
    //            }
    //        }
    //    }
    //    if (hitboxActive == false && collision.GetComponent<EnemyManager>() != null)
    //    {
    //        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
    //        {
    //            if (collider.isTrigger == false)
    //            {
    //                Physics2D.IgnoreCollision(collider, collision, false);
    //            }
    //        }
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (collision.GetComponent<EnemyManager>() != null)
    //    {
    //        foreach (Collider2D collider in GetComponentsInChildren<Collider2D>())
    //        {
    //            if (collider.isTrigger == false)
    //            {
    //                Physics2D.IgnoreCollision(collider, collision, false);
    //            }
    //        }
    //    }
    //}

    IEnumerator IdleCR()
    {
        idleCR = true;
        float walkTime = 4f;
        float waitTime = 2f;
        yield return new WaitForSeconds(waitTime);
        bool firstWalk = true;
        while (notGroundedTimer == 0)
        {
            RaycastHit2D rightWallCast = Physics2D.BoxCast(enemyRB.position + new Vector2(1, 0.5f), Vector2.one * 0.5f, 0, Vector2.zero, 0, obstacleLM);
            RaycastHit2D rightFloorCast = Physics2D.BoxCast(enemyRB.position + new Vector2(1.5f, -0.5f), Vector2.one * 0.25f, 0, Vector2.zero, 0, obstacleLM);
            RaycastHit2D leftWallCast = Physics2D.BoxCast(enemyRB.position + new Vector2(-1, 0.5f), Vector2.one * 0.5f, 0, Vector2.zero, 0, obstacleLM);
            RaycastHit2D leftFloorCast = Physics2D.BoxCast(enemyRB.position + new Vector2(1.5f, -0.5f), Vector2.one * 0.25f, 0, Vector2.zero, 0, obstacleLM);
            int rand = Random.Range(-1, 2);
            if (firstWalk)
            {
                rand = (Random.Range(0, 2) * 2) - 1;
            }
            if ((rightWallCast.collider != null || rightFloorCast.collider == null) && (leftWallCast.collider == null && leftFloorCast.collider != null))
            {
                rand = Random.Range(-1, 1);
            }
            else if ((rightWallCast.collider == null && rightFloorCast.collider != null) && (leftWallCast.collider != null || leftFloorCast.collider == null))
            {
                rand = Random.Range(0, 2);
            }
            else if ((rightWallCast.collider != null || rightFloorCast.collider == null) && (leftWallCast.collider != null || leftFloorCast.collider == null))
            {
                rand = 0;
            }



            if (rand == -1)
            {
                walking = true;
                walkingRight = false;
                StartCoroutine(Walk());
            }
            else if (rand == 1)
            {
                walking = true;
                walkingRight = true;
                StartCoroutine(Walk());
            }
            yield return new WaitForSeconds(walkTime * 0.25f + Random.Range(0f, walkTime));
            walking = false;
            while (walkCR)
            {
                yield return null;
            }
            yield return new WaitForSeconds(waitTime * 0.25f + Random.Range(0f, waitTime));
        }
        idleCR = false;
    }
    IEnumerator Walk()
    {
        walkCR = true;
        Transform[] step1 = new Transform[2] { rightFront, leftBack };
        Transform[] step2 = new Transform[2] { rightBack, leftFront };
        Vector2[] step1InitialPos = new Vector2[2] { step1[0].localPosition, step1[1].localPosition };

        float stepWindow = stepDistance * transform.localScale.x / moveSpeed; //0.3f;

        while (walking && notGroundedTimer == 0)
        {
            //step 1
            float timer = 0;
            while (timer < stepWindow && walking && notGroundedTimer == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    step1[i].localPosition = Vector2.MoveTowards(step1[i].localPosition, step1InitialPos[i] + (Vector2.left * stepDistance), (stepDistance / stepWindow) * Time.deltaTime);
                    step2[i].localPosition = Vector2.MoveTowards(step2[i].localPosition, step1InitialPos[i], (stepDistance / stepWindow) * Time.deltaTime);

                    float height = stepHeight * Mathf.Sin(timer / stepWindow * Mathf.PI);
                    if (walkingRight)
                    {
                        step1[i].localPosition = new Vector2(step1[i].localPosition.x, step1InitialPos[0].y - height);
                    }
                    else
                    {
                        step2[i].localPosition = new Vector2(step2[i].localPosition.x, step1InitialPos[0].y - height);
                    }

                }
                timer += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < 2; i++)
            {
                step1[i].localPosition = step1InitialPos[i] + (Vector2.left * stepDistance);
                step2[i].localPosition = step1InitialPos[i];
            }

            //step 2
            timer = 0;
            while (timer < stepWindow && walking && notGroundedTimer == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    step1[i].localPosition = Vector2.MoveTowards(step1[i].localPosition, step1InitialPos[i], (stepDistance / stepWindow) * Time.deltaTime);
                    step2[i].localPosition = Vector2.MoveTowards(step2[i].localPosition, step1InitialPos[i] + (Vector2.left * stepDistance), (stepDistance / stepWindow) * Time.deltaTime);

                    float height = stepHeight * Mathf.Sin(timer / stepWindow * Mathf.PI);
                    if (walkingRight == false)
                    {
                        step1[i].localPosition = new Vector2(step1[i].localPosition.x, step1InitialPos[0].y - height);
                    }
                    else
                    {
                        step2[i].localPosition = new Vector2(step2[i].localPosition.x, step1InitialPos[0].y - height);
                    }
                }
                timer += Time.deltaTime;
                yield return null;
            }

            for (int i = 0; i < 2; i++)
            {
                step1[i].localPosition = step1InitialPos[i];
                step2[i].localPosition = step1InitialPos[i] + (Vector2.left * stepDistance);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            step1[i].localPosition = step1InitialPos[i];
            step2[i].localPosition = step1InitialPos[i] + (Vector2.left * stepDistance);
        }
        walkCR = false;
    }
    IEnumerator Damaged(Vector2 boxVelocity, bool dash)
    {
        yield return null;
        while (EM.hitstopImpactActive)
        {
            yield return null;
        }
        justLaunched = true;
        enemyRB.velocity = new Vector2(boxVelocity.x / 20, Mathf.Max(boxVelocity.normalized.y, 0.8f)).normalized * launchSpeed;
        if (dash)
        {
            enemyRB.velocity = new Vector2(Box.dashDirection, 1).normalized * launchSpeed;
        }
        float mult = 1;
        if (dash)
        {
            mult *= 1.4f;
        }
        if (BoxPerks.speedActive || BoxPerks.starActive || BoxPerks.spikesActive)
        {
            mult *= 1.3f;
        }
        enemyRB.velocity *= mult;
        yield return new WaitForFixedUpdate();
        enemyRB.angularVelocity = -Mathf.Sign(boxVelocity.x) * 1000;
        enemyRB.angularVelocity *= mult;
        yield return new WaitForSeconds(0.2f);
        justLaunched = false;


    }
    IEnumerator Balled()
    {
        balled = true;
        balledCR = true;
        walking = false;
        while (EM.hitstopImpactActive)
        {
            yield return null;
        }

        foreach (Transform leg in legs)
        {
            leg.gameObject.SetActive(false);
        }
        foreach(Transform balledLeg in balledLegs)
        {
            balledLeg.gameObject.SetActive(true);
        }

        float stillTimer = 0;
        float stillWindow = 1f;
        while (true)
        {
            if (Mathf.Abs(enemyRB.velocity.y) < 0.1f && enemyHitstopActive == false)
            {
                stillTimer += Time.deltaTime;
            }
            else
            {
                stillTimer = 0;
            }

            if (stillTimer > stillWindow)
            {
                break;
            }

            yield return null;
        }
        Vector2 vector = Tools.AngleToVector(enemyRB.rotation);
        if (vector.y < 0.8f)
        {
            enemyRB.velocity = Vector2.up * 8f;
            float spinMagnitude = 330 * vector.y - 1;
            float spinDirection = -Mathf.Sign(vector.x);
            enemyRB.angularVelocity = spinMagnitude * spinDirection;
        }
        else
        {
            enemyRB.velocity = Vector2.up * 6f;
        }

        foreach (Transform leg in legs)
        {
            leg.gameObject.SetActive(true);
        }
        foreach (Transform balledLeg in balledLegs)
        {
            balledLeg.gameObject.SetActive(false);
        }
        balled = false;
        balledCR = false;
        notGroundedTimer = 0;
    }
    IEnumerator EnemyHitstop()
    {
        enemyHitstopActive = true;
        Vector2 enemyHitstopVelocity = enemyRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        enemyRB.velocity = new Vector2(0, 0);
        enemyRB.angularVelocity /= enemyHitstopRotationSlowDown;
        enemyRB.isKinematic = true;
        float window = Box.enemyHitstopDelay;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            if (extendHitstop)
            {
                timer = 0;
                extendHitstop = false;
            }
            yield return null;
        }
        extendHitstop = false;
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        enemyRB.isKinematic = false;
        enemyRB.angularVelocity *= enemyHitstopRotationSlowDown;
        enemyRB.velocity = enemyHitstopVelocity;
        enemyHitstopActive = false;
    }
}
