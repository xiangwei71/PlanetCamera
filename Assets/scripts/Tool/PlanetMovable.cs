﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;

public interface InputProxy
{
    Vector2 getHV();
    bool pressJump();
    bool pressFire();
    bool holdFire();
}

public interface GroundGravityGenerator
{
    Vector3 findGroundUp();
}

public interface MoveForceSelector
{
    void resetByGroundType(GroundType groundType, Rigidbody rigid);
}

public interface MoveForceMonitor
{
    float getMoveForceStrength(bool isOnAir, bool isTurble);
    float getGravityForceStrength(bool isOnAir);
    float getJumpForceStrength(bool isTurble);
    void setRigidbodyParamter(Rigidbody rigid);
}



public class PlanetMovable : MonoBehaviour
{
    public MeasuringJumpHeight measuringJumpHeight;
    public SlopeForceMonitor slopeForceMonitor;

    public GravityDirectionMonitor gravityDirectionMonitor;

    MoveForceSelector moveForceSelector;
    public MonoBehaviour moveForceSelectorSocket;

    MoveForceMonitor moveForceMonitor;
    public MonoBehaviour moveForceMonitorSocket;

    MoveController moveController;
    public MonoBehaviour moveControllerSocket;

    public Rigidbody rigid;
    public float rotationSpeed = 6f;
    public bool firstPersonMode = false;
    public float backOffset = -0.1f;

    int onAirHash;
    Animator animator;

    //https://docs.unity3d.com/Manual/ExecutionOrder.html
    List<ContactPoint[]> contactPointGround;
    List<ContactPoint[]> contactPointWall;
    bool contactGround;
    bool touchWall;
    bool isHit = false;
    bool ladding = false;
    bool isTurble=false;

    static readonly float isHitDistance = 0.2f;
    public static readonly float rayCastDistanceToGround = 2;
    Vector3 groundUp;
    Vector3 gravityDir;
    Vector3 planeNormal;
    Vector3 wallNormal;
   
    void setAnimatorInfo()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            return;

