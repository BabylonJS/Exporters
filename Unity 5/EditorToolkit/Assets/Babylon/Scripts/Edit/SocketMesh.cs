using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Socket Mesh", 3)]
    public sealed class SocketMesh: EditorScriptComponent
    {
        [Header("[Socket Properties]")]
        public bool createSocketMesh = true;
        public Vector3 socketMeshPosition = Vector3.zero;
        public Vector3 socketMeshRotation = Vector3.zero;
        public BabylonSocketProperties socketPrefabProperties = null;
    }

    [CustomEditor(typeof(SocketMesh)), CanEditMultipleObjects]
    public class SocketMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SocketMesh myScript = (SocketMesh)target;
            // Validate Prefab Based Properties
            if (myScript.gameObject.layer != ExporterWindow.PrefabIndex) {
                //if (myScript.socketPrefabProperties.defaultSocketPrefab == true) {
                //    myScript.socketPrefabProperties.defaultSocketPrefab = false;
                //}
            }
        }
    }
}