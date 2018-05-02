using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Asset References", 90)]
    public sealed class AssetReferences : EditorScriptComponent
    {
        [Header("-Reference Properties-")]
        
        [BabylonProperty]
        public TextAsset[] textAssets = null;

        [BabylonProperty]
        public AudioClip[] audioAssets = null;

        [BabylonProperty]
        public Texture2D[] textureAssets = null;

        [BabylonProperty]
        public Material[] materialAssets = null;

        [BabylonProperty]
        public Cubemap[] cubemapAssets = null;

        [BabylonProperty]
        public DefaultAsset[] defaultFileAssets = null;
    }
}
