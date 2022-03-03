using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    Color explosionColor;
    public float explosionRadius = 1;
    public float explosionDamage = 30;
    [HideInInspector] public bool damageEnemies = false;

    Rigidbody2D boxRB;
    Vector2 vectorToBox;

    RaycastHit2D explosion_RayToItem;

    int boxLM;
    int enemyLM;
    int obstacleAndBoxLM;

    void Start()
    {
        explosionColor = this.GetComponent<Renderer>().material.color;
        transform.localScale = new Vector2(explosionRadius * 2, explosionRadius * 2);

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Hazards", "Box");
        vectorToBox = new Vector2(boxRB.position.x - transform.position.x, boxRB.position.y - transform.position.y).normalized;

        RaycastHit2D explosionClose = Physics2D.CircleCast(transform.position, explosionRadius / 4,
            new Vector2(0, 0), 0f, boxLM);
        RaycastHit2D[] explosion = Physics2D.CircleCastAll(transform.position, explosionRadius,
            new Vector2(0, 0), 0f, LayerMask.GetMask("Box", "Enemies", "Enemy Device"));
        foreach (RaycastHit2D item in explosion)
        {
            Vector2 vectorToItem = new Vector2(item.transform.position.x - transform.position.x, item.transform.position.y - transform.position.y);
            if (1 << item.collider.gameObject.layer == boxLM)
            {
                // if the explosion sees the box and there's no obstacles in between, damage the box.
                RaycastHit2D[] explosion_RayToBox = Physics2D.RaycastAll(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Box"));

                float boxDist = 1000;
                float obstacleDist = 1000;
                foreach (RaycastHit2D col in explosion_RayToBox)
                {
                    if (col.collider != null && 1 << col.collider.gameObject.layer == boxLM)
                    {
                        boxDist = col.distance;
                    }
                    if (col.collider != null && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles") && col.collider.gameObject.tag != "Fence")
                    {
                        obstacleDist = col.distance;
                    }
                }
                if (boxDist < obstacleDist)
                {
                    Box.activateDamage = true;
                    Box.damageTaken = explosionDamage;
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - transform.position.x), 1).normalized;
                }
            }
            if (1 << item.collider.gameObject.layer == enemyLM)
            {
                explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Enemies"));
            }
            if (1 << item.collider.gameObject.layer == LayerMask.GetMask("Enemy Device"))
            {
                explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Enemy Device"));
            }


            // if the explosion sees an enemy object
            if (1 << item.collider.gameObject.layer == enemyLM && item.transform.root.GetComponent<EnemyManager>() != null &&
                explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == enemyLM)
            {
                EnemyManager EM = item.transform.root.GetComponent<EnemyManager>();
                Rigidbody2D RB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                // if the explosion isn't set to damage enemies AND the enemy itself won't take damage from normal explosions, push the enemy
                if (EM.normalExplosionsWillDamage == false && damageEnemies == false)
                {
                    if (RB != null && RB.isKinematic == false)
                    {
                        Vector2 forceVector = new Vector2(RB.position.x - transform.position.x, RB.position.y - transform.position.y);
                        RB.AddForce(forceVector * 2, ForceMode2D.Impulse);
                        while (RB.velocity.magnitude < 15)
                        {
                            RB.AddForce(forceVector, ForceMode2D.Impulse);
                        }
                    }
                }
                // otherwise deal damage to the enemy
                else
                {
                    EM.enemyWasDamaged = true;
                }
            }
            if (1 << item.collider.gameObject.layer == LayerMask.GetMask("Enemy Device") &&
                explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == LayerMask.GetMask("Enemy Device"))
            {
                if (item.collider.GetComponent<ProximityMine>() != null)
                {
                    item.collider.GetComponent<ProximityMine>().remoteExplode = true;
                }
                if (item.collider.GetComponent<Grenade>() != null)
                {
                    item.collider.GetComponent<Grenade>().remoteExplode = true;
                }
            }
            if (item.transform.root.GetComponent<HitSwitch>() != null)
            {
                item.transform.GetComponent<HitSwitch>().Hit();
            }
        }
        if (explosionClose.collider != null)
        {
            explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToBox, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Hazards", "Box"));
            if (explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == boxLM)
            {
                Box.activateDamage = true;
                Box.damageTaken = explosionDamage * 1.6f;
                Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - transform.position.x), 1).normalized;
            }
        }
        float distToBox = new Vector2(boxRB.position.x - transform.position.x, boxRB.position.y - transform.position.y).magnitude;
        if (distToBox <= 20 && GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
        {
            CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
            camScript.startCamShake = true;
            camScript.shakeInfo = new Vector2(explosionDamage,
                new Vector2(transform.position.x - boxRB.position.x, transform.position.y - boxRB.position.y).magnitude);
        }
    }

    // Update is called once per frame
    void Update()
    {
        explosionColor.a -= Time.deltaTime * 1.5f;
        this.GetComponent<Renderer>().material.color = explosionColor;
        if (explosionColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
