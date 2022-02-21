using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;
    GameObject[] items = new GameObject[3];

    public bool releaseOnExit = false;
    public bool pressToRelease = false;


    public bool boxCanActivate = true;
    public bool enemiesCanActivate = false;

    bool delayCRActive = false;
    bool delay = false;

    bool active = false;

    Transform button;
    Color initialColor;
    float initialYScale;
    float initialYPosition;

    void Start()
    {
        button = transform.GetChild(0);
        initialColor = button.GetComponent<SpriteRenderer>().color;
        initialYScale = button.localScale.y;
        initialYPosition = button.localPosition.y;
        items[0] = item1;
        items[1] = item2;
        items[2] = item3;
    }
    void Activate()
    {
        active = true;
        button.localScale = new Vector2(button.localScale.x, initialYScale * 0.4f);
        button.localPosition = new Vector2(button.localPosition.x, initialYPosition - 0.1f);
        button.GetComponent<SpriteRenderer>().color = Color.green;
        foreach (GameObject item in items)
        {
            if (item != null && item.GetComponent<Door>() != null)
            {
                item.GetComponent<Door>().Trigger();
            }
        }
    }

    void Deactivate()
    {
        active = false;
        button.localScale = new Vector2(button.localScale.x, initialYScale);
        button.localPosition = new Vector2(button.localPosition.x, initialYPosition);
        button.GetComponent<SpriteRenderer>().color = initialColor;
        foreach (GameObject item in items)
        {
            if (item != null && item.GetComponent<Door>() != null)
            {
                item.GetComponent<Door>().Trigger();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((boxCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Box")) || 
            (enemiesCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Enemies")))
        {
            if (active == false)
            {
                Activate();
            }
            else if (active && pressToRelease)
            {
                Deactivate();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((boxCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Box")) ||
            (enemiesCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Enemies")))
        {
            if (releaseOnExit)
            {
                Deactivate();
            }
        }
    }
}
