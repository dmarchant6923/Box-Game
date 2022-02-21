using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattlegroundUI : MonoBehaviour
{
    InputBroker UIInputs;
    UITools UITools;

    public Text timeText;

    public Image background;

    bool gameIsOver = false;
    public GameObject GameOver;
    public Text gameOverText;
    public Text gameOverStats;
    public Text roundText;
    public Text gameOverInstructions;

    public GameObject HiScores;
    public Text roundHSText;
    public Text timeHSText;
    public Text enemiesHSText;
    public Text HSLeftInstructions;
    public Text HSRightInstructions;

    int[] HSRounds = new int[6];
    float[] HSTime = new float[6];
    int[] HSEnemies = new int[6];
    bool resetHSCR = false;

    public BGStats stats = new BGStats();
    string fileName = SaveManager.BattlegroundFile;

    private void Start()
    {
        UIInputs = GetComponent<InputBroker>();
        UITools = GetComponent<UITools>();
        if (SaveManager.FileExists(fileName))
        {
            stats = JsonUtility.FromJson<BGStats>(SaveManager.Load(fileName));
        }

        background.enabled = false;
        GameOver.SetActive(false);
        HiScores.SetActive(false);
        Time.timeScale = 1;

        HSRounds = stats.wavesReached;
        HSTime = stats.times;
        HSEnemies = stats.enemiesKilled;
    }

    private void Update()
    {
        float wave = BattlegroundManager.wave;
        if (wave == 0)
        {
            wave = 1;
        }
        roundText.text = "Round: " + wave;


        if (Box.boxHealth <= 0 && gameIsOver == false)
        {
            UIManager.canPause = false;
            gameIsOver = true;
            if (HiScores.activeSelf == false)
            {
                GameOver.SetActive(true);
            }
            background.enabled = true;
            Time.timeScale = 0;
            gameOverStats.text = "You reached round " + BattlegroundManager.wave + "\nEnemies killed: " + BattlegroundManager.enemiesKilled;
            if (InputBroker.Keyboard)
            {
                gameOverInstructions.text = "Press Space to view High Scores\nPress Enter to play again";
            }
            else if (InputBroker.Controller)
            {
                gameOverInstructions.text = "Press X to view High Scores\nPress Start to play again";
            }
            if (BattlegroundManager.addToHiScores == true)
            {
                for (int i = 0; i < HSRounds.Length; i++)
                {
                    if (BattlegroundManager.wave > HSRounds[i])
                    {
                        for (int j = 5; j > i; j--)
                        {
                            HSRounds[j] = HSRounds[j - 1];
                            HSTime[j] = HSTime[j - 1];
                            HSEnemies[j] = HSEnemies[j - 1];
                        }
                        HSRounds[i] = BattlegroundManager.wave;
                        HSTime[i] = UIManager.rawTime;
                        HSEnemies[i] = BattlegroundManager.enemiesKilled;
                        if (i == 0)
                        {
                            gameOverText.text = "NEW HIGHSCORE!";
                        }
                        else
                        {
                            gameOverText.text = "GAME OVER";
                        }
                        break;
                    }
                }

                SaveHS();
            }
        }
        if (GameOver.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Xbox X") || Input.GetButtonDown("GC Y X"))
            {
                GameOver.SetActive(false);
                HiScores.SetActive(true);

                writeToHS();

                if (InputBroker.Keyboard)
                {
                    HSLeftInstructions.text = "Hold Space + R to reset High Scores";
                    HSRightInstructions.text = "Press Enter to play again";
                }
                else if (InputBroker.Controller)
                {
                    HSLeftInstructions.text = "Hold L + R to reset High Scores";
                    HSRightInstructions.text = "Press Start to play again";
                }
            }
        }


        if (HiScores.activeSelf == true)
        {
            if (((Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Space)) ||
                (Input.GetButton("GC L") && Input.GetButton("GC R")) ||
                (Input.GetAxisRaw("Xbox L") < -0.9f && Input.GetAxisRaw("Xbox R") < -0.9f))
                && resetHSCR == false)
            {
                StartCoroutine(ResetHS());
            }
        }

        if (GameOver.activeSelf == true || HiScores.activeSelf == true)
        {
            if (UIInputs.startButtonDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void SaveHS()
    {
        stats.wavesReached = HSRounds;
        stats.times = HSTime;
        stats.enemiesKilled = HSEnemies;

        string jsonString = JsonUtility.ToJson(stats);
        SaveManager.Save(jsonString, fileName);
    }

    private void writeToHS()
    {
        roundHSText.text = "Round:\n";
        timeHSText.text = "Time:\n";
        enemiesHSText.text = "Enemies Killed:\n";
        for (int i = 0; i < HSRounds.Length; i++)
        {

            if (HSRounds[i] == 0)
            {
                roundHSText.text += i + 1 + ". --- \n";
                timeHSText.text += "---\n";
                enemiesHSText.text += "---\n";
            }
            else
            {


                roundHSText.text += i + 1 + ". " + HSRounds[i] + "\n";
                timeHSText.text += UITools.RawTimeToString(HSTime[i], true) + "\n";
                enemiesHSText.text += HSEnemies[i] + "\n";
            }
        }
    }

    IEnumerator ResetHS()
    {
        resetHSCR = true;
        yield return new WaitForSecondsRealtime(1);
        if ((Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Space)) ||
            (Input.GetButton("GC L") && Input.GetButton("GC R")) ||
            (Input.GetAxisRaw("Xbox L") < -0.9f && Input.GetAxisRaw("Xbox R") < -0.9f))
        {

            HSRounds = new int[6];
            HSTime = new float[6];
            HSEnemies = new int[6];
            SaveHS();
            writeToHS();
        }
        else
        {
            resetHSCR = false;
        }
    }
}
