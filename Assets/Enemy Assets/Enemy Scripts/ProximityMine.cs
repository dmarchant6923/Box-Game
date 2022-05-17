using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProximityMine : MonoBehaviour
{
    bool lightActive = false;
    GameObject mineLight;
    float lightRadius = 1.5f;
    float initialA;
    Color initialColor;

    Rigidbody2D boxRB;

    public GameObject explosion;
    GameObject newExplosion;
    public float explosionRadius = 3.5f;
    float explosionDamage = 30;
    bool mineWasPulsed = false;

    public bool remoteExplode = false;

    bool detonateActive = false;
    float detonateDelay = 0.4f;

    Rigidbody2D rb;
    bool objectFound = false;
    Transform attachedObject;
    Vector3 relativePosition;

    int groundLM;
    int obstacleLM;
    int boxLM;
    int obstacleAndBoxLM;

    void Start()
    {
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
        obstacleLM = LayerMask.GetMask("Obstacles");
        boxLM = LayerMask.GetMask("Box");
        obstacleAndBoxLM = LayerMask.GetMask("Obstacles", "Box");

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody2D>();

        checkWalls();


        mineLight = transform.GetChild(1).gameObject;
        initialColor = mineLight.GetComponent<Renderer>().material.color;
        initialA = initialColor.a;
        mineLight.transform.localScale = new Vector2(lightRadius * 2 / transform.lossyScale.x, lightRadius * 2 / transform.lossyScale.y);
        StartCoroutine(Dormant());
    }

    void checkWalls()
    {
        RaycastHit2D groundRC = Physics2D.Raycast(transform.position, Vector2.down, 3, groundLM);
        RaycastHit2D wallLeftRC = Physics2D.Raycast(transform.position, Vector2.left, 1, obstacleLM);
        RaycastHit2D wallRightRC = Physics2D.Raycast(transform.position, Vector2.right, 1, obstacleLM);
        RaycastHit2D ceilingRC = Physics2D.Raycast(transform.position, Vector2.up, 1, obstacleLM);
        if (groundRC.collider != null)
        {
            transform.position = groundRC.point + Vector2.up * transform.lossyScale.y / 2;
            attachedObject = groundRC.transform;
            relativePosition = transform.position - attachedObject.position;
        }

        else if (wallRightRC.collider != null)
        {
            transform.position = wallRightRC.point + Vector2.left * transform.lossyScale.y / 2;
            transform.eulerAngles = Vector3.forward * (90);
            attachedObject = wallRightRC.transform;
            relativePosition = transform.position - attachedObject.position;
        }
        else if (ceilingRC.collider != null)
        {
            transform.position = ceilingRC.point + Vector2.down * transform.lossyScale.y / 2;
            transform.eulerAngles = Vector3.forward * (180);
            attachedObject = ceilingRC.transform;
            relativePosition = transform.position - attachedObject.position;
        }
        else if (wallLeftRC.collider != null)
        {
            transform.position = wallLeftRC.point + Vector2.right * transform.lossyScale.y / 2;
            transform.eulerAngles = Vector3.forward * (-90);
            attachedObject = wallLeftRC.transform;
            relativePosition = transform.position - attachedObject.position;
        }

        if (attachedObject == null)
        {
            objectFound = false;
            rb.isKinematic = false;
        }
        else
        {
            objectFound = true;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        Vector2 distanceVector = new Vector2(boxRB.position.x - transform.position.x, boxRB.position.y - transform.position.y);
        RaycastHit2D obstacleRC = Physics2D.Raycast(transform.position, distanceVector.normalized, lightRadius, obstacleAndBoxLM);
        RaycastHit2D boxRC = Physics2D.CircleCast(transform.position, lightRadius, Vector2.zero, 0, boxLM);
        if (boxRC.collider != null && obstacleRC.collider != null && 1 << obstacleRC.collider.gameObject.layer == boxLM && detonateActive == false && lightActive)
        {
            StartCoroutine(Detonate());
        }

        transform.GetChild(0).GetComponent<Renderer>().material.color = transform.GetComponent<Renderer>().material.color;

        if (Box.pulseActive && distanceVector.magnitude <= Box.pulseRadius && lightActive)
        {
            RaycastHit2D rayToBox = Physics2D.Raycast(transform.position, distanceVector.normalized, Box.pulseRadius, obstacleLM);
            if (rayToBox.collider == null)
            {
                mineWasPulsed = true;
                StartCoroutine(DelayedExplode());
            }
        }

        if (remoteExplode)
        {
            StartCoroutine(DelayedExplode());
        }

        if (objectFound)
        {
            transform.position = attachedObject.position + relativePosition;
        }

        if (rb.velocity.y < -20)
        {
            rb.velocity = new Vector2(rb.velocity.x, -20);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (objectFound == false && (1 << collision.gameObject.layer == obstacleLM || 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")))
        {
            checkWalls();
            if (objectFound == false)
            {
                for (int i = 0; i < 17; i++)
                {
                    float angle = i * 22.5f;
                    float distance = 0.6f;
                    Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                        Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized;
                    RaycastHit2D cast = Physics2D.Raycast(rb.position, vector, distance, LayerMask.GetMask("Obstacles", "Platforms", "Enemies"));
                    if (cast.collider != null)
                    {
                        rb.rotation = -Mathf.Atan2(cast.normal.x, cast.normal.y) * Mathf.Rad2Deg;
                        rb.position = cast.point + cast.normal * transform.lossyScale.y / 2;
                        transform.position = rb.position;
                        attachedObject = cast.collider.transform;
                        relativePosition = transform.position - attachedObject.position;
                        objectFound = true;
                        rb.isKinematic = true;
                        break;
                    }

                    Debug.DrawRay(rb.position, vector * distance);
                }
            }
        }
    }

    private void Explode()
    {
        newExplosion = Instantiate(explosion, transform.position, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().explosionRadius = explosionRadius;
        newExplosion.GetComponent<Explosion>().explosionDamage = explosionDamage;
        if (mineWasPulsed)
        {
            newExplosion.GetComponent<Explosion>().damageEnemies = true;
        }
        Destroy(gameObject);
    }

    IEnumerator Dormant()
    {
        Color initialColor = transform.GetComponent<Renderer>().material.color;
        Color dormantColor = new Color(0.7f, 0.5f, 0, 1);
        transform.GetComponent<Renderer>().material.color = dormantColor;
        while (objectFound == false)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        mineLight.SetActive(true);
        transform.GetComponent<Renderer>().material.color = initialColor;
        lightActive = true;
        StartCoroutine(LightFlash());
    }
    IEnumerator LightFlash()
    {
        Color lightColor = mineLight.GetComponent<Renderer>().material.color;
        lightColor.a *= 5;
        if (detonateActive == false)
        {
            mineLight.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
            mineLight.GetComponent<Renderer>().material.color = lightColor;
        }
        float colorDimSpeed = 10;
        while (lightColor.a >= initialA && detonateActive == false)
        {
            lightColor.a -= colorDimSpeed * Time.deltaTime;
            mineLight.GetComponent<Renderer>().material.color = lightColor;
            yield return null;
        }
        if (detonateActive == false)
        {
            lightColor.a = initialA;
            mineLight.GetComponent<Renderer>().material.color = lightColor;
            yield return new WaitForSeconds(1);
            StartCoroutine(LightFlash());
        }
    }
    IEnumerator Detonate()
    {
        detonateActive = true;
        mineLight.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
        mineLight.GetComponent<Renderer>().material.color = initialColor;
        Color lightColor = mineLight.GetComponent<Renderer>().material.color;
        lightColor.a = initialA * 7;
        mineLight.GetComponent<Renderer>().material.color = lightColor;
        float detonateTimer = 0;
        Color color = transform.GetComponent<Renderer>().material.color;
        float initialG = color.g;
        color.g *= 2;
        transform.GetComponent<Renderer>().material.color = color;
        while (detonateTimer <= detonateDelay)
        {
            float colorTimer = 0;
            while (colorTimer <= 0.07f && detonateTimer <= detonateDelay)
            {
                colorTimer += Time.deltaTime;
                if (Box.enemyHitstopActive == false)
                {
                    detonateTimer += Time.deltaTime;
                }
                yield return null;
            }
            lightColor.a /= 2;
            mineLight.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
            mineLight.GetComponent<Renderer>().material.color = lightColor;

            colorTimer = 0;
            color.g /= 2;
            transform.GetComponent<Renderer>().material.color = color;
            while (colorTimer <= 0.07f && detonateTimer <= detonateDelay)
            {
                colorTimer += Time.deltaTime;
                if (Box.enemyHitstopActive == false)
                {
                    detonateTimer += Time.deltaTime;
                }
                yield return null;
            }
            lightColor.a *= 2;
            mineLight.GetComponent<Renderer>().material.color = new Color(0, 0, 0, 0);
            mineLight.GetComponent<Renderer>().material.color = lightColor;

            color.g *= 2;
            transform.GetComponent<Renderer>().material.color = color;
        }
        Explode();
    }
    IEnumerator DelayedExplode()
    {
        yield return new WaitForSeconds(0.02f);
        Explode();
    }
}
