using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Default Scene Controller", 100)]
    public sealed class SceneController : EditorScriptComponent
    {
        [Header("[Default Properties]")]
        public BabylonSceneOptions sceneOptions;
        public BabylonConfigOptions configOptions;
        public BabylonSkyboxOptions skyboxOptions;
        public BabylonLightOptions lightingOptions;
        public BabylonManifestOptions manifestOptions;
    }
}
