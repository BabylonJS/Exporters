using Autodesk.Max;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private bool IsCameraExportable(IIGameNode cameraNode)
        {
            return IsNodeExportable(cameraNode);
        }

        private BabylonCamera ExportCamera(IIGameScene scene, IIGameNode cameraNode, BabylonScene babylonScene)
        {
            if (IsCameraExportable(cameraNode) == false)
            {
                return null;
            }

            var gameCamera = cameraNode.IGameObject.AsGameCamera();
            var maxCamera = gameCamera.MaxObject as ICameraObject;
            var initialized = gameCamera.InitializeData;
            var babylonCamera = new BabylonCamera();

            RaiseMessage(cameraNode.Name, 1);
            babylonCamera.name = cameraNode.Name;
            babylonCamera.id = cameraNode.MaxNode.GetGuid().ToString();
            if (cameraNode.NodeParent != null)
            {
                babylonCamera.parentId = cameraNode.NodeParent.MaxNode.GetGuid().ToString();
            }

            babylonCamera.fov = Tools.ConvertFov(maxCamera.GetFOV(0, Tools.Forever));

            if (maxCamera.ManualClip == 1)
            {
                babylonCamera.minZ = maxCamera.GetClipDist(0, 1, Tools.Forever);
                babylonCamera.maxZ = maxCamera.GetClipDist(0, 2, Tools.Forever);
            }
            else
            {
                babylonCamera.minZ = 0.1f;
                babylonCamera.maxZ = 10000.0f;
            }

            if (babylonCamera.minZ == 0.0f)
            {
                babylonCamera.minZ = 0.1f;
            }

            // Type
            babylonCamera.type = cameraNode.MaxNode.GetStringProperty("babylonjs_type", "FreeCamera");

            // Control
            babylonCamera.speed = cameraNode.MaxNode.GetFloatProperty("babylonjs_speed", 1.0f);
            babylonCamera.inertia = cameraNode.MaxNode.GetFloatProperty("babylonjs_inertia", 0.9f);

            // Collisions
            babylonCamera.checkCollisions = cameraNode.MaxNode.GetBoolProperty("babylonjs_checkcollisions");
            babylonCamera.applyGravity = cameraNode.MaxNode.GetBoolProperty("babylonjs_applygravity");
            babylonCamera.ellipsoid = cameraNode.MaxNode.GetVector3Property("babylonjs_ellipsoid");

            // Position / rotation
            exportTransform(babylonCamera, cameraNode);

            // Target
            var target = gameCamera.CameraTarget;
            if (target != null)
            {
                babylonCamera.lockedTargetId = target.MaxNode.GetGuid().ToString();
            }

            // Animations
            var animations = new List<BabylonAnimation>();

            GeneratePositionAnimation(cameraNode, animations);

            if (target == null)
            {
                // Export rotation animation
                GenerateRotationAnimation(cameraNode, animations);
            }
            else
            {
                // Animation temporary stored for gltf but not exported for babylon
                // TODO - Will cause an issue when externalizing the glTF export process
                var extraAnimations = new List<BabylonAnimation>();
                // Do not check if node rotation properties are animated
                GenerateRotationAnimation(cameraNode, extraAnimations, true);
                babylonCamera.extraAnimations = extraAnimations;
            }

            ExportFloatAnimation("fov", animations, key => new[] { Tools.ConvertFov((gameCamera.MaxObject as ICameraObject).GetFOV(key, Tools.Forever)) });

            babylonCamera.animations = animations.ToArray();

            if (cameraNode.MaxNode.GetBoolProperty("babylonjs_autoanimate"))
            {
                babylonCamera.autoAnimate = true;
                babylonCamera.autoAnimateFrom = (int)cameraNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_from");
                babylonCamera.autoAnimateTo = (int)cameraNode.MaxNode.GetFloatProperty("babylonjs_autoanimate_to");
                babylonCamera.autoAnimateLoop = cameraNode.MaxNode.GetBoolProperty("babylonjs_autoanimateloop");
            }

            babylonScene.CamerasList.Add(babylonCamera);

            return babylonCamera;
        }




        /// <summary>
        /// In 3DS Max the default camera look down (in the -z direction for the 3DS Max reference (+y for babylon))
        /// In Babylon the default camera look to the horizon (in the +z direction for the babylon reference)
        /// So to correct this difference, this function apply a rotation to the camera and its first children.
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="babylonScene">Use the exported babylon scene to get the final hierarchy</param>
        private void FixCamera(ref BabylonCamera camera, ref BabylonScene babylonScene)
        {
            string id = camera.id;
            IList<BabylonMesh> meshes = babylonScene.MeshesList.FindAll(mesh => mesh.parentId == null ? false : mesh.parentId.Equals(id));

            RaiseMessage($"{camera.name}", 2);

            if (camera.target == null)
            {
                // fix the vue
                // Rotation around the axis X of PI / 2 in the indirect direction
                double angle = Math.PI / 2;
                if (camera.rotation != null)
                {
                    camera.rotation[0] += (float)angle;
                }
                if (camera.rotationQuaternion != null)
                {
                    BabylonQuaternion rotationQuaternion = FixCameraQuaternion(camera, angle);

                    camera.rotationQuaternion = rotationQuaternion.ToArray();
                    camera.rotation = rotationQuaternion.toEulerAngles().ToArray();
                }

                // animation
                List<BabylonAnimation> animations = new List<BabylonAnimation>(camera.animations);
                BabylonAnimation animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                if (animationRotationQuaternion != null)
                {
                    foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                    {
                        key.values = FixCameraQuaternion(key.values, angle);
                    }
                }
                else   // if the camera has a lockedTargetId, it is the extraAnimations that stores the rotation animation
                {
                    if (camera.extraAnimations != null)
                    {
                        List<BabylonAnimation> extraAnimations = new List<BabylonAnimation>(camera.extraAnimations);
                        animationRotationQuaternion = extraAnimations.Find(animation => animation.property.Equals("rotationQuaternion"));
                        if (animationRotationQuaternion != null)
                        {
                            foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                            {
                                key.values = FixCameraQuaternion(key.values, angle);
                            }
                        }
                    }
                }

                // fix direct children
                // Rotation around the axis X of -PI / 2 in the direct direction
                angle = -Math.PI / 2;
                foreach (var mesh in meshes)
                {
                    RaiseVerbose($"{mesh.name}", 3);
                    mesh.position = new float[] { mesh.position[0], mesh.position[2], -mesh.position[1] };

                    // Add a rotation of PI/2 axis X in direct direction
                    if (mesh.rotationQuaternion != null)
                    {
                        // Rotation around the axis X of -PI / 2 in the direct direction
                        BabylonQuaternion quaternion = FixChildQuaternion(mesh, angle);

                        mesh.rotationQuaternion = quaternion.ToArray();
                    }
                    if (mesh.rotation != null)
                    {
                        mesh.rotation[0] += (float)angle;
                    }


                    // Animations
                    animations = new List<BabylonAnimation>(mesh.animations);
                    // Position
                    BabylonAnimation animationPosition = animations.Find(animation => animation.property.Equals("position"));
                    if (animationPosition != null)
                    {
                        foreach (BabylonAnimationKey key in animationPosition.keys)
                        {
                            key.values = new float[] { key.values[0], key.values[2], -key.values[1] };
                        }
                    }

                    // Rotation
                    animationRotationQuaternion = animations.Find(animation => animation.property.Equals("rotationQuaternion"));
                    if (animationRotationQuaternion != null)
                    {
                        foreach (BabylonAnimationKey key in animationRotationQuaternion.keys)
                        {
                            key.values = FixChildQuaternion(key.values, angle);
                        }
                    }
                }
            }
        }



        private BabylonQuaternion FixCameraQuaternion(BabylonNode node, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle / 2), 0, 0, (float)Math.Cos(angle / 2));
            BabylonQuaternion quaternion = new BabylonQuaternion(node.rotationQuaternion[0], node.rotationQuaternion[1], node.rotationQuaternion[2], node.rotationQuaternion[3]);
            BabylonQuaternion rotationQuaternion = quaternion.MultiplyWith(qFix);

            return rotationQuaternion;
        }

        private float[] FixCameraQuaternion(float[] q, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle / 2), 0, 0, (float)Math.Cos(angle / 2));
            BabylonQuaternion quaternion = new BabylonQuaternion(q[0], q[1], q[2], q[3]);
            BabylonQuaternion rotationQuaternion = quaternion.MultiplyWith(qFix);

            return rotationQuaternion.ToArray();
        }

        private BabylonQuaternion FixChildQuaternion(BabylonNode node, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle / 2), 0, 0, (float)Math.Cos(angle / 2));
            BabylonQuaternion quaternion = new BabylonQuaternion(node.rotationQuaternion[0], node.rotationQuaternion[1], node.rotationQuaternion[2], node.rotationQuaternion[3]);
            BabylonQuaternion rotationQuaternion = qFix.MultiplyWith(quaternion);

            return rotationQuaternion;
        }

        private float[] FixChildQuaternion(float[] q, double angle)
        {
            BabylonQuaternion qFix = new BabylonQuaternion((float)Math.Sin(angle / 2), 0, 0, (float)Math.Cos(angle / 2));
            BabylonQuaternion quaternion = new BabylonQuaternion(q[0], q[1], q[2], q[3]);
            BabylonQuaternion rotationQuaternion = qFix.MultiplyWith(quaternion);

            return rotationQuaternion.ToArray();
        }

    }
}
