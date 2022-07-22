using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class t : MonoBehaviour
{
    CharacterController controller;
    float horizontal;
    float vertical;
    [SerializeReference] Vector3 inputAxis = Vector3.zero;
    [SerializeReference] Vector3 inputAxisC = Vector3.zero;
    [SerializeReference] float inputAxisCMagnitude;
    enum FlyMod
    {
        hold,
        holdUp,
        slowSwimming,
        quickSwimming
    }
    FlyMod flyMod = FlyMod.slowSwimming;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        input();

        for(int i = 0; i < 4; i++)
        {
            if (Input.GetKeyDown((KeyCode)(282 + i)))
            {
                flyMod = (FlyMod)i;
                if (flyMod == FlyMod.quickSwimming)
                {
                    setFirstDir();
                }
            }
        }

        switch (flyMod)
        {
            case FlyMod.holdUp:
                holdUp();
                break;
            case FlyMod.slowSwimming:
                slowSwimmingMove();
                break;
            case FlyMod.quickSwimming:
                quickSwimmingMove();
                break;
        }
    }

    #region//basicInput

    void input()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        inputAxis.x = horizontal;
        inputAxis.y = vertical;
        inputAxisC = SquareToCircle(inputAxis);
        inputAxisCMagnitude = inputAxisC.magnitude;
    }

    Vector2 SquareToCircle(Vector2 input)
    {
        Vector2 output = Vector2.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.y = input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }

    #endregion

    #region//hold

    void hold()
    {

    }

    #endregion

    #region//holdUp

    float flySpeed = 1;

    enum GameMod
    {
        keyboard,
        gamepad
    }
    GameMod gameMod;
    float holdUpGravity = 2f;
    void holdUp()
    {
        Vector3 dir = inputAxis;

        dir.y = 0;
        dir.y += Mathf.Max(Mathf.Clamp(Input.GetAxis("Mouse Y"), 0, 0.5f) * 2, Input.GetAxis("LT")) * 3 * holdUpGravity;
        dir.y -= holdUpGravity;

        controller.Move(dir * flySpeed * Time.deltaTime);
    }

    #endregion

    #region//slowSwimming
    float slowSwimmingSpeed = 3;
    float rotateHeadSpeed = 100;
    Vector3 r = Vector3.zero;

    void rotateHead()
    {
        Vector3 inputDir = inputAxisC;
        if (inputDir.x > 0)
        {
            r.y = 0;
        }
        else if (inputDir.x < 0)
        {
            r.y = -180;
        }
        r.z = -Vector3.Angle(inputDir, Vector3.up);

        //float rs = 100f;
        //float t = rs / Quaternion.Angle(transform.rotation, Quaternion.Euler(r)) * Time.deltaTime;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(r), t);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(r), 1.4f * Time.deltaTime);
    }

    void slowSwimmingMove()
    {
        Vector3 inputDir = inputAxisC;

        float angle;

        if (inputAxisCMagnitude < 0.2f)
        {
            inputDir = Vector3.up;
        }
        angle = Vector3.Angle(transform.up, inputDir);
        if (angle > 170)
        {
            slowSwimmingSpeed = 1f;
        }
        else if (angle > 30)
        {
            slowSwimmingSpeed = 2.5f;
        }
        else if (angle > rotateHeadSpeed * Time.deltaTime)
        {
            slowSwimmingSpeed = 2.75f;
        }
        else
        {
            slowSwimmingSpeed = 3f;
        }

        if (inputAxisCMagnitude < 0.2f)
        {
            slowSwimmingSpeed = 0;
        }
        controller.Move(inputDir * slowSwimmingSpeed * 2/3 * Time.deltaTime);
    }

    #endregion

    #region//quickSwimming

    float rotateSpeed = 120;
    Vector3 currentDir = Vector3.up;
    float quickSwimmingSpeed = 5;


    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //之後要做一個如果輸入方向跟移動方向不一致的化的處理方案
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void setFirstDir()
    {
        currentDir = transform.up;
        currentDir.z = 0;
        currentDir = currentDir.normalized;
    }

    void quickSwimmingMove()
    {
        //transform.rotation = Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.up, currentDir, Vector3.forward));

        float angle = Vector3.SignedAngle(currentDir, inputAxisC,  Vector3.forward);

        if (currentDir.magnitude > 0 && inputAxisCMagnitude > 0 && angle < 170 && angle>-170)
        {
            if (angle>0)
            {
                currentDir = Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime) * currentDir;
            }
            else
            {
                currentDir = Quaternion.Euler(0, 0, -rotateSpeed * Time.deltaTime) * currentDir;
            }
        }
        controller.Move(currentDir * Time.deltaTime * quickSwimmingSpeed);
    }

    #endregion
}
