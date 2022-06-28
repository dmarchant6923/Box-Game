using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkSpawner : MonoBehaviour
{
    public Transform pivotLeft;
    Vector3 pivotLeftPos;
    public Transform pivotRight;
    Vector3 pivotRightPos;
    public Transform glow;
    public Transform mask;

    public GameObject heart;
    public GameObject heavy;
    public GameObject shield;
    public GameObject speed;

    Rigidbody2D rb;
    bool objectFound = false;
    Transform attachedObject;
    Vector3 relativePosition;

    bool activateCR = false;

    int obstacleLM;
    int groundLM;

    private void Start()
    {
        glow.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody2D>();

        pivotLeftPos = pivotLeft.position - transform.position;
        pivotRightPos = pivotRight.position - transform.position;

        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
    }

    private void Update()
    {
        if (objectFound)
        {
            rb.velocity = Vector2.zero;
            transform.position = attachedObject.position + relativePosition;
            if (activateCR == false)
            {
                StartCoroutine(SpawnPerk());
            }
            else
            {
                pivotLeft.position = transform.position + pivotLeftPos;
                pivotRight.position = transform.position + pivotRightPos;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (objectFound == false && (1 << collision.gameObject.layer == obstacleLM || 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")))
        {
            RaycastHit2D groundRC = Physics2D.Raycast(transform.position, Vector2.down, 3, groundLM);
            RaycastHit2D wallLeftRC = Physics2D.Raycast(transform.position, Vector2.left, 1, obstacleLM);
            RaycastHit2D wallRightRC = Physics2D.Raycast(transform.position, Vector2.right, 1, obstacleLM);
            RaycastHit2D ceilingRC = Physics2D.Raycast(transform.position, Vector2.up, 1, obstacleLM);
            if (groundRC.collider != null)
            {
                transform.position = groundRC.point + Vector2.up * transform.lossyScale.y / 4;
                attachedObject = groundRC.transform;
                relativePosition = transform.position - attachedObject.position;
            }

            else if (wallRightRC.collider != null)
            {
                transform.position = wallRightRC.point + Vector2.left * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (90);
                attachedObject = wallRightRC.transform;
                relativePosition = transform.position - attachedObject.position;
            }
            else if (ceilingRC.collider != null)
            {
                transform.position = ceilingRC.point + Vector2.down * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (180);
                attachedObject = ceilingRC.transform;
                relativePosition = transform.position - attachedObject.position;
            }
            else if (wallLeftRC.collider != null)
            {
                transform.position = wallLeftRC.point + Vector2.right * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (-90);
                attachedObject = wallLeftRC.transform;
                relativePosition = transform.position - attachedObject.position;
            }

            if (attachedObject == null)
            {
                objectFound = false;
                rb.isKinematic = false;
            }
            else
            {
                objectFound = true;
                rb.isKinematic = true;
            }


            if (objectFound == false)
            {
                for (int i = 0; i < 17; i++)
                {
                    float angle = i * 22.5f;
                    float distance = 0.6f;
                    Vector2 vector = Tools.AngleToVector(angle);
                    RaycastHit2D cast = Physics2D.Raycast(rb.position, vector, distance, LayerMask.GetMask("Obstacles", "Platforms", "Enemies"));
                    if (cast.collider != null)
                    {
                        rb.rotation = Tools.VectorToAngle(cast.normal);
                        rb.position = cast.point + cast.normal * transform.lossyScale.y / 4;
                        transform.position = rb.position;
                        attachedObject = cast.collider.transform;
                        relativePosition = transform.position - attachedObject.position;
                        objectFound = true;
                        rb.isKinematic = true;
                        break;
                    }
                }
            }
        }
    }

    IEnumerator SpawnPerk()
    {
        activateCR = true;
        pivotLeft.parent = null;
        pivotRight.parent = null;
        yield return new WaitForSeconds(0.25f);
        float changeSpeed = 250;
        while (pivotLeft.eulerAngles.z < 145)
        {
            pivotLeft.eulerAngles = new Vector3(0, 0, Mathf.MoveTowards(pivotLeft.eulerAngles.z, 145, changeSpeed * Time.deltaTime));
            pivotRight.eulerAngles = new Vector3(0, 0, Mathf.MoveTowards(pivotRight.eulerAngles.z, -145, changeSpeed * Time.deltaTime));
            yield return null;
        }
        pivotLeft.eulerAngles = new Vector3(0, 0, 145);
        pivotRight.eulerAngles = new Vector3(0, 0, -145);
        yield return new WaitForSeconds(0.25f);
        glow.gameObject.SetActive(true);
        int rand = Random.Range(0, 4);
        GameObject perk = heart;
        if (rand == 1) { perk = heavy; }
        if (rand == 2) { perk = shield; }
        if (rand == 3) { perk = speed; }
        GameObject newPerk = Instantiate(perk, rb.position + Tools.AngleToVector(transform.eulerAngles.z) * 1.5f, Quaternion.identity);
        newPerk.transform.parent = transform;
        newPerk.GetComponent<MovingObjects>().enabled = false;
        while (newPerk != null)
        {
            if (newPerk.GetComponent<SpriteRenderer>().enabled && glow.GetComponent<SpriteRenderer>().enabled == false)
            {
                glow.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (newPerk.GetComponent<SpriteRenderer>().enabled == false && glow.GetComponent<SpriteRenderer>().enabled)
            {
                glow.GetComponent<SpriteRenderer>().enabled = false;
            }
            yield return null;
        }
        glow.gameObject.SetActive(false);
        while (pivotLeft.eulerAngles.z > 0)
        {
            pivotLeft.eulerAngles = new Vector3(0, 0, Mathf.MoveTowards(pivotLeft.eulerAngles.z, 0, changeSpeed * Time.deltaTime));
            pivotRight.eulerAngles = new Vector3(0, 0, pivotRight.eulerAngles.z + changeSpeed * Time.deltaTime);
            yield return null;
        }
        pivotLeft.eulerAngles = Vector3.zero;
        pivotRight.eulerAngles = Vector3.zero;
        yield return new WaitForSeconds(3f);
        changeSpeed = 4;
        while (GetComponent<SpriteRenderer>().color.a > 0)
        {
            Color color = pivotLeft.GetComponentInChildren<SpriteRenderer>().color;
            color.a -= changeSpeed * Time.deltaTime;
            pivotLeft.GetComponentInChildren<SpriteRenderer>().color = color;

            color = pivotRight.GetComponentInChildren<SpriteRenderer>().color;
            color.a -= changeSpeed * Time.deltaTime;
            pivotRight.GetComponentInChildren<SpriteRenderer>().color = color;

            foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                color = sprite.color;
                color.a -= changeSpeed * Time.deltaTime;
                sprite.color = color;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
