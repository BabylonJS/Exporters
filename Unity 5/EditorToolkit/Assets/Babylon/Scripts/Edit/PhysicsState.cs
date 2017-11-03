using System;
using UnityEngine;

namespace UnityEditor
{
    [AddComponentMenu("BabylonJS/Physics State", 6)]
    public sealed class PhysicsState : EditorScriptComponent
    {
        [Header("[Physics Properties]")]

        public BabylonCollisionType type = BabylonCollisionType.Collider;
        public float mass = 1.0f;
        public float friction = 0.1f;
        public float restitution = 0.2f;
        public BabylonPhysicsImposter imposter = BabylonPhysicsImposter.Box;
        public BabylonPhysicsRotation rotation = BabylonPhysicsRotation.Normal;
    }

    

}