using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMCamera : MonoBehaviour
{
    public GameObject startCube;
    public GameObject endCube;

    float startX;
    float endX;

    void Start()
    {
        startX = startCube.transform.position.x;
        endX = endCube.transform.position.x;

        transform.position = new Vector3(Random.Range(startX, endX), 0, transform.position.z);
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(Mathf.MoveTowards(transform.position.x, 10000, 1.5f * Time.deltaTime), 0, transform.position.z);
        if (transform.position.x >= endX)
        {
            transform.position = new Vector3(transform.position.x - (endX - startX), 0, transform.position.z);
        }
    }
}
