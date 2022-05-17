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
    EnemyManager bossEM;
    Blginkrak bossScript;
    bool damageBlink = false;

    bool forceBlink = false;


    void Start()
    {
        box = GameObject.Find("Box").transform;
        eye = transform.GetChild(0);
        bossEM = transform.root.GetComponent<EnemyManager>();
        bossScript = transform.root.GetComponent<Blginkrak>();
        initialYScale = spriteMask.localScale.y;

        StartCoroutine(Blink());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        vectorToBox = (box.position - transform.position).normalized;
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
        spriteMask.position = eye.position;
        eyeLine.position = eye.position;

        if (bossEM.hitstopImpactActive && damageBlink == false)
        {
            damageBlink = true;
            StartCoroutine(BlinkDamage());
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
                while (damageBlink == false && forceBlink == false && spriteMask.localScale.y > 0)
                {
                    spriteMask.localScale = new Vector2(spriteMask.localScale.x, Mathf.MoveTowards(spriteMask.localScale.y, 0, 15 * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
                spriteMask.localScale = new Vector2(spriteMask.localScale.x, 0);
                yield return new WaitForSeconds(0.1f);
                while (damageBlink == false && forceBlink == false && spriteMask.localScale.y < initialYScale)
                {
                    spriteMask.localScale = new Vector2(spriteMask.localScale.x, Mathf.MoveTowards(spriteMask.localScale.y, initialYScale, 15 * Time.fixedDeltaTime));
                    yield return new WaitForFixedUpdate();
                }
                if (damageBlink == false && forceBlink == false)
                {
                    spriteMask.localScale = new Vector2(spriteMask.localScale.x, initialYScale);
                }
                if (blinks > 0)
                {
                    blinks--;
                    yield return new WaitForSeconds(0.05f);
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
}
