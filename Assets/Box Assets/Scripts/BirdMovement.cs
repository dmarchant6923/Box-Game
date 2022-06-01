using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdMovement : MonoBehaviour
{
    InputBroker inputs;
    InputBroker boxInputs;
    Rigidbody2D birdRB;

    float maxHorizSpeed = 8;
    float initialHorizAccel = 25;
    float horizAccel;

    float truePosMaxVertVelocity = 5;
    float truePosAcceleration = 10;
    float truePosVertVelocity;
    Vector2 truePosition;

    int groundLM;

    void Start()
    {
        foreach (InputBroker input in GetComponents<InputBroker>())
        {
            if (input.main == false)
            {
                inputs = input;
            }
        }

        birdRB = GetComponent<Rigidbody2D>();
        truePosition = birdRB.position;

        horizAccel = initialHorizAccel;

        groundLM = LayerMask.GetMask("Obstacles", "Platforms");

        StartCoroutine(Flap());
    }

    // Update is called once per frame
    void Update()
    {
        if (Box.boxInputsEnabled)
        {
            inputs.inputsEnabled = true;
        }
        else
        {
            inputs.inputsEnabled = false;
        }

        if (Box.damageActive || Box.boxEnemyPulseActive)
        {
            horizAccel = initialHorizAccel / 4;
        }
        else
        {
            horizAccel = initialHorizAccel;
        }

        truePosition = new Vector2(birdRB.position.x, truePosition.y);
        RaycastHit2D castUp = Physics2D.BoxCast(truePosition + Vector2.up * transform.localScale.y / 2,
                                                    new Vector2(transform.localScale.x * 0.8f, transform.localScale.y / 2), 0, Vector2.zero, 0, groundLM);
        RaycastHit2D castDown = Physics2D.BoxCast(truePosition + Vector2.down * transform.localScale.y / 2,
                                                    new Vector2(transform.localScale.x * 0.8f, transform.localScale.y / 2), 0, Vector2.zero, 0, groundLM);
        float correctSpeed = 4;
        if (castUp.collider != null && castUp.collider.GetComponent<PlatformDrop>() == null)
        {
            truePosition += Vector2.down * correctSpeed * Time.deltaTime;
            truePosVertVelocity = -correctSpeed * Time.deltaTime;
        }
        else if (castDown.collider != null && (castDown.collider.GetComponent<PlatformDrop>() == null || (castDown.collider.GetComponent<PlatformDrop>() != null && birdRB.velocity.y < 0)))
        {
            truePosition += Vector2.up * correctSpeed * Time.deltaTime;
            truePosVertVelocity = correctSpeed * Time.deltaTime;
        }
        else
        {
            if (inputs.jumpButtonDown || inputs.leftStick.y > 0 && truePosVertVelocity < truePosMaxVertVelocity)
            {
                float mult = 1;
                if (inputs.jumpButtonDown == false)
                {
                    mult = inputs.leftStick.y;
                }
                truePosVertVelocity = Mathf.Min(truePosVertVelocity + truePosAcceleration * Time.deltaTime, truePosMaxVertVelocity * mult);
            }
            else if (inputs.leftStick.y < 0 && truePosVertVelocity > -truePosMaxVertVelocity)
            {
                float mult = inputs.leftStick.y;
                truePosVertVelocity -= truePosAcceleration * Time.deltaTime;
            }
            else
            {
                truePosVertVelocity = Mathf.MoveTowards(truePosVertVelocity, 0, truePosAcceleration * 0.5f * Time.deltaTime);
            }
            truePosition = new Vector2(truePosition.x, truePosition.y + truePosVertVelocity * Time.deltaTime);
        }



        if (inputs.leftStick.x != 0)
        {
            if (inputs.leftStick.x > 0 && BoxVelocity.velocitiesX[0] < maxHorizSpeed)
            {
                BoxVelocity.velocitiesX[0] += horizAccel * Time.deltaTime;
            }
            if (inputs.leftStick.x < 0 && BoxVelocity.velocitiesX[0] > -maxHorizSpeed)
            {
                BoxVelocity.velocitiesX[0] -= horizAccel * Time.deltaTime;
            }
        }
        else
        {
            BoxVelocity.velocitiesX[0] = Mathf.MoveTowards(BoxVelocity.velocitiesX[0], 0, horizAccel * 0.75f * Time.deltaTime);
        }
        if (Mathf.Abs(BoxVelocity.velocitiesX[0]) > maxHorizSpeed)
        {
            BoxVelocity.velocitiesX[0] = Mathf.MoveTowards(BoxVelocity.velocitiesX[0], maxHorizSpeed * Mathf.Sign(BoxVelocity.velocitiesX[0]), horizAccel * Time.deltaTime);
        }
        if (Box.damageActive)
        {
            truePosition = birdRB.position;
            //BoxVelocity.velocitiesX[0] = Mathf.MoveTowards(BoxVelocity.velocitiesX[0], 0, horizAcceleration * 0.5f * Time.deltaTime);
            ////birdRB.velocity = new Vector2(birdRB.velocity.x, Mathf.MoveTowards(birdRB.velocity.y, 0, horizAcceleration * 0.5f * Time.deltaTime));
        }

        Debug.DrawRay(truePosition + Vector2.left, Vector2.right * 2);
    }

    IEnumerator Flap()
    {
        while (true)
        {
            while(birdRB.position.y > truePosition.y || Box.damageActive)
            {
                yield return null;
            }
            birdRB.velocity = new Vector2(birdRB.velocity.x, Random.Range(2f,3f) + truePosVertVelocity);
            yield return new WaitForSeconds(0.2f);
        }
    }
}
