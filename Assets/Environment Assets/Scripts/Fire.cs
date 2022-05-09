using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : MonoBehaviour
{
    Transform fireTop;
    float initialYScale;
    float flickerTime = 0.1f;

    public GameObject smoke;
    float smokeTime = 0.4f;

    public Rigidbody2D objectOnFire;
    bool onObject = false;
    float offset = 0.5f;

    bool mainFire = true;

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
            if (objectOnFire == null || objectOnFire.transform.root.GetComponent<EnemyManager>().enemyWasKilled)
            {
                stopFire = true;
            }
            else
            {
                transform.position = objectOnFire.position + Vector2.up * offset;
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
            yield return new WaitForSeconds(smokeTime * 0.8f + Random.Range(0f, smokeTime * 0.4f));
            Instantiate(smoke, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
    }
}
