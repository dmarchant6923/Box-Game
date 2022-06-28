using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    Rigidbody2D missileRB;

    public GameObject explosion;
    GameObject newExplosion;

    public GameObject fire;
    GameObject newFire;

    public float speed = 10;
    public Vector2 direction = Vector2.left;
    public float damage = 40;
    public float explosionRadius = 5;

    int obstacleLM;

    private void Start()
    {
        direction = direction.normalized;
        missileRB = GetComponent<Rigidbody2D>();
        missileRB.rotation = Tools.VectorToAngle(direction);
        missileRB.velocity = direction * speed;

        newFire = Instantiate(fire, missileRB.position - (direction * 0.5f), Quaternion.identity);
        newFire.transform.localScale *= (transform.localScale.y / 0.75f) * 0.6f;
        newFire.transform.eulerAngles = new Vector3(0, 0, -missileRB.rotation);
        newFire.GetComponent<Fire>().spawnSmoke = false;

        damage /= Explosion.closeExplosionDamageMult;

        obstacleLM = LayerMask.GetMask("Obstacles");
    }

    private void Update()
    {
        newFire.transform.position = missileRB.position - (direction * 0.5f);
        newFire.transform.eulerAngles = new Vector3(0, 0, -missileRB.rotation);
    }

    void Explode(bool overrideDamageDirection)
    {
        newExplosion = Instantiate(explosion, missileRB.position, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        newExplosion.GetComponent<Explosion>().explosionDamage = damage;
        if (overrideDamageDirection)
        {
            newExplosion.GetComponent<Explosion>().overrideDamageDirection = true;
            newExplosion.GetComponent<Explosion>().newDamageDirection = new Vector2(missileRB.velocity.normalized.x, 1).normalized;
        }
        //Destroy(newFire);
        Destroy(gameObject);

    }

    private void OnDestroy()
    {
        Destroy(newFire);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Box>() != null)
        {
            Explode(true);
        }
        else if (1 << collision.gameObject.layer == obstacleLM && collision.isTrigger == false)
        {
            Explode(false);
        }
    }
}
