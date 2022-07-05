using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public bool door;
    public bool cannon;

    EpisodeManager episodeManager;
    public Rigidbody2D doorTop;
    public Rigidbody2D doorBottom;
    // Start is called before the first frame update
    void Start()
    {
        episodeManager = FindObjectOfType<EpisodeManager>();

        if (door)
        {
            doorTop.isKinematic = true;
            doorBottom.isKinematic = true;
            //doorTop.freezeRotation = true;
            //doorBottom.freezeRotation = true;
        }
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
            if (door)
            {
                doorTop.isKinematic = false;
                doorBottom.isKinematic = false;
                //doorTop.freezeRotation = false;
                //doorBottom.freezeRotation = false;
            }
            FindObjectOfType<CameraFollowBox>().overridePosition = true;
            if (door)
            {
                FindObjectOfType<CameraFollowBox>().forcedPosition = transform.position;
            }
            else if (cannon)
            {
                FindObjectOfType<CameraFollowBox>().forcedPosition = transform.position + Tools.AngleToVector3(transform.GetChild(0).eulerAngles.z) * 7;
            }
            FindObjectOfType<CameraFollowBox>().ResetCamera();
        }
    }
}
