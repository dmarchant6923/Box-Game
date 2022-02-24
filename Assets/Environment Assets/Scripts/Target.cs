using System;
using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [HideInInspector] public RaycastHit2D targetRayCast;
    Transform target;
    [HideInInspector] public float targetRadius;
    int boxLayerMask;
    Rigidbody2D boxRigidBody;
    float boxAngularVelocity;
    bool targetRayCastEnabled = true;
    [HideInInspector] public bool targetWasHit = false;

    // Start is called before the first frame update
    void Start()
    {
        target = this.GetComponent<Transform>();
        targetRadius = target.lossyScale.x/2;
        boxLayerMask = LayerMask.GetMask("Box");
        boxRigidBody = GameObject.Find("Box").GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        boxAngularVelocity = boxRigidBody.angularVelocity;

        targetRayCast = Physics2D.CircleCast(target.position, targetRadius, new Vector2(0, 0), boxLayerMask);
        if (targetRayCast.collider != null && 1 << targetRayCast.collider.gameObject.layer == boxLayerMask && targetRayCastEnabled == true)
        {
            if (Box.teleportActive == false)
            {
                if (Box.boxHitboxActive && targetWasHit == false)
                {
                    targetRayCastEnabled = false;
                    Box.activateHitstop = true;
                    targetWasHit = true;
                    if (target.GetComponent<MovingObjects>() != null)
                    {
                        target.GetComponent<MovingObjects>().enabled = false;
                        target.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
                    }
                    TargetTestUI.targetsHit += 1;
                }
            }
        }
        if (Box.enemyHitstopActive == false && Box.activateHitstop == false && targetWasHit == true)
        {
            Destroy(gameObject);
            targetWasHit = false;
        }
    }
}
