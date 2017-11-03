using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using Unity3D2Babylon;

namespace UnityEditor
{
    [RequireComponent(typeof(UnityEngine.Animator))]
    [AddComponentMenu("BabylonJS/Animation State", 25)]
    public sealed class AnimationState : EditorScriptComponent
    {
        [Header("[Animation Properties]")]
        
        [BabylonProperty]
        public BabylonAnimationMode controlType = BabylonAnimationMode.None;
        [BabylonProperty]
        public BabylonAnimationLooping loopSettings = BabylonAnimationLooping.AnimationClip;
        [BabylonProperty]
        public bool enableEvents = true;
        [BabylonProperty]
        public bool automaticPlay = true;
        [BabylonProperty, Range(0.0f, 10.0f)]
        public float playbackSpeed = 1.0f;
        [BabylonProperty, Range(0.0f, 5.0f)]
        public float defaultBlending = 0.0f;

        [Header("[Translation Properties]")]

        [BabylonProperty, Range(0.0f, 100.0f)]
        public float clampFeetPositions = 0.0f;
        [BabylonProperty]
        public BabylonAnimationBaking bakeRootTransforms = BabylonAnimationBaking.GameBlend;

        [Header("[State Machine Properties]")]

        [BabylonProperty]
        public bool enableStateMachine = true;
        [Unity3D2Babylon.ReadOnly]
        public Animator stateMachineSource = null;

        public AnimationState()
        {
            this.babylonClass = "BABYLON.AnimationState";
            this.OnExportProperties = this.OnExportPropertiesHandler;
        }

        void OnAnimatorMove()
        {
            // ******************************************* //
            // Note: Keep Scripting Root Motion Reference  //
            // ******************************************* //
             //Animator animator = GetComponent<Animator>(); 
             //if (animator) {
                //float speed = 2.5f;
                //Vector3 newPosition = transform.position;
                //newPosition.z += speed * Time.deltaTime; 
                //transform.position = newPosition;
                //transform.position = animator.rootPosition;
                //transform.rotation = animator.rootRotation;
             //}            
        }        
        public void OnExportPropertiesHandler(GameObject unityGameObject, Dictionary<string, object> propertyBag)
        {
            string stateMachineName = "Unknown";
            if (this.stateMachineSource != null) stateMachineName = this.stateMachineSource.name;
            propertyBag.Add("stateMachineName", stateMachineName);
        }
    }

    [CustomEditor(typeof(AnimationState)), CanEditMultipleObjects]
    public class AnimationStateEditor : Editor
    {
        public void OnEnable()
        {
            AnimationState myScript = (AnimationState)target;
            myScript.stateMachineSource = myScript.GetComponent<Animator>();
        }
    }
}
