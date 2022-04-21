using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoxPerks : MonoBehaviour
{
    Box boxScript;
    Transform boxTransform;
    Rigidbody2D boxRB;
    public GameObject buff;
    GameObject newBuff;
    float flickerTime = 4;
    float buffTimer = 0;
    [HideInInspector] public static bool buffActive = false;

    InputBroker inputs;

    float initialDashSpeed;
    float initialGravityMult;
    float initialHMaxSpeed;
    float initialJumpSpeed;
    float initialDJumpSpeed;
    float initialAirAccel;

    float initialDashCD;
    float initialPulseCD;
    float initialTeleCD;

    [HideInInspector] public static bool activateSpeed = false;
    [HideInInspector] public static bool speedActive = false;
    float speedMult = 1.2f;
    float speedActiveTime = 12;

    [HideInInspector] public static bool activateShield = false;
    [HideInInspector] public static bool shieldActive = false;
    float shieldActiveTime = 25;

    [HideInInspector] public static bool activateHeavy = false;
    [HideInInspector] public static bool heavyActive = false;
    float heavyActiveTime = 10;
    [HideInInspector] public static float heavyMult = 0.92f;

    [HideInInspector] public static bool activateSpikes = false;
    [HideInInspector] public static bool spikesActive = false;
    float spikesActiveTime = 16;
    public GameObject spikes;
    GameObject newSpikes;

    [HideInInspector] public static bool activateStar = false;
    [HideInInspector] public static bool starActive = false;
    float starMult = 1.15f;
    [HideInInspector] public static float starCDMult = 0.18f;
    float starActiveTime = 10;
    [HideInInspector] public static bool starDeactivated = false;

    [HideInInspector] public static bool activateJump = false;
    [HideInInspector] public static bool jumpActive = false;
    float jumpActiveTime = 12;
    [HideInInspector] public static bool unlimitedJumps = true;

    void Start()
    {
        boxScript = GetComponent<Box>();
        boxTransform = GetComponent<Transform>();
        boxRB = GetComponent<Rigidbody2D>();
        inputs = GetComponent<InputBroker>();

        buffTimer = 0;
        speedActive = false;
        shieldActive = false;
        heavyActive = false;
        spikesActive = false;
        starActive = false;
        jumpActive = false;

        initialDashSpeed = boxScript.dashSpeed;
        initialGravityMult = boxScript.gravityMult;
        initialHMaxSpeed = boxScript.horizMaxSpeed;
        initialJumpSpeed = boxScript.jumpSpeed;
        initialDJumpSpeed = boxScript.djumpSpeed;
        initialAirAccel = boxScript.initialAirAccel;

        initialDashCD = boxScript.dashCooldown;
        initialPulseCD = boxScript.pulseCooldown;
        initialTeleCD = boxScript.teleportCooldown;
    }

    void Update()
    {
        if (activateSpeed)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Speed());
            activateSpeed = false;
            activateShield = false;
            activateHeavy = false;
            activateSpikes = false;
            activateStar = false;
            activateJump = false;
        }
        if (activateShield)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Shield());
            activateShield = false;
            activateHeavy = false;
            activateSpikes = false;
            activateStar = false;
            activateJump = false;
        }
        if (activateHeavy)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Heavy());
            activateHeavy = false;
            activateSpikes = false;
            activateStar = false;
            activateJump = false;
        }
        if (activateSpikes)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Spikes());
            activateSpikes = false;
            activateStar = false;
            activateJump = false;
        }
        if (activateStar)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Star());
            activateStar = false;
            activateJump = false;
        }
        if (activateJump)
        {
            if (buffActive)
            {
                buffActive = false;
            }
            StartCoroutine(Jump());
            activateJump = false;
        }
    }

    IEnumerator Speed()
    {
        yield return null;
        speedActive = true;
        StartCoroutine(SpawnBuff(speedActiveTime, Color.green));
        boxScript.dashSpeed *= speedMult;
        boxScript.gravityMult *= speedMult;
        boxScript.horizMaxSpeed *= speedMult;
        boxScript.jumpSpeed *= speedMult;
        boxScript.djumpSpeed *= speedMult;
        boxScript.initialAirAccel *= speedMult;
        boxScript.airAccel *= speedMult;
        while (buffActive == true )
        {
            yield return null;
        }
        boxScript.dashSpeed = initialDashSpeed;
        boxScript.gravityMult = initialGravityMult;
        boxScript.horizMaxSpeed = initialHMaxSpeed;
        boxScript.jumpSpeed = initialJumpSpeed;
        boxScript.djumpSpeed = initialDJumpSpeed;
        boxScript.initialAirAccel = initialAirAccel;
        boxScript.airAccel = initialAirAccel;
        speedActive = false;
    }
    IEnumerator Shield()
    {
        yield return null;
        shieldActive = true;
        StartCoroutine(SpawnBuff(shieldActiveTime, Color.blue));
        while (buffActive == true)
        {
            yield return null;
        }
        shieldActive = false;
    }
    IEnumerator Heavy()
    {
        yield return null;
        heavyActive = true;
        StartCoroutine(SpawnBuff(heavyActiveTime, new Color(1, 0.75f, 0)));
        boxScript.dashSpeed *= heavyMult;
        boxScript.gravityMult *= speedMult;
        boxScript.horizMaxSpeed *= heavyMult;
        boxScript.initialAirAccel *= heavyMult;
        boxScript.airAccel *= heavyMult;
        while (buffActive == true)
        {
            yield return null;
        }
        boxScript.dashSpeed = initialDashSpeed;
        boxScript.gravityMult = initialGravityMult;
        boxScript.horizMaxSpeed = initialHMaxSpeed;
        boxScript.initialAirAccel = initialAirAccel;
        boxScript.airAccel = initialAirAccel;
        heavyActive = false;
    }
    IEnumerator Spikes()
    {
        yield return null;
        spikesActive = true;
        buffActive = true;
        newSpikes = Instantiate(spikes);
        Rigidbody2D spikeRB = newSpikes.GetComponent<Rigidbody2D>();
        bool flicker = false;
        while (buffActive == true && buffTimer <= spikesActiveTime)
        {
            newSpikes.transform.position = boxTransform.position;
            float rotationVel = 500;
            if (buffTimer >= spikesActiveTime - flickerTime && flicker == false)
            {
                StartCoroutine(Flicker(newSpikes, flickerTime, spikesActiveTime));
                flicker = true;
            }
            if (Mathf.Abs(boxRB.angularVelocity) > 1000)
            {
                spikeRB.angularVelocity = boxRB.angularVelocity;
            }
            if (Box.canWallJump && Box.ceilingCling)
            {
                spikeRB.angularVelocity = 0;
            }
            else if (Box.canWallJump)
            {
                if ((inputs.leftStick.y > 0.5f && Box.wallJumpDirection == -1) || (inputs.leftStick.y < -0.5f && Box.wallJumpDirection == 1))
                {
                    spikeRB.angularVelocity = -rotationVel * Mathf.Abs(inputs.leftStick.y);
                }
                else if ((inputs.leftStick.y < -0.5f && Box.wallJumpDirection == -1) || (inputs.leftStick.y > 0.5f && Box.wallJumpDirection == 1))
                {
                    spikeRB.angularVelocity = rotationVel * Mathf.Abs(inputs.leftStick.y);
                }
                else
                {
                    spikeRB.angularVelocity = 0;
                }
            }
            else if (Box.ceilingCling)
            {
                if ((inputs.leftStick.x > 0.2f || inputs.leftStick.x < -0.2f))
                {
                    spikeRB.angularVelocity = rotationVel * inputs.leftStick.x;
                }
                else
                {
                    spikeRB.angularVelocity = 0;
                }
            }
            else if (Box.isGrounded)
            {
                spikeRB.angularVelocity = -(BoxVelocity.velocitiesX[0] + BoxVelocity.velocitiesX[2] + BoxVelocity.velocitiesX[3]) * 40;
            }
            else
            {
                spikeRB.angularVelocity = Mathf.MoveTowards(spikeRB.angularVelocity, 0, 500 * Time.deltaTime);
            }
            buffTimer += Time.deltaTime;
            yield return null;
        }
        buffTimer = 0;
        Destroy(newSpikes);
        buffActive = false;
        spikesActive = false;
    }
    IEnumerator Star()
    {
        yield return null;
        starActive = true;
        StartCoroutine(SpawnBuff(starActiveTime, Color.black));
        boxScript.dashSpeed *= starMult;
        boxScript.gravityMult *= starMult;
        boxScript.horizMaxSpeed *= starMult;
        boxScript.jumpSpeed *= starMult;
        boxScript.djumpSpeed *= starMult;
        boxScript.initialAirAccel *= starMult;
        boxScript.airAccel *= starMult;

        boxScript.dashCooldown *= starCDMult;
        boxScript.pulseCooldown *= starCDMult;
        boxScript.teleportCooldown *= starCDMult;
        while (buffActive == true)
        {
            yield return null;
        }
        boxScript.dashSpeed = initialDashSpeed;
        boxScript.gravityMult = initialGravityMult;
        boxScript.horizMaxSpeed = initialHMaxSpeed;
        boxScript.jumpSpeed = initialJumpSpeed;
        boxScript.djumpSpeed = initialDJumpSpeed;
        boxScript.initialAirAccel = initialAirAccel;
        boxScript.airAccel = initialAirAccel;

        boxScript.dashCooldown = initialDashCD;
        boxScript.pulseCooldown = initialPulseCD;
        boxScript.teleportCooldown = initialTeleCD;
        starDeactivated = true;
        yield return null;
        starDeactivated = false;
        starActive = false;
    }

    IEnumerator Jump()
    {
        yield return null;
        jumpActive = true;
        StartCoroutine(SpawnBuff(jumpActiveTime, new Color(0, 1, 0.85f)));
        while (buffActive == true)
        {
            yield return null;
        }
        jumpActive = false;
    }

    IEnumerator SpawnBuff(float activeTime, Color color)
    {
        buffActive = true;
        newBuff = Instantiate(buff);
        if (color == Color.black)
        {
            StartCoroutine(RainbowBuff(newBuff));
        }
        else
        {
            newBuff.GetComponent<Renderer>().material.color = color;
            newBuff.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
        }
        bool flicker = false;
        while (buffTimer <= activeTime && buffActive == true)
        {
            newBuff.transform.rotation = boxTransform.rotation;
            newBuff.transform.position = boxTransform.position;
            newBuff.transform.localScale = new Vector2(boxTransform.localScale.x + 0.3f, boxTransform.localScale.y + 0.3f);
            if (buffTimer >= activeTime - flickerTime && flicker == false)
            {
                StartCoroutine(Flicker(newBuff, flickerTime, activeTime));
                flicker = true;
            }
            buffTimer += Time.deltaTime;
            yield return null;
        }
        buffTimer = 0;
        Destroy(newBuff);
        buffActive = false;
    }

    IEnumerator Flicker(GameObject item, float flickerTime, float activeTime)
    {
        Renderer renderer = item.GetComponent<Renderer>();
        GameObject child = item.transform.GetChild(0).gameObject;
        Renderer childRenderer = child.GetComponent<Renderer>();
        float timer = 0;
        while (buffTimer >= activeTime - 4 && renderer != null && buffActive == true)
        {
            float onTime = 0.25f;
            float offTime = 0.1f;
            if (timer >= flickerTime * 2 / 3)
            {
                onTime /= 3;
                offTime /= 3;
            }
            renderer.enabled = false;
            childRenderer.enabled = false;
            if (renderer != null)
            {
                yield return new WaitForSeconds(offTime);
            }
            if (renderer != null)
            {
                timer += offTime;
                renderer.enabled = true;
                childRenderer.enabled = true;
            }
            if (renderer != null)
            {
                yield return new WaitForSeconds(onTime);
                timer += onTime;
            }
        }
    }

    IEnumerator RainbowBuff(GameObject item)
    {
        Color color = new Color(1, 0.5f, 0.5f);
        float minValue = 0.3f;
        float colorChangeSpeed = 10;
        while (color.g < 1 && item != null)
        {
            color.g += colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.g = 1;
        while (color.r > minValue && item != null)
        {
            color.r -= colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.r = minValue;
        while (color.b < 1 && item != null)
        {
            color.b += colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.b = 1;
        while (color.g > minValue && item != null)
        {
            color.g -= colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.g = minValue;
        while (color.r < 1 && item != null)
        {
            color.r += colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.r = 1;
        while (color.b > minValue && item != null)
        {
            color.b -= colorChangeSpeed * Time.deltaTime;
            item.GetComponent<Renderer>().material.color = color;
            item.transform.GetChild(0).GetComponent<Renderer>().material.color = color;
            yield return null;
        }
        color.b = minValue;
        if (item != null)
        {
            StartCoroutine(RainbowBuff(item));
        }
    }
}
