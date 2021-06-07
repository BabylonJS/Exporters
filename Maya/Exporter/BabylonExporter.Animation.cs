using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Maya2Babylon
{
    class AnimCurvData
    {
        public string animCurv;
        public Dictionary<int, float> valuePerFrame = new Dictionary<int, float>();
    }

    enum AnimationInterpolationMode { Linear };

    internal partial class BabylonExporter
    {
        private float _MaxRotationalKeyframeDifferenceDegrees = 179f;

        /// <summary>
        /// Export TRS animations of the transform
        /// </summary>
        /// <param name="babylonNode"></param>
        /// <param name="mFnTransform">Transform above mesh/camera/light</param>
        private void ExportNodeAnimation(BabylonNode babylonNode, MFnTransform mFnTransform)
        {
            _exporNodeAnimation(babylonNode, mFnTransform, GetAnimation);
        }

        /// <summary>
        /// Export TRS animations of the transform
        /// 
        /// </summary>
        /// <param name="babylonNode"></param>
        /// <param name="mFnTransform">Transform above mesh/camera/light</param>
        private void ExportNodeAnimationFrameByFrame(BabylonNode babylonNode, MFnTransform mFnTransform)
        {
            _exporNodeAnimation(babylonNode, mFnTransform, GetAnimationsFrameByFrame);
        }

        private void _exporNodeAnimation(BabylonNode babylonNode, MFnTransform mFnTransform, Func<MFnTransform, List<BabylonAnimation>>  getAnimationsFunc)
        {
            try
            {
                var animations = getAnimationsFunc(mFnTransform).ToArray();
                babylonNode.animations = animations != null && animations.Length != 0 ? animations : null;
                if (babylonNode.animations != null)
                {
                    // TODO - Retreive from Maya
                    babylonNode.autoAnimate = true;
                    babylonNode.autoAnimateFrom = Loader.GetMinTime();
                    babylonNode.autoAnimateTo = Loader.GetMaxTime();
                    babylonNode.autoAnimateLoop = true;
                }
            }
            catch (Exception e)
            {
                RaiseError("Error while exporting animation: " + e.Message, 2);
            }
        }

        private List<BabylonAnimation> GetAnimationsFrameByFrame(MFnTransform mFnTransform)
        {
            int start = Loader.GetMinTime();
            int end = Loader.GetMaxTime();

            // Animations
            List<BabylonAnimation> animations = new List<BabylonAnimation>();

            string[] babylonAnimationProperties = new string[] { "scaling", "rotationQuaternion", "position", "visibility" };


            Dictionary<string, List<BabylonAnimationKey>> keysPerProperty = new Dictionary<string, List<BabylonAnimationKey>>();
            keysPerProperty.Add("scaling", new List<BabylonAnimationKey>());
            keysPerProperty.Add("rotationQuaternion", new List<BabylonAnimationKey>());
            keysPerProperty.Add("position", new List<BabylonAnimationKey>());
            keysPerProperty.Add("visibility", new List<BabylonAnimationKey>());

            // get keys
            for (int currentFrame = start; currentFrame <= end; currentFrame++)
            {
                // get transformation matrix at this frame
                MDoubleArray mDoubleMatrix = new MDoubleArray();
                MGlobal.executeCommand($"getAttr -t {currentFrame} {mFnTransform.fullPathName}.matrix", mDoubleMatrix);
                mDoubleMatrix.get(out float[] localMatrix);
                MMatrix matrix = new MMatrix(localMatrix);
                var transformationMatrix = new MTransformationMatrix(matrix);

                // Retreive TRS vectors from matrix
                var position = transformationMatrix.getTranslation();
                var rotationQuaternion = transformationMatrix.getRotationQuaternion();
                var scaling = transformationMatrix.getScale();

                // Switch coordinate system at object level
                position[2] *= -1;
                rotationQuaternion[0] *= -1;
                rotationQuaternion[1] *= -1;

                // Apply unit conversion factor to meter
                position[0] *= scaleFactorToMeters;
                position[1] *= scaleFactorToMeters;
                position[2] *= scaleFactorToMeters;

                // create animation key for each property
                for (int indexAnimation = 0; indexAnimation < babylonAnimationProperties.Length; indexAnimation++)
                {
                    string babylonAnimationProperty = babylonAnimationProperties[indexAnimation];

                    BabylonAnimationKey key = new BabylonAnimationKey();
                    key.frame = currentFrame;
                    switch (indexAnimation)
                    {
                        case 0: // scaling
                            key.values = scaling.ToArray();
                            break;
                        case 1: // rotationQuaternion
                            key.values = rotationQuaternion.ToArray();
                            break;
                        case 2: // position
                            key.values = position.ToArray();
                            break;
                        case 3: // visibility
                            key.values = new float[] { Loader.GetVisibility(mFnTransform.fullPathName, currentFrame) };
                            break;
                    }

                    keysPerProperty[babylonAnimationProperty].Add(key);
                }
            }

            // create animation for each property
            for (int indexAnimation = 0; indexAnimation < babylonAnimationProperties.Length; indexAnimation++)
            {
                string babylonAnimationProperty = babylonAnimationProperties[indexAnimation];

                List<BabylonAnimationKey> keys = keysPerProperty[babylonAnimationProperty];

                var keysFull = new List<BabylonAnimationKey>(keys);

                // Optimization
                if (exportParameters.optimizeAnimations)
                {
                    OptimizeAnimations(keys, true);
                }

                // Ensure animation has at least 2 frames
                if (IsAnimationKeysRelevant(keys, babylonAnimationProperty))
                {
                    int dataType = 0;   // "scaling", "rotationQuaternion", "position", "visibility"
                    if (indexAnimation == 0 || indexAnimation == 2) // scaling and position
                    {
                        dataType = (int)BabylonAnimation.DataType.Vector3;
                    }
                    else if(indexAnimation == 1)    // rotationQuaternion
                    {
                        dataType = (int)BabylonAnimation.DataType.Quaternion;
                    }
                    else   // visibility
                    {
                        dataType = (int)BabylonAnimation.DataType.Float;
                    }
                    // Create BabylonAnimation
                    animations.Add(new BabylonAnimation()
                    {
                        dataType = dataType,
                        name = babylonAnimationProperty + " animation",
                        framePerSecond = Loader.GetFPS(),
                        loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                        property = babylonAnimationProperty,
                        keys = keys.ToArray(),
                        keysFull = keysFull
                    });
                }
            }
            return animations;
        }

        /// <summary>
        /// Get TRS and visiblity animations of the transform
        /// </summary>
        /// <param name="transform">Transform above mesh/camera/light</param>
        /// <returns></returns>
        private List<BabylonAnimation> GetAnimation(MFnTransform transform)
        {
            // Animations
            MPlugArray connections = new MPlugArray();
            MStringArray animCurvList = new MStringArray();
            MIntArray keysTime = new MIntArray();
            MDoubleArray keysValue = new MDoubleArray();

            MFloatArray translateValues = new MFloatArray();
            MFloatArray rotateValues = new MFloatArray();
            MFloatArray scaleValues = new MFloatArray();
            MFloatArray visibilityValues = new MFloatArray();
            MFloatArray keyTimes = new MFloatArray();

            List<BabylonAnimation> animationsObject = new List<BabylonAnimation>();

            //Get the animCurve
            MGlobal.executeCommand("listConnections -type \"animCurve\" " + transform.fullPathName + ";", animCurvList);

            List<AnimCurvData> animCurvesData = new List<AnimCurvData>();
            foreach (String animCurv in animCurvList)
            {
                AnimCurvData animCurvData = new AnimCurvData();
                animCurvesData.Add(animCurvData);

                animCurvData.animCurv = animCurv;

                //Get the key time for each curves
                MGlobal.executeCommand("keyframe -q " + animCurv + ";", keysTime);

                //Get the value for each curves
                MGlobal.executeCommand("keyframe - q -vc -absolute " + animCurv + ";", keysValue);

                // Switch coordinate system at object level
                if (animCurv.EndsWith("translateZ") || animCurv.EndsWith("rotateX") || animCurv.EndsWith("rotateY"))
                {
                    for (int index = 0; index < keysValue.Count; index++)
                    {
                        keysValue[index] *= -1.0f;
                    }
                }

                // Apply unit conversion factor to meter
                if (animCurv.Contains("translate"))
                {
                    for (int index = 0; index < keysValue.Count; index++)
                    {
                        keysValue[index] *= scaleFactorToMeters;
                    }
                }

                // Store data
                for (int index = 0; index < keysTime.Count; index++)
                {
                    int key = keysTime[index];
                    if (animCurvData.valuePerFrame.ContainsKey(key) == false)
                    {
                        animCurvData.valuePerFrame.Add(key, (float)keysValue[index]);
                    }
                }
            }

            string[] mayaAnimationProperties = new string[] { "translate", "rotate", "scale" };
            string[] babylonAnimationProperties = new string[] { "position", "rotationQuaternion", "scaling" };
            string[] axis = new string[] { "X", "Y", "Z" };

            // Init TRS default values
            Dictionary<string, float> defaultValues = new Dictionary<string, float>();
            float[] position = null;
            float[] rotationQuaternion = null;
            BabylonVector3.EulerRotationOrder rotationOrder = BabylonVector3.EulerRotationOrder.XYZ;
            float[] rotation = null;
            float[] scaling = null;
            GetTransform(transform, ref position, ref rotationQuaternion, ref rotation, ref rotationOrder, ref scaling); // coordinate system already switched
            defaultValues.Add("translateX", position[0]);
            defaultValues.Add("translateY", position[1]);
            defaultValues.Add("translateZ", position[2]);
            defaultValues.Add("rotateX", rotation[0]);
            defaultValues.Add("rotateY", rotation[1]);
            defaultValues.Add("rotateZ", rotation[2]);
            defaultValues.Add("scaleX", scaling[0]);
            defaultValues.Add("scaleY", scaling[1]);
            defaultValues.Add("scaleZ", scaling[2]);

            for (int indexAnimationProperty = 0; indexAnimationProperty < mayaAnimationProperties.Length; indexAnimationProperty++)
            {
                string mayaAnimationProperty = mayaAnimationProperties[indexAnimationProperty];

                // Retreive animation curves data for current animation property
                // Ex: all "translate" data are "translateX", "translateY", "translateZ"
                List<AnimCurvData> animDataProperty = animCurvesData.Where(data => data.animCurv.Contains(mayaAnimationProperty)).ToList();

                if (animDataProperty.Count == 0)
                {
                    // Property is not animated
                    continue;
                }

                // Get all frames for this property
                List<int> framesProperty = new List<int>();
                foreach (var animData in animDataProperty)
                {
                    framesProperty.AddRange(animData.valuePerFrame.Keys);
                }
                framesProperty = framesProperty.Distinct().ToList();
                framesProperty.Sort();

                // Get default values for this property
                BabylonAnimationKey lastBabylonAnimationKey = new BabylonAnimationKey();
                lastBabylonAnimationKey.frame = 0;
                lastBabylonAnimationKey.values = new float[] { defaultValues[mayaAnimationProperty + "X"], defaultValues[mayaAnimationProperty + "Y"], defaultValues[mayaAnimationProperty + "Z"] };

                // Compute all values for this property
                List<BabylonAnimationKey> babylonAnimationKeys = new List<BabylonAnimationKey>();
                foreach (var frameProperty in framesProperty)
                {
                    BabylonAnimationKey babylonAnimationKey = new BabylonAnimationKey();
                    babylonAnimationKeys.Add(babylonAnimationKey);

                    // Frame
                    babylonAnimationKey.frame = frameProperty;

                    // Values
                    float[] valuesProperty = new float[3];
                    for (int indexAxis = 0; indexAxis < axis.Length; indexAxis++)
                    {
                        AnimCurvData animCurvDataAxis = animDataProperty.Find(data => data.animCurv.EndsWith(axis[indexAxis]));

                        float value;
                        if (animCurvDataAxis != null && animCurvDataAxis.valuePerFrame.ContainsKey(frameProperty))
                        {
                            value = animCurvDataAxis.valuePerFrame[frameProperty];
                        }
                        else
                        {
                            value = lastBabylonAnimationKey.values[indexAxis];
                        }
                        valuesProperty[indexAxis] = value;
                    }
                    babylonAnimationKey.values = valuesProperty.ToArray();

                    // Update last known values
                    lastBabylonAnimationKey = babylonAnimationKey;
                }

                // Convert euler to quaternion angles
                if (indexAnimationProperty == 1) // Rotation
                {
                    BabylonAnimationKey nextBabylonAnimationKey = null;
                    BabylonVector3 nextBabylonAnimationEulerAngles = null;
                    int subdivisions = 0;

                    for (int keyframeIndex = 0; keyframeIndex < babylonAnimationKeys.Count; ++keyframeIndex)
                    {
                        nextBabylonAnimationEulerAngles = null;
                        var babylonAnimationKey = babylonAnimationKeys[keyframeIndex];
                        BabylonVector3 babylonAnimationEulerAngles = BabylonVector3.FromArray(babylonAnimationKey.values);
                        if (keyframeIndex < babylonAnimationKeys.Count - 1)
                        {
                            nextBabylonAnimationKey = babylonAnimationKeys[keyframeIndex + 1];
                            nextBabylonAnimationEulerAngles = BabylonVector3.FromArray(nextBabylonAnimationKey.values);
                        }

                        if (nextBabylonAnimationEulerAngles != null)
                        {
                            var rotationDiff = nextBabylonAnimationEulerAngles - babylonAnimationEulerAngles;
                            var frameDiff = (float)(nextBabylonAnimationKey.frame - babylonAnimationKey.frame);

                            // if any of our rotation axes have a keyframe diff large enough to lose information when converted to quaternion, break it down into a number of sub keyframes.
                            var largestRotationalComponentDiff = Math.Max(Math.Max(Math.Abs(rotationDiff.X), Math.Abs(rotationDiff.Y)), Math.Abs(rotationDiff.Z));
                            if (largestRotationalComponentDiff > _MaxRotationalKeyframeDifferenceDegrees && largestRotationalComponentDiff != 360.0f && !(subdivisions > 0))
                            {
                                subdivisions = Convert.ToInt32(Math.Ceiling(largestRotationalComponentDiff / _MaxRotationalKeyframeDifferenceDegrees));
                                this.RaiseWarning($"Animation Track \"{mayaAnimationProperty}\": Frames {babylonAnimationKey.frame} and {nextBabylonAnimationKey.frame} have a rotation difference of {largestRotationalComponentDiff} that is larger than {_MaxRotationalKeyframeDifferenceDegrees} degrees. Interpolating with {subdivisions} additional keyframes.", 2);
                                for (int subdivision = 1; subdivision <= subdivisions; ++subdivision)
                                {
                                    var newKeyframe = babylonAnimationKey.frame + (frameDiff * ((float)subdivision / (float)(subdivisions + 1)));
                                    var newKeyframeValues = InterpolateBetweenRotationalKeyframes(babylonAnimationKey, nextBabylonAnimationKey, newKeyframe, AnimationInterpolationMode.Linear);
                                    var subdividedAnimationKey = new BabylonAnimationKey() { frame = newKeyframe, values = newKeyframeValues };
                                    babylonAnimationKeys.Insert(keyframeIndex + subdivision, subdividedAnimationKey);
                                    this.RaiseWarning($"Frame inserted at {newKeyframe}", 3);
                                }
                                subdivisions += 1;
                            }
                            if (subdivisions > 0) subdivisions -= 1;
                        }

                        BabylonVector3 eulerAnglesRadians = babylonAnimationEulerAngles * (float)(Math.PI / 180);
                        BabylonQuaternion quaternionAngles = eulerAnglesRadians.toQuaternion(rotationOrder);
                        babylonAnimationKey.values = quaternionAngles.ToArray();
                    }
                }

                var keysFull = new List<BabylonAnimationKey>(babylonAnimationKeys);

                // Optimization
                OptimizeAnimations(babylonAnimationKeys, true);

                // Ensure animation has at least 2 frames
                string babylonAnimationProperty = babylonAnimationProperties[indexAnimationProperty];
                if (IsAnimationKeysRelevant(babylonAnimationKeys, babylonAnimationProperty))
                {
                    // Create BabylonAnimation
                    animationsObject.Add(new BabylonAnimation()
                    {
                        dataType = indexAnimationProperty == 1 ? (int)BabylonAnimation.DataType.Quaternion : (int)BabylonAnimation.DataType.Vector3,
                        name = babylonAnimationProperty + " animation",
                        framePerSecond = Loader.GetFPS(),
                        loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                        property = babylonAnimationProperty,
                        keys = babylonAnimationKeys.ToArray(),
                        keysFull = keysFull
                    });
                }
            }

            return animationsObject;
        }

        static float[] InterpolateBetweenRotationalKeyframes(BabylonAnimationKey from, BabylonAnimationKey to, float frame, AnimationInterpolationMode interpolationMode)
        {
            float frameDiffNormalized = (frame - from.frame)/(to.frame - from.frame);
            frameDiffNormalized = Math.Min(frameDiffNormalized, 1.0f);
            frameDiffNormalized = Math.Max(frameDiffNormalized, 0.0f);

            switch (interpolationMode)
            {
                case AnimationInterpolationMode.Linear:
                default:
                    return MathUtilities.LerpEulerAngle(from.values, to.values, frameDiffNormalized);
            }
        }

        static void OptimizeAnimations(List<BabylonAnimationKey> keys, bool removeLinearAnimationKeys)
        {
            for (int ixFirst = keys.Count - 3; ixFirst >= 0; --ixFirst)
            {
                while (keys.Count - ixFirst >= 3)
                {
                    if (!RemoveAnimationKey(keys, ixFirst, removeLinearAnimationKeys))
                    {
                        break;
                    }
                }
            }

        }

        static float[] weightedLerp(int frame0, int frame1, int frame2, float[] value0, float[] value2)
        {
            return weightedLerp(frame0, frame1, frame2, value0, value2);
        }

        static float[] weightedLerp(float frame0, float frame1, float frame2, float[] value0, float[] value2)
        {
            double weight2 = (frame1 - frame0) / (double)(frame2 - frame0);
            double weight0 = 1 - weight2;
            float[] result = new float[value0.Length];
            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = (float)(value0[i] * weight0 + value2[i] * weight2);
            }
            return result;
        }

        private static bool RemoveAnimationKey(List<BabylonAnimationKey> keys, int ixFirst, bool removeLinearAnimationKeys)
        {
            var first = keys[ixFirst];
            var middle = keys[ixFirst + 1];
            var last = keys[ixFirst + 2];

            // first pass, frame equality
            if (first.values.IsEqualTo(last.values) && first.values.IsEqualTo(middle.values))
            {
                keys.RemoveAt(ixFirst + 1);
                return true;
            }

            // second pass : linear interpolation detection
            if (removeLinearAnimationKeys)
            {
                var computedMiddleValue = weightedLerp(first.frame, middle.frame, last.frame, first.values, last.values);
                if (computedMiddleValue.IsEqualTo(middle.values))
                {
                    keys.RemoveAt(ixFirst + 1);
                    return true;
                }
            }
            return false;

        }

        private BabylonMatrix ConvertMayaToBabylonMatrix(MMatrix mMatrix)
        {
            var transformationMatrix = new MTransformationMatrix(mMatrix);

            // Retreive TRS vectors from matrix
            float[] position = transformationMatrix.getTranslation();
            float[] rotationQuaternion = transformationMatrix.getRotationQuaternion();
            float[] scaling = transformationMatrix.getScale();

            // Switch coordinate system at object level
            position[2] *= -1;
            rotationQuaternion[0] *= -1;
            rotationQuaternion[1] *= -1;

            // Apply unit conversion factor to meter
            position[0] *= scaleFactorToMeters;
            position[1] *= scaleFactorToMeters;
            position[2] *= scaleFactorToMeters;

            // The composed matrix
            return BabylonMatrix.Compose(new BabylonVector3(scaling[0], scaling[1], scaling[2]),   // scaling
                                                new BabylonQuaternion(rotationQuaternion[0], rotationQuaternion[1], rotationQuaternion[2], rotationQuaternion[3]), // rotation
                                                new BabylonVector3(position[0], position[1], position[2])   // position
                                            );
        }

        private BabylonAnimation GetAnimationsFrameByFrameMatrix(MFnTransform mFnTransform)
        {
            int start = Loader.GetMinTime();
            int end = Loader.GetMaxTime();
            BabylonAnimation animation = null;

            // get keys
            List<BabylonAnimationKey> keys = new List<BabylonAnimationKey>();
            for (int currentFrame = start; currentFrame <= end; currentFrame++)
            {
                // Set the animation key
                BabylonAnimationKey key = new BabylonAnimationKey() {
                    frame = currentFrame,
                    values = GetBabylonMatrix(mFnTransform, currentFrame).m.ToArray()
                };

                keys.Add(key);
            }

            var keysFull = new List<BabylonAnimationKey>(keys);

            // Optimization
            OptimizeAnimations(keys, false); // Do not remove linear animation keys for bones

            // Ensure animation has at least 2 frames
            if (IsAnimationKeysRelevant(keys, "_matrix", GetBabylonMatrix(mFnTransform, start).m.ToArray()))
            {
                // Animations
                animation = new BabylonAnimation()
                {
                    name = mFnTransform.name + "Animation", // override default animation name
                    dataType = (int)BabylonAnimation.DataType.Matrix,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    framePerSecond = Loader.GetFPS(),
                    keys = keys.ToArray(),
                    keysFull = keysFull,
                    property = "_matrix"
                };
            }

            return animation;
        }

        private MMatrix GetMMatrix(MFnTransform mFnTransform, double currentFrame = 0)
        {
            // get transformation matrix at this frame
            MDoubleArray mDoubleMatrix = new MDoubleArray();
            MGlobal.executeCommand($"getAttr -t {currentFrame} {mFnTransform.fullPathName}.matrix", mDoubleMatrix);
            mDoubleMatrix.get(out float[] localMatrix);

            return new MMatrix(localMatrix);
        }

        private BabylonMatrix GetBabylonMatrix(MFnTransform mFnTransform, double currentFrame = 0)
        {
            return ConvertMayaToBabylonMatrix(GetMMatrix(mFnTransform, currentFrame));
        }
        /// <summary>
        /// Determines if the animation key frames are relevant 
        /// </summary>
        /// <param name="keys">Animation key frames</param>
        /// <param name="property">The target property of the animation</param>
        /// <returns></returns>
        private bool IsAnimationKeysRelevant(List<BabylonAnimationKey> keys, string property, float[] expectedValues = null)
        {
            if (keys.Count > 1)
            {
                if (keys.Count == 2)
                {
                    if (keys[0].values.IsEqualTo(keys[1].values))
                    {
                        switch(property)
                        {
                            case "scaling":
                                expectedValues = expectedValues == null ? new BabylonVector3(1, 1, 1).ToArray() : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "rotationQuaternion":
                                expectedValues = expectedValues == null ? new BabylonQuaternion(0, 0, 0, 1).ToArray() : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "position":
                                expectedValues = expectedValues == null ? new BabylonVector3(0, 0, 0).ToArray() : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "_matrix":
                                expectedValues = expectedValues == null ? BabylonMatrix.Identity().m : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "uOffset":
                                expectedValues = expectedValues == null ? new float[1] { 0 } : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "vOffset":
                                expectedValues = expectedValues == null ? new float[1] { 0 } : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "uScale":
                                expectedValues = expectedValues == null ? new float[1] { 1 } : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "vScale":
                                expectedValues = expectedValues == null ? new float[1] { 1 } : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            case "wAng":
                                expectedValues = expectedValues == null ? new float[1] { 0 } : expectedValues;
                                if (keys[0].values.IsEqualTo(expectedValues))
                                {
                                    return false;
                                }
                                break;
                            default:
                                return true;

                        }
                    }
                }
                return true;
            }

            return false;
        }



        /// <summary>
        /// Find the keyframe of the blend shape and store the morph target and its weights in the map.
        /// </summary>
        /// <param name="blendShapeDeformerName"></param>
        /// <returns>A map with the morph target (Maya object) as key and its weights as value</returns>
        private IDictionary<double, IList<double>> GetMorphWeightsByFrame(string blendShapeDeformerName)
        {
            Dictionary<double, IList<double>> weights = new Dictionary<double, IList<double>>();

            IList<double> keys = GetKeyframes(blendShapeDeformerName);

            for (int index = 0; index < keys.Count; index++)
            {
                double key = keys[index];

                // Get the envelope
                MGlobal.executeCommand($"getAttr -t {key.ToString(System.Globalization.CultureInfo.InvariantCulture)} {blendShapeDeformerName}.envelope", out double envelope);

                // Get the weight at this keyframe
                MDoubleArray weightArray = new MDoubleArray();
                MGlobal.executeCommand($"getAttr -t {key.ToString(System.Globalization.CultureInfo.InvariantCulture)} {blendShapeDeformerName}.weight", weightArray);

                weights[key] = weightArray.Select(weight => envelope * weight).ToList();
            }

            return weights;
        }

        /// <summary>
        /// Export the morph target influence animation.
        /// </summary>
        /// <param name="blendShapeDeformerName"></param>
        /// <param name="weightIndex"></param>
        /// <returns>A list containing all animations</returns>
        private IList<BabylonAnimation> GetAnimationsInfluence(string blendShapeDeformerName, int weightIndex)
        {
            IList<BabylonAnimation> animations = new List<BabylonAnimation>();
            BabylonAnimation animation = null;

            IDictionary<double, IList<double>> morphWeights = GetMorphWeightsByFrame(blendShapeDeformerName);

            // get keys
            List<BabylonAnimationKey> keys = new List<BabylonAnimationKey>();
            for (int index = 0; index < morphWeights.Count; index++)
            {
                KeyValuePair<double, IList<double>> keyValue = morphWeights.ElementAt(index);
                // Set the animation key
                BabylonAnimationKey key = new BabylonAnimationKey()
                {
                    frame = (int)keyValue.Key,
                    values = new float[] { (float)keyValue.Value[weightIndex] }
                };

                keys.Add(key);
            }

            List<BabylonAnimationKey> keysFull = new List<BabylonAnimationKey>(keys);

            // Optimization
            OptimizeAnimations(keys, false);

            // Ensure animation has at least 2 frames
            if (IsAnimationKeysRelevant(keys, "influence"))
            {
                // Animations
                animation = new BabylonAnimation()
                {
                    name = "influence animation", // override default animation name
                    dataType = (int)BabylonAnimation.DataType.Float,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    framePerSecond = Loader.GetFPS(),
                    keys = keys.ToArray(),
                    keysFull = keysFull,
                    property = "influence"
                };

                animations.Add(animation);
            }

            return animations;
        }


        /// <summary>
        /// Using the Maya object name, find its keyframe.
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns>A sorted list of keyframe without duplication. This list contains at least the first and last key of the time range.</returns>
        public IList<double> GetKeyframes(string objectName)
        {
            IList<double> keys= new List<double>();

            int start = Loader.GetMinTime();
            int end = Loader.GetMaxTime();

            // Get the keyframe
            try
            {
                MDoubleArray keyArray = new MDoubleArray();
                MGlobal.executeCommand($"keyframe -t \":\" -q -timeChange {objectName}", keyArray);

                keyArray.Add(start);
                keyArray.Add(end);

                SortedSet<double> sortedKeys = new SortedSet<double>(keyArray);
                keys = new List<double>(sortedKeys);

            }
            catch { }

            return keys;
        }


        /// <summary>
        /// Using MEL commands, it return the babylon animation
        /// </summary>
        /// <param name="objectName">The name of the Maya object</param>
        /// <param name="mayaProperty">The attribut in Maya</param>
        /// <param name="babylonProperty">The attribut in Babylon</param>
        /// <returns>A Babylon animation that represents the Maya animation</returns>
        public BabylonAnimation GetAnimationFloat(string objectName, string mayaProperty, string babylonProperty)
        {
            // Get keyframes
            IList<double> keyframes = GetKeyframes(objectName);
            BabylonAnimation animation = null;

            // set the key for each keyframe
            List<BabylonAnimationKey> keys = new List<BabylonAnimationKey>();

            for (int index = 0; index < keyframes.Count; index++)
            {
                double keyframe = keyframes[index];
                MGlobal.executeCommand($"getAttr -t {keyframe} {objectName}.{mayaProperty}", out double value);

                // Set the animation key
                BabylonAnimationKey key = new BabylonAnimationKey()
                {
                    frame = (int)keyframe,
                    values = new float[] { (float)value }
                };

                keys.Add(key);
            }

            List<BabylonAnimationKey> keysFull = new List<BabylonAnimationKey>(keys);

            // Optimization
            OptimizeAnimations(keys, false);

            // Ensure animation has at least 2 frames
            if (IsAnimationKeysRelevant(keys, babylonProperty))
            {
                // Animations
                animation = new BabylonAnimation()
                {
                    name = $"{babylonProperty} animation", // override default animation name
                    dataType = (int)BabylonAnimation.DataType.Float,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    framePerSecond = Loader.GetFPS(),
                    keys = keys.ToArray(),
                    keysFull = keysFull,
                    property = babylonProperty
                };
            }

            return animation;
        }

        /// <summary>
        /// Convert the Maya texture animation of a MFnDependencyNode in Babylon animations
        /// </summary>
        /// <param name="textureDependencyNode">The MFnDependencyNode of the texture</param>
        /// <returns>A list of texture animation</returns>
        public List<BabylonAnimation> GetTextureAnimations(MFnDependencyNode textureDependencyNode)
        {
            List<BabylonAnimation> animations = new List<BabylonAnimation>();

            // Look for a "place2dTexture" object in the connections of the node.
            // The "place2dTexture" object contains the animation parameters
            MPlugArray connections = new MPlugArray();
            textureDependencyNode.getConnections(connections);

            int index = 0;
            string place2dTexture = null;
            while (index < connections.Count && place2dTexture == null)
            {
                MPlug connection = connections[index];
                MObject source = connection.source.node;
                if (source != null && source.hasFn(MFn.Type.kPlace2dTexture))
                {
                    MFnDependencyNode node = new MFnDependencyNode(source);
                    place2dTexture = node.name;
                }
                index++;
            }

            if(place2dTexture != null)
            {
                IDictionary<string, string> properties = new Dictionary<string, string>
                {
                    ["offsetU"] = "uOffset",
                    ["offsetU"] = "uOffset",
                    ["offsetV"] = "vOffset",
                    ["repeatU"] = "uScale",
                    ["repeatV"] = "vScale"
                };

                // Get the animation for each properties
                for (index = 0; index < properties.Count; index++)
                {
                    KeyValuePair<string, string> property = properties.ElementAt(index);
                    BabylonAnimation animation = GetAnimationFloat(place2dTexture, property.Key, property.Value);

                    if(animation != null)
                    {
                        animations.Add(animation);
                    }
                }

                // For the rotation, convert degree to radian
                BabylonAnimation rotationAnimation = GetAnimationFloat(place2dTexture, "rotateFrame", "wAng");
                if (rotationAnimation != null)
                {
                    BabylonAnimationKey[] keys = rotationAnimation.keys;
                    for (index = 0; index < keys.Length; index++)
                    {
                        var key = keys[index];
                        key.values[0] *= (float)(Math.PI / 180d);
                    }
                    animations.Add(rotationAnimation);
                }
            }

            return animations;
        }


        private IList<BabylonAnimationGroup> ExportAnimationGroups(BabylonScene babylonScene)
        {
            IList<BabylonAnimationGroup> animationGroups = new List<BabylonAnimationGroup>();

            // Retrieve and parse animation group data
            AnimationGroupList animationList = AnimationGroupList.InitAnimationGroups(this);
            bool exportNonAnimated = Loader.GetBoolProperty("babylonjs_animgroup_exportnonanimated");

            foreach (AnimationGroup animGroup in animationList)
            {
                RaiseMessage("Exporter.animationGroups | " + animGroup.Name, 1);

                BabylonAnimationGroup animationGroup = new BabylonAnimationGroup
                {
                    name = animGroup.Name,
                    from = animGroup.FrameStart,
                    to = animGroup.FrameEnd,
                    targetedAnimations = new List<BabylonTargetedAnimation>()
                };

                // add animations of each nodes in the animGroup
                List<BabylonNode> nodes = new List<BabylonNode>();
                nodes.AddRange(babylonScene.MeshesList);
                nodes.AddRange(babylonScene.CamerasList);
                nodes.AddRange(babylonScene.LightsList);

                foreach(BabylonMorphTargetManager morphTargetManager in babylonScene.MorphTargetManagersList)
                {
                    var morphTargets = morphTargetManager.targets;
                    if (morphTargets != null)
                    {
                        foreach (BabylonMorphTarget morphTarget in morphTargets)
                        {
                            var animations = GetSubAnimations(morphTarget, animationGroup.from, animationGroup.to);
                            foreach (BabylonAnimation animation in animations)
                            {
                                BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                {
                                    animation = animation,
                                    targetId = morphTarget.id
                                };
                                animationGroup.targetedAnimations.Add(targetedAnimation);
                            }
                        }
                    }
                    else
                    {
                        this.RaiseWarning("Empty BabylonMorphTargetManager found");
                    }
                }

                foreach (BabylonNode node in nodes)
                {
                    if (node.animations != null && node.animations.Length != 0)
                    {
                        IList<BabylonAnimation> animations = GetSubAnimations(node, animationGroup.from, animationGroup.to);
                        if (animations != null)
                        {
                            foreach (BabylonAnimation animation in animations)
                            {
                                BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                {
                                    animation = animation,
                                    targetId = node.id
                                };

                                animationGroup.targetedAnimations.Add(targetedAnimation);
                            }
                        }
                    }
                    else if (exportNonAnimated)
                    {
                        BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                        {
                            animation = CreatePositionAnimation(animationGroup.from, animationGroup.to, node.position),
                            targetId = node.id
                        };

                        animationGroup.targetedAnimations.Add(targetedAnimation);
                    }
                }

                foreach (BabylonSkeleton skel in babylonScene.SkeletonsList)
                {
                    if (skel.bones != null)
                    {
                        foreach (BabylonBone bone in skel.bones)
                        {
                            if (bone.animation != null)
                            {
                                IList<BabylonAnimation> animations = GetSubAnimations(bone, animationGroup.from, animationGroup.to);
                                foreach (BabylonAnimation animation in animations)
                                {
                                    BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                    {
                                        animation = animation,
                                        targetId = bone.id
                                    };

                                    animationGroup.targetedAnimations.Add(targetedAnimation);
                                }
                            }
                            else if (exportNonAnimated)
                            {
                                BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                {
                                    animation = CreateMatrixAnimation(animationGroup.from, animationGroup.to, bone.matrix),
                                    targetId = bone.id
                                };

                                animationGroup.targetedAnimations.Add(targetedAnimation);
                            }
                        }
                    }
                    else
                    {
                        this.RaiseWarning($"Empty Skeleton found {skel.name??string.Empty}");
                    }
                }

                if (animationGroup.targetedAnimations.Count > 0)
                {
                    animationGroups.Add(animationGroup);
                }
            }

            return animationGroups;
        }

        private IList<BabylonAnimation> GetSubAnimations(BabylonNode babylonNode, float from, float to)
        {
            IList<BabylonAnimation> subAnimations = new List<BabylonAnimation>();

            foreach (BabylonAnimation nodeAnimation in babylonNode.animations)
            {
                // clone the animation
                BabylonAnimation animation = (BabylonAnimation)nodeAnimation.Clone();

                // Select usefull keys
                var keys = animation.keysFull.FindAll(k => from <= k.frame && k.frame <= to);
                AddBoundaryKeyframes(animation, keys, from, to);

                bool keysInRangeAreRelevant = true;

                // Optimize these keys
                if (exportParameters.optimizeAnimations)
                {
                    OptimizeAnimations(keys, true);
                    keysInRangeAreRelevant = IsAnimationKeysRelevant(keys, animation.property);

                    // Do a less efficient check against all frames in the scene for this animation channel if the first check fails, to make sure we aren't overoptimizing
                    if (!keysInRangeAreRelevant)
                    {
                        List<BabylonAnimationKey> optimizedKeysFull = new List<BabylonAnimationKey>(nodeAnimation.keysFull);
                        OptimizeAnimations(optimizedKeysFull, true);
                        keysInRangeAreRelevant = IsAnimationKeysRelevant(optimizedKeysFull, nodeAnimation.property);
                    }
                }

                // If animation keys should be included in export, add to animation list.
                if (keysInRangeAreRelevant)
                {
                    animation.keys = keys.ToArray();
                    subAnimations.Add(animation);
                }
            }

            return subAnimations;
        }

        private IList<BabylonAnimation> GetSubAnimations(BabylonMorphTarget babylonMorphTarget, float from, float to)
        {
            IList<BabylonAnimation> subAnimations = new List<BabylonAnimation>();

            foreach (BabylonAnimation morphTargetAnimation in babylonMorphTarget.animations)
            {
                // clone the animation
                BabylonAnimation animation = (BabylonAnimation)morphTargetAnimation.Clone();

                // Select usefull keys
                var keys = animation.keysFull.FindAll(k => from <= k.frame && k.frame <= to);

                AddBoundaryKeyframes(animation, keys, from, to);

                bool keysInRangeAreRelevant = true;

                // Optimize these keys
                if (exportParameters.optimizeAnimations)
                {
                    // Optimize these keys
                    OptimizeAnimations(keys, true);
                    keysInRangeAreRelevant = IsAnimationKeysRelevant(keys, animation.property);

                    // Do a less efficient check against all frames in the scene for this animation channel if the first check fails, to make sure we aren't overoptimizing
                    if (!keysInRangeAreRelevant)
                    {
                        List<BabylonAnimationKey> optimizedKeysFull = new List<BabylonAnimationKey>(animation.keysFull);
                        OptimizeAnimations(optimizedKeysFull, true);
                        keysInRangeAreRelevant = IsAnimationKeysRelevant(optimizedKeysFull, animation.property);
                    }
                }

                // If animation keys should be included in export, add to animation list.
                if (keysInRangeAreRelevant)
                {
                    animation.keys = keys.ToArray();
                    subAnimations.Add(animation);
                }
            }

            return subAnimations;
        }

        private IList<BabylonAnimation> GetSubAnimations(BabylonBone babylonBone, float from, float to)
        {
            IList<BabylonAnimation> subAnimations = new List<BabylonAnimation>();

            // clone the animation
            BabylonAnimation animation = (BabylonAnimation)babylonBone.animation.Clone();

            // Select usefull keys
            var keys = animation.keysFull.FindAll(k => from <= k.frame && k.frame <= to);

            AddBoundaryKeyframes(animation, keys, from, to);

            bool keysInRangeAreRelevant = true;

            // Optimize these keys
            if (exportParameters.optimizeAnimations)
            {

                // Optimize these keys
                OptimizeAnimations(keys, false);
                keysInRangeAreRelevant = IsAnimationKeysRelevant(keys, animation.property);

                // Do a less efficient check against all frames in the scene for this animation channel if the first check fails, to make sure we aren't overoptimizing
                if (!keysInRangeAreRelevant)
                {
                    List<BabylonAnimationKey> optimizedKeysFull = new List<BabylonAnimationKey>(animation.keysFull);
                    OptimizeAnimations(optimizedKeysFull, true);
                    keysInRangeAreRelevant = IsAnimationKeysRelevant(optimizedKeysFull, animation.property);
                }
            }

            // If animation keys should be included in export, add to animation list.
            if (keysInRangeAreRelevant)
            {
                animation.keys = keys.ToArray();
                subAnimations.Add(animation);
            }

            return subAnimations;
        }

        private void AddBoundaryKeyframes(BabylonAnimation animation, List<BabylonAnimationKey> keysInRange, float from, float to)
        {
            // Add extra boundary keyframes to start and end of segment if appropriate
            // add a first frame key if we need to:
            if (!keysInRange.Any(key => key.frame == from))
            {
                var lastKeyBeforeFrom = animation.keysFull.LastOrDefault(key => key.frame < from);
                if (lastKeyBeforeFrom != null)
                {
                    var firstKeyAfterFrom = animation.keysFull.FirstOrDefault(key => key.frame > from);
                    var interpolatedKey = new BabylonAnimationKey() { frame = from };
                    if (firstKeyAfterFrom != null)
                    {
                        interpolatedKey.values = BabylonAnimationKey.Interpolate(animation, lastKeyBeforeFrom, firstKeyAfterFrom, from);
                    }
                    else
                    {
                        interpolatedKey.values = new List<float>(lastKeyBeforeFrom.values).ToArray();
                    }
                    keysInRange.Insert(0, interpolatedKey);
                }
            }

            // add a last frame key if we need to:
            if (!keysInRange.Any(key => key.frame == to))
            {
                var lastKeyBeforeTo = animation.keysFull.LastOrDefault(key => key.frame < to);
                if (lastKeyBeforeTo != null)
                {
                    var firstKeyAfterTo = animation.keysFull.FirstOrDefault(key => key.frame > to);
                    var interpolatedKey = new BabylonAnimationKey() { frame = to };
                    if (firstKeyAfterTo != null)
                    {
                        interpolatedKey.values = BabylonAnimationKey.Interpolate(animation, lastKeyBeforeTo, firstKeyAfterTo, to);
                    }
                    else
                    {
                        interpolatedKey.values = new List<float>(lastKeyBeforeTo.values).ToArray();
                    }
                    keysInRange.Add(interpolatedKey);
                }
            }
        }

        private BabylonAnimation CreatePositionAnimation(float from, float to, float[] position)
        {
            BabylonAnimation animation = new BabylonAnimation
            {
                name = "position animation",
                property = "position",
                dataType = 1,
                loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                framePerSecond = Loader.GetFPS(),
                keysFull = new List<BabylonAnimationKey>()
            };

            animation.keysFull.Add(new BabylonAnimationKey
            {
                frame = (int)from,
                values = position
            });
            animation.keysFull.Add(new BabylonAnimationKey
            {
                frame = (int)to,
                values = position
            });

            animation.keys = animation.keysFull.ToArray();

            return animation;
        }

        private BabylonAnimation CreateMatrixAnimation(float from, float to, float[] matrix)
        {
            BabylonAnimation animation = new BabylonAnimation
            {
                name = "_matrix animation",
                property = "_matrix",
                dataType = 3,
                loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                framePerSecond = Loader.GetFPS(),
                keysFull = new List<BabylonAnimationKey>()
            };

            animation.keysFull.Add(new BabylonAnimationKey
            {
                frame = (int)from,
                values = matrix
            });
            animation.keysFull.Add(new BabylonAnimationKey
            {
                frame = (int)from,
                values = matrix
            });

            animation.keys = animation.keysFull.ToArray();

            return animation;
        }
    }
}
