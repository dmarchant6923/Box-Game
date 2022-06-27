using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public bool multipleParts = false; // multiple parts all children of empty gameobject, first object contains rigidbody.

    [HideInInspector] public Rigidbody2D enemyRB;
    [HideInInspector] public Transform[] enemyChildren;
    Rigidbody2D boxRB;
    Transform cameraTransform;
    Camera mainCamera;

    RaycastHit2D enemyRC_ToBox;
    int obstacleAndBoxLM;
    int boxLM;
    [System.NonSerialized] public bool inBoxLOS = false; //line of sight
    [System.NonSerialized] public bool canSeeItem = false;
    Vector2 directionToBox;
    [System.NonSerialized] public float distanceToBox;

    bool exclamationActive = false;
    [System.NonSerialized] public bool attackActive = false;
    public GameObject exclamation;
    GameObject newExclamation;

    [HideInInspector] public bool outsidePulseActive = false;
    [HideInInspector] public Vector2 outsidePulseDirection;
    [HideInInspector] public float outsidePulseMagnitude = 16;

    [HideInInspector] public bool pulseActive = false;
    [HideInInspector] public bool enemyWasPulsed = false;
    [HideInInspector] public bool enemyIsInvulnerable = false;
    public bool enemyWasDamaged = false;
    [HideInInspector] public bool enemyWasKilled = false;

    [HideInInspector] public bool instantKill = false;

    [HideInInspector] public bool hitstopImpactActive = false;
    bool damageFlashCRActive = false;
    float hitstopTime = 0;
    [HideInInspector] public bool scriptsEnabled = true;

    public bool normalPulse = true;
    [HideInInspector] public bool normalDeath = true;
    [HideInInspector] public bool normalDamage = true;
    [HideInInspector] public bool normalHitstop = true;
    [HideInInspector] public bool blastzoneDeath = true;

    [HideInInspector] public bool normalExplosionsWillDamage = false;
    [HideInInspector] public bool explosionsWillPush = true;
    [HideInInspector] public bool reflectedBulletsWillDamage = true;

    [HideInInspector] public bool keepAsKinematic = false;

    [HideInInspector] public bool shieldCurrentlyActive = false;
    [HideInInspector] public bool canReceiveShield = true;

    public GameObject fire;
    GameObject newFire;
    bool fireActive = false;
    [HideInInspector] public bool aggroCurrentlyActive = false;
    [HideInInspector] public bool canReceiveAggro = true;
    [System.NonSerialized] public float aggroIncreaseMult = 1.3f;
    [System.NonSerialized] public float aggroDecreaseMult = 0.75f;

    [HideInInspector] public bool initialDelay = true;

    [HideInInspector] public bool touchingThisEnemy = false;
    [HideInInspector] public bool physicalHitboxActive = false;
    public bool canBeShocked = true;
    [System.NonSerialized] public bool shockCoolDown = false;
    [HideInInspector] public bool activateShock = false;
    [HideInInspector] public bool shockActive = false;
    [HideInInspector] public bool flashOn = false;
    float shockTime = 4f;
    public GameObject lightning;
    GameObject newLightning;
    [HideInInspector] public List<SpriteRenderer> enemyObjects;
    [HideInInspector] public List<Color> enemyColors;

    public float enemyHealth = 1; //all damage will take 1 health
    [HideInInspector] public float invulnerabilityPeriod = 2f;
    float invulnerabilityTime = 0;

    public bool startEnemyDeath = false;
    bool enemyDeathActive = false;

    public bool respawn;

    private void Start()
    {
        if (multipleParts == false)
        {
            enemyRB = gameObject.GetComponent<Rigidbody2D>();
        }
        else
        {
            enemyRB = gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>();
        }
        enemyChildren = gameObject.GetComponentsInChildren<Transform>(true);
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");
        boxLM = LayerMask.GetMask("Box");
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        cameraTransform = GameObject.Find("Main Camera").GetComponent<Transform>();

        shockActive = false;
        enemyWasPulsed = false;
        pulseActive = false;

        enemyObjects = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        enemyColors = new List<Color>();
        for (int i = 0; i < enemyObjects.Count; i++)
        {
            if (enemyObjects[i].gameObject.tag == "Effect" || enemyObjects[i].enabled == false)
            {
                enemyObjects.RemoveAt(i);
                i -= 1;
            }
            else
            {
                enemyColors.Add(enemyObjects[i].color);
            }
        }

        StartCoroutine(StartDelay());
    }
    void Update()
    {
        directionToBox = (boxRB.position - enemyRB.position).normalized;
        distanceToBox = (boxRB.position - enemyRB.position).magnitude;

        enemyRC_ToBox = Physics2D.Raycast(enemyRB.position, (boxRB.position - enemyRB.position).normalized,
            100, obstacleAndBoxLM);

        //inBoxLOS within pulse radius
        if (enemyRC_ToBox.collider != null && 1 << enemyRC_ToBox.collider.gameObject.layer == boxLM)
        {
            inBoxLOS = true;
        }
        else
        {
            inBoxLOS = false;
        }

        if (activateShock)
        {
            if (shockActive == false && canBeShocked && shockCoolDown == false)
            {
                shockCoolDown = true;
                shockActive = true;
                StartCoroutine(Shock());
            }
            activateShock = false;
        }
        if (shockActive && touchingThisEnemy && hitstopImpactActive == false)
        {
            if (physicalHitboxActive)
            {
                StartCoroutine(DelayShockDeactivate());
            }
            else if (Box.isInvulnerable == false)
            {
                Box.damageTaken = Lightning.contactDamage;
                Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - enemyRB.position.x), 1).normalized;
                Box.activateDamage = true;
                Box.activateShock = true;
                StartCoroutine(HitstopImpact(0.1f + (Lightning.contactDamage * Box.boxHitstopDelayMult * Box.shockHitstopMult)));
            }
            else
            {
                shockActive = false;
            }
        }

        if ((enemyWasDamaged == true && shieldCurrentlyActive == false) || instantKill)
        {
            if (enemyIsInvulnerable == false || instantKill)
            {
                enemyHealth -= 1;
                if (enemyHealth <= 0 || instantKill)
                {
                    enemyWasKilled = true;
                    if (normalDeath == true)
                    {
                        scriptsEnabled = false;
                    }
                }
                if (hitstopImpactActive == false)
                {
                    StartCoroutine(HitstopImpact(Box.enemyHitstopDelay));
                }
                enemyIsInvulnerable = true;
            }
            enemyWasDamaged = false;
            instantKill = false;
        }
        if (hitstopImpactActive == true)
        {
            hitstopTime += Time.deltaTime;
        }
        if (enemyIsInvulnerable == true && hitstopImpactActive == false)
        {
            invulnerabilityTime += Time.deltaTime;
            if (damageFlashCRActive == false)
            {
                StartCoroutine(EnemyDamageFlash());
            }
            if (invulnerabilityTime >= invulnerabilityPeriod)
            {
                enemyIsInvulnerable = false;
                invulnerabilityTime = 0;
            }
        }
        if (startEnemyDeath == true && enemyDeathActive == false)
        {
            StartCoroutine(EnemyDeath());
        }

        if (enemyWasDamaged && shieldCurrentlyActive && hitstopImpactActive == false && enemyIsInvulnerable == false)
        {
            StartCoroutine(HitstopImpact(Box.enemyHitstopDelay));
            foreach (Transform item in GetComponentsInChildren<Transform>())
            {
                if (item.GetComponent<Aura>() != null && item.GetComponent<Aura>().shield)
                {
                    item.GetComponent<Aura>().breakAura = true;
                }
            }
            enemyIsInvulnerable = true;
            shieldCurrentlyActive = false;
        }

        if (canSeeItem && exclamationActive == false && enemyWasKilled == false)
        {
            StartCoroutine(Exclamation());
        }

        if (aggroCurrentlyActive && fireActive == false)
        {
            fireActive = true;
            newFire = Instantiate(fire, enemyRB.position, Quaternion.identity);
            newFire.GetComponent<Fire>().objectOnFire = enemyRB;
        }
        if (aggroCurrentlyActive == false && fireActive)
        {
            fireActive = false;
            if (newFire != null)
            {
                newFire.GetComponent<Fire>().stopFire = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //activate pulse force on enemy
        if (pulseActive)
        {
            if (inBoxLOS && enemyIsInvulnerable == false && normalPulse && enemyWasKilled == false)
            {
                enemyRB.velocity = -directionToBox * 16 + Vector2.up * -directionToBox.y * 2;
            }
            if (inBoxLOS && shockActive)
            {
                shockActive = false;
            }
            pulseActive = false;
            StartCoroutine(PulseRecord());
        }

        if (outsidePulseActive && normalPulse && enemyWasKilled == false)
        {
            enemyRB.velocity = outsidePulseDirection * outsidePulseMagnitude;
            outsidePulseActive = false;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Hazard" && enemyIsInvulnerable == false)
        {
            enemyWasDamaged = true;
        }
    }

    IEnumerator HitstopImpact(float time)
    {
        hitstopImpactActive = true;
        Vector2 velocityBeforeHitstop = new Vector2(0, 0);
        float angularVelocityBeforeHitstop = 0;
        if (multipleParts == false)
        {
            //gameObject.GetComponent<Collider2D>().enabled = false;
            angularVelocityBeforeHitstop = enemyRB.angularVelocity;
            enemyRB.angularVelocity = 0;
            enemyRB.isKinematic = true;
            velocityBeforeHitstop = enemyRB.velocity;
            enemyRB.velocity = new Vector2(0, 0);
        }
        else
        {
            foreach (Transform transform in enemyChildren)
            {
                if (transform.GetComponent<Collider2D>() != null)
                {
                    //foreach (Collider2D c in transform.GetComponents<Collider2D>()){ c.enabled = false; }
                }
                if (transform.GetComponent<Rigidbody2D>() != null) 
                {
                    angularVelocityBeforeHitstop = transform.GetComponent<Rigidbody2D>().angularVelocity;
                    transform.GetComponent<Rigidbody2D>().angularVelocity = 0;
                    transform.GetComponent<Rigidbody2D>().isKinematic = true;
                    velocityBeforeHitstop = transform.GetComponent<Rigidbody2D>().velocity;
                    transform.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                }
            }
        }

        Vector2 freezePosition = transform.position;
        float shuffleDelay = 0.02f;
        int shuffleCount = 0;
        float shuffleRangeX = 0.3f;
        float shuffleRangeY = 0.05f;
        while (hitstopTime <= time)
        {
            bool shuffleFinished = false;
            if (shuffleCount == 0 || shuffleCount == 2 && shuffleFinished == false)
            {
                transform.position = new Vector2(freezePosition.x,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount += 1;
                shuffleFinished = true;
            }
            if (shuffleCount == 1 && shuffleFinished == false)
            {
                transform.position = new Vector2(freezePosition.x + shuffleRangeX,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount += 1;
                shuffleFinished = true;
            }
            if (shuffleCount == 3 && shuffleFinished == false)
            {
                transform.position = new Vector2(freezePosition.x - shuffleRangeX/3,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount = 0;
            }
           
            if (Box.enemyHitstopDelay - shuffleDelay > hitstopTime)
            {
                yield return new WaitForSeconds(shuffleDelay);
            }
            else
            {
                yield return new WaitForSeconds(Box.enemyHitstopDelay - hitstopTime);
            }
        }
        hitstopTime = 0;
        transform.position = freezePosition;

        if (enemyWasKilled == false || (enemyWasKilled == true && normalDeath == false))
        {
            if (multipleParts == false)
            {
                gameObject.GetComponent<Collider2D>().enabled = true;
                if (keepAsKinematic == false)
                {
                    enemyRB.isKinematic = false;
                    enemyRB.angularVelocity = angularVelocityBeforeHitstop;
                    enemyRB.velocity = velocityBeforeHitstop;
                }
            }
            else
            {
                foreach (Transform transform in enemyChildren)
                {
                    if (transform.GetComponent<Collider2D>() != null)
                    {
                        //foreach (Collider2D c in transform.GetComponents<Collider2D>()) { c.enabled = true; }
                    }
                    if (transform.GetComponent<Rigidbody2D>() != null && keepAsKinematic == false)
                    {
                        transform.GetComponent<Rigidbody2D>().isKinematic = false;
                        transform.GetComponent<Rigidbody2D>().angularVelocity = angularVelocityBeforeHitstop;
                        transform.GetComponent<Rigidbody2D>().velocity = velocityBeforeHitstop;
                    }
                }
            }
        }
        hitstopImpactActive = false;
        if (enemyWasKilled == true && normalDeath == true)
        {
            StartCoroutine(EnemyDeath());
            enemyIsInvulnerable = false;
        }
        if (shockActive)
        {
            shockActive = false;
        }
        if (scriptsEnabled == false)
        {
            MonoBehaviour[] enemyScripts = gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour script in enemyScripts)
            {
                script.enabled = false;
            }
        }
    }
    IEnumerator EnemyDeath()
    {
        enemyDeathActive = true;
        if (FindObjectOfType<EpisodeManager>() != null && respawn == false)
        {
            FindObjectOfType<EpisodeManager>().enemiesKilled++;
        }
        shieldCurrentlyActive = false;
        aggroCurrentlyActive = false;
        enemyRB.gravityScale = 7;
        enemyRB.drag = 0;
        enemyRB.angularDrag = 0;
        if (multipleParts == true)
        {
            foreach (Transform transform in enemyChildren)
            {
                if (transform.GetComponent<Collider2D>() != null)
                {
                    foreach (Collider2D c in transform.GetComponents<Collider2D>()) { c.enabled = false; }
                }
                transform.parent = null;
                if (transform.GetComponent<Renderer>() != null) { transform.GetComponent<Renderer>().sortingLayerName = "Dead Enemy"; }
                if (transform.GetComponent<Rigidbody2D>() == null)
                {
                    transform.gameObject.AddComponent<Rigidbody2D>();
                }
                transform.GetComponent<Rigidbody2D>().isKinematic = false;
                transform.GetComponent<Rigidbody2D>().gravityScale = 7;
                transform.GetComponent<Rigidbody2D>().freezeRotation = false;
                transform.GetComponent<Rigidbody2D>().velocity = new Vector2(-10 + Random.value * 20, 15 + Random.value * 15);
                transform.GetComponent<Rigidbody2D>().angularVelocity = -800 + Random.value * 1600;
            }
        }
        else
        {
            foreach (Transform transform in enemyChildren)
            {
                if (transform != null && transform.GetComponent<Renderer>() != null)
                {
                    transform.GetComponent<Renderer>().sortingLayerName = "Dead Enemy";
                }
                if (transform != null && transform.GetComponent<Collider2D>() != null)
                {
                    foreach (Collider2D c in transform.GetComponents<Collider2D>()) { c.enabled = false; }
                }
            }
            enemyRB.isKinematic = false;
            enemyRB.freezeRotation = false;
            enemyRB.velocity = new Vector2(0, 18);
            enemyRB.angularVelocity = (Mathf.Abs(boxRB.position.x - enemyRB.position.x) / (boxRB.position.x - enemyRB.position.x)) * 800;
        }
        yield return new WaitForSeconds(2.5f);
        if (multipleParts == true)
        {
            foreach (Transform transform in enemyChildren)
            {
                Destroy(transform.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    IEnumerator EnemyDamageFlash()
    {
        damageFlashCRActive = true;
        while (enemyIsInvulnerable == true)
        {
            foreach (Transform transform in enemyChildren)
            {
                if (transform != null && transform.GetComponent<Renderer>() != null)
                {
                    transform.GetComponent<Renderer>().enabled = true;
                }
            }
            if (enemyIsInvulnerable == true)
            {
                yield return new WaitForSeconds(0.12f);
            }
            foreach (Transform transform in enemyChildren)
            {
                if (transform != null && transform.GetComponent<Renderer>() != null && transform.tag != "Effect")
                {
                    transform.GetComponent<Renderer>().enabled = false;
                    if (transform.GetComponent<SpikeSentinel>() != null && gameObject.GetComponent<SpikeSentry>() != null)
                    {
                        Debug.Log("you are here");
                    }
                }
            }
            if (enemyIsInvulnerable == true)
            {
                yield return new WaitForSeconds(0.04f);
            }
        }
        foreach (Transform transform in enemyChildren)
        {
            if (transform != null && transform.GetComponent<Renderer>() != null)
            {
                transform.GetComponent<Renderer>().enabled = true;
            }
        }
        damageFlashCRActive = false;
    }
    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(1f);
        initialDelay = false;
    }
    IEnumerator Shock()
    {
        float window1 = shockTime; // length of shock status
        float window2 = 0.2f; // time between aesthetic lightnings
        StartCoroutine(ShockFlash());
        float timer1 = 0;
        float timer2 = 0;
        while (timer1 < window1 && shockActive && enemyWasKilled == false)
        {
            if (timer2 > window2)
            {
                float scale;
                if (multipleParts)
                {
                    scale = transform.GetChild(0).transform.lossyScale.x;
                }
                else
                {
                    scale = transform.lossyScale.x;
                }
                Vector2 pointA = enemyRB.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * scale * 2/ 3;
                Vector2 pointB = pointA + Vector2.right * (enemyRB.position.x - pointA.x) * 2;
                newLightning = Instantiate(lightning);
                newLightning.GetComponent<Lightning>().pointA = pointA;
                newLightning.GetComponent<Lightning>().pointB = pointB;
                newLightning.GetComponent<Lightning>().aestheticElectricity = true;
                timer2 = 0;
                window2 = 0.3f + Random.Range(0, 0.4f);
            }
            timer1 += Time.deltaTime;
            timer2 += Time.deltaTime;
            yield return null;
        }
        shockActive = false;
        yield return new WaitForSeconds(window1 - timer1 + 0.5f);
        shockCoolDown = false;
    }
    IEnumerator ShockFlash()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockActive)
        {
            flashOn = true;
            foreach (SpriteRenderer item in enemyObjects)
            {
                item.color = Color.white;
            }
            yield return new WaitForSeconds(window2 + Random.Range(0f, window2));
            flashOn = false;
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
    IEnumerator Exclamation()
    {
        exclamationActive = true;
        yield return new WaitForSeconds(0.1f);
        newExclamation = Instantiate(exclamation);
        newExclamation.GetComponent<Exclamation>().enemy = gameObject;
        float distanceUp = 1.5f;
        if (GetComponent<EnemyBehavior_GroundedVehicle>() != null)
        {
            distanceUp = 2f;
        }
        newExclamation.GetComponent<Exclamation>().distanceUp = distanceUp;
        newExclamation.transform.position = enemyRB.position + Vector2.up * distanceUp;

        float window2 = 1.5f;
        float timer = 0;
        while (timer < window2)
        {
            while (canSeeItem || attackActive)
            {
                timer = 0;
                yield return null;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        exclamationActive = false;
    }
    IEnumerator PulseRecord()
    {
        enemyWasPulsed = true;
        yield return new WaitForSeconds(0.1f);
        enemyWasPulsed = false;
    }
    IEnumerator DelayShockDeactivate()
    {
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        shockActive = false;
    }
}
