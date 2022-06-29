using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMovement : MonoBehaviour
{
    public Transform pointParent;
    List<Transform> points = new List<Transform>();

    Rigidbody2D rb;

    public bool loop = false;
    LineRenderer line;

    public float speed = 10;
    public bool pauseAtEnds = false;
    public float pauseTime = 1f;

    [HideInInspector] public bool stopMovement = false;

    private void Start()
    {
        for (int i = 0; i < pointParent.childCount; i++)
        {
            points.Add(pointParent.GetChild(i));
        }

        line = GetComponent<LineRenderer>();
        line.positionCount = points.Count;
        if (line.positionCount < 3)
        {
            loop = false;
        }
        if (loop)
        {
            line.loop = true;
        }

        for (int i = 0; i < points.Count; i++)
        {
            points[i].parent = null;
            points[i].transform.localScale = Vector2.one * 0.5f;

            line.SetPosition(i, points[i].position);
        }

        rb = GetComponent<Rigidbody2D>();

        transform.position = points[0].position;
        StartCoroutine(Movement());
    }

    IEnumerator Movement()
    {
        int i = 0;
        bool forward = true;
        while (true)
        {
            if (loop)
            {
                i++;
                if (i >= points.Count)
                {
                    i = 0;
                }
            }
            else
            {
                if (forward)
                {
                    i++;
                    if (i >= points.Count)
                    {
                        i = points.Count -2;
                        forward = false;
                    }
                }
                else
                {
                    i--;
                    if (i < 0)
                    {
                        i = 1;
                        forward = true;
                    }
                }
            }
            while ((transform.position - points[i].position).magnitude > 0.05f)
            {
                if (stopMovement == false)
                {
                    transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
                    rb.velocity = (points[i].position - transform.position).normalized * 0.01f;
                }
                yield return null;
            }
            transform.position = points[i].position;
            rb.velocity = Vector2.zero;
            if (pauseAtEnds)
            {
                yield return new WaitForSeconds(pauseTime);
            }
        }
    }
}
