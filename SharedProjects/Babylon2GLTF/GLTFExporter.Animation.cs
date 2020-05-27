using BabylonExport.Entities;
using GLTFExport.Entities;
using GLTFExport.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {

        private void ExportAnimationGroups(GLTF gltf, BabylonScene babylonScene)
        {
            // Retreive and parse animation group data
            var animationGroupList = babylonScene.animationGroups;
            var animationGroupCount = animationGroupList == null ? 0 : animationGroupList.Count;

            gltf.AnimationsList.Clear();
            gltf.AnimationsList.Capacity = Math.Max(gltf.AnimationsList.Capacity, animationGroupCount);

            if (animationGroupCount <= 0)
            {
                logger.RaiseMessage("GLTFExporter.Animation | No AnimationGroups: exporting all animations together.", 1);
                GLTFAnimation gltfAnimation = new GLTFAnimation();
                gltfAnimation.name = "All Animations";
                
                int startFrame = babylonScene.TimelineStartFrame;
                int endFrame = babylonScene.TimelineEndFrame;

                foreach (var pair in nodeToGltfNodeMap)
                {
                    BabylonNode node = pair.Key;
                    GLTFNode gltfNode = pair.Value;
                    bool nodeHasAnimations = node.animations != null && node.animations.Length > 0 && node.animations[0] != null;
                    bool nodeHasExtraAnimations = node.extraAnimations != null && node.extraAnimations.Count > 0 && node.extraAnimations[0] != null;
                    BabylonMesh meshNode = node as BabylonMesh;
                    BabylonMorphTargetManager morphTargetManager = null;
                    bool nodeHasAnimatedMorphTargets = false;
                    if (meshNode != null && meshNode.morphTargetManagerId != null)
                    {
                        morphTargetManager = GetBabylonMorphTargetManager(babylonScene, meshNode);
                        if (morphTargetManager != null)
                        {
                            nodeHasAnimatedMorphTargets = morphTargetManager.targets.Any(target => target.animations != null && target.animations.Length > 0 && target.animations[0] != null);
                        }
                    }

                    if (!nodeHasAnimations && !nodeHasExtraAnimations && !nodeHasAnimatedMorphTargets) continue;
                    if (nodeHasAnimations && node.animations[0].property == "_matrix")
                    {
                        ExportBoneAnimation(gltfAnimation, startFrame, endFrame, gltf, node, pair.Value);
                    }
                    else
                    {
                        ExportNodeAnimation(gltfAnimation, startFrame, endFrame, gltf, node, gltfNode, babylonScene);
                    }

                    if (nodeHasAnimatedMorphTargets)
                    {
                        ExportMorphTargetWeightAnimation(morphTargetManager, gltf, gltfNode, gltfAnimation.ChannelList, gltfAnimation.SamplerList, startFrame, endFrame, babylonScene);
                    }
                }

                if (gltfAnimation.ChannelList.Count > 0)
                {
                    gltf.AnimationsList.Add(gltfAnimation);
                }
                else
                {
                    logger.RaiseMessage("GLTFExporter.Animation | No animation data for this animation, it is ignored.", 2);
                }
            }
            else
            {
                foreach (BabylonAnimationGroup animGroup in animationGroupList)
                {
                    logger.RaiseMessage("GLTFExporter.Animation | " + animGroup.name, 1);

                    GLTFAnimation gltfAnimation = new GLTFAnimation();
                    gltfAnimation.name = animGroup.name;
                    
                    int startFrame = MathUtilities.RoundToInt(animGroup.from);
                    int endFrame = MathUtilities.RoundToInt(animGroup.to);

                    var uniqueNodeIds = animGroup.targetedAnimations.Select(targetAnim => targetAnim.targetId).Distinct();
                    foreach ( var id in uniqueNodeIds )
                    {
                        BabylonNode babylonNode = babylonNodes.Find(node => node.id.Equals(id));
                        GLTFNode gltfNode = null;
                        // search the babylon scene id map for the babylon node that matches this id
                        if (babylonNode != null)
                        {
                            BabylonMorphTargetManager morphTargetManager = null;

                            // search our babylon->gltf node mapping to see if this node is included in the exported gltf scene
                            if(!nodeToGltfNodeMap.TryGetValue(babylonNode, out gltfNode))
                            {
                                continue;
                            }

                            bool nodeHasAnimations = babylonNode.animations != null && babylonNode.animations.Length > 0 && babylonNode.animations[0] != null;
                            bool nodeHasExtraAnimations = babylonNode.extraAnimations != null && babylonNode.extraAnimations.Count > 0 && babylonNode.extraAnimations[0] != null;
                            if (!nodeHasAnimations && !nodeHasExtraAnimations) continue;

                            if (nodeHasAnimations && babylonNode.animations[0].property == "_matrix") //TODO: Is this check accurate for deciphering between bones and nodes?
                            {
                                ExportBoneAnimation(gltfAnimation, startFrame, endFrame, gltf, babylonNode, gltfNode, animGroup);
                            }
                            else
                            {
                                ExportNodeAnimation(gltfAnimation, startFrame, endFrame, gltf, babylonNode, gltfNode, babylonScene, animGroup);
                            }
                        }
                        else
                        {
                            // if the node isn't found in the scene id map, check if it is the id for a morph target
                            BabylonMorphTargetManager morphTargetManager = babylonScene.morphTargetManagers.FirstOrDefault(mtm => mtm.targets.Any(target => target.animations != null && target.animations.Length > 0 && target.animations[0] != null));
                            if (morphTargetManager != null)
                            {
                                BabylonMesh mesh = morphTargetManager.sourceMesh;
                                if (mesh != null && nodeToGltfNodeMap.TryGetValue(mesh, out gltfNode))
                                {
                                    ExportMorphTargetWeightAnimation(morphTargetManager, gltf, gltfNode, gltfAnimation.ChannelList, gltfAnimation.SamplerList, startFrame, endFrame, babylonScene);
                                }
                            }
                        }
                    }

                    if (gltfAnimation.ChannelList.Count > 0)
                    {
                        gltf.AnimationsList.Add(gltfAnimation);
                    }
                    else
                    {
                        logger.RaiseMessage("No data exported for this animation, it is ignored.", 2);
                    }
                    // clear the exported morph target cache, since we are exporting a new animation group. //TODO: we should probably do this more elegantly.
                    exportedMorphTargets.Clear();
                }
            }
        }

        private void ExportNodeAnimation(GLTFAnimation gltfAnimation, int startFrame, int endFrame, GLTF gltf, BabylonNode babylonNode, GLTFNode gltfNode, BabylonScene babylonScene, BabylonAnimationGroup animationGroup = null)
        {
            var channelList = gltfAnimation.ChannelList;
            var samplerList = gltfAnimation.SamplerList;

            bool exportNonAnimated = exportParameters.animgroupExportNonAnimated;
            
            // Combine babylon animations from .babylon file and cached ones
            var babylonAnimations = new List<BabylonAnimation>();
            if (animationGroup != null)
            {
                var targetedAnimation = animationGroup.targetedAnimations.FirstOrDefault(animation => animation.targetId == babylonNode.id);
                if (targetedAnimation != null)
                {
                    babylonAnimations.Add(targetedAnimation.animation);
                }
            }

            // Do not include the node animations if a provided animation group already includes them.
            if (babylonNode.animations != null && babylonAnimations.Count <= 0)
            {
                babylonAnimations.AddRange(babylonNode.animations);
            }
            if (babylonNode.extraAnimations != null)
            {
                babylonAnimations.AddRange(babylonNode.extraAnimations);
            }

            // Filter animations to only keep TRS ones
            babylonAnimations = babylonAnimations.FindAll(babylonAnimation => _getTargetPath(babylonAnimation.property) != null);

            if (babylonAnimations.Count > 0 || exportNonAnimated)
            {
                if (babylonAnimations.Count > 0)
                {
                    logger.RaiseMessage("GLTFExporter.Animation | Export animations of node named: " + babylonNode.name, 2);
                }
                else if (exportNonAnimated)
                {
                    logger.RaiseMessage("GLTFExporter.Animation | Export dummy animation for node named: " + babylonNode.name, 2);
                    // Export a dummy animation
                    babylonAnimations.Add(GetDummyAnimation(gltfNode, startFrame, endFrame, babylonScene));
                }


                foreach (BabylonAnimation babylonAnimation in babylonAnimations)
                {

                    var babylonAnimationKeysInRange = babylonAnimation.keys.Where(key => key.frame >= startFrame && key.frame <= endFrame);
                    if (babylonAnimationKeysInRange.Count() <= 0)
                        continue;

                    // Target
                    var gltfTarget = new GLTFChannelTarget
                    {
                        node = gltfNode.index
                    };
                    gltfTarget.path = _getTargetPath(babylonAnimation.property);

                    // --- Input ---
                    var accessorInput = _createAndPopulateInput(gltf, babylonAnimation, startFrame, endFrame);
                    if (accessorInput == null)
                        continue;

                    // --- Output ---
                    GLTFAccessor accessorOutput = _createAccessorOfPath(gltfTarget.path, gltf);

                    // Populate accessor
                    int numKeys = 0;
                    foreach (var babylonAnimationKey in babylonAnimationKeysInRange)
                    {
                        numKeys++;

                        // copy data before changing it in case animation groups overlap
                        float[] outputValues = new float[babylonAnimationKey.values.Length];
                        babylonAnimationKey.values.CopyTo(outputValues,0);

                        // Switch coordinate system at object level
                        if (babylonAnimation.property == "position")
                        {
                            outputValues[2] *= -1;
                        }
                        else if (babylonAnimation.property == "rotationQuaternion")
                        {
                            outputValues[0] *= -1;
                            outputValues[1] *= -1;
                        }

                        // Store values as bytes
                        foreach (var outputValue in outputValues)
                        {
                            accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
                        }
                    };
                    accessorOutput.count = numKeys;
                    if (accessorOutput.count == 0)
                    {
                        logger.RaiseWarning(String.Format("GLTFExporter.Animation | No frames to export in node animation \"{1}\" of node named \"{0}\". This will cause an error in the output gltf.", babylonNode.name, babylonAnimation.name));
                    }

                    // Animation sampler
                    var gltfAnimationSampler = new GLTFAnimationSampler
                    {
                        input = accessorInput.index,
                        output = accessorOutput.index
                    };
                    gltfAnimationSampler.index = samplerList.Count;
                    samplerList.Add(gltfAnimationSampler);

                    // Channel
                    var gltfChannel = new GLTFChannel
                    {
                        sampler = gltfAnimationSampler.index,
                        target = gltfTarget
                    };
                    channelList.Add(gltfChannel);
                }
            }

            ExportGLTFExtension(babylonNode, ref gltfAnimation,gltf);
        }

        private void ExportBoneAnimation(GLTFAnimation gltfAnimation, int startFrame, int endFrame, GLTF gltf, BabylonNode babylonNode, GLTFNode gltfNode, BabylonAnimationGroup animationGroup = null)
        {
            var channelList = gltfAnimation.ChannelList;
            var samplerList = gltfAnimation.SamplerList;

            if (babylonNode.animations != null && babylonNode.animations[0].property == "_matrix")
            {
                logger.RaiseMessage("GLTFExporter.Animation | Export animation of bone named: " + babylonNode.name, 2);

                BabylonAnimation babylonAnimation = null;
                if (animationGroup != null)
                {
                    var targetedAnimation = animationGroup.targetedAnimations.FirstOrDefault(animation => animation.targetId == babylonNode.id);
                    if (targetedAnimation != null)
                    {
                        babylonAnimation = targetedAnimation.animation;
                    }
                }

                // otherwise fall back to the full animation track on the node.
                if (babylonAnimation == null)
                {
                    babylonAnimation = babylonNode.animations[0];
                }

                var babylonAnimationKeysInRange = babylonAnimation.keys.Where(key => key.frame >= startFrame && key.frame <= endFrame);
                if (babylonAnimationKeysInRange.Count() <= 0)
                    return;

                // --- Input ---
                var accessorInput = _createAndPopulateInput(gltf, babylonAnimation, startFrame, endFrame);
                if (accessorInput == null)
                    return;

                // --- Output ---
                var paths = new string[] { "translation", "rotation", "scale" };
                var accessorOutputByPath = new Dictionary<string, GLTFAccessor>();

                foreach (string path in paths)
                {
                    GLTFAccessor accessorOutput = _createAccessorOfPath(path, gltf);
                    accessorOutputByPath.Add(path, accessorOutput);
                }

                // Populate accessors
                foreach (var babylonAnimationKey in babylonAnimationKeysInRange)
                {
                    var matrix = new BabylonMatrix();
                    matrix.m = babylonAnimationKey.values;

                    var translationBabylon = new BabylonVector3();
                    var rotationQuatBabylon = new BabylonQuaternion();
                    var scaleBabylon = new BabylonVector3();
                    matrix.decompose(scaleBabylon, rotationQuatBabylon, translationBabylon);
                    
                    // Switch coordinate system at object level
                    translationBabylon.Z *= -1;
                    rotationQuatBabylon.X *= -1;
                    rotationQuatBabylon.Y *= -1;

                    var outputValuesByPath = new Dictionary<string, float[]>();
                    outputValuesByPath.Add("translation", translationBabylon.ToArray());
                    outputValuesByPath.Add("rotation", rotationQuatBabylon.ToArray());
                    outputValuesByPath.Add("scale", scaleBabylon.ToArray());

                    // Store values as bytes
                    foreach (string path in paths)
                    {
                        var accessorOutput = accessorOutputByPath[path];
                        var outputValues = outputValuesByPath[path];

                        foreach (var outputValue in outputValues)
                        {
                            accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
                        }
                        accessorOutput.count++;
                    }
                };

                foreach (string path in paths)
                {
                    var accessorOutput = accessorOutputByPath[path];

                    // Animation sampler
                    var gltfAnimationSampler = new GLTFAnimationSampler
                    {
                        input = accessorInput.index,
                        output = accessorOutput.index
                    };
                    gltfAnimationSampler.index = samplerList.Count;
                    samplerList.Add(gltfAnimationSampler);

                    // Target
                    var gltfTarget = new GLTFChannelTarget
                    {
                        node = gltfNode.index
                    };
                    gltfTarget.path = path;

                    // Channel
                    var gltfChannel = new GLTFChannel
                    {
                        sampler = gltfAnimationSampler.index,
                        target = gltfTarget
                    };
                    channelList.Add(gltfChannel);
                }
            }

            ExportGLTFExtension(babylonNode, ref gltfAnimation,gltf);
        }

        private BabylonAnimation GetDummyAnimation(GLTFNode gltfNode, int startFrame, int endFrame, BabylonScene babylonScene)
        {
            BabylonAnimation dummyAnimation = new BabylonAnimation();
            dummyAnimation.name = "Dummy";
            dummyAnimation.property = "scaling";
            dummyAnimation.framePerSecond = babylonScene.TimelineFramesPerSecond;
            dummyAnimation.dataType = (int)BabylonAnimation.DataType.Vector3;

            BabylonAnimationKey startKey = new BabylonAnimationKey();
            startKey.frame = startFrame;
            startKey.values = gltfNode.scale;

            BabylonAnimationKey endKey = new BabylonAnimationKey();
            endKey.frame = endFrame;
            endKey.values = gltfNode.scale;

            dummyAnimation.keys = new BabylonAnimationKey[] { startKey, endKey };

            return dummyAnimation;
        }

        private GLTFAccessor _createAndPopulateInput(GLTF gltf, BabylonAnimation babylonAnimation, int startFrame, int endFrame, bool offsetToStartAtFrameZero = true)
        {
            var babylonAnimationKeysInRange = babylonAnimation.keys.Where(key => key.frame >= startFrame && key.frame <= endFrame);
            if (babylonAnimationKeysInRange.Count() <= 0) // do not make empty accessors, so bail out.
                return null;

            var buffer = GLTFBufferService.Instance.GetBuffer(gltf);
            var accessorInput = GLTFBufferService.Instance.CreateAccessor(
                gltf,
                GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                "accessorAnimationInput",
                GLTFAccessor.ComponentType.FLOAT,
                GLTFAccessor.TypeEnum.SCALAR
            );
            // Populate accessor
            accessorInput.min = new float[] { float.MaxValue };
            accessorInput.max = new float[] { float.MinValue };

            int numKeys = 0;
            foreach (var babylonAnimationKey in babylonAnimationKeysInRange)
            {
                numKeys++;
                float inputValue = babylonAnimationKey.frame;
                if (offsetToStartAtFrameZero) inputValue -= startFrame;
                inputValue /= babylonAnimation.framePerSecond;
                // Store values as bytes
                accessorInput.bytesList.AddRange(BitConverter.GetBytes(inputValue));
                // Update min and max values
                GLTFBufferService.UpdateMinMaxAccessor(accessorInput, inputValue);
            };
            accessorInput.count = numKeys;

            if (accessorInput.count == 0)
            {
                logger.RaiseWarning(String.Format("GLTFExporter.Animation | No input frames in GLTF Accessor for animation \"{0}\". This will cause an error in the output gltf.", babylonAnimation.name));
            }

            return accessorInput;
        }

        private GLTFAccessor _createAccessorOfPath(string path, GLTF gltf)
        {
            var buffer = GLTFBufferService.Instance.GetBuffer(gltf);
            GLTFAccessor accessorOutput = null;
            switch (path)
            {
                case "translation":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatVec3(gltf, buffer),
                        "accessorAnimationPositions",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC3
                    );
                    break;
                case "rotation":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatVec4(gltf, buffer),
                        "accessorAnimationRotations",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC4
                    );
                    break;
                case "scale":
                    accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                        gltf,
                        GLTFBufferService.Instance.GetBufferViewAnimationFloatVec3(gltf, buffer),
                        "accessorAnimationScales",
                        GLTFAccessor.ComponentType.FLOAT,
                        GLTFAccessor.TypeEnum.VEC3
                    );
                    break;
            }
            return accessorOutput;
        }

        private List<BabylonMorphTargetManager> exportedMorphTargets = new List<BabylonMorphTargetManager>();
        private bool ExportMorphTargetWeightAnimation(BabylonMorphTargetManager babylonMorphTargetManager, GLTF gltf, GLTFNode gltfNode, List<GLTFChannel> channelList, List<GLTFAnimationSampler> samplerList, int startFrame, int endFrame, BabylonScene babylonScene, bool offsetToStartAtFrameZero = true)
        {
            if ( exportedMorphTargets.Contains(babylonMorphTargetManager) || !_isBabylonMorphTargetManagerAnimationValid(babylonMorphTargetManager))
            {
                return false;
            }

            var influencesPerFrame = _getTargetManagerAnimationsData(babylonMorphTargetManager);
            var frames = new List<float>(influencesPerFrame.Keys);

            var framesInRange = frames.Where(frame => frame >= startFrame && frame <= endFrame).ToList();
            framesInRange.Sort(); // Mandatory to sort otherwise gltf loader of babylon doesn't understand
            if (framesInRange.Count() <= 0)
                return false;

            logger.RaiseMessage("GLTFExporter.Animation | Export animation of morph target manager with id: " + babylonMorphTargetManager.id, 2);
            
            // Target
            var gltfTarget = new GLTFChannelTarget
            {
                node = gltfNode.index
            };
            gltfTarget.path = "weights";

            // Buffer
            var buffer = GLTFBufferService.Instance.GetBuffer(gltf);

            // --- Input ---
            var accessorInput = GLTFBufferService.Instance.CreateAccessor(
                gltf,
                GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                "accessorAnimationInput",
                GLTFAccessor.ComponentType.FLOAT,
                GLTFAccessor.TypeEnum.SCALAR
            );
            // Populate accessor
            accessorInput.min = new float[] { float.MaxValue };
            accessorInput.max = new float[] { float.MinValue };

            int numKeys = 0;
            foreach (var frame in framesInRange)
            {
                numKeys++;
                float inputValue = frame;
                if (offsetToStartAtFrameZero) inputValue -= startFrame;
                inputValue /= (float)babylonScene.TimelineFramesPerSecond;
                // Store values as bytes
                accessorInput.bytesList.AddRange(BitConverter.GetBytes(inputValue));
                // Update min and max values
                GLTFBufferService.UpdateMinMaxAccessor(accessorInput, inputValue);
            }
            accessorInput.count = numKeys;

            if (accessorInput.count == 0)
            {
                logger.RaiseWarning(String.Format("GLTFExporter.Animation | No frames to export in morph target animation \"weight\" for mesh named \"{0}\". This will cause an error in the output gltf.", babylonMorphTargetManager.sourceMesh.name));
            }

            // --- Output ---
            GLTFAccessor accessorOutput = GLTFBufferService.Instance.CreateAccessor(
                gltf,
                GLTFBufferService.Instance.GetBufferViewAnimationFloatScalar(gltf, buffer),
                "accessorAnimationWeights",
                GLTFAccessor.ComponentType.FLOAT,
                GLTFAccessor.TypeEnum.SCALAR
            );
            // Populate accessor
            foreach (var frame in framesInRange)
            {
                var outputValues = influencesPerFrame[frame];
                // Store values as bytes
                foreach (var outputValue in outputValues)
                {
                    accessorOutput.count++;
                    accessorOutput.bytesList.AddRange(BitConverter.GetBytes(outputValue));
                }
            }

            // Animation sampler
            var gltfAnimationSampler = new GLTFAnimationSampler
            {
                input = accessorInput.index,
                output = accessorOutput.index
            };
            gltfAnimationSampler.index = samplerList.Count;
            samplerList.Add(gltfAnimationSampler);

            // Channel
            var gltfChannel = new GLTFChannel
            {
                sampler = gltfAnimationSampler.index,
                target = gltfTarget
            };
            channelList.Add(gltfChannel);

            // Mark this morph target as exported.
            exportedMorphTargets.Add(babylonMorphTargetManager);
            return true;
        }

        private bool _isBabylonMorphTargetManagerAnimationValid(BabylonMorphTargetManager babylonMorphTargetManager)
        {
            bool hasAnimation = false;
            bool areAnimationsValid = true;
            foreach (var babylonMorphTarget in babylonMorphTargetManager.targets)
            {
                if (babylonMorphTarget.animations != null && babylonMorphTarget.animations.Length > 0)
                {
                    hasAnimation = true;

                    // Ensure target has only one animation
                    if (babylonMorphTarget.animations.Length > 1)
                    {
                        areAnimationsValid = false;
                        logger.RaiseWarning("GLTFExporter.Animation | Only one animation is supported for morph targets", 3);
                        continue;
                    }

                    // Ensure the target animation property is 'influence'
                    bool targetHasInfluence = false;
                    foreach (BabylonAnimation babylonAnimation in babylonMorphTarget.animations)
                    {
                        if (babylonAnimation.property == "influence")
                        {
                            targetHasInfluence = true;
                        }
                    }
                    if (targetHasInfluence == false)
                    {
                        areAnimationsValid = false;
                        logger.RaiseWarning("GLTFExporter.Animation | Only 'influence' animation is supported for morph targets", 3);
                        continue;
                    }
                }
            }

            return hasAnimation && areAnimationsValid;
        }

        /// <summary>
        /// The keys of each BabylonMorphTarget animation ARE NOT assumed to be identical.
        /// This function merges together all keys and binds to each an influence value for all targets.
        /// A target influence value is automatically computed when necessary.
        /// Computation rules are:
        /// - linear interpolation between target key range
        /// - constant value outside target key range
        /// </summary>
        /// <example>
        /// When:
        /// animation1.keys = {0, 25, 50, 100}
        /// animation2.keys = {50, 75, 100}
        /// 
        /// Gives:
        /// mergedKeys = {0, 25, 50, 100, 75}
        /// range1=[0, 100]
        /// range2=[50, 100]
        /// for animation1, the value associated to key=75 is the interpolation of its values between 50 and 100
        /// for animation2, the value associated to key=0 is equal to the one at key=50 since 0 is out of range [50, 100] (same for key=25)</example>
        /// <param name="babylonMorphTargetManager"></param>
        /// <returns>A map which for each frame, gives the influence value of all targets</returns>
        private Dictionary<float, List<float>> _getTargetManagerAnimationsData(BabylonMorphTargetManager babylonMorphTargetManager)
        {
            // Merge all keys into a single set (no duplicated frame)
            var mergedFrames = new HashSet<float>();
            foreach (var babylonMorphTarget in babylonMorphTargetManager.targets)
            {
                if (babylonMorphTarget.animations != null)
                {
                    var animation = babylonMorphTarget.animations[0];
                    foreach (BabylonAnimationKey animationKey in animation.keys)
                    {
                        mergedFrames.Add(animationKey.frame);
                    }
                }
            }

            // For each frame, gives the influence value of all targets (gltf structure)
            var influencesPerFrame = new Dictionary<float, List<float>>();
            foreach (var frame in mergedFrames)
            {
                influencesPerFrame.Add(frame, new List<float>());
            }
            foreach (var babylonMorphTarget in babylonMorphTargetManager.targets)
            {
                // For a given target, for each frame, gives the influence value of the target (babylon structure)
                var influencePerFrameForTarget = new Dictionary<float, float>();

                if (babylonMorphTarget.animations != null && babylonMorphTarget.animations.Length > 0)
                {
                    var animation = babylonMorphTarget.animations[0];

                    if (animation.keys.Length == 1)
                    {
                        // Same influence for all frames
                        var influence = animation.keys[0].values[0];
                        foreach (var frame in mergedFrames)
                        {
                            influencePerFrameForTarget.Add(frame, influence);
                        }
                    }
                    else
                    {
                        // Retreive target animation key range [min, max]
                        var babylonAnimationKeys = new List<BabylonAnimationKey>(animation.keys);
                        babylonAnimationKeys.Sort();
                        var minAnimationKey = babylonAnimationKeys[0];
                        var maxAnimationKey = babylonAnimationKeys[babylonAnimationKeys.Count - 1];
                        
                        foreach (var frame in mergedFrames)
                        {
                            // Surround the current frame with closest keys available for the target
                            BabylonAnimationKey lowerAnimationKey = minAnimationKey;
                            BabylonAnimationKey upperAnimationKey = maxAnimationKey;
                            foreach (BabylonAnimationKey animationKey in animation.keys)
                            {
                                if (lowerAnimationKey.frame < animationKey.frame && animationKey.frame <= frame)
                                {
                                    lowerAnimationKey = animationKey;
                                }
                                if (frame <= animationKey.frame && animationKey.frame < upperAnimationKey.frame)
                                {
                                    upperAnimationKey = animationKey;
                                }
                            }

                            // In case the target has a key for this frame
                            // or the current frame is out of target animation key range
                            if (lowerAnimationKey.frame == upperAnimationKey.frame)
                            {
                                influencePerFrameForTarget.Add(frame, lowerAnimationKey.values[0]);
                            }
                            else
                            {
                                // Interpolate influence values
                                var t = 1.0f * (frame - lowerAnimationKey.frame) / (upperAnimationKey.frame - lowerAnimationKey.frame);
                                var influence = MathUtilities.Lerp(lowerAnimationKey.values[0], upperAnimationKey.values[0], t);
                                influencePerFrameForTarget.Add(frame, influence);
                            }
                        }
                    }
                }
                else
                {
                    // Target is not animated
                    // Fill all frames with 0
                    foreach (var frame in mergedFrames)
                    {
                        influencePerFrameForTarget.Add(frame, 0);
                    }
                }

                // Switch from babylon to gltf storage representation
                foreach (var frame in mergedFrames)
                {
                    List<float> influences = influencesPerFrame[frame];
                    influences.Add(influencePerFrameForTarget[frame]);
                }
            }

            return influencesPerFrame;
        }

        private string _getTargetPath(string babylonProperty)
        {
            switch (babylonProperty)
            {
                case "position":
                    return "translation";
                case "rotationQuaternion":
                    return "rotation";
                case "scaling":
                    return "scale";
                default:
                    return null;
            }
        }
    }
}
