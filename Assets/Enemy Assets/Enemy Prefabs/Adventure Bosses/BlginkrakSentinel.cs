using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlginkrakSentinel : MonoBehaviour
{
    public Blginkrak bossScript;
    EnemyManager EM;
    Rigidbody2D bossRB;
    Rigidbody2D sentinelRB;
    Rigidbody2D boxRB;
    public int index;

    bool boomerangCR = false;
    int damageDirection = 1;
    float damage = 30;
    bool sentinelHitstopActive = false;

    bool touchingThisEnemy;

    void Start()
    {
        EM = GetComponent<EnemyManager>();
        sentinelRB = GetComponent<Rigidbody2D>();
        bossRB = bossScript.GetComponent<Rigidbody2D>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //bool thisEnemyFound = false;
        //foreach (RaycastHit2D enemy in Box.attackRayCast)
        //{
        //    if (enemy.collider != null && enemy.collider.gameObject == this.gameObject)
        //    {
        //        thisEnemyFound = true;
        //    }
        //}
        //if (thisEnemyFound)
        //{
        //    touchingThisEnemy = true;
        //    EM.touchingThisEnemy = true;
        //}
        //else
        //{
        //    touchingThisEnemy = false;
        //    EM.touchingThisEnemy = false;
        //}

        //if (touchingThisEnemy && boomerangCR && sentinelHitstopActive == false && Box.isInvulnerable == false)
        //{
        //    Box.activateDamage = true;
        //    Box.damageTaken = damage;
        //    Box.boxDamageDirection = new Vector2(damageDirection, 1).normalized;
        //    StartCoroutine(EnemyHitstop());
        //}

        if (boomerangCR)
        {
            float radius = 0.4f;
            RaycastHit2D hitbox = Physics2D.CircleCast(sentinelRB.position, radius, Vector2.zero, 0, LayerMask.GetMask("Box"));
            if (hitbox.collider != null && Box.isInvulnerable == false)
            {
                Box.activateDamage = true;
                Box.damageTaken = damage;
                Box.boxDamageDirection = new Vector2(damageDirection, 1).normalized;
                StartCoroutine(EnemyHitstop());
            }
            if (bossScript.debugEnabled)
            {
                Debug.DrawRay(sentinelRB.position + Vector2.up * radius, Vector2.down * radius * 2);
                Debug.DrawRay(sentinelRB.position + Vector2.right * radius, Vector2.left * radius * 2);
            }
        }
    }

    void FixedUpdate()
    {
        if (bossScript.sentinelBoomerang[index] == true && boomerangCR == false)
        {
            StartCoroutine(Boomerang());
        }

        if (boomerangCR && sentinelHitstopActive == false)
        {
            sentinelRB.rotation += 3000 * Time.fixedDeltaTime;
        }
    }

    IEnumerator EnemyHitstop()
    {
        sentinelHitstopActive = true;
        Vector2 enemyHitstopVelocity = sentinelRB.velocity;
        float enemyHitstopRotationSlowDown = 10;
        sentinelRB.velocity = new Vector2(0, 0);
        sentinelRB.angularVelocity /= enemyHitstopRotationSlowDown;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        if (EM.shockActive)
        {
            EM.shockActive = false;
        }
        sentinelRB.angularVelocity *= enemyHitstopRotationSlowDown;
        sentinelRB.velocity = enemyHitstopVelocity;
        sentinelHitstopActive = false;
    }
    IEnumerator Boomerang()
    {
        boomerangCR = true;
        float maxSpeed = 35;
        float minSpeed = 3;
        Vector2 vectorToBox = boxRB.position - sentinelRB.position;
        float distance = Mathf.Max(15, vectorToBox.magnitude * 1.2f);
        Vector2 target = sentinelRB.position + (vectorToBox.normalized * distance);
        damageDirection = (int)Mathf.Sign(target.x - sentinelRB.position.x);
        while (Mathf.Abs((sentinelRB.position - target).magnitude) > 0.05f)
        {
            float speed = minSpeed + maxSpeed * (sentinelRB.position - target).magnitude / distance;
            if (sentinelHitstopActive == false)
            {
                //sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, target, speed * Time.fixedDeltaTime);
                sentinelRB.velocity = (target - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }

        float window = 0.8f;
        float timer = 0;
        while (timer < window)
        {
            sentinelRB.velocity = Vector2.zero;
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        damageDirection = -(int)Mathf.Sign(target.x - sentinelRB.position.x);
        while (Mathf.Abs((sentinelRB.position - (bossRB.position + bossScript.sentinelPositions[index])).magnitude) > 0.5f)
        {
            float speed = minSpeed + maxSpeed * (sentinelRB.position - target).magnitude / distance;
            if (sentinelHitstopActive == false)
            {
                //sentinelRB.position = Vector2.MoveTowards(sentinelRB.position, bossRB.position + bossScript.sentinelPositions[index], speed * Time.fixedDeltaTime);
                sentinelRB.velocity = (bossRB.position + bossScript.sentinelPositions[index] - sentinelRB.position).normalized * speed;
            }
            yield return new WaitForFixedUpdate();
        }
        sentinelRB.velocity = Vector2.zero;
        boomerangCR = false;
        bossScript.sentinelBoomerang[index] = false;
        bossScript.sentinelIdle[index] = true;
    }
}
