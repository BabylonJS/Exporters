using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mDagPath">DAG path to the transform above camera</param>
        /// <param name="babylonScene"></param>
        /// <returns></returns>
        private BabylonNode ExportCamera(MDagPath mDagPath, BabylonScene babylonScene)
        {
            RaiseMessage(mDagPath.partialPathName, 1);

            // Transform above camera
            MFnTransform mFnTransform = new MFnTransform(mDagPath);

            // Camera direct child of the transform
            MFnCamera mFnCamera = null;
            for (uint i = 0; i < mFnTransform.childCount; i++)
            {
                MObject childObject = mFnTransform.child(i);
                if (childObject.apiType == MFn.Type.kCamera)
                {
                    var _mFnCamera = new MFnCamera(childObject);
                    if (!_mFnCamera.isIntermediateObject)
                    {
                        mFnCamera = _mFnCamera;
                    }
                }
            }
            if (mFnCamera == null)
            {
                RaiseError("No camera found has child of " + mDagPath.fullPathName);
                return null;
            }


            // --- prints ---
            #region prints

            RaiseVerbose("BabylonExporter.Camera | mFnCamera data", 2);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.eyePoint(MSpace.Space.kWorld).toString()=" + mFnCamera.eyePoint(MSpace.Space.kTransform).toString(), 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.viewDirection(MSpace.Space.kWorld).toString()=" + mFnCamera.viewDirection(MSpace.Space.kTransform).toString(), 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.upDirection(MSpace.Space.kWorld).toString()=" + mFnCamera.upDirection(MSpace.Space.kTransform).toString(), 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.rightDirection(MSpace.Space.kWorld).toString()=" + mFnCamera.rightDirection(MSpace.Space.kTransform).toString(), 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.centerOfInterestPoint(MSpace.Space.kWorld).toString()=" + mFnCamera.centerOfInterestPoint(MSpace.Space.kTransform).toString(), 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.centerOfInterest=" + mFnCamera.centerOfInterest, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.aspectRatio=" + mFnCamera.aspectRatio, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.horizontalFieldOfView=" + mFnCamera.horizontalFieldOfView, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.verticalFieldOfView=" + mFnCamera.verticalFieldOfView, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.horizontalFieldOfView / mFnCamera.verticalFieldOfView=" + mFnCamera.horizontalFieldOfView / mFnCamera.verticalFieldOfView, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.focalLength=" + mFnCamera.focalLength, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.nearFocusDistance=" + mFnCamera.nearFocusDistance, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.nearClippingPlane=" + mFnCamera.nearClippingPlane, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.unnormalizedNearClippingPlane=" + mFnCamera.unnormalizedNearClippingPlane, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.farFocusDistance=" + mFnCamera.farFocusDistance, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.farClippingPlane=" + mFnCamera.farClippingPlane, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.unnormalizedFarClippingPlane=" + mFnCamera.unnormalizedFarClippingPlane, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.isClippingPlanes=" + mFnCamera.isClippingPlanes, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.isIntermediateObject=" + mFnCamera.isIntermediateObject, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.focusDistance=" + mFnCamera.focusDistance, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.isStereo=" + mFnCamera.isStereo, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.eyeOffset=" + mFnCamera.eyeOffset, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.shutterAngle=" + mFnCamera.shutterAngle, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.isDepthOfField=" + mFnCamera.isDepthOfField, 3);
            RaiseVerbose("BabylonExporter.Camera | mFnCamera.renderPanZoom=" + mFnCamera.renderPanZoom, 3);

            #endregion

            if (IsCameraExportable(mFnCamera, mDagPath) == false)
            {
                return null;
            }
            
            var babylonCamera = new BabylonCamera { name = mFnTransform.name, id = mFnTransform.uuid().asString() };

            // Hierarchy
            ExportHierarchy(babylonCamera, mFnTransform);

            // Position / rotation
            RaiseVerbose("BabylonExporter.Camera | ExportTransform", 2);
            float[] position = null;
            float[] rotationQuaternion = null;
            float[] rotation = null;
            float[] scaling = null;
            GetTransform(mFnTransform, ref position, ref rotationQuaternion, ref rotation, ref scaling);
            babylonCamera.position = position;
            if (_exportQuaternionsInsteadOfEulers)
            {
                babylonCamera.rotationQuaternion = rotationQuaternion;
            }
            babylonCamera.rotation = rotation;

            // Field of view of babylon is the vertical one
            babylonCamera.fov = (float)mFnCamera.verticalFieldOfView;

            // Clipping planes
            babylonCamera.minZ = (float)mFnCamera.nearClippingPlane;
            babylonCamera.maxZ = (float)mFnCamera.farClippingPlane;
            // Constraints on near clipping plane
            if (babylonCamera.minZ == 0.0f)
            {
                babylonCamera.minZ = 0.1f;
            }

            // TODO - Retreive from Maya
            //// Type
            //babylonCamera.type = cameraNode.MaxNode.GetStringProperty("babylonjs_type", "FreeCamera");

            //// Control
            //babylonCamera.speed = cameraNode.MaxNode.GetFloatProperty("babylonjs_speed", 1.0f);
            //babylonCamera.inertia = cameraNode.MaxNode.GetFloatProperty("babylonjs_inertia", 0.9f);

            //// Collisions
            //babylonCamera.checkCollisions = cameraNode.MaxNode.GetBoolProperty("babylonjs_checkcollisions");
            //babylonCamera.applyGravity = cameraNode.MaxNode.GetBoolProperty("babylonjs_applygravity");
            //babylonCamera.ellipsoid = cameraNode.MaxNode.GetVector3Property("babylonjs_ellipsoid");

            // TODO - Target
            //var target = mFnCamera.target;
            //if (target != null)
            //{
            //    babylonCamera.lockedTargetId = target.MaxNode.GetGuid().ToString();
            //}

            //// TODO - Check if should be local or world
            //var vDir = new MVector(0, 0, -1);
            //var transformationMatrix = new MTransformationMatrix(mFnTransform.transformationMatrix);
            //vDir *= transformationMatrix.asMatrix(1);
            //vDir = vDir.Add(position);
            //babylonCamera.target = new[] { vDir.X, vDir.Y, vDir.Z };

            // TODO - Animations

            babylonScene.CamerasList.Add(babylonCamera);
            RaiseMessage("BabylonExporter.Camera | done", 2);

            return babylonCamera;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mFnDagNode">DAG function set of the node (camera) below the transform</param>
        /// <param name="mDagPath">DAG path of the transform above the node</param>
        /// <returns></returns>
        private bool IsCameraExportable(MFnDagNode mFnDagNode, MDagPath mDagPath)
        {
            return IsNodeExportable(mFnDagNode, mDagPath);
        }
    }
}
