using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Asset References", 90)]
    public sealed class AssetReferences : EditorScriptComponent
    {
        [Header("[Reference Properties]")]
        
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
