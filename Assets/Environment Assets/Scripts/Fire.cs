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
    public bool boxFire = false;
    public GameObject fire;
    GameObject newFire;

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
                objectOnFire.transform.root.GetComponent<EnemyBehavior_Blitz>() != null ||
                objectOnFire.transform.root.GetComponent<Box>() != null)
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

        if (boxFire && Box.onFire == false && Box.activateFire == false)
        {
            stopFire = true;
        }
    }

    private void OnTriggerStay2D (Collider2D collision)
    {
        if (hazardFire && collision.GetComponent<Box>() != null)
        {
            if (Box.onFire == false)
            {
                Box.activateFire = true;
                newFire = Instantiate(fire, collision.GetComponent<Rigidbody2D>().position, Quaternion.identity);
                newFire.GetComponent<Fire>().objectOnFire = collision.GetComponent<Rigidbody2D>();
                newFire.GetComponent<Fire>().hazardFire = false;
                newFire.GetComponent<Fire>().boxFire = true;
            }
            else
            {
                Box.fireTimer = 0;
            }
        }
    }

    IEnumerator FireFlicker()
    {
        while (true)
        {
            yield return new WaitForSeconds(flickerTime * 0.8f + Random.Range(0f, flickerTime * 0.4f));
            fireTop.localScale = new Vector2(fireTop.localScale.x, initialYScale * 0.65f);
            yield return new WaitForSeconds(flickerTime * 0.8f + Random.Range(0f, flickerTime * 0.4f));
            fireTop.localScale = new Vector2(fireTop.localScale.x, initialYScale);
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
}
