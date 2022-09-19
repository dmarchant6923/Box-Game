using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    Color explosionColor;
    public float explosionRadius = 1;
    public float explosionDamage = 30;
    [HideInInspector] public bool damageEnemies = false;
    [HideInInspector] public bool aestheticExplosion = false;
    [HideInInspector] public bool circleExplosion = true;
    [HideInInspector] public bool overrideDamageDirection = false;
    [HideInInspector] public Vector2 newDamageDirection;

    public static float closeExplosionDamageMult = 1.6f;

    Rigidbody2D boxRB;
    Vector2 vectorToBox;

    RaycastHit2D explosion_RayToItem;

    int boxLM;
    int enemyLM;
    int obstacleAndBoxLM;
    int obstacleLM;

    void Start()
    {
        explosionColor = this.GetComponent<Renderer>().material.color;
        transform.localScale = new Vector2(explosionRadius * 2, explosionRadius * 2);

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxLM = LayerMask.GetMask("Box");
        enemyLM = LayerMask.GetMask("Enemies");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Hazards", "Box");
        obstacleLM = LayerMask.GetMask("Obstacles");
        vectorToBox = new Vector2(boxRB.position.x - transform.position.x, boxRB.position.y - transform.position.y).normalized;

        if (aestheticExplosion == false)
        {
            RaycastHit2D explosionClose = Physics2D.CircleCast(transform.position, explosionRadius / 4,
                new Vector2(0, 0), 0f, boxLM);
            RaycastHit2D[] explosion = Physics2D.CircleCastAll(transform.position, explosionRadius,
                new Vector2(0, 0), 0f, LayerMask.GetMask("Box", "Enemies", "Enemy Device"));
            if (circleExplosion == false)
            {
                explosionClose = Physics2D.BoxCast(transform.position, Vector2.one * (explosionRadius / 2), 0, Vector2.zero, 0, boxLM);
                explosion = Physics2D.BoxCastAll(transform.position, Vector2.one * (explosionRadius * 2), 0, Vector2.zero, LayerMask.GetMask("Box", "Enemies", "Enemy Device"));
                explosionRadius *= 10;
            }
            foreach (RaycastHit2D item in explosion)
            {
                Vector2 vectorToItem = new Vector2(item.transform.position.x - transform.position.x, item.transform.position.y - transform.position.y);
                if (Tools.LineOfSight(transform.position, vectorToItem) == false)
                {
                    continue;
                }

                if (1 << item.collider.gameObject.layer == boxLM)
                {
                    Box.activateDamage = true;
                    Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - transform.position.x), 1).normalized;
                    if (overrideDamageDirection)
                    {
                        Box.boxDamageDirection = newDamageDirection;
                    }
                    Box.boxWasBurned = true;
                    if (explosionClose.collider != null)
                    {
                        explosionDamage *= closeExplosionDamageMult;
                    }
                    Box.damageTaken = explosionDamage;
                }

                // if the explosion sees an enemy object
                if (1 << item.collider.gameObject.layer == enemyLM && item.transform.root.GetComponent<EnemyManager>() != null)
                {
                    EnemyManager EM = item.transform.root.GetComponent<EnemyManager>();
                    Rigidbody2D RB = item.transform.root.GetComponentInChildren<Rigidbody2D>();
                    // if the explosion isn't set to damage enemies AND the enemy itself won't take damage from normal explosions, push the enemy
                    if (EM.normalExplosionsWillDamage == false && damageEnemies == false)
                    {
                        if (RB != null && RB.isKinematic == false && EM.explosionsWillPush)
                        {
                            Vector2 forceVector = new Vector2(RB.position.x - transform.position.x, RB.position.y - transform.position.y);
                            RB.AddForce(forceVector * 2, ForceMode2D.Impulse);
                            while (RB.velocity.magnitude < 15)
                            {
                                RB.AddForce(forceVector, ForceMode2D.Impulse);
                            }
                        }
                        EM.enemyIsFrozen = false;
                    }
                    // otherwise deal damage to the enemy
                    else
                    {
                        EM.enemyWasDamaged = true;
                    }
                }
                if (1 << item.collider.gameObject.layer == LayerMask.GetMask("Enemy Device"))
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
            float distToBox = new Vector2(boxRB.position.x - transform.position.x, boxRB.position.y - transform.position.y).magnitude;
            if (distToBox <= 30 && GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
            {
                CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
                camScript.startCamShake = true;
                camScript.shakeInfo = new Vector2(explosionDamage,
                    new Vector2(transform.position.x - boxRB.position.x, transform.position.y - boxRB.position.y).magnitude);
            }
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

        //Debug.DrawRay(transform.position + Vector3.up * explosionRadius, Vector2.down * explosionRadius * 2);
        //Debug.DrawRay(transform.position + Vector3.up * explosionRadius / 4, Vector2.down * explosionRadius / 2, Color.green);
        //Debug.DrawRay(transform.position + Vector3.right * explosionRadius, Vector2.left * explosionRadius * 2);
        //Debug.DrawRay(transform.position + Vector3.right * explosionRadius / 4, Vector2.left * explosionRadius / 2, Color.green);
    }
}
