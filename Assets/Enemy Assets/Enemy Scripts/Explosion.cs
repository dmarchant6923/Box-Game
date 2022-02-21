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
                explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Hazards", "Box"));
            }
            if (1 << item.collider.gameObject.layer == enemyLM)
            {
                explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Hazards", "Enemies"));
            }
            if (1 << item.collider.gameObject.layer == LayerMask.GetMask("Enemy Device"))
            {
                explosion_RayToItem = Physics2D.Raycast(transform.position, vectorToItem.normalized, explosionRadius,
                    LayerMask.GetMask("Obstacles", "Hazards", "Enemy Device"));
            }

            // if the explosion sees the box and there's no obstacles in between, damage the box.
            if (explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == boxLM &&
                1 << item.collider.gameObject.layer == boxLM)
            {
                Box.activateDamage = true;
                Box.damageTaken = explosionDamage;
                Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - transform.position.x), 1).normalized;
            }
            // if the explosion sees an enemy object and the object contains EnemyManager script (AKA multipleParts = false)
            if (1 << item.collider.gameObject.layer == enemyLM && item.collider.gameObject.GetComponent<EnemyManager>() != null &&
                explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == enemyLM)
            {
                // if the explosion isn't set to damage enemies AND the enemy itself won't take damage from normal explosions, push the enemy
                if (item.collider.gameObject.GetComponent<EnemyManager>().normalExplosionsWillDamage == false && damageEnemies == false)
                {
                    if (item.collider.gameObject.GetComponent<Rigidbody2D>() != null &&
                        item.collider.gameObject.GetComponent<Rigidbody2D>().isKinematic == false)
                    {
                        Rigidbody2D itemRB = item.collider.gameObject.GetComponent<Rigidbody2D>();
                        Vector2 forceVector = new Vector2(itemRB.position.x - transform.position.x, itemRB.position.y - transform.position.y);
                        itemRB.AddForce(forceVector * 2, ForceMode2D.Impulse);
                        while (itemRB.velocity.magnitude < 15)
                        {
                            itemRB.AddForce(forceVector, ForceMode2D.Impulse);
                        }
                    }
                }
                // otherwise deal damage to the enemy
                else
                {
                    item.collider.gameObject.GetComponent<EnemyManager>().enemyWasDamaged = true;
                }
            }
            // if the explosion sees an enemy object and the object does NOT contain EnemyManager script (AKA multipleParts == true)
            else if (1 << item.collider.gameObject.layer == enemyLM && item.collider.gameObject.GetComponent<EnemyManager>() == null &&
                explosion_RayToItem.collider != null && 1 << explosion_RayToItem.collider.gameObject.layer == enemyLM)
            {
                Transform itemParent = item.collider.transform.root;
                if (itemParent.GetComponent<EnemyManager>().normalExplosionsWillDamage == false && damageEnemies == false)
                {
                    Rigidbody2D itemRB = itemParent.GetComponentInChildren<Rigidbody2D>();
                    if (itemRB.isKinematic == false)
                    {
                        Vector2 forceVector = new Vector2(itemRB.position.x - transform.position.x, itemRB.position.y - transform.position.y);
                        itemRB.AddForce(forceVector * 2, ForceMode2D.Impulse);
                        while (itemRB.velocity.magnitude < 0)
                        {
                            itemRB.AddForce(forceVector, ForceMode2D.Impulse);
                        }
                    }
                }
                else
                {
                    itemParent.GetComponent<EnemyManager>().enemyWasDamaged = true;
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
