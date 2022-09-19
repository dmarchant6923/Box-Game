using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDrop : MonoBehaviour
{
    InputBroker inputs;

    PlatformEffector2D platformEffector2D;
    Rigidbody2D rb;
    Rigidbody2D boxRB;
    Transform boxTransform;
    Color initialColor;
    Color platformColor;

    bool fallTimerCR_running = false; // whether or not the fallTimerCR is active. FallTimerCR for dropping down while on a platform.
    float downWindow = 0.2f; // window of time to press second down input to fall through platform while grounded.
    float secondsDisabled = 0.25f;
    float boxVelocityy;
    //float boxSpinSpeed;
    //float spinDropWindow = 0.15f;
    //float spinDropVelocity = -1f;
    //float spinDropTimer;

    float downThreshold = -0.5f;

    bool platformResetCR = false;
    bool airTimerCR = false;
    bool canAirDrop = false;

    [HideInInspector] public static bool platformsEnabled = true;

    static bool debugEnabled = false;

    [HideInInspector] public bool activateShock = false;
    [HideInInspector] public bool shockActive = false;
    [HideInInspector] public bool endShock = false;
    float shockTime = 4f;
    public GameObject lightning;
    GameObject newLightning;

    [SerializeField] bool active = true;

    int platformLM;

    private void Start()
    {
        inputs = GameObject.Find("Box").GetComponent<InputBroker>();

        platformLM = LayerMask.GetMask("Platforms");
        platformEffector2D = GetComponent<PlatformEffector2D>();
        platformsEnabled = true;

        rb = GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();

        if (gameObject.tag == "Ice")
        {
            initialColor = new Color(0.5f, 1, 0.85f);
        }
        else
        {
            initialColor = GetComponent<SpriteRenderer>().color;
        }
        platformColor = initialColor;

        active = true;

        if (GetComponent<MovingObjects>().active == false)
        {
            platformColor = initialColor / 3f;
            platformColor.a = 1;
            GetComponent<SpriteRenderer>().color = platformColor;
        }

    }
    void Update()
    {
        boxVelocityy = GameObject.Find("Box").GetComponent<Rigidbody2D>().velocity.y;
        //boxSpinSpeed = Mathf.Abs(GameObject.Find("Box").GetComponent<Rigidbody2D>().angularVelocity);

        //timed barrier to dropping while spinning in the air. Resets spinDropTimer if spinning fast enough AND falling slower than a limit.
        //currently not working as well as I would like. Ideal goal would be to not be able to drop down through a platform wile spinning
        //and under a certain downward velocity for a small amount of time. Not sure if I should just let you drop down freely though, and
        //the player just has to deal with not being able to immediately stop spinning on platforms. Maybe the better solution.
        //if (boxSpinSpeed > 500 && boxVelocityy > spinDropVelocity)
        //{
        //    spinDropTimer = 0;
        //    platformsEnabled = true;
        //}
        //spinDropTimer is always incrementing, but can get reset to 0 above
        //spinDropTimer += Time.deltaTime;
        //drop through the platform in the air
        if (Box.isGrounded == false && airTimerCR == false)
        {
            StartCoroutine(AirTimer());
        }

        if (Box.isGrounded == false && fallTimerCR_running == false && canAirDrop)// && spinDropTimer > spinDropWindow)
        {
            if (inputs.leftStick.y < downThreshold)
            {
                platformsEnabled = false;
            }
            else
            {
                platformsEnabled = true;
            }
        }
        if (Box.isGrounded == true && Box.groundRayCast.collider != null && 1 << Box.groundRayCast.collider.gameObject.layer == platformLM)
        {
            if (InputBroker.Controller)
            {
                if (fallTimerCR_running == false)
                {
                    platformsEnabled = true;
                }
                if (inputs.leftSmashD)
                {
                    StartCoroutine(FallTimer());
                }
            }
            else if (InputBroker.Keyboard)
            {
                if (Box.isGrounded == true)
                {
                    if (fallTimerCR_running == false)
                    {
                        platformsEnabled = true;
                    }
                    if (inputs.leftSmashD && fallTimerCR_running == false)
                    {
                        StartCoroutine(FallTimerKeyboard());
                    }
                }
            }
        }

        if (Box.groundRayCast.collider != null && Box.groundRayCast.collider.gameObject == this.gameObject && platformResetCR == false &&
            Box.isGrounded && boxRB.velocity.y < -0.1 && transform.GetComponent<PlatformDrop>().fallTimerCR_running == false &&
            boxRB.position.y < rb.position.y + transform.lossyScale.y / 2 + boxTransform.lossyScale.y / 2 &&
            boxRB.position.y > rb.position.y + transform.lossyScale.y / 2 + boxTransform.lossyScale.y / 2 - 0.2f)
        {
            StartCoroutine(PlatformReset());
        }

        if (activateShock)
        {
            if (shockActive == false)
            {
                shockActive = true;
                StartCoroutine(Shock());
            }
            activateShock = false;
        }
        if (endShock && shockActive)
        {
            StartCoroutine(EndShock());
            endShock = false;
        }
        if (Box.pulseActive && shockActive)
        {
            RaycastHit2D[] platforms = Physics2D.CircleCastAll(boxRB.position, Box.pulseRadius, Vector2.zero, 0, platformLM);
            if (platforms.Length != 0)
            {
                foreach (RaycastHit2D platform in platforms)
                {
                    if (platform.collider.gameObject == gameObject)
                    {
                        shockActive = false;
                    }
                }
            }
        }

        if (debugEnabled)
        {
            if (platformsEnabled == false)
            {
                gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().color = platformColor;
            }
        }
    }
    private void FixedUpdate()
    {
        if (platformsEnabled == true)
        {
            Physics2D.IgnoreLayerCollision(3, 9, false);
        }
        if (platformsEnabled == false)
        {
            Physics2D.IgnoreLayerCollision(3, 9, true);
        }


        if (GetComponent<MovingObjects>() != null)
        {
            if (GetComponent<MovingObjects>().active != active)
            {
                active = !active;
                if (active == false || GetComponent<MovingObjects>().bufferDeactivate)
                {
                    platformColor = initialColor / 3f;
                    platformColor.a = 1;
                    GetComponent<SpriteRenderer>().color = platformColor;
                }
                else
                {
                    platformColor = initialColor;
                    GetComponent<SpriteRenderer>().color = platformColor;
                }
            }
        }
    }
    IEnumerator FallTimer()
    {
        fallTimerCR_running = true;
        yield return new WaitForSeconds(0.01f);
        platformsEnabled = false;
        yield return new WaitForSeconds(secondsDisabled);
        platformsEnabled = true;
        fallTimerCR_running = false;
    }
    IEnumerator FallTimerKeyboard()
    {
        fallTimerCR_running = true;
        bool doubleTap = false;
        float downTimer = 0;
        yield return null;
        while (downTimer <= downWindow && doubleTap == false)
        {
            if (inputs.leftSmashD)
            {
                doubleTap = true;
            }
            downTimer += Time.deltaTime;
            yield return null;
        }
        if (doubleTap)
        {
            yield return new WaitForSeconds(0.01f);
            platformsEnabled = false;
            yield return new WaitForSeconds(secondsDisabled);
            platformsEnabled = true;
        }
        fallTimerCR_running = false;
    }
    IEnumerator PlatformReset()
    {
        platformResetCR = true;
        boxRB.position = new Vector2(boxRB.position.x, rb.position.y + transform.lossyScale.y / 2 + boxTransform.lossyScale.y / 2 + 0.05f);
        yield return new WaitForSeconds(0.1f);
        platformResetCR = false;
    }
    IEnumerator AirTimer()
    {
        airTimerCR = true;
        float airDropWindow = 0.2f;
        float airDropTimer = 0;
        while (airDropTimer <= airDropWindow && Box.isGrounded == false)
        {
            airDropTimer += Time.deltaTime;
            yield return null;
        }
        if (Box.isGrounded == false)
        {
            canAirDrop = true;
        }
        while (Box.isGrounded == false)
        {
            yield return null;
        }
        canAirDrop = false;
        airTimerCR = false;
    }
    IEnumerator Shock()
    {
        float window1 = shockTime;
        float window2 = 0.4f;
        float timer1 = 0;
        float timer2 = 0;
        bool LtoR = true;
        StartCoroutine(ShockFlash());
        while (timer1 < window1 && shockActive)
        {
            if (timer2 > window2)
            {
                Vector2 pointA;
                Vector2 pointB;
                if (LtoR)
                {
                    pointA = new Vector2(rb.position.x - transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    pointB = new Vector2(rb.position.x + transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    LtoR = false;
                }
                else
                {
                    pointA = new Vector2(rb.position.x + transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    pointB = new Vector2(rb.position.x - transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    LtoR = true;
                }
                newLightning = Instantiate(lightning);
                newLightning.GetComponent<Lightning>().pointA = pointA;
                newLightning.GetComponent<Lightning>().pointB = pointB;
                newLightning.GetComponent<Lightning>().pointsPerUnit = 2 - Mathf.Min(transform.localScale.x / 15, 1);
                if (transform.position.y == 5)
                {
                    Debug.Log(2 - Mathf.Min(transform.localScale.x / 15, 1));
                }
                newLightning.GetComponent<Lightning>().aestheticElectricity = true;
                timer2 = 0;
                window2 = 0.4f + Random.Range(-0.3f, 0.3f);
            }
            timer1 += Time.deltaTime;
            timer2 += Time.deltaTime;
            yield return null;
        }
        shockActive = false;
    }
    IEnumerator ShockFlash()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockActive)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(window2 + Random.Range(0, window2 * 1.5f));
            GetComponent<SpriteRenderer>().color = initialColor;
            yield return new WaitForSeconds(window1 + Random.Range(0, window1 * 1.5f));
        }
        GetComponent<SpriteRenderer>().color = initialColor;
    }
    IEnumerator EndShock()
    {
        yield return null;
        float window = Lightning.contactDamage * Box.boxHitstopDelayMult * 2f;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        shockActive = false;
    }
}
