using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    bool hold = true;

    float canFly = 0;
    float canFlyAddSpeed = 10;

    CharacterController controller;
    public float walkSpeed;
    public float flySpeed;
    public float jumpHeight;
    public float gravity;

    Vector3 velocity = Vector3.zero;
    public Transform[] groundCheck;
    public float rayLength;
    bool isGround = false;
    public LayerMask groundLayerMask;

    public List<Transform> flyCheckPointTransforms;
    List<Vector3> flyCheckPointVectors = new List<Vector3>();
    public FlyZoneManager flyZoneManager;
    [SerializeField] public List<Dictionary<Transform, Vector3>> flyZoneVectors = new List<Dictionary<Transform, Vector3>>();

    public GameObject[] dashTimes;

    enum State
    {
        stand,
        walk,
        fly,
        quickFly
    }
    State state;
    public Animator spriteAnimator;
    public SpriteRenderer spriteRenderer;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        quickFlyCDTimerCount();
        showDashTimes();
        controller.Move(Vector3.back * transform.position.z);
        if (checkCanFly())
        {
            canFly += Time.deltaTime * canFlyAddSpeed;
        }
        else
        {
            canFly = 0;
        }
        changeState();
    }

    Vector3 inputAxisC = Vector3.zero;

    void changeState()
    {
        inputAxisC.x = Input.GetAxis("Horizontal");
        inputAxisC.y = Input.GetAxis("Vertical");
        inputAxisC.z = 0;
        inputAxisC = SquareToCircle(inputAxisC);
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        if (state != State.quickFly)
        {
            if (hold)
            {
                if (Input.GetKey(KeyCode.Tab) && canFly >= 1)
                {
                    state = State.fly;
                }
                else
                {
                    state = State.stand;
                }
            }
            else
            {
                if (state == State.fly && canFly >= 1)
                {
                    state = State.stand;
                }
                if (Input.GetKeyDown(KeyCode.Tab) && canFly >= 1)
                {
                    if (state == State.fly)
                    {
                        state = State.stand;
                    }
                    else
                    {
                        state = State.fly;
                    }
                }
            }
            if (state == State.fly)
            {
                fly(inputAxisC.x, inputAxisC.y);
            }
            else
            {
                if (Mathf.Abs(inputAxisC.x) > 0.1)
                {
                    state = State.walk;
                }
                else
                {
                    state = State.stand;
                }
                walk(inputAxisC.x);
            }
        }
        else
        {
            quickFly();
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    }

    Vector2 SquareToCircle(Vector2 input)
    {
        Vector2 output = Vector2.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.y = input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }

    void rotateHead(Vector3 finalDir, float rotateSpeed)
    {
        Vector3 dir = finalDir;
        if (dir.x > 0)
        {
            r.y = 0;
        }
        else if (dir.x < 0)
        {
            r.y = -180;
        }
        r.z = -Vector3.Angle(dir, Vector3.up);

        //float rs = 100f;
        //float t = rs / Quaternion.Angle(transform.rotation, Quaternion.Euler(r)) * Time.deltaTime;
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(r), t);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(r), rotateSpeed * Time.deltaTime);
    }

    void walk(float horizontal)
    {
        controller.Move(Vector3.right * horizontal * walkSpeed * Time.deltaTime);

        float groundRayDis = float.MaxValue;
        RaycastHit[] hit = new RaycastHit[groundCheck.Length];
        for (int i = 0; i < groundCheck.Length; i++)
        {
            if (Physics.Raycast(groundCheck[i].position, Vector3.down, out hit[i], rayLength, groundLayerMask))
            {
                float groundRayDisTemp = groundCheck[i].position.y - hit[i].point.y;
                if (groundRayDisTemp < groundRayDis)
                {
                    groundRayDis = groundRayDisTemp;
                }
            }
        }
        isGround = (groundRayDis <= rayLength);

        if (isGround && velocity.y < 0)
        {
            velocity.y = 0;
            controller.Move(Vector3.down * groundRayDis);
        }
        if (isGround && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftAlt)))
        {
            Debug.LogError(123);
            velocity.y += Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        rotateHead(Vector3.up + Vector3.right * 0.000001f * inputAxisC.x, 5);
    }
    #region//飛行
    void fly(float horizontal, float vertical)
    {
        velocity = Vector3.zero;
        slowFly();

        if (Vector3.Angle(inputAxisC, transform.up) < 170 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftAlt)))
        {
            if (quickFlyCDDetermination() < quickFlyCD.Length)
            {
                quickFlyStart();
            }
        }

        rotateHead(inputAxisC, 1.4f);
    }

    float slowSwimmingSpeed = 3;
    float rotateHeadSpeed = 100;
    Vector3 r = Vector3.zero;


    void slowFly()
    {
        Vector3 inputDir = inputAxisC;

        float angle;

        if (inputAxisC.magnitude < 0.2f)
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

        if (inputAxisC.magnitude < 0.2f)
        {
            slowSwimmingSpeed = 0;
        }
        controller.Move(inputDir * slowSwimmingSpeed * 2 / 3 * Time.deltaTime);
    }
    #endregion

    #region//衝刺
    float rotateSpeed = 120;
    Vector3 currentDir = Vector3.up;
    float[] quickFlySpeed = new float[] { 10f, 10f, 15f };
    float quickFlyTimer = 0;

    void quickFlyStart()
    {
        quickFlyTimer = 0;
        currentDir = transform.up;
        currentDir.z = 0;
        currentDir = currentDir.normalized;
        state = State.quickFly;
    }

    void quickFly()
    {
        quickFlyTimer += Time.deltaTime;
        if (quickFlyTimer < 0.7f)
        {
            float angle = Vector3.SignedAngle(currentDir, inputAxisC, Vector3.forward);

            if (canFly>=1 && currentDir.magnitude > 0 && inputAxisC.magnitude > 0 && angle < 170 && angle > -170)
            {
                if (angle > 0)
                {
                    currentDir = Quaternion.Euler(0, 0, rotateSpeed * Time.deltaTime) * currentDir;
                }
                else
                {
                    currentDir = Quaternion.Euler(0, 0, -rotateSpeed * Time.deltaTime) * currentDir;
                }
            }
            controller.Move(currentDir * Time.deltaTime * quickFlySpeed[quickFlyCDState-1]);
            rotateHead(currentDir, 10);
        }
        else
        {
            quickFlyTimer = 0;
            state = State.fly;
        }
    }

    int quickFlyCDState = 0;
    float[] quickFlyCD = new float[] { 0, 1.5f, 2f, 4f };
    float quickFlyCDTimer = 999;

    void quickFlyCDTimerCount()
    {
        if (quickFlyCDTimer <= quickFlyCD[quickFlyCDState])
        {
            quickFlyCDTimer += Time.deltaTime;
        }
        else
        {
            quickFlyCDState = 0;
        }
    }

    int quickFlyCDDetermination()
    {
        if(quickFlyCDState < quickFlyCD.Length - 1)
        {
            quickFlyCDTimer = 0;
            return ++quickFlyCDState;
        }
        return quickFlyCDState + 1;
    }

    void showDashTimes()
    {
        for(int i = 0; i < quickFlyCD.Length - 1; i++)
        {
            dashTimes[i].SetActive(i - quickFlyCDState >= 0);
        }
    }
    #endregion

    #region//飛行區域計算
    bool checkCanFly()
    {
        setFlyCheckPointVectors();
        for (int i = 0; i < flyZoneManager.flyZoneTransforms.Count; i++)
        {
            if (flyZoneManager.flyZoneGroups[i].gameObject.activeSelf)
            {
                bool? PIP = pointsInPolygon(flyCheckPointVectors, new List<Vector3>(flyZoneManager.flyZoneVectors[i]));
                if (PIP == null)
                {
                    Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
                    PIP = true;
                }
                if (PIP == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    void setFlyCheckPointVectors()
    {
        flyCheckPointVectors.Clear();
        Vector3 v = Vector3.zero;
        for (int i = 0; i<flyCheckPointTransforms.Count; i++)
        {
            v = flyCheckPointTransforms[i].position;
            v.z = 0;
            flyCheckPointVectors.Add(v);
        }
    }

    bool? pointsInPolygon(List<Vector3> points, List<Vector3> polygon)
    {
        if (polygon.Count < 3)
        {
            Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
            return null;
        }

        for (int i= 0; i < points.Count; i++)
        {
            bool? PIP = pointInPolygon(points[i], polygon);
            if (PIP == null)
            {
                Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
                return null;
            }
            else if(PIP == true)
            {
                return true;
            }
        }
        return false;
    }

    bool? pointInLine(Vector3 a, Vector3 b1, Vector3 b2)
    {
        if (a.z != 0 || b1.z != 0 || b2.z != 0)
        {
            Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
            return null;
        }
        if(b1 == b2)
        {
            Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
            return null;
        }

        if(a == b1 || a == b2)
        {
            return true;
        }
        //b1a、b2a斜率不同
        if ((a.y - b1.y / a.x - b1.x) - (a.y - b2.y / a.x - b2.x) >= 0.0001f)
        {
            return false;
        }
        //快速排除法，a在b1、b2方框外
        if (a.x > Mathf.Max(b1.x, b2.x) || a.x < Mathf.Min(b1.x, b2.x)
        || a.y > Mathf.Max(b1.y, b2.y) || a.y < Mathf.Min(b1.y, b2.y))
        {
            return false;
        }

        return true;
    }

    bool? lineCrossLine(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        if(a1.z != 0 || a2.z !=0 || b1.z != 0 || b2.z != 0)
        {
            Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
            return null;
        }
        //https://blog.csdn.net/sinat_25415095/article/details/114293638
        //快速排除法
        if (Mathf.Min(a1.x,a2.x)> Mathf.Max(b1.x,b2.x) || Mathf.Max(a1.x, a2.x) < Mathf.Min(b1.x, b2.x)
            || Mathf.Min(a1.y, a2.y) > Mathf.Max(b1.y, b2.y) || Mathf.Max(a1.y, a2.y) < Mathf.Min(b1.y, b2.y))
        {
            return false;
        }

        //跨立
        Vector3 b1a1 = a1 - b1;
        Vector3 a1a2 = a2 - a1;
        Vector3 a1b1 = b1 - a1;
        Vector3 a1b2 = b2 - a1;
        Vector3 b1b2 = b2 - b1;
        Vector3 b1a2 = a2 - b1;

        if (Vector3.Dot(Vector3.Cross(a1b2,a1a2),Vector3.Cross(a1a2,a1b1 )) > 0
            && Vector3.Dot(Vector3.Cross(b1a1, b1b2), Vector3.Cross(b1b2, b1a2)) > 0)
        {
            return true;
        }
        return false;
    }

    bool? pointInPolygon(Vector3 a, List<Vector3> polygon)
    {
        Vector3 pFar = Random.insideUnitCircle * float.MaxValue;
        int crossCount = 0;

        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 b1 = polygon[i];
            Vector3 b2 = polygon[(i+1)% polygon.Count];

            // 如果點在多邊形邊上，直接視為在內
            bool? PIL = pointInLine(a, b1, b2);
            if (PIL == null)
            {
                Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
                return null;
            }
            else if (PIL == true)
            {
                return true;
            }

            //如果多邊形有一邊與射線平行，直接重新隨機射線
            if (Vector3.Cross(pFar - a, b2 - b1) == Vector3.zero)
            {
                return pointInPolygon(a, polygon);
            }

            //如果射線穿過多邊形頂點，直接重新隨機射線
            PIL = pointInLine(b1, pFar, a);
            if (PIL == null)
            {
                Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
                return null;
            }
            else if (PIL == true)
            {
                return pointInPolygon(a, polygon);
            }

            //其餘狀況，如果射線跟邊有交集就+1交集點
            bool? LCL = lineCrossLine(a, pFar, b1, b2);
            if(LCL == null)
            {
                Debug.LogError("   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   NULL   !!!   ");
                return null;
            }
            else if (LCL == true)
            {
                crossCount += 1;
            }
        }

        if (crossCount % 2 != 0)
            return true;
        else
            return false;
    }
    #endregion
}
