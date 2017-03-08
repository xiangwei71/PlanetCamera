﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface FollowCameraBehavior
{
    void rotateByAxis(float angle, Vector3 axis);
}

public interface MoveController
{
    Vector3 getMoveForce();
    bool doJump();
}

public class PlanetPlayerController : MonoBehaviour, MoveController
{

    public PlanetMovable planetMovable;
    FollowCameraBehavior followCameraBehavior;
    public MonoBehaviour followCameraBehaviorSocket;
    public bool adjustCameraWhenMove = true;
    InputProxy inputProxy;
    public MonoBehaviour inputPorxySocket;
    Transform m_Cam;

    Vector3 previousPosistion;
    Vector3 previousGroundUp;

    // Use this for initialization
    void Start () {
        previousPosistion = transform.position;
        previousGroundUp = planetMovable.getGroundUp();

        if (followCameraBehaviorSocket != null)
            followCameraBehavior = followCameraBehaviorSocket as FollowCameraBehavior;

        //print("cameraBehavior="+cameraBehavior);

        if (inputPorxySocket != null)
            inputProxy = inputPorxySocket as InputProxy;

        m_Cam = Camera.main.transform;
    }

    //void FixedUpdate()
    void Update()
    {
        //如果在FixedUpdate做會抖動？
        if (adjustCameraWhenMove)
            doAdjust();
    }

    void doAdjust()
    {
        //如果位置有更新，就更新FlowPoint
        //透過groundUp和向量(nowPosition-previouPosistion)的外積，找出旋轉軸Z

        Vector3 groundUp = planetMovable.getGroundUp();
        
        Vector3 diffV = transform.position - previousPosistion;

        Vector3 averageGroundUp = (groundUp + previousGroundUp)/2;
        Vector3 Z = Vector3.Cross(averageGroundUp, diffV);
        Z.Normalize();
        Debug.DrawLine(transform.position, transform.position + previousGroundUp * 16, Color.red);

        //算出2個frame之間在planet上移動的角度差
        float cosValue = Vector3.Dot(previousGroundUp, groundUp);

        //http://answers.unity3d.com/questions/778626/mathfacos-1-return-nan.html
        //上面說Dot有可能會>1或<-1
        cosValue = Mathf.Max(-1.0f, cosValue);
        cosValue = Mathf.Min(1.0f, cosValue);

        float rotDegree = Mathf.Acos(cosValue) * Mathf.Rad2Deg;

        //print("rotDegree=" + rotDegree);

        if (followCameraBehavior != null)
           followCameraBehavior.rotateByAxis(rotDegree, Z);

        previousPosistion = transform.position;
        previousGroundUp = groundUp;
    }

    public Vector3 getMoveForce()
    {
        //取得輸入
        Vector2 hv = inputProxy.getHV();
        float h = hv.x;
        float v = hv.y;

        if (h != 0 || v != 0)
        {
            //由camera向方計算出角色的移動方向
            Vector3 controllForce = h * m_Cam.right + v * m_Cam.up;
            return controllForce;
        }

        return Vector3.zero;
    }

    public bool doJump()
    {
        return inputProxy.pressJump();
    }
}
