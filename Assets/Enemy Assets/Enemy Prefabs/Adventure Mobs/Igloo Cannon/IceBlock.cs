using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceBlock : MonoBehaviour
{
    InputBroker iceInputs;
    Rigidbody2D rb;
    Rigidbody2D boxRB;
    float initialRotation = 0;

    public GameObject shard;
    GameObject newShard;

    public Rigidbody2D frozenRB;
    public EnemyManager EM;
    bool onBox = false;
    bool onEnemy = false;

    public float freezeTime = 10f;
    public Vector2 velocity;
    public float angularVelocity;
    float timer = 0;

    int stage = 0;
    GameObject crack1;
    GameObject crack2;
    GameObject crack3;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.rotation = Random.Range(0, 360f);
        initialRotation = rb.rotation;
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        crack1 = transform.GetChild(0).gameObject;
        crack2 = transform.GetChild(1).gameObject;
        crack3 = transform.GetChild(2).gameObject;

        foreach (SpriteRenderer sprite in crack1.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = false;
        }
        foreach (SpriteRenderer sprite in crack2.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = false;
        }
        foreach (SpriteRenderer sprite in crack3.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = false;
        }
        StartCoroutine(HitstopImpact());
        if (frozenRB == boxRB)
        {
            onBox = true;
            iceInputs = GetComponent<InputBroker>();
            StartCoroutine(Mash());
        }
        else if (EM != null)
        {
            onEnemy = true;
            freezeTime = EM.freezeLength * Random.Range(0.95f, 1.05f);
            Destroy(GetComponent<InputBroker>());
        }

        foreach (Collider2D col in frozenRB.transform.GetComponentsInChildren<Collider2D>())
        {
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), col);
        }
    }

    private void Update()
    {
        if (Box.pulseActive == true && onEnemy && rb.isKinematic == false && (rb.position - boxRB.position).magnitude < Box.pulseRadius + transform.localScale.x * 0.4f)
        {
            Vector2 vector = rb.position - boxRB.position;
            if (Tools.LineOfSight(boxRB.position, vector) == false)
            {
                return;
            }
            rb.velocity = (rb.position - boxRB.position).normalized * Box.enemyPulseMagnitude * 0.75f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((onBox && Box.frozen == false) || (onEnemy && EM.enemyIsFrozen == false) || timer > freezeTime)
        {
            shatter();
        }
        timer += Time.fixedDeltaTime;

        if (rb.isKinematic == false)
        {
            frozenRB.position = rb.position + rb.velocity * Time.fixedDeltaTime;
            frozenRB.rotation = rb.rotation - initialRotation;
            frozenRB.velocity = Vector2.zero;
        }

        if (timer > freezeTime * 0.25f && stage < 1)
        {
            stage = 1;
            StartCoroutine(Shake(false));
            foreach (SpriteRenderer sprite in crack1.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = true;
            }
        }
        if (timer > freezeTime * 0.5f && stage < 2)
        {
            stage = 2;
            StartCoroutine(Shake(false));
            foreach (SpriteRenderer sprite in crack2.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = true;
            }
        }
        if (timer > freezeTime * 0.75f && stage < 3)
        {
            stage = 3;
            StartCoroutine(Shake(true));
            foreach (SpriteRenderer sprite in crack3.GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = true;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.tag == "Hazard")
        {
            shatter();
        }
    }

    void shatter()
    {
        if (onBox)
        {
            Box.frozen = false;
        }
        if (onEnemy)
        {
            frozenRB.transform.root.GetComponent<EnemyManager>().enemyIsFrozen = false;
        }
        for (int i = 0; i < 12; i++)
        {
            Vector2 vector = Random.insideUnitCircle;
            newShard = Instantiate(shard, rb.position + vector * transform.localScale.x / 2, Quaternion.identity);
            newShard.GetComponent<Rigidbody2D>().velocity = vector * 6;
        }

        Destroy(gameObject);
    }

    IEnumerator HitstopImpact()
    {
        float damageWindow = 0.4f;
        float shuffleDelay = 0.06f;
        int shuffleCount = 0;
        float shuffleRangeX = 0.3f;
        float shuffleRangeY = 0.05f;
        Vector2 freezePosition = rb.position;
        float freezeRotation = rb.rotation;

        while (timer <= damageWindow)
        {
            bool shuffleFinished = false;
            float shuffleTimer = 0;
            if (shuffleCount == 0 || shuffleCount == 2 && shuffleFinished == false)
            {
                shuffleCount += 1;
                shuffleFinished = true;
                float rand = Random.value;
                while (shuffleTimer < shuffleDelay && timer <= damageWindow)
                {
                    rb.position = new Vector2(freezePosition.x,
                        freezePosition.y + (-shuffleRangeY / 2) + rand * shuffleRangeY);
                    rb.rotation = freezeRotation;
                    shuffleTimer += Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            if (shuffleCount == 1 && shuffleFinished == false)
            {

                shuffleCount += 1;
                shuffleFinished = true;
                float rand = Random.value;
                while (shuffleTimer < shuffleDelay && timer <= damageWindow)
                {
                    rb.position = new Vector2(freezePosition.x + shuffleRangeX,
                        freezePosition.y + (-shuffleRangeY / 2) + rand * shuffleRangeY);
                    rb.rotation = freezeRotation;
                    shuffleTimer += Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            if (shuffleCount == 3 && shuffleFinished == false)
            {
                shuffleCount = 0;
                float rand = Random.value;
                while (shuffleTimer < shuffleDelay && timer <= damageWindow)
                {
                    rb.position = new Vector2(freezePosition.x - shuffleRangeX / 5,
                        freezePosition.y + (-shuffleRangeY / 2) + rand * shuffleRangeY);
                    rb.rotation = freezeRotation;
                    shuffleTimer += Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }

        float magnitude = velocity.magnitude;
        Vector2 trueVel = velocity;
        if (onBox)
        {
            trueVel = Tools.DI(velocity.normalized, iceInputs.leftStickDisabled, 0.4f) * magnitude;
        }
        rb.velocity = trueVel;
        rb.angularVelocity = angularVelocity;
    }
    IEnumerator Shake(bool forever)
    {
        float distance = 0.05f;
        float time = 0.04f;

        rb.position += Vector2.right * distance * 3 / 2;
        yield return new WaitForSeconds(time);
        rb.position += Vector2.left * distance * 3;
        yield return new WaitForSeconds(time);
        rb.position += Vector2.right * distance * 3;

        while (forever)
        {
            yield return new WaitForSeconds(time);
            rb.position += Vector2.left * distance;
            yield return new WaitForSeconds(time);
            rb.position += Vector2.right * distance;
        }
        rb.position += Vector2.left * distance * 3 / 2;
    }
    IEnumerator Mash()
    {
        bool right = false;
        bool left = false;
        bool up = false;
        bool down = false;

        float mashWindow = 0.07f; //0.07
        float mashTimer = mashWindow;

        int activeFrames = 0;
        int inactiveFrames = 0;

        while (true)
        {
            if (iceInputs.leftStickDisabled.x > 0.8f && right == false)
            {
                right = true;
                left = false;
                mashTimer = 0;
            }
            if (iceInputs.leftStickDisabled.x < -0.8f && left == false)
            {
                left = true;
                right = false;
                mashTimer = 0;
            }
            if (iceInputs.leftStickDisabled.y > 0.8f && up == false)
            {
                up = true;
                down = false;
                mashTimer = 0;
            }
            if (iceInputs.leftStickDisabled.y < -0.8f && down == false)
            {
                down = true;
                up = false;
                mashTimer = 0;
            }
            if (mashTimer == 0)
            {
                rb.position += new Vector2(iceInputs.leftStickDisabled.x, iceInputs.leftStickDisabled.y / 3) * 0.12f;
            }

            if (Mathf.Abs(iceInputs.leftStickDisabled.x) < 0.2f)
            {
                right = false;
                left = false;
            }
            if (Mathf.Abs(iceInputs.leftStickDisabled.y) < 0.2f)
            {
                up = false;
                down = false;
            }

            if (mashTimer < mashWindow)
            {
                timer += Time.fixedDeltaTime * 1.5f;
                activeFrames++;
            }
            else
            {
                inactiveFrames++;
            }

            //Debug.Log("active: " + activeFrames + ". inactive: " + inactiveFrames + ". total: " + (activeFrames + inactiveFrames));

            yield return new WaitForFixedUpdate();
            mashTimer += Time.fixedDeltaTime;
        }
    }
}
