using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleBlock : MonoBehaviour
{
    List<GameObject> blocks = new List<GameObject>();
    public GameObject block;
    GameObject newBlock;
    Vector2 blockGrid;
    public Vector2 blockSize = new Vector2(0.5f, 0.5f);

    public bool triggerDestroy = false;
    bool destroyCRActive = false;

    void Start()
    {
        blockSize = new Vector2(Mathf.Max(0.1f, blockSize.x), Mathf.Max(0.1f, blockSize.y));

        int gridX = Mathf.Max(1, (int)Mathf.Floor(transform.localScale.x / blockSize.x));
        float sizeX = transform.localScale.x / gridX;

        int gridY = Mathf.Max(1, (int)Mathf.Floor(transform.localScale.y / blockSize.y));
        float sizeY = transform.localScale.y / gridY;

        blockGrid = new Vector2(gridX, gridY);
        blockSize = new Vector2(sizeX, sizeY);
    }
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
            StartCoroutine(DashDestroyBlock());
        }
    }

    IEnumerator DashDestroyBlock()
    {
        float window = 0.1f;
        float timer = 0;
        while (Box.wallBounceActive == false && timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (Box.wallBounceActive)
        {
            StartCoroutine(DestroyBlock());
        }
    }

    IEnumerator DestroyBlock()
    {
        destroyCRActive = true;
        transform.GetComponent<SpriteRenderer>().enabled = false;
        transform.GetComponent<BoxCollider2D>().enabled = false;

        //grid starts from bottom left, goes left to right, then up one row, repeat until top right corner
        for (int y = 0; y < blockGrid.y; y++)
        {
            for (int x = 0; x < blockGrid.x; x++)
            {
                float blockPositionX = transform.position.x - (transform.localScale.x / 2) + (blockSize.x * x) + (blockSize.x / 2);
                float blockPositionY = transform.position.y - (transform.localScale.y / 2) + (blockSize.y * y) + (blockSize.y / 2);
                newBlock = Instantiate(block, new Vector2(blockPositionX, blockPositionY), Quaternion.identity);
                newBlock.GetComponent<MovingObjects>().enabled = false;
                newBlock.GetComponent<Collider2D>().enabled = false;
                newBlock.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
                newBlock.transform.localScale = blockSize;
                blocks.Add(newBlock);
            }
        }

        foreach (GameObject block in blocks)
        {
            block.transform.parent = null;
            block.GetComponent<Rigidbody2D>().isKinematic = false;
            block.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(5f, 25f));
            block.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(300f, 1000f) * (Random.Range(0, 2) * 2 - 1);
            block.GetComponent<Rigidbody2D>().gravityScale = 7;
            block.GetComponent<SpriteRenderer>().sortingLayerName = "Above All";
        }

        yield return new WaitForSeconds(2);
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }
        Destroy(gameObject);
    }
}
