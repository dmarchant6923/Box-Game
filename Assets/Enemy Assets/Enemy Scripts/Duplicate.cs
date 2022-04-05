using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour
{
    Rigidbody2D boxRB;
    Transform boxTransform;
    Transform aura;
    SpriteRenderer sprite;
    public GameObject energy;
    GameObject newEnergy;
    [System.NonSerialized] float secondsBehind = 2f;
    List<Vector2> boxPositionArray = new List<Vector2>();
    List<float> boxRotationArray = new List<float>();
    List<float> boxYScaleArray = new List<float>();
    float initialYScale;
    float boxInitialYScale;

    float aura1InitialScale;
    float aura2InitialScale;

    public float damage = 30;
    bool damagedBox = false;
    bool willDamageBox = false;

    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();
        aura = transform.GetChild(0);
        sprite = GetComponent<SpriteRenderer>();
        int listSize = Mathf.CeilToInt(secondsBehind * 50);

        boxInitialYScale = boxTransform.localScale.y;
        initialYScale = transform.localScale.y;

        aura1InitialScale = aura.transform.localScale.y;
        aura2InitialScale = aura.GetChild(0).localScale.y;

        for (int i = 0; i < listSize; i++)
        {
            boxPositionArray.Add(boxRB.position);
            boxRotationArray.Add(boxRB.rotation);
            boxYScaleArray.Add(boxTransform.localScale.y);
        }

        Color newColor = new Color(0, 0, 0, 0);
        sprite.color = newColor;
        willDamageBox = false;
        StartCoroutine(InitialDelay());
        StartCoroutine(EnergySpawn());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
        float window = secondsBehind * 0.6f;
        float timer = 0;
        while (timer < window)
        {
            float window2 = 0.08f;
            float timer2 = 0;
            while (timer2 < window2)
            {
                timer += Time.deltaTime;
                timer2 += Time.deltaTime;
                yield return null;
            }
            newEnergy = Instantiate(energy, transform);
            newEnergy.transform.localPosition = Random.insideUnitCircle.normalized * Random.Range(0.8f, 2f);
            newEnergy.GetComponent<DuplicateEnergy>().inwards = true;
            Debug.Log("you are here");

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
}
