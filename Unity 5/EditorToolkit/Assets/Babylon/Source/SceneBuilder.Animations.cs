using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BabylonExport.Entities;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Unity3D2Babylon
{
    partial class SceneBuilder
    {
        private static void ExportAnimations(Transform transform, BabylonIAnimatable animatable)
        {
            var animator = transform.gameObject.GetComponent<Animator>();
            
            // if transform is root object
            if (transform.parent == null)
            {
                // Do not apply animation to root
                animator = null;
            }
            // else if my root has Animator
            else if (transform.root && transform.root.gameObject.GetComponent<Animator>())
            {
                animator = transform.root.gameObject.GetComponent<Animator>();
            }

            if (animator != null)
            {
                AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
                if (ac == null)
                {
                    return;
                }
                var layer = ac.layers[0];
                if (layer == null)
                {
                    return;
                }
                AnimatorStateMachine sm = layer.stateMachine;
                if (sm.states.Length > 0)
                {
                    var state = sm.states[0].state; // We only support the first one
                    AnimationClip clip = state.motion as AnimationClip;
                    if (clip != null)
                    {
                        ExportAnimationClip(clip, true, animatable, transform.name);
                    }
                }
            }
            else
            {
                var animation = transform.gameObject.GetComponent<Animation>();
                if (animation != null && animation.clip != null)
                {
                    ExportAnimationClip(animation.clip, animation.playAutomatically, animatable, transform.name);
                }
            }
        }

        private static bool IsRotationQuaternionAnimated(BabylonIAnimatable animatable)
        {
            if (animatable.animations == null)
            {
                return false;
            }

            return animatable.animations.Any(animation => animation.property.Contains("rotationQuaternion"));
        }

        private static void ExportSkeletonAnimationClips(Animator animator, bool autoPlay, BabylonSkeleton skeleton, Transform[] bones, BabylonMesh babylonMesh)
        {
            AnimationClip clip = null;
            AnimatorController ac = animator.runtimeAnimatorController as AnimatorController;
            if (ac == null)
            {
                return;
            }
            var layer = ac.layers[0];
            if (layer == null)
            {
                return;
            }
            AnimatorStateMachine sm = layer.stateMachine;
            if (sm.states.Length > 0)
            {
                // Only the first state is supported so far.
                var state = sm.states[0].state;
                clip = state.motion as AnimationClip;
            }

            if (clip == null)
            {
                return;
            }

            ExportSkeletonAnimationClipData(animator, autoPlay, skeleton, bones, babylonMesh, clip);
        }

        private static void ExportSkeletonAnimationClipData(Animator animator, bool autoPlay, BabylonSkeleton skeleton, Transform[] bones, BabylonMesh babylonMesh, AnimationClip clip)
        {
            var frameTime = 1.0f / clip.frameRate;
            int animationFrameCount = (int)(clip.length * clip.frameRate);

            if (autoPlay)
            {
                babylonMesh.autoAnimate = true;
                babylonMesh.autoAnimateFrom = 0;
                babylonMesh.autoAnimateTo = animationFrameCount;
                babylonMesh.autoAnimateLoop = true;
            }

            foreach (var bone in skeleton.bones)
            {
                var keys = new List<BabylonAnimationKey>();
                var transform = bones.Single(b => b.name == bone.name);

                AnimationMode.BeginSampling();
                for (var i = 0; i < animationFrameCount; i++)
                {
                    clip.SampleAnimation(animator.gameObject, i * frameTime);

                    var local = (transform.parent.localToWorldMatrix.inverse * transform.localToWorldMatrix);
                    float[] matrix = new[] {
                        local[0, 0], local[1, 0], local[2, 0], local[3, 0],
                        local[0, 1], local[1, 1], local[2, 1], local[3, 1],
                        local[0, 2], local[1, 2], local[2, 2], local[3, 2],
                        local[0, 3], local[1, 3], local[2, 3], local[3, 3]
                    };

                    var key = new BabylonAnimationKey
                    {
                        frame = i,
                        values = matrix,
                    };
                    keys.Add(key);
                }
                AnimationMode.EndSampling();

                var babylonAnimation = new BabylonAnimation
                {
                    name = bone.name + "Animation",
                    property = "_matrix",
                    dataType = (int)BabylonAnimation.DataType.Matrix,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    framePerSecond = (int)clip.frameRate,
                    keys = keys.ToArray()
                };

                bone.animation = babylonAnimation;
            }
        }

        private static void ExportAnimationClip(AnimationClip clip, bool autoPlay, BabylonIAnimatable animatable, string name)
        {
            var curveBindings = AnimationUtility.GetCurveBindings(clip);
            var animations = new List<BabylonAnimation>();

            var maxFrame = 0;

            // get Quatanion values and inTangent and outTangent List for one mesh
            List<CurveValueData> curveDataItems = CurveUtil.GetQuatanionItems(curveBindings, clip, name);

            foreach (var binding in curveBindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                string property;

                // pass only my item
                var pathItems = binding.path.Split('/');
                if (pathItems[pathItems.Length - 1] != name)
                {
                    continue;
                }

                switch (binding.propertyName)
                {
                    case "m_LocalPosition.x":
                        property = "position.x";
                        break;
                    case "m_LocalPosition.y":
                        property = "position.y";
                        break;
                    case "m_LocalPosition.z":
                        property = "position.z";
                        break;

                    // EulerAnglesRaw
                    case "localEulerAnglesRaw.x":
                        property = "rotation.x";
                        break;
                    case "localEulerAnglesRaw.y":
                        property = "rotation.y";
                        break;
                    case "localEulerAnglesRaw.z":
                        property = "rotation.z";
                        break;

                    // Quaternion rotation
                    // if rotation interpolation type is Quaternion, Quaternion xyzw curves exist.
                    // so we use only one curve -> "m_LocalRotation.x"
                    case "m_LocalRotation.x":
                        property = "rotationQuaternion";
                        break;
                    // canceling
                    case "m_LocalRotation.y":
                    case "m_LocalRotation.z":
                    case "m_LocalRotation.w":
                        continue;
                        break;

                    case "m_LocalScale.x":
                        property = "scaling.x";
                        break;
                    case "m_LocalScale.y":
                        property = "scaling.y";
                        break;
                    case "m_LocalScale.z":
                        property = "scaling.z";
                        break;
                    default:
                        continue;
                }

                var isLocalEulerAnglesRaw = binding.propertyName.IndexOf("localEulerAnglesRaw") >= 0;
                var isRotationQuaternion = binding.propertyName.IndexOf("m_LocalRotation") >= 0;

                var babylonAnimation = new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    
                    framePerSecond = (int)clip.frameRate,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                };

                // rotationQuaternion curve
                if (isRotationQuaternion)
                {
                    var ratio = 1 / clip.frameRate;
                    var keys = new List<BabylonAnimationKey>();
                    
                    for (int i = 0; i < curve.keys.Length; i++)
                    {
                        float time = curve.keys[i].time;
                        // we use merged array (value, inTangent and outTangent)
                        var values = curveDataItems[i].getValuesAndTowTangentArray();

                        // if BabylonAnimationKey has inTangent property, we can use it.
                        //var inTangent = curveDataItems[i].getInTangentArray();
                        //var outTangent = curveDataItems[i].getOutTangentArray();

                        var babylonAnimationKey = new BabylonAnimationKey();
                        babylonAnimationKey.frame = (int)(time * clip.frameRate);
                        babylonAnimationKey.values = values;
                        
                        keys.Add(babylonAnimationKey);
                    }

                    babylonAnimation.keys = keys.ToArray();

                    babylonAnimation.name = "quaternion animation";
                    babylonAnimation.property = "rotationQuaternion";
                    babylonAnimation.dataType = (int)BabylonAnimation.DataType.Quaternion;
                }
                // isLocalEulerAnglesRaw or other parameter
                else
                {
                    var ratio = 1/clip.frameRate * (isLocalEulerAnglesRaw ? (float)Math.PI / 180 : 1);

                    babylonAnimation.keys = curve.keys.Select(keyFrame => new BabylonAnimationKey
                    {
                        frame = (int)(keyFrame.time * clip.frameRate),
                        values = new[] { isLocalEulerAnglesRaw ? keyFrame.value * (float)Math.PI / 180 : keyFrame.value
                        // we use merged array
                        ,keyFrame.inTangent * ratio,
                        keyFrame.outTangent * ratio
                        }

                    }).ToArray();
                }

                maxFrame = Math.Max(babylonAnimation.keys.Last().frame, maxFrame);

                animations.Add(babylonAnimation);
            }

            if (animations.Count > 0)
            {
                animatable.animations = animations.ToArray();
                if (autoPlay)
                {
                    animatable.autoAnimate = true;
                    animatable.autoAnimateFrom = 0;
                    animatable.autoAnimateTo = maxFrame;
                    animatable.autoAnimateLoop = clip.isLooping;
                }
            }
        }
    }
}
