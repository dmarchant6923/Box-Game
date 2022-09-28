using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    InputBroker cannonInputs;
    Rigidbody2D boxRB;
    Transform cannon;
    public GameObject explosion;
    GameObject newExplosion;

    public bool rotate = true;
    public bool controlRotation = true;
    public bool autoShoot = false;
    public float autoShootTimer = 1f;
    public float launchVel = 55;
    public Vector2 angleLimits = new Vector2(0, -90); //higher, lower. Using unity's coordinate system (0 at y+, pos. rotation CCW). Must be less than 180 degrees apart
    public bool horizControls = true;
    public float rotateVel = 70f;
    float launchAngle;
    Vector3 launchVector;

    bool active = false;

    void Start()
    {
        cannon = transform.GetChild(0);
        boxRB = GameObject.Find("Box").GetComponent<Rigidbody2D>();
        cannonInputs = GetComponent<InputBroker>();
        cannonInputs.enabled = false;
    }

    void Update()
    {
        if (active)
        {
            launchAngle = cannon.eulerAngles.z;
            boxRB.position = transform.GetChild(0).GetChild(0).GetChild(0).transform.position;
            boxRB.velocity = Vector2.zero;
            BoxVelocity.velocitiesX[0] = 0;
            if (rotate && controlRotation)
            {
                if ((cannonInputs.leftStick.x > 0.2f && horizControls) || (cannonInputs.leftStick.y > 0.2f && horizControls == false))
                {
                    cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.y, rotateVel * Time.deltaTime));
                }
                if ((cannonInputs.leftStick.x < -0.2f && horizControls) || (cannonInputs.leftStick.y < -0.2f && horizControls == false))
                {
                    cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.x, rotateVel * Time.deltaTime));
                }
            }
            if ((cannonInputs.attackButtonDown || cannonInputs.dashButtonDown || cannonInputs.teleportButtonDown || cannonInputs.pulseButtonDown || cannonInputs.dodgeButtonDown)
                && autoShoot == false)
            {
                StartCoroutine(Launch());
            }
        }
    }

    IEnumerator Activate()
    {
        float timer = 0.2f;
        StartCoroutine(boxRB.GetComponent<Box>().Invulnerability(0));
        yield return new WaitForSeconds(timer);
        cannonInputs.enabled = true;
        cannonInputs.inputsEnabled = true;
        if (rotate && controlRotation == false)
        {
            StartCoroutine(AutoRotate());
        }
        if (autoShoot)
        {
            yield return new WaitForSeconds(autoShootTimer - timer);
            StartCoroutine(Launch());
        }
    }

    IEnumerator AutoRotate()
    {
        while (active)
        {
            while (Mathf.Abs(Mathf.DeltaAngle(angleLimits.y, cannon.eulerAngles.z)) > 0.5f && active)
            {
                cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.y, rotateVel * Time.deltaTime));
                yield return null;
            }
            cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.y, rotateVel * Time.deltaTime));
            while (Mathf.Abs(Mathf.DeltaAngle(angleLimits.x, cannon.eulerAngles.z)) > 0.5f && active)
            {
                cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.x, rotateVel * Time.deltaTime));
                yield return null;
            }
            cannon.eulerAngles = new Vector3(cannon.eulerAngles.x, cannon.eulerAngles.y, Mathf.MoveTowardsAngle(cannon.eulerAngles.z, angleLimits.x, rotateVel * Time.deltaTime));
            yield return null;
        }
    }

    IEnumerator Launch()
    {
        cannonInputs.inputsEnabled = false;
        launchVector = new Vector2(Mathf.Cos(launchAngle * Mathf.Deg2Rad + Mathf.PI / 2),
            Mathf.Sin(launchAngle * Mathf.Deg2Rad + Mathf.PI / 2)).normalized * launchVel;
        newExplosion = Instantiate(explosion, transform.GetChild(0).GetChild(0).GetChild(0).transform.position + launchVector.normalized * 2f, Quaternion.identity);
        newExplosion.GetComponent<Explosion>().aestheticExplosion = true;
        newExplosion.GetComponent<Explosion>().explosionRadius = 1.5f;
        yield return null;
        BoxVelocity.velocitiesX[0] = launchVector.x;
        boxRB.velocity = new Vector2(boxRB.velocity.x, launchVector.y);
        active = false;
        cannonInputs.enabled = false;
        if (FindObjectOfType<CameraFollowBox>() != null)
        {
            FindObjectOfType<CameraFollowBox>().StartCameraShake(10,4);
        }
        yield return null;
        Box.forceEndInvul = true;
        StartCoroutine(boxRB.GetComponent<Box>().ReduceDrag());
        int frames = 0;
        float window = 0.02f * launchVel;
        float timer = 0;
        while (timer < window && Box.damageActive == false && Box.boxEnemyPulseActive == false)
        {
            if (boxRB.velocity.magnitude < 25 && Box.enemyHitstopActive == false)
            {
                frames++;
            }
            else
            {
                frames = 0;
            }
            if (frames > 2)
            {
                break;
            }
            if (Box.enemyHitstopActive == false)
            {
                boxRB.angularVelocity = Mathf.Sign(launchVector.x) * -1500;
                timer += Time.deltaTime;
            }
            yield return null;
        }
        Box.endForcedDisable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Box>() != null && Box.damageActive == false && active == false)
        {
            StartCoroutine(collision.GetComponent<Box>().DisableInputs(0));
            StartCoroutine(Activate());
            active = true;
            BoxVelocity.velocitiesX[0] = 0;
            boxRB.angularVelocity = 0;
        }
    }
}
