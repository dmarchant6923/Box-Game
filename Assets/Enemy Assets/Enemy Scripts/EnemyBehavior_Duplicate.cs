using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior_Duplicate : MonoBehaviour
{
    public Transform body;
    public Transform mask1;
    public Transform maskMid;
    public Transform mask2;
    public Transform fallingSand;

    public float flipTime = 6;
    float flipTransitionTime = 1f;
    bool upright = true;
    bool flipTransition = false;

    float maskStartPos = -0.65f;
    float maskEndPos = 0.14f;
    float maskIdlePos = -0.85f;

    float midStartPos = -0.225f;
    float midEndPos = -0.075f;
    float midStartScale = 0.45f;
    float midEndScale = 0.15f;

    float linestartpos;


    void Start()
    {
        StartCoroutine(MaskMovement1());
        StartCoroutine(MaskMovementMid());
        StartCoroutine(MaskMovement2());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator MaskMovement1()
    {
        if (flipTransition)
        {
            float window = flipTransitionTime;
            float timer = 0;
            if (upright)
            {
                while (timer < window)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                mask1.transform.localPosition = new Vector2(mask1.transform.localPosition.x, maskIdlePos);
            }
            else
            {
                while (timer < window)
                {
                    mask1.transform.localPosition = Vector2.MoveTowards(mask1.transform.localPosition, new Vector2(mask1.transform.localPosition.x, maskStartPos),
                        Mathf.Abs(maskIdlePos - maskStartPos) / window * Time.deltaTime);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            flipTransition = false;
            upright = !upright;
        }
        else if (upright)
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                mask1.transform.localPosition = Vector2.MoveTowards(mask1.transform.localPosition, new Vector2(mask1.transform.localPosition.x, maskEndPos),
                    Mathf.Abs(maskStartPos - maskEndPos) / window * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
            flipTransition = true;
            StartCoroutine(FlipGlass());
        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            flipTransition = true;
            StartCoroutine(FlipGlass());
        }
        yield return null;
        StartCoroutine(MaskMovement1());
        StartCoroutine(MaskMovement2());
        StartCoroutine(MaskMovementMid());
    }
    IEnumerator MaskMovementMid()
    {
        if (flipTransition)
        {
            float startpos = maskMid.localPosition.y;
            float targetPos = -midStartPos;
            if (upright == false)
            {
                targetPos = midStartPos;
            }


            while (flipTransition)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, targetPos),
                    Mathf.Abs(startpos - targetPos) / flipTransitionTime * Time.deltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midStartScale,
                    Mathf.Abs(midStartScale - midEndScale) / flipTransitionTime * Time.deltaTime));
                yield return null;
            }
        }
        else if (upright)
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, midEndPos),
                    Mathf.Abs(midStartPos - midEndPos) / window * Time.deltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midEndScale,
                    Mathf.Abs(midStartScale - midEndScale) / window * Time.deltaTime));
                timer += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                maskMid.transform.localPosition = Vector2.MoveTowards(maskMid.transform.localPosition, new Vector2(maskMid.transform.localPosition.x, -midEndPos),
                    Mathf.Abs(midStartPos - midEndPos) / window * Time.deltaTime);
                maskMid.localScale = new Vector2(maskMid.localScale.x, Mathf.MoveTowards(maskMid.localScale.y, midEndScale,
                    Mathf.Abs(midStartScale - midEndScale) / window * Time.deltaTime));
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }
    IEnumerator MaskMovement2()
    {
        if (flipTransition)
        {
            float window = flipTransitionTime;
            float timer = 0;
            if (upright == false)
            {
                while (timer < window)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                mask2.transform.localPosition = new Vector2(mask2.transform.localPosition.x, maskIdlePos);
            }
            else
            {
                while (timer < window)
                {
                    mask2.transform.localPosition = Vector2.MoveTowards(mask2.transform.localPosition, new Vector2(mask2.transform.localPosition.x, maskStartPos),
                        Mathf.Abs(maskIdlePos - maskStartPos) / window * Time.deltaTime);
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }
        else if (upright)
        {

        }
        else
        {
            float window = flipTime - flipTransitionTime;
            float timer = 0;
            while (timer < window)
            {
                mask2.transform.localPosition = Vector2.MoveTowards(mask2.transform.localPosition, new Vector2(mask2.transform.localPosition.x, maskEndPos),
                    Mathf.Abs(maskStartPos - maskEndPos) / window * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
        }
    }

    IEnumerator FlipGlass()
    {
        float window = flipTransitionTime;
        float timer = 0;
        float target = 180;
        if (upright == false)
        {
            target = 360;
        }
        while (timer < window)
        {
            body.eulerAngles = new Vector3(body.eulerAngles.x, body.eulerAngles.y,
                Mathf.MoveTowards(body.eulerAngles.z, target, 180 / flipTransitionTime * Time.deltaTime));
            timer += Time.deltaTime;
            yield return null;
        }
        if (target == 360)
        {
            body.eulerAngles = new Vector3(body.eulerAngles.x, body.eulerAngles.y, 0);
        }
    }
}
