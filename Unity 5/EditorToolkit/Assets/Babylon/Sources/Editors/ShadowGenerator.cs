using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [RequireComponent(typeof(UnityEngine.Light))]
    [AddComponentMenu("Babylon/System Components/Shadow Map Generator", 99)]
    public sealed class ShadowGenerator : EditorScriptComponent
    {
        [Header("-Shadow Properties-")]
       
        [Range(128, 8192)]
        public int shadowMapSize = 1024;
        [Range(0.0f, 1.0f)]
        public float shadowMapBias = 0.00005f;
        public BabylonLightingFilter shadowMapFilter = BabylonLightingFilter.NoFilter;
        public bool shadowKernelBlur = false;
        [Range(0.0f, 256.0f)]
        public float shadowBlurKernel = 32.0f;
        [Range(0.0f, 10.0f)]
        public float shadowBlurScale = 2.0f;
        [Range(0.0f, 10.0f)]
        public float shadowBlurOffset = 0.0f;
        [Range(0.0f, 1.0f)]
        public float shadowOrthoScale = 0.5f;
        [Range(0.0f, 1.0f)]
        public float shadowStrengthScale = 1.0f;
        public float shadowDepthScale = 30.0f;
        public bool forceBackFacesOnly = false;
    }
}
