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

    int i;
    [HideInInspector] public int currentIndex = 0;

    bool aggro = false;
    [HideInInspector] public int targetIndex = 0;
    int aggroIndex;
    float aggroMult = 0.6f;

    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();
        aura = transform.GetChild(0);
        sprite = GetComponent<SpriteRenderer>();
        int listSize = Mathf.CeilToInt(secondsBehind * 50);
        aggroIndex = Mathf.FloorToInt(listSize * (1 - aggroMult));

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

        targetIndex = 0;

        Color newColor = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
        sprite.color = newColor;
        willDamageBox = false;
        StartCoroutine(InitialDelay());
        StartCoroutine(EnergySpawn());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (boxRB == null || boxRB.GetComponent<Collider2D>().enabled == false)
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
            damage *= sourceEM.aggroIncreaseMult;
            if (willDamageBox == false)
            {
                secondsBehind *= aggroMult;
                targetIndex = aggroIndex;
            }

        }
        else if (sourceEM.aggroCurrentlyActive == false && aggro)
        {
            aggro = false;
            initialYScale /= 1.5f;
            transform.localScale = new Vector2(aura.localScale.x / 1.5f, aura.localScale.y);
            sprite.color = new Color(0, 0, 0, sprite.color.a);
            aura.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, aura.GetComponent<SpriteRenderer>().color.a);
            aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, aura.transform.GetChild(0).GetComponent<SpriteRenderer>().color.a);
            damage /= sourceEM.aggroIncreaseMult;
            if (willDamageBox == false)
            {
                secondsBehind /= aggroMult;
                targetIndex = 0;
            }
        }

        if (aggro && willDamageBox && targetIndex < aggroIndex && i % 3 == 0)
        {
            targetIndex += 1;
        }
        if (aggro == false && willDamageBox && targetIndex > 0)
        {
            targetIndex -= 1;
        }

        if (damagedBox == false)
        {
            boxPositionArray.Add(boxRB.position);
            boxPositionArray.RemoveAt(0);
            
            if (aggro == false)
            {
                transform.position = boxPositionArray[targetIndex] + Vector2.up * 0.1f;
            }
            else
            {
                transform.position = boxPositionArray[targetIndex] + Vector2.up * 0.3f;
            }

            boxRotationArray.Add(boxRB.rotation);
            boxRotationArray.RemoveAt(0);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, boxRotationArray[targetIndex]);

            boxYScaleArray.Add(boxTransform.localScale.y);
            boxYScaleArray.RemoveAt(0);
            float newScale = initialYScale * (boxYScaleArray[targetIndex] / boxInitialYScale);
            transform.localScale = new Vector2(transform.localScale.x, newScale);
            if (boxYScaleArray[targetIndex] != boxInitialYScale)
            {
                aura.localScale = new Vector2(aura.localScale.x, aura1InitialScale + ((boxYScaleArray[targetIndex] / boxInitialYScale) * 0.2222222f));
                aura.GetChild(0).localScale = new Vector2(aura.localScale.x, aura2InitialScale + ((boxYScaleArray[targetIndex] / boxInitialYScale) * 0.1666666f));
            }
            else
            {
                aura.localScale = new Vector2(aura.localScale.x, aura1InitialScale);
                aura.GetChild(0).localScale = new Vector2(aura.localScale.x, aura2InitialScale);
            }
        }


        if (currentIndex % 8 == 0 && willDamageBox)
        {
            int trailIndex = 40;
            newEnergy = Instantiate(energy);
            newEnergy.transform.position = boxPositionArray[targetIndex + trailIndex];
            newEnergy.transform.eulerAngles = new Vector3(newEnergy.transform.eulerAngles.x, newEnergy.transform.eulerAngles.y, boxRotationArray[targetIndex + trailIndex]);
            newEnergy.GetComponent<DuplicateEnergy>().trail = true;
            newEnergy.GetComponent<DuplicateEnergy>().trailIndex = trailIndex + currentIndex;
            newEnergy.GetComponent<DuplicateEnergy>().parent = GetComponent<Rigidbody2D>();
        }

        i++;
        currentIndex = i + targetIndex;
    }

    void CreateEnergy()
    {
        newEnergy = Instantiate(energy);
        newEnergy.transform.position = transform.position;
        newEnergy.GetComponent<DuplicateEnergy>().parent = GetComponent<Rigidbody2D>();
        if (aggro == false)
        {
            newEnergy.GetComponent<DuplicateEnergy>().startPosition = Random.insideUnitCircle.normalized * Random.Range(1f, 3f);
        }
        else
        {
            newEnergy.GetComponent<DuplicateEnergy>().startPosition = Random.insideUnitCircle.normalized * Random.Range(1.5f, 4f);
            newEnergy.transform.localScale *= 1.2f;
        }
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
            window = secondsBehind;
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
        float mult = 0.3f;
        float window = secondsBehind * mult;
        float timer = 0;
        while (timer < window)
        {
            float window2 = 0.03f;
            float timer2 = 0;
            while (timer2 < window2)
            {
                window = secondsBehind * mult;
                timer += Time.deltaTime;
                timer2 += Time.deltaTime;
                yield return null;
            }
            CreateEnergy();
        }
        yield return new WaitForSeconds(secondsBehind * (1 - mult));
        StartCoroutine(EnergyRelease());
    }

    IEnumerator EnergyRelease()
    {
        while (true)
        {
            float window = 0.2f;
            if (aggro)
            {
                window = 0.16f;
            }
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
