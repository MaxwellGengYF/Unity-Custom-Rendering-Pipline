using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct DeferredLighting {
    private static int _CurrentLightDir = Shader.PropertyToID("_CurrentLightDir");
    private static int _LightFinalColor = Shader.PropertyToID("_LightFinalColor");
    private static int _CubeMap = Shader.PropertyToID("_CubeMap");
    public Material lightingMat;
    public Light directionalLight;
    public Cubemap cubemap;

    public void DrawLight(RenderTexture[] gbuffers, int[] gbufferIDs, RenderTexture target, Camera cam)
    {
        lightingMat.SetVector(_CurrentLightDir, -directionalLight.transform.forward);
        lightingMat.SetVector(_LightFinalColor, directionalLight.color * directionalLight.intensity);
        lightingMat.SetTexture(_CubeMap, cubemap);
        Graphics.Blit(null, target, lightingMat, 0);
    }
}
