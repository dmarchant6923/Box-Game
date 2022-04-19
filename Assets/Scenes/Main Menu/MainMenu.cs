using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    InputBroker UIInputs;
    UITools UITools;

    bool inputsEnabled;

    public int currentSelection = 1;

    bool selectionCRActive = false;
    bool cycleCRActive = false;
    float initOptionsArrowX;
    float initSeasonArrowX;
    float initEpUArrowY;
    float initEpDArrowY;

    public Image foreground;
    public Image arrow;
    public GameObject infoBox;
    public Text title;

    bool mainMenu;
    public GameObject mainMenuOptions;

    public bool subMenu1;
    public GameObject adventureModeMenu;
    public bool scrollingSeasons = true;
    public int seasonSelected = 1;
    bool episodeSelectCRActive = false;
    bool episodeHighlightCR = false;
    GameObject seasonsOptions;
    GameObject episodesOptions;
    GameObject episodeInfo;
    Text episodeInfoText;
    Text seasonInfoText;
    public Image seasonArrow;
    public Image episodeArrowUp;
    public Image episodeArrowDown;
    public Transform stickers;
    public Transform locks;

    bool subMenu2;
    string BGInfo;
    string TTInfo;
    public GameObject subMenu2Options;

    bool subMenuTT;
    public GameObject subMenuTTOptions;
    public Text infoText;
    string TTInfoEasy;
    string TTInfoMedium;
    string TTInfoHard;

    bool subMenuBG;
    public GameObject subMenuBGOptions;
    GameObject stages;
    int numStages;
    public Image BGArrowLeft;
    public Image BGArrowRight;
    bool stageSelectCRActive = false;
    float initBGLArrowX;
    float initBGRArrowX;

    //bool subMenu3;


    void Start()
    {
        inputsEnabled = true;
        UIInputs = GetComponent<InputBroker>();
        UITools = GetComponent<UITools>();

        mainMenu = true; subMenu1 = false; subMenu2 = false; subMenuTT = false; //subMenu3 = false;
        subMenu2Options.SetActive(false); subMenuTTOptions.SetActive(false); infoBox.SetActive(false);

        Time.timeScale = 1;

        BGInfo = "I do not know where I am or why I am here.\n\n<i>All I know is that I must kill.</i>\n\nHigh Score: ";
        if (DataManager.BGHighScore != 0)
        {
            BGInfo += "Round " + DataManager.BGHighScore;
        }
        else
        {
            BGInfo += "---";
        }

        TTInfo = "Break the targets!\n\nSmash all targets in the arena as quick as you can.\n\n Pick from easy, medium, and hard difficulties.";
        TTInfoEasy = TTHSUpdate(DataManager.TTHighScoreEasy, 0);
        TTInfoMedium =  TTHSUpdate(DataManager.TTHighScoreMed, 1);
        TTInfoHard =  TTHSUpdate(DataManager.TTHighScoreHard, 2);

        stages = subMenuBGOptions.transform.GetChild(0).gameObject;
        numStages = stages.transform.childCount;

        seasonsOptions = adventureModeMenu.transform.GetChild(0).gameObject;
        episodesOptions = adventureModeMenu.transform.GetChild(1).gameObject;
        episodeInfo = adventureModeMenu.transform.GetChild(2).gameObject;
        episodeInfoText = episodeInfo.transform.GetChild(0).GetComponent<Text>();
        seasonInfoText = episodeInfo.transform.GetChild(1).GetComponent<Text>();

        stickers.gameObject.SetActive(false);

        episodeArrowUp.enabled = false;
        episodeArrowDown.enabled = false;

        initOptionsArrowX = arrow.transform.position.x;
        initSeasonArrowX = seasonArrow.transform.position.x;
        initEpUArrowY = episodeArrowUp.transform.position.y;
        initEpDArrowY = episodeArrowDown.transform.position.y;

        initBGLArrowX = BGArrowLeft.transform.position.x;
        initBGRArrowX = BGArrowRight.transform.position.x;

        if (DataManager.exitFromAdventureMode)
        {
            mainMenu = false;
            subMenu1 = true;
            title.enabled = false;
            arrow.enabled = false;
            scrollingSeasons = false;
            seasonSelected = (int) DataManager.exitFromAdventureModeEpisode.x;
            currentSelection = (int)DataManager.exitFromAdventureModeEpisode.y;
            DataManager.exitFromAdventureMode = false;
            DataManager.exitFromAdventureModeEpisode = Vector2.one;
        }
    }
    string TTHSUpdate(float rawTime, int difficulty)
    {
        rawTime = Mathf.Floor(rawTime * 100) / 100;
        string timeString = UITools.RawTimeToString(rawTime, true);
        string difficultyString = "Easy\n11";


        string medalString = "<color=#cd7f32ff>Bronze Medal </color>";
        if (rawTime <= TargetTestUI.platinumMedalTimes[difficulty])
        {
            medalString = "<color=#d9d7d5ff>Platinum Medal</color>";
        }
        else if (rawTime <= TargetTestUI.goldMedalTimes[difficulty])
        {
            medalString = "<color=#e4b849ff>Gold Medal</color>";
        }
        else if (rawTime <= TargetTestUI.silverMedalTimes[difficulty])
        {
            medalString = "<color=grey>Silver Medal</color>";
        }

        if (difficulty == 1)
        {
            difficultyString = "Medium\n12";
        }
        if (difficulty == 2)
        {
            difficultyString = "Hard\nXX";
        }

        if (rawTime < 0.1f)
        {
            timeString = "---"; medalString = "";
        }
        string text = "Level: "+ difficultyString +" Targets\n\nHigh Score: " + timeString + "\n" + medalString;
        return text;
    }

    void Update()
    {
        GameObject.Find("Box").GetComponent<InputBroker>().inputsEnabled = false;

        if (mainMenu == true)
        {
            MainMenuScreen();
            mainMenuOptions.SetActive(true);
            adventureModeMenu.SetActive(false);
            subMenu2Options.SetActive(false);
            subMenuTTOptions.SetActive(false);
            subMenuBGOptions.SetActive(false);
        }
        else if (subMenu1 == true)
        {
            AdventureModeScreen();
            mainMenuOptions.SetActive(false);
            adventureModeMenu.SetActive(true);
            subMenu2Options.SetActive(false);
            subMenuTTOptions.SetActive(false);
            subMenuBGOptions.SetActive(false);
        }
        else if (subMenu2 == true)
        {
            SubMenu2Screen();
            mainMenuOptions.SetActive(false);
            adventureModeMenu.SetActive(false);
            subMenu2Options.SetActive(true);
            subMenuTTOptions.SetActive(false);
            subMenuBGOptions.SetActive(false);
        }
        else if (subMenuTT == true)
        {
            SubMenuTTScreen();
            mainMenuOptions.SetActive(false);
            adventureModeMenu.SetActive(false);
            subMenu2Options.SetActive(false);
            subMenuTTOptions.SetActive(true);
            subMenuBGOptions.SetActive(false);
        }
        else if (subMenuBG == true)
        {
            SubMenuBGScreen();
            mainMenuOptions.SetActive(false);
            adventureModeMenu.SetActive(false);
            subMenu2Options.SetActive(false);
            subMenuTTOptions.SetActive(false);
            subMenuBGOptions.SetActive(true);
        }

        if ((UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8) && cycleCRActive == false && inputsEnabled == true && subMenuBG == false)
        {
            StartCoroutine(CycleSelect(true));
        }
    }

    void MainMenuScreen()
    {
        infoBox.SetActive(false);
        if (currentSelection > 4)
        {
            currentSelection = 1;
        }
        if (currentSelection < 1)
        {
            currentSelection = 4;
        }

        if (selectionCRActive == false)
        {
            StartCoroutine(SelectionCR(mainMenuOptions));
        }

        if (inputsEnabled)
        {
            //adventure mode
            if (currentSelection == 1 && UITools.actionButton)
            {
                subMenu1 = true;
                mainMenu = false;
                currentSelection = 1;
                title.enabled = false;
                arrow.enabled = false;
                StartCoroutine(IgnoreInputs());
                return;
            }
            //minigames
            if (currentSelection == 2 && UITools.actionButton)
            {
                subMenu2 = true;
                mainMenu = false;
                currentSelection = 1;
                StartCoroutine(IgnoreInputs());
                return;
            }
            //options
            if (currentSelection == 3 && UITools.actionButton)
            {
                return;
            }
            //exit
            if (currentSelection == 4 && UITools.actionButton)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
        }
    }

    void AdventureModeScreen()
    {
        if ((UIInputs.leftStick.x > 0.8 || UITools.actionButton) && currentSelection != 6 && scrollingSeasons == true) 
            { scrollingSeasons = false; seasonSelected = currentSelection; currentSelection = 1; StartCoroutine(IgnoreInputs()); }
        if ((UIInputs.leftStick.x < -0.8 || UITools.backButton) && scrollingSeasons == false) 
            { scrollingSeasons = true; currentSelection = seasonSelected; StartCoroutine(IgnoreInputs()); }

        if (scrollingSeasons == true)
        {
            if (currentSelection > 6) { currentSelection = 1; }
            if (currentSelection < 1) { currentSelection = 6; }
            seasonSelected = currentSelection;
            seasonArrow.enabled = true;

            episodeInfoText.text = "";
            stickers.gameObject.SetActive(false);
        }
        else
        {
            if (currentSelection > 5) { currentSelection = 5; }
            if (currentSelection < 1) { currentSelection = 1; }
            seasonArrow.enabled = false;
            stickers.gameObject.SetActive(true);

            string completedText = "Incomplete";
            string fastestTimeText = "---";
            bool completed = DataManager.episodeStats[seasonSelected - 1, currentSelection - 1].completed;
            if (completed)
            {
                completedText = "<color=green>Complete!</color>";
                fastestTimeText = UITools.RawTimeToString(DataManager.episodeStats[seasonSelected - 1, currentSelection - 1].fastestTime, true);
            }

            episodeInfoText.text = "Season " + seasonSelected + " Episode " + currentSelection + "\n" + completedText + "\nFastest Time:\n" + fastestTimeText;
            for (int i = 0; i < stickers.transform.childCount; i++)
            {
                if (DataManager.episodeStats[seasonSelected - 1, currentSelection - 1].stickersFound[i] == false)
                {
                    stickers.GetChild(i).GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    stickers.GetChild(i).GetChild(0).gameObject.SetActive(true);
                }
            }
        }
        if (currentSelection != 6)
        {
            seasonInfoText.gameObject.SetActive(true);
            seasonInfoText.text = "Season " + seasonSelected + "\n" +
                "Episodes Finished: " + DataManager.episodesCompletedPerSeason[seasonSelected - 1] + "/5\n" +
                "     " + DataManager.stickersCollectedPerSeason[seasonSelected - 1] + "/30";

        }
        else
        {
            seasonInfoText.gameObject.SetActive(false);
        }

        for (int i = 1; i < DataManager.episodes.x; i++)
        {
            if (DataManager.seasonsUnlocked[i] && locks.GetChild(i).gameObject.activeSelf)
            {
                locks.GetChild(i).gameObject.SetActive(false);
            }
            else if (DataManager.seasonsUnlocked[i] == false && locks.GetChild(i).gameObject.activeSelf == false)
            {
                locks.GetChild(i).gameObject.SetActive(true);
            }
        }

        if (selectionCRActive == false && scrollingSeasons == true)
        {
            StartCoroutine(SeasonSelectionCR(seasonsOptions));
        }
        else if (episodeSelectCRActive == false && scrollingSeasons == false)
        {
            StartCoroutine(EpisodeSelectCR());
        }

        if (scrollingSeasons == false && episodeHighlightCR == false)
        {
            StartCoroutine(EpisodeHighlight());
        }

        for (int i = 0; i < episodesOptions.transform.childCount; i++)
        {
            episodesOptions.transform.GetChild(i).gameObject.SetActive(false);
        }

        if (scrollingSeasons == true && currentSelection != 6)
        {
            episodesOptions.transform.GetChild(currentSelection - 1).gameObject.SetActive(true);
        }
        else if (scrollingSeasons == false)
        {
            episodesOptions.transform.GetChild(seasonSelected - 1).gameObject.SetActive(true);
        }


        if (inputsEnabled)
        {
            if (scrollingSeasons == false && UITools.actionButton)
            {
                string sceneName = "S" + seasonSelected + "E" + currentSelection;
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (scene.path == "Assets/Scenes/Adventure Mode/Season "+ seasonSelected + "/" + sceneName + ".unity")
                    {
                        StartCoroutine(LoadScreen(sceneName));
                    }
                }
            }

            //back
            if (scrollingSeasons == true && ((currentSelection == 6 && UITools.actionButton) || UITools.backButton))
            {
                subMenu1 = false;
                mainMenu = true;
                currentSelection = 1;
                infoBox.SetActive(false);
                title.enabled = true;
                arrow.enabled = true;
                StartCoroutine(IgnoreInputs());
                return;
            }
        }
    }

    void SubMenu2Screen()
    {
        if (currentSelection > 4)
        {
            currentSelection = 1;
        }
        if (currentSelection < 1)
        {
            currentSelection = 4;
        }

        if (selectionCRActive == false)
        {
            StartCoroutine(SelectionCR(subMenu2Options));
        }


        if (inputsEnabled)
        {
            //battleground
            if (currentSelection == 1 && UITools.actionButton)
            {
                subMenu2 = false;
                subMenuBG = true;
                currentSelection = 1;
                infoBox.SetActive(false);
                arrow.enabled = false;
                StartCoroutine(IgnoreInputs());
                //StartCoroutine(LoadScreen("Battleground"));
                return;
            }
            //targetRB test
            if (currentSelection == 2 && UITools.actionButton)
            {
                subMenu2 = false;
                subMenuTT = true;
                currentSelection = 1;
                infoBox.SetActive(true);
                StartCoroutine(IgnoreInputs());
                return;
            }
            //targetRB test
            if (currentSelection == 3 && UITools.actionButton)
            {
                StartCoroutine(LoadScreen("Playground"));
                return;
            }
            //back
            if ((currentSelection == 4 && UITools.actionButton) || UITools.backButton)
            {
                subMenu2 = false;
                mainMenu = true;
                currentSelection = 2;
                infoBox.SetActive(false);
                StartCoroutine(IgnoreInputs());
                return;
            }
        }

        if (currentSelection == 1)
        {
            infoBox.SetActive(true);
            infoText.text = BGInfo;
            infoText.fontSize = 25;
        }
        if (currentSelection == 2)
        {
            infoBox.SetActive(true);
            infoText.text = TTInfo;
            infoText.fontSize = 25;
        }
        if (currentSelection == 3)
        {
            infoBox.SetActive(false);
            infoText.text = "";
        }
    }

    void SubMenuTTScreen()
    {

        if (currentSelection > 4)
        {
            currentSelection = 1;
        }
        if (currentSelection < 1)
        {
            currentSelection = 4;
        }

        if (selectionCRActive == false)
        {
            StartCoroutine(SelectionCR(subMenuTTOptions));
        }


        if (inputsEnabled)
        {
            if (currentSelection == 1 && UITools.actionButton)
            {
                StartCoroutine(LoadScreen("Target Test Easy"));
                return;
            }
            if (currentSelection == 2 && UITools.actionButton)
            {
                StartCoroutine(LoadScreen("Target Test Medium"));
                return;
            }
            if (currentSelection == 3 && UITools.actionButton)
            {
                return;
            }
            if ((currentSelection == 4 && UITools.actionButton) || UITools.backButton)
            {
                subMenu2 = true;
                subMenuTT = false;
                currentSelection = 2;
                infoBox.SetActive(false);
                StartCoroutine(IgnoreInputs());
                return;
            }
        }

        if (currentSelection == 1)
        {
            infoText.text = TTInfoEasy;
            infoText.fontSize = 35;
        }
        if (currentSelection == 2)
        {
            infoText.text = TTInfoMedium;
            infoText.fontSize = 35;
        }
        if (currentSelection == 3)
        {
            infoText.text = TTInfoHard;
            infoText.fontSize = 35;
        }
    }

    void SubMenuBGScreen()
    {
        infoBox.SetActive(false);
        if ((UIInputs.leftStick.x > 0.8 || UIInputs.leftStick.x < -0.8) && cycleCRActive == false && inputsEnabled == true)
        {
            StartCoroutine(CycleSelect(false));
        }

        if (currentSelection > numStages)
        {
            currentSelection = 1;
        }
        if (currentSelection < 1)
        {
            currentSelection = numStages;
        }

        if (stageSelectCRActive == false)
        {
            StartCoroutine(StageSelectCR());
        }

        if (UITools.backButton)
        {
            subMenuBG = false;
            subMenu2 = true;
            currentSelection = 1;
            infoBox.SetActive(true);
            arrow.enabled = true;
            StartCoroutine(IgnoreInputs());
            return;
        }
    }





    IEnumerator SelectionCR(GameObject options)
    {
        selectionCRActive = true;
        int selectionActive = currentSelection;
        GameObject option;
        option = options.transform.GetChild(selectionActive - 1).gameObject;

        Color color = option.GetComponent<Image>().color;
        Color newColor = new Color(color.r, color.g, color.b, 1);
        option.GetComponent<Image>().color = newColor;
        option.transform.localScale *= 1.2f;
        Color shadowColor = option.GetComponent<Shadow>().effectColor;
        option.GetComponent<Shadow>().effectColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0.8f);
        arrow.rectTransform.position = new Vector2(initOptionsArrowX, option.transform.position.y);
        IEnumerator CR = UITools.SelectArrowOsc(arrow, true, true);
        StartCoroutine(CR);
        while (selectionActive == currentSelection && inputsEnabled == true && options.activeSelf == true)
        {
            yield return null;
        }
        StopCoroutine(CR);
        option.transform.localScale /= 1.2f;
        option.GetComponent<Image>().color = color;
        option.GetComponent<Shadow>().effectColor = shadowColor;
        selectionCRActive = false;
    }
    IEnumerator SeasonSelectionCR(GameObject options)
    {
        selectionCRActive = true;
        int selectionActive = currentSelection;
        GameObject option;
        option = options.transform.GetChild(selectionActive - 1).gameObject;

        Color color = option.GetComponent<Image>().color;
        Color newColor = new Color(color.r, color.g, color.b, 1);
        option.GetComponent<Image>().color = newColor;
        option.transform.localScale *= 1.2f;
        seasonArrow.GetComponent<Image>().rectTransform.position = new Vector2(initSeasonArrowX, option.transform.position.y);
        IEnumerator CR = UITools.SelectArrowOsc(seasonArrow.GetComponent<Image>(), true, false);
        StartCoroutine(CR);
        while ((selectionActive == currentSelection || selectionActive == seasonSelected) && inputsEnabled == true && options.activeSelf == true)
        {
            yield return null;
        }
        StopCoroutine(CR);
        option.transform.localScale /= 1.2f;
        option.GetComponent<Image>().color = color;
        selectionCRActive = false;
    }
    IEnumerator EpisodeSelectCR()
    {
        episodeSelectCRActive = true;
        int currentSeasonSelected = seasonSelected;
        GameObject selectedSeason = episodesOptions.transform.GetChild(seasonSelected - 1).gameObject;
        int lastEpisodeSelected = currentSelection;
        episodeArrowUp.rectTransform.position = new Vector2(episodeArrowUp.rectTransform.position.x, initEpUArrowY);
        episodeArrowDown.rectTransform.position = new Vector2(episodeArrowDown.rectTransform.position.x, initEpDArrowY);
        IEnumerator CRUp = UITools.SelectArrowOsc(episodeArrowUp, false, true);
        IEnumerator CRDown = UITools.SelectArrowOsc(episodeArrowDown, false, false);
        StartCoroutine(CRUp); StartCoroutine(CRDown);
        yield return null;
        while (scrollingSeasons == false)
        {
            float yPosition = Mathf.Min((currentSelection * 200) - 200, 800);
            yPosition = Mathf.Max(yPosition, 0);
            selectedSeason.GetComponent<RectTransform>().localPosition = new Vector2(selectedSeason.GetComponent<RectTransform>().localPosition.x,
                Mathf.MoveTowards(selectedSeason.GetComponent<RectTransform>().localPosition.y, yPosition, 2000 * Time.unscaledDeltaTime));
            lastEpisodeSelected = currentSelection;
            yield return null;
        }
        StopCoroutine(CRUp); StopCoroutine(CRDown);
        while (scrollingSeasons && seasonSelected == currentSeasonSelected && subMenu1)
        {
            yield return null;
        }
        if ((scrollingSeasons == true && seasonSelected != currentSeasonSelected) || subMenu1 == false)
        {
            selectedSeason.GetComponent<RectTransform>().localPosition = new Vector2(selectedSeason.GetComponent<RectTransform>().localPosition.x, 0);
        }
        else if (scrollingSeasons == false && seasonSelected == currentSeasonSelected)
        {
            currentSelection = lastEpisodeSelected;
        }

        episodeSelectCRActive = false;
    }
    IEnumerator EpisodeHighlight()
    {
        episodeHighlightCR = true;
        int episodeSelected = currentSelection;
        GameObject selectedSeason = episodesOptions.transform.GetChild(seasonSelected - 1).gameObject;
        GameObject selectedEpisode = selectedSeason.transform.GetChild(episodeSelected - 1).gameObject;
        Color color = selectedEpisode.GetComponent<Image>().color;
        Color newColor = new Color(color.r, color.g, color.b, 1);
        Vector2 initialSize = selectedEpisode.transform.localScale;
        float episodeScale = 1.6f;
        Vector2 newSize = selectedEpisode.transform.localScale * episodeScale;
        Color shadowColor = selectedEpisode.GetComponent<Shadow>().effectColor;
        selectedEpisode.GetComponent<Shadow>().effectColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, 0.8f);
        int modCurrentSelection = currentSelection;
        yield return null;
        while (modCurrentSelection == episodeSelected && scrollingSeasons == false)
        {
            yield return null;
            modCurrentSelection = Mathf.Min(currentSelection, 5);
            modCurrentSelection = Mathf.Max(modCurrentSelection, 1);
            if (modCurrentSelection != 1)
            {
                episodeArrowUp.enabled = true;
            }
            if (modCurrentSelection != 5)
            {
                episodeArrowDown.enabled = true;
            }
            selectedEpisode.transform.localScale = Vector2.MoveTowards(selectedEpisode.transform.localScale, newSize, 10 * Time.deltaTime);
            selectedEpisode.GetComponent<Image>().color = Color.Lerp(selectedEpisode.GetComponent<Image>().color, newColor, 10 * Time.deltaTime);
        }
        episodeArrowDown.enabled = false;
        episodeArrowUp.enabled = false;
        selectedEpisode.GetComponent<Image>().color = color;
        selectedEpisode.transform.localScale = initialSize;
        selectedEpisode.GetComponent<Shadow>().effectColor = shadowColor;
        episodeHighlightCR = false;
    }
    IEnumerator StageSelectCR()
    {
        stageSelectCRActive = true;
        BGArrowLeft.rectTransform.position = new Vector2(initBGLArrowX, BGArrowLeft.rectTransform.position.y);
        BGArrowRight.rectTransform.position = new Vector2(initBGRArrowX, BGArrowRight.rectTransform.position.y);
        IEnumerator CRLeft = UITools.SelectArrowOsc(BGArrowLeft, true, false);
        IEnumerator CRRight = UITools.SelectArrowOsc(BGArrowRight, true, true);
        StartCoroutine(CRLeft); StartCoroutine(CRRight);
        while (subMenuBG == true)
        {
            float xPosition = (currentSelection - 1) * -300;
            xPosition = Mathf.Max(xPosition, 0);
            stages.GetComponent<RectTransform>().localPosition =
                new Vector2(Mathf.MoveTowards(stages.GetComponent<RectTransform>().localPosition.x, xPosition, 2000 * Time.unscaledDeltaTime),
                stages.GetComponent<RectTransform>().localPosition.y);
            Debug.Log(stages.GetComponent<RectTransform>().localPosition.x);
            yield return null;
        }
        StopCoroutine(CRLeft); StopCoroutine(CRRight);

        episodeSelectCRActive = false;
    }
    IEnumerator CycleSelect(bool vertical)
    {
        Debug.Log(currentSelection);
        cycleCRActive = true;
        int direction = -(int)Mathf.Sign(UIInputs.leftStick.y);
        if (vertical == false)
        {
            direction = (int)Mathf.Sign(UIInputs.leftStick.x);
        }
        currentSelection += direction;
        float window1 = 0.3f;
        float timer = 0;
        while ((((UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8) && vertical) || ((UIInputs.leftStick.x > 0.8 || UIInputs.leftStick.x < -0.8) && vertical == false))
            && timer <= window1)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        float window2 = 0.12f;
        timer = 0;
        while ((((UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8) && vertical) || ((UIInputs.leftStick.x > 0.8 || UIInputs.leftStick.x < -0.8) && vertical == false))
            && inputsEnabled == true)
        {
            timer += Time.unscaledDeltaTime;
            if (timer > window2)
            {
                currentSelection += direction;
                timer = 0;
            }
            yield return null;
        }
        cycleCRActive = false;
    }
    IEnumerator IgnoreInputs()
    {
        inputsEnabled = false;
        yield return new WaitForSecondsRealtime(0.02f);
        inputsEnabled = true;
    }
    IEnumerator LoadScreen(string sceneName)
    {
        Time.timeScale = 0.3f;
        float loadWindow = 2f;
        float loadTimer = 0;
        Color initialColor = foreground.color;
        Color color = initialColor;
        while (loadTimer <= loadWindow)
        {
            inputsEnabled = false;
            color.a += 1 * Time.unscaledDeltaTime;
            foreground.color = color;
            loadTimer += Time.unscaledDeltaTime;
            yield return null;
        }
        inputsEnabled = true;
        Time.timeScale = 1;
        SceneManager.LoadScene(sceneName);
    }
}
