using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Socket Mesh", 4)]
    public sealed class SocketMesh: EditorScriptComponent
    {
        [Header("-Socket Properties-")]
        public bool enableSocket = true;
        public string socketName = String.Empty;
        public Vector3 socketPosition = Vector3.zero;
        public Vector3 socketRotation = Vector3.zero;
    }

    [CustomEditor(typeof(SocketMesh)), CanEditMultipleObjects]
    public class SocketMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SocketMesh myScript = (SocketMesh)target;
        }
    }
}