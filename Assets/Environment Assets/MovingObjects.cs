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

    //movementType2: sine wave type movement in one axis. Will move between distance and -distance. Choose horizontal or vertical.
    //starting position is at the center of the wave.
    public bool movementType2 = false;
    public bool type2Horizontal = true;
    public float type2Period = 1;
    public float type2Distance = 1;
    float type2Time = 0;

    [HideInInspector] public bool isMoving = false;

    public bool startReverse = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();

        boxLM = LayerMask.GetMask("Box");
        platformLM = LayerMask.GetMask("Platforms");
        obstacleLM = LayerMask.GetMask("Obstacles");
        hazardLM = LayerMask.GetMask("Hazards");

        BoxVelocity.velocitiesX[1] = 0;

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
            type1Distance *= -1;
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

        if (initialDelayPassed == true)
        {
            if (movementType1 == true)
            {
                movementType2 = false;

                if (type1Horizontal == true)
                {
                    if (rb.position.x > InitialPosition.x + type1Distance + 0.05f)
                    {
                        rb.position = new Vector2(InitialPosition.x + type1Distance, InitialPosition.y);
                        rb.velocity = Vector2.zero;
                    }
                    else if (rb.position.x < InitialPosition.x - 0.05f)
                    {
                        rb.position = new Vector2(InitialPosition.x, InitialPosition.y);
                        rb.velocity = Vector2.zero;
                    }
                    else
                    {
                        rb.velocity = new Vector2(type1Velocity, 0);
                    }
                }
                else
                {
                    if (rb.position.y > InitialPosition.y + type1Distance + 0.05f)
                    {
                        rb.position = new Vector2(InitialPosition.x, InitialPosition.y + type1Distance);
                        rb.velocity = Vector2.zero;
                    }
                    else if (rb.position.y < InitialPosition.y - 0.05f)
                    {
                        rb.position = new Vector2(InitialPosition.x, InitialPosition.y);
                        rb.velocity = Vector2.zero;
                    }
                    else
                    {
                        rb.velocity = new Vector2(0, type1Velocity);
                    }
                }
            }

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
        yield return new WaitForSeconds(type1StayTime2);
        type1Velocity = type1Velocity1;
        yield return new WaitForSeconds(type1MoveTime1);
        type1Velocity = 0;
        yield return new WaitForSeconds(type1StayTime1);
        type1Velocity = -type1Velocity2;
        yield return new WaitForSeconds(type1MoveTime2);
        type1Velocity = 0;
        transform.position = InitialPosition;
        StartCoroutine(MovementType1());
    }

    IEnumerator InitialDelay()
    {
        yield return null;
        yield return new WaitForSeconds(initialDelay);
        initialDelayPassed = true;
    }
}
