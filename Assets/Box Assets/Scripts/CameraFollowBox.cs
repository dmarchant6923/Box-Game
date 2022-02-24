using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraFollowBox : MonoBehaviour
{
    InputBroker inputs;

    Transform followBox;

    Vector2 boxOriginalScale;
    public Transform blastZone;
    Camera cam;
    Vector2 boxPositionAvg;
    float camDisplacementScalarx = 1;
    float camDisplacementMaxx = 1.5f;

    Vector3 camTruePosition;

    public float maxCamSize = 14;
    public float minCamSize = 5;
    float camSizeShiftSpeed = 10;
    public bool overrideCamSize = false;
    public float overrideSize = 10;


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
    [HideInInspector] public bool overridePosition;
    [HideInInspector] public Vector2 forcedPosition;

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
        for (int i = 0; i < camPositionArray.Count; i++)
        {
            camPositionArray[i] = followBox.position;
        }
    }

    void FixedUpdate()
    {
        //camera size
        if (overrideCamSize)
        {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, overrideSize, camSizeShiftSpeed * Time.deltaTime);
        }
        else if (cam.orthographicSize != PlayerPrefs.GetFloat("camSize", 5))
        {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, PlayerPrefs.GetFloat("camSize", 5), camSizeShiftSpeed * Time.deltaTime);
        }
        else if (inputs.downDpad && cam.orthographicSize < maxCamSize)
        {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, maxCamSize, camSizeShiftSpeed * Time.deltaTime);
            PlayerPrefs.SetFloat("camSize", cam.orthographicSize);
            PlayerPrefs.Save();
        }
        else if (inputs.upDpad && cam.orthographicSize > minCamSize)
        {
            cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, minCamSize, camSizeShiftSpeed * Time.deltaTime);
            PlayerPrefs.SetFloat("camSize", cam.orthographicSize);
            PlayerPrefs.Save();
        }


        //shifting campositionarray values over by one, then creating the new value
        for (int i = 0; i < camPositionArray.Count - 1; i++)
        {
            camPositionArray[i] = camPositionArray[i + 1];
        }
        if (disable == false && overridePosition == false)
        {
            camPositionArray[camPositionArray.Count - 1] = new Vector2(followBox.position.x, followBox.position.y - followBox.localScale.y / 2 + boxOriginalScale.y);
        }
        else if (overridePosition && disable == false)
        {
            camPositionArray[camPositionArray.Count - 1] = forcedPosition;
        }
        else
        {
            camPositionArray[camPositionArray.Count - 1] = camPositionArray[camPositionArray.Count - 2];
        }


        //calculating a Vector2 of the x and y values of camPositionArray
        boxPositionAvg = new Vector2(camPositionArray.Average(x=>x.x), camPositionArray.Average(x=>x.y));


        //shifting camera movement left and right based on box.lookingright
        if (disable == false && overridePosition == false)
        {
            camDisplacementScalarx = Mathf.MoveTowards(camDisplacementScalarx, Box.lookingRight, (0.25f + Mathf.Abs(Box.lookingRight - camDisplacementScalarx) * 5) * Time.deltaTime);
        }
        else if (disable)
        {
            camDisplacementScalarx = 0;
        }
        else if (overridePosition)
        {
            camDisplacementScalarx = Mathf.MoveTowards(camDisplacementScalarx, 0, 5 * Time.deltaTime);
        }


        //calculate the true position of the camera, which is the average position plus camera shifting left and right. If camera can see blast zone, move up.
        if (blastZone != null && boxPositionAvg.y <= blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize + boxOriginalScale.y * 2)
        {
            camTruePosition = new Vector3(boxPositionAvg.x + (camDisplacementMaxx * camDisplacementScalarx), 
                blastZone.position.y + blastZone.lossyScale.y / 2 + cam.orthographicSize + boxOriginalScale.y * 2);
        }
        else
        {
            camTruePosition = new Vector3(boxPositionAvg.x + (camDisplacementMaxx * camDisplacementScalarx), boxPositionAvg.y);
        }


        //peeking left and right based on dpad inputs
        if (inputs.leftDpad == false && inputs.rightDpad && disable == false && overridePosition == false)
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, camLookMax, (camLookSpeed - camLookOffset.x * (camLookSpeed / camLookMax) * 0.95f) * Time.deltaTime), 0);
        }
        else if (inputs.leftDpad && inputs.rightDpad == false && disable == false && overridePosition == false)
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, -camLookMax, (camLookSpeed + camLookOffset.x * (camLookSpeed / camLookMax) * 0.95f) * Time.deltaTime), 0);
        }
        else
        {
            camLookOffset = new Vector3(Mathf.MoveTowards(camLookOffset.x, 0, camLookSpeed * Time.deltaTime), 0);
        }


        //calculating the final position of the camera based on the box avg position, shifting left and right, camera shaking, and peeking left and right.
        if (disable == false)
        {
            transform.position = camTruePosition + camShakeOffset + camLookOffset + Vector3.back * 10;
        }
        else
        {
            transform.position += camShakeOffset;
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

    public IEnumerator ResetCamera()
    {
        camPositionArray = new List<Vector2>(new Vector2[30]);
        for (int i = 0; i < camPositionArray.Count; i++)
        {
            camPositionArray[i] = transform.position;
        }
        while (camPositionArray.Count > camArrayLength)
        {
            float window = 0.03f;
            float timer = 0;
            while (timer < window)
            {
                timer += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
            List<Vector2> temp = new List<Vector2>(new Vector2[camPositionArray.Count - 1]);
            Vector2 boxPositionAvg = new Vector2(camPositionArray.Average(x => x.x), camPositionArray.Average(x => x.y));
            for (int i = 0; i < temp.Count; i++)
            {
                temp[i] = boxPositionAvg;
            }
            camPositionArray = temp;
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
