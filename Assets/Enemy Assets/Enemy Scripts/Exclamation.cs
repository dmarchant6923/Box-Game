using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exclamation : MonoBehaviour
{
    [System.NonSerialized] public GameObject enemy;
    [System.NonSerialized] public float distanceUp;
    float exclamationTime = 0.7f;
    float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    { 
        timer += Time.deltaTime;
        if (timer > exclamationTime || enemy == null || enemy.GetComponent<EnemyManager>().enemyWasKilled)
        {
            Destroy(gameObject);
        }

        if (enemy != null)
        {
            transform.position = enemy.GetComponent<EnemyManager>().enemyRB.position + Vector2.up * distanceUp;
        }
    }
}
