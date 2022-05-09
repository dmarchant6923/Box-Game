using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{
    Rigidbody2D boxRB;
    Vector2 center;
    Vector2 halfExtents;

    // Start is called before the first frame update
    void Start()
    {
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();

        center = transform.position;
        //halfExtents = new Vector2(transform.lossyScale.x / 2, transform.lossyScale.y / 2);
        halfExtents = new Vector2(GetComponent<BoxCollider2D>().size.x * transform.lossyScale.x / 2, GetComponent<BoxCollider2D>().size.y * transform.lossyScale.y / 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        bool teleport = true;
        if (collision.GetComponent<Box>() != null)
        {
            if (Box.boxHealth > 0 && BoxPerks.shieldActive == false && BoxPerks.starActive == false)
            {
                if (BoxPerks.heavyActive)
                {
                    Box.boxHealth -= 2 * BoxPerks.heavyMult;
                }
                else
                {
                    Box.boxHealth -= 2;
                }
            }
            if (Box.boxHealth <= 0)
            {
                teleport = false;
            }
            if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
            {
                StartCoroutine(CameraFocus());
            }
        }
        if (teleport && GetComponent<EnemyManager>() == null)
        {
            Vector2 difference = new Vector2(collision.transform.position.x - center.x, collision.transform.position.y - center.y);
            if (difference.x >= halfExtents.x)
            {
                collision.transform.position = new Vector2(center.x - halfExtents.x + Mathf.Abs(collision.transform.position.x - (center.x + halfExtents.x)), collision.transform.position.y);
            }
            if (difference.x <= -halfExtents.x)
            {
                collision.transform.position = new Vector2(center.x + halfExtents.x - Mathf.Abs(collision.transform.position.x - (center.x - halfExtents.x)), collision.transform.position.y);
            }

            if (difference.y >= halfExtents.y)
            {
                collision.transform.position = new Vector2(collision.transform.position.x, center.y - halfExtents.y + Mathf.Abs(collision.transform.position.y - (center.y + halfExtents.y)));
            }
            if (difference.y <= -halfExtents.y)
            {
                collision.transform.position = new Vector2(collision.transform.position.x, center.y + halfExtents.y - Mathf.Abs(collision.transform.position.y - (center.y - halfExtents.y)));
            }
        }
        if (collision.GetComponent<EnemyManager>() != null)
        {
            Rigidbody2D enemyRB = collision.GetComponent<EnemyManager>().enemyRB;
            Vector2 difference = new Vector2(enemyRB.position.x - center.x, enemyRB.position.y - center.y);
            if (difference.x >= halfExtents.x)
            {
                enemyRB.position = new Vector2(center.x - halfExtents.x + Mathf.Abs(enemyRB.position.x - (center.x + halfExtents.x)), enemyRB.position.y);
            }
            if (difference.x <= -halfExtents.x)
            {
                enemyRB.position = new Vector2(center.x + halfExtents.x - Mathf.Abs(enemyRB.position.x - (center.x - halfExtents.x)), enemyRB.position.y);
            }

            if (difference.y >= halfExtents.y)
            {
                enemyRB.position = new Vector2(enemyRB.position.x, center.y - halfExtents.y + Mathf.Abs(enemyRB.position.y - (center.y + halfExtents.y)));
            }
            if (difference.y <= -halfExtents.y)
            {
                enemyRB.position = new Vector2(enemyRB.position.x, center.y + halfExtents.y - Mathf.Abs(enemyRB.position.y - (center.y - halfExtents.y)));
            }
        }
        if (collision.GetComponent<BulletScript>() != null)
        {
            Destroy(collision.gameObject);
        }
    }

    IEnumerator CameraFocus()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        GameObject.Find("Main Camera").GetComponent<CameraFollowBox>().RefocusBox();
        yield return null;
    }
}
