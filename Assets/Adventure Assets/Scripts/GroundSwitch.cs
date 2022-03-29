using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSwitch : MonoBehaviour
{
    Switch switchScript;
    [HideInInspector] public bool active = false;

    Color activeColor = new Color(1, 1, 0);
    Color inactiveColor = new Color(0.5f, 0.5f, 0);

    bool onPlatform = false;

    public bool stayActive = false;
    public bool triggerOnEnter = false;
    public bool triggerOnExit = false;

    public bool externalOnlyActivates = true;

    public bool removeIfInactive = true;
    GameObject line;
    Collider2D colBox;
    SpriteRenderer rend;


    // Start is called before the first frame update
    void Start()
    {
        switchScript = GetComponent<Switch>();

        line = transform.GetChild(0).gameObject;
        colBox = GetComponent<BoxCollider2D>();
        rend = GetComponent<SpriteRenderer>();
        line.SetActive(false);

        active = false;
        if (switchScript.startActive)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            if (triggerOnExit && onPlatform == true)
            {
                Trigger(false);
            }
            onPlatform = false;
        }
        if (Box.isGrounded && onPlatform == false)
        {
            if (triggerOnEnter)
            {
                Trigger(false);
            }
            onPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            if (triggerOnExit && onPlatform == true)
            {
                Trigger(false);
            }
            onPlatform = false;
        }
    }
    public void Trigger(bool external)
    {
        if (active && stayActive == false && ((externalOnlyActivates == false && external == true) || external == false))
        {
            Deactivate();
        }
        else if (active == false)
        {
            Activate();
        }
    }

    void Activate()
    {
        active = true;
        if (removeIfInactive)
        {
            line.SetActive(false);
            rend.enabled = true;
            colBox.enabled = true;
        }
        else
        {
            rend.color = activeColor;
        }
        switchScript.Activate();
    }

    void Deactivate()
    {
        active = false;
        if (removeIfInactive)
        {
            line.SetActive(true);
            rend.enabled = false;
            colBox.enabled = false;
        }
        else
        {
            rend.color = inactiveColor;
        }
        switchScript.Deactivate();
    }
}
