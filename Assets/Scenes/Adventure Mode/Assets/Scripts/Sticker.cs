using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sticker : MonoBehaviour
{
    Transform childTransform;
    EpisodeManager episodeManager;

    public bool isDuplicate = false;
    public bool found = false;
    public bool timeFound = false;

    float spinPeriod = 5f;

    float tiltPeriod = 1.5f;
    float tiltMax = 7;
    float tiltTimer = 0;

    void Start()
    {
        childTransform = transform.GetChild(0);
        episodeManager = FindObjectOfType<EpisodeManager>();

        tiltTimer = Random.Range(-tiltPeriod / 2, tiltPeriod / 2);
        tiltPeriod += Random.Range(-tiltPeriod / 10, tiltPeriod / 10);
        spinPeriod += Random.Range(-spinPeriod / 10, spinPeriod / 10);

        if (found)
        {
            StartCoroutine(FoundAnimation());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float tilt = tiltMax * Mathf.Sin(tiltTimer * 2 * Mathf.PI / tiltPeriod) / tiltPeriod;
        if (tiltTimer >= tiltPeriod)
        {
            tilt = 0;
            tiltTimer -= tiltPeriod;
        }
        tiltTimer += Time.deltaTime;

        float adjustedSpinPeriod = spinPeriod * (0.2f + Mathf.Abs(childTransform.eulerAngles.y % 180 - 90) / 120);
        childTransform.eulerAngles = new Vector3(childTransform.eulerAngles.x, 
            Mathf.MoveTowards(childTransform.eulerAngles.y, 360, 360 / adjustedSpinPeriod * Time.deltaTime), tilt);
        if (childTransform.eulerAngles.y == 360)
        {
            childTransform.eulerAngles = Vector3.zero + Vector3.forward * tilt;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            if (isDuplicate == false)
            {
                episodeManager.StickerFound(gameObject);
                StartCoroutine(FoundAnimation());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator FoundAnimation()
    {
        spinPeriod /= 10;
        tiltMax = 0;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<MovingObjects>().enabled = false;
        foreach (Renderer item in GetComponentsInChildren<Renderer>())
        {
            item.sortingLayerName = "Above All";
        }
        Rigidbody2D boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        Vector2 initialPosition = boxRB.position;
        float window = 1f;
        float timer = 0;
        while (timer < window)
        {
            if (timeFound)
            {
                transform.position = initialPosition + Vector2.up * (1f + timer / 1.5f);
            }
            else
            {
                transform.position = boxRB.position + Vector2.up * (1f + timer / 1.5f);
            }
            transform.localScale -= Vector3.one * 0.25f * Time.deltaTime;
            foreach (SpriteRenderer item in GetComponentsInChildren<SpriteRenderer>())
            {
                Color newColor = item.color;
                newColor.a -= Time.deltaTime;
                item.color = newColor;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
