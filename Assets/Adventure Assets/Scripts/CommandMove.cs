using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandMove : MonoBehaviour
{
    Rigidbody2D RB;
    MovingObjects MO;

    public float distance;
    public Vector2 direction;
    public float speed = 5;

    Vector2 destination;
    bool moving = false;

    public bool moveOnce = true;
    public bool moved = false;
    public bool canStopMoving = false;
    void Start()
    {
        RB = GetComponent<Rigidbody2D>();
        MO = GetComponent<MovingObjects>();
        direction = direction.normalized;
        destination = RB.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            MO.isMoving = true;
            RB.velocity = direction * speed;
            if ((RB.position - destination).magnitude < 0.05)
            {
                RB.position = destination;
                moving = false;
            }
        }
        else
        {
            MO.isMoving = false;
            RB.velocity = Vector2.zero;
        }
    }
    public void Trigger()
    {
        Debug.Log("you are here");
        if (moveOnce == false || (moveOnce == true && moved == false))
        {
            destination += direction * distance;
            moving = true;
            moved = true;
        }
        else if (canStopMoving && moving)
        {
            moving = false;
            destination = RB.position;
        }
    }
}
