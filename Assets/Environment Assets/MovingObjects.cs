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
    int hazardLM;

    public float initialDelay;
    public bool randomInitialDelay;
    bool initialDelayPassed = false;

    //movementType1: Platform moves back and forth, stays stationary at the ends for a moment. Choose horizontal or vertical.
    //Customize values per platform, including both speed values and times. Place platform on left or bottom stationary position.
    public bool movementType1 = true;
    public bool type1Horizontal = true;
    public float type1Velocity1 = 2.6f;
    public float type1Velocity2 = 2.6f;
    public float type1Distance = 5;
    public float type1StayTime1 = 1;
    public float type1StayTime2 = 1;
    float type1Velocity;
    float type1MoveTime1;
    float type1MoveTime2;

    //movementType2: sine wave type movement in one axis. Will move between distance / 2 and -distance / 2. Choose horizontal or vertical.
    //starting position is at the center of the wave.
    public bool movementType2 = false;
    public bool type2Horizontal = true;
    public float type2Period = 1;
    public float type2Distance = 1;
    float type2Time = 0;

    [HideInInspector] public bool isMoving = false;

    public bool startReverse = false;

    public bool activateBySwitch = false;
    public bool startActive = false;
    public bool stayActive = false;
    [HideInInspector] public bool active = true;
    public bool pauseOnDeactivate = false;
    float storedVelocity;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();

        boxLM = LayerMask.GetMask("Box");
        platformLM = LayerMask.GetMask("Platforms");
        obstacleLM = LayerMask.GetMask("Obstacles");
        hazardLM = LayerMask.GetMask("Hazards");

        type1MoveTime1 = type1Distance / type1Velocity1;
        type1MoveTime2 = type1Distance / type1Velocity2;
        InitialPosition = transform.position;

        initialDelayPassed = false;
        if (movementType1 == true)
        {
            StartCoroutine(MovementType1());
        }
        if (randomInitialDelay)
        {
            initialDelay = Random.Range(0, initialDelay);
        }
        StartCoroutine(InitialDelay());

        if (startReverse)
        {
            type2Distance *= -1;
        }

        active = true;
        if (activateBySwitch && startActive == false)
        {
            active = false;
        }
    }

    void FixedUpdate()
    {
        if (movementType1 == false && movementType2 == false)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        if (initialDelayPassed == true && active)
        {
            if (movementType2 == true)
            {
                movementType1 = false;
                type2Time += Time.deltaTime;

                if (type2Horizontal == true)
                {
                    rb.velocity = new Vector2(Mathf.PI * type2Distance * (float)Mathf.Cos(type2Time * 2 * Mathf.PI / type2Period) / type2Period, 0);
                }
                if (type2Horizontal == false)
                {
                    rb.velocity = new Vector2(0, Mathf.PI * type2Distance * (float)Mathf.Cos(type2Time * 2 * Mathf.PI / type2Period) / type2Period);
                }
                if (type2Time >= type2Period)
                {
                    transform.position = InitialPosition;
                    type2Time -= type2Period;
                }
            }
        }
        if (activateBySwitch && active == false)
        {
            rb.velocity = Vector2.zero;
        }

        if (activateBySwitch)
        {
            Debug.Log(type1Velocity);
        }
    }
    public void Trigger()
    {
        if (((stayActive && active == false) || stayActive == false) && activateBySwitch)
        {
            if (active && movementType1)
            {
                storedVelocity = type1Velocity;
            }
            if (active == false && movementType1)
            {
                type1Velocity = storedVelocity;
            }
            active = !active;
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        bool applyVelocity = false;
        if (1 << (collision.gameObject.layer) == boxLM && Box.isGrounded && 1 << (gameObject.layer) == platformLM &&
            boxRB.position.y >= transform.position.y + transform.lossyScale.y / 2 + boxTransform.lossyScale.y / 2 - 0.01f)
        {
            if ((movementType1 == true && type1Horizontal == true) || (movementType2 == true && type2Horizontal == true))
            {
                applyVelocity = true;
            }
        }
        if (1 << (collision.gameObject.layer) == boxLM && Box.isGrounded && (1 << (gameObject.layer) == obstacleLM || 1 << (gameObject.layer) == hazardLM))
        {
            if ((movementType1 == true && type1Horizontal == true) || (movementType2 == true && type2Horizontal == true))
            {
                applyVelocity = true;
                
            }
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

        float velocity = 0;
        float window = 0;
        float timer = 0;
        bool reachedEnd = false;
        while (true)
        {
            velocity = type1Velocity1 * directionMult;
            if (type1Horizontal)
            {
                rb.velocity = new Vector2(velocity, 0);
            }
            else
            {
                rb.velocity = new Vector2(0, velocity);
            }
            while (reachedEnd == false)
            {
                yield return new WaitForFixedUpdate();
                if (type1Horizontal && Mathf.Abs(rb.position.x - InitialPosition.x) >= type1Distance - Mathf.Abs(velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
                else if (type1Horizontal == false && Mathf.Abs(rb.position.y - InitialPosition.y) >= type1Distance - Mathf.Abs(velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
            }
            reachedEnd = false;

            rb.velocity = Vector2.zero;
            if (type1Horizontal)
            {
                rb.position = new Vector2(InitialPosition.x + type1Distance * directionMult, InitialPosition.y);
            }
            else
            {
                rb.position = new Vector2(InitialPosition.x, InitialPosition.y + type1Distance * directionMult);
            }
            window = type1StayTime1;
            timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            velocity = type1Velocity1 * -directionMult;
            if (type1Horizontal)
            {
                rb.velocity = new Vector2(velocity, 0);
            }
            else
            {
                rb.velocity = new Vector2(0, velocity);
            }
            while (reachedEnd == false)
            {
                yield return new WaitForFixedUpdate();
                if (type1Horizontal && Mathf.Abs(rb.position.x - InitialPosition.x) <= Mathf.Abs(velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
                else if (type1Horizontal == false && Mathf.Abs(rb.position.y - InitialPosition.y) <= Mathf.Abs(velocity) * Time.fixedDeltaTime)
                {
                    reachedEnd = true;
                }
            }
            reachedEnd = false;

            rb.velocity = Vector2.zero;
            rb.position = InitialPosition;
            window = type1StayTime2;
            timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
    }

    IEnumerator InitialDelay()
    {
        yield return null;
        yield return new WaitForSeconds(initialDelay);
        initialDelayPassed = true;
    }
}
