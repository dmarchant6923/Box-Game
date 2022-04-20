using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    MainMenu mainMenuScript;

    public static BGStats BGStats = new BGStats();
    string BGFileName = SaveManager.BattlegroundFile;
    public static int BGHighScore1;
    public static int BGHighScore2;
    public static int BGHighScore3;

    public static TargetStats TTStats = new TargetStats();
    string TTFileName = SaveManager.TargetTestFile;
    public static float TTHighScoreEasy;
    public static float TTHighScoreMed;
    public static float TTHighScoreHard;

    public static Vector2 episodes = new Vector2(5, 5); //x = season, y = episode
    public static EpisodeStats[,] episodeStats = new EpisodeStats[5, 5]; //i = season, j = episode. GetLength(0) = number of seasons, GetLength(1) = number of episodes
    public static bool[] seasonsUnlocked = new bool[5];
    public static int stickersCollected;
    public static int[] stickersCollectedPerSeason = new int[5];
    public static int[] episodesCompletedPerSeason = new int[5];

    public bool mainMenu = false;
    public static bool exitFromAdventureMode = false;
    public static Vector2 exitFromAdventureModeEpisode = new Vector2(1, 1);

    void Start()
    {
        mainMenuScript = GetComponent<MainMenu>();

        //load data
        SaveManager.InitializeData();
        BGStats = JsonUtility.FromJson<BGStats>(SaveManager.Load(BGFileName));
        TTStats = JsonUtility.FromJson<TargetStats>(SaveManager.Load(TTFileName));
        episodeStats = LoadEpisodes();

        AnalyzeData();
    }

    public static EpisodeStats[,] LoadEpisodes()
    {
        EpisodeStats[,] episodeStats = new EpisodeStats[5, 5];
        for (int i = 1; i < episodeStats.GetLength(0) + 1; i++)
        {
            for (int j = 1; j < episodeStats.GetLength(1) + 1; j++)
            {
                if (SaveManager.FileExists(SaveManager.episodeDirectory + "S" + i + "E" + j))
                {
                    string jsonString = SaveManager.Load(SaveManager.episodeDirectory + "S" + i + "E" + j);
                    episodeStats[i - 1, j - 1] = JsonUtility.FromJson<EpisodeStats>(jsonString);
                }
            }
        }
        return episodeStats;
    }

    void AnalyzeData()
    {
        BGHighScore1 = BGStats.wavesReached[0];
        BGHighScore2 = BGStats.wavesReached[6];
        BGHighScore3 = BGStats.wavesReached[12];
        TTHighScoreEasy = TTStats.easyTimes[0];
        TTHighScoreMed = TTStats.medTimes[0];
        TTHighScoreHard = TTStats.hardTimes[0];

        stickersCollectedPerSeason = new int[5];
        episodesCompletedPerSeason = new int[5];
        seasonsUnlocked = new bool[5];
        stickersCollected = 0;

        seasonsUnlocked[0] = true;
        for (int i = 0; i < episodeStats.GetLength(0); i++)
        {
            for (int j = 0; j < episodeStats.GetLength(1); j++)
            {
                if (episodeStats[i, j].completed)
                {
                    episodesCompletedPerSeason[i]++;
                }
                foreach (bool stickerFound in episodeStats[i, j].stickersFound)
                {
                    if (stickerFound)
                    {
                        stickersCollectedPerSeason[i]++;
                    }
                }
            }
            if (episodesCompletedPerSeason[i] == episodeStats.GetLength(1) && i < episodeStats.GetLength(0) - 2)
            {
                seasonsUnlocked[i + 1] = true;
            }
            else if (i < episodeStats.GetLength(0) - 2)
            {
                seasonsUnlocked[i + 1] = false;
            }
        }
        foreach (int number in stickersCollectedPerSeason)
        {
            stickersCollected += number;
        }
    }


    void Update()
    {
        //at this point this should only be used for debugging
        if (mainMenu && mainMenuScript.scrollingSeasons == false && mainMenuScript.subMenu1)
        {
            if (Input.GetKeyDown(KeyCode.Space) && episodeStats[mainMenuScript.seasonSelected - 1, mainMenuScript.currentSelection - 1].completed == false)
            {
                int season = mainMenuScript.seasonSelected;
                int episode = mainMenuScript.currentSelection;
                episodeStats[season - 1, episode - 1].completed = true;
                SaveManager.SaveEpisode(season, episode, episodeStats[season - 1, episode - 1]);
                AnalyzeData();
                Debug.Log("Season " + season + " Episode " + episode + " Completed!");
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                SaveManager.ClearEpisodeData();
                episodeStats = LoadEpisodes();
                AnalyzeData();
                Debug.Log("episodes were deleted");
            }
        }

    }
}
