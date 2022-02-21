using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraFollowBox : MonoBehaviour
{
    InputBroker inputs;

    Transform followBox;
    Vector2 boxOriginalScale;
    float boxTruePositionY;
    public Transform blastZone;
    Camera cam;
    Vector2 boxPositionAvg;
    float camDisplacementScalarx = 1;
    float camDisplacementMaxx = 1.5f;

    Vector3 camTruePosition;

    public float maxCamSize = 14;
    public float minCamSize = 5;
    float camSizeShiftSpeed = 10;


    int camArrayLength = 12;
    List<Vector2> camPositionArray;

    [HideInInspector] public bool startCamShake = false;
    bool ignoreBoxDamageShake = false;
    [HideInInspector] public bool boxDamageShake = false;
    [HideInInspector] public float boxDamageTaken = 0;
    [HideInInspector] public Vector2 shakeInfo; // x = damage, y = distance
    Vector3 camShakeOffset;

    Vector3 camLookOffset;
    float camLookMax = 4;
    float camLookSpeed = 20;

    public bool disable = false;

    private void Awake()
    {
        inputs = GameObject.Find("Box").GetComponent<InputBroker>();
        followBox = GameObject.Find("Box").GetComponent<Transform>();

        boxOriginalScale = followBox.localScale;
        cam = GetComponent<Camera>();
        if (PlayerPrefs.GetFloat("camSize", 5) > maxCamSize)
        {
            PlayerPrefs.SetFloat("camSize", maxCamSize);
        }
        if (PlayerPrefs.GetFloat("camSize", 5) < minCamSize)
        {
            PlayerPrefs.SetFloat("camSize", minCamSize);
        }
        cam.orthographicSize = PlayerPrefs.GetFloat("camSize", 5);

        camPositionArray = new List<Vector2>(new Vector2[camArrayLength]);
        for (int i = 0; i < camArrayLength; i++)
        {
            camPositionArray[i] = followBox.position;
        }
    }

    void FixedUpdate()
    {
        if (inputs.downDpad && cam.orthographicSize < maxCamSize)
        {
            cam.orthographicSize += camSizeShiftSpeed * Time.deltaTime;
            if (cam.orthographicSize > maxCamSize)
            {
                cam.orthographicSize = maxCamSize;
            }
            PlayerPrefs.SetFloat("camSize", cam.orthographicSize);
            PlayerPrefs.Save();
        }
        if (inputs.upDpad && cam.orthographicSize > minCamSize)
        {
            cam.orthographicSize -= camSizeShiftSpeed * Time.deltaTime;
            if (cam.orthographicSize < minCamSize)
            {
                cam.orthographicSize = minCamSize;
            }
            PlayerPrefs.SetFloat("camSize", cam.orthographicSize);
            PlayerPrefs.Save();
        }

        for (int i = 0; i < camArrayLength - 1; i++)
        {
            camPositionArray[i] = camPositionArray[i + 1];
        }
        
        if (disable == false)
        {
            camPositionArray[camArrayLength - 1] = new Vector2(followBox.position.x, followBox.position.y - followBox.localScale.y / 2 + boxOriginalScale.y); //followBox.position;
            boxTruePositionY = followBox.position.y - followBox.localScale.y / 2 + boxOriginalScale.y;
            if (blastZone != null && boxTruePositionY <= blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize)
            {
                camPositionArray[camArrayLength - 1] = new Vector2(camPositionArray[camArrayLength - 1].x,
                    blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize + boxOriginalScale.y);
            }
        }
        else
        {
            camPositionArray[camArrayLength - 1] = camPositionArray[camArrayLength - 2];
        }


        boxPositionAvg = new Vector2(camPositionArray.Average(x=>x.x), camPositionArray.Average(x=>x.y));

        camDisplacementScalarx = Mathf.MoveTowards(camDisplacementScalarx, Box.lookingRight, (0.25f + Mathf.Abs(Box.lookingRight - camDisplacementScalarx) * 5) * Time.deltaTime);

        if (blastZone != null && boxPositionAvg.y <= blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize + boxOriginalScale.y)
        {
            camTruePosition = new Vector3(boxPositionAvg.x + (camDisplacementMaxx * camDisplacementScalarx), 
                blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize + boxOriginalScale.y);
        }
        else
        {
            camTruePosition = new Vector3(boxPositionAvg.x + (camDisplacementMaxx * camDisplacementScalarx), boxPositionAvg.y);
        }


        if (inputs.leftDpad == false && inputs.rightDpad)
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, camLookMax, (camLookSpeed - camLookOffset.x * (camLookSpeed / camLookMax) * 0.95f) * Time.deltaTime), 0);
        }
        else if (inputs.leftDpad && inputs.rightDpad == false)
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, -camLookMax, (camLookSpeed + camLookOffset.x * (camLookSpeed / camLookMax) * 0.95f) * Time.deltaTime), 0);
        }
        else
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, 0, camLookSpeed * Time.deltaTime), 0);
        }



        if (disable == false)
        {
            transform.position = camTruePosition + camShakeOffset + camLookOffset + Vector3.back * 10;
        }
    }

    private void Update()
    {
        if (startCamShake == true)
        {
            StartCoroutine(CameraShake(shakeInfo.x, shakeInfo.y));
            startCamShake = false;
        }
        if (boxDamageShake == true)
        {
            if (ignoreBoxDamageShake == false)
            {
                StartCoroutine(CameraShake(boxDamageTaken, 12));
            }
            boxDamageShake = false;
        }
    }

    IEnumerator CameraShake(float damage, float distance)
    {
        if (distance < 2) { distance = 2; }
        float shakeDistance = Mathf.Sqrt(damage) / (Mathf.Sqrt(distance) * 4);
        if (shakeDistance > 1) { shakeDistance = 1f; }
        if (shakeDistance < 0.1f) { shakeDistance = 0.1f; }
        //Debug.Log(shakeDistance);
        float currentDistance = shakeDistance;
        Vector3 addToOffset;
        ignoreBoxDamageShake = true;
        yield return new WaitForFixedUpdate();
        ignoreBoxDamageShake = false;
        float shakeStepTime = 0.03f;
        addToOffset = new Vector3(Random.Range(-1f,1f), Random.Range(-1f, 1f)).normalized;
        //Debug.Log(addToOffset);
        int i = 0;
        while (currentDistance > 0.05f)
        {
            addToOffset = -addToOffset.normalized * currentDistance;
            float currentCamSize = cam.orthographicSize;
            camShakeOffset += addToOffset * currentCamSize / maxCamSize;
            yield return new WaitForSeconds(shakeStepTime);
            camShakeOffset -= addToOffset * currentCamSize / maxCamSize;
            currentDistance -= (1f + shakeDistance) * shakeStepTime;
            i++;
        }
    }
}
