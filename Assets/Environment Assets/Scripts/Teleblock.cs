using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleblock : MonoBehaviour
{
    public GameObject x;
    GameObject newX;
    public bool xActive = false;
    public float xHeight = 0;

    Transform aura;
    Transform inside;
    float outsideOffset = 0.25f;
    float insideOffset = 0.15f;

    void Start()
    {
        aura = transform.GetChild(0);
        aura.parent = null;
        aura.localScale = transform.localScale + Vector3.one * outsideOffset;
        aura.parent = transform;

        inside = transform.GetChild(0);
        inside.parent = null;
        inside.localScale = transform.localScale - Vector3.one * insideOffset;
        inside.parent = transform;

        StartCoroutine(AuraGlow());
    }

    private void Update()
    {
        if (xActive && newX == null)
        {
            xActive = true;
            newX = Instantiate(x, new Vector2(transform.position.x, xHeight), Quaternion.identity);
        }

        if (xActive == false && newX != null)
        {
            Destroy(newX);
        }

        if (newX != null)
        {
            newX.transform.position = new Vector2(transform.position.x, xHeight);
        }

        if (Box.teleportActive == false)
        {
            xActive = false;
        }

        xActive = false;
    }
    IEnumerator AuraGlow()
    {
        SpriteRenderer sprite = aura.GetComponent<SpriteRenderer>();
        float changeSpeed = 0.4f;
        while (true)
        {
            while (sprite.color.a < 0.5f)
            {
                Color color = sprite.color;
                color.a += changeSpeed * Time.deltaTime;
                sprite.color = color;
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            while (sprite.color.a > 0.05f)
            {
                Color color = sprite.color;
                color.a -= changeSpeed * Time.deltaTime;
                sprite.color = color;
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
}
