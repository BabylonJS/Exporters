using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Shadow Map", 2)]
    public sealed class ShadowMap : EditorScriptComponent
    {
        [Header("[Shadow Properties]")]
        public BabylonEnabled runtimeShadows = BabylonEnabled.Enabled;
    }
}