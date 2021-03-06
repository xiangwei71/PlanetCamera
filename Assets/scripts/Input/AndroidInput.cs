﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AndroidInput : MonoBehaviour, InputProxy
{
    bool isFire;
    bool isJump;
    Vector2 memoryPos;
    Vector2 nowInput;

    Vector2 InputProxy.getHV()
    {
        float h = CrossPlatformInputManager.GetAxis("Horizontal");
        float v = CrossPlatformInputManager.GetAxis("Vertical");
        return new Vector2(h, v);
    }

    bool InputProxy.pressJump()
    {
        if (isJump)
        {
            isJump = false;
            return true;
        }
        else
            return false;
    }

    bool InputProxy.pressFire()
    {
        return false;
    }

    bool InputProxy.holdFire()
    {
        return isFire;
    }

    public void doJump()
    {
        isJump = true;
    }

    public void toggleTurbo(Button btn)
    {
        isFire = !isFire;
        var colors = btn.colors;
        colors.normalColor = isFire ? Color.yellow : Color.gray;
        colors.highlightedColor = isFire ? Color.yellow : Color.gray;
        btn.colors = colors;
    }

    float scale = 0.25f;
    float InputProxy.yawScale()
    {
        return scale;
    }

    float InputProxy.pitchScale()
    {
        return scale;
    }

    bool InputProxy.enableControlUI()
    {
        return true;
    }
}
