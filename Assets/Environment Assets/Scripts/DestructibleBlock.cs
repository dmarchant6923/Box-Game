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

    Rigidbody2D boxRB;

    public GameObject spawnObject;
    GameObject newSpawn;

    public bool spinAttackCanDestroy = false;
    public bool dashAttackCanDestroy = true;
    public bool timedDestroyOnGrounded = false;
    public float timedDestroyWindow = 3f;
    float timedDestroyTimer = 0;

    public bool respawn = false;
    public float respawnTimer = 5f;
    Color initialColor;
    Color depletedColor;

    public bool triggerDestroy = false;
    bool destroyCRActive = false;

    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        blockSize = new Vector2(Mathf.Max(0.1f, blockSize.x), Mathf.Max(0.1f, blockSize.y));

        int gridX = Mathf.Max(1, (int)Mathf.Floor(transform.localScale.x / blockSize.x));
        float sizeX = transform.localScale.x / gridX;

        int gridY = Mathf.Max(1, (int)Mathf.Floor(transform.localScale.y / blockSize.y));
        float sizeY = transform.localScale.y / gridY;

        blockGrid = new Vector2(gridX, gridY);
        blockSize = new Vector2(sizeX, sizeY);

        initialColor = GetComponent<SpriteRenderer>().color;
        depletedColor = new Color(0.6f, 0.6f, 0.6f);

        if (spawnObject != null && spawnObject.GetComponent<EnemyManager>() != null && FindObjectOfType<EpisodeManager>() != null)
        {
            FindObjectOfType<EpisodeManager>().enemiesToKill++;
        }
    }
    void FixedUpdate()
    {
        if (triggerDestroy && destroyCRActive == false)
        {
            StartCoroutine(DestroyBlock());
        }

        if (timedDestroyTimer > timedDestroyWindow && destroyCRActive == false)
        {
            StartCoroutine(DestroyBlock());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Box>() != null)
        {
            if (Box.dashActive && dashAttackCanDestroy)
            {
                StartCoroutine(DashDestroyBlock());
            }
            else if (Box.spinAttackActive && spinAttackCanDestroy)
            {
                StartCoroutine(PushBack(collision.GetContact(0).normal));
                StartCoroutine(DestroyBlock());
            }
        }

        if (collision.collider.GetComponent<Box>() != null && boxRB.position.y > transform.position.y + transform.localScale.y / 2)
        {
            if (timedDestroyTimer > 0 && timedDestroyTimer < timedDestroyWindow - 0.5f)
            {
                timedDestroyTimer = Mathf.Min(timedDestroyTimer + 1.5f, timedDestroyWindow - 0.4f);
            }
            else if (timedDestroyTimer > timedDestroyWindow - 0.5f)
            {
                timedDestroyTimer = timedDestroyWindow;
                GetComponent<SpriteRenderer>().color = depletedColor;
            }
        }
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Box>() != null && Box.isGrounded)
        {
            timedDestroyTimer += Time.deltaTime;
            Color color = GetComponent<SpriteRenderer>().color;
            Vector3 initialColorVector = new Vector3(initialColor.r, initialColor.g, initialColor.b);
            Vector3 targetVector = new Vector3(depletedColor.r, depletedColor.g, depletedColor.b);
            Vector3 directionVector = new Vector3(targetVector.x - initialColorVector.x, targetVector.y - initialColorVector.y, targetVector.z - initialColorVector.z);
            Vector3 colorVector = initialColorVector + (directionVector * (timedDestroyTimer / timedDestroyWindow));
            Debug.Log(timedDestroyTimer / timedDestroyWindow);
            color = new Color(colorVector.x, colorVector.y, colorVector.z);
            GetComponent<SpriteRenderer>().color = color;
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
    IEnumerator PushBack(Vector2 collision)
    {
        yield return new WaitForFixedUpdate();
        if (Mathf.Abs(collision.x) > 0.8f)
        {
            BoxVelocity.velocitiesX[0] = -Mathf.Sign(collision.x) * Box.horizMaxSpeed;
        }
        if (collision.y < -0.8f)
        {
            boxRB.velocity = new Vector2(boxRB.velocity.x, 15);
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
            //block.transform.parent = null;
            block.GetComponent<Rigidbody2D>().isKinematic = false;
            block.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5f, 5f), Random.Range(5f, 25f));
            block.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(300f, 1000f) * (Random.Range(0, 2) * 2 - 1);
            block.GetComponent<Rigidbody2D>().gravityScale = 7;
            block.GetComponent<SpriteRenderer>().sortingLayerName = "Above All";
        }

        if (spawnObject != null)
        {
            newSpawn = Instantiate(spawnObject, GetComponent<Rigidbody2D>().position, Quaternion.identity);
            if (newSpawn.GetComponent<EnemyManager>() != null)
            {
                newSpawn.GetComponent<EnemyManager>().enemyIsInvulnerable = true;
                //newSpawn.GetComponent<EnemyManager>().enemyHealth++;
                //newSpawn.GetComponent<EnemyManager>().enemyWasDamaged = true;
                if (newSpawn.GetComponent<Rigidbody2D>().gravityScale > 2)
                {
                    newSpawn.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-4f, 4f), 3 * newSpawn.GetComponent<Rigidbody2D>().gravityScale);
                }
            }
        }

        yield return new WaitForSeconds(1.2f);
        foreach (GameObject block in blocks)
        {
            Destroy(block);
        }
        blocks = new List<GameObject>();

        if (respawn == false)
        {
            Destroy(gameObject);
        }
        else
        {
            yield return new WaitForSeconds(respawnTimer);
            destroyCRActive = false;
            transform.GetComponent<SpriteRenderer>().enabled = true;
            transform.GetComponent<BoxCollider2D>().enabled = true;
            GetComponent<SpriteRenderer>().color = initialColor;
            timedDestroyTimer = 0;
        }
    }
}
