using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportCheck : MonoBehaviour
{
    InputBroker inputs;

    Transform followBox;
    float teleportDistancex;
    float cornerCheckDistMult = 0.3f;

    LayerMask mask;

    float checkExtents;
    Vector2 boxPointA;
    Vector2 boxPointB;
    Vector3 boxPointC;
    Vector4 boxPointD;
    Vector2 boxPointE;
    Vector2 boxPointF;
    Vector3 boxPointG;
    Vector4 boxPointH;
    bool centerCovered;
    bool pointACovered; //Sw
    bool pointBCovered; //SE
    bool pointCCovered; //NW
    bool pointDCovered; //NE
    bool pointECovered; //N
    bool pointFCovered; //S
    bool pointGCovered; //E
    bool pointHCovered; //W

    bool teleportBlocked = false;
    public bool successfulTeleport = true;

    bool debugEnabled = false;

    private void Start()
    {
        inputs = GameObject.Find("Box").GetComponent<InputBroker>();
        followBox = GameObject.Find("Box").GetComponent<Transform>();
        if (followBox.GetComponent<Box>().debugEnabled)
        {
            debugEnabled = true;
        }
        StartCoroutine(teleport());

        mask = LayerMask.GetMask("Obstacles");
    }
    void Update()
    {
        //Debug.Log("successful teleport?: " + successfulTeleport);

        transform.position = new Vector2(followBox.position.x + teleportDistancex, followBox.position.y);

        checkExtents = transform.lossyScale.x * 0.75f;
        boxPointA = new Vector2(transform.position.x - checkExtents * cornerCheckDistMult, transform.position.y - checkExtents * cornerCheckDistMult); //SW
        boxPointB = new Vector2(transform.position.x + checkExtents * cornerCheckDistMult, transform.position.y - checkExtents * cornerCheckDistMult); //SE
        boxPointC = new Vector2(transform.position.x - checkExtents * cornerCheckDistMult, transform.position.y + checkExtents * cornerCheckDistMult); //NW
        boxPointD = new Vector2(transform.position.x + checkExtents * cornerCheckDistMult, transform.position.y + checkExtents * cornerCheckDistMult); //NE
        boxPointE = new Vector2(transform.position.x, transform.position.y + checkExtents); //N
        boxPointF = new Vector2(transform.position.x, transform.position.y - checkExtents); //S
        boxPointG = new Vector2(transform.position.x + checkExtents, transform.position.y); //E
        boxPointH = new Vector2(transform.position.x - checkExtents, transform.position.y); //W
        pointACovered = Physics2D.OverlapPoint(boxPointA, mask);
        pointBCovered = Physics2D.OverlapPoint(boxPointB, mask);
        pointCCovered = Physics2D.OverlapPoint(boxPointC, mask);
        pointDCovered = Physics2D.OverlapPoint(boxPointD, mask);
        pointECovered = Physics2D.OverlapPoint(boxPointE, mask);
        pointFCovered = Physics2D.OverlapPoint(boxPointF, mask);
        pointGCovered = Physics2D.OverlapPoint(boxPointG, mask);
        pointHCovered = Physics2D.OverlapPoint(boxPointH, mask);
        centerCovered = Physics2D.OverlapPoint(transform.position, mask);

        if (debugEnabled)
        {
            Debug.DrawLine(boxPointE, boxPointD);
            Debug.DrawLine(boxPointD, boxPointG);
            Debug.DrawLine(boxPointG, boxPointB);
            Debug.DrawLine(boxPointB, boxPointF);
            Debug.DrawLine(boxPointF, boxPointA);
            Debug.DrawLine(boxPointA, boxPointH);
            Debug.DrawLine(boxPointH, boxPointC);
            Debug.DrawLine(boxPointC, boxPointE);
        }



        teleportBlocked = false;
        RaycastHit2D[] blockCast = Physics2D.RaycastAll(followBox.position, (transform.position - followBox.position).normalized,
            (transform.position - followBox.position).magnitude, mask);
        foreach (RaycastHit2D item in blockCast)
        {
            if (item.collider.GetComponent<Teleblock>() != null)
            {
                teleportBlocked = true;
                item.collider.GetComponent<Teleblock>().xHeight = item.point.y;
                item.collider.GetComponent<Teleblock>().xActive = true;
            }
        }

        if (debugEnabled)
        {
            Color color = Color.white;
            if (teleportBlocked) { color = Color.red; }
            Debug.DrawLine(transform.position, followBox.position, color);
        }



        if ((pointACovered == true && pointBCovered == true && pointCCovered == true && pointDCovered == true && centerCovered == true
            && pointECovered == true && pointFCovered == true && pointGCovered == true && pointHCovered == true) || teleportBlocked)
        {
            successfulTeleport = false;
            gameObject.GetComponent<Renderer>().material.color = Color.red;
        }
        else
        {
            successfulTeleport = true;
            gameObject.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
    IEnumerator teleport()
    {
        if (inputs.leftStick.x != 0)
        {
            teleportDistancex = Box.teleportRange * Mathf.Sign(inputs.leftStick.x);
        }
        else
        {
            teleportDistancex = Box.teleportRange * Box.lookingRight;
        }
        yield return new WaitForSeconds(Box.teleportDelay);
    }
}
