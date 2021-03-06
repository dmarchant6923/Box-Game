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

    public bool dashUnlocked = false;
    public bool teleportUnlocked = false;
    public bool pulseUnlocked = false;

    public GameObject UIBackground;
    AdventureUI adventureUI;

    public int season = 1;
    public int episode = 1;
    [HideInInspector] public EpisodeStats episodeStats = new EpisodeStats();

    public GameObject[] inGameStickers = new GameObject[4]; //2 extra stickers, one for completion time and another for enemies killed
    [HideInInspector] public bool[] inGameStickersFound = new bool[6];
    public GameObject sticker;
    public GameObject duplicateSticker;

    GameObject checkpoint;
    public bool ignoreCheckpoint = false;

    EnemyManager[] enemies;
    [HideInInspector] public int enemiesToKill = 0;
    [HideInInspector] public int enemiesKilled = 0;
    bool allEnemiesKilled = false;

    public GameObject perkSpawner;
    GameObject newPerkSpawner;

    [HideInInspector] public bool episodeComplete = false;
    bool finishedCRActive = false;
    public float timeToBeat = 10;
    [HideInInspector] public bool startAtCheckpoint = false;

    public GameObject pauseInfo;

    bool restartActive = false;

    public bool bossFight = false;

    void Start()    
    {
        adventureUI = FindObjectOfType<AdventureUI>();
        restartActive = false;

        UIManager.initialHealth = initialHealth;
        Box.boxHealth = initialHealth;

        if (dashUnlocked == false)
        {
            Box.dashUnlocked = false;
        }
        if (pulseUnlocked == false)
        {
            Box.pulseUnlocked = false;
        }
        if (teleportUnlocked == false)
        {
            Box.teleportUnlocked = false;
        }

        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        boxScript = boxRB.GetComponent<Box>();
        episodeStats = DataManager.episodeStats[season - 1, episode - 1];
        inGameStickersFound = episodeStats.stickersFound;

        startAtCheckpoint = false;
        checkpoint = GameObject.Find("Checkpoint");
        if (episodeStats.checkpoint && bossFight == false)
        {
            checkpoint.GetComponent<Checkpoint>().checkpointActive = true;
            if (ignoreCheckpoint == false)
            {
                boxRB.position = checkpoint.transform.GetChild(0).position;
            }
            startAtCheckpoint = true;
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
            if (episodeStats.stickersFound[i] && bossFight == false)
            {
                Vector2 position = inGameStickers[i].transform.position;
                Destroy(inGameStickers[i]);
                GameObject newSticker = Instantiate(duplicateSticker, position, Quaternion.identity);
                inGameStickers[i] = newSticker;
            }
        }

        timeToBeat = (int)Mathf.Floor(timeToBeat);

        StartCoroutine(EpisodeStart());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            CheckpointDeactivated();
        }

        if (episodeComplete && finishedCRActive == false)
        {
            finishedCRActive = true;
            StartCoroutine(EpisodeFinished());
        }

        if (enemiesKilled >= enemiesToKill && allEnemiesKilled == false && startAtCheckpoint == false)
        {
            allEnemiesKilled = true;
            GameObject thisSticker = sticker;
            if (episodeStats.stickersFound[5])
            {
                thisSticker = duplicateSticker;
            }
            inGameStickersFound[4] = true;
            GameObject newSticker = Instantiate(thisSticker);
            newSticker.GetComponent<Sticker>().found = true;
        }

        if (Box.boxHealth <= 0 && restartActive == false)
        {
            restartActive = true;
            StartCoroutine(RestartLevel());
        }
    }

    public void EnemyKilled(Vector2 position)
    {
        enemiesKilled++;
        int rand = Random.Range(0, 5);
        if (rand == 0)
        {
            newPerkSpawner = Instantiate(perkSpawner, position, Quaternion.identity);
            newPerkSpawner.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-5f, 5f), 15);
            newPerkSpawner.GetComponent<Rigidbody2D>().angularVelocity = Random.Range(500f, 1000f);
        }
    }
    public void StickerFound(GameObject sticker)
    {
        for (int i = 0; i < inGameStickers.Length; i++)
        {
            if (sticker == inGameStickers[i])
            {
                episodeStats.stickersFound[i] = true;
                SaveManager.SaveEpisode(season, episode, episodeStats);
                adventureUI.StickerFound(i);
                break;
            }
        }
    }
    public void CheckpointActivated()
    {
        episodeStats.checkpoint = true;
        SaveManager.SaveEpisode(season, episode, episodeStats);

        for (int i = 0; i < episodeStats.stickersFound.Length; i++)
        {
            if (episodeStats.stickersFound[i] == true)
            {
                pauseInfo.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
        }
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
        float waitTime = 3f;
        StartCoroutine(boxScript.DisableInputs(waitTime));
        yield return new WaitForSeconds(waitTime);
        UIManager.stopClock = false;
    }

    IEnumerator EpisodeFinished()
    {
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(1000));

        episodeStats.completed = true;
        episodeStats.checkpoint = false;
        episodeStats.stickersFound = inGameStickersFound;
        if ((UIManager.rawTime <= episodeStats.fastestTime || episodeStats.fastestTime == 0) && startAtCheckpoint == false)
        {
            episodeStats.fastestTime = UIManager.rawTime;
        }
        if (UIManager.rawTime <= timeToBeat && startAtCheckpoint == false)
        {
            GameObject thisSticker = sticker;
            if (episodeStats.stickersFound[5])
            {
                thisSticker = duplicateSticker;
            }
            episodeStats.stickersFound[5] = true;
            GameObject newSticker = Instantiate(thisSticker);
            newSticker.GetComponent<Sticker>().found = true;
            newSticker.GetComponent<Sticker>().timeFound = true;
            
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

    IEnumerator RestartLevel()
    {
        UIManager.stopClock = true;
        StartCoroutine(boxScript.DisableInputs(5));
        float timer = 0;
        float window = 0.4f;
        boxScript.GetComponent<Renderer>().sortingLayerName = "Dead Enemy";
        FindObjectOfType<CameraFollowBox>().overridePosition = true;
        FindObjectOfType<CameraFollowBox>().forcedPosition = FindObjectOfType<CameraFollowBox>().transform.position;
        while (timer < window)
        {
            boxScript.GetComponent<BoxCollider2D>().enabled = false;
            boxScript.GetComponent<Rigidbody2D>().gravityScale = 0;
            boxScript.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            boxScript.GetComponent<Rigidbody2D>().angularVelocity = 0;
            BoxVelocity.velocitiesX[0] = 0;
            timer += Time.deltaTime;
            yield return null;
        }
        boxScript.GetComponent<Rigidbody2D>().gravityScale = 6;
        boxScript.GetComponent<Rigidbody2D>().velocity = Vector2.up * 17;
        boxScript.GetComponent<Rigidbody2D>().angularVelocity = 1000;
        boxScript.GetComponent<Rigidbody2D>().angularDrag = 0;
        timer = 0;
        window = 2f;
        while (timer < window)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        timer = 0;
        window = 1.5f;
        while (timer < window)
        {
            Color color = UIBackground.GetComponent<Image>().color;
            color.a += Time.deltaTime;
            UIBackground.GetComponent<Image>().color = color;
            timer += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene("S" + season + "E" + episode);
    }
}
