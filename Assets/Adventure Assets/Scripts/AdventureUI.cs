using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class AdventureUI : MonoBehaviour
{
    EpisodeManager episodeManager;
    EpisodeStats episodeStats;

    public GameObject episodeStartBox;
    public GameObject pauseInfo;
    public GameObject UIBackground;

    int enemiesToKill;
    float timeToBeat;

    int season = 1;
    int episode = 1;
    bool startAtCheckpoint;

    private void Start()
    {
        UIManager.canPause = false;
        episodeManager = FindObjectOfType<EpisodeManager>();
        episodeStats = episodeManager.episodeStats;
        timeToBeat = episodeManager.timeToBeat;
        enemiesToKill = episodeManager.enemiesToKill;
        startAtCheckpoint = episodeManager.startAtCheckpoint;
        season = episodeManager.season;
        episode = episodeManager.episode;

        for (int i = 0; i < episodeStats.stickersFound.Length; i++)
        {
            if (episodeStats.stickersFound[i] == true)
            {
                pauseInfo.transform.GetChild(1).GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
        }

        StartCoroutine(EpisodeStart());
    }

    public void StickerFound(int index)
    {
        pauseInfo.transform.GetChild(1).GetChild(index).GetChild(0).gameObject.SetActive(true);
    }

    IEnumerator EpisodeStart()
    {
        yield return new WaitForFixedUpdate();
        UIManager.canPause = false;
        episodeStartBox.SetActive(true);
        foreach (BattleSpawner spawner in FindObjectsOfType<BattleSpawner>())
        {
            enemiesToKill += spawner.numEnemies;
        }
        episodeManager.enemiesToKill = enemiesToKill;

        string targetTime = Mathf.Floor(timeToBeat / 60) + ":" + (timeToBeat % 60).ToString("00");
        string beatText = "Time To Beat: " + targetTime;
        if (startAtCheckpoint)
        {
            beatText = "Time To Beat: ---";
        }
        pauseInfo.transform.GetChild(0).GetComponent<Text>().text = "Season " + season + " Episode " + episode +
            "\n" + beatText + "\nEnemies To Kill: " + enemiesToKill;
        episodeStartBox.transform.GetChild(0).GetComponent<Text>().text = "Season " + season + " Episode " + episode;
        episodeStartBox.transform.GetChild(3).GetComponent<Text>().text = targetTime + "\n\n" + enemiesToKill;


        float waitTime1 = 1f;
        float waitTime2 = 2f;
        yield return new WaitForSeconds(waitTime1);
        Color color = UIBackground.GetComponent<Image>().color;
        while (UIBackground.GetComponent<Image>().color.a > 0)
        {
            color.a -= 1 / waitTime2 * Time.deltaTime;
            UIBackground.GetComponent<Image>().color = color;
            yield return null;
        }
        color.a = 0;
        UIBackground.GetComponent<Image>().color = color;
        while (UIManager.stopClock == true)
        {
            yield return null;
        }
        episodeStartBox.SetActive(false);
        UIManager.canPause = true;
    }
}