        onAirHash = Animator.StringToHash("Base Layer.onAir");
    }

    // Use this for initialization
    void Awake () {

        Debug.Assert(moveForceMonitorSocket != null);

        contactPointGround = new List<ContactPoint[]>();
        contactPointWall= new List<ContactPoint[]>();
        setAnimatorInfo();

        if (moveControllerSocket != null)
            moveController = moveControllerSocket as MoveController;

        moveForceMonitor = moveForceMonitorSocket as MoveForceMonitor;

        if (moveForceSelectorSocket != null)
        {
            moveForceSelector = moveForceSelectorSocket as MoveForceSelector;
            moveForceSelector.resetByGroundType(GroundType.Normal, rigid);
        }
           
    }

    public void ResetGravityGenetrator(GravityGeneratorEnum pggEnum)
    {
        gravityDirectionMonitor.ResetGravityGenerator(pggEnum);
    }

    private void FixedUpdate()
    {
        if (animator != null)
        {
            bool moving = rigid.velocity.magnitude > 0.05;
            animator.SetBool("moving", moving);
        }
    }

    bool doJump=false;
    private void Update()
    {
        //判定有沒有接觸
        contactGround = isContactGround();
        touchWall =isTouchWall();

        if (moveController == null)
            return;

        //這邊要加上if (!doJump)的判斷，因為：
        //如果在|frame1|按下跳，其實會在|frame2|的Update裡才執行GetButtonDown檢查(在同個Frame裡FixedUpdate會先於Update執行)
        //這時GetButtonDown為true，但要等到|frame3|才會執行到fixedUPdate
        //如果|frame3|裡沒有fixedUpdate，接著還是會執行Update，這時GetButtonDown檢查已經變成false了
        //所以到|frame4|時執行fixedUpdate還是不會跳

        // |frame1| |frame2||frame3||frame4|
        //http://gpnnotes.blogspot.tw/2017/04/blog-post_22.html
        if (!doJump)
            doJump = moveController.doJump();
    }

    private void getGroundNormalNow(out bool isHit)
    {
        RaycastHit hit;
        //https://www.youtube.com/watch?v=Cq_Wh8o96sc
        //往後退一步，下斜坡不卡住(因為在交界處有如果直直往下打可能打中斜坡)
        Vector3 from = transform.forward * backOffset + groundUp + transform.position;
        //Debug.DrawRay(from, -groundUp*2 , Color.red);
        isHit = false;
        int layerMask = 1 << LayerDefined.ground | 1 << LayerDefined.groundNotBlockCamera;
        planeNormal = groundUp;
        if (Physics.Raycast(from, -groundUp, out hit, rayCastDistanceToGround, layerMask))
        {
            Vector3 diff = hit.point - transform.position;
            float height = Vector3.Dot(diff, groundUp);

            planeNormal = hit.normal;

            float distance = diff.magnitude;
            //如果距離小於某個值就判定是在地面上
            if (distance < isHitDistance)
            {
                isHit = true;
            }   
        }
    }

    void OnCollisionStay(Collision collision)
    {
        bool ground = collision.gameObject.layer == LayerDefined.ground;
        bool groundNotBlockCamera = collision.gameObject.layer == LayerDefined.groundNotBlockCamera;

        if (ground || groundNotBlockCamera)
        {
            //有可能同時碰到2個以上的物件，所以先收集起來
            contactPointGround.Add(collision.contacts);
        }

        bool wall = collision.gameObject.layer == LayerDefined.wall;
        bool wallNotBlockCamera = collision.gameObject.layer == LayerDefined.wallNotBlockCamera;
        if (wall | wallNotBlockCamera)
        {
            //有可能同時碰到2個以上的物件，所以先收集起來
            contactPointWall.Add(collision.contacts);
        }
    }

    bool isContactGround()
    {
        int listCount = contactPointGround.Count;
        for (int x = 0; x < listCount; x++)
        {
            ContactPoint[] cp = contactPointGround[x];
            for (int i = 0; i < cp.Length; i++)
            {
                Vector3 diif = cp[i].point - transform.position;
                float height = Vector3.Dot(groundUp, diif);
                if (height < 0.15f)
                {
                    return true;
                }
            }

        }
        return false;
    }

    Vector3 touchWallNormal;
    bool isTouchWall()
    {
        int listCount = contactPointWall.Count;
        for (int x = 0; x < listCount; x++)
        {
            ContactPoint[] cp = contactPointWall[x];
            for (int i = 0; i < cp.Length; i++)
            {
                Vector3 diif = cp[i].point - transform.position;
                float height = Vector3.Dot(groundUp, diif);
                if (height > 0.15f && height < 1.0f)
                {
                    touchWallNormal = cp[i].normal;
                    return true;
                }
            }

        }
        return false;
    }

    public void gravitySetup()
    {
        //計算重力方向
        groundUp = gravityDirectionMonitor.findGroundUp();
        gravityDir = -groundUp;
    }

    public void dataSetup()
    {
        //清空
        contactPointGround.Clear();
        contactPointWall.Clear();

        //設定面向
        Vector3 forward = Vector3.Cross(transform.right, groundUp);
        Quaternion targetRotation = Quaternion.LookRotation(forward, groundUp);
        transform.rotation = targetRotation;

        //判定是否擊中平面
        getGroundNormalNow(out isHit);

        //如果只用contact判定，下坡時可能contact為false
        ladding = contactGround || isHit;

        isTurble = false;
        if(moveController!=null)
            isTurble = moveController.doTurbo();
    }

    public void processGravity()
    {
        //如果在空中的重力加速度和在地面上時一樣，就會覺的太快落下
        rigid.AddForce(moveForceMonitor.getGravityForceStrength(!ladding) * gravityDir, ForceMode.Acceleration);
        //Debug.DrawRay(transform.position, gravityDir, Color.green);
    }

    public void processMoving()
    {
        if (moveController != null)
        {
            Vector3 moveForce = moveController.getMoveForce();
            //Debug.DrawLine(transform.position, transform.position + moveForce * 10, Color.blue);

            //在地面才作
            if (ladding)
            {
                moveForce = Vector3.ProjectOnPlane(moveForce, planeNormal);

                moveForce.Normalize();
                Debug.DrawRay(transform.position + transform.up, moveForce * 5, Color.blue);
            }

            //更新面向begin
            Vector3 forward2 = moveForce;
            if (forward2 != Vector3.zero && !firstPersonMode)
            {
                Quaternion targetRotation2 = Quaternion.LookRotation(forward2, groundUp);
                Quaternion newRot = Quaternion.Slerp(transform.rotation, targetRotation2, Time.deltaTime * rotationSpeed);
                transform.rotation = newRot;
            }
            //更新面向end

            //addForce可以有疊加的效果
            float moveForceStrength = moveForceMonitor.getMoveForceStrength(!ladding, isTurble);
            Vector3 moveForceWithStrength = moveForceStrength * moveForce;
            if (slopeForceMonitor != null && ladding)
            {
                moveForceWithStrength = slopeForceMonitor.modifyMoveForce(moveForce, moveForceStrength, moveForceMonitor.getGravityForceStrength(!ladding), groundUp, planeNormal);
            }

            rigid.AddForce(moveForceWithStrength, ForceMode.Acceleration);
        }
    }

    public void processLadding()
    {
        if (ladding)
        {
            if (animator != null)
            {
                bool isOnAir = onAirHash == animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
                if (isOnAir)
                {
                    animator.SetBool("onAir", false);
                    if (measuringJumpHeight != null)
                        measuringJumpHeight.stopRecord();
                }       
            }
        }
    }

    public void processWallJump()
    {
        //jump from wall
        if (!ladding && touchWall)
        {
            if (doJump)
            {
                rigid.AddForce(moveForceMonitor.getJumpForceStrength(isTurble) * -gravityDir, ForceMode.Acceleration);
                float s = 20;
                rigid.AddForce(s * touchWallNormal, ForceMode.VelocityChange);
                doJump = false;
            }
        }
    }

    public void processJump()
    {
        //跳
        if (ladding)
        {
            Debug.DrawLine(transform.position, transform.position - transform.up, Color.green);
            if (doJump)
            {
                if (measuringJumpHeight != null)
                    measuringJumpHeight.startRecord();

                rigid.AddForce(moveForceMonitor.getJumpForceStrength(isTurble) * -gravityDir, ForceMode.Acceleration);

                if (animator != null)
                    animator.SetBool("doJump", true);

                doJump = false;
            }
        }
        else
        {
            //不是ladding時按下doJump，也要把doJump設為false
            //不然的話會一直持續到當ladding為true再進行跳躍
            if (doJump)
                doJump = false;
        }
    }

    public Vector3 getGroundUp()
    {
        return groundUp;
    }

    public void resetGroundType(GroundType groundType)
    {
        if(moveForceSelector!=null)
            moveForceSelector.resetByGroundType(groundType,rigid);
    }
}
