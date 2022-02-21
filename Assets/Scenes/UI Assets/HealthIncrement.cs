using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthIncrement : MonoBehaviour
{
    float window = 2;
    float timer = 0;
    Color initialColor;
    Color newColor;
    public Text incrementText;
    

    void Start()
    {

    }

    void Update()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y - 70 * Time.deltaTime);
        if (timer > window * 0.5f)
        {
            newColor = incrementText.color;
            newColor = new Color(newColor.r, newColor.g, newColor.b, newColor.a - 2 / window * Time.deltaTime);
            incrementText.color = newColor;
        }
        if (timer > window)
        {
            Destroy(gameObject);
        }
        timer += Time.deltaTime;
    }
}
