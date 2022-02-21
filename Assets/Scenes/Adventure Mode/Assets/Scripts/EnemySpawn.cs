using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject spawn;
    public GameObject prefab;
    public float timer = 1.5f;

    bool spawnCRActive = false;

    void Start()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (spawn == null && spawnCRActive == false)
        {
            StartCoroutine(Respawn());
            Debug.Log("you are here");
        }
    }

    IEnumerator Respawn()
    {

        spawnCRActive = true;
        float time = 0;
        while (time < timer)
        {
            time += Time.deltaTime;
            yield return null;
        }
        spawn = Instantiate(prefab, transform.position, Quaternion.identity);
        spawnCRActive = false;
    }
}
