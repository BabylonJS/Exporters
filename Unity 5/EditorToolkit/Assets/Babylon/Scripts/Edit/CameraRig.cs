using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Camera Rig", 0)]
    public sealed class CameraRig : EditorScriptComponent
    {
        [Header("[Camera Properties]")]
        
        public BabylonCameraOptions cameraType = BabylonCameraOptions.UniversalCamera;
        public BabylonCameraInput cameraInput = BabylonCameraInput.NoCameraUserInput;
        [Range(0.0f, 20.0f)]
        public float cameraSpeed = 1.0f;
        [Range(0.0f, 1.0f)]
        public float inputMoveSpeed = 1.0f;
        [Range(0.0f, 10.0f)]
        public float inputRotateSpeed = 0.005f;
        [Range(0.0f, 10.0f)]
        public float inertiaScaleFactor = 0.9f;
        [Range(0.0f, 1.0f)]
        public float interaxialDistance = 0.0637f;
        public bool preventDefaultEvents = false;
        public bool stereoscopicSideBySide = true;
        public BabylonWebVirtualReality virtualRealityWebPlatform = null;
        public BabylonVirtualReality virtualRealityHeadsetOptions = null;
        public BabylonRenderPipeline highDynamicRenderingPipeline = null;
    }
}
