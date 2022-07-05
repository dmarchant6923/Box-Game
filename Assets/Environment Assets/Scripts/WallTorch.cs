using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallTorch : MonoBehaviour
{
    public GameObject fire;
    GameObject newFire;

    public bool side = false;

    private void Start()
    {
        if (side == false)
        {
            newFire = Instantiate(fire, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }
        else
        {
            int direction = (int)Mathf.Sign(transform.eulerAngles.y - 90);
            newFire = Instantiate(fire, transform.position + new Vector3(0.2f * direction, 0.6f, 0), Quaternion.identity);
        }
        newFire.GetComponent<Fire>().hazardFire = true;
        newFire.GetComponent<Fire>().fireWindow = 0;
    }
}
