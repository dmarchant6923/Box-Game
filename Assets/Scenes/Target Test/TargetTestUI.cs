using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TargetTestUI : MonoBehaviour
{
    float maxHealth = 250;

    public float difficulty = 2;
    InputBroker UIInputs;
    UITools UITools;
    Box boxScript;

    public Text timeText;
    int numberOfTargets;
    [HideInInspector] public static int targetsHit;
    public Image targetsBackground;
    public Image targetImage;
    List<Image> UITargets = new List<Image>();

    float countdownLength = 2;
    public Text countdownText;
    public Image countdownBar;

    bool gameIsOver = false;
    public Image background;
    public Text finishText;
    string finishFirstLine;
    public Text medalText;
    public Text medalTimeText;
    public GameObject finishBlock;
    string medal;

    public GameObject HiScores;
    public Text timeHSText;
    public Text medalHSText;

    float silverMedalTime;
    [HideInInspector] public static float[] silverMedalTimes = new float[] { 60, 80, 0 }; // easy, medium, hard
    string silverTimeString = "0";

    float goldMedalTime;
    [HideInInspector] public static float[] goldMedalTimes = new float[] { 25, 48, 0 };
    string goldTimeString = "0";

    float platinumMedalTime;
    [HideInInspector] public static float[] platinumMedalTimes = new float[] { 12, 28, 0 };
    string platinumTimeString;

    float[] HSTime = new float[6];
    string[] HSTimeString = new string[6];
    string[] HSMedal = new string[6];
    bool resetHSCR = false;

    TargetStats stats = new TargetStats();
    string fileName = SaveManager.TargetTestFile;

    private void Start()
    {
        Box.dashUnlocked = true;
        Box.teleportUnlocked = true;
        Box.pulseUnlocked = true;

        UIInputs = GetComponent<InputBroker>();
        UITools = GetComponent<UITools>();
        boxScript = GameObject.Find("Box").GetComponent<Box>();
        if (SaveManager.FileExists(fileName))
        {
            stats = JsonUtility.FromJson<TargetStats>(SaveManager.Load(fileName));
        }

        numberOfTargets = GameObject.FindGameObjectsWithTag("Target").Length;
        for (int i = 0; i < numberOfTargets; i++)
        {
            UITargets.Add(Instantiate(targetImage, targetsBackground.transform, false));
            int ix = i - (i / 6) * 6;
            int iy = i / 6;
            UITargets[i].rectTransform.localPosition = new Vector2(-10 + ix * -30, -10 + iy * -30);
        }
        targetsBackground.rectTransform.sizeDelta = new Vector2(20 + (Mathf.Min(numberOfTargets, 6) * 30), 30 + (numberOfTargets - 1) / 6 * 50);
        targetsHit = 0;

        finishBlock.SetActive(false);
        background.enabled = false;

        HiScores.SetActive(false);

        if (difficulty == 1)
        {
            silverMedalTime = silverMedalTimes[0];
            goldMedalTime = goldMedalTimes[0];
            platinumMedalTime = platinumMedalTimes[0];
            HSTime = stats.easyTimes;
        }
        if (difficulty == 2)
        {
            silverMedalTime = silverMedalTimes[1];
            goldMedalTime = goldMedalTimes[1];
            platinumMedalTime = platinumMedalTimes[1];
            HSTime = stats.medTimes;
        }
        if (difficulty == 3)
        {
            silverMedalTime = silverMedalTimes[2];
            goldMedalTime = goldMedalTimes[2];
            platinumMedalTime = platinumMedalTimes[2];
            HSTime = stats.hardTimes;
        }

        MovingObjects[] movingObjects = FindObjectsOfType<MovingObjects>();
        foreach (MovingObjects item in movingObjects)
        {
            item.initialDelay = countdownLength;
        }

        platinumTimeString = Mathf.FloorToInt(platinumMedalTime / 60).ToString("00") + ":" + (platinumMedalTime % 60).ToString("00") + ":00";
        goldTimeString = Mathf.FloorToInt(goldMedalTime / 60).ToString("00") + ":" + (goldMedalTime % 60).ToString("00") + ":00";
        silverTimeString = Mathf.FloorToInt(silverMedalTime / 60).ToString("00") + ":" + (silverMedalTime % 60).ToString("00") + ":00";

        Box.boxHealth = maxHealth;
        UIManager.initialHealth = (int) maxHealth;

        countdownText.gameObject.SetActive(true);
        countdownText.transform.GetChild(0).gameObject.SetActive(true);
        UIManager.canPause = false;
        UIManager.stopClock = true;
        timeText.enabled = false;
        StartCoroutine(CountDown());
    }

    private void Update()
    {
        if (UITargets.Count > numberOfTargets - targetsHit && targetsHit != numberOfTargets)
        {
            StartCoroutine(DestroyTarget(UITargets[UITargets.Count - 1].transform.GetChild(0).gameObject));
            UITargets.RemoveAt(UITargets.Count - 1);
        }

        if (targetsHit == numberOfTargets && gameIsOver == false)
        {
            float rawTime = UIManager.rawTime + UIManager.minutes * 60 + Time.deltaTime;
            rawTime = Mathf.Floor(rawTime * 100) / 100;
            gameIsOver = true;
            UIManager.canPause = false;
            Destroy(UITargets[0].transform.GetChild(0).gameObject);

            finishFirstLine = "Success!";

            if (rawTime <= platinumMedalTime)
            {
                medal = "<color=#d9d7d5ff>Platinum Medal</color>";
            }
            else if (rawTime <= goldMedalTime)
            {
                medal = "<color=#e4b849ff>Gold Medal</color>";
            }
            else if (rawTime <= silverMedalTime)
            {
                medal = "<color=grey>Silver Medal</color>";
            }
            else
            {
                medal = "<color=#cd7f32ff>Bronze Medal </color>";
            }
            Time.timeScale = 0f;

            for (int i = 0; i < HSTime.Length; i++)
            {
                if (rawTime < HSTime[i] || HSTime[i] == 0)
                {
                    for (int j = 5; j > i; j--)
                    {
                        HSTime[j] = HSTime[j - 1];
                    }
                    HSTime[i] = rawTime;
                    if (i == 0)
                    {
                        finishFirstLine = "NEW HIGHSCORE!";
                    }
                    break;
                }
            }
            SaveTimes();

            finishText.text = "<size=50><b>" + finishFirstLine + "</b></size>\nYour time is " + UITools.RawTimeToString(rawTime, true) + "\nYou earned a <b>" + medal + "</b>";
            medalTimeText.text = platinumTimeString + "\n" + goldTimeString + "\n" + silverTimeString;
            medalText.text = "<color=#d9d7d5ff>Platinum Medal:</color>\n<color=#e4b849ff>Gold Medal:</color>\n<color=grey>Silver Medal:</color>";
            background.enabled = true;
            finishBlock.SetActive(true);
        }

        if (finishBlock.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Xbox X") || Input.GetButtonDown("GC Y X"))
            {
                finishBlock.SetActive(false);
                HiScores.SetActive(true);

                for (int i = 0; i < HSTime.Length; i++)
                {
                    if (HSTime[i] == 0)
                    {
                        HSTimeString[i] = "---";
                        HSMedal[i] = "---";
                    }
                    else
                    {
                        HSTimeString[i] = UITools.RawTimeToString(HSTime[i], true);
                        HSTime[i] = Mathf.Floor(HSTime[i] * 100) / 100;
                        if (HSTime[i] <= platinumMedalTime)
                        {
                            HSMedal[i] = "<color=#d9d7d5ff>Platinum Medal</color>";
                        }
                        else if (HSTime[i] <= goldMedalTime)
                        {
                            HSMedal[i] = "<color=#e4b849ff>Gold Medal</color>";
                        }
                        else if (HSTime[i] <= silverMedalTime)
                        {
                            HSMedal[i] = "<color=grey>Silver Medal</color>";
                        }
                        else
                        {
                            HSMedal[i] = "<color=#cd7f32ff>Bronze Medal </color>";
                        }
                    }

                    timeHSText.text =
                                    "1. " + HSTimeString[0] + "\n" +
                                    "2. " + HSTimeString[1] + "\n" +
                                    "3. " + HSTimeString[2] + "\n" +
                                    "4. " + HSTimeString[3] + "\n" +
                                    "5. " + HSTimeString[4] + "\n" +
                                    "6. " + HSTimeString[5];

                    medalHSText.text =
                                    HSMedal[0] + "\n" +
                                    HSMedal[1] + "\n" +
                                    HSMedal[2] + "\n" +
                                    HSMedal[3] + "\n" +
                                    HSMedal[4] + "\n" +
                                    HSMedal[5];
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

        if (finishBlock.activeSelf == true || HiScores.activeSelf == true)
        {
            if (UIInputs.startButtonDown)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
    private void SaveTimes()
    {
        if (difficulty == 1)
        {
            stats.easyTimes = HSTime;
        }
        if (difficulty == 2)
        {
            stats.medTimes = HSTime;
        }
        if (difficulty == 3)
        {
            stats.hardTimes = HSTime;
        }

        string jsonString = JsonUtility.ToJson(stats);
        SaveManager.Save(jsonString, fileName);
    }

    IEnumerator CountDown()
    {
        float timer = countdownLength;
        IEnumerator CR = boxScript.DisableInputs(countdownLength);
        StartCoroutine(CR);
        float countdownInitialLength = countdownBar.rectTransform.localScale.x;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            countdownText.text = UITools.RawTimeToString(Mathf.Max(timer, 0), false);
            countdownBar.rectTransform.localScale = new Vector2(countdownInitialLength * timer / countdownLength, countdownBar.rectTransform.localScale.y);
            UIManager.canPause = false;
            yield return null;
        }
        timeText.enabled = true;
        countdownText.transform.GetChild(0).gameObject.SetActive(false);
        UIManager.canPause = true;
        UIManager.stopClock = false;

        countdownText.text = "GO!";
        timer = 0;
        while (timer < 1.5f && UIManager.paused == false)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        countdownText.gameObject.SetActive(false);
    }
    IEnumerator ResetHS()
    {
        resetHSCR = true;
        yield return new WaitForSecondsRealtime(1);
        if ((Input.GetKey(KeyCode.R) && Input.GetKey(KeyCode.Space)) ||
            (Input.GetButton("GC L") && Input.GetButton("GC R")) ||
            (Input.GetAxisRaw("Xbox L") < -0.9f && Input.GetAxisRaw("Xbox R") < -0.9f))
        {
            HSTime = new float[6];
            SaveTimes();

            for (int i = 0; i < HSTime.Length; i++)
            {
                HSTimeString[i] = "---";
                HSMedal[i] = "---";
            }
            timeHSText.text =
                            "1. " + HSTimeString[0] + "\n" +
                            "2. " + HSTimeString[1] + "\n" +
                            "3. " + HSTimeString[2] + "\n" +
                            "4. " + HSTimeString[3] + "\n" +
                            "5. " + HSTimeString[4] + "\n" +
                            "6. " + HSTimeString[5];

            medalHSText.text =
                            HSMedal[0] + "\n" +
                            HSMedal[1] + "\n" +
                            HSMedal[2] + "\n" +
                            HSMedal[3] + "\n" +
                            HSMedal[4] + "\n" +
                            HSMedal[5];
        }
        else
        {
            resetHSCR = false;
        }
    }
    IEnumerator DestroyTarget(GameObject UITarget)
    {
        yield return new WaitForSeconds(Box.enemyHitstopDelay);
        Destroy(UITarget);
    }
}
