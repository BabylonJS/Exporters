using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    [AddComponentMenu("Babylon/System Components/Camera Rig", 1)]
    public sealed class CameraRig : EditorScriptComponent
    {
        [Header("-Camera Properties-")]
        
        [BabylonProperty]
        public BabylonCameraOptions cameraType = BabylonCameraOptions.UniversalCamera;
        [BabylonProperty]
        public BabylonCameraInput cameraInput = BabylonCameraInput.NoCameraUserInput;
        [BabylonProperty, Range(0.0f, 20.0f)]
        public float cameraSpeed = 1.0f;
        [BabylonProperty, Range(0.0f, 1.0f)]
        public float inputMoveSpeed = 1.0f;
        [BabylonProperty, Range(0.0f, 10.0f)]
        public float inputRotateSpeed = 0.005f;
        [BabylonProperty, Range(0.0f, 10.0f)]
        public float inertiaScaleFactor = 0.9f;
        [BabylonProperty, Range(0.0f, 1.0f)]
        public float interaxialDistance = 0.0637f;
        [BabylonProperty]
        public MultiPlayerStartup multiPlayerStartup = MultiPlayerStartup.StartSinglePlayerView;
        [BabylonProperty]
        public bool multiPlayerElements = true;
        [BabylonProperty]
        public bool stereoSideBySide = true;
        [BabylonProperty]
        public bool preventDefaultEvents = true;
        [BabylonProperty]
        public bool checkCameraCollision = true;
        public BabylonArcRotateOptions arcRotateCameraOptions = null;
        public BabylonWebVirtualReality virtualRealityWebPlatform = null;
        public BabylonVirtualReality virtualRealityHeadsetOptions = null;
        public CameraRig()
        {
            this.babylonClass = "BABYLON.UniversalCameraRig";
        }
    }

    [CustomEditor(typeof(CameraRig)), CanEditMultipleObjects]
    public class CameraRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            CameraRig script = (CameraRig)target;
        }
    }    
}
