using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindBox : MonoBehaviour
{
    Vector2 windDirection;
    public float windMagnitude = 2;
    float mult;

    Transform background;
    Rigidbody2D boxRB;
    Transform boxTransform;

    int boxLM;
    int obstacleLM;
    void Start()
    {
        boxLM = LayerMask.GetMask("Box");
        obstacleLM = LayerMask.GetMask("Obstacles", "Hazards");

        background = transform.GetChild(0);
        background.localScale = new Vector2(transform.GetComponent<SpriteRenderer>().size.x, transform.GetComponent<SpriteRenderer>().size.y);
        GetComponent<BoxCollider2D>().size = background.localScale;

        windDirection = new Vector2(Mathf.Cos((transform.rotation.eulerAngles.z - 90) * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin((transform.rotation.eulerAngles.z - 90) * Mathf.Deg2Rad + Mathf.PI / 2));

        mult = 0;

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxTransform = GameObject.Find("Box").GetComponent<Transform>();

        Box.windGravity = 0;
        BoxVelocity.windVelocity = 0;

        StartCoroutine(Glow());
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (1 << collider.gameObject.layer == boxLM)
        {
            if (Box.isGrounded)
            {
                if (Box.isCrouching)
                {
                    mult = 0.4f;
                }
                else
                {
                    mult = 1;
                }
            }
            else
            {
                mult = 1.5f;
            }

            RaycastHit2D detectWall = Physics2D.BoxCast(boxRB.position, new Vector2(boxTransform.lossyScale.x * 1.05f, boxTransform.lossyScale.y * 0.9f),
                0, Vector2.down, 0, obstacleLM);

            if (detectWall.collider != null || Mathf.Abs(windDirection.x * windMagnitude * mult) < 0.1)
            {
                mult = 0;
            }

            BoxVelocity.windVelocity = windDirection.x * windMagnitude * mult;


            if (Box.dashActive || Box.dodgeInvulActive)
            {
                Box.windGravity = 0;
            }
            else
            {
                Box.windGravity = -windDirection.y * windMagnitude / 2;
            }
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (1 << collider.gameObject.layer == boxLM)
        {
            mult = 0;
            Box.windGravity = 0;
            BoxVelocity.windVelocity = 0;
        }
    }

    IEnumerator Glow()
    {
        Renderer renderer = transform.GetComponent<Renderer>();
        Color initialColor = renderer.material.color;
        float colorChangeSpeed = 1f;
        if (windMagnitude > 8)
        {
            initialColor = Color.red;
            initialColor.a = renderer.material.color.a * 3;
            colorChangeSpeed *= 5;
        }
        Color newColor = initialColor;
        while (true)
        {
            while (newColor.a > 0)
            {
                newColor.a -= colorChangeSpeed * Time.deltaTime;
                renderer.material.color = newColor;
                yield return null;
            }
            while (newColor.a < initialColor.a)
            {
                newColor.a += colorChangeSpeed * Time.deltaTime;
                renderer.material.color = newColor;
                yield return null;
            }
        }
    }
}
