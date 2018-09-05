using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
public struct CullMesh : IJobParallelFor
{
    public static RenderObject[] allObjects;
    private static Plane[] frustumPlanes = new Plane[6];
    public static Vector3 cameraPos;
    public static float cameraFarClipDistance;
    public static void UpdateFrame(Camera cam, ref Matrix4x4 invvp, Vector3 cameraPosition)
    {
        GetCullingPlanes(ref invvp);
        cameraFarClipDistance = cam.farClipPlane;
        cameraPos = cameraPosition;
    }

    public static void GetCullingPlanes(ref Matrix4x4 invVp)
    {
        Vector3 nearLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 1));
        Vector3 nearLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 1));
        Vector3 nearRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 1));
        Vector3 nearRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 1));
        Vector3 farLeftButtom = invVp.MultiplyPoint(new Vector3(-1, -1, 0));
        Vector3 farLeftTop = invVp.MultiplyPoint(new Vector3(-1, 1, 0));
        Vector3 farRightButtom = invVp.MultiplyPoint(new Vector3(1, -1, 0));
        Vector3 farRightTop = invVp.MultiplyPoint(new Vector3(1, 1, 0));
        //Near
        frustumPlanes[0] = new Plane(nearRightTop, nearRightButtom, nearLeftButtom);
        //Up
        frustumPlanes[1] = new Plane(farLeftTop, farRightTop, nearRightTop);
        //Down
        frustumPlanes[2] = new Plane(nearRightButtom, farRightButtom, farLeftButtom);
        //Left
        frustumPlanes[3] = new Plane(farLeftButtom, farLeftTop, nearLeftTop);
        //Right
        frustumPlanes[4] = new Plane(farRightButtom, nearRightButtom, nearRightTop);
        //Far
        frustumPlanes[5] = new Plane(farLeftButtom, farRightButtom, farRightTop);
    }

    private static bool PlaneTest(ref Matrix4x4 ObjectToWorld, ref Vector3 extent, out Vector3 position)
    {
        Vector3 right = new Vector3(ObjectToWorld.m00, ObjectToWorld.m10, ObjectToWorld.m20);
        Vector3 up = new Vector3(ObjectToWorld.m01, ObjectToWorld.m11, ObjectToWorld.m21);
        Vector3 forward = new Vector3(ObjectToWorld.m02, ObjectToWorld.m12, ObjectToWorld.m22);
        position = new Vector3(ObjectToWorld.m03, ObjectToWorld.m13, ObjectToWorld.m23);
        for (int i = 0; i < 6; ++i)
        {
            Plane plane = frustumPlanes[i];
            float r = Vector3.Dot(position, plane.normal);
            Vector3 absNormal = new Vector3(Mathf.Abs(Vector3.Dot(plane.normal, right)), Mathf.Abs(Vector3.Dot(plane.normal, up)), Mathf.Abs(Vector3.Dot(plane.normal, forward)));
            float f = Vector3.Dot(absNormal, extent);
            if ((r - f) >= -plane.distance)
                return false;
        }
        return true;
    }

    public void Execute(int i)
    {
        RenderObject obj = allObjects[i];
        Vector3 position;
        
        if (PlaneTest(ref obj.localToWorldMatrices, ref obj.extent, out position))
        {
            float distance = Vector3.Distance(position, cameraPos);
            float layer = distance / cameraFarClipDistance;
            int layerValue = (int)Mathf.Clamp(Mathf.Lerp(0, SortMesh.LAYERCOUNT, layer), 0, SortMesh.LAYERCOUNT - 1);
            SortMesh.sorts[layerValue].Add(distance, obj);
        }
    }

    public static JobHandle Schedule()
    {
        return (new CullMesh()).Schedule(allObjects.Length, 64);
    }
}
