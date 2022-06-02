using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    GameObject player;

    CircleCollider2D circleCol;
    float initialColRadius;

    BoxCollider2D boxCol;
    Vector2 initialColSize;

    Transform mountedBumper;

    public bool circle = true;
    public bool box = false;
    public bool mounted = false;

    bool bounce = false;
    public float bounceMagnitude = 20;
    Vector2 initialScale;

    void Start()
    {
        player = GameObject.Find("Box");
        if (circle)
        {
            circleCol = GetComponent<CircleCollider2D>();
            initialColRadius = circleCol.radius;
            initialScale = transform.localScale;
        }
        else if (box)
        {
            boxCol = GetComponent<BoxCollider2D>();
            initialColSize = boxCol.size;
            initialScale = transform.localScale;
        }
        else if (mounted)
        {
            mountedBumper = transform.GetChild(0);
            initialScale = mountedBumper.localScale;
        }


    }

    private void FixedUpdate()
    {
        if (bounce)
        {
            bounce = false;
            StartCoroutine(Bounce());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 launch = -collision.GetContact(0).normal;
        if (mounted)
        {
            launch = Tools.AngleToVector(transform.eulerAngles.z);
        }
        if (collision.gameObject == player)
        {
            if (Box.damageActive)
            {
                BoxVelocity.velocitiesX[0] = launch.x * bounceMagnitude;
                collision.collider.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.collider.GetComponent<Rigidbody2D>().velocity.x, launch.y * bounceMagnitude);
                bounce = true;
                if (mounted)
                {
                    StartCoroutine(DisableCollision());
                }
            }
            else if (Box.boxWasPulsed == false)
            {
                Box.boxPulsedWhileInvulnerable = true;
                Box.boxWasPulsed = true;
                Box.boxEnemyPulseDirection = launch;
                Box.boxEnemyPulseMagnitude = bounceMagnitude;
                bounce = true;
                if (mounted)
                {
                    StartCoroutine(DisableCollision());
                }
            }
        }

        if (collision.gameObject.GetComponent<EnemyManager>() != null)
        {
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseDirection = launch;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseActive = true;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseMagnitude = bounceMagnitude;
            bounce = true;
        }


        if (collision.gameObject.GetComponent<Grenade>() != null)
        {
            Rigidbody2D grenadeRB = collision.collider.GetComponent<Rigidbody2D>();
            grenadeRB.velocity = launch * bounceMagnitude;
            bounce = true;
        }
    }

    IEnumerator Bounce()
    {
        yield return new WaitForFixedUpdate();
        float mult = 1.35f;
        float shrinkSpeed = 3;
        if (circle)
        {
            transform.localScale = Vector2.one * initialScale * mult;
        }
        else if (box)
        {
            mult += (mult - 1) * 2;
            shrinkSpeed *= 1.5f;
            transform.localScale = new Vector2(transform.localScale.x + transform.localScale.y * (mult - 1), transform.localScale.y * mult);
        }
        else if (mounted)
        {
            mountedBumper.localScale = Vector2.one * initialScale * mult;
        }
        float timer = 0;
        while (((transform.localScale.y > initialScale.y && mounted == false) || 
                (mountedBumper != null && mountedBumper.transform.localScale.y > initialScale.y && mounted))
                 && bounce == false)
        {
            if (circle)
            {
                circleCol.radius = initialColRadius * (initialScale.y / transform.localScale.y);

                transform.localScale = new Vector2(transform.localScale.x - shrinkSpeed * initialScale.y * Time.deltaTime,
                                                   transform.localScale.y - shrinkSpeed * initialScale.y * Time.deltaTime);

            }
            else if (box)
            {
                boxCol.size = new Vector2(initialColSize.x * (initialScale.x / transform.localScale.x), 
                                          initialColSize.y * (initialScale.y / transform.localScale.y));

                transform.localScale = new Vector2(transform.localScale.x - shrinkSpeed * initialScale.y * Time.deltaTime,
                                                   transform.localScale.y - shrinkSpeed * initialScale.y * Time.deltaTime);
            }
            else if (mounted)
            {
                mountedBumper.localScale = new Vector2(mountedBumper.localScale.x - shrinkSpeed * initialScale.y * Time.deltaTime,
                                                       mountedBumper.localScale.y - shrinkSpeed * initialScale.y * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (circle)
        {
            circleCol.radius = initialColRadius;
            transform.localScale = Vector2.one * initialScale;
        }
        else if (box)
        {
            boxCol.size = initialColSize;
            transform.localScale = Vector2.one * initialScale;
        }
        else if (mounted)
        {
            mountedBumper.localScale = Vector2.one * initialScale;
        }
    }
    IEnumerator DisableCollision()
    {
        GetComponent<PolygonCollider2D>().enabled = false;
        yield return new WaitForFixedUpdate();
        GetComponent<PolygonCollider2D>().enabled = true;
    }
}
