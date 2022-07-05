using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneWayObstacle : MonoBehaviour
{
    BoxCollider2D boxCollider;
    LineRenderer line;
    SpriteRenderer sprite;
    int mult = 1;

    void Start()
    {
        foreach (BoxCollider2D collider in GetComponents<BoxCollider2D>())
        {
            if (collider.isTrigger == false)
            {
                boxCollider = collider;
                break;
            }
        }
        line = GetComponent<LineRenderer>();
        line.endWidth = transform.localScale.x;
        line.startWidth = transform.localScale.x;

        sprite = GetComponent<SpriteRenderer>();
        sprite.enabled = false;
    }

    public void Trigger()
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + (180 * mult));
        mult *= -1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.GetComponent<Box>() != null && Box.boxHitstopActive == false) || collision.GetComponent<Box>() == null)
        {
            Physics2D.IgnoreCollision(collision, boxCollider, true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Physics2D.IgnoreCollision(collision, boxCollider, false);
    }
}
