using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSwitch : MonoBehaviour
{
    Switch switchScript;
    [HideInInspector] public bool active = false;
    public bool hitToDeactivate = false;

    public bool timedDeactivate = false;
    public float timeToDeactivate = 5f;
    float hitTimer = 0;

    Transform diamond;

    bool invulnerable = false;

    [HideInInspector] public bool activateShock = false;
    [HideInInspector] public bool canBeShocked = true;
    float shockCooldown = 4f;

    void Start()
    {
        diamond = transform.GetChild(0);
        switchScript = GetComponent<Switch>();
        active = false;
        if (switchScript.startActive)
        {
            Activate();
        }
        invulnerable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Box.boxHitboxActive && Box.attackRayCast.Length > 0)
        {
            foreach (RaycastHit2D item in Box.attackRayCast)
            {
                if (item.collider != null && item.transform.root != null && item.transform.root == transform && invulnerable == false)
                {
                    Hit();
                    Box.activateHitstop = true;
                    break;
                }
            }
        }

        if (activateShock)
        {
            if (canBeShocked)
            {
                StartCoroutine(Shock());
                canBeShocked = false;
            }
            activateShock = false;
        }
    }

    public void Hit()
    {
        if (invulnerable == false)
        {
            invulnerable = true;
            if (active == false)
            {
                Activate();
                if (timedDeactivate)
                {
                    StartCoroutine(TimedDeactivate());
                }
            }
            else if (hitToDeactivate)
            {
                Deactivate();
            }
            else if (timedDeactivate)
            {
                hitTimer = 0;
            }

            StartCoroutine(Invulnerability());
        }
    }

    void Activate()
    {
        if (active == false)
        {
            Color color = diamond.GetComponent<SpriteRenderer>().color;
            diamond.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g / 3, color.b / 2);
            transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(color.r, color.g / 3, color.b / 2, 0.1f);
            foreach (SpriteRenderer child in diamond.GetComponentsInChildren<SpriteRenderer>())
            {
                child.color = new Color(child.color.r, child.color.g / 1.5f, child.color.b);
            }
            active = true;
            switchScript.Activate();
        }
    }

    void Deactivate()
    {
        if (active)
        {
            Color color = diamond.GetComponent<SpriteRenderer>().color;
            diamond.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g * 3, color.b * 2);
            transform.GetChild(1).GetComponent<SpriteRenderer>().color = new Color(color.r, color.g * 3, color.b * 2, 0.1f);
            foreach (SpriteRenderer child in diamond.GetComponentsInChildren<SpriteRenderer>())
            {
                child.color = new Color(child.color.r, child.color.g * 1.5f, child.color.b);
            }
            active = false;
            switchScript.Deactivate();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyBehavior_Grounded>() != null && collision.GetComponent<EnemyBehavior_Grounded>().willDamageEnemies && invulnerable == false)
        {
            Hit();
            StartCoroutine(collision.GetComponent<EnemyBehavior_Grounded>().EnemyHitstop());
        }
        if (collision.GetComponent<Box>() != null && Box.boxHitboxActive && invulnerable == false)
        {
            Hit();
            StartCoroutine(collision.GetComponent<Box>().HitStop());
        }
    }

    IEnumerator Invulnerability()
    {
        float window = 0.5f;
        float timer = 0;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        invulnerable = false;
    }
    IEnumerator TimedDeactivate()
    {
        while (hitTimer < timeToDeactivate && active)
        {
            hitTimer += Time.deltaTime;
            yield return null;
        }
        Deactivate();
        hitTimer = 0;
    }

    IEnumerator Shock()
    {
        float timer = 0;
        Hit();
        while (timer < shockCooldown)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        canBeShocked = true;
    }
}
