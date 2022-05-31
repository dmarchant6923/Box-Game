using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    GameObject box;

    CircleCollider2D circleCol;
    float initialColRadius;

    BoxCollider2D boxCol;
    Vector2 initialColSize;


    public bool circle = true;

    bool bounce = false;
    public float bounceMagnitude = 20;
    Vector2 initialScale;

    void Start()
    {
        box = GameObject.Find("Box");
        if (circle)
        {
            circleCol = GetComponent<CircleCollider2D>();
            initialColRadius = circleCol.radius;
        }
        else
        {
            boxCol = GetComponent<BoxCollider2D>();
            initialColSize = boxCol.size;
        }
        initialScale = transform.localScale;


    }

    private void FixedUpdate()
    {
        if (bounce)
        {
            bounce = false;
            StartCoroutine(Bounce());
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject == box)
    //    {
    //        if (Box.damageActive)
    //        {
    //            Vector2 launch = (boxRB.position - bumperRB.position).normalized * bounceMagnitude;
    //            BoxVelocity.velocitiesX[0] = launch.x;
    //            collision.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.GetComponent<Rigidbody2D>().velocity.x, launch.y);
    //            StartCoroutine(Bounce());
    //            Debug.Log("you are here");
    //        }
    //        else if (Box.boxWasPulsed == false)
    //        {
    //            Box.boxPulsedWhileInvulnerable = true;
    //            Box.boxWasPulsed = true;
    //            Box.boxEnemyPulseDirection = (boxRB.position - bumperRB.position).normalized;
    //            Box.boxEnemyPulseMagnitude = bounceMagnitude;
    //            StartCoroutine(Bounce());
    //        }
    //    }
    //    if (collision.gameObject.GetComponent<EnemyManager>() != null)
    //    {
    //        Vector2 direction = new Vector2(collision.transform.position.x - bumperRB.position.x, collision.transform.position.y - bumperRB.position.y).normalized;
    //        collision.gameObject.GetComponent<EnemyManager>().outsidePulseDirection = direction;
    //        collision.gameObject.GetComponent<EnemyManager>().outsidePulseActive = true;
    //    }
    //    if (collision.gameObject.GetComponent<Grenade>() != null)
    //    {
    //        Rigidbody2D grenadeRB = collision.GetComponent<Rigidbody2D>();
    //        Vector2 enemyReflectVector = (grenadeRB.position - bumperRB.position).normalized;
    //        grenadeRB.velocity = enemyReflectVector * bounceMagnitude;
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 launch = -collision.GetContact(0).normal;
        if (collision.gameObject == box)
        {
            if (Box.damageActive)
            {
                BoxVelocity.velocitiesX[0] = launch.x;
                collision.collider.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.collider.GetComponent<Rigidbody2D>().velocity.x, launch.y);
                bounce = true;
            }
            else if (Box.boxWasPulsed == false)
            {
                Box.boxPulsedWhileInvulnerable = true;
                Box.boxWasPulsed = true;
                Box.boxEnemyPulseDirection = launch;
                Box.boxEnemyPulseMagnitude = bounceMagnitude;
                bounce = true;
            }
        }

        if (collision.gameObject.GetComponent<EnemyManager>() != null)
        {
            //Vector2 direction = new Vector2(collision.transform.position.x - bumperRB.position.x, collision.transform.position.y - bumperRB.position.y).normalized;
            //collision.gameObject.GetComponent<EnemyManager>().outsidePulseDirection = direction;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseDirection = launch;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseActive = true;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseMagnitude = bounceMagnitude;
            bounce = true;
        }


        if (collision.gameObject.GetComponent<Grenade>() != null)
        {
            Rigidbody2D grenadeRB = collision.collider.GetComponent<Rigidbody2D>();
            //Vector2 enemyReflectVector = (grenadeRB.position - bumperRB.position).normalized;
            //grenadeRB.velocity = enemyReflectVector * bounceMagnitude;
            grenadeRB.velocity = launch * bounceMagnitude;
            bounce = true;
        }
    }

    IEnumerator Bounce()
    {
        yield return new WaitForFixedUpdate();
        float mult = 1.3f;
        transform.localScale = Vector2.one * initialScale * mult;
        float shrinkSpeed = 1;
        float timer = 0;
        while (transform.localScale.y > initialScale.y && bounce == false)
        {
            if (circle)
            {
                circleCol.radius = initialColRadius * (initialScale.y / transform.localScale.y);
                
            }
            else
            {
                boxCol.size = initialColSize * (initialScale.y / transform.localScale.y);
            }
            transform.localScale = new Vector2(transform.localScale.x - shrinkSpeed * initialScale.x * Time.deltaTime, transform.localScale.y - shrinkSpeed * initialScale.y * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector2.one * initialScale;
        if (circle)
        {
            circleCol.radius = initialColRadius;
        }
        else
        {
            boxCol.size = initialColSize;
        }
    }
}
