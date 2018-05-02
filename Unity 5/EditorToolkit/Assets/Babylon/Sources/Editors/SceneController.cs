using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Default Scene Controller", 1000)]
    public sealed class SceneController : EditorScriptComponent
    {
        [Header("-Default Properties-")]
        public BabylonSceneOptions sceneOptions;
        public BabylonConfigOptions configOptions;
        [Header("-Render Properties-")]
        public BabylonSkyboxOptions skyboxOptions;
        public BabylonLightOptions lightingOptions;
        [Header("-Offline Properties-")]
        public BabylonManifestOptions manifestOptions;
    }
}
