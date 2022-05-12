using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    GameObject[] blocks;

    public bool triggerDestroy = false;
    bool destroyCRActive = false;

    void Start()
    {
        blocks = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            blocks[i] = transform.GetChild(i).gameObject;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (triggerDestroy && destroyCRActive == false)
        {
            StartCoroutine(DestroyBlock());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Box>() != null && Box.dashActive)
        {
            StartCoroutine(DestroyBlock());
        }
    }

    IEnumerator DestroyBlock()
    {
        destroyCRActive = true;
        float window = 0.4f;
        float timer = 0;
        while (Box.wallBounceActive == false && timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (Box.wallBounceActive)
        {
            foreach (GameObject block in blocks)
            {
                block.transform.parent = null;
                block.GetComponent<Rigidbody2D>().isKinematic = false;
                block.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(14f, 18f));
                block.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(600f, 1000f) * (Random.Range(0, 2) * 2 - 1);
                block.GetComponent<SpriteRenderer>().sortingLayerName = "Above All";
            }
            transform.GetComponent<SpriteRenderer>().enabled = false;
            transform.GetComponent<BoxCollider2D>().enabled = false;

            yield return new WaitForSeconds(2);
            foreach (GameObject block in blocks)
            {
                Destroy(block);
            }
            Destroy(gameObject);
        }
    }
}
