using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSwitch : MonoBehaviour
{
    public bool cameraSwitch = false;
    public bool averageBoxAndTransform = false;
    public float forceCameraSize = 15;
    float camMoveSpeed = 50;

    Transform boxTransform;

    public bool debugEnabled = false;

    void Start()
    {
        boxTransform = GameObject.Find("Box").transform;
        if (debugEnabled == false)
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == boxTransform && FindObjectOfType<CameraFollowBox>() != null && cameraSwitch)
        {
            FindObjectOfType<CameraFollowBox>().disable = true;
            FindObjectOfType<CameraFollowBox>().overrideCamSize = true;
            FindObjectOfType<CameraFollowBox>().overrideSize = forceCameraSize;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform == boxTransform && FindObjectOfType<CameraFollowBox>() != null && cameraSwitch)
        {
            Transform camera = GameObject.Find("Main Camera").transform;
            float trueMoveSpeed = 1;
            Vector3 truePosition;
            if (averageBoxAndTransform)
            {
                truePosition = (boxTransform.position + transform.position) / 2;
                trueMoveSpeed = camMoveSpeed / 2;
            }
            else
            {
                truePosition = transform.position;
                trueMoveSpeed = 1 + (camMoveSpeed * (transform.position - camera.position + Vector3.back * 10).magnitude / 20);
            }

            camera.position = Vector2.MoveTowards(camera.position, truePosition, trueMoveSpeed * Time.deltaTime);
            camera.position = camera.position + Vector3.back * 10;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform == boxTransform && FindObjectOfType<CameraFollowBox>() != null && cameraSwitch)
        {
            FindObjectOfType<CameraFollowBox>().disable = false;
            StartCoroutine(FindObjectOfType<CameraFollowBox>().ResetCamera());
            FindObjectOfType<CameraFollowBox>().overrideCamSize = false;
        }
    }
}
