using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tC : MonoBehaviour
{
    [SerializeField] VelocityVerlet[] velocityVerlet = new VelocityVerlet[2] {new VelocityVerlet(0,0,0,0), new VelocityVerlet(0, 0, 0, 0) };
    [SerializeField] Vector3 velocity;
    Vector3 characterSize;

    [HeaderAttribute("走路")]
    [SerializeField] float maxWalkSpeed;
    [SerializeField] float walkAddSpeed;
    [SerializeField, Tooltip("移動瞬間轉向時的額外加速度加成倍率")] float walkSlowDownPercent;
    [SerializeField, Tooltip("移動瞬間轉向時的額外加速度加成倍率")] float walkReactivityPercent;

    [HeaderAttribute("跳躍")]
    int jumpCount = 1;
    [SerializeField] float jumpHeight;
    [SerializeField] float gravity;
    [SerializeField] float fallDownSpeedLimit;
    Dictionary<string, Transform> checkPoses = new Dictionary<string, Transform>();
    [SerializeField] float isGroundRayLength;
    [SerializeField] float advanceisGroundRayLength;
    [SerializeField] LayerMask groundLayerMask;
    bool isGround = true;
    bool advanceisGround = true;

    void Start()
    {
        foreach (Transform child in GameObject.Find("CheckPos").transform)
        {
            checkPoses.Add(child.name, child);
        }
        characterSize = transform.Find("Body").localScale;
        velocityVerlet[1].a = gravity;
        velocityVerlet[1].aNext = gravity;
    }

    float a = 10;
    float v0 = 0;
    float t = 0;
    float p = 0;

    VelocityVerlet[] verlets = new VelocityVerlet[2] { new VelocityVerlet(10, 10, 0, 0), new VelocityVerlet(10, 10, 0, 0) };

    void Update()
    {
        t += Time.deltaTime;
        p = v0 * t + 0.5f * a * t * t;

        verlets[0].addSpeed();
        verlets[0].p += verlets[0].getDeltaP();

        verlets[1].p += verlets[1].getDeltaP();
        verlets[1].addSpeed();

        getInput();
        walk();
        chekOnLand();
        jump();
    }

    #region//讀取輸入

    float horizontal;
    float vertical;
    Vector3 inputAxis = Vector3.zero;

    bool pressJump = false;
    bool pressFly = false;
    bool pressDash = false;
    bool advancePressJump = false;
    bool pressJumpAdvanced = false;
    bool advancePressFly = false;
    bool advancePressDash = false;

    void getInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        inputAxis.x = horizontal;
        inputAxis.y = vertical;
        inputAxis = SquareToCircle(inputAxis);
    }

    Vector2 SquareToCircle(Vector2 input)
    {
        Vector2 output = Vector2.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.y = input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }

    #endregion

    void chekOnLand()
    {
        float[] hitDis;
        RaycastHit[] hit = castHit("U", "L", "R", Vector3.down, groundLayerMask, out hitDis);

        isGround = Mathf.Min(hitDis) <= isGroundRayLength;
        advanceisGround = Mathf.Min(hitDis) <= advanceisGroundRayLength;

        //print(Mathf.Min(hitDis) +","+ isGround);
        if (velocityVerlet[1].v < fallDownSpeedLimit)
        {
            velocityVerlet[1].v = fallDownSpeedLimit;
        }
        if (!advanceisGround)
        {
            //Debug.LogError("a");
        }
        if (isGround && velocityVerlet[1].v < 0)
        {
            transform.Translate(Vector3.up * (-Mathf.Min(hitDis) + (characterSize.y / 2)));
            velocityVerlet[1].v = 0;
            jumpCount = 0;
        }
    }

    void walk()
    {
        transform.Translate(velocityVerlet[0].getDeltaP(), 0, 0);
        if (inputAxis.x * velocityVerlet[0].v < 0)
        {
            velocityVerlet[0].aNext = Mathf.Sign(inputAxis.x) * (walkAddSpeed + walkAddSpeed * walkReactivityPercent);
        }
        else if (inputAxis.x * velocity.x > 0)
        {
            velocityVerlet[0].aNext = Mathf.Sign(inputAxis.x) * walkAddSpeed;
        }
        else if (inputAxis.x == 0)
        {
            velocityVerlet[0].aNext = Mathf.Sign(-velocity.x) * (walkAddSpeed + walkAddSpeed * walkReactivityPercent);
        }
        else if (velocity.x == 0)
        {
            velocityVerlet[0].aNext = Mathf.Sign(inputAxis.x) * walkAddSpeed;
        }

        velocityVerlet[0].addSpeed();

        if (inputAxis.x * velocityVerlet[0].v != 0)
        {
            velocityVerlet[0].v = Mathf.Clamp(velocityVerlet[0].v, -maxWalkSpeed, maxWalkSpeed);
        }
        else if (inputAxis.x == 0)
        {
            velocityVerlet[0].v = Mathf.Clamp(velocityVerlet[0].v, -maxWalkSpeed, maxWalkSpeed);
            velocityVerlet[0].aNext = Mathf.Sign(-velocity.x) * (walkAddSpeed + walkAddSpeed * walkReactivityPercent);
        }
    }

    void jump()
    {
        if (advanceisGround && !isGround && pressJumpAdvanced && velocity.y < 0)
        {
            pressJumpAdvanced = false;
            pressJump = false;
            advancePressJump = true;
            print("adv");
        }
        if (isGround && (pressJump || advancePressJump))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            jumpCount++;
            pressJump = false;
            advancePressJump = false;
        }


        transform.Translate(Vector3.up * velocity.y * Time.fixedDeltaTime);
    }

    string colliderName(int i, string name0, string name1, string name2)
    {
        string s = "";
        switch (i)
        {
            case 0:
                s = "CheckPos_" + name0;
                break;
            case 1:
                s = "CheckPos_" + name1;
                break;
            case 2:
                s = "CheckPos_" + name2;
                break;
        }
        return s;
    }

    RaycastHit[] castHit(string name0, string name1, string name2, Vector3 castDir, LayerMask layerMask, out float[] hitDis)
    {
        hitDis = new float[3] { 9999, 9999, 9999 };
        RaycastHit[] hit = new RaycastHit[3];

        for (int i = 0; i < 3; i++)
        {
            string s = colliderName(i, name0, name1, name2);
            Physics.Raycast(checkPoses[s].position, castDir, out hit[i], 9999, layerMask);
            hitDis[i] = Vector3.Distance(checkPoses[s].position, hit[i].point);
        }
        return hit;
    }
}
