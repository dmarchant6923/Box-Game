using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITools : MonoBehaviour
{
    InputBroker UIInput;
    [HideInInspector] public bool actionButton;
    [HideInInspector] public bool backButton;

    public void Start()
    {
        UIInput = GetComponent<InputBroker>();
    }
    public void Update()
    {
        if (InputBroker.Keyboard)
        {
            if (UIInput.startButtonDown || UIInput.attackButtonDown)
            {
                actionButton = true;
            }
            else
            {
                actionButton = false;
            }

            if (UIInput.teleportButtonDown || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
            {
                backButton = true;
            }
            else
            {
                backButton = false;
            }
        }

        else if (InputBroker.GameCube)
        {
            if (UIInput.startButtonDown || Input.GetButtonDown("Xbox A") || Input.GetButtonDown("GC A"))
            {
                actionButton = true;
            }
            else
            {
                actionButton = false;
            }
        }

        else if (InputBroker.Xbox) 
        { 
            if (Input.GetButtonDown("Xbox B") || Input.GetButtonDown("GC B"))
            {
                backButton = true;
            }
            else
            {
                backButton = false;
            }
        }
    }

    public string RawTimeToString(float rawTime, bool includeMinutes)
    {
        rawTime = Mathf.Floor(rawTime * 100) / 100;
        int minutes = (int)Mathf.Floor(rawTime / 60);
        int seconds = (int)Mathf.Floor(rawTime - (minutes * 60));
        int centiseconds = (int)((rawTime - Mathf.Floor(rawTime)) * 100);
        string returnString;
        if (includeMinutes)
        {
            returnString = minutes + ":" + seconds.ToString("D2") + ":" + centiseconds.ToString("D2");
        }
        else
        {
            returnString = seconds.ToString("D1") + ":" + centiseconds.ToString("D2");
        }
        return returnString;
    }
    public IEnumerator SelectArrowOsc(Image arrow, bool horizontal, bool initUporRight)
    {
        Vector2 initialPosition = arrow.rectTransform.position;
        float period = 1;
        float distance = 10;
        if (initUporRight == false)
        {
            distance *= -1;
        }
        float timer = 0;
        if (horizontal)
        {
            while (true)
            {
                arrow.rectTransform.position = new Vector2(initialPosition.x + distance * Mathf.Sin(timer * 2 * Mathf.PI / period) / period,
                    arrow.rectTransform.position.y);
                if (timer >= period)
                {
                    arrow.rectTransform.position = new Vector2(initialPosition.x, arrow.rectTransform.position.y);
                    timer -= period;
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        else
        {
            while (true)
            {
                arrow.rectTransform.position = new Vector2(arrow.rectTransform.position.x,
                    initialPosition.y + distance * Mathf.Sin(timer * 2 * Mathf.PI / period) / period);
                if (timer >= period)
                {
                    arrow.rectTransform.position = new Vector2(arrow.rectTransform.position.x, initialPosition.y);
                    timer -= period;
                }
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}
