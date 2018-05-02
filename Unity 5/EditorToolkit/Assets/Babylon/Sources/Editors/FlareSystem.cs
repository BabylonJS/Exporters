using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Lens Flare Systems", 96)]
    public sealed class FlareSystem : EditorScriptComponent
    {
        [Header("-Flare Properties-")]
        
        public string flareName = String.Empty;
        public int borderLimit = 300;
        public UnityLensFlareItem[] lensFlares = null;
    }
}