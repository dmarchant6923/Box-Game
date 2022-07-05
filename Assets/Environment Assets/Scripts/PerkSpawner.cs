using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkSpawner : MonoBehaviour
{
    Transform pivotLeft;
    Transform pivotRight;
    Transform glow;

    Vector3 pivotLeftPos;
    Vector3 pivotRightPos;

    public GameObject heart;
    public GameObject heavy;
    public GameObject shield;
    public GameObject speed;
    public GameObject doubleJump;
    public GameObject spikes;
    public GameObject star;

    public bool spawnHeart = true;
    public bool spawnHeavy = true;
    public bool spawnShield = true;
    public bool spawnSpeed = true;
    public bool spawnDoubleJump = false;
    public bool spawnSpikes = false;
    public bool spawnStar = false;

    List<GameObject> perks = new List<GameObject>();

    public float heartHealth = 0;
    public bool singleDoubleJump = true;
    public bool despawnPerk = true;

    Rigidbody2D rb;
    bool objectFound = false;
    Transform attachedObject;
    Vector3 relativePosition;

    bool activateCR = false;

    int obstacleLM;
    int groundLM;

    private void Start()
    {
        pivotLeft = transform.GetChild(0);
        pivotRight = transform.GetChild(1);
        glow = transform.GetChild(2);

        glow.gameObject.SetActive(false);
        rb = GetComponent<Rigidbody2D>();

        if (spawnHeart) { perks.Add(heart); }
        if (spawnHeavy) { perks.Add(heavy); }
        if (spawnShield) { perks.Add(shield); }
        if (spawnSpeed) { perks.Add(speed); }
        if (spawnDoubleJump) { perks.Add(doubleJump); }
        if (spawnSpikes) { perks.Add(spikes); }
        if (spawnStar) { perks.Add(star); }

        obstacleLM = LayerMask.GetMask("Obstacles");
        groundLM = LayerMask.GetMask("Obstacles", "Platforms");
    }

    private void Update()
    {
        if (objectFound)
        {
            transform.position = attachedObject.position + relativePosition;
            if (activateCR == false)
            {
                StartCoroutine(SpawnPerk());
            }
            else if (pivotLeft.parent == null)
            {
                pivotLeft.transform.position = transform.position + pivotLeftPos;
                pivotRight.transform.position = transform.position + pivotRightPos;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (objectFound == false && (1 << collision.gameObject.layer == obstacleLM || 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms")))
        {
            float dist = 0.75f;
            RaycastHit2D groundRC = Physics2D.Raycast(transform.position, Vector2.down, dist, groundLM);
            RaycastHit2D wallLeftRC = Physics2D.Raycast(transform.position, Vector2.left, dist, obstacleLM);
            RaycastHit2D wallRightRC = Physics2D.Raycast(transform.position, Vector2.right, dist, obstacleLM);
            RaycastHit2D ceilingRC = Physics2D.Raycast(transform.position, Vector2.up, dist, obstacleLM);
            if (groundRC.collider != null)
            {
                transform.position = groundRC.point + Vector2.up * transform.lossyScale.y / 4;
                attachedObject = groundRC.transform;
                relativePosition = transform.position - attachedObject.position;
                rb.rotation = Tools.VectorToAngle(Vector2.up);
            }

            else if (wallRightRC.collider != null)
            {
                transform.position = wallRightRC.point + Vector2.left * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (90);
                attachedObject = wallRightRC.transform;
                relativePosition = transform.position - attachedObject.position;
                rb.rotation = Tools.VectorToAngle(Vector2.left);
            }
            else if (ceilingRC.collider != null)
            {
                transform.position = ceilingRC.point + Vector2.down * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (180);
                attachedObject = ceilingRC.transform;
                relativePosition = transform.position - attachedObject.position;
                rb.rotation = Tools.VectorToAngle(Vector2.down);
            }
            else if (wallLeftRC.collider != null)
            {
                transform.position = wallLeftRC.point + Vector2.right * transform.lossyScale.y / 4;
                transform.eulerAngles = Vector3.forward * (-90);
                attachedObject = wallLeftRC.transform;
                relativePosition = transform.position - attachedObject.position;
                rb.rotation = Tools.VectorToAngle(Vector2.right);
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
                    RaycastHit2D cast = Physics2D.Raycast(rb.position, vector, distance, groundLM);
                    if (vector.y > 0)
                    {
                        cast = Physics2D.Raycast(rb.position, vector, distance, obstacleLM);
                    }
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
            if (objectFound)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
            }
        }
    }

    IEnumerator SpawnPerk()
    {
        activateCR = true;
        yield return new WaitForSeconds(0.25f);
        pivotLeft.parent = null;
        pivotRight.parent = null;
        pivotLeftPos = pivotLeft.position - transform.position;
        pivotRightPos = pivotRight.position - transform.position;
        float changeSpeed = 250;
        float timer = 0;
        float initialZ = pivotLeft.eulerAngles.z;
        while (timer < 145 / changeSpeed)
        {
            pivotLeft.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(pivotLeft.eulerAngles.z, initialZ + 145, changeSpeed * Time.deltaTime));
            pivotRight.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(pivotRight.eulerAngles.z, initialZ - 145, changeSpeed * Time.deltaTime));
            timer += Time.deltaTime;
            yield return null;
        }
        pivotLeft.eulerAngles = new Vector3(0, 0, initialZ + 145);
        pivotRight.eulerAngles = new Vector3(0, 0, initialZ - 145);
        yield return new WaitForSeconds(0.25f);
        glow.gameObject.SetActive(true);
        int rand = Random.Range(0, perks.Count);
        GameObject perk = perks[rand];
        GameObject newPerk = Instantiate(perk, rb.position + Tools.AngleToVector(transform.eulerAngles.z) * 1.5f, Quaternion.identity);
        newPerk.transform.parent = transform;
        newPerk.GetComponent<MovingObjects>().enabled = false;
        if (perk == doubleJump && singleDoubleJump)
        {
            perk.GetComponent<perks>().unlimitedJumps = false;
        }
        else if (perk == heart && heartHealth != 0)
        {
            perk.GetComponent<perks>().amountHealed = heartHealth;
        }
        if (despawnPerk == false)
        {
            newPerk.GetComponent<perks>().willDespawn = false;
        }
        timer = 0;
        while (newPerk != null)
        {
            newPerk.transform.position = rb.position + Tools.AngleToVector(transform.eulerAngles.z) * (1.5f + (0.2f * Mathf.Sin(timer * 2)));


            if (newPerk.GetComponent<SpriteRenderer>().enabled && glow.GetComponent<SpriteRenderer>().enabled == false)
            {
                glow.GetComponent<SpriteRenderer>().enabled = true;
            }
            if (newPerk.GetComponent<SpriteRenderer>().enabled == false && glow.GetComponent<SpriteRenderer>().enabled)
            {
                glow.GetComponent<SpriteRenderer>().enabled = false;
            }

            timer += Time.deltaTime;
            yield return null;
        }
        glow.gameObject.SetActive(false);
        timer = 0;
        while (timer < 145 / changeSpeed)
        {
            pivotLeft.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(pivotLeft.eulerAngles.z, initialZ, changeSpeed * Time.deltaTime));
            pivotRight.eulerAngles = new Vector3(0, 0, Mathf.MoveTowardsAngle(pivotRight.eulerAngles.z, initialZ, changeSpeed * Time.deltaTime));
            timer += Time.deltaTime;
            yield return null;
        }
        pivotLeft.eulerAngles = new Vector3(0, 0, initialZ);
        pivotRight.eulerAngles = new Vector3(0, 0, initialZ);
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
