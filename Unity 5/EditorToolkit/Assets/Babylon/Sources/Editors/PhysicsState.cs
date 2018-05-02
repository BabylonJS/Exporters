using System;
using UnityEngine;
using Unity3D2Babylon;

namespace UnityEditor
{
    [AddComponentMenu("Babylon/System Components/Physics State", 8)]
    public sealed class PhysicsState : EditorScriptComponent
    {
        [Header("-Physics Properties-")]

        public BabylonCollisionType type = BabylonCollisionType.Collider;
        public float mass = 1.0f;
        public float friction = 0.3f;
        public float restitution = 0.3f;
        public BabylonPhysicsImposter imposter = BabylonPhysicsImposter.Box;

        [Header("-Runtime Properties-")]

        public BabylonPhysicsRotation angularRotation = BabylonPhysicsRotation.Normal;
        
        public bool detachFromParent = false;

        [Header("-Collision Properties-")]

        public BabylonCollisionFilter filterGroup = BabylonCollisionFilter.GROUP1;
        public BabylonCollisionMask collisionMask = new BabylonCollisionMask();
    }
}