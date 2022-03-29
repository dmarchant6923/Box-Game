using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public bool send = true;
    public bool recieve = true;
    public GameObject destination;

    public bool active = true;
    public bool stayActive = false;
    public bool startActive = true;

    bool cooldown = false;
    bool currentlyTeleporting = false;

    GameObject box;

    GameObject body;


    void Start()
    {
        cooldown = false;
        box = GameObject.Find("Box");
        body = transform.GetChild(0).gameObject;
        if (startActive == false)
        {
            body.SetActive(false);
            active = false;
        }
    }

    void Update()
    {
        
    }

    public void Trigger()
    {
        if (active && stayActive == false)
        {
            active = false;
            body.SetActive(false);
        }
        else if (active == false)
        {
            active = true;
            body.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Box>() != null && send && destination.GetComponent<Teleport>().recieve &&
            active && destination.GetComponent<Teleport>().active && 
            Box.boxHitstopActive == false && cooldown == false)
        {
            StartCoroutine(BeginTeleport());
            currentlyTeleporting = true;
        }
    }
    
    IEnumerator BeginTeleport()
    {
        float window = 0.7f;
        float timer = 0;
        box.GetComponent<BoxCollider2D>().enabled = false;
        box.GetComponent<SpriteRenderer>().enabled = false;
        StartCoroutine(box.GetComponent<Box>().DisableInputs(window));
        Vector2 position = box.transform.position;
        Vector2 velocity = box.GetComponent<Rigidbody2D>().velocity;
        float angularVelocity = box.GetComponent<Rigidbody2D>().angularVelocity;
        float offset = box.transform.position.y - transform.position.y;
        while (timer < window)
        {
            box.GetComponent<SpriteRenderer>().enabled = false;
            box.GetComponent<BoxCollider2D>().enabled = false;
            box.transform.position = position;
            box.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            box.GetComponent<Rigidbody2D>().angularVelocity = 0;
            timer += Time.deltaTime;
            yield return null;
        }

        box.GetComponent<BoxCollider2D>().enabled = true;
        box.GetComponent<SpriteRenderer>().enabled = true;
        box.transform.position = destination.transform.position + Vector3.up * offset;
        box.GetComponent<Rigidbody2D>().velocity = velocity;
        BoxVelocity.velocitiesX[0] = velocity.x;
        box.GetComponent<Rigidbody2D>().angularVelocity = angularVelocity;

        StartCoroutine(destination.GetComponent<Teleport>().CoolDown());
        yield return null;
        if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
        {
            GameObject.Find("Main Camera").GetComponent<CameraFollowBox>().RefocusBox();
        }
    }

    public IEnumerator CoolDown()
    {
        cooldown = true;
        float window = 0.5f;
        float timer = 0;
        while (timer <= window)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        cooldown = false;
    }
}
