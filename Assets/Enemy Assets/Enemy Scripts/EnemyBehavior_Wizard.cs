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
    MeshRenderer meshRenderer;
    LOSMeshGenerator meshGenerator;

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
    public bool aggro = false;

    Color shieldColor;
    Color pulseColor;
    Color aggroColor;

    float pulseMagnitude = 20;

    public GameObject aura;
    public GameObject gradientAura;
    public GameObject ringAura;
    GameObject newAura;
    Color thisAuraColor;
    Color initialAuraColor;
    bool auraCheck = false;
    float auraCheckColorSpeed = 1.4f;
    public float auraRadius = 5.5f;
    public float auraCheckPeriod = 3.5f;
    bool checkThisFrame = false;
    bool enemyFadeActive = false;
    List<GameObject> spawnedAuras = new List<GameObject>();

    float pulseDetectRadius = 8;
    bool pulseCheckSuccess = false;

    bool aggroActive = false;
    public GameObject fire;
    GameObject newFire;
    GameObject wizardFire;

    void Start()
    {
        spawnedAuras.Clear();
        wizardColor = GetComponent<SpriteRenderer>().material.color;

        obstacleLM = LayerMask.GetMask("Obstacles");
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

        shieldColor = new Color(1, 0, 0.92f);
        pulseColor = new Color(0.5f, 0.3f, 0.92f);
        aggroColor = new Color(1, 0, 0.2f);
        if (shield)
        {
            thisAuraColor = shieldColor;
        }
        else if (pulse)
        {
            thisAuraColor = pulseColor;
        }
        else if (aggro)
        {
            thisAuraColor = aggroColor;
            wizardFire = Instantiate(fire, enemyRB.position, Quaternion.identity);
            wizardFire.GetComponent<Fire>().objectOnFire = enemyRB;
        }
        initialAuraColor = thisAuraColor;

        if (shield)
        {
            EM.canReceiveShield = false;
        }
        if (aggro)
        {
            EM.canReceiveAggro = false;
        }

        thisAuraColor.a = 0.1f;

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshGenerator = GetComponent<LOSMeshGenerator>();
        meshGenerator.radius = auraRadius;
        meshRenderer.material.color = new Color(thisAuraColor.r, thisAuraColor.g, thisAuraColor.b, 0.2f);
    }

    private void FixedUpdate()
    {
        if (aggro)
        {
            foreach (GameObject aura in spawnedAuras)
            {
                if (aura == null || aura.transform.root.GetComponent<EnemyManager>() == null || aura.transform.root.GetComponent<EnemyManager>().aggroCurrentlyActive == false)
                {
                    if (aura != null && aura.GetComponent<Aura>() != null)
                    {
                        aura.GetComponent<Aura>().breakAura = true;
                    }
                    spawnedAuras.Remove(aura);
                    break;
                }
            }
        }

        if (EM.enemyIsFrozen)
        {
            meshGenerator.generate = false;
            //if (thisAura.GetComponent<SpriteRenderer>().enabled)
            //{
            //    thisAura.GetComponent<SpriteRenderer>().enabled = false;
            //    if (aggro)
            //    {
            //        wizardFire.GetComponent<Fire>().stopFire = true;
            //    }
            //    //lr.enabled = false;
            //}
            return;
        }
        else if (meshGenerator.generate == false)
        {
            meshGenerator.generate = true;
            if (aggro)
            {
                wizardFire = Instantiate(fire, enemyRB.position, Quaternion.identity);
                wizardFire.GetComponent<Fire>().objectOnFire = enemyRB;
            }
            //lr.enabled = true;
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

        if (EM.aggroCurrentlyActive && aggroActive == false)
        {
            aggroActive = true;

            auraRadius *= EM.aggroIncreaseMult;
            meshGenerator.radius = auraRadius;
            auraCheckPeriod *= EM.aggroDecreaseMult;

            truePositionXVelocity *= EM.aggroIncreaseMult;
            forceMagnitudeX *= EM.aggroIncreaseMult;

            pulseDetectRadius *= EM.aggroIncreaseMult;
            pulseMagnitude *= EM.aggroIncreaseMult;
        }
        if (EM.aggroCurrentlyActive == false && aggroActive == true)
        {
            aggroActive = false;

            auraRadius /= EM.aggroIncreaseMult;
            meshGenerator.radius = auraRadius;
            auraCheckPeriod /= EM.aggroDecreaseMult;

            truePositionXVelocity /= EM.aggroIncreaseMult;
            forceMagnitudeX /= EM.aggroIncreaseMult;

            pulseDetectRadius /= EM.aggroIncreaseMult;
            pulseMagnitude /= EM.aggroIncreaseMult;
        }
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
            meshRenderer.enabled = false;
            foreach (GameObject aura in spawnedAuras)
            {
                if (aura != null)
                {
                    if (aura.GetComponent<Aura>() != null)
                    {
                        aura.GetComponent<Aura>().breakAura = true;
                    }

                    if (shield)
                    {
                        aura.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive = false;
                    }
                    if (aggro)
                    {
                        aura.transform.root.GetComponent<EnemyManager>().aggroCurrentlyActive = false;
                        if (wizardFire != null)
                        {
                            wizardFire.GetComponent<Fire>().stopFire = true;
                        }
                    }
                }
            }
        }

        if (EM.enemyIsFrozen)
        {
            return;
        }

        if (auraCheck == false && (shield || aggro))
        {
            StartCoroutine(AuraCheck());
        }
        if (pulse)
        {
            EM.canSeeItem = false;
            pulseCheckSuccess = false;
            Physics2D.queriesHitTriggers = true;
            RaycastHit2D[] pulseDetectCast = Physics2D.CircleCastAll(enemyRB.position, pulseDetectRadius, Vector2.zero, 0, LayerMask.GetMask("Box", "Enemies", "Enemy Device", "Projectiles"));
            Physics2D.queriesHitTriggers = false;
            if (pulseDetectCast.Length > 0)
            {
                foreach (RaycastHit2D item in pulseDetectCast)
                {
                    bool checkSuccessful = false;
                    if ((item.transform.GetComponent<BulletScript>() != null && item.transform.GetComponent<BulletScript>().bulletWasReflected) ||
                        (item.transform.GetComponent<Grenade>() != null && item.transform.GetComponent<Grenade>().grenadeWasReflected == true) ||
                        (item.transform.GetComponent<EnemyBehavior_Flying>() != null && item.transform.GetComponent<EnemyBehavior_Flying>().diveWasReflected == true) ||
                        (item.transform.GetComponent<EnemyBehavior_Grounded>() != null && item.transform.GetComponent<EnemyBehavior_Grounded>().willDamageEnemies == true) ||
                        (item.transform.GetComponent<SpikeSentinel>() != null && item.transform.GetComponent<SpikeSentinel>().reflectActive) ||
                        (item.transform.GetComponent<StarManProjectile>() != null && item.transform.GetComponent<StarManProjectile>().projectileWasReflected) ||
                        (item.transform.GetComponent<Box>() != null))
                    {
                        checkSuccessful = true;
                    }

                    Vector2 itemPosition = item.transform.position;
                    float obstacleDist = 1000;
                    RaycastHit2D[] rayToItem = Physics2D.RaycastAll(enemyRB.position, (itemPosition - enemyRB.position).normalized, pulseDetectRadius, obstacleLM);
                    foreach (RaycastHit2D col in rayToItem)
                    {
                        if (col.collider.gameObject.tag != "Fence")
                        {
                            obstacleDist = Mathf.Min(obstacleDist, col.distance);
                        }
                    }

                    if ((itemPosition - enemyRB.position).magnitude < obstacleDist && checkSuccessful)
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
        if (checkThisFrame && (shield || aggro))
        {
            RaycastHit2D[] auraRC = Physics2D.CircleCastAll(enemyRB.position, auraRadius, Vector2.zero, 0f, enemyLM);
            foreach (RaycastHit2D enemy in auraRC)
            {
                GameObject parentEnemy;
                if (enemy.collider.gameObject == gameObject)
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

                Vector2 enemyPosition = enemy.transform.position;
                float obstacleDist = 1000;
                bool success = false;
                RaycastHit2D[] rayToItem = Physics2D.RaycastAll(enemyRB.position, (enemyPosition - enemyRB.position).normalized, pulseDetectRadius, obstacleLM);
                foreach (RaycastHit2D col in rayToItem)
                {
                    if (col.collider.gameObject.tag != "Fence")
                    {
                        obstacleDist = Mathf.Min(obstacleDist, col.distance);
                    }
                }

                if ((enemyPosition - enemyRB.position).magnitude < obstacleDist)
                {
                    success = true;
                }

                if (shield && success && parentEnemy.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive == false
                    && parentEnemy.transform.root.GetComponent<EnemyManager>().canReceiveShield)
                {
                    parentEnemy.transform.root.GetComponent<EnemyManager>().shieldCurrentlyActive = true;
                    newAura = Instantiate(ringAura, parentEnemy.transform.root.GetComponent<EnemyManager>().enemyRB.position, Quaternion.identity);
                    newAura.transform.localScale = Mathf.Max(parentEnemy.transform.localScale.x, parentEnemy.transform.localScale.y) * Vector2.one * 1.9f;
                    newAura.transform.parent = parentEnemy.transform;
                    newAura.GetComponent<SpriteRenderer>().color = new Color(shieldColor.r, shieldColor.g, shieldColor.b, 0.5f);
                    newAura.transform.localPosition = Vector2.zero;
                    newAura.GetComponent<Aura>().shield = true;
                    spawnedAuras.Add(newAura);
                }
                else if (aggro && success && parentEnemy.transform.root.GetComponent<EnemyManager>().aggroCurrentlyActive == false
                    && parentEnemy.transform.root.GetComponent<EnemyManager>().canReceiveAggro)
                {
                    parentEnemy.transform.root.GetComponent<EnemyManager>().aggroCurrentlyActive = true;
                    newAura = Instantiate(gradientAura, parentEnemy.transform.root.GetComponent<EnemyManager>().enemyRB.position, Quaternion.identity);
                    newAura.transform.localScale = Mathf.Max(parentEnemy.transform.localScale.x, parentEnemy.transform.localScale.y) * Vector2.one * 1.7f;
                    newAura.transform.parent = parentEnemy.transform;
                    newAura.GetComponent<SpriteRenderer>().color = new Color(aggroColor.r, aggroColor.g, aggroColor.b, 0.4f);
                    newAura.transform.localPosition = Vector2.zero;
                    newAura.GetComponent<Aura>().aggro = true;
                    spawnedAuras.Add(newAura);

                    //parentEnemy.transform.root.GetComponent<EnemyManager>().aggroCurrentlyActive = true;
                    //newFire = Instantiate(fire, parentEnemy.transform.root.GetComponent<EnemyManager>().enemyRB.position, Quaternion.identity);
                    //newFire.GetComponent<Fire>().objectOnFire = parentEnemy.transform.root.GetComponent<EnemyManager>().enemyRB;
                    //spawnedAuras.Add(newFire);
                }
            }
        }
        if (checkThisFrame == true && pulse == true)
        {
            Physics2D.queriesHitTriggers = true;
            RaycastHit2D[] pulse = Physics2D.CircleCastAll(enemyRB.position, auraRadius, Vector2.zero, 0f, LayerMask.GetMask("Projectiles", "Enemy Device", "Enemies", "Box"));
            Physics2D.queriesHitTriggers = false;
            foreach (RaycastHit2D item in pulse)
            {
                float itemDist = new Vector2(item.transform.position.x - enemyRB.position.x, item.transform.position.y - enemyRB.position.y).magnitude;
                float obstacleDist = 1000;
                Vector2 directionToItem = new Vector2(item.transform.position.x - enemyRB.position.x, item.transform.position.y - enemyRB.position.y).normalized;
                RaycastHit2D[] ray = Physics2D.RaycastAll(enemyRB.position, directionToItem, auraRadius, obstacleLM);
                foreach (RaycastHit2D point in ray)
                {
                    if (point.collider.gameObject.tag != "Fence")
                    {
                        obstacleDist = Mathf.Min(point.distance,obstacleDist);
                    }
                }
                if (itemDist < obstacleDist)
                {
                    //box
                    if (item.transform.GetComponent<Box>() != null)
                    {
                        Box.boxWasPulsed = true;
                        Box.boxEnemyPulseDirection = (boxRB.position - enemyRB.position).normalized;
                        Box.boxEnemyPulseMagnitude = pulseMagnitude;
                    }
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

                    //sentry sentinel
                    if (item.transform.GetComponent<SpikeSentinel>() != null && item.transform.GetComponent<SpikeSentinel>().reflectActive)
                    {
                        Rigidbody2D RB = item.transform.GetComponent<Rigidbody2D>();
                        Vector2 enemyReflectVector = (RB.position - enemyRB.position).normalized;
                        RB.velocity = enemyReflectVector * RB.velocity.magnitude * 1.2f;
                        item.transform.GetComponent<SpikeSentinel>().reflectTimer = 0;
                        item.transform.GetComponent<SpikeSentinel>().willDamageEnemies = false;
                        item.transform.GetComponent<SpikeSentinel>().damage *= 1.2f;
                    }

                    //starman projectile
                    if (item.transform.GetComponent<StarManProjectile>() != null && item.transform.GetComponent<StarManProjectile>().projectileWasReflected)
                    {
                        Rigidbody2D RB = item.transform.GetComponent<Rigidbody2D>();
                        Vector2 enemyReflectVector = (RB.position - enemyRB.position).normalized;
                        RB.velocity = enemyReflectVector * RB.velocity.magnitude * 1.2f;
                        item.transform.GetComponent<StarManProjectile>().projectileWasReflected = false;
                        item.transform.GetComponent<StarManProjectile>().timer = 0;
                    }
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

    private void LateUpdate()
    {
        meshGenerator.GenerateMesh();
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
        while (timer <= warmUpWindow && EM.enemyIsFrozen == false)
        {
            spinIncrease = true;
            trueAngularVelocity = 4000 * direction;
            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        spinIncrease = false;
        if (EM.enemyWasKilled == false && EM.enemyIsFrozen == false)
        {
            StartCoroutine(AuraColor());
            checkThisFrame = true;
            yield return null;
            checkThisFrame = false;
            enemyRB.angularVelocity /= 4;
            timer = 0;
            float remainderWindow = auraCheckPeriod - warmUpWindow + Random.Range(0, 0.2f);
            while (timer <= remainderWindow && EM.enemyIsFrozen == false)
            {
                timer += Time.deltaTime;
                yield return null;
            }
        }
        auraCheck = false;
    }
    IEnumerator AuraColor()
    {
        yield return null;
        thisAuraColor = initialAuraColor;
        thisAuraColor.a = 0.8f;
        meshRenderer.material.color = thisAuraColor;

        float timer = 0;
        while (thisAuraColor.a >= 0.1f && EM.enemyWasKilled == false)
        {
            thisAuraColor.a -= auraCheckColorSpeed * Time.deltaTime;
            meshRenderer.material.color = thisAuraColor;

            if (EM.hitstopImpactActive == false)
            {
                timer += Time.deltaTime;
            }
            yield return null;
        }
        thisAuraColor.a = 0.1f;
        meshRenderer.material.color = thisAuraColor;
    }
}