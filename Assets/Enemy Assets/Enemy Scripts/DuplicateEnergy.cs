using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicateEnergy : MonoBehaviour
{
    public bool inwards = true;
    public bool trail = false;
    public int trailIndex = 0;
    int parentIndex = 0;
    int initialIndex = 0;
    float velocity;
    float initialVelocity;
    SpriteRenderer sprite;
    Color color;

    public Rigidbody2D parent;
    public Duplicate parentScript;
    public Vector2 startPosition;
    public float maxDist = 2;
    Transform energy;
    Vector2 destination;
    Vector2 randomVector;

    public bool slow = false;

    void Start()
    {
        energy = transform.GetChild(0);
        sprite = energy.GetComponent<SpriteRenderer>();
        color = sprite.color;
        energy.transform.localPosition = startPosition;

        destination = Random.insideUnitCircle.normalized * maxDist * Random.Range(0.3f, 1);
        if (inwards)
        {
            initialVelocity = startPosition.magnitude / 0.5f;
        }
        else
        {
            initialVelocity = destination.magnitude / 0.5f;
        }
        if (slow)
        {
            initialVelocity *= 0.4f;
        }
        velocity = initialVelocity;

        if (trail)
        {
            initialVelocity = 0;
            startPosition = Vector2.zero;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0);
            parentScript = parent.GetComponent<Duplicate>();
            parentIndex = parentScript.currentIndex;
            initialIndex = trailIndex - parentIndex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (trail == false)
        {
            if (parent != null)
            {
                transform.position = parent.position;
            }

            if (inwards)
            {
                energy.transform.localPosition = Vector2.MoveTowards(energy.transform.localPosition, Vector2.zero, velocity * Time.deltaTime);
            }
            else
            {

                energy.transform.localPosition = Vector2.MoveTowards(energy.transform.localPosition, destination, velocity * Time.deltaTime);
            }

            color.a -= Time.deltaTime * 0.25f;
            if (slow == false)
            {
                color.a -= Time.deltaTime * 0.45f;
            }
            sprite.color = color;

            if (color.a <= 0.005f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (trail)
        {
            parentIndex = parentScript.currentIndex;
            float difference = trailIndex - parentIndex;
            float percentDifference = (difference / initialIndex);

            Color color = sprite.color;
            color.a = 0.5f * (1 - percentDifference);
            sprite.color = color;

            if (difference <= 0 || parent == null)
            {
                Destroy(gameObject);
            }
        }
    }
}
