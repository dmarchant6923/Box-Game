using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{
    [HideInInspector] public float sizeX = 1;
    [HideInInspector] public float sizeY = 1;

    Color initialColor;
    [HideInInspector] public bool activateShock = false;
    [HideInInspector] public bool shockActive = false;
    [HideInInspector] public bool endShock = false;
    float shockTime = 4f;
    public GameObject lightning;
    GameObject newLightning;

    Rigidbody2D boxRB;
    Rigidbody2D rb;

    int obstacleLM;


    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();

        sizeX = GetComponent<SpriteRenderer>().size.x;
        sizeY = GetComponent<SpriteRenderer>().size.y;
        GetComponent<BoxCollider2D>().size = new Vector2(sizeX, sizeY);

        initialColor = GetComponent<SpriteRenderer>().color;

        obstacleLM = LayerMask.GetMask("Obstacles");
    }

    private void Update()
    {
        if (activateShock)
        {
            if (shockActive == false)
            {
                shockActive = true;
                StartCoroutine(Shock());
            }
            activateShock = false;
        }
        if (endShock && shockActive)
        {
            StartCoroutine(EndShock());
            endShock = false;
        }
        if (Box.pulseActive && shockActive)
        {
            RaycastHit2D[] obstacles = Physics2D.CircleCastAll(boxRB.position, Box.pulseRadius, Vector2.zero, 0, obstacleLM);
            if (obstacles.Length != 0)
            {
                foreach (RaycastHit2D obstacle in obstacles)
                {
                    if (obstacle.collider.gameObject == gameObject)
                    {
                        shockActive = false;
                    }
                }
            }
        }
    }

    IEnumerator Shock()
    {
        float window1 = shockTime;
        float window2 = 0.4f;
        float timer1 = 0;
        float timer2 = 0;
        Vector2 direction = Vector2.right;
        float distance = sizeX * transform.localScale.x / 2;
        if (sizeY > sizeX)
        {
            direction = Vector2.up;
            distance = sizeY * transform.localScale.y / 2;
        }
        bool forward = true;
        StartCoroutine(ShockFlash());
        while (timer1 < window1 && shockActive)
        {
            if (timer2 > window2)
            {
                Vector2 pointA;
                Vector2 pointB;
                if (forward)
                {
                    //pointA = new Vector2(rb.position.x - transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    //pointB = new Vector2(rb.position.x + transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    pointA = rb.position - (distance) * direction + Random.Range(-0.2f, 0.2f) * Vector2.Perpendicular(direction);
                    pointB = rb.position + (distance) * direction + Random.Range(-0.2f, 0.2f) * Vector2.Perpendicular(direction);
                    forward = false;
                }
                else
                {
                    //pointA = new Vector2(rb.position.x + transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    //pointB = new Vector2(rb.position.x - transform.lossyScale.x / 2, rb.position.y + Random.Range(-0.2f, 0.2f));
                    pointA = rb.position + (distance) * direction + Random.Range(-0.2f, 0.2f) * Vector2.Perpendicular(direction);
                    pointB = rb.position - (distance) * direction + Random.Range(-0.2f, 0.2f) * Vector2.Perpendicular(direction);
                    forward = true;
                }
                newLightning = Instantiate(lightning);
                newLightning.GetComponent<Lightning>().pointA = pointA;
                newLightning.GetComponent<Lightning>().pointB = pointB;
                newLightning.GetComponent<Lightning>().pointsPerUnit = 2 - Mathf.Min(transform.localScale.x / 15, 1);
                newLightning.GetComponent<Lightning>().aestheticElectricity = true;
                timer2 = 0;
                window2 = 0.4f + Random.Range(-0.3f, 0.3f);
            }
            timer1 += Time.deltaTime;
            timer2 += Time.deltaTime;
            yield return null;
        }
        shockActive = false;
    }

    IEnumerator ShockFlash()
    {
        float window1 = 0.15f;
        float window2 = 0.05f;
        while (shockActive)
        {
            GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(window2 + Random.Range(0, window2 * 1.5f));
            GetComponent<SpriteRenderer>().color = initialColor;
            yield return new WaitForSeconds(window1 + Random.Range(0, window1 * 1.5f));
        }
        GetComponent<SpriteRenderer>().color = initialColor;
    }
    IEnumerator EndShock()
    {
        yield return null;
        float window = Lightning.contactDamage * Box.boxHitstopDelayMult * 2f;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        shockActive = false;
    }
}
