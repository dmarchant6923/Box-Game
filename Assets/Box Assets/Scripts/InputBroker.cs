using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputBroker : MonoBehaviour
{
    [HideInInspector] public bool inputsEnabled;

    public static bool Keyboard { get; private set; } = true;
    public static bool Controller { get; private set; } = false;

    public static bool GameCube { get; private set; } = false;
    public static bool Xbox { get; private set; } = false;

    public Vector2 leftStick { get; private set; } //calibration for sticks is done in this script, not with input manager
    public Vector2 rightStick { get; private set; }

    Vector2 leftStick1f;
    Vector2 rightStick1f;
    Vector2 leftStick2f;
    Vector2 rightStick2f;
    Vector2 leftStick3f;
    Vector2 rightStick3f;

    public float XboxL { get; private set; } // axis goes from 0 to -1
    public float XboxR { get; private set; }

    float XboxL1f = 0;
    float XboxR1f = 0;
    float XboxL2f = 0;
    float XboxR2f = 0;
    float XboxL3f = 0;
    float XboxR3f = 0;

    public bool leftKey { get; private set; } // only used for keyboards, so far just for easier walljumps
    public bool rightKey { get; private set; } // only used for keyboards, so far just for easier walljumps

    float GCSensitivityL = 1.35f;
    float GCSensitivityR = 1.5f;
    Vector2 GCDisplacementL = new Vector2(0.05f, 0f);
    Vector2 GCDisplacementR = new Vector2(-0.1f, 0f);
    float GCDeadZoneL = 0.2f;
    float GCDeadZoneR = 0.2f;

    float XboxSensitivityL = 1f;
    float XboxSensitivityR = 1f;
    Vector2 XboxDisplacementL = new Vector2(0.0f, 0f);
    Vector2 XboxDisplacementR = new Vector2(0.0f, 0f);
    float XboxDeadZoneL = 0.2f;
    float XboxDeadZoneR = 0.2f;

    float smashInputLimit = 0.6f;

    float leftSmashX;
    float leftSmashY;
    float rightSmashX;
    float rightSmashY;

    float XboxSmashValueL;
    float XboxSmashValueR;

    bool XboxSmashL;
    bool XboxSmashR;

    public bool leftSmashU { get; private set; }
    public bool leftSmashD { get; private set; }
    public bool leftSmashL { get; private set; }
    public bool leftSmashR { get; private set; }

    public bool rightSmashU { get; private set; }
    public bool rightSmashD { get; private set; }
    public bool rightSmashL { get; private set; }
    public bool rightSmashR { get; private set; }

    public bool jumpButtonDown { get; private set; }
    public bool jumpButtonUp { get; private set; }
    public bool jumpButton { get; private set; }

    public bool upDpad { get; private set; }
    public bool downDpad { get; private set; }
    public bool leftDpad { get; private set; }
    public bool rightDpad { get; private set; }

    public bool attackButtonDown { get; private set; }
    public bool attackButtonUp { get; private set; }
    public bool attackButton { get; private set; }

    public bool teleportButtonDown { get; private set; }
    public bool teleportButton { get; private set; }

    public bool dashButtonDown { get; private set; }
    public bool dashButton { get; private set; }

    public bool pulseButtonDown { get; private set; }
    public bool pulseButtonUp { get; private set; }
    public bool pulseButton { get; private set; }

    public bool rightShiftKeyDown { get; private set; }
    public bool rightShiftKeyUp { get; private set; }
    public bool rightShiftKey { get; private set; }

    public bool startButtonDown { get; private set; }
    public bool startButtonUp { get; private set; }
    public bool startButton { get; private set; }

    //list of whitelisted inputs during hitstop:
    bool bufferedLeftSmashU = false;
    bool bufferedLeftSmashD = false;
    bool bufferedLeftSmashL = false;
    bool bufferedLeftSmashR = false;

    bool bufferedJumpButtonDown = false;
    bool bufferedTeleportButtonDown = false;
    bool bufferedDashButtonDown = false;
    bool bufferedPulseButtonDown = false;
    Vector2 bufferedLeftStick = Vector2.zero;
    bool bufferInputs = false;

    bool hitstopActive;

    private void Awake()
    {
        inputsEnabled = true;
    }
    void Update()
    {
        hitstopActive = Box.enemyHitstopActive;

        if (inputsEnabled == true)
        {
            if (Controller)
            {
                Keyboard = false;
                if (GameCube)
                {
                    Keyboard = false;
                    Xbox = false;

                    leftStick = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
                    rightStick = new Vector2(Input.GetAxisRaw("C Stick X"), Input.GetAxisRaw("C Stick Y"));

                    leftStick = joystickCalibrate(leftStick, "Left");
                    rightStick = joystickCalibrate(rightStick, "Right");

                    if (leftSmashU) { leftSmashU = false; }
                    if (leftSmashD) { leftSmashD = false; }
                    if (leftSmashL) { leftSmashL = false; }
                    if (leftSmashR) { leftSmashR = false; }

                    if (rightSmashU) { rightSmashU = false; }
                    if (rightSmashD) { rightSmashD = false; }
                    if (rightSmashL) { rightSmashL = false; }
                    if (rightSmashR) { rightSmashR = false; }

                    if (leftSmashY > smashInputLimit) { leftSmashU = true; }
                    if (leftSmashY < -smashInputLimit) { leftSmashD = true; }
                    if (leftSmashX < -smashInputLimit) { leftSmashL = true; }
                    if (leftSmashX > smashInputLimit) { leftSmashR = true; }

                    if (rightSmashY > smashInputLimit) { rightSmashU = true; }
                    if (rightSmashY < -smashInputLimit) { rightSmashD = true; }
                    if (rightSmashX < -smashInputLimit) { rightSmashL = true; }
                    if (rightSmashX > smashInputLimit) { rightSmashR = true; }

                    jumpButtonDown = Input.GetButtonDown("GC Y X");
                    jumpButtonUp = Input.GetButtonUp("GC Y X");
                    jumpButton = Input.GetButton("GC Y X");

                    attackButtonDown = Input.GetButtonDown("GC A");
                    attackButtonUp = Input.GetButtonUp("GC A");
                    attackButton = Input.GetButton("GC A");

                    teleportButtonDown = Input.GetButtonDown("GC R");
                    teleportButton = Input.GetButton("GC R");

                    dashButtonDown = Input.GetButtonDown("GC L");
                    dashButton = Input.GetButton("GC L");

                    pulseButtonDown = Input.GetButtonDown("GC B");
                    pulseButtonUp = Input.GetButtonUp("GC B");
                    pulseButton = Input.GetButton("GC B");

                    // upDpad = Input.GetButton("GC Dpad U"); always active

                    // downDpad = Input.GetButton("GC Dpad D"); always active

                    //leftDpad = Input.GetButton("GC Dpad L"); always active

                    // rightDpad = Input.GetButton("GC Dpad R"); always active

                    //startButtonDown = Input.GetButtonDown("GC Start");
                    startButtonUp = Input.GetButtonUp("GC Start");
                    startButton = Input.GetButton("GC Start");
                }
                else if (Xbox)
                {
                    GameCube = false;

                    leftStick = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
                    rightStick = new Vector2(Input.GetAxisRaw("Right Stick X"), Input.GetAxisRaw("Right Stick Y"));

                    leftStick = joystickCalibrate(leftStick, "Left");
                    rightStick = joystickCalibrate(rightStick, "right");

                    if (leftSmashU) { leftSmashU = false; }
                    if (leftSmashD) { leftSmashD = false; }
                    if (leftSmashL) { leftSmashL = false; }
                    if (leftSmashR) { leftSmashR = false; }

                    if (rightSmashU) { rightSmashU = false; }
                    if (rightSmashD) { rightSmashD = false; }
                    if (rightSmashL) { rightSmashL = false; }
                    if (rightSmashR) { rightSmashR = false; }

                    if (leftSmashY > smashInputLimit) { leftSmashU = true; }
                    if (leftSmashY < -smashInputLimit) { leftSmashD = true; }
                    if (leftSmashX < -smashInputLimit) { leftSmashL = true; }
                    if (leftSmashX > smashInputLimit) { leftSmashR = true; }

                    if (rightSmashY > smashInputLimit) { rightSmashU = true; }
                    if (rightSmashY < -smashInputLimit) { rightSmashD = true; }
                    if (rightSmashX < -smashInputLimit) { rightSmashL = true; }
                    if (rightSmashX > smashInputLimit) { rightSmashR = true; }

                    if (Input.GetButtonDown("Xbox A")) { jumpButtonDown = true; }
                    else { jumpButtonDown = false; }
                    if (Input.GetButtonUp("Xbox A")) { jumpButtonUp = true; }
                    else { jumpButtonUp = false; }
                    if (Input.GetButton("Xbox A")) { jumpButton = true; }
                    else { jumpButton = false; }

                    attackButtonDown = Input.GetButtonDown("Xbox B");
                    attackButtonUp = Input.GetButtonUp("Xbox B");
                    attackButton = Input.GetButton("Xbox B");

                    XboxL = Input.GetAxisRaw("Xbox L");
                    XboxR = Input.GetAxisRaw("Xbox R");

                    teleportButtonDown = XboxSmashR;
                    if (XboxR > smashInputLimit) { teleportButton = true; }
                    else { teleportButton = false; }

                    dashButtonDown = XboxSmashL;
                    if (XboxL > smashInputLimit) { dashButton = true; }
                    else { dashButton = false; }

                    pulseButtonDown = Input.GetButtonDown("Xbox X");
                    pulseButtonUp = Input.GetButtonUp("Xbox X");
                    pulseButton = Input.GetButton("Xbox X");

                    // upDpad always active

                    // downDpad always active

                    // leftDpad always active

                    // rightDpad always active

                    // startButtonDown = Input.GetButtonDown("Xbox Start"); always active
                    startButtonUp = Input.GetButtonUp("Xbox Start");
                    startButton = Input.GetButton("Xbox Start");
                }
            }
            else if (Keyboard)
            {
                Controller = false;
                GameCube = false;
                Xbox = false;

                //figure out if not using .normalized at the end affects anything. Can get a value of (1,1) without it.
                leftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                rightStick = new Vector2(Input.GetAxisRaw("Horizontal Num Pad"), Input.GetAxisRaw("Vertical Num Pad"));

                leftKey = Input.GetKey(KeyCode.LeftArrow);
                rightKey = Input.GetKey(KeyCode.RightArrow);

                leftSmashU = Input.GetKeyDown(KeyCode.UpArrow);
                leftSmashD = Input.GetKeyDown(KeyCode.DownArrow);
                leftSmashL = Input.GetKeyDown(KeyCode.LeftArrow);
                leftSmashR = Input.GetKeyDown(KeyCode.RightArrow);

                rightSmashU = Input.GetKeyDown(KeyCode.Keypad8);
                rightSmashD = Input.GetKeyDown(KeyCode.Keypad2);
                rightSmashL = Input.GetKeyDown(KeyCode.Keypad4);
                rightSmashR = Input.GetKeyDown(KeyCode.Keypad6);

                jumpButtonDown = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space);
                jumpButtonUp = Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space);
                jumpButton = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Space);

                attackButtonDown = Input.GetKeyDown(KeyCode.X);
                attackButtonUp = Input.GetKeyUp(KeyCode.X);
                attackButton = Input.GetKey(KeyCode.X);

                teleportButtonDown = Input.GetKeyDown(KeyCode.C);
                teleportButton = Input.GetKey(KeyCode.C);

                dashButtonDown = Input.GetKeyDown(KeyCode.Z);
                dashButton = Input.GetKey(KeyCode.Z);

                pulseButtonDown = Input.GetKeyDown(KeyCode.S);
                pulseButtonUp = Input.GetKeyUp(KeyCode.S);
                pulseButton = Input.GetKey(KeyCode.S);

                // upDpad = Input.GetKey(KeyCode.Alpha2); always active

                // downDpad = Input.GetKey(KeyCode.W); always active

                // leftDpad = Input.GetKey(KeyCode.Q); always active

                // rightDpad = Input.GetKey(KeyCode.E); always active

                //startButtonDown = Input.GetKeyDown(KeyCode.Return);
                startButtonUp = Input.GetKeyUp(KeyCode.Return);
                startButton = Input.GetKey(KeyCode.Return);
            }
        }
        else
        {
            leftStick = Vector2.zero;
            rightStick = Vector2.zero;

            leftSmashU = false;
            leftSmashD = false;
            leftSmashL = false;
            leftSmashR = false;

            rightSmashU = false;
            rightSmashD = false;
            rightSmashL = false;
            rightSmashR = false;

            leftKey = false;
            rightKey = false;

            jumpButtonDown = false;
            jumpButtonUp = false;
            jumpButton = false;

            attackButtonDown = false;
            attackButtonUp = false;
            attackButton = false;

            teleportButtonDown = false;
            teleportButton = false;

            dashButtonDown = false;
            dashButton = false;

            pulseButtonDown = false;
            pulseButtonUp = false;
            pulseButton = false;

            rightShiftKeyDown = false;
            rightShiftKeyUp = false;
            rightShiftKey = false;

            // upDpad always active

            // downDpad always active

            // leftDpad always active

            // rightDpad always active

            //startbuttondown always active
            startButtonUp = false;
            startButton = false;
        }
        // list of inputs that are always active.
        if (Controller)
        {
            if (GameCube)
            {
                upDpad = Input.GetButton("GC Dpad U");
                downDpad = Input.GetButton("GC Dpad D");
                leftDpad = Input.GetButton("GC Dpad L");
                rightDpad = Input.GetButton("GC Dpad R");

                startButtonDown = Input.GetButtonDown("GC Start");
            }
            if (Xbox)
            {
                if (XboxSmashL) { XboxSmashL = false; }
                if (XboxSmashR) { XboxSmashR = false; }

                if (XboxSmashValueL < -smashInputLimit) { XboxSmashL = true; }
                if (XboxSmashValueR < -smashInputLimit) { XboxSmashR = true; }

                if (Input.GetAxis("Xbox Dpad Y") >= 0.8f) { upDpad = true; }
                else { upDpad = false; }
                if (Input.GetAxis("Xbox Dpad Y") <= -0.8f) { downDpad = true; }
                else { downDpad = false; }
                if (Input.GetAxis("Xbox Dpad X") <= -0.8f) { leftDpad = true; }
                else { leftDpad = false; }
                if (Input.GetAxis("Xbox Dpad X") >= 0.8f) { rightDpad = true; }
                else { rightDpad = false; }

                startButtonDown = Input.GetButtonDown("Xbox Start");
            }
        }
        else if (Keyboard)
        {
            upDpad = Input.GetKey(KeyCode.Alpha2);
            downDpad = Input.GetKey(KeyCode.W);
            leftDpad = Input.GetKey(KeyCode.Q);
            rightDpad = Input.GetKey(KeyCode.E);

            startButtonDown = Input.GetKeyDown(KeyCode.Return);
        }

        //the following is a white list of inputs that will be buffered during hitstop. Any changes in control scheme will have to be made here too.
        if (hitstopActive == true)
        {
            if (Controller)
            {
                if (GameCube)
                {
                    if (leftSmashY > 0.8f) { bufferedLeftSmashU = true; }
                    if (leftSmashY < -0.8f) { bufferedLeftSmashD = true; }
                    if (leftSmashY < -0.8f) { bufferedLeftSmashL = true; }
                    if (leftSmashY > 0.8f) { bufferedLeftSmashR = true; }

                    if (Input.GetButtonDown("GC Y X"))
                    {
                        bufferedJumpButtonDown = true;
                    }
                    if (Input.GetButtonDown("GC R"))
                    {
                        bufferedTeleportButtonDown = true;
                    }
                    if (Input.GetButtonDown("GC L"))
                    {
                        bufferedDashButtonDown = true;
                    }
                    if (Input.GetButtonDown("GC B"))
                    {
                        bufferedPulseButtonDown = true;
                    }
                    bufferedLeftStick = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
                }
                if (Xbox)
                {
                    if (leftSmashY > 0.8f) { bufferedLeftSmashU = true; }
                    if (leftSmashY < -0.8f) { bufferedLeftSmashD = true; }
                    if (leftSmashY < -0.8f) { bufferedLeftSmashL = true; }
                    if (leftSmashY > 0.8f) { bufferedLeftSmashR = true; }

                    if (Input.GetButtonDown("Xbox Y") || Input.GetButtonDown("Xbox B")) { bufferedJumpButtonDown = true; }
                    if (XboxR > smashInputLimit) { bufferedTeleportButtonDown = true; }
                    if (XboxL > smashInputLimit) { bufferedDashButtonDown = true; }
                    if (Input.GetButtonDown("Xbox X")) { bufferedPulseButtonDown = true; }
                    bufferedLeftStick = new Vector2(Input.GetAxisRaw("Left Stick X"), Input.GetAxisRaw("Left Stick Y"));
                }
            }
            else if (Keyboard)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    bufferedLeftSmashU = true;
                    bufferedJumpButtonDown = true;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    bufferedLeftSmashD = true;
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    bufferedLeftSmashL = true;
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    bufferedLeftSmashR = true;
                }

                if (Input.GetKeyDown(KeyCode.C))
                {
                    bufferedTeleportButtonDown = true;
                }
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    bufferedDashButtonDown = true;
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    bufferedPulseButtonDown = true;
                }
                bufferedLeftStick = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            bufferInputs = true;
        }
        if (hitstopActive == false && bufferInputs == true)
        {
            if (inputsEnabled)
            {
                leftSmashU = bufferedLeftSmashU;
                leftSmashD = bufferedLeftSmashD;
                leftSmashL = bufferedLeftSmashL;
                leftSmashR = bufferedLeftSmashR;
                jumpButtonDown = bufferedJumpButtonDown;
                teleportButtonDown = bufferedTeleportButtonDown;
                dashButtonDown = bufferedDashButtonDown;
                pulseButtonDown = bufferedPulseButtonDown;
                leftStick = bufferedLeftStick;
            }

            bufferInputs = false;

            bufferedLeftSmashU = false;
            bufferedLeftSmashD = false;
            bufferedLeftSmashL = false;
            bufferedLeftSmashR = false;
            bufferedJumpButtonDown = false;
            bufferedTeleportButtonDown = false;
            bufferedDashButtonDown = false;
            bufferedPulseButtonDown = false;
            bufferedLeftStick = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        //only used for analog axis. If the stick is pushed fast enough, registers as a "smash" input for one Update frame (not FixedUpdate)
        //problems: Smash inputs from one end of the stick to the other may not register if it was done too fast, since it's done by
        //axis and not each individual direction. It needs to register the stick in the center for a frame and refresh rate may miss it.

        leftStick3f = leftStick2f;
        leftStick2f = leftStick1f;
        leftStick1f = leftStick;
        if (Mathf.Abs(leftStick3f.x) <= 0.3f && Mathf.Abs(leftStick2f.x) < 0.6f && Mathf.Abs(leftStick1f.x) >= smashInputLimit)
        {
            leftSmashX = Mathf.Sign(leftStick1f.x);
        }
        else
        {
            leftSmashX = 0;
        }
        if (Mathf.Abs(leftStick3f.y) <= 0.3f && Mathf.Abs(leftStick2f.y) < 0.6f && Mathf.Abs(leftStick1f.y) >= smashInputLimit)
        {
            leftSmashY = Mathf.Sign(leftStick1f.y);
        }
        else
        {
            leftSmashY = 0;
        }


        rightStick3f = rightStick2f;
        rightStick2f = rightStick1f;
        rightStick1f = rightStick;

        if (Mathf.Abs(rightStick3f.x) <= 0.3f && Mathf.Abs(rightStick2f.x) < 0.6f && Mathf.Abs(rightStick1f.x) >= smashInputLimit)
        {
            rightSmashX = Mathf.Sign(rightStick1f.x);
        }
        else
        {
            rightSmashX = 0;
        }
        if (Mathf.Abs(rightStick3f.y) <= 0.3f && Mathf.Abs(rightStick2f.y) < 0.6f && Mathf.Abs(rightStick1f.y) >= smashInputLimit)
        {
            rightSmashY = Mathf.Sign(rightStick1f.y);
        }
        else
        {
            rightSmashY = 0;
        }

        if (Xbox)
        {
            XboxL3f = XboxL2f;
            XboxL2f = XboxL1f;
            XboxL1f = XboxL;
            if (XboxL3f >= -0.3f && XboxL2f > -0.6f && XboxL1f <= -smashInputLimit)
            {
                XboxSmashValueL = XboxL1f;
            }
            else
            {
                XboxSmashValueL = 0;
            }

            XboxR3f = XboxR2f;
            XboxR2f = XboxR1f;
            XboxR1f = XboxR;
            if (XboxR3f >= -0.3f && XboxR2f > -0.6f && XboxR1f <= -smashInputLimit)
            {
                XboxSmashValueR = XboxR1f;
            }
            else
            {
                XboxSmashValueR = 0;
            }
        }

    }

    public Vector2 joystickCalibrate(Vector2 input, string stick)
    {
        Vector2 displacement = Vector2.zero;
        float sensitivity = 0;
        float deadZone = 0;
        if (GameCube)
        {
            if (stick == "Left")
            {
                displacement = GCDisplacementL;
                sensitivity = GCSensitivityL;
                deadZone = GCDeadZoneL;
            }
            if (stick == "Right")
            {
                displacement = GCDisplacementR;
                sensitivity = GCSensitivityR;
                deadZone = GCDeadZoneR;
            }
        }
        if (Xbox)
        {
            if (stick == "Left")
            {
                displacement = XboxDisplacementL;
                sensitivity = XboxSensitivityL;
                deadZone = XboxDeadZoneL;
            }
            if (stick == "Right")
            {
                displacement = XboxDisplacementR;
                sensitivity = XboxSensitivityR;
                deadZone = XboxDeadZoneR;
            }
        }

        Vector2 output = (input + displacement) * sensitivity;

        if (Mathf.Abs(output.x) <= deadZone)
        {
            output = new Vector2(0, output.y);
        }
        if (Mathf.Abs(output.y) <= deadZone)
        {
            output = new Vector2(output.x, 0);
        }
        if (Mathf.Abs(output.magnitude) >= 1)
        {
            output = new Vector2(output.x, output.y).normalized;
        }
        return output;
    }
}
