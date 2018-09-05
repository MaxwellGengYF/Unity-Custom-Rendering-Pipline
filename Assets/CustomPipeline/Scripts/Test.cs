using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
public class Test : MonoBehaviour
{
    private static int _DepthTexture = Shader.PropertyToID("_DepthTexture");
    private static int _InvVP = Shader.PropertyToID("_InvVP");
    private RenderTexture cameraTarget;
    private RenderBuffer[] GBuffers;
    private RenderTexture[] GBufferTextures;
    private int[] gbufferIDs;
    public Transform[] cubeTransforms;
    public Mesh cubeMesh;
    public Material deferredMaterial;
    public SkyboxDraw skyDraw;
    public DeferredLighting lighting;
    private RenderTexture depthTexture;
    public RenderObject[] allRenderObjects;
    [Range(0.5f, 4f)]
    public float superSample = 1;
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        cameraTarget = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
        GBufferTextures = new RenderTexture[]
        {
            new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear),
            new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear)
        };
        depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
        GBuffers = new RenderBuffer[GBufferTextures.Length];
        for (int i = 0; i < GBuffers.Length; ++i)
        {
            GBuffers[i] = GBufferTextures[i].colorBuffer;
        }
        gbufferIDs = new int[]
        {
            Shader.PropertyToID("_GBuffer0"),
            Shader.PropertyToID("_GBuffer1"),
            Shader.PropertyToID("_GBuffer2"),
            Shader.PropertyToID("_GBuffer3"),
        };
        SortMesh.InitSortMesh(allRenderObjects.Length);
        CullMesh.allObjects = allRenderObjects;
        foreach (var i in allRenderObjects)
        {
            i.Init();
        }
    }
    int screenWidth;
    int screenHeight;
    private static void ReSize(RenderTexture rt, int width, int height)
    {
        rt.Release();
        rt.width = width;
        rt.height = height;
        rt.Create();
    }
    private void OnPostRender()
    {
        Camera cam = Camera.current;
        if (screenHeight != cam.pixelHeight * superSample || screenWidth != cam.pixelWidth * superSample)
        {
            screenHeight =(int) (cam.pixelHeight * superSample);
            screenWidth =(int)( cam.pixelWidth * superSample);
            ReSize(cameraTarget, screenWidth, screenHeight);
            ReSize(depthTexture, screenWidth, screenHeight);
            foreach(var i in GBufferTextures)
            {
                ReSize(i, screenWidth, screenHeight);
            }
            for (int i = 0; i < GBuffers.Length; ++i)
            {
                GBuffers[i] = GBufferTextures[i].colorBuffer;
            }
        }
        Shader.SetGlobalTexture(_DepthTexture, depthTexture);
        Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
        Matrix4x4 vp = proj * cam.worldToCameraMatrix;
        Matrix4x4 invvp = vp.inverse;
        Shader.SetGlobalMatrix(_InvVP, invvp);
        CullMesh.UpdateFrame(cam, ref invvp, transform.position);
        SortMesh.UpdateFrame();
        JobHandle cullhandle = CullMesh.Schedule();
        JobHandle sortHandle = SortMesh.Schedule(cullhandle);
        JobHandle.ScheduleBatchedJobs();
        for (int i = 0; i < gbufferIDs.Length; ++i)
        {
            Shader.SetGlobalTexture(gbufferIDs[i], GBufferTextures[i]);
        }
        Graphics.SetRenderTarget(GBuffers, depthTexture.depthBuffer);
        GL.Clear(true, true, Color.black);
        //Start Draw Mesh
        deferredMaterial.SetPass(0);
        sortHandle.Complete();
        for (int i = 0; i < SortMesh.sorts.Length; ++i)
        {
            DrawElements(ref SortMesh.sorts[i]);
        }
        lighting.DrawLight(GBufferTextures, gbufferIDs, cameraTarget, cam);
        //End Deferred Lighting
        skyDraw.DrawSkybox(cam, cameraTarget.colorBuffer, depthTexture.depthBuffer);
        Graphics.Blit(cameraTarget, cam.targetTexture);

    }

    public static void DrawElements(ref BinarySort<RenderObject> binarySort)
    {
        RenderObject[] objs = binarySort.meshes;
        for (int j = 0; j < binarySort.count; ++j)
        {
            Graphics.DrawMeshNow(objs[j].targetMesh, objs[j].localToWorldMatrices);
        }
    }
}
