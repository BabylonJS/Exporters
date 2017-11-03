using System;
using UnityEditor;
using UnityEngine;

namespace UnityEditor
{
    [RequireComponent(typeof(UnityEditor.PhysicsState))]
    [AddComponentMenu("BabylonJS/Character Controller", 98)]
	public class CharacterSystem : EditorScriptComponent
	{
        [Header("[Movement Properties]")]

        [BabylonProperty]
        public float avatarHeight = 2.0f;
        [BabylonProperty]
        public float avatarRadius = 0.25f;
        [BabylonProperty, Range(0.01f, 1.0f)]
        public float fallingVelocity = 0.1f;

        [BabylonProperty, Range(0.01f, 10.0f)]
        public float slidingVelocity = 0.5f;
        [BabylonProperty]
        public BabylonMovementType movementType = BabylonMovementType.DirectVelocity;

		protected CharacterSystem()
		{
			this.babylonClass = "BABYLON.CharacterController";
		}
	}
}