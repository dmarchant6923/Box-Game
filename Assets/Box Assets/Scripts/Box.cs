using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Box : MonoBehaviour
{
    InputBroker inputs;

    private Rigidbody2D rigidBody; //this box rigidbody properties
    private BoxCollider2D boxCollider; //this box boxcollider properties

    int groundLayerMask; //layermask for both platforms and obstacles
    int platformLayerMask; //layermask for platforms
    int attackLayerMask; //layermask for attackable objects (currently targets and enemies)
    int enemyLayerMask; //layermask for enemies
    int obstacleLayerMask;

    float initialGravityScale = 4; //keeps initial gravity scale so it can be modified and returned to normal
    float gravityScale = 4;
    [System.NonSerialized] public float gravityMult = 1;
    [System.NonSerialized] public static float windGravity = 0;

    [System.NonSerialized] public bool enteredBlastZone = false;
    float blastZoneRespawnWait = 1f; // delay before respawning after hitting blast zone
    [System.NonSerialized] public bool blastZoneCRActive = false; //blast zone coroutine, true if it's active
    int blastZoneLookingRight; //lookingRight value that gets frozen upon entering blast zone

    [System.NonSerialized] public static int lookingRight = 1; //1 if true, -1 if false
    [System.NonSerialized] public float horizMaxSpeed = 15; //max horizontal speed
    public float initialHorizMaxSpeed = 15;
    float crouchMaxSpeed = 4f; //max horizontal speed while crouching on the ground
    [System.NonSerialized] public float groundFriction = 50; //time based, friction value while grounded
    float initialGroundFriction;
    float crouchFriction = 100; //time based, friction value while grounded and crouching
    float crouchThreshold = -0.5f;
    [System.NonSerialized] public static bool isCrouching = false;
    float crouchScale = 0.6f;
    bool deactivateCrouch = false;
    float crouchAccel = 25;
    Vector2 originalScale;

    [System.NonSerialized] public static bool isGrounded;
    bool touchedGround;
    [System.NonSerialized] public static bool isOnIce = false;
    bool iceCRActive = false;
    [System.NonSerialized] public static RaycastHit2D groundRayCast; // boxcast used to determine isGrounded
    [System.NonSerialized] public static float groundVerticalVelocity;
    bool sticky = false;
    float stickyFriction;

    [System.NonSerialized] public float jumpSpeed = 15; //vertical jump velocity
    float groundTime = 0; //timer showing how long isGrounded has been true
    float shortJumpWindow = 0.12f; //window of time to let go of jump input to perform a short jump
    bool shortJumpEnum = false; // short jump coroutine, true if it's active
    [System.NonSerialized] public static bool groundedJump = false;
    [System.NonSerialized] public static bool bufferJump = false;

    bool startAirTimer = false; //whether or not the air timer is active
    [System.NonSerialized] public static float airTime; //timer showing how long isGrounded has been false

    [System.NonSerialized] public float djumpSpeed = 12; //vertical double jump velocity
    bool canDoubleJump = true; //whether or not a double jump is available
    bool doubleJumpUsed = false;
    [System.NonSerialized] public float maxFallSpeed = -20; //maximum fall speed and fast fall speed

    [System.NonSerialized] public float airFriction = 30; //time based, friction value while airborne
    [System.NonSerialized] public float initialAirFriction = 30;
    [System.NonSerialized] public float airAccel = 50; //time based, horizontal acceleration value when holding a direction while airborne
    [System.NonSerialized] public float initialAirAccel = 50;

    [System.NonSerialized] public static bool canWallJump = false; //whether or not a walljump is available
    bool touchingWall = false; //whether or not a wall is being touched
    float wallClingSpeed = -1.2f; //vertical speed while clinging onto a wall
    float wallClingFastFallSpeed = -8f; //max vertical speed while clinging onto a wall and holding down
    float wallJumpWindow = 0.15f; //leniency window to perform a wall jump after letting go of towards the wall
    float wallJumpTime = 10f; //timer used to to check if the walljump was performed within the wall jump window. defaults to 10.
    float postWallJumpTime = 0.1f;
    bool resetWallJumpTime = false;
    bool startPostWallJumpTimer = false;
    [System.NonSerialized] public static int wallJumpDirection; //1 or -1, similar to lookingRight
    int wallJumpCounter = 0; //counts number of walljumps performed before hitting the ground
    int wallJumpLimit = 6; //number of walljumps that will give a vertical boost.
    bool pressFromWall; //keycode that changes based on the side of the wall
    bool pressToWall; //keycode that changes based on the side of the wall
    [System.NonSerialized]public static bool holdToWallActive = false;
    float wallJumpExtraSpeed;

    [System.NonSerialized] public static bool ceilingCling = false;

    [System.NonSerialized] public static bool spinAttackActive = false;
    float attackTimer = 0;
    float timeToActivateAttack = 0.05f;
    float attackSpinSpeed = -3000; //angular velocity given upon spin attacking
    float attackSpinDamageSpeed = -1000; //angular velocity value above which will count as an attack
    float attackJumpSpeed = 3; //vertical jump upon activating spin attack while grounded

    [System.NonSerialized] public static bool teleportUnlocked = true;
    [System.NonSerialized] public static bool canTeleport = true; //whether or not activating teleport is possible
    public GameObject teleportCheck;
    GameObject newTeleportCheck;
    [System.NonSerialized] public static float teleportRange = 5; //distance teleported
    [HideInInspector] public static float teleportDistancex; //distance teleported multiplied by lookingRight or horizontal axis input
    [System.NonSerialized] public static bool teleportActive = false; //whether or not a teleport is currently active
    [System.NonSerialized] public static float teleportDelay = 0.5f; //how long a teleport takes after activating
    [System.NonSerialized] public float teleportCooldown = 2f;
    float teleportSpeedx = 0; //stored horizontal speed after activating teleport, then divided by an amount before being applied
    float teleportspeedy = 0; //stored vertical speed after activating teleport, then divided by an amount before being applied
    bool successfulTeleport; //whether or not the teleport was successful

    [System.NonSerialized] public static bool dashUnlocked = true;
    [System.NonSerialized] public static bool canDash = true; //whether or not a dash is available
    [System.NonSerialized] public float dashCooldown = 1f;
    [System.NonSerialized] public static bool dashActive = false; //whether or not the dash is currently active
    [System.NonSerialized] public float dashSpeed = 30; // horizontal dash speed
    float dashTimeLimit = 0.2f; //how long the dash will last
    int dashDirection = 1; //same as lookingRight but stores dash direction
    RaycastHit2D leftWallCheck;
    RaycastHit2D rightWallCheck;
    bool dashWallBounceActivate = false; // will turn on for a frame when a dash wallbounce becomes active to activate the coroutine
    bool wallBounceActive = false;
    float reboundTime = 0.4f; // how long the wall bounce lasts and inputs are disabled

    [System.NonSerialized] public static bool pulseUnlocked = true;
    bool canPulse = true;
    [System.NonSerialized] public float pulseCooldown = 3f;
    public GameObject pulseField;
    GameObject newPulseField;
    [System.NonSerialized] public static bool pulseActive = false;
    [System.NonSerialized] public static float pulseRadius = 5;
    [System.NonSerialized] public static float enemyPulseMagnitude = 10;
    [System.NonSerialized] public static float projectilePulseMagnitude = 1.2f;

    [System.NonSerialized] public static bool boxWasPulsed = false;
    bool boxEnemyPulseActive = false;
    [System.NonSerialized] public static float boxEnemyPulseMagnitude;
    [System.NonSerialized] public static Vector2 boxEnemyPulseDirection;

    [System.NonSerialized] public static float enemyHitstopDelay = 0.12f; //how long hitstop lasts for enemies
    [HideInInspector] public static float boxHitstopDelay;
    [HideInInspector] public static float boxHitstopDelayMult = 0.1f/30;
    [HideInInspector] public static float shockHitstopMult = 2.5f;
    [System.NonSerialized] public static bool enemyHitstopActive = false; //whether or not histop from hitting the enemy is currently active
    [System.NonSerialized] public static bool boxHitstopActive = false; //whether or not hitstop from the box getting hit is currently active
    float hitstopRotationSlowDown = 30; //used for the still rotating effect during hitstop

    [System.NonSerialized] public static bool boxHitboxActive = false;
    [System.NonSerialized] public static RaycastHit2D[] attackRayCast = new RaycastHit2D[0]; // boxcast used to detect targets, enemies, etc. that respond to attacks or attack the player.
    [HideInInspector] public static bool activateHitstop = false; //will turn on for a frame when hitstop becomes active to activate the CR
    bool extendHitstop = false;
    [HideInInspector] public static bool activateDamage = false;
    bool ignoreProjectileDamage = false;
    [HideInInspector] public static bool activateRebound = false;
    bool reboundActive = false;
    [HideInInspector] public static bool activatePushBack = false;
    [HideInInspector] public static float pushBackMagnitude = 3;
    Vector2 enemyPosition;

    [HideInInspector] public static float boxHealth;
    [HideInInspector] public static Vector2 boxDamageDirection;
    [System.NonSerialized] public static bool damageActive = false;
    [HideInInspector] public static bool isInvulnerable = false;
    float damageAirFrictionMult = 10;
    float damageTime = 0;
    bool isCurrentlyFlashing = false;
    bool canTech = false;
    bool techCRActive = false;
    bool techWindowActive = false;
    bool techSuccessful = false;
    bool techWalljump = false;
    [System.NonSerialized] public static float damageTaken;

    [System.NonSerialized] public static bool activateShock = false;
    [System.NonSerialized] public static bool shockActive = false;
    [System.NonSerialized] public static bool canBeShocked = true;
    public GameObject lightning;
    GameObject newLightning;
    [System.NonSerialized] public static bool inShockRadius = false;
    bool shockCR = false;

    bool forceInputsDisabled = false;

    public bool debugEnabled = false;
    public bool playtesting = false;

    private void Awake()
    {
        inputs = GetComponent<InputBroker>();

        rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        originalScale = transform.lossyScale;

        groundLayerMask = LayerMask.GetMask("Obstacles", "Platforms");
        platformLayerMask = LayerMask.GetMask("Platforms");
        attackLayerMask = LayerMask.GetMask("Enemies", "Targets");
        enemyLayerMask = LayerMask.GetMask("Enemies");
        obstacleLayerMask = LayerMask.GetMask("Obstacles");

        BoxVelocity.velocitiesX[0] = 0;

        isInvulnerable = false;
        dashActive = false;
        canDash = true;
        teleportActive = false;
        rigidBody.isKinematic = false;
        inputs.inputsEnabled = true;
        activateDamage = false;
        enemyHitstopActive = false;
        boxHitstopActive = false;
        canTeleport = true;
        shockActive = false;
        inShockRadius = false;
        doubleJumpUsed = false;

        initialGroundFriction = groundFriction;
        stickyFriction = initialGroundFriction * 4;
        airAccel = initialAirAccel;
        airFriction = initialAirFriction;

        damageActive = false;
    }
    private void checkIsGrounded()
    {
        groundVerticalVelocity = 0;
        groundRayCast = Physics2D.BoxCast(new Vector2(boxCollider.bounds.center.x,boxCollider.bounds.center.y-(transform.lossyScale.y/2)-0.02f),
            new Vector2(transform.lossyScale.x/2*0.9f,0.05f), 0, Vector2.down, 0f, groundLayerMask);
        Collider2D ground = groundRayCast.collider;
        if (ground != null && ground.GetComponent<MovingObjects>() != null)
        {
            groundVerticalVelocity = ground.GetComponent<Rigidbody2D>().velocity.y;
        }
        else
        {
            groundVerticalVelocity = 0;
        }

        if (ground != null && Mathf.Abs(rigidBody.velocity.y - groundVerticalVelocity) < 0.05f && boxHitstopActive == false)
        {
            if (1 << ground.gameObject.layer == platformLayerMask && PlatformDrop.platformsEnabled == true)
            {
                groundTime += Time.deltaTime;
            }
            else if (1 << ground.gameObject.layer != platformLayerMask)
            {
                groundTime += Time.deltaTime;
            }
        }
        else
        {
            groundTime = 0;
        }
        float isGroundedWindow = 0.01f;
        if (ground != null && groundTime > isGroundedWindow) 
        {
            isGrounded = true;
            startAirTimer = false;
            airTime = 0;
            wallJumpCounter = 0;
            canDoubleJump = true;
            doubleJumpUsed = false;
            if (ground.tag == "Ice" && BoxPerks.spikesActive == false)
            {
                isOnIce = true;
                if (iceCRActive == false)
                {
                    StartCoroutine(Ice());
                }
            }
            if (ground.tag == "Fence")
            {
                sticky = true;
            }
        }
        else
        {
            isGrounded = false;
            isOnIce = false;
        }
    }

    private void FixedUpdate()
    {
        if (groundedJump)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x,
            jumpSpeed * (1.05f + Mathf.Max(Mathf.Abs(BoxVelocity.velocitiesX[0]), Mathf.Abs(rigidBody.velocity.x)) / 200) + Mathf.Max(0, groundVerticalVelocity));
            bufferJump = false;
            groundedJump = false;
        }
    }
    private void Update()
    {
        if (UIManager.paused == true)
        {
            inputs.inputsEnabled = false;
        }
        //list of all possible states that could cause inputs to be disabled
        else if (damageActive == false && dashActive == false && teleportActive == false && enemyHitstopActive == false && wallBounceActive == false
            && reboundActive == false && boxEnemyPulseActive == false && blastZoneCRActive == false && forceInputsDisabled == false)
        {
            inputs.inputsEnabled = true;
        }

        //normal movement if hitstop is not active
        if (enemyHitstopActive == false)
        {
            rigidBody.velocity = new Vector2(BoxVelocity.resultVelocitiesX, rigidBody.velocity.y);
        }
        else
        {
            rigidBody.velocity = new Vector2(0,0);
        }

        rigidBody.gravityScale = gravityScale * gravityMult + windGravity;
        maxFallSpeed = rigidBody.gravityScale * -5;

        checkIsGrounded();

        if (BoxPerks.spikesActive)
        {
            sticky = true;
        }
        if (sticky == true)
        {
            groundFriction = stickyFriction;
        }
        else
        {
            groundFriction = initialGroundFriction;
        }

        float adjustedCrouchSpeed = crouchMaxSpeed * Mathf.Min(Mathf.Abs(inputs.leftStick.x), 0.5f) * 2;
        float adjustedHorizSpeed = horizMaxSpeed * Mathf.Max(Mathf.Abs(inputs.leftStick.x), 0.2f);
        if (isGrounded == true)
        {
            //crouch activation / deactivation and conditions
            if (inputs.leftStick.y <= crouchThreshold && isCrouching == false && teleportActive == false && dashActive == false)
            {
                isCrouching = true;
                transform.position = new Vector2(transform.position.x, transform.position.y - (originalScale.y * (1 - crouchScale)) / 2);
                transform.localScale = new Vector2(originalScale.x, originalScale.y * crouchScale);
            }
            if (inputs.leftStick.y > crouchThreshold && isCrouching == true)
            {
                deactivateCrouch = true;
            }

            if (isOnIce == false)
            {
                //crouch movements
                if (isCrouching == true)
                {
                    if (inputs.leftStick.x > 0.1f)
                    {
                        //set horizontal velocity to 0 when inputting right while moving left
                        if (BoxVelocity.velocitiesX[0] < 0)
                        {
                            BoxVelocity.velocitiesX[0] = 0;
                        }
                        //if velocity is less than max, speed up
                        if (BoxVelocity.velocitiesX[0] + crouchAccel * Time.deltaTime < adjustedCrouchSpeed)
                        {
                            BoxVelocity.velocitiesX[0] += crouchAccel * Time.deltaTime;
                        }
                        //if velocity is higher than max, slow down
                        else if (BoxVelocity.velocitiesX[0] - crouchFriction * Time.deltaTime > adjustedCrouchSpeed)
                        {
                            BoxVelocity.velocitiesX[0] -= crouchFriction * Time.deltaTime;
                        }
                        //if close enough to max speed, set equal to max speed
                        else
                        {
                            BoxVelocity.velocitiesX[0] = adjustedCrouchSpeed;
                        }
                    }
                    else if (inputs.leftStick.x < -0.1f)
                    {
                        //set horizontal velocity to 0 when inputting left while moving right
                        if (BoxVelocity.velocitiesX[0] > 0)
                        {
                            BoxVelocity.velocitiesX[0] = 0;
                        }
                        //if velocity is less than max, speed up
                        if (BoxVelocity.velocitiesX[0] - crouchAccel * Time.deltaTime > -adjustedCrouchSpeed)
                        {
                            BoxVelocity.velocitiesX[0] -= crouchAccel * Time.deltaTime;
                        }
                        //if velocity is higher than max, slow down
                        else if (BoxVelocity.velocitiesX[0] + crouchFriction * Time.deltaTime < -adjustedCrouchSpeed)
                        {
                            BoxVelocity.velocitiesX[0] += crouchFriction * Time.deltaTime;
                        }
                        //if close enough to max speed, set equal to max speed
                        else
                        {
                            BoxVelocity.velocitiesX[0] = -adjustedCrouchSpeed;
                        }
                    }
                    else
                    {
                        //slow down when moving right with no L/R input
                        if (BoxVelocity.velocitiesX[0] - crouchFriction * Time.deltaTime > 0)
                        {
                            BoxVelocity.velocitiesX[0] -= crouchFriction * Time.deltaTime;
                        }
                        //slow down when moving left with no L/R input
                        else if (BoxVelocity.velocitiesX[0] + crouchFriction * Time.deltaTime < 0)
                        {
                            BoxVelocity.velocitiesX[0] += crouchFriction * Time.deltaTime;
                        }
                        //set velocity to 0 when close enough to 0 with no L/R input
                        else
                        {
                            BoxVelocity.velocitiesX[0] = 0;
                        }
                    }
                }
                //regular movements, non crouched
                else
                {
                    if (inputs.leftStick.x != 0 && Mathf.Abs(BoxVelocity.velocitiesX[0]) <= Mathf.Abs(dashSpeed))
                    {
                        BoxVelocity.velocitiesX[0] = adjustedHorizSpeed * Mathf.Sign(inputs.leftStick.x);
                    }
                    else
                    {
                        //regular grounded friction L and R
                        if (BoxVelocity.velocitiesX[0] - groundFriction * Time.deltaTime > 0)
                        {
                            BoxVelocity.velocitiesX[0] += -groundFriction * Time.deltaTime;
                        }
                        else if (BoxVelocity.velocitiesX[0] + groundFriction * Time.deltaTime < 0)
                        {
                            BoxVelocity.velocitiesX[0] += groundFriction * Time.deltaTime;
                        }
                        else
                        {
                            BoxVelocity.velocitiesX[0] = 0;
                        }
                    }
                }
            }

            //grounded jump + air timer. Jump higher if you're moving horizontally + buffered jump
            if (inputs.jumpButtonDown || inputs.leftSmashU || bufferJump)
            {
                StartCoroutine(ShortJump());
                groundedJump = true;
                groundTime = 0;
                bufferJump = false;
            }
            //grounded spin attack
            if (inputs.attackButton && teleportActive == false && dashActive == false)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, attackJumpSpeed);
                rigidBody.angularVelocity = attackSpinSpeed * lookingRight;
            }
        }
        //turn crouch off when isGrounded = false
        if (isCrouching == true && (isGrounded == false || deactivateCrouch == true))
        {
            transform.localScale = new Vector2(originalScale.x, originalScale.y);
            transform.position = new Vector2(transform.position.x, transform.position.y + (originalScale.y * (1 - crouchScale)) / 2);
            isCrouching = false;
            deactivateCrouch = false;
        }

        //looking R/L
        if (Mathf.Abs(BoxVelocity.velocitiesX[0]) >= 0.5)
        {
            lookingRight = (int)(BoxVelocity.velocitiesX[0] / Mathf.Abs(BoxVelocity.velocitiesX[0]));
        }
        //start air timer
        if (isGrounded == false)
        {
            startAirTimer = true;
        }
        if (airTime <= 0.06f && shortJumpEnum == false)
        {
            if (inputs.jumpButtonDown || inputs.leftSmashU)
            {
                StartCoroutine(ShortJump());
                groundedJump = true;
                bufferJump = false;
            }
            if (inputs.attackButtonDown)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, attackJumpSpeed);
            }
            if (wallBounceActive == false && reboundActive == false && Mathf.Abs(rigidBody.angularVelocity) < 400 )
            {
                rigidBody.rotation = 0;
                rigidBody.angularVelocity = 0;
            }

        }
        //air timer
        if (startAirTimer == true)
        {
            airTime += Time.deltaTime;
        }
        //air acceleration L/R and air friction + larger air friction if moving faster than horiz max speed
        if ((isGrounded == false || isOnIce == true) && ceilingCling == false)
        {
            if (Mathf.Abs(BoxVelocity.velocitiesX[0]) > horizMaxSpeed)
            {
                if (lookingRight == 1)
                {
                    BoxVelocity.velocitiesX[0] += -airFriction * Time.deltaTime;
                }
                else if (lookingRight == -1)
                {
                    BoxVelocity.velocitiesX[0] += airFriction * Time.deltaTime;
                }
            }
            else if (inputs.leftStick.x != 0 && canWallJump == false && Mathf.Abs(BoxVelocity.velocitiesX[0]) < adjustedHorizSpeed)
            {
                BoxVelocity.velocitiesX[0] += airAccel * Time.deltaTime * Mathf.Sign(inputs.leftStick.x);
            }
            else
            {
                if (BoxVelocity.velocitiesX[0] - airFriction * Time.deltaTime > 0)
                {
                    BoxVelocity.velocitiesX[0] += -airFriction * Time.deltaTime;
                }
                else if (BoxVelocity.velocitiesX[0] + airFriction * Time.deltaTime < 0)
                {
                    BoxVelocity.velocitiesX[0] += airFriction * Time.deltaTime;
                }
                else
                {
                    BoxVelocity.velocitiesX[0] = 0;
                }
            }
        }
        //double jump L/R/else + initiate grounded jump buffer if no double jump
        if (canDoubleJump == false && (inputs.jumpButtonDown || inputs.leftSmashU) && isGrounded == false && dashActive == false)
        {
            StartCoroutine(BufferGroundedJump());
        }
        if (canDoubleJump && (inputs.jumpButtonDown || inputs.leftSmashU) && (sticky == false || (sticky && canWallJump == false))
            && airTime >= shortJumpWindow / 4 && isGrounded == false && dashActive == false && groundedJump == false)
        {
            if (inputs.leftStick.x != 0)
            {
                if (BoxVelocity.velocitiesX[0] > horizMaxSpeed)
                {
                    BoxVelocity.velocitiesX[0] = Mathf.Min(adjustedHorizSpeed / 2 * Mathf.Sign(inputs.leftStick.x) + BoxVelocity.velocitiesX[0],
                        BoxVelocity.velocitiesX[0]);
                }
                else if (BoxVelocity.velocitiesX[0] < -horizMaxSpeed)
                {
                    BoxVelocity.velocitiesX[0] = Mathf.Max(adjustedHorizSpeed / 2 * Mathf.Sign(inputs.leftStick.x) + BoxVelocity.velocitiesX[0],
                        BoxVelocity.velocitiesX[0]);
                }
                else
                {
                    BoxVelocity.velocitiesX[0] = adjustedHorizSpeed * Mathf.Sign(inputs.leftStick.x);
                }
            }
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, djumpSpeed);
            canDoubleJump = false;
            if (BoxPerks.jumpActive && BoxPerks.unlimitedJumps == false && doubleJumpUsed == false)
            {
                canDoubleJump = true;
                doubleJumpUsed = true;
            }
            else if (BoxPerks.jumpActive && BoxPerks.unlimitedJumps == false && doubleJumpUsed)
            {
                BoxPerks.buffActive = false;
            }
        }
        if (BoxPerks.jumpActive && BoxPerks.unlimitedJumps)
        {
            canDoubleJump = true;
        }
        //fastfall condition and action and buffer
        if (rigidBody.velocity.y > maxFallSpeed && rigidBody.gravityScale > 0 && inputs.leftSmashD && canWallJump == false && isGrounded == false)
        {
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
            }
            else if (techCRActive == false)
            {
                StartCoroutine(FastFallBuffer());
            }
        }
        if (rigidBody.velocity.y < maxFallSpeed)
        {
            //rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
            if (rigidBody.gravityScale >= 0)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.MoveTowards(rigidBody.velocity.y, maxFallSpeed, 9.81f * 4 * Time.deltaTime));
            }
            else
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.MoveTowards(rigidBody.velocity.y, maxFallSpeed, 9.81f * Mathf.Abs(rigidBody.gravityScale) * Time.deltaTime));
            }
        }
        //walljump
        if (isGrounded == false && Mathf.Abs(rigidBody.velocity.x) > horizMaxSpeed * 0.8f && boxHitboxActive == false)
        {
            StartCoroutine(WallJumpBuffer());
        }
        if (canWallJump == true)
        {
            if (pressFromWall && postWallJumpTime >= 0.1f)
            {
                startPostWallJumpTimer = true;
                postWallJumpTime = 0;
                canWallJump = false;
                touchingWall = false;
                if (wallJumpCounter <= wallJumpLimit)
                {
                    BoxVelocity.velocitiesX[0] = horizMaxSpeed * wallJumpDirection + wallJumpExtraSpeed;
                    rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Max(jumpSpeed - (wallJumpCounter * 2), rigidBody.velocity.y));
                }
                else
                {
                    BoxVelocity.velocitiesX[0] = horizMaxSpeed * wallJumpDirection + wallJumpExtraSpeed;
                }
                wallJumpCounter += 1;
            }
        }
        if (startPostWallJumpTimer == true)
        {
            postWallJumpTime += Time.deltaTime;
        }
        if (postWallJumpTime >= 0.1f)
        {
            startPostWallJumpTimer = false;
        }
        //aerial spin attack + attack timer to activate attack
        if (isGrounded == false && inputs.attackButton && teleportActive == false)
        {
            rigidBody.angularVelocity = attackSpinSpeed * lookingRight;
        }
        if (Mathf.Abs(rigidBody.angularVelocity) >= -attackSpinDamageSpeed || enemyHitstopActive)
        {
            attackTimer += Time.deltaTime;
        }
        else
        {
            attackTimer = 0;
        }
        if (attackTimer >= timeToActivateAttack)
        {
            spinAttackActive = true;
        }
        else
        {
            spinAttackActive = false;
        }
        //teleporting
        if (teleportUnlocked && inputs.teleportButtonDown && canTeleport && teleportActive == false && dashActive == false && techCRActive == false)
        {
            deactivateCrouch = true;
            teleportActive = true; canTeleport = false;
            StartCoroutine(TeleportEnum());
        }
        if (teleportActive)
        {
            rigidBody.velocity = new Vector2(teleportSpeedx / 4, teleportspeedy / 4);
        }
        //dash attack
        if ((dashUnlocked && canDash && inputs.dashButtonDown && dashActive == false && teleportActive == false && techCRActive == false) || dashActive)
        {
            leftWallCheck = Physics2D.BoxCast(rigidBody.position + Vector2.left * transform.lossyScale.x / 2, new Vector2(0.1f, transform.lossyScale.y / 5),
                0, Vector2.zero, 0, obstacleLayerMask);
            rightWallCheck = Physics2D.BoxCast(rigidBody.position + Vector2.right * transform.lossyScale.x / 2, new Vector2(0.1f, transform.lossyScale.y / 5),
                0, Vector2.zero, 0, obstacleLayerMask);
        }
        if (dashUnlocked && canDash && inputs.dashButtonDown && dashActive == false && teleportActive == false && techCRActive == false)
        {
            if ((lookingRight == 1 && rightWallCheck.collider == null) || (lookingRight == -1 && leftWallCheck.collider == null))
            {
                StartCoroutine(Dash());
            }
        }
        if (dashWallBounceActivate == true && wallBounceActive == false)
        {
            StartCoroutine(WallBounce());
        }
        //pulse
        if (pulseActive)
        {
            pulseActive = false;
        }
        if (pulseUnlocked && canPulse && inputs.pulseButtonDown && dashActive == false && teleportActive == false)
        {
            pulseActive = true;
            RaycastHit2D[] enemies = Physics2D.CircleCastAll(rigidBody.position, pulseRadius + 0.5f, Vector2.zero, 0, enemyLayerMask);
            foreach (RaycastHit2D enemy in enemies)
            {
                if (enemy.transform.root.GetComponent<EnemyManager>() != null)
                {
                    enemy.transform.root.GetComponent<EnemyManager>().pulseActive = true;
                }
            }
            if (sticky == false || (sticky && canWallJump == false))
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 11 / 12 + 5);
            }
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * 0.3f + 5);
            }
            newPulseField = Instantiate(pulseField, rigidBody.position, Quaternion.identity);
            newPulseField.GetComponent<Transform>().localScale = new Vector2(pulseRadius * 2, pulseRadius * 2);
            StartCoroutine(PulseCooldown());
        }
        //enemy pulse
        if (boxWasPulsed && boxEnemyPulseActive == false)
        {
            if (isInvulnerable == false)
            {
                boxEnemyPulseActive = true;
                StartCoroutine(EnemyPulse());
            }
            boxWasPulsed = false;
        }

        //
        //enemy interactions to activate when boxcast covering player body encounters an enemy. Mostly used in other scripts.
        //
        if (GetComponent<BoxCollider2D>().enabled == true)
        {
            attackRayCast = Physics2D.BoxCastAll(rigidBody.position, new Vector2(transform.localScale.x, transform.localScale.y),
                rigidBody.rotation, Vector2.down, 0f, attackLayerMask);
        }
        else
        {
            attackRayCast = Physics2D.BoxCastAll(rigidBody.position, new Vector2(transform.localScale.x, transform.localScale.y),
                rigidBody.rotation, Vector2.down, 0f, LayerMask.GetMask(""));
        }

        if (BoxPerks.spikesActive)
        {
            attackRayCast = Physics2D.CircleCastAll(rigidBody.position, 0.85f, Vector2.down, 0f, attackLayerMask);
        }
        if (attackRayCast.Length > 0 && 1 << attackRayCast[0].collider.gameObject.layer == enemyLayerMask)
        {
            enemyPosition = attackRayCast[0].transform.position;
        }
        if (attackRayCast.Length == 0)
        {
            activatePushBack = false;
        }
        //activate box hitbox
        if ((spinAttackActive && damageActive == false) || Mathf.Abs(BoxVelocity.velocitiesX[0]) >= dashSpeed * 0.95f ||
            ((Mathf.Abs(rigidBody.velocity.x) >= 15 || rigidBody.velocity.y >= 15 || rigidBody.velocity.y < maxFallSpeed * 1.05f) && 
            damageActive && boxHitstopActive == false && ignoreProjectileDamage == false) || 
            enemyHitstopActive == true || BoxPerks.spikesActive || BoxPerks.starActive)
        {
            boxHitboxActive = true;
        }
        else
        {
            boxHitboxActive = false;
        }
        //hurt enemy and activate hitstop
        if (activateHitstop == true)
        {
            if (enemyHitstopActive == false)
            {
                StartCoroutine(HitStop());
            }
            else
            {
                extendHitstop = true;
            }
            activateHitstop = false;
        }
        //rebound
        if (activateRebound == true)
        {
            if (enemyHitstopActive == false)
            {
                StartCoroutine(EnemyRebound());
            }
            activateRebound = false;
        }
        //shock
        if (activateShock)
        {
            if (shockActive == false)
            {
                shockActive = true;
                canBeShocked = false;
                StartCoroutine(Shock(true));
                StartCoroutine(ResidualShock());
            }
            activateShock = false;
        }
        if (inShockRadius && shockCR == false)
        {
            StartCoroutine(Shock(false));
        }
        //take damage
        if (activateDamage == true)
        {
            if (isInvulnerable == false)
            {
                if (isCrouching == true)
                {
                    damageTaken *= 0.6f;
                }
                if (BoxPerks.heavyActive)
                {
                    damageTaken *= 0.3f;
                }
                if (BoxPerks.shieldActive == false)
                {
                    boxHealth -= damageTaken;
                }
                if (damageTaken >= 3)
                {
                    dashActive = false;
                    teleportActive = false;
                    boxHitstopDelay = 0.1f + Mathf.Min(damageTaken, 200) * boxHitstopDelayMult;
                    if (shockActive)
                    {
                        boxHitstopDelay *= shockHitstopMult;
                    }
                    StartCoroutine(HitstopImpact(damageTaken));
                    if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
                    {
                        GameObject.Find("Main Camera").GetComponent<CameraFollowBox>().boxDamageShake = true;
                        GameObject.Find("Main Camera").GetComponent<CameraFollowBox>().boxDamageTaken = damageTaken;
                    }
                }
            }
            damageTaken = 0;
            activateDamage = false;
        }
        if (isInvulnerable == true && isCurrentlyFlashing == false && BoxPerks.starActive == false && boxHitstopActive == false)
        {
            StartCoroutine(DamageFlash());
        }
        if (damageActive == true || boxEnemyPulseActive)
        {
            if (InputBroker.Keyboard && Input.GetKey(KeyCode.Z) == false) { canTech = true; }
            if (InputBroker.GameCube && Input.GetButton("GC L") == false && Input.GetButton("GC R") == false) { canTech = true; }
            if (InputBroker.Xbox && Input.GetAxisRaw("Xbox L") > -0.9f && Input.GetAxisRaw("Xbox R") > -0.9f) { canTech = true; }

            if (InputBroker.Keyboard && canTech && techCRActive == false && Input.GetKeyDown(KeyCode.Z))
                { StartCoroutine(TechBuffer()); }
            if (InputBroker.GameCube && canTech && techCRActive == false && (Input.GetButtonDown("GC L") || Input.GetButtonDown("GC R")))
                { StartCoroutine(TechBuffer()); }
            if (InputBroker.Xbox && canTech && techCRActive == false && (Input.GetAxisRaw("Xbox L") < -0.9f || Input.GetAxisRaw("Xbox R") < -0.9f))
                { StartCoroutine(TechBuffer()); }

            if (damageActive && enemyHitstopActive == false)
            {
                damageTime += Time.deltaTime;
            }
        }
        if (boxHealth <= 0)
        {
            boxHealth = 0;
        }
        //pushback
        if (activatePushBack == true)
        {
            if (enemyHitstopActive == false)
            {
                int pushDirection = (int)(Mathf.Abs(rigidBody.position.x - enemyPosition.x) / (rigidBody.position.x - enemyPosition.x));
                BoxVelocity.velocitiesX[2] = pushBackMagnitude * pushDirection;
            }
        }
        else
        {
            BoxVelocity.velocitiesX[2] = 0;
        }

        //star perk
        if (BoxPerks.starActive)
        {
            isInvulnerable = true;
        }
        if (BoxPerks.starDeactivated)
        {
            if (BoxPerks.buffActive)
            {
                isInvulnerable = false;
            }
            else
            {
                StartCoroutine(Invulnerability(1.5f));
            }
        }

        if (debugEnabled)
        {
            //debug color changes (will probably keep walljump color in normal game)
            if (inputs.inputsEnabled == false && boxHitboxActive == false)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            } // red = inputs disabled, aka damage or emeny pulse or anything that disables inputs, but not when hitbox is active
            else if (boxHitboxActive)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.green;
            } // green = spin or dash attack is active
            else if (isGrounded == true)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.cyan;
            } //cyan = grounded
            else if (startAirTimer == true && shortJumpEnum == true)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.blue;
            } // blue = After grounded jump and within short jump window
            else if (canWallJump == true)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.magenta;
            } // magenta = can wall jump
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.white;
            } // white = none of the above
        }
        else
        {
            if (damageActive)
            {
                gameObject.GetComponent<Renderer>().material.color = Color.red;
            }
            else
            {
                gameObject.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        //isInvulnerable = true;
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        //cause damage from hazards
        if (collision.gameObject.tag == "Hazard" && isInvulnerable == false)
        {
            activateDamage = true;
            damageTaken = 40;
            boxDamageDirection = collision.contacts[0].normal;
            if (Mathf.Abs(boxDamageDirection.y) < 0.1f)
            {
                boxDamageDirection = new Vector2(boxDamageDirection.x, 1).normalized;
            }
            if (Mathf.Abs(boxDamageDirection.x) < 0.1f)
            {
                boxDamageDirection = new Vector2(0, 1 * Mathf.Sign(boxDamageDirection.y));
            }
        }
        if (1 << collision.gameObject.layer == platformLayerMask && collision.gameObject.GetComponent<PlatformDrop>().shockActive && isGrounded)
        {
            if (isInvulnerable == false)
            {
                activateDamage = true;
                damageTaken = Lightning.contactDamage;
                boxDamageDirection = collision.contacts[0].normal;
                activateShock = true;
                if (Mathf.Abs(boxDamageDirection.y) < 0.1f)
                {
                    boxDamageDirection = new Vector2(boxDamageDirection.x, 1).normalized;
                }
                if (Mathf.Abs(boxDamageDirection.x) < 0.1f)
                {
                    boxDamageDirection = new Vector2(0, 1 * Mathf.Sign(boxDamageDirection.y));
                }
                collision.gameObject.GetComponent<PlatformDrop>().endShock = true;
            }
            else
            {
                collision.gameObject.GetComponent<PlatformDrop>().shockActive = false;
            }
        }

        touchedGround = false;
        float groundVelocityY = 0;

        bool touchingLeftWall = false;
        bool touchingRightWall = false;
        Vector2 wallVelocity = Vector2.zero;
        GameObject wall = collision.gameObject;

        bool touchingCeiling = false;
        float ceilingPositionY = 0;
        Vector2 ceilingVelocity = Vector2.zero;
        foreach (ContactPoint2D col in collision.contacts)
        {
            if (col.normal.y > 0.8f && (1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles") || 1 << col.collider.gameObject.layer == LayerMask.GetMask("Platforms")))
            { 
                touchedGround = true; groundVelocityY = col.collider.GetComponent<Rigidbody2D>().velocity.y;
            }
            if (col.normal.x > 0.8f && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles"))
            {
                touchingLeftWall = true;
                wallVelocity = col.collider.gameObject.GetComponent<Rigidbody2D>().velocity;
                wall = col.collider.gameObject;
            }
            if (col.normal.x < -0.8f && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles"))
            { 
                touchingRightWall = true;
                wallVelocity = col.collider.gameObject.GetComponent<Rigidbody2D>().velocity;
                wall = col.collider.gameObject;
            }
            if (col.normal.y < -0.8f && 1 << col.collider.gameObject.layer == LayerMask.GetMask("Obstacles"))
            { 
                touchingCeiling = true; ceilingPositionY = col.point.y;
                ceilingVelocity = col.collider.gameObject.GetComponent<Rigidbody2D>().velocity;
            }
            if (col.collider.gameObject.tag == "Fence")
            {
                sticky = true;
            }
            else if (BoxPerks.spikesActive == false)
            {
                sticky = false;
            }
        }

        if (1 << collision.gameObject.layer == LayerMask.GetMask("Obstacles") || 1 << collision.gameObject.layer == LayerMask.GetMask("Platforms"))
        {
            //stop spinning when pressing down
            if (touchedGround && inputs.attackButton == false && Mathf.Abs(rigidBody.velocity.y - groundVelocityY) < 0.05f)
            {
                canDoubleJump = true;
                rigidBody.angularVelocity = 0;
                rigidBody.rotation = 0;
            }
        }
        if ((touchingLeftWall || touchingRightWall) && dashActive)
        {
            if (touchingLeftWall && leftWallCheck.collider != null)
            {
                wallJumpDirection = 1;
                dashWallBounceActivate = true;
            }
            else if (touchingRightWall && rightWallCheck.collider != null)
            {
                wallJumpDirection = -1;
                dashWallBounceActivate = true;
            }
        }

        float spikeClimbSpeed = 8f;
        if (touchingCeiling && sticky && inputs.leftStick.y > 0.2f && damageActive == false && spinAttackActive == false)
        {
            ceilingCling = true;
            rigidBody.position = new Vector2(rigidBody.position.x, ceilingPositionY - transform.localScale.y / 2);
            if (inputs.leftStick.x > 0.2f || inputs.leftStick.x < -0.2f)
            {
                BoxVelocity.velocitiesX[0] = spikeClimbSpeed * inputs.leftStick.x;
            }
            else
            {
                BoxVelocity.velocitiesX[0] = 0;
            }

            if (ceilingVelocity.y > 0)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, 10);
            }
            else
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
            }
            BoxVelocity.velocitiesX[1] = ceilingVelocity.x;
        }
        else
        {
            ceilingCling = false;
        }

        touchingWall = false;
        if ((1 << collision.gameObject.layer == obstacleLayerMask && collision.gameObject.tag != "Hazard") || (collision.gameObject.tag == "Hazard" && isInvulnerable))
        {
            RaycastHit2D wallcast = Physics2D.BoxCast(rigidBody.position + Vector2.up * transform.localScale.y / 4, new Vector2(transform.localScale.x * 1.1f, transform.localScale.y / 3),
                0, Vector2.zero, 0, obstacleLayerMask);
            //Direction + key press + wallbounce condition for being on the right side of the wall
            if (touchingLeftWall && wallcast.collider != null)
            {
                wallJumpDirection = 1;
                if (InputBroker.Controller)
                {
                    pressFromWall = inputs.leftStick.x > 0.8f;
                    pressToWall = inputs.leftStick.x < 0;
                }
                else if (InputBroker.Keyboard)
                {
                    pressFromWall = inputs.rightKey;
                    pressToWall = inputs.leftStick.x < 0;
                }

                if (BoxVelocity.velocitiesX[0] < 0)
                {
                    BoxVelocity.velocitiesX[0] = 0;
                }

                if (isGrounded == false)
                {
                    touchingWall = true;
                }
            }
            //Direction + key press + wallbounce condition for being on the left side of the wall
            if (touchingRightWall && wallcast.collider != null)
            {
                wallJumpDirection = -1;
                if (InputBroker.Controller)
                {
                    pressFromWall = inputs.leftStick.x < -0.8f;
                    pressToWall = inputs.leftStick.x > 0;
                }
                else if (InputBroker.Keyboard)
                {
                    pressFromWall = inputs.leftKey;
                    pressToWall = inputs.leftStick.x > 0;
                }

                if (BoxVelocity.velocitiesX[0] > 0)
                {
                    BoxVelocity.velocitiesX[0] = 0;
                }

                if (isGrounded == false)
                {
                    touchingWall = true;
                }
            }
            //Condition for being on either side of the wall
            if (touchingWall == true && isGrounded == false && (collision.gameObject.tag != "Ice" || (collision.gameObject.tag == "Ice" && sticky == true)))
            {
                //wall cling + walljumptime
                if (pressToWall)
                {
                    if (holdToWallActive == false)
                    {
                        StartCoroutine(HoldToWall(wall));
                    }
                    if (sticky == false)
                    {
                        if (rigidBody.velocity.y <= wallClingSpeed && inputs.leftStick.y > crouchThreshold)
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Min(wallClingSpeed, wallVelocity.y));
                        }
                        if (rigidBody.velocity.y > wallClingFastFallSpeed && inputs.leftStick.y <= crouchThreshold)
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y);
                        }
                        if (rigidBody.velocity.y <= wallClingFastFallSpeed && inputs.leftStick.y <= crouchThreshold)
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, wallClingFastFallSpeed);
                        }
                        rigidBody.position = new Vector2(rigidBody.position.x, rigidBody.position.y + Mathf.Max(wallVelocity.y,0) * Time.deltaTime);
                    }
                    else
                    { 
                        if (inputs.leftStick.y > 0.2f)
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, spikeClimbSpeed * inputs.leftStick.y);
                        }
                        else if (inputs.leftStick.y < -0.2f)
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, spikeClimbSpeed * inputs.leftStick.y);
                        }
                        else
                        {
                            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.gravityScale * Time.deltaTime * 9.81f);
                        }
                        rigidBody.position = new Vector2(rigidBody.position.x, rigidBody.position.y + wallVelocity.y * Time.deltaTime);
                    }

                }
                if (pressToWall || resetWallJumpTime)
                {
                    wallJumpTime = 0;
                    resetWallJumpTime = false;
                }
                wallJumpTime += Time.deltaTime;
                //walljump condition
                if (wallJumpTime <= wallJumpWindow && Mathf.Abs(rigidBody.velocity.y) >= 0.01)
                {
                    canWallJump = true;
                }
                else
                {
                    canWallJump = false;
                }
            }
            else
            {
                canWallJump = false;
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // what I'm colliding with properties
        bool touchingGround = false;
        bool touchingLeftWall = false;
        bool touchingRightWall = false;
        bool touchingCeiling = false;
        foreach (ContactPoint2D col in collision.contacts)
        {
            if (col.normal.y > 0.6f) { touchingGround = true; }
            if (col.normal.x > 0.8f && col.collider.GetComponent<PlatformDrop>() == null) { touchingLeftWall = true; }
            if (col.normal.x < -0.8f && col.collider.GetComponent<PlatformDrop>() == null) { touchingRightWall = true; }
            if (col.normal.y < -0.8f && col.collider.GetComponent<PlatformDrop>() == null) { touchingCeiling = true; }
        }

        if ((touchingLeftWall || touchingRightWall) && touchingGround == false && touchingCeiling == false && dashActive == false
            && 1 << (collision.gameObject.layer) != platformLayerMask && isGrounded == false && damageActive == false && collision.rigidbody != null)
        {
            BoxVelocity.velocitiesX[0] = collision.rigidbody.velocity.x;
        }

        if ((touchingLeftWall && inputs.leftStick.x < -0.8f) || (touchingRightWall && inputs.leftStick.x > 0.8f))
        {
            rigidBody.angularVelocity = 0;
            rigidBody.rotation = 0;
        }

        if ((touchingLeftWall || touchingRightWall || touchingCeiling) && damageActive && damageTime > boxHitstopDelay + 0.05f && techWindowActive)
        {
            techSuccessful = true;
            if ((touchingLeftWall && (Input.GetAxisRaw("Left Stick X") > 0.8f || Input.GetAxisRaw("Horizontal") > 0.8f)) ||
                (touchingRightWall && (Input.GetAxisRaw("Left Stick X") < -0.8f || Input.GetAxisRaw("Horizontal") < -0.8f)))
            {
                techWalljump = true;
            }
        }
        else if ((touchingLeftWall || touchingRightWall) && damageActive)
        {
            if (Mathf.Abs(BoxVelocity.velocitiesX[0]) > 15f)
            {
                BoxVelocity.velocitiesX[0] *= -0.6f;
            }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        canWallJump = false;
        wallJumpTime = 10;
        touchingWall = false;
        isOnIce = false;
        sticky = false;
        ceilingCling = false;

        if (damageActive)
        {
            rigidBody.angularVelocity = -Mathf.Sign(BoxVelocity.velocitiesX[0]) * rigidBody.velocity.magnitude * 100;
        }
    }

    IEnumerator FastFallBuffer()
    {
        float bufferWindow = 0.1f;
        float bufferTime = 0;
        while (bufferTime <= bufferWindow)
        {
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, maxFallSpeed);
                break;
            }
            bufferTime += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator WallJumpBuffer()
    {
        float bufferTimer = 0.02f;
        float bufferTime = 0;
        while (isGrounded == false && resetWallJumpTime == false && postWallJumpTime >= 0.1f && bufferTime <= bufferTimer)
        {
            if (touchingWall == true)
            {
                resetWallJumpTime = true;
            }
            bufferTime += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator ShortJump()
    {
        shortJumpEnum = true;
        float shortJumpTime = 0;
        bool shorthop = false;
        int initialWJCounter = wallJumpCounter;
        while (shortJumpTime <= shortJumpWindow)
        {
            if (inputs.jumpButton == false && inputs.leftStick.y < 0.8f && damageActive == false && inputs.inputsEnabled == true)
            {
                shorthop = true;
            }
            if (canDoubleJump == false)
            {
                shorthop = false;
            }
            if (enemyHitstopActive == false)
            {
                shortJumpTime += Time.deltaTime;
            }
            yield return null;
        }
        if (shorthop == true && inputs.jumpButton == false && inputs.leftStick.y < 0.8f && inputs.inputsEnabled && initialWJCounter == wallJumpCounter)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, Mathf.Max(2, rigidBody.velocity.y - 10));
        }
        shortJumpEnum = false;
    }
    IEnumerator BufferGroundedJump()
    {
        float bufferWindow = 0.05f;
        float bufferTime = 0;
        yield return null;
        while ((inputs.jumpButtonDown || inputs.leftSmashU) == false && bufferTime <= bufferWindow && bufferJump == false && boxHitstopActive == false)
        {
            if (isGrounded)
            {
                bufferJump = true;
            }
            bufferTime += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator BlastZoneRestart()
    {
        inputs.inputsEnabled = false;
        blastZoneCRActive = true;
        blastZoneLookingRight = lookingRight;
        float timer = 0;
        while (timer < blastZoneRespawnWait)
        {
            airAccel = 0;
            rigidBody.velocity = Vector2.zero;
            lookingRight = blastZoneLookingRight;
            timer += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        blastZoneCRActive = false;
    }
    public IEnumerator HitStop() // for when the player does damage to an enemy
    {
        enemyHitstopActive = true;
        inputs.inputsEnabled = false;
        float hitstopVerticalVelocity = rigidBody.velocity.y;
        float hitstopHorizontalVelocity = BoxVelocity.velocitiesX[0];
        BoxVelocity.velocitiesX[0] = 0;
        rigidBody.velocity = new Vector2(0,0);
        float hitstopAngularVelocity = rigidBody.angularVelocity;
        rigidBody.angularVelocity = rigidBody.angularVelocity / hitstopRotationSlowDown;
        rigidBody.isKinematic = true;
        float enemyHitstopTime = 0;
        bool projectileDamage = false;
        if (boxHitstopActive == false && (damageActive == true || boxEnemyPulseActive))
        {
            projectileDamage = true;
        }
        while (enemyHitstopTime <= enemyHitstopDelay && boxHitstopActive == false)
        {
            enemyHitstopTime += Time.deltaTime;
            if (extendHitstop)
            {
                enemyHitstopTime = 0;
                extendHitstop = false;
            }
            yield return null;
        }
        if (damageActive == false || projectileDamage == true)
        {
            BoxVelocity.velocitiesX[0] = hitstopHorizontalVelocity;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, hitstopVerticalVelocity);
            rigidBody.angularVelocity = hitstopAngularVelocity;
            if (dashActive == false && damageActive == false)
            {
                inputs.inputsEnabled = true;
            }
        }
        enemyHitstopActive = false;
        rigidBody.isKinematic = false;
    }
    IEnumerator TeleportEnum()
    {
        teleportActive = true;
        rigidBody.angularVelocity /= 4;
        rigidBody.angularDrag /= 100;
        if (inputs.leftStick.x != 0)
        {
            teleportDistancex = teleportRange * Mathf.Sign(inputs.leftStick.x);
        }
        else
        {
            teleportDistancex = teleportRange * lookingRight;
        }
        teleportSpeedx = rigidBody.velocity.x;
        teleportspeedy = rigidBody.velocity.y;
        inputs.inputsEnabled = false;
        newTeleportCheck = Instantiate(teleportCheck);
        newTeleportCheck.transform.position = new Vector2(transform.position.x + teleportDistancex, transform.position.y);
        float window = teleportDelay;
        float timer = 0;
        while (timer <= window && damageActive == false && boxEnemyPulseActive == false && blastZoneCRActive == false)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        successfulTeleport = newTeleportCheck.GetComponent<TeleportCheck>().successfulTeleport;
        Destroy(newTeleportCheck);
        if (blastZoneCRActive == true || damageActive == true || boxEnemyPulseActive == true)
        {
            successfulTeleport = false;
        }
        if (successfulTeleport == true)
        {
            transform.position = new Vector2(transform.position.x + teleportDistancex, transform.position.y);
        }
        else if (timer >= window)
        {
            int direction = (int)Mathf.Sign(teleportDistancex);
            RaycastHit2D rayToWall = Physics2D.Raycast(rigidBody.position, Vector2.right * direction, teleportRange, obstacleLayerMask);
            transform.position = rayToWall.point + Vector2.left * direction * transform.lossyScale.x / 2;
        }
        rigidBody.angularVelocity *= 4;
        rigidBody.angularDrag *= 100;
        teleportActive = false;
        yield return null;
        timer = 0;
        window = teleportCooldown;
        while (timer < window)
        {
            window = teleportCooldown;
            timer += Time.deltaTime;
            yield return null;
        }
        canTeleport = true;
    }
    IEnumerator Dash()
    {
        deactivateCrouch = true;
        dashActive = true;
        canDash = false;
        rigidBody.position = new Vector2(rigidBody.position.x, rigidBody.position.y + 0.01f);
        if (inputs.leftStick.x != 0)
        {
            dashDirection = (int)Mathf.Sign(inputs.leftStick.x);
        }
        else
        {
            dashDirection = lookingRight;
        }
        float dashTimer = 0;
        while (dashTimer <= dashTimeLimit && dashActive == true)
        {
            if (enemyHitstopActive == false)
            {
                inputs.inputsEnabled = false;
                gravityScale = 0;
                BoxVelocity.velocitiesX[0] = dashSpeed * dashDirection;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
                dashTimer += Time.deltaTime;
            }
            yield return null;
        }
        bool touchedGround = false;
        dashActive = false;
        if (damageActive == false && reboundActive == false && boxEnemyPulseActive == false)
        {
            inputs.inputsEnabled = true;
        }
        gravityScale = initialGravityScale;
        float Window = 0.2f;
        float timer1 = 0;
        if (damageActive == false && boxEnemyPulseActive == false)
        {
            airAccel = initialAirAccel * 5;
            groundFriction *= 3;
            airFriction = initialAirFriction * 5;
            if (Mathf.Abs(BoxVelocity.velocitiesX[0]) > horizMaxSpeed)
            {
                while (Mathf.Abs(BoxVelocity.velocitiesX[0]) > horizMaxSpeed && timer1 <= Window)
                {
                    if (enemyHitstopActive == false)
                    {
                        timer1 += Time.deltaTime;
                    }
                    if (isGrounded)
                    {
                        touchedGround = true;
                    }
                    yield return null;
                }
            }
            airAccel = initialAirAccel;
            groundFriction /= 3;
            airFriction = initialAirFriction;
        }
        float cooldownWindow = dashCooldown;
        float timer2 = dashTimer + timer1;
        while (timer2 <= cooldownWindow || touchedGround == false)
        {
            cooldownWindow = dashCooldown;
            timer2 += Time.deltaTime;
            if (isGrounded)
            {
                touchedGround = true;
            }
            yield return null;
        }
        canDash = true;
    }
    IEnumerator WallBounce()
    {
        wallBounceActive = true;
        inputs.inputsEnabled = false;
        BoxVelocity.velocitiesX[0] = horizMaxSpeed * wallJumpDirection / 1.5f;
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed * 0.8f);
        canWallJump = false;
        dashActive = false;
        if (GameObject.Find("Main Camera").GetComponent<CameraFollowBox>() != null)
        {
            CameraFollowBox camScript = GameObject.Find("Main Camera").GetComponent<CameraFollowBox>();
            camScript.startCamShake = true;
            camScript.shakeInfo = new Vector2(20, 20);
        }
        yield return null;
        yield return null;
        inputs.inputsEnabled = false;
        airFriction = initialAirFriction / 3;
        gravityScale = initialGravityScale * 0.625f;
        rigidBody.angularVelocity = -950 * wallJumpDirection;
        dashWallBounceActivate = false;
        float window = reboundTime;
        float timer = 0;
        while (timer <= window && damageActive == false)
        {
            if (timer >= window * 0.6f)
            {
                airFriction = initialAirFriction;
                gravityScale = initialGravityScale;
                rigidBody.angularVelocity = Mathf.MoveTowards(rigidBody.angularVelocity, 0, 2000 * Time.deltaTime);
            }
            timer += Time.deltaTime;
            yield return null;
        }
        if (damageActive == false)
        {
            inputs.inputsEnabled = true;
        }
        airFriction = initialAirFriction;
        gravityScale = initialGravityScale;
        wallBounceActive = false;
    }
    IEnumerator PulseCooldown()
    {
        canPulse = false;
        float window = pulseCooldown;
        float timer = 0;
        while (timer <= window)
        {
            window = pulseCooldown;
            timer += Time.deltaTime;
            yield return null;
        }
        canPulse = true;
    }
    IEnumerator HoldToWall(GameObject wall)
    {
        holdToWallActive = true;
        while (canWallJump && pressToWall && dashActive == false && inputs.attackButton == false && (wall.tag != "Ice" || wall.tag == "Ice" && sticky == true))
        {
            if (wall.GetComponent<MovingObjects>().isMoving == true)
            {
                if (Mathf.Abs(wall.transform.position.x + wallJumpDirection * (transform.lossyScale.x / 2 + wall.transform.lossyScale.x / 2) - rigidBody.position.x) < 0.05f)
                {
                    rigidBody.position = new Vector2(wall.transform.position.x + wallJumpDirection * (transform.lossyScale.x / 2 + wall.transform.lossyScale.x / 2),
                        rigidBody.position.y);
                }
                BoxVelocity.velocitiesX[0] = wall.GetComponent<Rigidbody2D>().velocity.x;
            }
            rigidBody.angularVelocity = 0;
            rigidBody.rotation = 0;
            if (wallJumpDirection == 1)
            {
                wallJumpExtraSpeed = Mathf.Max(wall.GetComponent<Rigidbody2D>().velocity.x, 0);
            }
            if (wallJumpDirection == -1)
            {
                wallJumpExtraSpeed = Mathf.Min(wall.GetComponent<Rigidbody2D>().velocity.x, 0);
            }
            yield return null;
        }
        holdToWallActive = false;
    }
    IEnumerator EnemyRebound()
    {
        reboundActive = true;
        int reboundDirection;
        float reboundMagnitude;
        if (dashActive == true)
        {
            reboundDirection = (int)new Vector2(-rigidBody.velocity.x, 0).normalized.x;
            reboundMagnitude = 1.4f;
        }
        else
        {
            reboundDirection = (int)((rigidBody.position.x - enemyPosition.x) / Mathf.Abs(rigidBody.position.x - enemyPosition.x));
            reboundMagnitude = 1;
        }
        dashActive = false;
        inputs.inputsEnabled = false;
        BoxVelocity.velocitiesX[0] = reboundDirection*reboundMagnitude*horizMaxSpeed;
        rigidBody.angularVelocity = -950*reboundDirection;
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed * 0.7f * reboundMagnitude);
        StartCoroutine(Invulnerability(0.1f));
        yield return new WaitForSeconds(reboundTime*0.8f*reboundMagnitude);
        if (damageActive == false && boxEnemyPulseActive == false)
        {
            inputs.inputsEnabled = true;
        }
        reboundActive = false;

    }
    IEnumerator HitstopImpact(float damageTaken)
    {
        canTech = false;
        boxHitstopActive = true;
        //gameObject.GetComponent<BoxCollider2D>().enabled = false;
        inputs.inputsEnabled = false;
        isInvulnerable = true;
        damageActive = true;
        rigidBody.isKinematic = true;
        if (isCrouching == true)
        {
            transform.localScale = new Vector2(originalScale.x, originalScale.y);
            transform.position = new Vector2(transform.position.x, transform.position.y + (originalScale.y * (1 - crouchScale)) / 2);
            isCrouching = false;
        }
        float currentHorizVelocity = BoxVelocity.velocitiesX[0];
        float currentVertVelocity = rigidBody.velocity.y;
        rigidBody.angularVelocity = 0;
        Vector2 freezePosition = rigidBody.position;
        float shuffleDelay = 0.02f;
        int shuffleCount = 0;
        float shuffleRangeX = 0.3f;
        float shuffleRangeY = 0.05f;

        while (damageTime <= boxHitstopDelay)
        {
            bool shuffleFinished = false;
            if (shuffleCount == 0 || shuffleCount == 2 && shuffleFinished == false)
            {
                rigidBody.position = new Vector2(freezePosition.x,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount += 1;
                shuffleFinished = true;
            }
            if (shuffleCount == 1 && shuffleFinished == false)
            {
                rigidBody.position = new Vector2(freezePosition.x + shuffleRangeX,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount += 1;
                shuffleFinished = true;
            }
            if (shuffleCount == 3 && shuffleFinished == false)
            {
                rigidBody.position = new Vector2(freezePosition.x - shuffleRangeX / 5,
                    freezePosition.y + (-shuffleRangeY / 2) + Random.value * shuffleRangeY);
                shuffleCount = 0;
            }
            yield return new WaitForSeconds(shuffleDelay);
        }
        rigidBody.position = freezePosition;
        //gameObject.GetComponent<BoxCollider2D>().enabled = true;
        boxHitstopActive = false;
        rigidBody.isKinematic = false;
        if (BoxPerks.shieldActive == false)
        {
            StartCoroutine(DamageLaunch(damageTaken));
        }
        else
        {
            BoxPerks.buffActive = false;
            inputs.inputsEnabled = true;
            damageActive = false;
            damageTime = 0;
            BoxVelocity.velocitiesX[0] = currentHorizVelocity;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, currentVertVelocity);
            yield return new WaitForSeconds(1f);
            isInvulnerable = false;
            shockActive = false;
        }
    } //for when the player is damaged
    IEnumerator DamageLaunch(float damageTaken)
    {
        damageTaken = Mathf.Min(damageTaken, 200);
        float maxDamageStunTime = 0.4f + damageTaken * 0.014f + boxHitstopDelay;
        float launchSpeed = 7 + damageTaken / 2.5f;
        rigidBody.angularDrag /= 3;
        //DI overrides inputs, may be necessary to change later
        Vector2 DI = Vector2.zero;
        float DIMult = 0.25f;
        if (InputBroker.Controller)
        {
            DI = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
            DI = inputs.joystickCalibrate(DI, "Left");
            if (DI.magnitude < 0.1f)
            {
                DI = Vector2.zero;
            }
        }
        else if (InputBroker.Keyboard)
        {
            DI = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (DI.magnitude > 1)
            {
                DI = DI.normalized;
            }
        }
        DI *= DIMult;
        Vector2 perpVector = Vector2.Perpendicular(boxDamageDirection).normalized * Vector2.Dot(DI, Vector2.Perpendicular(boxDamageDirection).normalized);
        Vector2 trueDamageVelocity = (boxDamageDirection.normalized + perpVector).normalized * launchSpeed;
        if (Vector2.Dot(trueDamageVelocity.normalized, boxDamageDirection.normalized) > 0.999f)
        {
            trueDamageVelocity = boxDamageDirection.normalized * launchSpeed;
        }
        if (BoxPerks.speedActive)
        {
            trueDamageVelocity *= 1.3f;
        }
        if (playtesting == false && boxHealth <= 0)
        {
            trueDamageVelocity = Vector2.zero;
        }

        ignoreProjectileDamage = true;
        BoxVelocity.velocitiesX[0] = trueDamageVelocity.x;
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, trueDamageVelocity.y);

        Vector2 launchPosition = rigidBody.position;

        yield return new WaitForFixedUpdate();
        airFriction = initialAirFriction / damageAirFrictionMult;
        rigidBody.angularVelocity = -Mathf.Sign(trueDamageVelocity.x) * damageTaken * 30;
        float launchTime = 0;
        while ((isGrounded == false || enemyHitstopActive) && damageTime < maxDamageStunTime && techSuccessful == false)
        {
            if (damageTime > boxHitstopDelay + 0.15f)
            {
                ignoreProjectileDamage = false;
            }
            yield return null;
            if (debugEnabled)
            {
                Debug.DrawRay(launchPosition, boxDamageDirection.normalized * 3);
                Debug.DrawRay(launchPosition + boxDamageDirection.normalized * 3, DI * 3, Color.green);
                Debug.DrawRay(launchPosition + boxDamageDirection.normalized * 3, perpVector * 3, Color.yellow);
                Debug.DrawRay(launchPosition, trueDamageVelocity.normalized * 3, Color.blue);
            }
            launchTime += Time.deltaTime;
        }
        float remainderTime = 0;
        while (remainderTime <= 0.3f && techSuccessful == false)
        {
            remainderTime += Time.deltaTime;
            yield return null;
        }
        if (techSuccessful == true)
        {
            techSuccessful = false;
            if (techWalljump == false)
            {
                BoxVelocity.velocitiesX[0] = 0;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, 2);
            }
            else
            {
                BoxVelocity.velocitiesX[0] = horizMaxSpeed * wallJumpDirection + wallJumpExtraSpeed;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpSpeed);
            }
            rigidBody.rotation = 0;
            rigidBody.angularVelocity = 0;
            techWalljump = false;
        }
        rigidBody.angularDrag *= 3;
        airFriction = initialAirFriction;
        inputs.inputsEnabled = true;
        damageActive = false;
        yield return new WaitForSeconds(1.5f);
        isInvulnerable = false;
        damageTime = 0;
    }
    IEnumerator TechBuffer()
    {
        techCRActive = true;
        float techWindow = 0.4f;
        float techTime = 0;
        while (boxHitstopActive == true)
        {
            techTime += Time.deltaTime;
            yield return null;
        }
        techWindowActive = true;
        while (inputs.inputsEnabled == false && techTime <= techWindow && (damageActive == true || boxEnemyPulseActive))
        {
            if (techSuccessful)
            {
                break;
            }

            if (boxEnemyPulseActive && damageActive)
            {
                break;
            }

            techTime += Time.deltaTime;
            yield return null;
        }
        techWindowActive = false;
        float techDownTime = 0;
        while (inputs.inputsEnabled == false && techDownTime <= techWindow && techSuccessful == false && (damageActive == true || boxEnemyPulseActive))
        {
            techDownTime += Time.deltaTime;
            yield return null;
        }
        if (techSuccessful)
        {
            if (techWalljump == false)
            {
                float afterTechTime = 0;
                float afterTechWinow = 0.1f;
                while (afterTechTime <= afterTechWinow)
                {
                    gravityScale *= 0.1f;
                    afterTechTime += Time.deltaTime;
                    yield return null;
                }
                gravityScale = initialGravityScale;
            }
            yield return null;
        }
        techSuccessful = false;
        techCRActive = false;
    }
    IEnumerator EnemyPulse()
    {
        inputs.inputsEnabled = false;
        dashActive = false;
        rigidBody.angularDrag /= 3;
        airFriction = initialAirFriction / 4;
        groundFriction /= 3;
        while (enemyHitstopActive)
        {
            yield return null;
        }
        yield return null;


        Vector2 DI = Vector2.zero;
        float DIMult = 0.75f;
        if (InputBroker.Controller)
        {
            DI = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
            DI = inputs.joystickCalibrate(DI, "Left");
            if (DI.magnitude < 0.1f)
            {
                DI = Vector2.zero;
            }
        }
        else if (InputBroker.Keyboard)
        {
            DI = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (DI.magnitude > 1)
            {
                DI = DI.normalized;
            }
        }
        DI *= DIMult;
        Vector2 perpVector = Vector2.Perpendicular(boxEnemyPulseDirection) * Vector2.Dot(DI, Vector2.Perpendicular(boxEnemyPulseDirection));
        Vector2 pulseVelocity = (boxEnemyPulseDirection + perpVector).normalized * boxEnemyPulseMagnitude;
        if (Vector2.Dot(pulseVelocity.normalized, boxEnemyPulseDirection) > 0.999f)
        {
            pulseVelocity = boxEnemyPulseDirection * boxEnemyPulseMagnitude;
        }
        Vector2 launchPosition = rigidBody.position;


        BoxVelocity.velocitiesX[0] = pulseVelocity.x;
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, pulseVelocity.y);
        float window = boxEnemyPulseMagnitude / 50;
        float timer = 0;
        rigidBody.angularVelocity = -Mathf.Sign(pulseVelocity.x) * boxEnemyPulseMagnitude * 65;
        while (timer <= window && activateDamage == false && techSuccessful == false && boxWasPulsed == false)
        {
            if (enemyHitstopActive == false)
            {
                if (isGrounded == false)
                {
                    timer += Time.deltaTime;
                }
                else
                {
                    timer += Time.deltaTime * 1.5f;
                }
            }
            Debug.DrawRay(launchPosition, boxEnemyPulseDirection.normalized * 3);
            Debug.DrawRay(launchPosition + boxEnemyPulseDirection.normalized * 3, DI * 3, Color.green);
            Debug.DrawRay(launchPosition + boxEnemyPulseDirection.normalized * 3, perpVector * 3, Color.yellow);
            Debug.DrawRay(launchPosition, pulseVelocity.normalized * 3, Color.blue);
            yield return null;
        }
        if (techSuccessful == true)
        {
            techSuccessful = false;
            BoxVelocity.velocitiesX[0] /= 10;
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 2);
            rigidBody.rotation = 0;
            rigidBody.angularVelocity = 0;
        }
        rigidBody.angularDrag *= 3;
        airFriction = initialAirFriction;
        groundFriction *= 3;
        if (damageActive == false)
        {
            inputs.inputsEnabled = true;
        }
        boxEnemyPulseActive = false;
    }
    IEnumerator DamageFlash()
    {
        isCurrentlyFlashing = true;
        while (isInvulnerable == true && ((boxHealth > 0 && playtesting == false) || playtesting == true))
        {
            gameObject.GetComponent<Renderer>().enabled = true;
            if (isInvulnerable == true)
            {
                yield return new WaitForSeconds(0.12f);
            }
            if (isInvulnerable == true)
            {
                gameObject.GetComponent<Renderer>().enabled = false;
                yield return new WaitForSeconds(0.04f);
            }
        }
        gameObject.GetComponent<Renderer>().enabled = true;
        isCurrentlyFlashing = false;
    }
    IEnumerator Ice()
    {
        float iceMult = 1.5f;
        iceCRActive = true;
        airFriction = initialAirFriction / iceMult;
        airAccel = initialAirAccel / iceMult;
        float initialHorizSpeed = horizMaxSpeed;
        float horizSpeed = horizMaxSpeed;
        while (isOnIce)
        {
            if (isCrouching)
            {
                horizSpeed = crouchMaxSpeed;
            }
            else
            {
                horizSpeed = initialHorizSpeed;
            }
            horizMaxSpeed = horizSpeed;
            yield return null;
        }
        horizMaxSpeed = initialHorizSpeed;
        airFriction = initialAirFriction;
        airAccel = initialAirAccel;
        iceCRActive = false;
    }
    IEnumerator Invulnerability(float time)
    {
        float timer = 0;
        yield return null;
        while (timer <= time)
        {
            isInvulnerable = true;
            timer += Time.deltaTime;
            yield return null;
        }
        isInvulnerable = false;
    }
    IEnumerator Shock(bool damaged)
    {
        float window = 0.1f;
        float timer = window;
        if (damaged == false)
        {
            window *= 3;
        }
        shockCR = true;
        int direction = 1;
        while ((damaged == true && shockActive) || (damaged == false && inShockRadius && shockActive == false))
        {
            if (timer> window)
            {
                Vector2 pointA = rigidBody.position + new Vector2(direction, Random.Range(-1f, 1f)) * transform.lossyScale.x;
                Vector2 pointB = pointA + Vector2.right * -transform.lossyScale.x * 2 * direction;
                newLightning = Instantiate(lightning);
                newLightning.GetComponent<Lightning>().pointA = pointA;
                newLightning.GetComponent<Lightning>().pointB = pointB;
                newLightning.GetComponent<Lightning>().aestheticElectricity = true;
                if (damaged == false)
                {
                    float alphaValue = 0.4f;
                    Color color = newLightning.GetComponent<LineRenderer>().startColor;
                    Color smallColor = newLightning.transform.GetChild(0).GetComponent<LineRenderer>().startColor;
                    color.a = alphaValue * alphaValue;
                    smallColor.a = alphaValue;
                    newLightning.GetComponent<LineRenderer>().material.SetColor("_Color", color);
                    newLightning.transform.GetChild(0).GetComponent<LineRenderer>().material.SetColor("_Color", smallColor);
                }
                timer = 0;
                direction *= -1;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        shockCR = false;
    }
    IEnumerator ResidualShock()
    {
        while (damageActive)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1);
        shockActive = false;
        yield return new WaitForSeconds(2);
    }
    public IEnumerator DisableInputs(float window)
    {
        float timer = 0;
        inputs.inputsEnabled = false;
        while (timer < window)
        {
            inputs.inputsEnabled = false;
            forceInputsDisabled = true;
            timer += Time.deltaTime;
            yield return null;
        }
        forceInputsDisabled = false;
    }
}