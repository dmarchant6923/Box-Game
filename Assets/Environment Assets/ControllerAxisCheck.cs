using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAxisCheck : MonoBehaviour
{
    InputBroker inputs;
    private void Start()
    {
        inputs = GameObject.Find("Box").GetComponent<InputBroker>();
    }
    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(transform.position, inputs.leftStick * transform.lossyScale.x / 2);
        Debug.DrawRay(transform.position, inputs.rightStick * transform.lossyScale.x / 2, Color.yellow);
        Debug.DrawRay(new Vector2(transform.position.x - transform.lossyScale.x / 2, transform.position.y - transform.lossyScale.y / 2),
            Vector2.down * inputs.XboxL * transform.lossyScale.x);
        Debug.DrawRay(new Vector2(transform.position.x + transform.lossyScale.x / 2, transform.position.y - transform.lossyScale.y / 2),
            Vector2.down * inputs.XboxR * transform.lossyScale.x);
    }
}
