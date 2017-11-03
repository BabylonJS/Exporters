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
        private static void ExportTransformAnimationClips(Transform transform, BabylonIAnimatable animatable, ref UnityMetaData metaData)
        {
            Animator animator = transform.gameObject.GetComponent<Animator>();
            Animation legacy = transform.gameObject.GetComponent<Animation>();
            if (legacy != null) UnityEngine.Debug.LogWarning("Legacy animation component not supported for game object: " + transform.gameObject.name);
            if (animator != null && animator.runtimeAnimatorController != null && animator.runtimeAnimatorController.animationClips != null && animator.runtimeAnimatorController.animationClips.Length > 0) {
                UnityEditor.AnimationState astate = transform.gameObject.GetComponent<UnityEditor.AnimationState>();
                if (astate == null) UnityEngine.Debug.LogWarning("AnimationState component not found for game object: " + transform.gameObject.name);
                if (astate != null && astate.isActiveAndEnabled == true && astate.controlType == BabylonAnimationMode.Transform) {
                    if (animator != null) animator.enabled = true;
                    List<AnimationClip> states = Tools.GetAnimationClips(animator);
                    if (states != null && states.Count > 0) {
                        ExportTransformAnimationClipData(animator.gameObject, transform, animatable, astate, ref states, ref metaData, animator);
                    }
                }
            }
        }

        private static void ExportTransformAnimationClipData(GameObject source, Transform transform, BabylonIAnimatable animatable, UnityEditor.AnimationState animationState, ref List<AnimationClip> states, ref UnityMetaData metaData, Animator animator)
        {
            ExporterWindow.ReportProgress(1, "Exporting transform clips: " + transform.gameObject.name);
            int frameRate = 0;
            int firstClipEnd = 0;
            int totalFrameCount = 0;
            List<string> stateNameCache = new List<string>();
            List<BabylonAnimation> animations = new List<BabylonAnimation>();

            var positionX = new List<BabylonAnimationKey>();
            var positionY = new List<BabylonAnimationKey>();
            var positionZ = new List<BabylonAnimationKey>();

            var rotationX = new List<BabylonAnimationKey>();
            var rotationY = new List<BabylonAnimationKey>();
            var rotationZ = new List<BabylonAnimationKey>();
            var rotationW = new List<BabylonAnimationKey>();

            var scaleX = new List<BabylonAnimationKey>();
            var scaleY = new List<BabylonAnimationKey>();
            var scaleZ = new List<BabylonAnimationKey>();

            int frameOffest = 0;
            float playbackSpeed = (animationState != null) ? animationState.playbackSpeed : 1.0f;
            foreach (var state in states)
            {
                if (state == null) continue;
                AnimationClip clip = state as AnimationClip;
                if (frameRate <= 0) frameRate = (int)clip.frameRate;
                //var frameTime = 1.0f / frameRate;
                int clipFrameCount = (int)(clip.length * frameRate);
                if (firstClipEnd <= 0) firstClipEnd = (clipFrameCount - 1);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                BabylonLoopBehavior behavior = (settings.loopTime) ? BabylonLoopBehavior.Cycle : BabylonLoopBehavior.Constant;
                if (settings.loopTime && settings.loopBlend) behavior = BabylonLoopBehavior.Relative;
                ExporterWindow.ReportProgress(1, "Transforming: " + transform.gameObject.name + " - "  + clip.name);
                // Set Animation State Meta Data
                if (!stateNameCache.Contains(clip.name)) {
                    stateNameCache.Add(clip.name);
                    // Animation Clip Information
                    Dictionary<string, object> animStateInfo = new Dictionary<string, object>();
                    animStateInfo.Add("type", "transform");
                    animStateInfo.Add("name", clip.name);
                    animStateInfo.Add("start", frameOffest);
                    animStateInfo.Add("stop", (frameOffest + clipFrameCount - 1));
                    animStateInfo.Add("rate", frameRate);
                    animStateInfo.Add("behavior", (int)behavior);
                    animStateInfo.Add("playback", playbackSpeed);
                    metaData.animationClips.Add(animStateInfo);
                }

                // Animation Curve Bindings
                var curveBindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var binding in curveBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    switch (binding.propertyName)
                    {
                        //Position
                        case "m_LocalPosition.x":
                            IEnumerable<BabylonAnimationKey> px_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            positionX.AddRange(px_keys);
                            break;
                        case "m_LocalPosition.y":
                            IEnumerable<BabylonAnimationKey> py_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            positionY.AddRange(py_keys);
                            break;
                        case "m_LocalPosition.z":
                            IEnumerable<BabylonAnimationKey> pz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            positionZ.AddRange(pz_keys);
                            break;

                        // Rotation    
                        case "localEulerAnglesRaw.x":
                            IEnumerable<BabylonAnimationKey> rx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationX.AddRange(rx_keys);
                            break;
                        case "localEulerAnglesRaw.y":
                            IEnumerable<BabylonAnimationKey> ry_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationY.AddRange(ry_keys);
                            break;
                        case "localEulerAnglesRaw.z":
                            IEnumerable<BabylonAnimationKey> rz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationZ.AddRange(rz_keys);
                            break;
                        case "localEulerAnglesRaw.w":
                            IEnumerable<BabylonAnimationKey> rw_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationW.AddRange(rw_keys);
                            break;

                        // Scaling
                        case "m_LocalScale.x":
                            IEnumerable<BabylonAnimationKey> sx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scaleX.AddRange(sx_keys);
                            break;
                        case "m_LocalScale.y":
                            IEnumerable<BabylonAnimationKey> sy_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scaleY.AddRange(sy_keys);
                            break;
                        case "m_LocalScale.z":
                            IEnumerable<BabylonAnimationKey> sz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scaleZ.AddRange(sz_keys);
                            break;
                        default:
                            continue;
                    }
                }
                frameOffest += clipFrameCount;
                totalFrameCount += clipFrameCount;
            }

            // Position properties
            string property = "none";
            if (positionX.Count > 0)
            {
                property = "position.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = positionX.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (positionY.Count > 0)
            {
                property = "position.y";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = positionY.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (positionZ.Count > 0)
            {
                property = "position.z";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = positionZ.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }

            // Rotation properties
            property = "none";
            if (rotationX.Count > 0)
            {
                property = "rotation.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = rotationX.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (rotationY.Count > 0)
            {
                property = "rotation.y";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = rotationY.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (rotationZ.Count > 0)
            {
                property = "rotation.z";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = rotationZ.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (rotationW.Count > 0)
            {
                property = "rotation.w";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = rotationW.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }

            // Scale properties
            property = "none";
            if (scaleX.Count > 0)
            {
                property = "scaling.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = scaleX.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (scaleY.Count > 0)
            {
                property = "scaling.y";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = scaleY.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            property = "none";
            if (scaleZ.Count > 0)
            {
                property = "scaling.z";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = scaleZ.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    property = property
                });
            }
            if (animations.Count > 0)
            {
                animatable.animations = animations.ToArray();
            }
        }

        private static void ExportSkeletonAnimationClips(Animator animator, BabylonSkeleton skeleton, SkinnedMeshRenderer skinnedMesh, BabylonMesh babylonMesh, UnityEditor.AnimationState animationState, ref UnityMetaData metaData)
        {
            List<AnimationClip> states = Tools.GetAnimationClips(animator);
            if (states != null && states.Count > 0)
            {
                ExportSkeletonAnimationClipData(animator.gameObject, skeleton, skinnedMesh, babylonMesh, animationState, ref states, ref metaData, animator);
            }
        }

        private static void ExportSkeletonAnimationClipData(GameObject source, BabylonSkeleton skeleton, SkinnedMeshRenderer skinnedMesh, BabylonMesh babylonMesh, UnityEditor.AnimationState animationState, ref List<AnimationClip> states, ref UnityMetaData metaData, Animator animator)
        {
            ExporterWindow.ReportProgress(1, "Exporting skeleton clips: " + skinnedMesh.name);
            //string sourceId = GetID(source);
            int frameRate = 0;
            int firstClipEnd = 0;
            int totalFrameCount = 0;
            Transform[] bones = skinnedMesh.bones;
            List<string> stateNameCache = new List<string>();
            if (!AnimationMode.InAnimationMode()) {
                AnimationMode.StartAnimationMode();
            }
            //var anims = new List<BabylonAnimation>();
            //var pxkeys = new List<BabylonAnimationKey>();
            float playbackSpeed = (animationState != null) ? animationState.playbackSpeed : 1.0f;
            float clampFeetPositions = (animationState != null) ? animationState.clampFeetPositions : 0.0f;
            BabylonAnimationBaking bakeRootTransforms = (animationState != null) ? animationState.bakeRootTransforms : BabylonAnimationBaking.GameBlend;
            foreach (var bone in skeleton.bones)
            {
                int frameOffest = 0;
                var keys = new List<BabylonAnimationKey>();
                Transform transform = bones.Single(b => b.name == bone.name);
                foreach (var state in states)
                {
                    if (state == null) continue;
                    AnimationClip clip = state as AnimationClip;
                    if (frameRate <= 0) frameRate = (int)clip.frameRate;
                    var frameTime = 1.0f / frameRate;
                    int clipFrameCount = (int)(clip.length * frameRate);
                    if (firstClipEnd <= 0) firstClipEnd = (clipFrameCount - 1);
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    BabylonLoopBehavior behavior = (settings.loopTime) ? BabylonLoopBehavior.Cycle : BabylonLoopBehavior.Constant;
                    if (settings.loopTime && settings.loopBlend) behavior = BabylonLoopBehavior.Relative;
                    ExporterWindow.ReportProgress(1, "Sampling: " + babylonMesh.name + " - "  + bone.name + " - " + clip.name);
                    // Set Animation State Meta Data
                    if (!stateNameCache.Contains(clip.name)) {
                        stateNameCache.Add(clip.name);
                        // Animation Clip Information
                        Dictionary<string, object> animStateInfo = new Dictionary<string, object>();
                        animStateInfo.Add("type", "skeleton");
                        animStateInfo.Add("name", clip.name);
                        animStateInfo.Add("start", frameOffest);
                        animStateInfo.Add("stop", (frameOffest + clipFrameCount - 1));
                        animStateInfo.Add("rate", frameRate);
                        animStateInfo.Add("behavior", (int)behavior);
                        animStateInfo.Add("playback", playbackSpeed);
                        metaData.animationClips.Add(animStateInfo);
                    }
                    AnimationMode.BeginSampling();
                    for (var i = 0; i < clipFrameCount; i++)
                    {
                        Matrix4x4 local;
                        int frameIndex = (i + frameOffest);
                        float originalPX = transform.localPosition.x;
                        float originalPY = transform.localPosition.y;
                        float originalPZ = transform.localPosition.z;
                        float originalRY = transform.localRotation.eulerAngles.y;
                        clip.SampleAnimation(source, i * frameTime);
                        if (transform == skinnedMesh.rootBone) {
                            float positionX = transform.localPosition.x;
                            float positionY = transform.localPosition.y;
                            float positionZ = transform.localPosition.z;
                            Quaternion rotationQT = transform.localRotation;
                            if (settings.loopBlendOrientation || settings.keepOriginalOrientation) {
                                if (settings.keepOriginalOrientation) {
                                    // Original Rotation - ???
                                    rotationQT = Quaternion.Euler(rotationQT.eulerAngles.x, originalRY, rotationQT.eulerAngles.z);
                                } else {
                                    // Body Orientation - ???
                                    rotationQT = Quaternion.Euler(rotationQT.eulerAngles.x, settings.orientationOffsetY, rotationQT.eulerAngles.z);
                                }
                            }
                            if (settings.loopBlendPositionY || settings.keepOriginalPositionY) {
                                if (settings.keepOriginalPositionY) {
                                    // Original Position Y
                                    positionY = originalPY;
                                } else  if (settings.heightFromFeet) {
                                    // Feet Position Y
                                    positionY = (settings.level + clampFeetPositions);
                                } else {
                                    // Center Of Mass
                                    positionY = 0.0f;
                                }
                            }
                            if (settings.loopBlendPositionXZ || settings.keepOriginalPositionXZ) {
                                if (settings.keepOriginalPositionXZ) {
                                    // Original Position XZ
                                    positionX = originalPX;
                                    positionZ = originalPZ;
                                } else {
                                    // Center Of Mass
                                    positionX = 0.0f;
                                    positionZ = 0.0f;
                                }
                            }
                            if (bakeRootTransforms == BabylonAnimationBaking.GameBlend) {
                                positionX = 0.0f;
                                positionZ = 0.0f;
                            }
                            local = Matrix4x4.TRS(new Vector3(positionX, positionY, positionZ), rotationQT, transform.localScale);
                        } else {
                            // DEPRECIATED: local = (transform.parent.localToWorldMatrix.inverse * transform.localToWorldMatrix);
                            local = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
                        }
                        float[] matrix = new[] {
                            local[0, 0], local[1, 0], local[2, 0], local[3, 0],
                            local[0, 1], local[1, 1], local[2, 1], local[3, 1],
                            local[0, 2], local[1, 2], local[2, 2], local[3, 2],
                            local[0, 3], local[1, 3], local[2, 3], local[3, 3]
                        };
                        var key = new BabylonAnimationKey
                        {
                            frame = frameIndex,
                            values = matrix
                        };
                        keys.Add(key);
                    }
                    AnimationMode.EndSampling();
                    frameOffest += clipFrameCount;
                    totalFrameCount += clipFrameCount;
                }
                var babylonAnimation = new BabylonAnimation
                {
                    name = bone.name + "Animation",
                    property = "_matrix",
                    dataType = (int)BabylonAnimation.DataType.Matrix,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Relative,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    framePerSecond = frameRate,
                    keys = keys.ToArray()
                };
                bone.animation = babylonAnimation;
            }
            if (AnimationMode.InAnimationMode()) {
                AnimationMode.StopAnimationMode();
            }
            /*
            //
            // TODO: Format Custom Curve Keys
            //
            string property = "none";
            if (pxkeys.Count > 0)
            {
                property = "metadata.state.animPosition.x";
                anims.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = property + " animation",
                    keys = pxkeys.ToArray(),
                    framePerSecond = frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            //
            // Cache Babylon Animation Keys
            //
            if (anims.Count > 0)
            {
                List<BabylonAnimation> sourceAnimiamtions = null;
                if (SceneBuilder.AnimationCurveKeys.ContainsKey(sourceId)) {
                    sourceAnimiamtions = SceneBuilder.AnimationCurveKeys[sourceId];
                } else {
                    sourceAnimiamtions = new List<BabylonAnimation>();
                    SceneBuilder.AnimationCurveKeys.Add(sourceId, sourceAnimiamtions);
                }
                foreach (var anim in anims) {
                    sourceAnimiamtions.Add(anim);
                }
            }
            */
        }

        private static bool IsRotationQuaternionAnimated(BabylonIAnimatable animatable)
        {
            if (animatable.animations == null)
            {
                return false;
            }

            return animatable.animations.Any(animation => animation.property.Contains("rotationQuaternion"));
        }
    }
}
