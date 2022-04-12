using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSpawner : MonoBehaviour
{
    List<GameObject> enemies = new List<GameObject> { };
    List<Vector2> enemyPositions = new List<Vector2> { };
    List<GameObject> spawnedEnemies = new List<GameObject> { };

    public bool triggerOnce = true;
    bool triggered = false;
    public int numEnemies;

    Switch switchScript;

    public bool debugEnabled = false;

    void Start()
    {
        switchScript = GetComponent<Switch>();
        if (debugEnabled == false)
        {
            GetComponent<SpriteRenderer>().enabled = false;
        }
        foreach (BattleEnemy enemy in GetComponentsInChildren<BattleEnemy>())
        {
            if (enemy.enemy != null)
            {
                numEnemies++;
                enemies.Add(enemy.enemy);
                enemyPositions.Add(enemy.transform.position);
            }
            if (debugEnabled == false)
            {
                enemy.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnedEnemies.Count > 0)
        {
            for (int i = 0; i < spawnedEnemies.Count; i++)
            {
                if (spawnedEnemies[i] == null || spawnedEnemies[i].GetComponent<EnemyManager>().enemyWasKilled == true)
                {
                    spawnedEnemies.RemoveAt(i);
                    if (spawnedEnemies.Count == 0)
                    {
                        if (switchScript.active == false)
                        {
                            switchScript.Activate();
                        }
                        else
                        {
                            switchScript.Deactivate();
                        }
                    }
                }
            }
        }
    }

    public void Trigger()
    {
        if ((triggerOnce && triggered == false) || triggerOnce == false)
        {
            triggered = true;
            StartCoroutine(SpawnEnemies());
        }
    }

    IEnumerator SpawnEnemies()
    {
        float window = 2f;
        float timer = 0;
        while (timer < window && triggered == true)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (triggered == true)
        {
            for (int i = 0; i < enemyPositions.Count; i++)
            {
                GameObject newEnemy = Instantiate(enemies[i], enemyPositions[i], Quaternion.identity);
                spawnedEnemies.Add(newEnemy);
            }
        }
    }
}
