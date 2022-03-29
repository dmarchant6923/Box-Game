using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaCoil : MonoBehaviour
{
    public GameObject lightning;
    GameObject newLightning;
    float radius = 7;

    Rigidbody2D boxRB;

    int phase = 1;
    public float timePerPhase = 3;

    Vector2 breakoutPoint;
    SpriteRenderer topload;
    Vector3 toploadColorVector;
    Vector3 initialColor;
    Vector3 activeColor;
    Vector3 fireColor;
    float colorChangeSpeed = 0.6f;
    float initialColorChangeSpeed;

    List<SpriteRenderer> coilObjects;
    List<Color> coilColors = new List<Color> { };
    bool flash = false;

    bool shockActive = true;

    void Start()
    {
        topload = transform.GetChild(1).GetComponent<SpriteRenderer>();
        breakoutPoint = transform.GetChild(0).position;

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        initialColor = new Vector3(topload.color.r, topload.color.g, topload.color.b);
        activeColor = new Vector3(0.6f, 0.3f, 0.7f);
        fireColor = new Vector3(1, 0, 0);

        toploadColorVector = initialColor;
        colorChangeSpeed *= 3 / timePerPhase;
        initialColorChangeSpeed = colorChangeSpeed;

        coilObjects = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        foreach (SpriteRenderer item in coilObjects)
        {
            coilColors.Add(item.color);
        }

        phase = 1;
        StartCoroutine(PhaseChange());
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 color = initialColor;
        if (phase == 2)
        {
            color = activeColor;
        }
        else if (phase == 3)
        {
            color = fireColor;
        }
        toploadColorVector = Vector3.MoveTowards(toploadColorVector, color, colorChangeSpeed * Time.deltaTime);
        if (flash == false)
        {
            topload.color = new Color(toploadColorVector.x, toploadColorVector.y, toploadColorVector.z);
        }

        if (Box.pulseActive && (breakoutPoint - boxRB.position).magnitude < Box.pulseRadius)
        {
            RaycastHit2D rayToBox = Physics2D.Raycast(breakoutPoint, (boxRB.position - breakoutPoint).normalized, Box.pulseRadius, LayerMask.GetMask("Obstacles", "Box"));
            if (rayToBox.collider != null && rayToBox.collider.GetComponent<Box>() != null)
            {
                shockActive = false;
                phase = 0;
            }
        }
    }

    IEnumerator PhaseChange()
    {
        float window = timePerPhase;
        float timer = 0;
        if (phase > 3)
        {
            phase = 1;
            colorChangeSpeed = initialColorChangeSpeed;
        }
        if (phase == 2)
        {
            window *= 1.4f;
            colorChangeSpeed = initialColorChangeSpeed * 0.5f;
        }
        if (phase == 3)
        {
            window *= 0.6f;
            colorChangeSpeed = initialColorChangeSpeed;
        }
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        phase++;
        StartCoroutine(PhaseChange());
        if (phase == 2)
        {
            window = timePerPhase * 0.5f;
            timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            shockActive = true;
            StartCoroutine(Shock());
        }
        if (phase == 3)
        {

        }
        if (phase == 1 && shockActive)
        {
            StartCoroutine(lightning.GetComponent<Lightning>().LightningStrike(radius, 0, Lightning.baseThunderDamage, breakoutPoint, true, transform.GetChild(0).gameObject));
            StartCoroutine(ReleaseFlash());
            shockActive = false;
        }
    }

    IEnumerator Shock()
    {
        float window1 = 0.12f;
        StartCoroutine(ShockFlash());
        bool LtoR = true;
        yield return null;
        Vector2 pointA;
        Vector2 pointB;
        Vector2 toploadPosition = topload.transform.position;
        Vector2 toploadScale = topload.transform.lossyScale;
        Vector2 normal = (breakoutPoint - GetComponent<Rigidbody2D>().position).normalized;
        Vector2 perpendicular = Vector2.Perpendicular(normal);
        while (shockActive)
        {
            if (LtoR)
            {
                pointA = toploadPosition - (toploadScale.x * perpendicular * 2) + (Random.Range(-1f, 1f) * normal * toploadScale.y / 2);
                pointB = toploadPosition + (toploadScale.x * perpendicular * 2) + (Random.Range(-1f, 1f) * normal * toploadScale.y / 2);
                LtoR = false;
            }
            else
            {
                pointA = toploadPosition - (toploadScale.x * perpendicular * 2) + (Random.Range(-1f, 1f) * normal * toploadScale.y / 2);
                pointB = toploadPosition + (toploadScale.x * perpendicular * 2) + (Random.Range(-1f, 1f) * normal * toploadScale.y / 2);
                LtoR = true;
            }
            newLightning = Instantiate(lightning);
            newLightning.GetComponent<Lightning>().pointA = pointA;
            newLightning.GetComponent<Lightning>().pointB = pointB;
            newLightning.GetComponent<Lightning>().pointsPerUnit = 4;
            newLightning.GetComponent<Lightning>().aestheticElectricity = true;

            //Debug.DrawLine(toploadPosition - (toploadScale.x * perpendicular * 2), toploadPosition + (toploadScale.x * perpendicular * 2));

            yield return new WaitForSeconds(window1 + Random.Range(0, window1 * 5));
        }
    }

    IEnumerator ShockFlash()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockActive)
        {
            flash = true;
            topload.color = Color.white;
            yield return new WaitForSeconds(window2 + Random.Range(0f, window2));
            flash = false;
            topload.color = new Color(toploadColorVector.x, toploadColorVector.y, toploadColorVector.z);
            yield return new WaitForSeconds(window1 + Random.Range(0f, window1));
        }
    }

    IEnumerator ReleaseFlash()
    {
        float window2 = 0.2f;
        topload.color = Color.white;
        flash = true;
        yield return new WaitForSeconds(window2);
        flash = false;
        toploadColorVector = new Vector3(1, 1, 1);
    }
}
