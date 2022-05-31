using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    GameObject box;
    Rigidbody2D boxRB;
    Rigidbody2D bumperRB;

    CircleCollider2D trigger;
    CircleCollider2D col;
    float initialColRadius;

    public float bounceMagnitude = 20;
    float initialScale;

    void Start()
    {
        box = GameObject.Find("Box");
        boxRB = box.GetComponent<Rigidbody2D>();
        bumperRB = GetComponent<Rigidbody2D>();
        initialScale = transform.lossyScale.y;

        foreach (CircleCollider2D collider in GetComponents<CircleCollider2D>())
        {
            if (collider.isTrigger)
            {
                trigger = collider;
            }
            else if (collider.isTrigger == false)
            {
                col = collider;
            }
        }

        initialColRadius = col.radius;
        //col.enabled = false;

    }

    private void FixedUpdate()
    {
        //if (Box.damageActive)
        //{
        //    col.radius = trigger.radius * 0.9f;
        //}
        //else
        //{
        //    col.radius = initialColRadius;
        //}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == box)
        {
            if (Box.damageActive)
            {
                Vector2 launch = (boxRB.position - bumperRB.position).normalized * bounceMagnitude;
                BoxVelocity.velocitiesX[0] = launch.x;
                collision.GetComponent<Rigidbody2D>().velocity = new Vector2(collision.GetComponent<Rigidbody2D>().velocity.x, launch.y);
                StartCoroutine(Bounce());
                Debug.Log("you are here");
            }
            else if (Box.boxWasPulsed == false)
            {
                Box.boxPulsedWhileInvulnerable = true;
                Box.boxWasPulsed = true;
                Box.boxEnemyPulseDirection = (boxRB.position - bumperRB.position).normalized;
                Box.boxEnemyPulseMagnitude = bounceMagnitude;
                StartCoroutine(Bounce());
            }
        }
        if (collision.gameObject.GetComponent<EnemyManager>() != null)
        {
            Vector2 direction = new Vector2(collision.transform.position.x - bumperRB.position.x, collision.transform.position.y - bumperRB.position.y).normalized;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseDirection = direction;
            collision.gameObject.GetComponent<EnemyManager>().outsidePulseActive = true;
        }
        if (collision.gameObject.GetComponent<Grenade>() != null)
        {
            Rigidbody2D grenadeRB = collision.GetComponent<Rigidbody2D>();
            Vector2 enemyReflectVector = (grenadeRB.position - bumperRB.position).normalized;
            grenadeRB.velocity = enemyReflectVector * bounceMagnitude;
        }
    }

    IEnumerator Bounce()
    {
        float mult = 1.3f;
        transform.localScale = Vector2.one * initialScale * mult;
        float shrinkSpeed = 5;
        float timer = 0;
        while (transform.localScale.y > initialScale)
        {
            col.radius = initialColRadius * (initialScale / transform.localScale.y);
            transform.localScale = new Vector2(transform.localScale.x - shrinkSpeed * Time.deltaTime, transform.localScale.y - shrinkSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector2.one * initialScale;
    }
}
