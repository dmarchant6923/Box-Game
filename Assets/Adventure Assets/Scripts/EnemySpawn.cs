using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject spawn;
    public GameObject prefab;
    Transform killzone;

    public bool killzoneActive = false;
    float killzoneTimer = 0;
    public float killzoneTime = 1f;

    public float respawnTime = 1.5f;

    bool spawnCRActive = false;

    public bool debugEnabled = false;

    void Start()
    {
        killzone = transform.GetChild(0);

        if (debugEnabled == false)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        if (killzoneActive)
        {
            if (debugEnabled == false)
            {
                killzone.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
        else
        {
            killzone.GetComponent<SpriteRenderer>().enabled = false;
            killzone.localScale = Vector2.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawn == null && spawnCRActive == false)
        {
            StartCoroutine(Respawn());
        }

        if (killzoneTimer > killzoneTime)
        {
            spawn.GetComponent<EnemyManager>().instantKill = true;
        }
    }

    public void Trigger()
    {
        killzoneActive = !killzoneActive;
        Debug.Log(killzoneActive);
    }

    IEnumerator Respawn()
    {
        spawnCRActive = true;
        float timer = 0;
        while (timer < respawnTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        spawn = Instantiate(prefab, transform.position, Quaternion.identity);
        spawnCRActive = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (killzoneActive && collision.gameObject == spawn && collision.isTrigger == false)
        {
            killzoneTimer += Time.deltaTime;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (killzoneActive && collision.gameObject == spawn)
        {
            killzoneTimer = 0;
        }
    }
}
