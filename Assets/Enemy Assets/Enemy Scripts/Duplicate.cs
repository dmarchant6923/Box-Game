using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour
{
    Rigidbody2D boxRB;
    Transform boxTransform;
    Transform aura;
    SpriteRenderer sprite;
    public EnemyManager sourceEM;
    public GameObject energy;
    GameObject newEnergy;
    public float secondsBehind = 6f;
    List<Vector2> boxPositionArray = new List<Vector2>();
    List<float> boxRotationArray = new List<float>();
    List<float> boxYScaleArray = new List<float>();
    float initialYScale;
    float boxInitialYScale;

    float aura1InitialScale;
    float aura2InitialScale;

    public float damage = 25;
    bool damagedBox = false;
    bool willDamageBox = false;

    bool aggro = false;

    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();
        aura = transform.GetChild(0);
        sprite = GetComponent<SpriteRenderer>();
        int listSize = Mathf.CeilToInt(secondsBehind * 50);

        boxInitialYScale = 0.5f;
        initialYScale = transform.localScale.y;

        aura1InitialScale = aura.transform.localScale.y;
        aura2InitialScale = aura.GetChild(0).localScale.y;

        for (int i = 0; i < listSize; i++)
        {
            boxPositionArray.Add(boxRB.position);
            boxRotationArray.Add(boxRB.rotation);
            boxYScaleArray.Add(boxTransform.localScale.y);
        }

        Color newColor = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
        sprite.color = newColor;
        willDamageBox = false;
        StartCoroutine(InitialDelay());
        StartCoroutine(EnergySpawn());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (boxRB == null)
        {
            death();
        }

        if (sourceEM.aggroCurrentlyActive && aggro == false)
        {
            aggro = true;
            initialYScale *= 1.5f;
            transform.localScale = new Vector2(transform.localScale.x * 1.5f, transform.localScale.y);
            sprite.color = new Color(0.2f, 0, 0, sprite.color.a);
            aura.GetComponent<SpriteRenderer>().color = new Color(1, 0.8f, 0.8f, aura.GetComponent<SpriteRenderer>().color.a);
            aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 0.8f, 0.8f, aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a);
            Debug.Log(sprite.color);

        }
        else if (sourceEM.aggroCurrentlyActive == false && aggro)
        {
            aggro = false;
            initialYScale /= 1.5f;
            transform.localScale = new Vector2(aura.localScale.x / 1.5f, aura.localScale.y);
            sprite.color = new Color(0, 0, 0, sprite.color.a);
            aura.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, aura.GetComponent<SpriteRenderer>().color.a);
            aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a);
        }

        if (damagedBox == false)
        {
            boxPositionArray.Add(boxRB.position);
            boxPositionArray.RemoveAt(0);
            transform.position = boxPositionArray[0];

            boxRotationArray.Add(boxRB.rotation);
            boxRotationArray.RemoveAt(0);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, boxRotationArray[0]);

            boxYScaleArray.Add(boxTransform.localScale.y);
            boxYScaleArray.RemoveAt(0);
            float newScale = initialYScale * (boxYScaleArray[0] / boxInitialYScale);
            transform.localScale = new Vector2(transform.localScale.x, newScale);
            if (boxYScaleArray[0] != boxInitialYScale)
            {
                aura.localScale = new Vector2(aura.localScale.x, aura1InitialScale + ((boxYScaleArray[0] / boxInitialYScale) * 0.2222222f));
                aura.GetChild(0).localScale = new Vector2(aura.localScale.x, aura2InitialScale + ((boxYScaleArray[0] / boxInitialYScale) * 0.1666666f));
            }
            else
            {
                aura.localScale = new Vector2(aura.localScale.x, aura1InitialScale);
                aura.GetChild(0).localScale = new Vector2(aura.localScale.x, aura2InitialScale);
            }
        }
    }

    void CreateEnergy()
    {
        newEnergy = Instantiate(energy);
        newEnergy.transform.position = transform.position;
        newEnergy.GetComponent<DuplicateEnergy>().parent = GetComponent<Rigidbody2D>();
        newEnergy.GetComponent<DuplicateEnergy>().startPosition = Random.insideUnitCircle.normalized * Random.Range(1f, 3f);
        newEnergy.GetComponent<DuplicateEnergy>().inwards = true;
    }

    public void death()
    {
        for (int i = 0; i < 14; i++)
        {
            newEnergy = Instantiate(energy);
            newEnergy.transform.position = transform.position;
            newEnergy.transform.localScale *= 1.2f;
            newEnergy.GetComponent<DuplicateEnergy>().startPosition = Vector2.zero;
            newEnergy.GetComponent<DuplicateEnergy>().maxDist = 4;
            newEnergy.GetComponent<DuplicateEnergy>().inwards = false;
            newEnergy.GetComponent<DuplicateEnergy>().slow = true;
        }
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box") && Box.isInvulnerable == false && willDamageBox)
        {
            StartCoroutine(DamageBox());
            Box.damageTaken = damage;
            Box.boxDamageDirection = Box.boxDamageDirection = new Vector2(Mathf.Sign(boxRB.position.x - transform.position.x), 1).normalized;
            Box.activateDamage = true;
        }
    }

    IEnumerator InitialDelay()
    {
        float window = secondsBehind;
        float timer = 0;
        float changeRate = 1 / secondsBehind;
        Color color = sprite.color;
        while (timer < window)
        {
            color = sprite.color;
            color.a += changeRate * Time.deltaTime;
            sprite.color = color;
            timer += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        sprite.color = color;
        willDamageBox = true;
    }

    IEnumerator EnergySpawn()
    {
        float window = secondsBehind * 0.8f;
        float timer = 0;
        while (timer < window)
        {
            float window2 = 0.06f;
            float timer2 = 0;
            while (timer2 < window2)
            {
                timer += Time.deltaTime;
                timer2 += Time.deltaTime;
                yield return null;
            }
            CreateEnergy();
        }
        StartCoroutine(EnergyRelease());
    }

    IEnumerator EnergyRelease()
    {
        while (true)
        {
            float window = 0.2f;
            float timer = 0;
            while (timer < window)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            CreateEnergy();
        }
    }

    IEnumerator DamageBox()
    {
        damagedBox = true;
        yield return null;
        while (Box.boxHitstopActive)
        {
            yield return null;
        }
        damagedBox = false;
    }

    public IEnumerator DamageFlicker()
    {
        Debug.Log("you are here");
        yield return null;
        if (sourceEM.enemyWasKilled == false)
        {
            while (sourceEM.enemyIsInvulnerable == true)
            {
                GetComponent<SpriteRenderer>().enabled = true;
                aura.GetComponent<SpriteRenderer>().enabled = true;
                aura.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;

                if (sourceEM.enemyIsInvulnerable == true)
                {
                    yield return new WaitForSeconds(0.12f);
                }
                GetComponent<SpriteRenderer>().enabled = false;
                aura.GetComponent<SpriteRenderer>().enabled = false;
                aura.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

                if (sourceEM.enemyIsInvulnerable == true)
                {
                    yield return new WaitForSeconds(0.04f);
                }
            }
            GetComponent<SpriteRenderer>().enabled = true;
            aura.GetComponent<SpriteRenderer>().enabled = true;
            aura.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
