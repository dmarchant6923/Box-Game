using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarManProjectile : MonoBehaviour
{
    Rigidbody2D rb;

    public GameObject starMan;
    GameObject newStarMan;

    public GameObject starBullet;
    GameObject newStarBullet;

    public GameObject explosion;
    GameObject newExplosion;
    float explosionRadius = 2.5f;
    float explosionDamage = 30;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 500;

        StartCoroutine(StarParticles());
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<MovingObjects>() != null && collision.collider.GetComponent<MovingObjects>().interactableObstacle)
        {
            newStarMan = Instantiate(starMan, rb.position, Quaternion.identity);

            newExplosion = Instantiate(explosion, rb.position, Quaternion.identity);
            newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
            newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;

            Destroy(gameObject);
        }
    }
    IEnumerator StarParticles()
    {
        float interval = 0.2f;

        while (true)
        {
            float randInterval = interval * 0.5f + interval * Random.Range(0f, 1f);
            yield return new WaitForSeconds(randInterval);
            newStarBullet = Instantiate(starBullet, rb.position + Random.insideUnitCircle * transform.localScale.x / 2, Quaternion.identity);
            newStarBullet.GetComponent<BulletScript>().bulletDespawnWindow = 5;
            newStarBullet.GetComponent<BulletScript>().bulletDamage = 0;
            newStarBullet.GetComponent<Rigidbody2D>().velocity = Vector2.down * 5;
            newStarBullet.GetComponent<StarBullet>().aestheticBullet = true;
        }
    }
}
