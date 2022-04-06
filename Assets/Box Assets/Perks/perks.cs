using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class perks : MonoBehaviour
{
    public bool heart;
    public float amountHealed = 25;

    public bool speed;

    public bool shield;

    public bool heavy;

    public bool spikes;

    public bool star;

    public bool jump;
    public bool unlimitedJumps = true;

    public bool willDespawn = true;
    float activeTime = 12;
    void Start()
    {
        if (willDespawn)
        {
            StartCoroutine(Despawn(activeTime));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            if (heart)
            {
                Box.boxHealth += amountHealed;
                Destroy(gameObject);
            }
            if (speed)
            {
                BoxPerks.activateSpeed = true;
                Destroy(gameObject);
            }
            if (shield)
            {
                BoxPerks.activateShield = true;
                Destroy(gameObject);
            }
            if (heavy)
            {
                BoxPerks.activateHeavy = true;
                Destroy(gameObject);
            }
            if (spikes)
            {
                BoxPerks.activateSpikes = true;
                Destroy(gameObject);
            }
            if (star)
            {
                BoxPerks.activateStar = true;
                Destroy(gameObject);
            }
            if (jump)
            {
                BoxPerks.activateJump = true;
                BoxPerks.unlimitedJumps = unlimitedJumps;
                Destroy(gameObject);
            }
        }
    }

    IEnumerator Despawn(float activeTime)
    {
        yield return new WaitForSeconds(activeTime / 2);
        StartCoroutine(Flicker(gameObject, activeTime / 2));
        yield return new WaitForSeconds(activeTime / 2);
        Destroy(gameObject);
    }
    IEnumerator Flicker(GameObject item, float flickerTime)
    {
        Renderer renderer = item.GetComponent<Renderer>();
        GameObject child = item.transform.GetChild(0).gameObject;
        Renderer childRenderer = child.GetComponent<Renderer>();
        float timer = 0;
        while (true)
        {
            float onTime = 0.3f;
            float offTime = 0.2f;
            if (timer >= flickerTime * 2 / 3)
            {
                onTime /= 3;
                offTime /= 3;
            }
            renderer.enabled = false;
            childRenderer.enabled = false;
            yield return new WaitForSeconds(offTime);
            timer += offTime;
            renderer.enabled = true;
            childRenderer.enabled = true;
            yield return new WaitForSeconds(onTime);
            timer += onTime;
        }
    }
}
