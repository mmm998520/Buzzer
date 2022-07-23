using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tC : MonoBehaviour
{
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
    }

    double Da = 10;
    double DaChange = 0;//加速度的變化量，如恆量則為0加速度
    double Dv = 0;
    double Dp = 0;

    void Update()
    {
        Dp += Time.deltaTime * (Dv + Time.deltaTime * Da * 0.5d);
        Dv += Time.deltaTime * Da;
        double DaNext = Da + DaChange;//加速度變化是因為質量or力發生變化，瞬間變化下不用Time.deltaTime
        Dv += Time.deltaTime * (DaNext-Da)*0.5d;
        Da = DaNext;

        getInput();
    }

    void FixedUpdate()
    {
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


        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.LeftAlt))/* || advancePressJump*/)
        {
            if (jumpCount < 1)
            {
                pressJump = true;
            }
            /*else
            {
                pressJumpAdvanced = true;
            }*/
        }
        /*if (Input.GetButtonDown("Jump"))
        {
            pressJump = true;
        }
        if (Input.GetButtonDown("Fly"))
        {
            pressFly = true;
        }
        if (Input.GetButtonDown("Dash"))
        {
            pressDash = true;
        }*/
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

        velocity.y += gravity * Time.fixedDeltaTime;
        if (velocity.y < fallDownSpeedLimit)
        {
            velocity.y = fallDownSpeedLimit;
        }
        if (!advanceisGround)
        {
            //Debug.LogError("a");
        }
        if (isGround && velocity.y < 0)
        {
            transform.Translate(Vector3.up * (-Mathf.Min(hitDis) + (characterSize.y / 2)));
            velocity.y = 0;
            jumpCount = 0;
        }
    }

    void walk()
    {
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        if (horizontalRaw * velocity.x < 0)
        {
            velocity.x = velocity.x - velocity.x / Mathf.Abs(velocity.x) * (walkAddSpeed + walkAddSpeed * walkReactivityPercent) * Time.fixedDeltaTime;
        }
        else if (horizontalRaw * velocity.x > 0)
        {
            velocity.x = velocity.x + velocity.x / Mathf.Abs(velocity.x) * walkAddSpeed * Time.fixedDeltaTime;
        }
        else if (horizontalRaw == 0)
        {
            float a = velocity.x / Mathf.Abs(velocity.x) * (walkAddSpeed + walkAddSpeed * walkSlowDownPercent) * Time.fixedDeltaTime;
            if (velocity.x > a)
            {
                velocity.x = velocity.x - a;
            }
            else
            {
                velocity.x = 0;
            }
        }
        else if (velocity.x == 0)
        {
            velocity.x = velocity.x + horizontalRaw / Mathf.Abs(horizontalRaw) * walkAddSpeed * Time.fixedDeltaTime;
        }
        else
        {
            velocity.x = 0;
        }
        velocity.x = Mathf.Clamp(velocity.x, -maxWalkSpeed, maxWalkSpeed);

        transform.Translate(Vector3.right * velocity.x * Time.fixedDeltaTime);
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
