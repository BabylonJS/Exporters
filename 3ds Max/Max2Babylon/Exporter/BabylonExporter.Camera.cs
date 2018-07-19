using Autodesk.Max;
using BabylonExport.Entities;
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
    }
}
