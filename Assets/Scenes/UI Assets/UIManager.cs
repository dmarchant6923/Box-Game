using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    InputBroker UIInputs;
    UITools UITools;
    Box boxScript;

    [HideInInspector] public static bool canPause;
    [HideInInspector] public static bool paused;
    public GameObject PauseText; //activating this activates pause menu
    public Image background;
    public GameObject option1;
    public GameObject option2;
    public GameObject option3;
    public Image arrow;

    float initialArrowX;

    int currentSelection = 1;
    bool cycleCRActive = false;
    bool selectionCRActive = false;

    public Text timeText;
    [HideInInspector] public static float rawTime;
    [HideInInspector] public static int minutes;
    public static bool stopClock = false;

    public Text healthText;
    public Text healthIncrement;
    public bool spawnIncrement = false;
    public Image healthBar;
    public float healthInitialWidth;
    Text lostHealth;
    int healthLastFrame;
    [HideInInspector] public static int initialHealth;

    public GameObject dash;
    public Image dashBar;
    public Image dashBackgroundImage;
    float dashBarInitialWidth;
    bool dashBarRefill = false;
    bool dashNotGrounded = false;

    public GameObject pulse;
    public Image pulseBar;
    public Image pulseBackgroundImage;
    float pulseBarInitialWidth;
    bool pulseBarRefill = false;
    public static bool killToPulse = false;
    public static bool pulseNoKill = false;

    public GameObject teleport;
    public Image teleportBar;
    float teleportBarInitialWidth;
    bool teleportBarRefill = false;

    bool starPowerCR = false;
    Color starPowerColor;
    Color teleBarInitialColor;
    Color pulseBarInitialColor;
    Color pulseBackgroundInitialColor;
    Color dashBarInitialColor;
    Color dashBackgroundInitialColor;

    bool pauseWaitFinished = true;

    private void Start()
    {
        UIInputs = GetComponent<InputBroker>();
        UITools = GetComponent<UITools>();
        boxScript = GameObject.Find("Box").GetComponent<Box>();

        canPause = true;
        paused = false;

        rawTime = 0;
        minutes = 0;

        dashBarInitialWidth = dashBar.transform.localScale.x;
        pulseBarInitialWidth = pulseBar.transform.localScale.x;
        teleportBarInitialWidth = dashBar.transform.localScale.x;
        healthInitialWidth = healthBar.transform.localScale.x;

        PauseText.SetActive(false);
        background.enabled = false;

        initialArrowX = arrow.transform.position.x;

        starPowerColor = new Color(1, 1, 0.5f);
        teleBarInitialColor = teleportBar.color;
        pulseBarInitialColor = pulseBar.color;
        pulseBackgroundInitialColor = pulseBackgroundImage.color;
        dashBarInitialColor = dashBar.color;
        dashBackgroundInitialColor = dashBackgroundImage.color;

        Time.timeScale = 1;
        //Time.timeScale = 0.5f;

        StartCoroutine(InitialDelay());
    }

    // Update is called once per frame
    private void Update()
    {
        if (stopClock == false)
        {
            rawTime += Time.deltaTime;
        }
        timeText.text = UITools.RawTimeToString(rawTime, true);

        healthText.text = "Health: " + Mathf.Ceil(Box.boxHealth) + "/" + initialHealth;
        healthBar.transform.localScale = new Vector2(Mathf.MoveTowards(healthBar.transform.localScale.x, Mathf.Min(healthInitialWidth * Box.boxHealth / initialHealth, healthInitialWidth),
            healthInitialWidth * 50 * Time.deltaTime / initialHealth), healthBar.transform.localScale.y);

        if (Box.dashActive && dashBarRefill == false)
        {
            StartCoroutine(DashBarRefill());
            StartCoroutine(DashGroundedCheck());
        }
        if (Box.teleportActive && teleportBarRefill == false)
        {
            StartCoroutine(TeleportBarRefill());
        }
        if (Box.pulseActive && pulseBarRefill == false)
        {
            StartCoroutine(PulseBarRefill());
        }
        if (healthLastFrame != (int)Mathf.Ceil(Box.boxHealth) && spawnIncrement == true)
        {
            lostHealth = Instantiate(healthIncrement, healthText.transform);
            if (healthLastFrame > (int)Mathf.Ceil(Box.boxHealth))
            {
                lostHealth.text = ((int)Mathf.Ceil(Box.boxHealth) - healthLastFrame).ToString();
            }
            else
            {
                lostHealth.text = "+"+((int)Mathf.Ceil(Box.boxHealth) - healthLastFrame).ToString();
                lostHealth.color = Color.magenta;
            }
        }
        if (Box.boxHealth > initialHealth)
        {
            Box.boxHealth = initialHealth;
        }

        healthLastFrame = (int)Mathf.Ceil(Box.boxHealth);

        if (UIInputs.startButtonDown && paused == false && canPause)
        {
            paused = true;
            Time.timeScale = 0;
            PauseText.SetActive(true);
            background.enabled = true;
            StartCoroutine(DisableInputs());
        }
        else if (paused == true)
        {
            if (currentSelection > 3)
            {
                currentSelection = 1;
            }
            if (currentSelection < 1)
            {
                currentSelection = 3;
            }
            if ((UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8) && cycleCRActive == false)
            {
                StartCoroutine(CycleSelect());
            }

            if (selectionCRActive == false)
            {
                StartCoroutine(SelectionCR());
            }

            if (pauseWaitFinished == true)
            {
                if ((currentSelection == 1 && UITools.actionButton) || UITools.backButton)
                {
                    paused = false;
                    Time.timeScale = 1;
                    PauseText.SetActive(false);
                    background.enabled = false;
                }
                if (currentSelection == 2 && UITools.actionButton)
                {
                    paused = false;
                    Time.timeScale = 1;
                    PauseText.SetActive(false);
                    background.enabled = false;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
                if (currentSelection == 3 && UITools.actionButton)
                {
                    SceneManager.LoadScene("Main Menu");
                }
            }
        }

        if (BoxPerks.starActive)
        {
            teleportBar.color = starPowerColor;
            pulseBar.color = starPowerColor;
            dashBar.color = starPowerColor;
            if (starPowerCR == false)
            {
                StartCoroutine(StarPower());
            }
        }
        else
        {
            teleportBar.color = teleBarInitialColor;
            pulseBar.color = pulseBarInitialColor;
            dashBar.color = dashBarInitialColor;
        }
        dashBackgroundImage.color = dashBackgroundInitialColor;
        if (dashNotGrounded)
        {
            float colorMult = 2.5f;
            dashBar.color = new Color(dashBar.color.r / colorMult, dashBar.color.g / colorMult, dashBar.color.b / colorMult);
            dashBackgroundImage.color = new Color(dashBackgroundImage.color.r / colorMult, dashBackgroundImage.color.g / colorMult, dashBackgroundImage.color.b / colorMult);
        }

        if (killToPulse)
        {
            if (Box.pulseActive)
            {
                pulseNoKill = true;
                Box.pulseUnlocked = false;
            }

            pulseBackgroundImage.color = pulseBackgroundInitialColor;
            if (pulseNoKill)
            {
                float colorMult = 2.5f;
                pulseBar.color = new Color(pulseBar.color.r / colorMult, pulseBar.color.g / colorMult, pulseBar.color.b / colorMult);
                pulseBackgroundImage.color = new Color(pulseBackgroundImage.color.r / colorMult, pulseBackgroundImage.color.g / colorMult, pulseBackgroundImage.color.b / colorMult);
            }
            else
            {
                Box.pulseUnlocked = true;
            }
        }
    }

    IEnumerator DashBarRefill()
    {
        dashBarRefill = true;
        float window = boxScript.dashCooldown;
        float timer = 0;
        float dashBarWidth = 0;
        while (timer <= window)
        {
            window = boxScript.dashCooldown;
            dashBarWidth = (timer / window) * dashBarInitialWidth;
            dashBar.transform.localScale = new Vector2(dashBarWidth, dashBar.transform.localScale.y);
            timer += Time.deltaTime;
            yield return null;
        }
        dashBar.transform.localScale = new Vector2(dashBarInitialWidth, dashBar.transform.localScale.y);
        while (Box.dashActive)
        {
            yield return null;
        }
        dashBarRefill = false;
    }
    IEnumerator DashGroundedCheck()
    {
        dashNotGrounded = true;
        while (Box.dashActive)
        {
            yield return null;
        }
        while (Box.isGrounded == false)
        {
            if (Box.canDash)
            {
                break;
            }
            yield return null;
        }
        dashNotGrounded = false;
    }
    IEnumerator TeleportBarRefill()
    {
        teleportBarRefill = true;
        float window = boxScript.teleportCooldown;
        float timer = 0;
        float teleportBarWidth = 0;
        while (Box.teleportActive)
        {
            yield return null;
        }
        while (timer <= window)
        {
            window = boxScript.teleportCooldown;
            teleportBarWidth = (timer / window) * teleportBarInitialWidth;
            teleportBar.transform.localScale = new Vector2(teleportBarWidth, teleportBar.transform.localScale.y);
            timer += Time.deltaTime;
            yield return null;
        }
        teleportBar.transform.localScale = new Vector2(teleportBarInitialWidth, teleportBar.transform.localScale.y);
        teleportBarRefill = false;
    }
    IEnumerator PulseBarRefill()
    {
        pulseBarRefill = true;
        float window = boxScript.pulseCooldown;
        float timer = 0;
        float pulseBarWidth = 0;
        while (timer <= window)
        {
            window = boxScript.pulseCooldown;
            pulseBarWidth = (timer / window) * pulseBarInitialWidth;
            pulseBar.transform.localScale = new Vector2(pulseBarWidth, pulseBar.transform.localScale.y);
            timer += Time.deltaTime;
            yield return null;
        }
        pulseBar.transform.localScale = new Vector2(pulseBarInitialWidth, pulseBar.transform.localScale.y);
        pulseBarRefill = false;
    }

    IEnumerator CycleSelect()
    {
        cycleCRActive = true;
        int direction = -(int) Mathf.Sign(UIInputs.leftStick.y);
        currentSelection += direction;
        float window1 = 0.3f;
        float timer = 0;
        while ((UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8) && timer <= window1)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        float window2 = 0.12f;
        timer = 0;
        while (UIInputs.leftStick.y > 0.8 || UIInputs.leftStick.y < -0.8)
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
    IEnumerator SelectionCR()
    {
        selectionCRActive = true;
        int selectionActive = currentSelection;
        GameObject option;
        if (selectionActive == 1)
        {
            option = option1;
        }
        else if (selectionActive == 2)
        {
            option = option2;
        }
        else
        {
            option = option3;
        }
        Color color = option.GetComponent<Image>().color;
        Color newColor = new Color(color.r, color.g, color.b, 1);
        option.GetComponent<Image>().color = newColor;
        option.transform.localScale *= 1.2f;
        arrow.rectTransform.position = new Vector2(initialArrowX, option.transform.position.y);
        float initialPositionX = arrow.rectTransform.position.x;
        IEnumerator CR = UITools.SelectArrowOsc(arrow, true, true);
        StartCoroutine(CR);
        while (selectionActive == currentSelection)
        {
            yield return null;
        }
        arrow.rectTransform.position = new Vector2(initialPositionX, arrow.rectTransform.position.y);
        StopCoroutine(CR);
        option.transform.localScale /= 1.2f;
        option.GetComponent<Image>().color = color;
        selectionCRActive = false;
    }
    IEnumerator InitialDelay()
    {
        dash.SetActive(false);
        pulse.SetActive(false);
        teleport.SetActive(false);
        yield return null;
        if (initialHealth == 0)
        {
            initialHealth = 250;
        }
        if (Box.boxHealth < initialHealth)
        {
            lostHealth = Instantiate(healthIncrement, healthText.transform);
            lostHealth.text = ((int)Mathf.Ceil(Box.boxHealth) - initialHealth).ToString();
        }
        if (Box.dashUnlocked)
        {
            dash.SetActive(true);
        }
        if (Box.pulseUnlocked)
        {
            pulse.SetActive(true);
        }
        if (Box.teleportUnlocked)
        {
            teleport.SetActive(true);
        }
        spawnIncrement = true;
    }
    IEnumerator DisableInputs()
    {
        pauseWaitFinished = false;
        yield return new WaitForSecondsRealtime(0.05f);
        pauseWaitFinished = true;
    }
    IEnumerator StarPower()
    {
        starPowerCR = true;
        starPowerColor = new Color(1, 0.5f, 0.5f);
        float minValue = 0.3f;
        float colorChangeSpeed = 5;
        while (starPowerColor.g < 1 && BoxPerks.starActive)
        {
            starPowerColor.g += colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.g = 1;
        while (starPowerColor.r > minValue && BoxPerks.starActive)
        {
            starPowerColor.r -= colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.r = minValue;
        while (starPowerColor.b < 1 && BoxPerks.starActive)
        {
            starPowerColor.b += colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.b = 1;
        while (starPowerColor.g > minValue && BoxPerks.starActive)
        {
            starPowerColor.g -= colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.g = minValue;
        while (starPowerColor.r < 1 && BoxPerks.starActive)
        {
            starPowerColor.r += colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.r = 1;
        while (starPowerColor.b > minValue && BoxPerks.starActive)
        {
            starPowerColor.b -= colorChangeSpeed * Time.deltaTime;
            yield return null;
        }
        starPowerColor.b = minValue;
        if (BoxPerks.starActive)
        {
            StartCoroutine(StarPower());
        }
        else
        {
            starPowerCR = false;
        }
    }
}