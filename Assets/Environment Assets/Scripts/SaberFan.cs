using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberFan : MonoBehaviour
{
    Rigidbody2D rb;

    public bool rotate = false;
    public float angularVelocity = 80f;
    public float length = 4;

    public float damage = 30;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rotate)
        {
            rb.angularVelocity = angularVelocity;
        }

        foreach (CapsuleCollider2D saber in GetComponentsInChildren<CapsuleCollider2D>())
        {
            saber.transform.localScale = new Vector2(saber.transform.localScale.x, length / 2 + 0.5f);
            saber.transform.localPosition = new Vector2(-length / 2 + 0.5f, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Box>() != null && Box.isInvulnerable == false)
        {
            Box.activateDamage = true;
            Box.damageTaken = damage;
            Box.boxDamageDirection = new Vector2(Mathf.Sign(collision.transform.position.x - rb.position.x), 1).normalized;
            Box.boxWasBurned = true;
            StartCoroutine(Hitstop(true));
        }

        if (collision.GetComponent<EnemyManager>() != null && collision.GetComponent<EnemyManager>().enemyIsInvulnerable == false &&
            collision.isTrigger == false)
        {
            collision.GetComponent<EnemyManager>().enemyWasDamaged = true;
            StartCoroutine(Hitstop(false));
        }
    }

    IEnumerator Hitstop(bool box)
    {
        rb.angularVelocity /= 10;
        if (GetComponent<PathMovement>() != null)
        {
            GetComponent<PathMovement>().stopMovement = true;
        }
        yield return null;
        if (box)
        {
            while (Box.boxHitstopActive)
            {
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(Box.enemyHitstopDelay);
        }
        rb.angularVelocity = angularVelocity;
        if (GetComponent<PathMovement>() != null)
        {
            GetComponent<PathMovement>().stopMovement = false;
        }
    }
}
