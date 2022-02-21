using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseField : MonoBehaviour
{
    Color pulseColor;
    // Start is called before the first frame update
    void Start()
    {
        pulseColor = this.GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        pulseColor.a -= Time.deltaTime * 4;
        this.GetComponent<Renderer>().material.color = pulseColor;
        if (pulseColor.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
