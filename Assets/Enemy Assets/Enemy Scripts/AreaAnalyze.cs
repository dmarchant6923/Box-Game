using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaAnalyze : MonoBehaviour
{
    public float radius = 10;
    public float offset = 1;
    public int numPoints = 360;
    Vector2[] points;

    LineRenderer perimeter;
    Rigidbody2D rb;
    SpriteRenderer sprite;

    float startAngle = 0;

    int obstacleLM;

    void Start()
    {
        perimeter = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        obstacleLM = LayerMask.GetMask("Obstacles");

        points = new Vector2[numPoints];
        perimeter.positionCount = points.Length;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        for (int i = 0; i < numPoints; i++)
        {
            float angle = startAngle + (float)i * 360 / (float)numPoints;
            Vector2 vector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad + Mathf.PI / 2),
                Mathf.Sin(angle * Mathf.Deg2Rad + Mathf.PI / 2));

            RaycastHit2D circleCast = Physics2D.CircleCast(rb.position, offset, vector, radius, obstacleLM);
            float distance = radius;
            if (circleCast.collider != null)
            {
                distance = circleCast.distance;
            }
            points[i] = rb.position + vector * distance;
            perimeter.SetPosition(i, points[i]);


            if (i == 0)
            {
                Debug.DrawRay(transform.position, vector * distance, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position, vector * distance);
            }
        }
    }
}
