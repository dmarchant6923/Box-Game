using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EpisodeStartBox : MonoBehaviour
{
    public Text episodeText;
    public Text targetText;
    public GameObject stickers;
    EpisodeManager manager;

    int timeToBeat;

    void Start()
    {
        manager = GameObject.Find("Game Manager").GetComponent<EpisodeManager>();
        episodeText.text = "Season " + manager.season + " Episode " + manager.episode;

        timeToBeat = (int)Mathf.Floor(manager.timeToBeat);
        if (manager.startAtCheckpoint == false)
        {
            targetText.text = Mathf.Floor(timeToBeat / 60) + ":" + (timeToBeat % 60).ToString("D2") + "\n\n" + manager.enemiesToKill;
        }
        else
        {
            targetText.text = "---\n\n" + manager.enemiesToKill;
        }

        for (int i = 0; i < stickers.transform.childCount; i++)
        {
            if (manager.episodeStats.stickersFound[i] == true)
            {
                stickers.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
