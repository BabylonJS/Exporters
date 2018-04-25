using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        /// <summary>
        /// Export TRS and visiblity animations of the transform
        /// </summary>
        /// <param name="babylonNode"></param>
        /// <param name="mFnTransform">Transform above mesh/camera/light</param>
        private void ExportNodeAnimation(BabylonNode babylonNode, MFnTransform mFnTransform)
        {
            try
            {
                babylonNode.animations = GetAnimation(mFnTransform).ToArray();
                
                // TODO - Retreive from Maya
                babylonNode.autoAnimate = true;
                babylonNode.autoAnimateFrom = GetMinTime()[0];
                babylonNode.autoAnimateTo = GetMaxTime()[0];
                babylonNode.autoAnimateLoop = true;
            }
            catch (Exception e)
            {
                RaiseVerbose("No animation found", 2);
            }
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

            List<BabylonAnimationKey> keys = new List<BabylonAnimationKey>();
            List<BabylonAnimation> animationsObject = new List<BabylonAnimation>();

            //Get the animCurve
            MGlobal.executeCommand("listConnections -type \"animCurve\" " + transform.fullPathName + ";", animCurvList);

            foreach (String animCurv in animCurvList)
            {
                //Get the key time for each curves
                MGlobal.executeCommand("keyframe -q " + animCurv + ";", keysTime);
                //Get the value for each curves
                MGlobal.executeCommand("keyframe - q - vc - absolute " + animCurv + ";", keysValue);
                
                //Parse for each type of curve
                foreach (float keyValue in keysValue)
                {
                    if (animCurv.Contains("translate"))
                    {
                        translateValues.Add(keyValue);
                    }
                    else if (animCurv.Contains("rotate"))
                    {
                        rotateValues.Add(keyValue);
                    }
                    else if (animCurv.Contains("scale"))
                    {
                        scaleValues.Add(keyValue);
                    }
                    else if (animCurv.Contains("visibility"))
                    {
                        visibilityValues.Add(keyValue);
                    }
                }
            }

            //Optimisation for same keys values
            var scaleQuery = scaleValues.Where(num => num == 1);
            var visQuery = visibilityValues.Where(num => num == 1);

            // Switch coordinate system at object level
            // Position.z
            for (int j = keysTime.Count * 2; j < keysTime.Count * 3; j++)
            {
                translateValues[j] *= -1;
            }
            // Rotation.x and Rotation.y
            for (int j = 0; j < keysTime.Count * 2; j++)
            {
                rotateValues[j] *= -1;
            }

            // --- Position ---
            List<BabylonAnimationKey> keysObject = new List<BabylonAnimationKey>();
            int testOpti = 0;
            long i = 0;

            foreach (int keyTime in keysTime)
            {
                float[] vectorValuesTestOpti = { translateValues[0], translateValues[(int)(keysTime.length)], translateValues[(int)(keysTime.length * 2)] };
                float[] vectorValues = { translateValues[(int)i], translateValues[(int)(i + keysTime.length)], translateValues[(int)(i + (keysTime.length * 2))] };

                //Optimisation for same keys values
                if (vectorValuesTestOpti[0] == vectorValues[0] && vectorValuesTestOpti[1] == vectorValues[1] && vectorValuesTestOpti[2] == vectorValues[2])
                {
                    testOpti++;
                }

                keysObject.Add(new BabylonAnimationKey()
                {
                    frame = keyTime,
                    values = vectorValues
                });
                i++;
            }

            if (testOpti != keysTime.length)
            {
                animationsObject.Add(new BabylonAnimation()
                {
                    dataType = 1,
                    name = "position animation",
                    framePerSecond = 30,
                    loopBehavior = 1,
                    property = "position",
                    keys = keysObject.ToArray()
                });
            }

            // --- Rotation ---
            keysObject = new List<BabylonAnimationKey>();
            testOpti = 0;
            i = 0;

            foreach (int keyTime in keysTime)
            {
                BabylonVector3 vectorValues = new BabylonVector3(rotateValues[(int)i], rotateValues[(int)(i + keysTime.length)], rotateValues[(int)(i + (keysTime.length * 2))]);
                BabylonVector3 vectorValuesTestOpti = new BabylonVector3(rotateValues[0], rotateValues[(int)(keysTime.length)], rotateValues[(int)(keysTime.length * 2)]);

                float[] quatValuesTestOpti = { vectorValuesTestOpti.toQuaternion().X, vectorValuesTestOpti.toQuaternion().Y, vectorValuesTestOpti.toQuaternion().Z, vectorValuesTestOpti.toQuaternion().W };
                float[] quatValues = { vectorValues.toQuaternion().X, vectorValues.toQuaternion().Y, vectorValues.toQuaternion().Z, vectorValues.toQuaternion().W };

                if (quatValuesTestOpti[0] == quatValues[0] && quatValuesTestOpti[1] == quatValues[1] && quatValuesTestOpti[2] == quatValues[2] && quatValuesTestOpti[3] == quatValues[3])
                {
                    testOpti++;
                }

                keysObject.Add(new BabylonAnimationKey()
                {
                    frame = keyTime,
                    values = quatValues
                });
                i++;
            }

            if (testOpti != keysTime.length)
            {
                animationsObject.Add(new BabylonAnimation()
                {
                    dataType = 2,
                    name = "rotationQuaternion animation",
                    framePerSecond = 30,
                    loopBehavior = 1,
                    property = "rotationQuaternion",
                    keys = keysObject.ToArray()
                });
            }

            // --- Scaling ---
            if (scaleValues.length != scaleQuery.Count())
            {
                keysObject = new List<BabylonAnimationKey>();
                testOpti = 0;
                i = 0;

                foreach (int keyTime in keysTime)
                {
                    float[] vectorValuesTestOpti = { translateValues[0], translateValues[(int)(keysTime.length)], translateValues[(int)(keysTime.length * 2)] };
                    float[] vectorValues = { scaleValues[(int)i], scaleValues[(int)(i + keysTime.length)], scaleValues[(int)(i + (keysTime.length * 2))] };

                    if (vectorValuesTestOpti[0] == vectorValues[0] && vectorValuesTestOpti[1] == vectorValues[1] && vectorValuesTestOpti[2] == vectorValues[2])
                    {
                        testOpti++;
                    }

                    keysObject.Add(new BabylonAnimationKey()
                    {
                        frame = keyTime,
                        values = vectorValues
                    });
                    i++;
                }

                if (testOpti != keysTime.length)
                {
                    animationsObject.Add(new BabylonAnimation()
                    {
                        dataType = 1,
                        name = "scaling animation",
                        framePerSecond = 30,
                        loopBehavior = 1,
                        property = "scaling",
                        keys = keysObject.ToArray()
                    });
                }
            }

            // --- Visibility ---
            if (visibilityValues.length != visQuery.Count())
            {
                keysObject = new List<BabylonAnimationKey>();
                i = 0;

                foreach (int keyTime in keysTime)
                {
                    float[] visibilityValue = { visibilityValues[(int)i] };
                    keysObject.Add(new BabylonAnimationKey()
                    {
                        frame = keyTime,
                        values = visibilityValue
                    });
                    i++;
                }

                animationsObject.Add(new BabylonAnimation()
                {
                    dataType = 0,
                    name = "visibility animation",
                    framePerSecond = 30,
                    loopBehavior = 1,
                    property = "visibility",
                    keys = keysObject.ToArray()
                });
            }

            return animationsObject;
        }

        private MIntArray GetMinTime()
        {
            MIntArray minTime = new MIntArray();
            MGlobal.executeCommand("playbackOptions -q -minTime", minTime);
            return minTime;
        }

        private MIntArray GetMaxTime()
        {
            MIntArray maxTime = new MIntArray();
            MGlobal.executeCommand("playbackOptions -q -maxTime", maxTime);
            return maxTime;
        }
    }
}
