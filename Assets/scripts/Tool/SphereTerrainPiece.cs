﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereTerrainPiece : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {
        var mRenderer = GetComponent<MeshRenderer>();
        mRenderer.enabled = true;
        var m = mRenderer.material;
        m.SetVector("_local_pos", transform.localPosition);

        var meshFilter = this.GetComponent<MeshFilter>();
        var bounds = meshFilter.sharedMesh.bounds;
        var nMin = toSphere(bounds.min);
        var nMax = toSphere(bounds.max);

        var newCenter = (nMax + nMin) / 2;
        var size = nMax - nMin;
        // print(size);
        meshFilter.mesh.bounds = new Bounds(newCenter, size);
        // print("modify");
    }

    // Update is called once per frame
    void Update()
    {

    }

    Vector3 toSphere(Vector3 v)
    {
        var localV = v + transform.localPosition;
        var nV = localV.normalized;
        var R = 510.0f;
        return nV * R - transform.localPosition;
    }
}
