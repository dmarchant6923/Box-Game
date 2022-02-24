using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeslaCoil : MonoBehaviour
{
    public GameObject lightning;
    GameObject newLightning;
    float radius = 7;

    int phase = 1;
    public float timePerPhase = 3;

    Vector2 breakoutPoint;
    SpriteRenderer topload;
    Vector3 toploadColorVector;
    Vector3 initialColor;
    Vector3 activeColor;
    Vector3 fireColor;
    float colorChangeSpeed = 1.6f;

    List<SpriteRenderer> coilObjects;
    List<Color> coilColors = new List<Color> { };
    bool flash = false;

    bool shockActive = true;

    void Start()
    {
        topload = transform.GetChild(1).GetComponent<SpriteRenderer>();
        breakoutPoint = transform.GetChild(0).position;

        initialColor = new Vector3(topload.color.r, topload.color.g, topload.color.b);
        activeColor = new Vector3(0.6f, 0.3f, 0.7f);
        fireColor = new Vector3(1, 0, 0);

        toploadColorVector = initialColor;

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
    }

    IEnumerator PhaseChange()
    {
        float window = timePerPhase;
        float timer = 0;
        if (phase > 3)
        {
            phase = 1;
            colorChangeSpeed *= 2;
        }
        if (phase == 2)
        {
            window *= 1.5f;
            colorChangeSpeed /= 2;
        }
        if (phase == 3)
        {
            window *= 0.5f;
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
            window = 1f;
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
            Debug.Log("you are here");
            StartCoroutine(lightning.GetComponent<Lightning>().LightningStrike(radius, 0, Lightning.thunderDamage, breakoutPoint, true, transform.GetChild(0).gameObject));
            shockActive = false;
        }
    }

    IEnumerator Shock()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockActive)
        {
            flash = true;
            foreach (SpriteRenderer item in coilObjects)
            {
                item.color = Color.white;
            }
            yield return new WaitForSeconds(window2 + Random.Range(0f, window2));
            flash = false;
            for (int i = 0; i < coilObjects.Count; i++)
            {
                coilObjects[i].color = coilColors[i];
            }
            topload.color = new Color(toploadColorVector.x, toploadColorVector.y, toploadColorVector.z);
            yield return new WaitForSeconds(window1 + Random.Range(0f, window1));
        }


        for (int i = 0; i < coilObjects.Count; i++)
        {
            coilObjects[i].color = coilColors[i];
        }
    }
}
