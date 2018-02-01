using Autodesk.Maya.OpenMaya;
using BabylonExport.Entities;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        private bool IsNodeExportable(MFnDagNode mFnDagNode, MDagPath mDagPath)
        {
            // TODO - Add custom property
            //if (gameNode.MaxNode.GetBoolProperty("babylonjs_noexport"))
            //{
            //    return false;
            //}

            // TODO - Fix fatal error: Attempting to save in C:/Users/Fabrice/AppData/Local/Temp/Fabrice.20171205.1613.ma
            //if (_onlySelected && !MGlobal.isSelected(mDagPath.node))
            //{
            //    return false;
            //}

            // TODO - Fix fatal error: Attempting to save in C:/ Users / Fabrice / AppData / Local / Temp / Fabrice.20171205.1613.ma
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

                MObject parentMObject = mFnTransform.parent(0);
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
    }
}
