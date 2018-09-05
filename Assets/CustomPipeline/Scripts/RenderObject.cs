using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderObject : MonoBehaviour
{
    public Mesh targetMesh;
    public Material targetMaterial;
    [System.NonSerialized]
    public Matrix4x4 localToWorldMatrices;
    [System.NonSerialized]
    public Vector3 extent;
    public void Init()
    {
        localToWorldMatrices = transform.localToWorldMatrix;
        extent = targetMesh.bounds.extents;
    }
}
