using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlginkrakEye : MonoBehaviour
{
    Transform box;

    Vector2 vectorToBox;
    float angletoBox;

    public Transform spriteMask;
    public Transform eyeLine;
    Transform eye;
    float initialYScale;
    float maskPosition = 0;
    EnemyManager bossEM;
    Blginkrak bossScript;
    public bool damageBlink = false;

    bool forceBlink = false;

    public bool isBoss = true;
    SpikeSentry sentryScript;
    bool scared = false;
    public GameObject sweat;
    GameObject newSweat;


    void Start()
    {
        box = GameObject.Find("Box").transform;
        eye = transform.GetChild(0);
        bossEM = transform.root.GetComponent<EnemyManager>();
        initialYScale = spriteMask.localScale.y;

        if (isBoss)
        {
            bossScript = transform.root.GetComponent<Blginkrak>();
        }
        else
        {
            sentryScript = transform.root.GetComponent<SpikeSentry>();
        }

        StartCoroutine(Blink());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        vectorToBox = (box.position - transform.position).normalized;

        if (isBoss)
        {
            angletoBox = -Mathf.Atan2(vectorToBox.x, vectorToBox.y) * Mathf.Rad2Deg;
            if (damageBlink == false && bossScript.dashActive == false && forceBlink == false)
            {
                transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, angletoBox, 500 * Time.fixedDeltaTime));
            }
            if (bossScript.dashActive)
            {
                Vector2 vector = new Vector2(bossScript.dashDirection, 0);
                float angle = -Mathf.Atan2(vector.x, vector.y) * Mathf.Rad2Deg;
                transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, angle, 500 * Time.fixedDeltaTime));
            }
        }
        else if (damageBlink == false)
        {
            angletoBox = -Mathf.Atan2(sentryScript.visibleVectorToBox.x, sentryScript.visibleVectorToBox.y) * Mathf.Rad2Deg;
            if (sentryScript.stopIdleMovement)
            {
                transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, angletoBox, 500 * Time.fixedDeltaTime));
            }
            else
            {
                float directionAngle = -Mathf.Atan2(sentryScript.direction, -0.5f) * Mathf.Rad2Deg;
                transform.localEulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, directionAngle, 500 * Time.fixedDeltaTime));
            }
        }
        spriteMask.position = eye.position + Vector3.up * maskPosition;
        eyeLine.position = eye.position;

        if (bossEM.hitstopImpactActive && damageBlink == false && bossEM.enemyIsInvulnerable)
        {
            damageBlink = true;
            StartCoroutine(BlinkDamage());
        }

        if (isBoss == false && sentryScript.sentinelsKilled[0] && sentryScript.sentinelsKilled[1] && scared == false)
        {
            StartCoroutine(ScaredCR());
        }
    }

    IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.8f, 5f));
            int blinks = Random.Range(0, 2) + 1;
            while (blinks > 0 && damageBlink == false && forceBlink == false)
            {
                while (bossEM.enemyIsFrozen)
                {
                    yield return new WaitForFixedUpdate();
                }
                spriteMask.localScale = new Vector2(spriteMask.localScale.x, 0);
                yield return new WaitForSeconds(0.15f);
                while (bossEM.enemyIsFrozen)
                {
                    yield return new WaitForFixedUpdate();
                }
                if (damageBlink == false && forceBlink == false)
                {
                    spriteMask.localScale = new Vector2(spriteMask.localScale.x, initialYScale);
                }
                if (blinks > 0)
                {
                    blinks--;
                    yield return new WaitForSeconds(0.08f);
                }
                while (damageBlink || forceBlink)
                {
                    yield return null;
                }
            }
        }
    }
    IEnumerator BlinkDamage()
    {
        float window = 1.2f;
        float timer = 0;
        float initialTarget = 0.07f;
        float target = initialTarget;
        while (timer < window)
        {
            spriteMask.localScale = new Vector2(spriteMask.localScale.x, 0);
            transform.localPosition = new Vector2(Mathf.MoveTowards(transform.localPosition.x, target, 2f * Time.deltaTime), transform.localPosition.y);
            eyeLine.position = eye.position;
            if ((target == initialTarget && transform.localPosition.x >= target) || (target == -initialTarget && transform.localPosition.x <= target))
            {
                target *= -1;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = Vector2.zero;
        eyeLine.position = eye.position;
        if (forceBlink == false)
        {
            spriteMask.localScale = new Vector2(spriteMask.localScale.x, initialYScale);
        }
        damageBlink = false;
    }
    public IEnumerator ForceBlink(float duration)
    {
        forceBlink = true;
        spriteMask.localScale = new Vector2(spriteMask.localScale.x, 0);
        yield return new WaitForSeconds(duration);
        if (damageBlink == false)
        {
            spriteMask.localScale = new Vector2(spriteMask.localScale.x, initialYScale);
        }
        forceBlink = false;
    }

    IEnumerator ScaredCR()
    {
        scared = true;
        float window = 1.2f;
        yield return new WaitForSeconds(window / 2);

        Vector2 facingVector = Tools.AngleToVector(transform.localEulerAngles.z);
        while (true)
        {
            Vector2 position = new Vector2(transform.position.x - Mathf.Sign(facingVector.x) * 0.5f, transform.position.y + transform.lossyScale.y / 2);
            newSweat = Instantiate(sweat, position, Quaternion.identity);
            newSweat.transform.parent = bossEM.transform;
            SpriteRenderer sprite = sweat.GetComponent<SpriteRenderer>();
            Color color = sprite.color;
            color.a = 0.7f;
            sprite.color = color;
            StartCoroutine(SweatDrips(newSweat));

            float timer = 0;
            while (timer < window)
            {
                facingVector = new Vector2(Mathf.Cos(transform.localEulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2),
                    Mathf.Sin(transform.localEulerAngles.z * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                maskPosition = 0.17f + facingVector.y * 0.08f;

                if (bossEM.enemyIsFrozen == false)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }
    }
    IEnumerator SweatDrips(GameObject sweat)
    {
        float window = 0.8f;
        float timer = 0;
        while (timer < window && sentryScript.GetComponent<EnemyManager>().enemyWasKilled == false)
        {
            float deltaTime = Time.fixedDeltaTime;
            if (bossEM.enemyIsFrozen)
            {
                deltaTime = 0;
            }

            sweat.transform.position += Vector3.down * deltaTime * 0.5f;
            if (timer > window * 0.75f)
            {
                SpriteRenderer sprite = sweat.GetComponent<SpriteRenderer>();
                Color color = sprite.color;
                color.a -= (4 * 0.7f / window) * deltaTime;
                sprite.color = color;
            }
            Vector2 facingVector = Tools.AngleToVector(transform.localEulerAngles.z);
            Vector2 position = new Vector2(transform.position.x - Mathf.Sign(facingVector.x) * 0.5f, sweat.transform.position.y);
            sweat.transform.position = position;

            timer += deltaTime;
            yield return new WaitForFixedUpdate();
        }
        Destroy(sweat);
    }
}
