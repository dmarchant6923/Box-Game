using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedVehicleIsGrounded : MonoBehaviour
{
    int obstacleLM;
    int platformLM;
    float zAngle;
    float maxZAngle = 30;

    float groundedTime = 0;
    float timeTilIsGrounded = 0.2f;
    bool isGrounded = false;

    bool scriptEnabled = true;
    Transform vehicleParent;
    Rigidbody2D enemyRB;
    float brakeSpeed = 10;

    void Start()
    {
        obstacleLM = LayerMask.GetMask("Obstacles");
        platformLM = LayerMask.GetMask("Platforms");
        vehicleParent = transform.parent;
        enemyRB = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        scriptEnabled = vehicleParent.GetComponent<EnemyManager>().scriptsEnabled;
        if (scriptEnabled == false)
        {
            gameObject.GetComponent<GroundedVehicleIsGrounded>().enabled = false;
        }

        zAngle = transform.rotation.eulerAngles.z;

        if (isGrounded == true)
        {
            groundedTime += Time.deltaTime;
            if (enemyRB.velocity.x > 0.5f)
            {
                enemyRB.velocity = new Vector2(enemyRB.velocity.x - brakeSpeed * Time.deltaTime, enemyRB.velocity.y);
            }
            if (enemyRB.velocity.x < -0.5f)
            {
                enemyRB.velocity = new Vector2(enemyRB.velocity.x + brakeSpeed * Time.deltaTime, enemyRB.velocity.y);
            }
        }
        else
        {
            groundedTime = 0;
        }
        if (groundedTime >= timeTilIsGrounded)
        {
            vehicleParent.GetComponent<EnemyBehavior_GroundedVehicle>().enemyIsGrounded = true;
        }
        else
        {
            vehicleParent.GetComponent<EnemyBehavior_GroundedVehicle>().enemyIsGrounded = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == obstacleLM || 1 << collision.gameObject.layer == platformLM)
        {
            if (zAngle >= 360 - maxZAngle || zAngle <= maxZAngle)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
            vehicleParent.GetComponent<EnemyBehavior_GroundedVehicle>().movingPlatformsExtraWalkSpeed = collision.attachedRigidbody.velocity.x;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == obstacleLM || 1 << collision.gameObject.layer == platformLM)
        {
            isGrounded = false;
        }
    }
}
