using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    Transform fireTop;
    float initialYScale;
    float flickerTime = 0.1f;

    public GameObject smoke;
    float smokeTime = 0.5f;

    public Rigidbody2D objectOnFire;
    bool onObject = false;
    float offset = 0.5f;

    public bool hazardFire = false;
    [System.NonSerialized] public float fireWindow = 3f; //3
    public float fireTime = 0;
    public Vector2 surfaceNormal = Vector2.up;
    public GameObject fire;

    static int firesTouchingBox = 0;

    public bool stopFire;

    void Start()
    {
        fireTop = transform.GetChild(0);
        initialYScale = fireTop.localScale.y;
        StartCoroutine(FireFlicker());
        StartCoroutine(Smoke());

        if (objectOnFire != null)
        {
            if (objectOnFire.transform.root.GetComponent<EnemyBehavior_GroundedVehicle>() != null)
            {
                offset = 0.9f;
            }
            else if (objectOnFire.transform.root.GetComponent<EnemyBehavior_Wizard>() != null ||
                objectOnFire.transform.root.GetComponent<EnemyBehavior_Blitz>() != null)
            {
                offset = 0.2f;
            }
            else if (objectOnFire.transform.root.GetComponent<EnemyBehavior_Turret>() != null)
            {
                offset = 0.7f;
            }
            onObject = true;

            if (objectOnFire.GetComponent<Box>() != null)
            {
                transform.localScale = new Vector2(transform.localScale.x * 0.7f, transform.localScale.y);
            }
        }

        if (hazardFire)
        {
            StartCoroutine(FireDespawn());
            surfaceNormal = surfaceNormal.normalized;
            if (Mathf.Abs(surfaceNormal.y) < 0.3f)
            {
                surfaceNormal = new Vector2(Mathf.Sign(surfaceNormal.x), 1).normalized;
            }
            float angle = -Mathf.Atan2(surfaceNormal.x, surfaceNormal.y) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }
    }


    void Update()
    {
        if (stopFire)
        {
            Destroy(gameObject);
        }

        if (onObject == true)
        {
            if (objectOnFire == null)
            {
                stopFire = true;
            }
            if (objectOnFire.transform.root.GetComponent<EnemyManager>() != null && objectOnFire.transform.root.GetComponent<EnemyManager>().enemyWasKilled)
            {
                stopFire = true;
            }
            else
            {
                transform.position = objectOnFire.position + Vector2.up * offset;
            }
        }

        if (firesTouchingBox > 0)
        {
            Box.onFire = true;
        }
        else
        {
            Box.onFire = false;
        }
    }

    private void OnTriggerEnter2D (Collider2D collision)
    {
        if (hazardFire && collision.GetComponent<Box>() != null)
        {
            firesTouchingBox++;
            //Box.onFire = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (hazardFire && collision.GetComponent<Box>() != null)
        {
            firesTouchingBox--;
            //Box.onFire = false;
        }
    }

    IEnumerator FireFlicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(flickerTime * 0.8f + Random.Range(0f, flickerTime * 0.4f));
            fireTop.localScale = new Vector2(fireTop.localScale.x, initialYScale * Random.Range(0.45f, 0.7f));
            yield return new WaitForSeconds(flickerTime * 0.8f + Random.Range(0f, flickerTime * 0.4f));
            fireTop.localScale = new Vector2(fireTop.localScale.x, initialYScale * Random.Range(0.9f, 1.2f));
        }
    }

    IEnumerator Smoke()
    {
        while (true)
        {
            yield return new WaitForSeconds(smokeTime * 0.7f + Random.Range(0f, smokeTime * 0.6f));
            float angle = transform.eulerAngles.z;
            Vector3 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2), Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
            Instantiate(smoke, transform.position + vector * 0.5f, Quaternion.identity);
        }
    }

    IEnumerator FireDespawn()
    {
        while (fireTime < fireWindow)
        {
            fireTime += Time.deltaTime;
            yield return null;
        }
        stopFire = true;
    }
}
