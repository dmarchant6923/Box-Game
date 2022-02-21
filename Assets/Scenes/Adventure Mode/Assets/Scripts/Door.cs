using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool areaTrigger = false;
    public bool startOpen = false;
    public bool stayOpen = false;
    public bool moveDoor = false;

    Transform door;

    [HideInInspector] public bool open = false;

    public float changeSpeed = 15f;
    float initialPosition;
    float initialScale;

    void Start()
    {
        door = transform.GetChild(0);

        open = false;
        if (startOpen)
        {
            open = true;
        }
        initialPosition = door.localPosition.y;
        initialScale = door.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        float targetPosition = initialPosition;
        float targetScale = initialScale;
        if (open)
        {
            targetPosition = initialPosition - initialScale + 1;
            if (moveDoor == false)
            {
                targetPosition = 0.5f;
            }
            targetScale = 1;
        }

        if (moveDoor)
        {
            door.localPosition = new Vector2(door.localPosition.x, Mathf.MoveTowards(door.localPosition.y, targetPosition, changeSpeed * Time.deltaTime));
        }
        else
        {
            door.localScale = new Vector2(door.localScale.x, Mathf.MoveTowards(door.localScale.y, targetScale, changeSpeed * Time.deltaTime));
            door.localPosition = new Vector2(door.localPosition.x, Mathf.MoveTowards(door.localPosition.y, targetPosition, changeSpeed * Time.deltaTime / 2));
        }

    }

    public void Trigger()
    {
        if ((stayOpen && open == false) || stayOpen == false)
        {
            open = !open;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box") && areaTrigger == true)
        {
            Trigger();
            Debug.Log("you are here");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box") && areaTrigger == true)
        {
            Trigger();
        }
    }
}
