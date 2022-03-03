using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : MonoBehaviour
{
    float sizeX = 1;
    float sizeY = 1;
    // Start is called before the first frame update
    void Start()
    {
        sizeX = GetComponent<SpriteRenderer>().size.x;
        sizeY = GetComponent<SpriteRenderer>().size.y;
        GetComponent<BoxCollider2D>().size = new Vector2(sizeX, sizeY);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
