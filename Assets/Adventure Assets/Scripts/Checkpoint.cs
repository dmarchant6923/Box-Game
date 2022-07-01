using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    EpisodeManager episodeManager;
    Rigidbody2D boxRB;
    Transform lights;

    public bool checkpointActive = false;
    public float checkpointTime;
    void Start()
    {
        episodeManager = FindObjectOfType<EpisodeManager>();
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        lights = transform.GetChild(3);
        if (checkpointActive)
        {
            foreach (SpriteRenderer light in lights.GetComponentsInChildren<SpriteRenderer>())
            {
                light.color = Color.green;
            }
        }

        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (checkpointActive == false && 1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            episodeManager.CheckpointActivated();
            checkpointActive = true;
            StartCoroutine(CheckpointSpin());
        }
    }

    public void DeactivateCheckpoint()
    {
        foreach (SpriteRenderer light in lights.GetComponentsInChildren<SpriteRenderer>())
        {
            light.color = Color.red;
        }
    }

    IEnumerator CheckpointSpin()
    {
        Transform bars = transform.GetChild(2);
        Rigidbody2D barsRB = bars.GetComponent<Rigidbody2D>();

        Vector2 directionToBox = (boxRB.position - barsRB.position).normalized;
        Vector2 perpVector = Vector2.Perpendicular(directionToBox);
        int spinDirection = 1; //1 meaning CCW, -1 meaning CW
        if (Vector2.Dot(boxRB.velocity.normalized, perpVector) < 0)
        {
            spinDirection = -1;
        }
        barsRB.angularVelocity = Mathf.Abs(100 + 35 * boxRB.velocity.magnitude) * spinDirection;
        
        int numFlashes = 0;
        float waitTime = 0.2f;
        while (numFlashes <= 3)
        {
            if (checkpointActive)
            {
                foreach (SpriteRenderer light in lights.GetComponentsInChildren<SpriteRenderer>())
                {
                    light.color = Color.green;
                }
            }
            yield return new WaitForSeconds(waitTime);
            foreach (SpriteRenderer light in lights.GetComponentsInChildren<SpriteRenderer>())
            {
                light.color = Color.red;
            }
            yield return new WaitForSeconds(waitTime/2);
            numFlashes++;
        }
        if (checkpointActive)
        {
            foreach (SpriteRenderer light in lights.GetComponentsInChildren<SpriteRenderer>())
            {
                light.color = Color.green;
            }
        }
    }
}
