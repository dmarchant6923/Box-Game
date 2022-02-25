using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Wizard : MonoBehaviour
{
    Color wizardColor;

    Vector2 initialPosition;

    Rigidbody2D enemyRB;
    Rigidbody2D boxRB;
    EnemyManager EM;

    RaycastHit2D obstacleCheckRC;

    int forceDirectionY;
    int forceDirectionX;
    float floatVelocity = 4;
    float forceMagnitudeY = 10f;
    float forceMagnitudeX = 13f;
    float spinMultiplier = 300;
    bool spinIncrease = false;

    float trueAngularVelocity;

    float truePositionX;
    int truePositionXDirection;
    float truePositionXDisplacement = 20;
    float truePositionXVelocity = 3f;

    bool touchingThisEnemy = false;

    int obstacleLM;
    int enemyLM;
    int boxLM;
    int obstacleAndBoxLM;

    public bool debugEnabled = false;

    public bool shield = true;
    public bool pulse = false;

    Color shieldColor;
    Color pulseColor;

    float pulseMagnitude = 20;

    public GameObject aura;
    GameObject newAura;
    GameObject thisAura;
    Color thisAuraColor;
    Color initialAuraColor;
    bool auraCheck = false;
    float auraCheckColorSpeed = 2;
    float auraRadius = 5.5f;
    public float auraCheckPeriod = 3.5f;
    bool checkThisFrame = false;
    bool enemyFadeActive = false;
    List<GameObject> spawnedAuras = new List<GameObject>();

    float pulseDetectRadius = 8;
    bool pulseCheckSuccess = false;

    void Start()
    {
        spawnedAuras.Clear();
        wizardColor = GetComponent<SpriteRenderer>().material.color;

        obstacleLM = LayerMask.GetMask("Obstacles", "Hazards");
        enemyLM = LayerMask.GetMask("Enemies");
        boxLM = LayerMask.GetMask("Box");
        obstacleAndBoxLM = LayerMask.GetMask("Box", "Obstacles");

        enemyRB = gameObject.GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        EM = gameObject.GetComponent<EnemyManager>();

        initialPosition = enemyRB.position;
        truePositionX = initialPosition.x;
        enemyRB.velocity = Vector2.up * 3;

        truePositionXDirection = Random.Range(0, 2);
        if (truePositionXDirection == 0) { truePositionXDirection = -1; }

        thisAura = Instantiate(aura, transform);

        shieldColor = new Color(1, 0, 0.92f);
        pulseColor = new Color(0.5f, 0.3f, 0.92f);
        if (shield)
        {
            thisAuraColor = shieldColor;
        }
        else if (pulse)
        {
            thisAuraColor = pulseColor;
        }
        thisAuraColor.a = 0.1f;
        thisAura.GetComponent<SpriteRenderer>().color = thisAuraColor;
        initialAuraColor = thisAuraColor;

        thisAura.transform.localScale = Vector2.one * auraRadius * 2;

        if (shield)
        {
            EM.canReceiveShield = false;
        }
    }

    private void FixedUpdate()
    {
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
        if (Mathf.Abs(enemyRB.position.x - truePositionX) <= 1) { forceDirectionX = 0; }
        else { forceDirectionX = (int)-Mathf.Sign(enemyRB.position.x - truePositionX); }
        if (Mathf.Abs(enemyRB.position.x - truePositionX) >= 3)
        {
            if ((forceDirectionX == -1 && enemyRB.velocity.x >= -floatVelocity) || (forceDirectionX == 1 && enemyRB.velocity.x <= floatVelocity))
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * forceMagnitudeX);
            }
        }
        else
        {
            if ((forceDirectionX == -1 && enemyRB.velocity.x >= truePositionXVelocity) || (forceDirectionX == 1 && enemyRB.velocity.x <= truePositionXVelocity))
            {
                enemyRB.AddForce(Vector2.right * forceDirectionX * forceMagnitudeX / 4);
            }
        }

        //moving truePositionX
        truePositionX += truePositionXVelocity * truePositionXDirection * Time.deltaTime;
        if (truePositionX >= initialPosition.x + truePositionXDisplacement && truePositionXDirection == 1)
        {
            truePositionXDirection = -1;
        }
        if (truePositionX <= initialPosition.x - truePositionXDisplacement && truePositionXDirection == -1)
        {
            truePositionXDirection = 1;
        }

        //changing truePositionXDirection if an obstacle is detected, turning around if so
        obstacleCheckRC = Physics2D.BoxCast(new Vector2(truePositionX + transform.lossyScale.x * 4f * truePositionXDirection,
            initialPosition.y), new Vector2(transform.lossyScale.x / 4, transform.lossyScale.y), 0, Vector2.down, 0f, obstacleLM);
        if (obstacleCheckRC.collider != null)
        {
            truePositionXDirection *= -1;
        }

        //magical spinning
        if (spinIncrease == false)
        {
            trueAngularVelocity = -Mathf.Sqrt(Mathf.Abs(enemyRB.velocity.x)) * Mathf.Sign(enemyRB.velocity.x) * spinMultiplier;
        }
        float spinAccel = 2000;
        if (pulse) { spinAccel *= 3; }
        enemyRB.angularVelocity = Mathf.MoveTowards(enemyRB.angularVelocity, trueAngularVelocity, spinAccel * Time.deltaTime);
    }
    void Update()
    {
        //logic for player to physically kill enemy
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
        if (touchingThisEnemy && Box.boxHitboxActive && EM.enemyWasDamaged == false)
        {
            EM.enemyWasDamaged = true;
            if (EM.enemyIsInvulnerable == false)
            {
                Box.activateHitstop = true;
            }
        }
        if (EM.enemyWasKilled == true && enemyFadeActive == false)
        {
            StartCoroutine(EnemyFade());
            thisAura.GetComponent<Aura>().breakShield = true;
            foreach (GameObject aura in spawnedAuras)
            {
                if (aura != null)
                {
                    aura.GetComponent<Aura>().breakShield = true;
                    aura.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive = false;
                }
            }
        }

        if (auraCheck == false && shield)
        {
            StartCoroutine(AuraCheck());
        }
        if (pulse)
        {
            EM.canSeeItem = false;
            pulseCheckSuccess = false;
            RaycastHit2D[] pulseDetectCast = Physics2D.CircleCastAll(enemyRB.position, pulseDetectRadius, Vector2.zero, 0, LayerMask.GetMask("Box", "Enemies", "Enemy Device", "Projectiles"));
            if (pulseDetectCast.Length > 0)
            {
                foreach (RaycastHit2D item in pulseDetectCast)
                {
                    bool checkSuccessful = false;
                    if ((item.transform.GetComponent<BulletScript>() != null && item.transform.GetComponent<BulletScript>().bulletWasReflected) ||
                        (item.transform.GetComponent<Grenade>() != null && item.transform.GetComponent<Grenade>().grenadeWasReflected == true) ||
                        (item.transform.GetComponent<EnemyBehavior_Flying>() != null && item.transform.GetComponent<EnemyBehavior_Flying>().diveWasReflected == true) ||
                        (item.transform.GetComponent<EnemyBehavior_Grounded>() != null && item.transform.GetComponent<EnemyBehavior_Grounded>().willDamageEnemies == true) ||
                        (item.transform.GetComponent<Box>() != null))
                    {
                        checkSuccessful = true;
                    }

                    Vector2 itemPosition = item.transform.position;
                    RaycastHit2D rayToItem = Physics2D.Raycast(enemyRB.position, (itemPosition - enemyRB.position).normalized, pulseDetectRadius, obstacleLM);
                    if ((rayToItem.collider == null || (rayToItem.collider != null && (itemPosition - enemyRB.position).magnitude < rayToItem.distance)) && checkSuccessful)
                    {
                        pulseCheckSuccess = true;
                        EM.canSeeItem = true;
                    }
                }
            }
            if (pulseCheckSuccess && auraCheck == false)
            {
                StartCoroutine(AuraCheck());
            }
        }
        if (checkThisFrame == true && shield == true)
        {
            RaycastHit2D[] auraRC = Physics2D.CircleCastAll(enemyRB.position, auraRadius, Vector2.zero, 0f, enemyLM);
            foreach (RaycastHit2D enemy in auraRC)
            {
                GameObject parentEnemy;
                if (enemy.collider.gameObject == this.gameObject)
                {
                    continue;
                }

                if (enemy.collider.transform.root.GetComponent<EnemyManager>() != null && enemy.collider.GetComponent<Rigidbody2D>() != null)
                {
                    parentEnemy = enemy.collider.gameObject;
                }
                else
                {
                    continue;
                }
                if (parentEnemy.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive == false 
                    && parentEnemy.transform.root.GetComponent<EnemyManager>().canReceiveShield == true)
                {
                    parentEnemy.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive = true;
                    newAura = Instantiate(aura, parentEnemy.transform.root.GetComponent<EnemyManager>().enemyRB.position, Quaternion.identity);
                    newAura.GetComponent<Renderer>().material.color = shieldColor;
                    newAura.transform.localScale = Mathf.Max(parentEnemy.transform.localScale.x, parentEnemy.transform.localScale.y) * Vector2.one * 1.8f;
                    newAura.transform.parent = parentEnemy.transform;
                    spawnedAuras.Add(newAura);
                }
            }
        }
        if (checkThisFrame == true && pulse == true)
        {
            //box
            RaycastHit2D rayToBox = Physics2D.Raycast(enemyRB.position, (boxRB.position - enemyRB.position).normalized, auraRadius, obstacleAndBoxLM);
            if (rayToBox.collider != null && 1 << rayToBox.collider.gameObject.layer == boxLM)
            {
                Box.boxWasPulsed = true;
                Box.boxEnemyPulseDirection = (boxRB.position - enemyRB.position).normalized;
                Box.boxEnemyPulseMagnitude = pulseMagnitude;
            }

            RaycastHit2D[] pulse = Physics2D.CircleCastAll(enemyRB.position, auraRadius, Vector2.zero, 0f, LayerMask.GetMask("Projectiles", "Enemy Device", "Enemies"));
            foreach (RaycastHit2D item in pulse)
            {
                //bullet
                if (item.transform.GetComponent<BulletScript>() != null && item.transform.GetComponent<BulletScript>().bulletWasReflected == true)
                {
                    BulletScript bullet = item.transform.GetComponent<BulletScript>();
                    Rigidbody2D bulletRB = item.transform.GetComponent<Rigidbody2D>();
                    bullet.bulletWasReflected = false;
                    Vector2 enemyReflectVector = (bulletRB.position - enemyRB.position).normalized;
                    bulletRB.velocity = enemyReflectVector * bulletRB.velocity.magnitude * Box.projectilePulseMagnitude;
                    bullet.bulletDamage *= 1.2f;
                }

                //grenade
                if (item.transform.GetComponent<Grenade>() != null && item.transform.GetComponent<Grenade>().grenadeWasReflected == true)
                {
                    Grenade grenade = item.transform.GetComponent<Grenade>();
                    Rigidbody2D grenadeRB = item.transform.GetComponent<Rigidbody2D>();
                    grenade.grenadeWasReflected = false;
                    Vector2 enemyReflectVector = (grenadeRB.position - enemyRB.position).normalized;
                    grenadeRB.velocity = enemyReflectVector * grenadeRB.velocity.magnitude * Box.projectilePulseMagnitude;
                }

                //kamikaze bird
                if (item.transform.GetComponent<EnemyBehavior_Flying>() != null && item.transform.GetComponent<EnemyBehavior_Flying>().diveWasReflected == true)
                {
                    Rigidbody2D birdRB = item.transform.GetComponent<Rigidbody2D>();
                    Vector2 enemyReflectVector = (birdRB.position - enemyRB.position).normalized;
                    birdRB.velocity = enemyReflectVector * birdRB.velocity.magnitude;
                }

                //grounded enemy
                if (item.transform.GetComponent<EnemyBehavior_Grounded>() != null && item.transform.GetComponent<EnemyBehavior_Grounded>().willDamageEnemies == true)
                {
                    Rigidbody2D RB = item.transform.GetComponent<Rigidbody2D>();
                    Vector2 enemyReflectVector = (RB.position - enemyRB.position).normalized;
                    RB.velocity = enemyReflectVector * RB.velocity.magnitude;
                }
            }
        }



        //debug lines
        if (debugEnabled)
        {
            Debug.DrawRay(new Vector2(truePositionX - 1, initialPosition.y), Vector2.right * 2);

            if (obstacleCheckRC.collider != null)
            {
                Debug.DrawRay(new Vector2(truePositionX + transform.lossyScale.x * 4 * truePositionXDirection,
                    (initialPosition.y) - transform.lossyScale.y / 2), Vector2.up * transform.lossyScale.y, Color.red);
                Debug.DrawRay(new Vector2((truePositionX + transform.lossyScale.x * 4 * truePositionXDirection) - transform.lossyScale.x / 8,
                    initialPosition.y), Vector2.right * transform.lossyScale.x / 4, Color.red);
            }
            else
            {
                Debug.DrawRay(new Vector2(truePositionX + transform.lossyScale.x * 4 * truePositionXDirection,
                    (initialPosition.y) - transform.lossyScale.y / 2), Vector2.up * transform.lossyScale.y, Color.white);
                Debug.DrawRay(new Vector2((truePositionX + transform.lossyScale.x * 4 * truePositionXDirection) - transform.lossyScale.x / 8,
                    initialPosition.y), Vector2.right * transform.lossyScale.x / 4, Color.white);
            }

            if (pulse)
            {
                if (pulseCheckSuccess == false)
                {
                    Debug.DrawRay(enemyRB.position + Vector2.left * pulseDetectRadius, Vector2.right * pulseDetectRadius * 2, Color.white);
                    Debug.DrawRay(enemyRB.position + Vector2.down * pulseDetectRadius, Vector2.up * pulseDetectRadius * 2, Color.white);
                }
                else
                {
                    Debug.DrawRay(enemyRB.position + Vector2.left * pulseDetectRadius, Vector2.right * pulseDetectRadius * 2, Color.red);
                    Debug.DrawRay(enemyRB.position + Vector2.down * pulseDetectRadius, Vector2.up * pulseDetectRadius * 2, Color.red);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Hazards"))
        {
            GetComponent<EnemyManager>().enemyWasDamaged = true;
        }
    }
    IEnumerator EnemyFade()
    {
        enemyFadeActive = true;
        yield return new WaitForSeconds(Box.enemyHitstopDelay);
        while (wizardColor.a >= 0)
        {
            wizardColor.a -= Time.deltaTime * 1.5f;
            this.GetComponent<SpriteRenderer>().material.color = wizardColor;
            yield return null;
        }
    }
    IEnumerator AuraCheck()
    {
        auraCheck = true;
        while (EM.initialDelay)
        {
            yield return null;
        }
        float timer = 0;
        float warmUpWindow = 0.8f;
        if (pulse) { warmUpWindow = 0.3f; }
        float direction = Mathf.Sign(enemyRB.angularVelocity);
        while (timer <= warmUpWindow)
        {
            spinIncrease = true;
            trueAngularVelocity = 4000 * direction;
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        checkThisFrame = true;
        if (EM.enemyWasKilled == false)
        {
            StartCoroutine(AuraColor());
        }
        yield return null;
        checkThisFrame = false;
        enemyRB.angularVelocity /= 4;
        spinIncrease = false;
        timer = 0;
        float remainderWindow = auraCheckPeriod - warmUpWindow + Random.Range(0, 0.2f);
        while (timer <= remainderWindow)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        auraCheck = false;
    }
    IEnumerator AuraColor()
    {
        thisAuraColor = initialAuraColor;
        thisAuraColor.a = 1;
        thisAura.GetComponent<SpriteRenderer>().color = thisAuraColor;
        float timer = 0;
        while (thisAuraColor.a >= 0.1f && EM.enemyWasKilled == false)
        {
            thisAuraColor.a -= auraCheckColorSpeed * Time.deltaTime;
            thisAura.GetComponent<SpriteRenderer>().color = thisAuraColor;
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        thisAuraColor.a = 0.1f;
    }
}