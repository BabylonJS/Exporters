using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;
using System.Collections.Generic;

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
            
            if (_onlySelected && !selectedNodeFullPaths.Contains(mDagPath.fullPathName))
            {
                return false;
            }
            
            if (!_exportHiddenObjects && !mDagPath.isVisible)
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

        private void GetTransform(MFnTransform mFnTransform, ref float[] position, ref float[] rotationQuaternion, ref float[] rotation, ref float[] scaling)
        {
            var transformationMatrix = new MTransformationMatrix(mFnTransform.transformationMatrix);

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
        }

        private void GetTransform(MFnTransform mFnTransform, ref float[] position)
        {
            var transformationMatrix = new MTransformationMatrix(mFnTransform.transformationMatrix);

            position = transformationMatrix.getTranslation();

            // Switch coordinate system at object level
            position[2] *= -1;
        }
    }
}
