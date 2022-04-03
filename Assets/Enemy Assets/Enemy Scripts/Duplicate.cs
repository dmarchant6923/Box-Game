using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicate : MonoBehaviour
{
    Rigidbody2D box;
    [System.NonSerialized] float secondsBehind = 2f;
    List<Vector2> boxPositionArray = new List<Vector2>();
    List<float> boxRotationArray = new List<float>();

    public float damage = 30;
    bool damagedBox = false;
    bool willDamageBox = false;

    void Start()
    {
        box = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        int listSize = Mathf.CeilToInt(secondsBehind * 50);
        for (int i = 0; i < listSize; i++)
        {
            boxPositionArray.Add(box.position);
            boxRotationArray.Add(box.rotation);
        }
        Debug.Log(boxPositionArray.Count);

        willDamageBox = false;
        StartCoroutine(InitialDelay());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (damagedBox == false)
        {
            boxPositionArray.Add(box.position);
            boxPositionArray.RemoveAt(0);
            transform.position = boxPositionArray[0];

            boxRotationArray.Add(box.rotation);
            boxRotationArray.RemoveAt(0);
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, boxRotationArray[0]);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box") && Box.isInvulnerable == false && willDamageBox)
        {
            StartCoroutine(DamageBox());
            Box.damageTaken = damage;
            Box.boxDamageDirection = Box.boxDamageDirection = new Vector2(Mathf.Sign(box.position.x - transform.position.x), 1).normalized;
            Box.activateDamage = true;
        }
    }

    IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(secondsBehind);
        willDamageBox = true;
    }

    IEnumerator DamageBox()
    {
        damagedBox = true;
        yield return null;
        while (Box.boxHitstopActive)
        {
            Debug.Log("you are here");
            yield return null;
        }
        damagedBox = false;
    }
}
