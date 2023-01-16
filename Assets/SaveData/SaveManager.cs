using UnityEngine;
using System.IO;

//save classes
[System.Serializable]
public class BGStats
{
    public int[] wavesReached = new int[18];
    public float[] times = new float[18];
    public int[] enemiesKilled = new int[18];
}

[System.Serializable]
public class TargetStats
{
    public float[] easyTimes = new float[6];
    public float[] medTimes = new float[6];
    public float[] hardTimes = new float[6];
}

[System.Serializable]
public class EpisodeStats
{
    public bool completed = false;
    public bool checkpoint = false;

    public bool[] stickersFound = new bool[6]; //first four spots are stickers in the map, last two are for enemies killed and completion time
    public float fastestTime;
}



public static class SaveManager
{
    public static string directory = "/SaveData/text files/";
    public static string episodeDirectory = "episodes/";
    public static string extension = ".txt";

    public static string TargetTestFile = "TTStats";
    public static string BattlegroundFile = "BGStats";
    //episode files are created automatically by the initializeSaveData in the main menu script. they are located in /SaveData/text files/episodes
    //and are named by their season i and episode j like "SiEj". Writing to or loading a specific episode file will require the following:
    //SaveManager.episodeDirectory + "S" + i + "E" + j + SaveManager.extension. For season 3 episode 4 this would search for the following file:
    // /Savedata/text files/episodes/S3E4.txt

    //***************   final build should be using persistentDataPath, not dataPath   ****************************8

    public static void SaveEpisode(int season, int episode, EpisodeStats episodeStats)
    {
        string jsonString = JsonUtility.ToJson(episodeStats);
        string fileName = episodeDirectory + "S" + season + "E" + episode;
        Save(jsonString, fileName);
    }
    public static void Save(string jsonString, string fileName)
    {
        string dir = Application.dataPath + directory;

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(dir + fileName + extension, jsonString);
    }
    public static string Load(string fileName)
    {
        string fullPath = Application.dataPath + directory + fileName + extension;

        string jsonString = "";
        if (File.Exists(fullPath))
        {
            jsonString = File.ReadAllText(fullPath);
            
        }
        else
        {
            File.WriteAllText(Application.dataPath + directory + fileName + extension, "");
        }

        return jsonString;
    }
    public static void InitializeData()
    {
        if (Directory.Exists(Application.dataPath + directory) == false)
        {
            Directory.CreateDirectory(Application.dataPath + directory);
        }
        if (FileExists(TargetTestFile) == false)
        {
            string jsonString = JsonUtility.ToJson(new TargetStats());
            Save(jsonString, TargetTestFile);
        }
        if (FileExists(BattlegroundFile) == false)
        {
            string jsonString = JsonUtility.ToJson(new BGStats());
            Save(jsonString, BattlegroundFile);
        }



        if (Directory.Exists(Application.dataPath + directory + episodeDirectory) == false)
        {
            Directory.CreateDirectory(Application.dataPath + directory + episodeDirectory);
        }
        for (int i = 1; i < DataManager.episodes.x + 1; i++)
        {
            for (int j = 1; j < DataManager.episodes.y + 1; j++)
            {
                if (FileExists(episodeDirectory + "S" + i + "E" + j) == false)
                {
                    string jsonString = JsonUtility.ToJson(new EpisodeStats());
                    Save(jsonString, episodeDirectory + "S" + i + "E" + j);
                }
            }
        }
    }
    public static bool FileExists(string fileName)
    {
        string fullPath = Application.dataPath + directory + fileName + extension;
        if (File.Exists(fullPath))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void ClearAllData()
    {
        string dir1 = Application.dataPath + directory + episodeDirectory;

        if (Directory.Exists(dir1))
        {
            string[] files = Directory.GetFiles(dir1);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(dir1);
        }


        string dir2 = Application.dataPath + directory;

        if (Directory.Exists(dir2))
        {
            string[] files = Directory.GetFiles(dir2);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(dir2);
        }

        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        InitializeData();
    }
    public static void ClearEpisodeData()
    {
        string dir = Application.dataPath + directory + episodeDirectory;

        if (Directory.Exists(dir))
        {
            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Directory.Delete(dir);
        }
        Directory.CreateDirectory(dir);
        InitializeData();
    }
}
