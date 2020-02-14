using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System;
using System.Collections.Generic;
using Utilities;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        /// <summary>
        /// List of full path names of selected nodes
        /// Only kTransform are listed
        /// </summary>
        private List<string> selectedNodeFullPaths;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mFnDagNode">DAG function set of the node below the transform</param>
        /// <param name="mDagPath">DAG path of the transform above the node</param>
        /// <returns></returns>
        private bool IsNodeExportable(MFnDagNode mFnDagNode, MDagPath mDagPath)
        {
            // TODO - Add custom property
            //if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            //{
            //    return false;
            //}
            
            if (exportParameters.exportOnlySelected && !selectedNodeFullPaths.Contains(mDagPath.fullPathName))
            {
                return false;
            }
            
            if (!exportParameters.exportHiddenObjects && !mDagPath.isVisible)
            {
                return false;
            }

            return true;
        }

        private void ExportHierarchy(BabylonNode babylonNode, MFnTransform mFnTransform)
        {
            if (mFnTransform.parentCount != 0)
            {
                RaiseVerbose("BabylonExporter.Node | Hierarchy", 2);

                var mDagPath = new MDagPath(mFnTransform.dagPath);
                mDagPath.pop();

                MObject parentMObject = mDagPath.node;
                // Children of World node don't have parent in Babylon
                if (parentMObject.apiType != MFn.Type.kWorld)
                {
                    MFnDagNode mFnTransformParent = new MFnDagNode(parentMObject);
                    babylonNode.parentId = mFnTransformParent.uuid().asString();
                }
            }
        }

        private void ExportTransform(BabylonNode babylonNode, MFnTransform mFnTransform)
        {
            // Position / rotation / scaling
            RaiseVerbose("BabylonExporter.Node | ExportTransform", 2);
            float[] position = null;
            float[] rotationQuaternion = null;
            float[] rotation = null;
            float[] scaling = null;
            BabylonVector3.EulerRotationOrder rotationOrder = BabylonVector3.EulerRotationOrder.XYZ;
            GetTransform(mFnTransform, ref position, ref rotationQuaternion, ref rotation, ref rotationOrder, ref scaling);

            babylonNode.position = position;
            if (_exportQuaternionsInsteadOfEulers)
            {
                babylonNode.rotationQuaternion = rotationQuaternion;
            }
            else
            {
                babylonNode.rotation = rotation;
            }
            babylonNode.scaling = scaling;
        }

        private void GetTransform(MFnTransform mFnTransform, ref float[] position, ref float[] rotationQuaternion, ref float[] rotation, ref BabylonVector3.EulerRotationOrder rotationOrder, ref float[] scaling)
        {
            var transformationMatrix = new MTransformationMatrix(mFnTransform.transformationMatrix);
            var mayaRotationOrder = 0;
            MGlobal.executeCommand($"getAttr {mFnTransform.fullPathName}.rotateOrder", out mayaRotationOrder);
            rotationOrder = Tools.ConvertMayaRotationOrder((MEulerRotation.RotationOrder)mayaRotationOrder);

            position = transformationMatrix.getTranslation();
            rotationQuaternion = transformationMatrix.getRotationQuaternion();
            rotation = transformationMatrix.getRotation();
            scaling = transformationMatrix.getScale();

            // Switch coordinate system at object level
            position[2] *= -1;
            rotationQuaternion[0] *= -1;
            rotationQuaternion[1] *= -1;
            rotation[0] *= -1;
            rotation[1] *= -1;
            rotationOrder = Tools.InvertRotationOrder(rotationOrder);

            // Apply unit conversion factor to meter
            position[0] *= scaleFactorToMeters;
            position[1] *= scaleFactorToMeters;
            position[2] *= scaleFactorToMeters;
        }
    }
}
