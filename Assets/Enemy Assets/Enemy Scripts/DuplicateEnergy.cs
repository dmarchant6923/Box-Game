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
    public EnemyManager wizardEM;
    public Vector2 startPosition;
    public float maxDist = 2;
    Transform energy;
    Vector2 destination;

    float activeDeltaTime = 0;

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
        
        if (parent != null && parent.GetComponent<EnemyManager>())
        {
            wizardEM = parent.GetComponent<EnemyManager>();
        }
        else if (parent != null && parent.GetComponent<Duplicate>() != null)
        {
            wizardEM = parent.GetComponent<Duplicate>().sourceEM;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (parent != null && wizardEM.enemyIsFrozen)
        {
            activeDeltaTime = 0;
        }
        else
        {
            activeDeltaTime = Time.deltaTime;
        }

        if (trail == false)
        {
            if (parent != null)
            {
                transform.position = parent.position;
            }

            if (inwards)
            {
                energy.transform.localPosition = Vector2.MoveTowards(energy.transform.localPosition, Vector2.zero, velocity * activeDeltaTime);
            }
            else
            {

                energy.transform.localPosition = Vector2.MoveTowards(energy.transform.localPosition, destination, velocity * activeDeltaTime);
            }

            color.a -= activeDeltaTime * 0.25f;
            if (slow == false)
            {
                color.a -= activeDeltaTime * 0.45f;
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

            if (difference <= 0 || parent == null || wizardEM.enemyIsFrozen)
            {
                Destroy(gameObject);
            }
        }
    }
}
