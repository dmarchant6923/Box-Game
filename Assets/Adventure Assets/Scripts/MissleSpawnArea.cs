using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissleSpawnArea : MonoBehaviour
{
    Rigidbody2D boxRB;

    public GameObject missile;
    GameObject newMissile;
    List<GameObject> activeMissiles = new List<GameObject>();

    public bool movingSpawnLocation = true;
    Transform spawnLocation;
    bool timerActive = false;
    float timer;
    float window;
    public float delayBetweenMissiles = 2.5f;

    public float speed = 10;
    public Vector2 direction = Vector2.left;
    public float damage = 40;
    public float explosionRadius = 5;

    public bool debugEnabled = false;

    private void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        if (debugEnabled == false)
        {
            foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.enabled = false;
            }
        }

        spawnLocation = transform.GetChild(0);

        window = delayBetweenMissiles * 0.75f + Random.Range(0, delayBetweenMissiles * 0.5f);
        timer = window;
    }

    private void FixedUpdate()
    {
        if (movingSpawnLocation && timerActive)
        {
            if (Mathf.Abs(direction.x) == 1)
            {
                spawnLocation.position = new Vector2(boxRB.position.x - direction.x * 30, spawnLocation.position.y);
            }
            else if (Mathf.Abs(direction.y) == 1)
            {
                spawnLocation.position = new Vector2(spawnLocation.position.x, boxRB.position.y - direction.y * 30);
            }
        }

        if (timerActive)
        {
            timer += Time.fixedDeltaTime;
            if (timer > window)
            {
                Vector2 position = spawnLocation.position + new Vector3(Random.Range(-0.5f, 0.5f) * spawnLocation.lossyScale.x, Random.Range(-0.5f, 0.5f) * spawnLocation.lossyScale.y);
                SpawnMissile(position);

                timer -= window;
                window = delayBetweenMissiles * 0.75f + Random.Range(0, delayBetweenMissiles * 0.5f);
            }
        }

        foreach(GameObject missile in activeMissiles)
        {
            if (missile == null)
            {
                activeMissiles.Remove(missile);
                break;
            }
        }
    }

    void SpawnMissile(Vector2 position)
    {
        newMissile = Instantiate(missile, position, Quaternion.identity);
        newMissile.transform.localScale *= 2f;
        newMissile.GetComponent<Missile>().speed = speed;
        newMissile.GetComponent<Missile>().direction = direction;
        newMissile.GetComponent<Missile>().damage = damage;
        newMissile.GetComponent<Missile>().explosionRadius = explosionRadius;
        activeMissiles.Add(newMissile);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Box>() != null)
        {
            timerActive = true;

            float distBetweenMissiles = delayBetweenMissiles * speed;
            float distance = (boxRB.transform.position - spawnLocation.position).magnitude - distBetweenMissiles;
            int i = 1;
            while (distance > 40)
            {
                Vector2 position = new Vector2(spawnLocation.position.x, spawnLocation.position.y) + (direction * distBetweenMissiles * i) +
                    Vector2.Perpendicular(direction) * Random.Range(-0.5f, 0.5f) * spawnLocation.lossyScale.y;
                distance -= distBetweenMissiles;
                i++;
                SpawnMissile(position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<Box>() != null)
        {
            timerActive = false;
            timer = 0;
            foreach(GameObject missile in activeMissiles)
            {
                if ((boxRB.transform.position - missile.transform.position).magnitude > 25)
                {
                    Destroy(missile);
                }
            }
        }
    }
}
