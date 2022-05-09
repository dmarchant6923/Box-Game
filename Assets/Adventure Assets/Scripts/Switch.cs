using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    public GameObject item1;
    public GameObject item2;
    public GameObject item3;
    public GameObject item4;
    GameObject[] items = new GameObject[4];

    public bool releaseOnExit = false;
    float itemsInTrigger = 0;

    public bool pressToRelease = false;
    public bool triggerOnRelease = true;

    public bool hitSwitch = false;
    public bool groundSwitch = false;

    public bool boxCanActivate = true;
    public bool enemiesCanActivate = false;
    public bool platformsCanActivate = false;

    public bool startActive = false;

    public int value = 1;

    [HideInInspector] public bool active = false;

    Transform button;
    Color initialColor;
    float initialYScale;
    float initialYPosition;

    void Start()
    {
        if (hitSwitch == false && groundSwitch == false)
        {
            button = transform.GetChild(0);
            initialColor = button.GetComponent<SpriteRenderer>().color;
            initialYScale = button.localScale.y;
            initialYPosition = button.localPosition.y;
        }
        items[0] = item1;
        items[1] = item2;
        items[2] = item3;
        items[3] = item4;

        if (startActive)
        {
            Activate();
        }
    }
    public void Activate()
    {
        if (hitSwitch == false && groundSwitch == false)
        {
            button.localScale = new Vector2(button.localScale.x, initialYScale * 0.4f);
            button.localPosition = new Vector2(button.localPosition.x, initialYPosition - 0.1f);
            button.GetComponent<SpriteRenderer>().color = Color.green;
        }
        active = true;
        Action(value);
    }

    public void Deactivate()
    {
        if (hitSwitch == false && groundSwitch == false)
        {
            button.localScale = new Vector2(button.localScale.x, initialYScale);
            button.localPosition = new Vector2(button.localPosition.x, initialYPosition);
            button.GetComponent<SpriteRenderer>().color = initialColor;
        }
        active = false;
        Action(-value);
    }

    void softRelease()
    {
        if (hitSwitch == false && groundSwitch == false)
        {
            button.localScale = new Vector2(button.localScale.x, initialYScale);
            button.localPosition = new Vector2(button.localPosition.x, initialYPosition);
            button.GetComponent<SpriteRenderer>().color = initialColor;
        }
        active = false;
    }

    public void Action(int value)
    {
        foreach (GameObject item in items)
        {
            if (item != null && item.GetComponent<Door>() != null)
            {
                item.GetComponent<Door>().Trigger(value);
            }
            else if (item != null && item.GetComponent<MovingObjects>() != null)
            {
                item.GetComponent<MovingObjects>().Trigger();
            }
            if (item != null && item.GetComponent<CommandMove>() != null)
            {
                item.GetComponent<CommandMove>().Trigger();
            }
            else if (item != null && item.GetComponent<BattleSpawner>() != null)
            {
                item.GetComponent<BattleSpawner>().Trigger();
            }
            else if (item != null && item.GetComponent<GroundSwitch>() != null)
            {
                item.GetComponent<GroundSwitch>().Trigger(true);
            }
            else if (item != null && item.GetComponent<Teleport>() != null)
            {
                item.GetComponent<Teleport>().Trigger();
            }
            else if (item != null && item.GetComponent<OneWayObstacle>() != null)
            {
                item.GetComponent<OneWayObstacle>().Trigger();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((boxCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Box")) ||
            (enemiesCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Enemies")) ||
            (platformsCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")))
        {
            if (active == false)
            {
                Activate();
            }
            else if (active && pressToRelease)
            {
                Deactivate();
            }
            itemsInTrigger++;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((boxCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Box")) ||
            (enemiesCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Enemies")) ||
            (platformsCanActivate && 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")))
        {
            itemsInTrigger--;
            if (releaseOnExit && itemsInTrigger == 0)
            {
                if (triggerOnRelease)
                {
                    Deactivate();
                }
                else
                {
                    softRelease();
                }
            }
        }
    }
}
