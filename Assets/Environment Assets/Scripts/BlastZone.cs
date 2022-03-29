using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastZone : MonoBehaviour
{
    Box boxScript;
    public bool normalRestart = true;

    // Start is called before the first frame update
    void Start()
    {
        boxScript = GameObject.Find("Box").GetComponent<Box>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (1 << collision.gameObject.layer == LayerMask.GetMask("Box"))
        {
            Box.boxHealth = 0;
            if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
            {
                CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
                camScript.startCamShake = true;
                camScript.shakeInfo = new Vector2(20, 5);
            }
            if (normalRestart)
            {
                StartCoroutine(boxScript.BlastZoneRestart());
            }
        }
        if (collision.GetComponent<EnemyManager>() != null)
        {
            collision.GetComponent<EnemyManager>().startEnemyDeath = true;
        }
    }
}
