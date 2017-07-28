﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour {

    
    

    void OnTriggerEnter(Collider other)
    {
        if(!TagDefined.canOnMovableSet(other.gameObject.tag))
             return;

        other.transform.parent = transform;

        SetCameraPivot setCameraPivot = other.gameObject.GetComponent<SetCameraPivot>();
        if (setCameraPivot != null)
            setCameraPivot.setFollowHighSpeed(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!TagDefined.canOnMovableSet(other.gameObject.tag))
            return;

        other.transform.parent = null;

        SetCameraPivot setCameraPivot = other.gameObject.GetComponent<SetCameraPivot>();
        if (setCameraPivot != null)
            setCameraPivot.setFollowHighSpeed(false);
    }
}