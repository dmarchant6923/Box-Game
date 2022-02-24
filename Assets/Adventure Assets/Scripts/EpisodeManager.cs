using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeManager : MonoBehaviour
{
    Rigidbody2D boxRB;
    Box boxScript;
    public int initialHealth = 100;

    public GameObject UIBackground;
    public CameraFollowBox cameraScript;

    public int season = 1;
    public int episode = 1;
    public EpisodeStats episodeStats = new EpisodeStats();

    public GameObject[] inGameStickers = new GameObject[4]; //2 extra stickers, one for completion time and another for enemies killed
    public bool[] inGameStickersFound = new bool[6];
    public GameObject sticker;
    public GameObject duplicateSticker;

    public GameObject checkpoint;

    EnemyManager[] enemies;
    int enemiesToKill = 0;
    public int enemiesKilled = 0;
    bool allEnemiesKilled = false;

    [HideInInspector] public bool episodeComplete = false;
    bool finishedCRActive = false;
    public float timeToBeat = 10;

    void Start()    
    {
        UIManager.initialHealth = initialHealth;
        Box.boxHealth = initialHealth;

        if (season <= 1)
        {
            Box.dashUnlocked = false;
        }
        if (season <= 2)
        {
            Box.pulseUnlocked = false;
        }
        if (season <= 3)
        {
            Box.teleportUnlocked = false;
        }

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxScript = boxRB.GetComponent<Box>();
        episodeStats = DataManager.episodeStats[season - 1, episode - 1];
        inGameStickersFound = episodeStats.stickersFound;
        if (episodeStats.checkpoint)
        {
            checkpoint.GetComponent<Checkpoint>().checkpointActive = true;
            boxRB.position = checkpoint.transform.position + Vector3.left * 5;
        }

        enemies = FindObjectsOfType<EnemyManager>();
        foreach (EnemyManager enemy in enemies)
        {
            if (enemy.respawn == false)
            {
                enemiesToKill++;
            }
        }

        for (int i = 0; i < inGameStickers.Length; i++)
        {
            if (episodeStats.stickersFound[i])
            {
                Vector2 position = inGameStickers[i].transform.position;
                Destroy(inGameStickers[i]);
                GameObject newSticker = Instantiate(duplicateSticker, position, Quaternion.identity);
                inGameStickers[i] = newSticker;
            }
        }

        StartCoroutine(EpisodeStart());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CheckpointDeactivated();
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            cameraScript.disable = !cameraScript.disable;
        }

        if (episodeComplete && finishedCRActive == false)
        {
            finishedCRActive = true;
            StartCoroutine(EpisodeFinished());
        }

        if (allEnemiesKilled == false && episodeStats.stickersFound[4] == false)
        {
            if (enemiesKilled == enemiesToKill)
            {
                allEnemiesKilled = true;
                inGameStickersFound[4] = true;
                GameObject newSticker = Instantiate(sticker);
                newSticker.GetComponent<Sticker>().found = true;
            }
        }

        Debug.Log(enemiesKilled + " " + enemiesToKill);




    }

    public void StickerFound(GameObject sticker)
    {
        for (int i = 0; i < inGameStickers.Length; i++)
        {
            if (sticker == inGameStickers[i])
            {
                inGameStickersFound[i] = true;
                break;
            }
        }
    }

    public void CheckpointActivated()
    {
        episodeStats.checkpoint = true;
        SaveManager.SaveEpisode(season, episode, episodeStats);
    }
    public void CheckpointDeactivated()
    {
        checkpoint.GetComponent<Checkpoint>().checkpointActive = false;
        checkpoint.GetComponent<Checkpoint>().DeactivateCheckpoint();
        episodeStats.checkpoint = false;
        SaveManager.SaveEpisode(season, episode, episodeStats);
    }

    IEnumerator EpisodeStart()
    {
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(0.5f));
        yield return null;
        foreach (BattleSpawner spawner in FindObjectsOfType<BattleSpawner>())
        {
            enemiesToKill += spawner.numEnemies;
        }
        yield return new WaitForSeconds(0.5f);
        float waitTime =1.5f;
        float timer = 0;
        UIManager.stopClock = false;
        while (timer < waitTime)
        {
            Color color = UIBackground.GetComponent<Image>().color;
            color.a -= 1 / waitTime * Time.deltaTime;
            UIBackground.GetComponent<Image>().color = color;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator EpisodeFinished()
    {
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(1000));

        episodeStats.completed = true;
        episodeStats.checkpoint = false;
        episodeStats.stickersFound = inGameStickersFound;
        if (UIManager.rawTime <= episodeStats.fastestTime || episodeStats.fastestTime == 0)
        {
            episodeStats.fastestTime = UIManager.rawTime;
        }
        if (UIManager.rawTime <= timeToBeat && episodeStats.stickersFound[5] == false)
        {
            episodeStats.stickersFound[5] = true;
            GameObject newSticker = Instantiate(sticker);
            newSticker.GetComponent<Sticker>().found = true;
            newSticker.GetComponent<Sticker>().timeFound = true;
        }
        allEnemiesKilled = true;
        foreach (EnemyManager enemy in enemies)
        {
            if (enemy != null && enemy.enemyWasKilled == false)
            {
                allEnemiesKilled = false;
                Debug.Log("you are here");
                break;
            }
        }
        if (allEnemiesKilled)
        {
            episodeStats.stickersFound[4] = true;
        }

        SaveManager.SaveEpisode(season, episode, episodeStats);
        DataManager.exitFromAdventureMode = true;
        DataManager.exitFromAdventureModeEpisode = new Vector2(season, episode);
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Main Menu");
    }
}
