using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuplicateEnergy : MonoBehaviour
{
    public bool inwards = true;
    Vector2 destination;
    float velocity = 3;
    int rotateDirection = 1;
    float rotateVelocity = 400;
    SpriteRenderer sprite;
    Color color;
    void Start()
    {
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Random.Range(0, 90f));
        sprite = GetComponent<SpriteRenderer>();
        color = sprite.color;
        int rand = Random.Range(0, 2);
        if (rand == 1)
        {
            rotateDirection *= -1;
        }
        rotateVelocity += Random.Range(0, 400f);
        rotateVelocity *= rotateDirection;

        destination = Random.insideUnitCircle.normalized * Random.Range(0.8f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (inwards)
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, Vector2.zero, velocity * Time.deltaTime);
        }
        else
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, destination, velocity * Time.deltaTime);
        }
        velocity -= Time.deltaTime;

        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + rotateVelocity * Time.deltaTime);

        color.a -= Time.deltaTime;
        sprite.color = color;

        if (color.a <= 0.05f)
        {
            Destroy(gameObject);
        }
    }
}
