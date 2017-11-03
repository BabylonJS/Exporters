using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Mesh Details", 4)]
    public sealed class MeshDetails: EditorScriptComponent
    {
        [Header("[Mesh Properties]")]
        public bool enableMesh = true;

        // TODO: Additional mesh details here (collision, lod, etc...)
        
        public bool generateCollider = true;

        public bool forceCheckCollisions = false;

        [Header("[Detail Properties]")]

        public BabylonPrefabProperties meshPrefabProperties = null;

        public BabylonPerformanceProperties meshRuntimeProperties = null;

        public BabylonEllipsoidProperties meshEllipsoidProperties = null;
        
        public BabylonOverrideVisibility meshVisibilityProperties = null;
    }

    [CustomEditor(typeof(MeshDetails)), CanEditMultipleObjects]
    public class MeshDetailsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MeshDetails myScript = (MeshDetails)target;
            // Validate Prefab Based Properties
            if (myScript.gameObject.layer != ExporterWindow.PrefabIndex) {
                if (myScript.meshPrefabProperties.makePrefabInstance == true) {
                    myScript.meshPrefabProperties.makePrefabInstance = false;
                }
            }
        }
    }
}