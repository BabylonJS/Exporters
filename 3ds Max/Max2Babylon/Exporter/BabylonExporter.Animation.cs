using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private IList<BabylonAnimationGroup> ExportAnimationGroups(BabylonScene babylonScene)
        {
            IList<BabylonAnimationGroup> animationGroups = new List<BabylonAnimationGroup>();

            // Retrieve and parse animation group data
            AnimationGroupList animationList = AnimationGroupList.InitAnimationGroups(this);

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

                // add animations of each nodes contained in the animGroup
                foreach (Guid guid in animGroup.NodeGuids)
                {
                    IINode maxNode = Tools.GetINodeByGuid(guid);

                    // node could have been deleted, silently ignore it
                    if (maxNode == null)
                        continue;


                    // Helpers can be exported as dummies and as bones
                    string nodeId = guid.ToString();
                    string boneId = guid.ToString()+"-bone";   // the suffix "-bone" is added in babylon export format to assure the uniqueness of IDs


                    // Node
                    BabylonNode node = null;
                    babylonScene.NodeMap.TryGetValue(nodeId, out node);
                    if (node != null)
                    {
                        if (node.animations != null && node.animations.Length != 0)
                        {
                            IList<BabylonAnimation> animations = GetSubAnimations(node, animationGroup.from, animationGroup.to);
                            foreach (BabylonAnimation animation in animations)
                            {
                                BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                {
                                    animation = animation,
                                    targetId = nodeId
                                };

                                animationGroup.targetedAnimations.Add(targetedAnimation);
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

                    // bone
                    BabylonBone bone = null;
                    int index = 0;
                    while (index < babylonScene.SkeletonsList.Count && bone == null)
                    {
                        BabylonSkeleton skel = babylonScene.SkeletonsList[index];
                        bone = skel.bones.FirstOrDefault(b => b.id == boneId);
                        index++;
                    }

                    if (bone != null)
                    {
                        if (bone.animation != null)
                        {
                            IList<BabylonAnimation> animations = GetSubAnimations(bone, animationGroup.from, animationGroup.to);
                            foreach (BabylonAnimation animation in animations)
                            {
                                BabylonTargetedAnimation targetedAnimation = new BabylonTargetedAnimation
                                {
                                    animation = animation,
                                    targetId = boneId
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
                var keys = animation.keysFull = animation.keysFull.FindAll(k => from <= k.frame && k.frame <= to);

                // Optimize these keys
                if (optimizeAnimations)
                {
                    OptimizeAnimations(keys, true);
                }

                // 
                animation.keys = keys.ToArray();
                subAnimations.Add(animation);
            }

            return subAnimations;
        }

        private IList<BabylonAnimation> GetSubAnimations(BabylonBone babylonBone, float from, float to)
        {
            IList<BabylonAnimation> subAnimations = new List<BabylonAnimation>();

            // clone the animation
            BabylonAnimation animation = (BabylonAnimation)babylonBone.animation.Clone();

            // Select usefull keys
            var keys = animation.keysFull = animation.keysFull.FindAll(k => from <= k.frame && k.frame <= to);

            // Optimize these keys
            if (optimizeAnimations)
            {
                OptimizeAnimations(keys, true);
            }

            // 
            animation.keys = keys.ToArray();
            subAnimations.Add(animation);

            return subAnimations;
        }
        private BabylonAnimation CreatePositionAnimation(float from, float to, float[] position)
        {
            BabylonAnimation animation = new BabylonAnimation
            {
                name = "position animation",
                property = "position",
                dataType = 1,
                loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                framePerSecond = Loader.Global.FrameRate,
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
                framePerSecond = Loader.Global.FrameRate,
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


        private static bool ExportBabylonKeys(List<BabylonAnimationKey> keys, string property, List<BabylonAnimation> animations, BabylonAnimation.DataType dataType, BabylonAnimation.LoopBehavior loopBehavior)
        {
            if (keys.Count == 0)
            {
                return false;
            }

            var end = Loader.Core.AnimRange.End;
            if (keys[keys.Count - 1].frame != end / Loader.Global.TicksPerFrame)
            {
                keys.Add(new BabylonAnimationKey()
                {
                    frame = end / Loader.Global.TicksPerFrame,
                    values = keys[keys.Count - 1].values
                });
            }

            var babylonAnimation = new BabylonAnimation
            {
                dataType = (int)dataType,
                name = property + " animation",
                keys = keys.ToArray(),
                framePerSecond = Loader.Global.FrameRate,
                loopBehavior = (int)loopBehavior,
                property = property
            };

            animations.Add(babylonAnimation);

            return true;
        }

        // -----------------------
        // -- From GameControl ---
        // -----------------------

        private bool ExportFloatGameController(IIGameControl control, string property, List<BabylonAnimation> animations)
        {
            return ExportGameController(control, property, animations, IGameControlType.Float, BabylonAnimation.DataType.Float, gameKey => new float[] { gameKey.SampleKey.Fval / 100.0f });
        }

        private bool ExportGameController(IIGameControl control, string property, List<BabylonAnimation> animations, IGameControlType type, BabylonAnimation.DataType dataType, Func<IIGameKey, float[]> extractValueFunc)
        {
            var keys = ExportBabylonKeysFromGameController(control, type, extractValueFunc);

            if (keys == null)
            {
                return false;
            }

            var loopBehavior = BabylonAnimation.LoopBehavior.Cycle;
            return ExportBabylonKeys(keys, property, animations, dataType, loopBehavior);
        }

        private List<BabylonAnimationKey> ExportBabylonKeysFromGameController(IIGameControl control, IGameControlType type, Func<IIGameKey, float[]> extractValueFunc)
        {
            if (control == null)
            {
                return null;
            }

            ITab<IIGameKey> gameKeyTab = GlobalInterface.Instance.Tab.Create<IIGameKey>();
            control.GetQuickSampledKeys(gameKeyTab, type);

            if (gameKeyTab == null)
            {
                return null;
            }

            var keys = new List<BabylonAnimationKey>();
            for (int indexKey = 0; indexKey < gameKeyTab.Count; indexKey++)
            {
#if MAX2017 || MAX2018 || MAX2019 || MAX2020
                var gameKey = gameKeyTab[indexKey];
#else
                var gameKey = gameKeyTab[new IntPtr(indexKey)];
#endif

                var key = new BabylonAnimationKey()
                {
                    frame = gameKey.T / Loader.Global.TicksPerFrame,
                    values = extractValueFunc(gameKey)
                };
                keys.Add(key);
            }

            return keys;
        }

        // -----------------------
        // ---- From Control -----
        // -----------------------

        private static BabylonAnimationKey GenerateFloatFunc(int index, IIKeyControl keyControl)
        {
            var key = Loader.Global.ILinFloatKey.Create();
            keyControl.GetKey(index, key);

            return new BabylonAnimationKey
            {
                frame = key.Time / Loader.Global.TicksPerFrame,
                values = new[] { key.Val }
            };
        }

        private static bool ExportFloatController(IControl control, string property, List<BabylonAnimation> animations)
        {
            return ExportController(control, property, animations, 0x2001, BabylonAnimation.DataType.Float, GenerateFloatFunc);
        }

        private static bool ExportQuaternionController(IControl control, string property, List<BabylonAnimation> animations)
        {
            IQuat previousQuat = null;

            return ExportController(control, property, animations, 0x2003, BabylonAnimation.DataType.Quaternion,
                (index, keyControl) =>
                {
                    var key = Loader.Global.ILinRotKey.Create();
                    keyControl.GetKey(index, key);
                    var newQuat = key.Val;

                    if (index > 0)
                    {
                        newQuat = previousQuat.Multiply(newQuat);
                    }

                    previousQuat = newQuat;

                    return new BabylonAnimationKey
                    {
                        frame = key.Time / Loader.Global.TicksPerFrame,
                        values = newQuat.ToArray()
                    };
                });
        }

        private static bool ExportVector3Controller(IControl control, string property, List<BabylonAnimation> animations)
        {
            var result = false;

            if (control == null)
            {
                return false;
            }

            if (control.XController != null || control.YController != null || control.ZController != null)
            {
                result |= ExportFloatController(control.XController, property + ".x", animations);
                result |= ExportFloatController(control.ZController, property + ".y", animations);
                result |= ExportFloatController(control.YController, property + ".z", animations);

                return result;
            }

            if (ExportController(control, property, animations, 0x2002, BabylonAnimation.DataType.Vector3,
                (index, keyControl) =>
                {
                    var key = Loader.Global.ILinPoint3Key.Create();
                    keyControl.GetKey(index, key);

                    return new BabylonAnimationKey
                    {
                        frame = key.Time / Loader.Global.TicksPerFrame,
                        values = key.Val.ToArraySwitched()
                    };
                }))
            {
                return true;
            }

            return ExportController(control, property, animations, 0x2004, BabylonAnimation.DataType.Vector3,
                (index, keyControl) =>
                {
                    var key = Loader.Global.ILinScaleKey.Create();
                    keyControl.GetKey(index, key);

                    return new BabylonAnimationKey
                    {
                        frame = key.Time / Loader.Global.TicksPerFrame,
                        values = key.Val.S.ToArraySwitched()
                    };
                });
        }

        private static bool ExportController(IControl control, string property, List<BabylonAnimation> animations, uint classId, BabylonAnimation.DataType dataType, Func<int, IIKeyControl, BabylonAnimationKey> generateFunc)
        {
            if (control == null)
            {
                return false;
            }

            var keyControl = control.GetInterface(InterfaceID.Keycontrol) as IIKeyControl;

            if (keyControl == null)
            {
                return false;
            }

            if (control.ClassID.PartA != classId)
            {
                return false;
            }

            BabylonAnimation.LoopBehavior loopBehavior;
            switch (control.GetORT(2))
            {
                case 2:
                    loopBehavior = BabylonAnimation.LoopBehavior.Cycle;
                    break;
                default:
                    loopBehavior = BabylonAnimation.LoopBehavior.Relative;
                    break;
            }

            var keys = new List<BabylonAnimationKey>();
            for (var index = 0; index < keyControl.NumKeys; index++)
            {
                keys.Add(generateFunc(index, keyControl));
            }

            return ExportBabylonKeys(keys, property, animations, dataType, loopBehavior);
        }

        // -----------------------
        // ---- From ext func ----
        // -----------------------

        private void ExportColor3Animation(string property, List<BabylonAnimation> animations,
            Func<int, float[]> extractValueFunc)
        {
            ExportAnimation(property, animations, extractValueFunc, BabylonAnimation.DataType.Color3);
        }

        private void ExportVector3Animation(string property, List<BabylonAnimation> animations,
            Func<int, float[]> extractValueFunc)
        {
            ExportAnimation(property, animations, extractValueFunc, BabylonAnimation.DataType.Vector3);
        }

        private void ExportQuaternionAnimation(string property, List<BabylonAnimation> animations,
            Func<int, float[]> extractValueFunc)
        {
            ExportAnimation(property, animations, extractValueFunc, BabylonAnimation.DataType.Quaternion);
        }

        private void ExportFloatAnimation(string property, List<BabylonAnimation> animations,
            Func<int, float[]> extractValueFunc)
        {
            ExportAnimation(property, animations, extractValueFunc, BabylonAnimation.DataType.Float);
        }

        private BabylonAnimation ExportMatrixAnimation(string property, Func<int, float[]> extractValueFunc, bool removeLinearAnimationKeys = true)
        {
            return ExportAnimation(property, extractValueFunc, BabylonAnimation.DataType.Matrix, removeLinearAnimationKeys);
        }

        private void OptimizeAnimations(List<BabylonAnimationKey> keys, bool removeLinearAnimationKeys)
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

        private float[] weightedLerp(int frame0, int frame1, int frame2, float[] value0, float[] value2)
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

        private bool RemoveAnimationKey(List<BabylonAnimationKey> keys, int ixFirst, bool removeLinearAnimationKeys)
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

        private void ExportAnimation(string property, List<BabylonAnimation> animations, Func<int, float[]> extractValueFunc, BabylonAnimation.DataType dataType, bool removeLinearAnimationKeys = true)
        {
            var babylonAnimation = ExportAnimation(property, extractValueFunc, dataType, removeLinearAnimationKeys);
            if (babylonAnimation != null)
            {
                animations.Add(babylonAnimation);
            }
        }

        private BabylonAnimation ExportAnimation(string property, Func<int, float[]> extractValueFunc, BabylonAnimation.DataType dataType, bool removeLinearAnimationKeys = true)
        {
            var optimizeAnimations = !Loader.Core.RootNode.GetBoolProperty("babylonjs_donotoptimizeanimations"); // reverse negation for clarity

            var start = Loader.Core.AnimRange.Start;
            var end = Loader.Core.AnimRange.End;

            float[] previous = null;
            var keys = new List<BabylonAnimationKey>();
            for (var key = start; key <= end; key += Loader.Global.TicksPerFrame)
            {
                var current = extractValueFunc(key);

                keys.Add(new BabylonAnimationKey()
                {
                    frame = key / Loader.Global.TicksPerFrame,
                    values = current
                });

                previous = current;
            }
            var keysFull = new List<BabylonAnimationKey>(keys);

            // Optimization process always keeps first and last frames
            if (optimizeAnimations)
            {
                OptimizeAnimations(keys, removeLinearAnimationKeys);
            }

            if (IsAnimationKeysRelevant(keys))
            {
                if (keys[keys.Count - 1].frame != end / Loader.Global.TicksPerFrame)
                {
                    keys.Add(new BabylonAnimationKey()
                    {
                        frame = end / Loader.Global.TicksPerFrame,
                        values = (float[])keys[keys.Count - 1].values.Clone()
                    });
                }

                var babylonAnimation = new BabylonAnimation
                {
                    dataType = (int)dataType,
                    name = property + " animation",
                    keys = keys.ToArray(),
                    keysFull = keysFull,
                    framePerSecond = Loader.Global.FrameRate,
                    loopBehavior = (int)BabylonAnimation.LoopBehavior.Cycle,
                    property = property
                };
                return babylonAnimation;
            }
            return null;
        }

        private bool IsAnimationKeysRelevant(List<BabylonAnimationKey> keys)
        {
            if (keys.Count > 1)
            {
                if (keys.Count == 2)
                {
                    if (keys[0].values.IsEqualTo(keys[1].values))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public void GeneratePositionAnimation(IIGameNode gameNode, List<BabylonAnimation> animations)
        {
            if (gameNode.IGameControl.IsAnimated(IGameControlType.Pos) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.PosX) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.PosY) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.PosZ))
            {
                ExportVector3Animation("position", animations, key =>
                {
                    var localMatrix = gameNode.GetLocalTM(key);

                    if (float.IsNaN(localMatrix.Determinant))
                    {
                        RaiseError($"Determinant of {gameNode.Name} of position animation at {key} localMatrix is NaN ");
                    }

                    var tm_babylon = new BabylonMatrix();
                    tm_babylon.m = localMatrix.ToArray();

                    var s_babylon = new BabylonVector3();
                    var q_babylon = new BabylonQuaternion();
                    var t_babylon = new BabylonVector3();

                    tm_babylon.decompose(s_babylon, q_babylon, t_babylon);

                    return new[] { t_babylon.X, t_babylon.Y, t_babylon.Z };
                });
            }
        }

        public void GenerateRotationAnimation(IIGameNode gameNode, List<BabylonAnimation> animations, bool force = false)
        {
            if (gameNode.IGameControl.IsAnimated(IGameControlType.Rot) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.EulerX) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.EulerY) ||
                gameNode.IGameControl.IsAnimated(IGameControlType.EulerZ) ||
                (gameNode.IGameObject.IGameType == Autodesk.Max.IGameObject.ObjectTypes.Light && gameNode.IGameObject.AsGameLight().LightTarget != null) || // Light with target are indirectly animated by their target
                force)
            {
                ExportQuaternionAnimation("rotationQuaternion", animations, key =>
                {
                    var localMatrix = gameNode.GetLocalTM(key);

                    if (float.IsNaN(localMatrix.Determinant))
                    {
                        RaiseError($"Determinant of {gameNode.Name} of rotation animation at {key} localMatrix is NaN ");
                    }

                    var tm_babylon = new BabylonMatrix();
                    tm_babylon.m = localMatrix.ToArray();

                    var s_babylon = new BabylonVector3();
                    var q_babylon = new BabylonQuaternion();
                    var t_babylon = new BabylonVector3();

                    tm_babylon.decompose(s_babylon, q_babylon, t_babylon);

                    // normalize
                    var q = q_babylon;
                    float q_length = (float)Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W);

                    return new[] { q_babylon.X / q_length, q_babylon.Y / q_length, q_babylon.Z / q_length, q_babylon.W / q_length };
                });
            }
        }

        public void GenerateScalingAnimation(IIGameNode gameNode, List<BabylonAnimation> animations)
        {
            if (gameNode.IGameControl.IsAnimated(IGameControlType.Scale))
            {
                ExportVector3Animation("scaling", animations, key =>
                {
                    var localMatrix = gameNode.GetLocalTM(key);

                    if (float.IsNaN(localMatrix.Determinant))
                    {
                        RaiseError($"Determinant of {gameNode.Name} of scale animation at {key} localMatrix is NaN ");
                    }

                    var tm_babylon = new BabylonMatrix();
                    tm_babylon.m = localMatrix.ToArray();

                    var s_babylon = new BabylonVector3();
                    var q_babylon = new BabylonQuaternion();
                    var t_babylon = new BabylonVector3();

                    tm_babylon.decompose(s_babylon, q_babylon, t_babylon);

                    return new[] { s_babylon.X, s_babylon.Y, s_babylon.Z };
                });
            }
        }
    }
}
