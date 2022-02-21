using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    EpisodeManager episodeManager;
    public Rigidbody2D doorTop;
    public Rigidbody2D doorBottom;
    // Start is called before the first frame update
    void Start()
    {
        episodeManager = FindObjectOfType<EpisodeManager>();
        doorTop.freezeRotation = true;
        doorBottom.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            episodeManager.episodeComplete = true;
            doorTop.freezeRotation = false;
            doorBottom.freezeRotation = false;
        }
    }
}
