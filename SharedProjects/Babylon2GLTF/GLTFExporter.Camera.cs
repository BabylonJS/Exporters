using BabylonExport.Entities;
using GLTFExport.Entities;
using System.Collections.Generic;

namespace Babylon2GLTF
{
    partial class GLTFExporter
    {
        private GLTFCamera ExportCamera(ref GLTFNode gltfNode, BabylonCamera babylonCamera, GLTF gltf, GLTFNode gltfParentNode)
        {
            logger.RaiseMessage("GLTFExporter.Camera | Export camera named: " + babylonCamera.name, 2);

            // --- prints ---
            #region prints

            logger.RaiseVerbose("GLTFExporter.Camera | babylonCamera data", 3);
            logger.RaiseVerbose("GLTFExporter.Camera | babylonCamera.type=" + babylonCamera.type, 4);
            logger.RaiseVerbose("GLTFExporter.Camera | babylonCamera.fov=" + babylonCamera.fov, 4);
            logger.RaiseVerbose("GLTFExporter.Camera | babylonCamera.maxZ=" + babylonCamera.maxZ, 4);
            logger.RaiseVerbose("GLTFExporter.Camera | babylonCamera.minZ=" + babylonCamera.minZ, 4);
            #endregion


            // --------------------------
            // ------- gltfCamera -------
            // --------------------------

            logger.RaiseMessage("GLTFExporter.Camera | create gltfCamera", 3);

            // Camera
            var gltfCamera = new GLTFCamera { name = babylonCamera.name };
            gltfCamera.index = gltf.CamerasList.Count;
            gltf.CamerasList.Add(gltfCamera);
            gltfNode.camera = gltfCamera.index;
            gltfCamera.gltfNode = gltfNode;

            // Custom user properties
            if(babylonCamera.metadata != null && babylonCamera.metadata.Count != 0)
            {
                gltfCamera.extras = babylonCamera.metadata;
            }

            // Camera type
            switch (babylonCamera.mode)
            {
                case (BabylonCamera.CameraMode.ORTHOGRAPHIC_CAMERA):
                    var gltfCameraOrthographic = new GLTFCameraOrthographic();
                    gltfCameraOrthographic.xmag = 1; // Do not bother about it - still mandatory
                    gltfCameraOrthographic.ymag = 1; // Do not bother about it - still mandatory
                    gltfCameraOrthographic.zfar = babylonCamera.maxZ;
                    gltfCameraOrthographic.znear = babylonCamera.minZ;

                    gltfCamera.type = GLTFCamera.CameraType.orthographic.ToString();
                    gltfCamera.orthographic = gltfCameraOrthographic;
                    break;
                case (BabylonCamera.CameraMode.PERSPECTIVE_CAMERA):
                    var gltfCameraPerspective = new GLTFCameraPerspective();
                    gltfCameraPerspective.aspectRatio = null; // Do not bother about it - use default glTF value
                    gltfCameraPerspective.yfov = babylonCamera.fov; // Babylon camera fov mode is assumed to be vertical (FOVMODE_VERTICAL_FIXED)
                    gltfCameraPerspective.zfar = babylonCamera.maxZ;
                    gltfCameraPerspective.znear = babylonCamera.minZ;

                    gltfCamera.type = GLTFCamera.CameraType.perspective.ToString();
                    gltfCamera.perspective = gltfCameraPerspective;
                    break;
                default:
                    logger.RaiseError("GLTFExporter.Camera | camera mode not found");
                    break;
            }
            
            ExportGLTFExtension(babylonCamera, ref gltfCamera,gltf);

            return gltfCamera;
        }
    }
}
