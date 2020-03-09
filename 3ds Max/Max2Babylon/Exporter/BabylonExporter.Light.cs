using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;
using System.Linq;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        void ExportDefaultLight(BabylonScene babylonScene)
        {
            var babylonLight = new BabylonLight();
            babylonLight.name = "Default light";
            babylonLight.id = Guid.NewGuid().ToString();
            babylonLight.type = 3;
            babylonLight.groundColor = new float[] { 0, 0, 0 };
            babylonLight.direction = new[] { 0, 1.0f, 0 };

            babylonLight.intensity = 1;

            babylonLight.diffuse = new[] { 1.0f, 1.0f, 1.0f };
            babylonLight.specular = new[] { 1.0f, 1.0f, 1.0f };

            babylonScene.LightsList.Add(babylonLight);
        }

        private bool IsLightExportable(IIGameNode lightNode)
        {
            if (IsNodeExportable(lightNode) == false)
            {
                return false;
            }

            if (exportParameters.exportAnimationsOnly && lightNode.IGameControl != null && !isAnimated(lightNode))
            {
                var gameLight = lightNode.IGameObject.AsGameLight();
                var initialized = gameLight.InitializeData;

                if (gameLight.LightTarget != null)
                {
                    return IsNodeExportable(gameLight.LightTarget);
                }

                return false;
            }

            return true;
        }

        private BabylonNode ExportLight(IIGameScene scene, IIGameNode lightNode, BabylonScene babylonScene)
        {
            if (IsLightExportable(lightNode) == false)
            {
                return null;
            }

            var gameLight = lightNode.IGameObject.AsGameLight();
            var initialized = gameLight.InitializeData;
            var babylonLight = new BabylonLight();

            RaiseMessage(lightNode.Name, 1);
            babylonLight.name = lightNode.Name;

            // Export the custom attributes of this light
            babylonLight.metadata = ExportExtraAttributes(lightNode, babylonScene);

            // To preserve the position/rotation and the hierarchy, we create a dummy that will contains as direct children the light and the light children
            // The light will have no children. The dummy will contains the position and rotation animations.
            bool createDummy = lightNode.ChildCount > 0;
            BabylonNode dummy = null;
            if (createDummy)
            {
                dummy = ExportDummy(scene, lightNode, babylonScene);
                dummy.name = "_" + dummy.name + "_";
                babylonLight.id = Guid.NewGuid().ToString();
                babylonLight.parentId = dummy.id;
                babylonLight.hasDummy = true;
            }
            else
            {
                babylonLight.id = lightNode.MaxNode.GetGuid().ToString();
                if (lightNode.NodeParent != null)
                {
                    babylonLight.parentId = lightNode.NodeParent.MaxNode.GetGuid().ToString();
                }
            }

            // Type
            var maxLight = (lightNode.MaxNode.ObjectRef as ILightObject);
            var lightState = Loader.Global.LightState.Create();
            maxLight.EvalLightState(0, Tools.Forever, lightState);

            switch (lightState.Type)
            {
                case LightType.OmniLgt:
                    babylonLight.type = 0;
                    break;
                case LightType.SpotLgt:
                    babylonLight.type = 2;
                    babylonLight.angle = (float)(maxLight.GetFallsize(0, Tools.Forever) * Math.PI / 180.0f);
                    babylonLight.exponent = 1;
                    break;
                case LightType.DirectLgt:
                    babylonLight.type = 1;
                    break;
                case LightType.AmbientLgt:
                    babylonLight.type = 3;
                    babylonLight.groundColor = new float[] { 0, 0, 0 };
                    break;
            }


            // Shadows 
            if (maxLight.ShadowMethod == 1)
            {
                if (lightState.Type == LightType.DirectLgt || lightState.Type == LightType.SpotLgt || lightState.Type == LightType.OmniLgt)
                {
                    ExportShadowGenerator(lightNode.MaxNode, babylonScene, babylonLight);
                }
                else
                {
                    RaiseWarning("Shadows maps are only supported for point, directional and spot lights", 2);
                }
            }

            // Position / rotation / scaling
            if (createDummy)
            {
                // The position is stored by the dummy parent and the default direction is downward and it is updated by the rotation of the parent dummy
                babylonLight.position = new[] { 0f, 0f, 0f };
                babylonLight.direction = new[] { 0f, -1f, 0f };
            }
            else
            {
                exportTransform(babylonLight, lightNode);
                
                // Position
                var localMatrix = lightNode.GetLocalTM(0);
                var position = localMatrix.Translation;

                // Direction
                var target = gameLight.LightTarget;
                if (target != null)
                {
                    var targetWm = target.GetObjectTM(0);
                    var targetPosition = targetWm.Translation;

                    var direction = targetPosition.Subtract(position).Normalize;
                    babylonLight.direction = new[] { direction.X, direction.Y, direction.Z };
                }
                else
                {
                    var vDir = Loader.Global.Point3.Create(0, -1, 0);
                    vDir = localMatrix.ExtractMatrix3().VectorTransform(vDir).Normalize;
                    babylonLight.direction = new[] { vDir.X, vDir.Y, vDir.Z };
                }
            }

            // The HemisphericLight simulates the ambient environment light, so the passed direction is the light reflection direction, not the incoming direction.
            // So we need the opposite direction
            if (babylonLight.type == 3)
            {
                var worldRotation = lightNode.GetWorldTM(0).Rotation;
                BabylonQuaternion quaternion = new BabylonQuaternion(worldRotation.X, worldRotation.Y, worldRotation.Z, worldRotation.W);

                babylonLight.direction = quaternion.Rotate(new BabylonVector3(0f, 1f, 0f)).ToArray();
            }


            var maxScene = Loader.Core.RootNode;

            // Exclusion
            try
            {
                var inclusion = maxLight.ExclList.TestFlag(1); //NT_INCLUDE 
                var checkExclusionList = maxLight.ExclList.TestFlag(2); //NT_AFFECT_ILLUM

                if (checkExclusionList)
                {
                    var excllist = new List<string>();
                    var incllist = new List<string>();

                    foreach (var meshNode in maxScene.NodesListBySuperClass(SClass_ID.Geomobject))
                    {
#if MAX2017 || MAX2018 || MAX2019 || MAX2020
                        if (meshNode.CastShadows)
#else
                        if (meshNode.CastShadows == 1)
#endif
                        {
                            var inList = maxLight.ExclList.FindNode(meshNode) != -1;

                            if (inList)
                            {
                                if (inclusion)
                                {
                                    incllist.Add(meshNode.GetGuid().ToString());
                                }
                                else
                                {
                                    excllist.Add(meshNode.GetGuid().ToString());
                                }
                            }
                        }
                    }

                    babylonLight.includedOnlyMeshesIds = incllist.ToArray();
                    babylonLight.excludedMeshesIds = excllist.ToArray();
                }
            }
            catch (Exception e)
            {
                RaiseMessage("Light exclusion not supported", 2);
            }

            // Other fields 
            babylonLight.intensity = maxLight.GetIntensity(0, Tools.Forever);


            babylonLight.diffuse = lightState.AffectDiffuse ? maxLight.GetRGBColor(0, Tools.Forever).ToArray() : new float[] { 0, 0, 0 };
            babylonLight.specular = lightState.AffectDiffuse ? maxLight.GetRGBColor(0, Tools.Forever).ToArray() : new float[] { 0, 0, 0 };


            if (maxLight.UseAtten)
            {
                babylonLight.range = maxLight.GetAtten(0, 3, Tools.Forever);
            }

            if (exportParameters.exportAnimations)
            {
                // Animations
                var animations = new List<BabylonAnimation>();

                if(createDummy)
                {
                    // Position and rotation animations are stored by the parent (the dummy). The direction result from the parent rotation except for the HemisphericLight.
                    if (babylonLight.type == 3)
                    {
                        BabylonVector3 direction = new BabylonVector3(0, 1, 0);
                        ExportVector3Animation("direction", animations, key =>
                        {
                            var worldRotation = lightNode.GetWorldTM(key).Rotation;
                            BabylonQuaternion quaternion = new BabylonQuaternion(worldRotation.X, worldRotation.Y, worldRotation.Z, worldRotation.W);

                            return quaternion.Rotate(direction).ToArray();
                        });
                    }
                }
                else
                {
                    GeneratePositionAnimation(lightNode, animations);

                    ExportVector3Animation("direction", animations, key =>
                    {
                        var localMatrixAnimDir = lightNode.GetLocalTM(key);

                        var positionLight = localMatrixAnimDir.Translation;
                        var lightTarget = gameLight.LightTarget;
                        if (lightTarget != null)
                        {
                            var targetWm = lightTarget.GetObjectTM(key);
                            var targetPosition = targetWm.Translation;

                            var direction = targetPosition.Subtract(positionLight).Normalize;
                            return new[] { direction.X, direction.Y, direction.Z };
                        }
                        else
                        {
                            var vDir = Loader.Global.Point3.Create(0, -1, 0);
                            vDir = localMatrixAnimDir.ExtractMatrix3().VectorTransform(vDir).Normalize;

                            // The HemisphericLight (type == 3) simulates the ambient environment light, so the passed direction is the light reflection direction, not the incoming direction.
                            // So we need the opposite direction
                            return babylonLight.type != 3 ? new[] { vDir.X, vDir.Y, vDir.Z } : new[] { -vDir.X, -vDir.Y, -vDir.Z };
                        }
                    });

                    // Animation temporary stored for gltf but not exported for babylon
                    // TODO - Will cause an issue when externalizing the glTF export process
                    var extraAnimations = new List<BabylonAnimation>();
                    // Do not check if node rotation properties are animated
                    GenerateRotationAnimation(lightNode, extraAnimations, true);
                    babylonLight.extraAnimations = extraAnimations;
                }

                ExportFloatAnimation("intensity", animations, key => new[] { maxLight.GetIntensity(key, Tools.Forever) });

                ExportColor3Animation("diffuse", animations, key =>
                {
                    return lightState.AffectDiffuse? maxLight.GetRGBColor(key, Tools.Forever).ToArray() : new float[] { 0, 0, 0 };
                });

                babylonLight.animations = animations.ToArray();

                if (lightNode.MaxNode.GetBoolProperty("babylonjs_autoanimate"))
                {
                    babylonLight.autoAnimate = true;
                    babylonLight.autoAnimateFrom = (int)lightNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_from");
                    babylonLight.autoAnimateTo = (int)lightNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_to");
                    babylonLight.autoAnimateLoop = lightNode.MaxNode.GetBoolProperty("babylonjs_autoanimateloop");
                }
            }

            babylonScene.LightsList.Add(babylonLight);

            return createDummy ? dummy : babylonLight;
        }
    }
}
