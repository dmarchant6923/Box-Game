using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObjects : MonoBehaviour
{
    Vector2 InitialPosition;
    Rigidbody2D rb;
    Rigidbody2D boxRB;
    Transform boxTransform;
    int boxLM;
    int platformLM;
    int obstacleLM;

    public float initialDelay;
    public bool randomInitialDelay;
    bool initialDelayPassed = false;

    public float distance = 5;
    public bool horizontal = true;

    //movementType1: Platform moves back and forth, stays stationary at the ends for a moment. Choose horizontal or vertical.
    //Customize values per platform, including both speed values and times. Place platform on left or bottom stationary position.
    public bool movementType1 = true;
    public float type1Velocity1 = 2.6f;
    public float type1Velocity2 = 2.6f;
    public float type1StayTime1 = 1;
    public float type1StayTime2 = 1;
    float type1Velocity;

    //movementType2: sine wave type movement in one axis. Will move between distance / 2 and -distance / 2. Choose horizontal or vertical.
    //starting position is at the center of the wave.
    public bool movementType2 = false;
    public float type2Period = 1;
    float type2Time = 0;

    [HideInInspector] public bool isMoving = false;

    public bool startReverse = false;

    public bool activateBySwitch = false;
    public bool startActive = false;
    public bool stayActive = false;
    [HideInInspector] public bool active = true;
    public bool pauseOnDeactivate = false; // True: will stop the object in place when it is deactivated. False: will wait to reach initial position.
    [HideInInspector] public bool bufferDeactivate = false;
    bool reachedInitialPosition = false;
    float storedVelocity;

    public bool interactableObstacle = true;

    Transform end1;
    Transform end2;
    LineRenderer line;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();

        boxLM = LayerMask.GetMask("Box");
        platformLM = LayerMask.GetMask("Platforms");
        obstacleLM = LayerMask.GetMask("Obstacles");

        InitialPosition = transform.position;

        initialDelayPassed = false;
        if (movementType1 == true)
        {
            StartCoroutine(MovementType1());
            movementType2 = false;
        }
        if (randomInitialDelay)
        {
            initialDelay = Random.Range(0, initialDelay);
        }
        StartCoroutine(InitialDelay());

        if (movementType1 == false && movementType2 == false)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        if (startReverse)
        {
            distance *= -1;
        }

        active = true;
        if (activateBySwitch && startActive == false)
        {
            active = false;
        }

        if ((movementType1 || movementType2) && GetComponent<PlatformDrop>() != null)
        {
            end1 = transform.GetChild(0);
            end2 = transform.GetChild(1);
            line = GetComponent<LineRenderer>();

            end1.gameObject.SetActive(true); end2.gameObject.SetActive(true); line.enabled = true;
            end1.parent = null; end2.parent = null;
            end1.transform.localScale = Vector2.one * 0.5f; end2.transform.localScale = Vector2.one * 0.5f;

            if (horizontal)
            {
                if (movementType1)
                {
                    end1.position = transform.position; end2.position = transform.position + Vector3.right * distance;
                    line.SetPosition(0, end1.position); line.SetPosition(1, end2.position);
                }
                else
                {
                    end1.position = transform.position + Vector3.left * (distance / 2); end2.position = transform.position + Vector3.right * (distance / 2);
                    line.SetPosition(0, end1.position); line.SetPosition(1, end2.position);
                }
            }
            else
            {
                if (movementType1)
                {
                    end1.position = transform.position; end2.position = transform.position + Vector3.up * distance;
                    line.SetPosition(0, end1.position); line.SetPosition(1, end2.position);
                }
                else
                {
                    end1.position = transform.position + Vector3.down * (distance / 2); end2.position = transform.position + Vector3.up * (distance / 2);
                    line.SetPosition(0, end1.position); line.SetPosition(1, end2.position);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (initialDelayPassed == true && active && movementType2)
        {
            movementType1 = false;
            type2Time += Time.deltaTime;

            if (horizontal == true)
            {
                rb.velocity = new Vector2(Mathf.PI * distance * (float)Mathf.Cos(type2Time * 2 * Mathf.PI / type2Period) / type2Period, 0);
            }
            if (horizontal == false)
            {
                rb.velocity = new Vector2(0, Mathf.PI * distance * (float)Mathf.Cos(type2Time * 2 * Mathf.PI / type2Period) / type2Period);
            }
            if (type2Time >= type2Period)
            {
                transform.position = InitialPosition;
                type2Time -= type2Period;
            }
        }
        if (activateBySwitch && active == false)
        {
            rb.velocity = Vector2.zero;
        }
    }
    public void Trigger()
    {
        if (((stayActive && active == false) || stayActive == false) && activateBySwitch)
        {
            if (pauseOnDeactivate)
            {
                if (active && movementType1)
                {
                    storedVelocity = type1Velocity;
                    rb.velocity = Vector2.zero;
                }
                if (active == false && movementType1)
                {
                    type1Velocity = storedVelocity;
                    if (horizontal)
                    {
                        rb.velocity = new Vector2(type1Velocity, 0);
                    }
                    else
                    {
                        rb.velocity = new Vector2(0, type1Velocity);
                    }
                }
                active = !active;
            }
            else
            {
                if (active)
                {
                    if (bufferDeactivate)
                    {
                        bufferDeactivate = false;
                    }
                    else
                    {
                        StartCoroutine(WaitForInitialPosition());
                    }
                }
                else
                {
                    active = true;
                }
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        bool applyVelocity = false;
        bool boxOnTop = false;

        foreach (ContactPoint2D col in collision.contacts)
        {
            if (1 << col.collider.gameObject.layer == boxLM && Box.isGrounded)
            {
                boxOnTop = true;
                break;
            }
        }

        if (boxOnTop && 1 << (gameObject.layer) == platformLM && isMoving && 
            boxRB.position.y >= transform.position.y + transform.lossyScale.y / 2 + boxTransform.lossyScale.y / 2 - 0.01f)
        {
            applyVelocity = true;
        }
        if (boxOnTop && 1 << gameObject.layer == obstacleLM && isMoving)
        {
            applyVelocity = true;
        }
        if (applyVelocity == true)
        {
            if (Box.isOnIce == false)
            {
                BoxVelocity.velocitiesX[1] = rb.velocity.x;
            }
            else
            {
                BoxVelocity.velocitiesX[1] = Mathf.MoveTowards(BoxVelocity.velocitiesX[1], rb.velocity.x, 4 * Time.deltaTime);
            }

        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (1 << (collision.gameObject.layer) == boxLM)
        {
            BoxVelocity.velocitiesX[0] += BoxVelocity.velocitiesX[1];
            BoxVelocity.velocitiesX[1] = 0;
        }
    }
    IEnumerator InitialDelay()
    {
        yield return null;
        yield return new WaitForSeconds(initialDelay);
        initialDelayPassed = true;
    }
    IEnumerator MovementType1()
    {
        while (initialDelayPassed == false)
        {
            yield return null;
        }
        float directionMult = 1;
        if (startReverse)
        {
            directionMult = -1;
        }
        distance = Mathf.Abs(distance);

        float window;
        float timer;
        bool reachedEnd = false;
        while (true)
        {
            while (active == false)
            {
                yield return null;
            }
            type1Velocity = type1Velocity1 * directionMult;
            if (horizontal)
            {
                rb.velocity = new Vector2(type1Velocity, 0);
            }
            else
            {
                rb.velocity = new Vector2(0, type1Velocity);
            }
            while (reachedEnd == false)
            {
                yield return null;
                if (horizontal && Mathf.Abs(rb.position.x - InitialPosition.x) >= distance - Mathf.Abs(type1Velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
                else if (horizontal == false && Mathf.Abs(rb.position.y - InitialPosition.y) >= distance - Mathf.Abs(type1Velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
            }
            reachedEnd = false;

            type1Velocity = 0;
            rb.velocity = Vector2.zero;
            if (horizontal)
            {
                rb.position = new Vector2(InitialPosition.x + distance * directionMult, InitialPosition.y);
            }
            else
            {
                rb.position = new Vector2(InitialPosition.x, InitialPosition.y + distance * directionMult);
            }
            window = type1StayTime1;
            timer = 0;
            while (timer < window)
            {
                type1Velocity = 0;
                timer += Time.deltaTime;
                yield return null;
            }
            while (active == false)
            {
                yield return null;
            }

            type1Velocity = type1Velocity2 * -directionMult;
            if (horizontal)
            {
                rb.velocity = new Vector2(type1Velocity, 0);
            }
            else
            {
                rb.velocity = new Vector2(0, type1Velocity);
            }
            while (reachedEnd == false)
            {
                yield return null;
                if (horizontal && Mathf.Abs(rb.position.x - InitialPosition.x) <= Mathf.Abs(type1Velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
                else if (horizontal == false && Mathf.Abs(rb.position.y - InitialPosition.y) <= Mathf.Abs(type1Velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
            }
            reachedEnd = false;

            type1Velocity = 0;
            rb.velocity = Vector2.zero;
            rb.position = InitialPosition;
            reachedInitialPosition = true;
            window = type1StayTime2;
            timer = 0;
            while (timer < window)
            {
                type1Velocity = 0;
                timer += Time.deltaTime;
                yield return null;
            }
            reachedInitialPosition = false;
        }
    }
    IEnumerator WaitForInitialPosition()
    {
        bufferDeactivate = true;
        while (reachedInitialPosition == false && bufferDeactivate)
        {
            yield return null;
        }
        if (bufferDeactivate)
        {
            active = false;
        }
        bufferDeactivate = false;
    }
}
