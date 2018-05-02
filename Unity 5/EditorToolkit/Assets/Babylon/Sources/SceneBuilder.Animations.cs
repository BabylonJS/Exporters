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
                if (astate != null && astate.isActiveAndEnabled == true && astate.controlType == BabylonAnimationMode.TransformAnimation) {
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
            ExporterWindow.ReportProgress(1, "Baking " + transform.gameObject.name.ToLower() + " transform... This may take a while.");
            string sourceId = GetID(source);
            int frameRate = 0;
            var anims = new List<BabylonAnimation>();
            List<BabylonRange> ranges = new List<BabylonRange>();
            List<string> stateNameCache = new List<string>();
            List<BabylonAnimation> animations = new List<BabylonAnimation>();

            var positionX = new List<BabylonAnimationKey>();
            var positionY = new List<BabylonAnimationKey>();
            var positionZ = new List<BabylonAnimationKey>();

            var rotationX = new List<BabylonAnimationKey>();
            var rotationY = new List<BabylonAnimationKey>();
            var rotationZ = new List<BabylonAnimationKey>();

            var quaternionX = new List<BabylonAnimationKey>();
            var quaternionY = new List<BabylonAnimationKey>();
            var quaternionZ = new List<BabylonAnimationKey>();
            var quaternionW = new List<BabylonAnimationKey>();

            var scalingX = new List<BabylonAnimationKey>();
            var scalingY = new List<BabylonAnimationKey>();
            var scalingZ = new List<BabylonAnimationKey>();

            int frameOffest = 0;
            foreach (var state in states)
            {
                if (state == null) continue;
                AnimationClip clip = state as AnimationClip;
                string clipName = FormatSafeClipName(clip.name);
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                BabylonLoopBehavior behavior = (settings.loopTime) ? BabylonLoopBehavior.Relative : BabylonLoopBehavior.Constant;
                if (settings.loopTime && settings.loopBlend) behavior = BabylonLoopBehavior.Cycle;
                // ..
                // Sample Animation Frame
                // ..
                if (frameRate <= 0) frameRate = (int)clip.frameRate;
                int clipFrameCount = (int)(clip.length * clip.frameRate);
                int lastFrameCount = clipFrameCount - 1;
                // ..
                // Animation Curve Bindings
                // ..
                var curveBindings = AnimationUtility.GetCurveBindings(clip);
                foreach (var binding in curveBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    switch (binding.propertyName)
                    {
                        //Positions
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

                        // Rotations    
                        case "localEuler.x":
                        case "localEulerAnglesRaw.x":
                        case "localEulerAnglesBaked.x":
                            IEnumerable<BabylonAnimationKey> rx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationX.AddRange(rx_keys);
                            break;
                        case "localEuler.y":
                        case "localEulerAnglesRaw.y":
                        case "localEulerAnglesBaked.y":
                            IEnumerable<BabylonAnimationKey> ry_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationY.AddRange(ry_keys);
                            break;
                        case "localEuler.z":
                        case "localEulerAnglesRaw.z":
                        case "localEulerAnglesBaked.z":
                            IEnumerable<BabylonAnimationKey> rz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value * (float)Math.PI / 180 }
                            });
                            rotationZ.AddRange(rz_keys);
                            break;

                        // Quaternions
                        case "localRotation.x":
                        case "m_LocalRotation.x":
                            IEnumerable<BabylonAnimationKey> qx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            quaternionX.AddRange(qx_keys);
                            break;
                        case "localRotation.y":
                        case "m_LocalRotation.y":
                            IEnumerable<BabylonAnimationKey> qy_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            quaternionY.AddRange(qy_keys);
                            break;
                        case "localRotation.z":
                        case "m_LocalRotation.z":
                            IEnumerable<BabylonAnimationKey> qz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            quaternionZ.AddRange(qz_keys);
                            break;
                        case "localRotation.w":
                        case "m_LocalRotation.w":
                            IEnumerable<BabylonAnimationKey> qw_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            quaternionW.AddRange(qw_keys);
                            break;

                        // Scaling
                        case "m_LocalScale.x":
                            IEnumerable<BabylonAnimationKey> sx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scalingX.AddRange(sx_keys);
                            break;
                        case "m_LocalScale.y":
                            IEnumerable<BabylonAnimationKey> sy_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scalingY.AddRange(sy_keys);
                            break;
                        case "m_LocalScale.z":
                            IEnumerable<BabylonAnimationKey> sz_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                frame = (int)((keyFrame.time * frameRate) + frameOffest),
                                values = new[] { keyFrame.value }
                            });
                            scalingZ.AddRange(sz_keys);
                            break;
                        default:
                            continue;
                    }
                }
                // ..
                // Set Animation State Meta Data
                // ..
                if (!stateNameCache.Contains(clipName)) {
                    stateNameCache.Add(clipName);
                    // Animation Clip Information
                    int fromFrame = frameOffest, toFrame = frameOffest + lastFrameCount;
                    Dictionary<string, object> animStateInfo = new Dictionary<string, object>();
                    animStateInfo.Add("type", "transform");
                    animStateInfo.Add("name", clipName);
                    animStateInfo.Add("start", fromFrame);
                    animStateInfo.Add("stop", toFrame);
                    animStateInfo.Add("rate", clip.frameRate);
                    animStateInfo.Add("frames", clipFrameCount);
                    animStateInfo.Add("weight", 1.0f);
                    animStateInfo.Add("behavior", (int)behavior);
                    animStateInfo.Add("apparentSpeed", clip.apparentSpeed);
                    animStateInfo.Add("averageSpeed", clip.averageSpeed.ToFloat());
                    animStateInfo.Add("averageDuration", clip.averageDuration);
                    animStateInfo.Add("averageAngularSpeed", clip.averageAngularSpeed);
                    List<string> customCurveKeyNames = new List<string>();
                    var aparams = Tools.GetAnimationParameters(animator);
                    if (aparams != null && aparams.Count > 0) {
                        foreach (var aparam in aparams) {
                            if (aparam.curve == true) {
                                var curve = Tools.GetAnimationCurve(clip, aparam.name);
                                if (curve != null) {
                                    IEnumerable<BabylonAnimationKey> cx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                        frame = (int)(keyFrame.time * frameRate),
                                        values = new[] { keyFrame.value }
                                    });
                                    BabylonAnimationKey[] xkeys = (cx_keys != null && cx_keys.Count() > 0) ? cx_keys.ToArray() : null;
                                    if (xkeys != null && xkeys.Length > 0) {
                                        string xkey = aparam.name;
                                        string xprop = "metadata.state.floats." + xkey;
                                        string xname = "custom:" + clipName.Replace(" ", "") + ":" + System.Guid.NewGuid().ToString();
                                        customCurveKeyNames.Add(xname);
                                        anims.Add(new BabylonAnimation
                                        {
                                            dataType = (int)BabylonAnimation.DataType.Float,
                                            name = xname,
                                            keys = xkeys,
                                            framePerSecond = frameRate,
                                            enableBlending = false,
                                            blendingSpeed = 0.0f,
                                            loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                                            property = xprop
                                        });
                                    }
                                }
                            }
                        }
                    }
                    animStateInfo.Add("customCurveKeyNames", (customCurveKeyNames.Count > 0) ? customCurveKeyNames.ToArray() : null);
                    metaData.animationClips.Add(animStateInfo);
                    ranges.Add(new BabylonRange{ name = clipName, from = fromFrame, to = toFrame });
                }
                // ..
                frameOffest += clipFrameCount;
            }

            // Position properties
            string prefix = "transform:";
            string suffix = ":animation";
            string property = "none";
            if (positionX.Count > 0)
            {
                property = "position.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = positionX.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
                    name = prefix + property.ToLower() + suffix,
                    keys = positionY.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
                    name = prefix + property.ToLower() + suffix,
                    keys = positionZ.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
                    name = prefix + property.ToLower() + suffix,
                    keys = rotationX.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
                    name = prefix + property.ToLower() + suffix,
                    keys = rotationY.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
                    name = prefix + property.ToLower() + suffix,
                    keys = rotationZ.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }

            // Quaternion properties
            property = "none";
            if (quaternionX.Count > 0)
            {
                property = "rotationQuaternion.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = quaternionX.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            property = "none";
            if (quaternionY.Count > 0)
            {
                property = "rotationQuaternion.y";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = quaternionY.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            property = "none";
            if (quaternionZ.Count > 0)
            {
                property = "rotationQuaternion.z";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = quaternionZ.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            property = "none";
            if (quaternionW.Count > 0)
            {
                property = "rotationQuaternion.w";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = quaternionW.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }

            // Scaling properties
            property = "none";
            if (scalingX.Count > 0)
            {
                property = "scaling.x";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = scalingX.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            property = "none";
            if (scalingY.Count > 0)
            {
                property = "scaling.y";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = scalingY.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            property = "none";
            if (scalingZ.Count > 0)
            {
                property = "scaling.z";
                animations.Add(new BabylonAnimation
                {
                    dataType = (int)BabylonAnimation.DataType.Float,
                    name = prefix + property.ToLower() + suffix,
                    keys = scalingZ.ToArray(),
                    framePerSecond = (int)frameRate,
                    enableBlending = false,
                    blendingSpeed = 0.0f,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                });
            }
            // Serialize animations
            if (animations.Count > 0)
            {
                animatable.animations = animations.ToArray();
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
            ExporterWindow.ReportProgress(1, "Baking " + skinnedMesh.name.ToLower() + " skeleton... This may take a while.");
            string sourceId = GetID(source);
            int frameRate = 0;
            Transform[] bones = skinnedMesh.bones;
            var anims = new List<BabylonAnimation>();
            List<BabylonRange> ranges = new List<BabylonRange>();
            List<string> stateNameCache = new List<string>();
            if (!AnimationMode.InAnimationMode()) {
                AnimationMode.StartAnimationMode();
            }
            foreach (var bone in skeleton.bones)
            {
                int frameOffest = 0;
                var keys = new List<BabylonAnimationKey>();
                Transform transform = bones.Single(b => b.name == bone.name);
                foreach (var state in states)
                {
                    if (state == null) continue;
                    AnimationClip clip = state as AnimationClip;
                    string clipName = FormatSafeClipName(clip.name);
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    BabylonLoopBehavior behavior = (settings.loopTime) ? BabylonLoopBehavior.Relative : BabylonLoopBehavior.Constant;
                    if (settings.loopTime && settings.loopBlend) behavior = BabylonLoopBehavior.Cycle;
                    // ..
                    int framePadding = 1;
                    float deltaTime = 1.0f / clip.frameRate;
                    if (frameRate <= 0) frameRate = (int)clip.frameRate;
                    float clipFrameTotal = clip.length * clip.frameRate;
                    int clipFrameCount = (int)clipFrameTotal + framePadding;
                    int lastFrameCount = clipFrameCount - 1;
                    // ..
                    AnimationMode.BeginSampling();
                    for (int i = 0; i < clipFrameCount; i++)
                    {
                        Matrix4x4 local;
                        int frameIndex = (int)(i + frameOffest);
                        float sampleTime = (i < lastFrameCount) ? (i * deltaTime) : clip.length;
                        clip.SampleAnimation(source, sampleTime);
                        if (transform == skinnedMesh.rootBone) {
                            float positionX = transform.localPosition.x;
                            float positionY = transform.localPosition.y;
                            float positionZ = transform.localPosition.z;
                            Quaternion rotationQT = transform.localRotation;
                            if (settings.loopBlendOrientation) {
                                if (settings.keepOriginalOrientation) {
                                    rotationQT = Quaternion.Euler(rotationQT.eulerAngles.x, (rotationQT.eulerAngles.y + settings.orientationOffsetY), rotationQT.eulerAngles.z);
                                } else {
                                    rotationQT = Quaternion.Euler(rotationQT.eulerAngles.x, settings.orientationOffsetY, rotationQT.eulerAngles.z);
                                }
                            }
                            if (settings.loopBlendPositionY) {
                                if (settings.keepOriginalPositionY || settings.heightFromFeet) {
                                    positionY += settings.level;
                                } else {
                                    positionY = settings.level;
                                }
                            }
                            if (settings.loopBlendPositionXZ) {
                                positionX = 0.0f;
                                positionZ = 0.0f;
                            }
                            local = Matrix4x4.TRS(new Vector3(positionX, positionY, positionZ), rotationQT, transform.localScale);
                        } else {
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
                    // ..
                    // Set Animation State Meta Data
                    // ..
                    if (!stateNameCache.Contains(clipName)) {
                        stateNameCache.Add(clipName);
                        // Animation Clip Information
                        int fromFrame = frameOffest, toFrame = frameOffest + lastFrameCount;
                        Dictionary<string, object> animStateInfo = new Dictionary<string, object>();
                        animStateInfo.Add("type", "skeleton");
                        animStateInfo.Add("name", clipName);
                        animStateInfo.Add("start", fromFrame);
                        animStateInfo.Add("stop", toFrame);
                        animStateInfo.Add("rate", clip.frameRate);
                        animStateInfo.Add("frames", clipFrameCount);
                        animStateInfo.Add("weight", 1.0f);
                        animStateInfo.Add("behavior", (int)behavior);
                        animStateInfo.Add("apparentSpeed", clip.apparentSpeed);
                        animStateInfo.Add("averageSpeed", clip.averageSpeed.ToFloat());
                        animStateInfo.Add("averageDuration", clip.averageDuration);
                        animStateInfo.Add("averageAngularSpeed", clip.averageAngularSpeed);
                        List<string> customCurveKeyNames = new List<string>();
                        var aparams = Tools.GetAnimationParameters(animator);
                        if (aparams != null && aparams.Count > 0) {
                            foreach (var aparam in aparams) {
                                if (aparam.curve == true) {
                                    var curve = Tools.GetAnimationCurve(clip, aparam.name);
                                    if (curve != null) {
                                        IEnumerable<BabylonAnimationKey> cx_keys = curve.keys.Select(keyFrame => new BabylonAnimationKey {
                                            frame = (int)(keyFrame.time * frameRate),
                                            values = new[] { keyFrame.value }
                                        });
                                        BabylonAnimationKey[] xkeys = (cx_keys != null && cx_keys.Count() > 0) ? cx_keys.ToArray() : null;
                                        if (xkeys != null && xkeys.Length > 0) {
                                            string xkey = aparam.name;
                                            string xprop = "metadata.state.floats." + xkey;
                                            string xname = "curve:" + clipName.Replace(" ", "") + ":" + System.Guid.NewGuid().ToString();
                                            customCurveKeyNames.Add(xname);
                                            anims.Add(new BabylonAnimation
                                            {
                                                dataType = (int)BabylonAnimation.DataType.Float,
                                                name = xname,
                                                keys = xkeys,
                                                framePerSecond = frameRate,
                                                enableBlending = false,
                                                blendingSpeed = 0.0f,
                                                loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                                                property = xprop
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        animStateInfo.Add("customCurveKeyNames", (customCurveKeyNames.Count > 0) ? customCurveKeyNames.ToArray() : null);
                        metaData.animationClips.Add(animStateInfo);
                        ranges.Add(new BabylonRange{ name = clipName, from = fromFrame, to = toFrame });
                    }
                    // ..
                    frameOffest += clipFrameCount;
                }
                var babylonAnimation = new BabylonAnimation
                {
                    name = "skeleton:" + bone.name.ToLower() + ":animation",
                    property = "_matrix",
                    dataType = (int)BabylonAnimation.DataType.Matrix,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
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
            //
            // Serialize Skeleton Clip Ranges
            //
            skeleton.ranges = (ranges.Count > 0) ? ranges.ToArray() : null;
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
        }

        private static string FormatSafeClipName(string name)
        {
            string result = name;
            int marker = result.IndexOf("@");
            if (marker >= 0) result = result.Substring(marker + 1);
            return result;
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
