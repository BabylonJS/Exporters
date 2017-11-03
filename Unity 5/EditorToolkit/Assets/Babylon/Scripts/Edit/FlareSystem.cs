using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Lens Flare Systems", 96)]
    public sealed class FlareSystem : EditorScriptComponent
    {
        [Header("[Flare Properties]")]
        
        public string flareName = String.Empty;
        public int borderLimit = 300;
        public UnityLensFlareItem[] lensFlares = null;
    }
}