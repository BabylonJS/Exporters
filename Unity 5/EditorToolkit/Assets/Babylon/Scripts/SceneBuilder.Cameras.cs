using System;
using System.IO;
using System.Collections.Generic;
using BabylonExport.Entities;
using UnityEngine;
using UnityEditor;

namespace Unity3D2Babylon
{
    partial class SceneBuilder
    {
        private void ConvertUnityCameraToBabylon(Camera camera, GameObject gameObject, float progress, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags)
        {
            ExporterWindow.ReportProgress(progress, "Exporting camera: " + camera.name);

            BabylonUniversalCamera babylonCamera = new BabylonUniversalCamera
            {
                name = camera.name,
                id = GetID(camera.gameObject),
                fov = camera.fieldOfView * (float)Math.PI / 180,
                minZ = camera.nearClipPlane,
                maxZ = camera.farClipPlane,
                parentId = GetParentID(camera.transform),
                position = camera.transform.localPosition.ToFloat()
            };
            
            if (camera.transform.parent != null) {
                babylonCamera.rotation = new float[3];
                babylonCamera.rotation[0] = camera.transform.localRotation.eulerAngles.x * (float)Math.PI / 180;
                babylonCamera.rotation[1] = camera.transform.localRotation.eulerAngles.y * (float)Math.PI / 180;
                babylonCamera.rotation[2] = camera.transform.localRotation.eulerAngles.z * (float)Math.PI / 180;
            } else {
                var target = new Vector3(0, 0, 1);
                var transformedTarget = camera.transform.TransformDirection(target);
                babylonCamera.target = (camera.transform.position + transformedTarget).ToFloat();
            }
            
            if (camera.orthographic)
            {
                babylonCamera.tags += " [ORTHOGRAPHIC]";
                babylonCamera.mode = 1;
            }
            else
            {
                babylonCamera.mode = 0;
            }

            bool preventDefault = false;
            bool virtualJoystick = false;

            bool trackPosition = false;
            float positionScale = 1.0f;
            string displayName = "";

            int horizontalRes = 1280;
            int verticalRes = 800;
            float horizontalScreen = 0.1497f;
            float verticalScreen = 0.0935f;
            float screenCenter = 0.0468f;
            float cameraBridge = 0.005f;
            float eyeToScreen = 0.0410f;
            float interpupillary = 0.0640f;
            float lensSeparation = 0.0635f;
            float lensCenterOffset = 0.1520f;
            float postProcessScale = 1.7146f;
            bool compensateDistortion = true;

            float ratio = 1.0f;
            float exposure = 1.0f;
            float gaussCoeff = 0.3f;
            float gaussMean = 1.0f;
            float gaussStandDev = 0.8f;
            float gaussMultiplier = 4.0f;
            float brightThreshold = 0.8f;
            float minimumLuminance = 1.0f;
            float maximumLuminance = 1e20f;
            float luminanceIncrease = 0.5f;
            float luminanceDecrease = 0.5f;
            bool stereoSideBySide = false;
            int cameraRigInput = 0;
            float cameraMoveSpeed = 1.0f;
            float cameraRotateSpeed = 0.005f;
            string cameraRigType = "UniversalCamera";
            var rigger = gameObject.GetComponent<CameraRig>();
            if (rigger != null && rigger.isActiveAndEnabled) {
                cameraRigType = rigger.cameraType.ToString();
                cameraRigInput = (int)rigger.cameraInput;
                cameraMoveSpeed = rigger.inputMoveSpeed;
                cameraRotateSpeed = rigger.inputRotateSpeed;
                babylonCamera.speed = rigger.cameraSpeed;   
                babylonCamera.inertia = rigger.inertiaScaleFactor;
                babylonCamera.interaxialDistance = rigger.interaxialDistance;     
                preventDefault = rigger.preventDefaultEvents;
                stereoSideBySide = rigger.stereoscopicSideBySide;
                virtualJoystick = (rigger.cameraType == BabylonCameraOptions.VirtualJoysticksCamera);

                trackPosition = rigger.virtualRealityWebPlatform.trackPosition;
                positionScale = rigger.virtualRealityWebPlatform.positionScale;
                displayName = rigger.virtualRealityWebPlatform.displayName;;

                horizontalRes = rigger.virtualRealityHeadsetOptions.horizontalResolution;
                verticalRes = rigger.virtualRealityHeadsetOptions.verticalResolution;
                horizontalScreen = rigger.virtualRealityHeadsetOptions.horizontalScreen;
                verticalScreen = rigger.virtualRealityHeadsetOptions.verticalScreen;
                screenCenter = rigger.virtualRealityHeadsetOptions.screenCenter;
                cameraBridge = rigger.virtualRealityHeadsetOptions.cameraBridge;
                eyeToScreen = rigger.virtualRealityHeadsetOptions.eyeToScreen;
                interpupillary = rigger.virtualRealityHeadsetOptions.interpupillary;
                lensSeparation = rigger.virtualRealityHeadsetOptions.lensSeparation;
                lensCenterOffset = rigger.virtualRealityHeadsetOptions.lensCenterOffset;
                postProcessScale = rigger.virtualRealityHeadsetOptions.postProcessScale;
                compensateDistortion = rigger.virtualRealityHeadsetOptions.compensateDistortion;
                
                ratio = rigger.highDynamicRenderingPipeline.ratio;
                exposure = rigger.highDynamicRenderingPipeline.exposure;
                gaussCoeff = rigger.highDynamicRenderingPipeline.gaussCoeff;
                gaussMean = rigger.highDynamicRenderingPipeline.gaussMean;
                gaussStandDev = rigger.highDynamicRenderingPipeline.gaussStandDev;
                gaussMultiplier = rigger.highDynamicRenderingPipeline.gaussMultiplier;
                brightThreshold = rigger.highDynamicRenderingPipeline.brightThreshold;
                minimumLuminance = rigger.highDynamicRenderingPipeline.minimumLuminance;
                maximumLuminance = rigger.highDynamicRenderingPipeline.maximumLuminance;
                luminanceIncrease = rigger.highDynamicRenderingPipeline.luminanceIncrease;
                luminanceDecrease = rigger.highDynamicRenderingPipeline.luminanceDecrease;
            }
            SceneBuilder.Metadata.properties["virtualJoystickAttached"] = virtualJoystick;

            metaData.type = "Camera";
            metaData.properties.Add("cameraType", cameraRigType);
            metaData.properties.Add("cameraInput", cameraRigInput);
            metaData.properties.Add("clearFlags", camera.clearFlags.ToString());
            metaData.properties.Add("clearColor", camera.backgroundColor.ToFloat());
            metaData.properties.Add("cullingMask", camera.cullingMask);
            metaData.properties.Add("isOrthographic", camera.orthographic);
            metaData.properties.Add("orthographicSize", camera.orthographicSize);
            metaData.properties.Add("cameraMoveSpeed", cameraMoveSpeed);
            metaData.properties.Add("cameraRotateSpeed", cameraRotateSpeed);
            metaData.properties.Add("useOcclusionCulling", camera.useOcclusionCulling);
            metaData.properties.Add("preventDefaultEvents", preventDefault);
            metaData.properties.Add("stereoscopicSideBySide", stereoSideBySide);

            metaData.properties.Add("wvrTrackPosition", trackPosition);
            metaData.properties.Add("wvrPositionScale", positionScale);
            metaData.properties.Add("wvrDisplayName", displayName);

            metaData.properties.Add("vrHorizontalRes", horizontalRes);
            metaData.properties.Add("vrVerticalRes", verticalRes);
            metaData.properties.Add("vrHorizontalScreen", horizontalScreen);
            metaData.properties.Add("vrVerticalScreen", verticalScreen);
            metaData.properties.Add("vrScreenCenter", screenCenter);
            metaData.properties.Add("vrCameraBridge", cameraBridge);
            metaData.properties.Add("vrEyeToScreen", eyeToScreen);
            metaData.properties.Add("vrInterpupillary", interpupillary);
            metaData.properties.Add("vrLensSeparation", lensSeparation);
            metaData.properties.Add("vrLensCenterOffset", lensCenterOffset);
            metaData.properties.Add("vrPostProcessScale", postProcessScale);
            metaData.properties.Add("vrCompensateDistortion", compensateDistortion);
            
			metaData.properties.Add("hdr", camera.allowHDR);
            metaData.properties.Add("hdrPipeline", null);
            metaData.properties.Add("hdrRatio", ratio);
            metaData.properties.Add("hdrExposure", exposure);
            metaData.properties.Add("hdrGaussCoeff", gaussCoeff);
            metaData.properties.Add("hdrGaussMean", gaussMean);
            metaData.properties.Add("hdrGaussStandDev", gaussStandDev);
            metaData.properties.Add("hdrGaussMultiplier", gaussMultiplier);
            metaData.properties.Add("hdrBrightThreshold", brightThreshold);
            metaData.properties.Add("hdrMinimumLuminance", minimumLuminance);
            metaData.properties.Add("hdrMaximumLuminance", maximumLuminance);
            metaData.properties.Add("hdrLuminanceIncrease", luminanceIncrease);
            metaData.properties.Add("hdrLuminanceDecrease", luminanceDecrease);
            
            babylonCamera.isStereoscopicSideBySide = stereoSideBySide;
            babylonCamera.type = cameraRigType;   
            babylonCamera.tags = componentTags;

            // Animations
            ExportTransformAnimationClips(camera.transform, babylonCamera, ref metaData);

            // Tagging
            if (!String.IsNullOrEmpty(babylonCamera.tags))
            {
                babylonCamera.tags = babylonCamera.tags.Trim();
            }

            babylonCamera.metadata = metaData;
            babylonScene.CamerasList.Add(babylonCamera);

            if (Camera.main == camera)
            {
                babylonScene.activeCameraID = babylonCamera.id;
                babylonScene.clearColor = camera.backgroundColor.ToFloat();
            }

            // Collisions
            if (exportationOptions.ExportCollisions)
            {
                // TODO: Move To Camera Rig Options and Otherwise defaults
                babylonCamera.checkCollisions = true;
                if (SceneController != null) {
                    babylonCamera.applyGravity = (SceneController.sceneOptions.defaultGravity.y == 0.0f && SceneController.sceneOptions.defaultGravity.y == 0.0f && SceneController.sceneOptions.defaultGravity.z == 0.0f) ? false : true;
                    babylonCamera.ellipsoid = SceneController.sceneOptions.defaultEllipsoid.ToFloat();
                }
            }

            // Lens Flares
            ParseLensFlares(gameObject, babylonCamera.id, ref lensFlares);

            // Particles Systems
            if (!exportationOptions.ExportMetadata) babylonCamera.metadata = null;
        }

        private void ConvertUnitySkyboxToBabylon(Camera camera, float progress)
        {
            // Note: Only Support Main Camera Skyboxes
            if (Camera.main == camera && (camera.clearFlags & CameraClearFlags.Skybox) == CameraClearFlags.Skybox)
            {
                // Note: Only Support Tone Mapped Skyboxes
                if (RenderSettings.skybox != null)
                {
                    BabylonTexture skytex = null;
                    if (RenderSettings.skybox.shader.name == "Skybox/Cubemap")
                    {
                        var cubeMap = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                        if (cubeMap != null)
                        {
                            var cubeTextureFile = AssetDatabase.GetAssetPath(cubeMap);
                            var cubeTextureExt = Path.GetExtension(cubeTextureFile);
                            if (!cubeTextureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase)) {
                                ExporterWindow.ReportProgress(progress, "Baking skybox environment textures... This may take a while.");
                                var faceTextureExt = ".jpg";
                                var faceTextureFormat = BabylonImageFormat.JPEG;
                                if (exportationOptions.ImageEncodingOptions == (int)BabylonImageFormat.PNG) {
                                    faceTextureExt = ".png";
                                    faceTextureFormat = BabylonImageFormat.PNG;
                                }
                                string frontTextureExt = "_pz" + faceTextureExt;
                                string backTextureExt = "_nz" + faceTextureExt;
                                string leftTextureExt = "_px" + faceTextureExt;
                                string rightTextureExt = "_nx" + faceTextureExt;
                                string upTextureExt = "_py" + faceTextureExt;
                                string downTextureExt = "_ny" + faceTextureExt;
                                skytex = new BabylonTexture();
                                skytex.name = String.Format("{0}_Skybox", SceneName);
                                skytex.isCube = true;
                                skytex.coordinatesMode = 5;
                                skytex.extensions = new string[] { leftTextureExt, upTextureExt, frontTextureExt, rightTextureExt, downTextureExt, backTextureExt };
                                Tools.SetTextureWrapMode(skytex, cubeMap);
                                var outputFile = Path.Combine(babylonScene.OutputPath, skytex.name + faceTextureExt);
                                var splitterOpts = new BabylonSplitterOptions();
                                this.skyboxTextures = Tools.ExportCubemap(cubeMap, outputFile, faceTextureFormat, splitterOpts);
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cubemap texture type of " + cubeTextureExt + " for " + Path.GetFileName(cubeTextureFile));
                                return;
                            }
                        }
                    }
                    else if (RenderSettings.skybox.shader.name == "Skybox/6 Sided" || RenderSettings.skybox.shader.name == "Mobile/Skybox")
                    {
                        // 6-Sided Skybox Textures (Tone Mapped Image Formats Only)
                        var frontTexture = RenderSettings.skybox.GetTexture("_FrontTex") as Texture2D;
                        var backTexture = RenderSettings.skybox.GetTexture("_BackTex") as Texture2D;
                        var leftTexture = RenderSettings.skybox.GetTexture("_LeftTex") as Texture2D;
                        var rightTexture = RenderSettings.skybox.GetTexture("_RightTex") as Texture2D;
                        var upTexture = RenderSettings.skybox.GetTexture("_UpTex") as Texture2D;
                        var downTexture = RenderSettings.skybox.GetTexture("_DownTex") as Texture2D;
                        if (frontTexture != null && backTexture != null && leftTexture != null && rightTexture != null && upTexture != null && downTexture != null)
                        {
                            ExporterWindow.ReportProgress(progress, "Exporting skybox environment textures... This may take a while.");
                            string frontTextureExt = "_pz.jpg";
                            string backTextureExt = "_nz.jpg";
                            string leftTextureExt = "_px.jpg";
                            string rightTextureExt = "_nx.jpg";
                            string upTextureExt = "_py.jpg";
                            string downTextureExt = "_ny.jpg";
                            skytex = new BabylonTexture();
                            skytex.name = String.Format("{0}_Skybox", SceneName);
                            skytex.isCube = true;
                            skytex.coordinatesMode = 5;
                            Tools.SetTextureWrapMode(skytex, frontTexture);
                            List<Tools.TextureInfo> faces = new List<Tools.TextureInfo>();
                            var faceTextureFile = AssetDatabase.GetAssetPath(frontTexture);
                            var faceTextureExt = Path.GetExtension(faceTextureFile);
                            var faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                frontTextureExt = "_pz" + faceTextureExt;
                                var frontTextureName = String.Format("{0}_pz{1}", skytex.name, faceTextureExt);
                                var frontTexturePath = Path.Combine(babylonScene.OutputPath, frontTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(frontTexturePath, frontTextureName, frontTexture);
                                faces.Add(new Tools.TextureInfo { filename = frontTexturePath, texture = frontTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            faceTextureFile = AssetDatabase.GetAssetPath(backTexture);
                            faceTextureExt = Path.GetExtension(faceTextureFile);
                            faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                backTextureExt = "_nz" + faceTextureExt;
                                var backTextureName = String.Format("{0}_nz{1}", skytex.name, faceTextureExt);
                                var backTexturePath = Path.Combine(babylonScene.OutputPath, backTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(backTexturePath, backTextureName, backTexture);
                                faces.Add(new Tools.TextureInfo { filename = backTexturePath, texture = backTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            faceTextureFile = AssetDatabase.GetAssetPath(leftTexture);
                            faceTextureExt = Path.GetExtension(faceTextureFile);
                            faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                leftTextureExt = "_px" + faceTextureExt;
                                var leftTextureName = String.Format("{0}_px{1}", skytex.name, faceTextureExt);
                                var leftTexturePath = Path.Combine(babylonScene.OutputPath, leftTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(leftTexturePath, leftTextureName, leftTexture);
                                faces.Add(new Tools.TextureInfo { filename = leftTexturePath, texture = leftTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            faceTextureFile = AssetDatabase.GetAssetPath(rightTexture);
                            faceTextureExt = Path.GetExtension(faceTextureFile);
                            faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                rightTextureExt = "_nx" + faceTextureExt;
                                var rightTextureName = String.Format("{0}_nx{1}", skytex.name, faceTextureExt);
                                var rightTexturePath = Path.Combine(babylonScene.OutputPath, rightTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(rightTexturePath, rightTextureName, rightTexture);
                                faces.Add(new Tools.TextureInfo { filename = rightTexturePath, texture = rightTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            faceTextureFile = AssetDatabase.GetAssetPath(upTexture);
                            faceTextureExt = Path.GetExtension(faceTextureFile);
                            faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                upTextureExt = "_py" + faceTextureExt;
                                var upTextureName = String.Format("{0}_py{1}", skytex.name, faceTextureExt);
                                var upTexturePath = Path.Combine(babylonScene.OutputPath, upTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(upTexturePath, upTextureName, upTexture);
                                faces.Add(new Tools.TextureInfo { filename = upTexturePath, texture = upTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            faceTextureFile = AssetDatabase.GetAssetPath(downTexture);
                            faceTextureExt = Path.GetExtension(faceTextureFile);
                            faceImportTool = new BabylonTextureImporter(faceTextureFile);
                            if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                                downTextureExt = "_ny" + faceTextureExt;
                                var downTextureName = String.Format("{0}_ny{1}", skytex.name, faceTextureExt);
                                var downTexturePath = Path.Combine(babylonScene.OutputPath, downTextureName);
                                faceImportTool.SetReadable();
                                CopyTextureFace(downTexturePath, downTexturePath, downTexture);
                                faces.Add(new Tools.TextureInfo { filename = downTexturePath, texture = downTexture.Copy() });
                            } else {
                                UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                            }
                            skytex.extensions = new string[] { leftTextureExt, upTextureExt, frontTextureExt, rightTextureExt, downTextureExt, backTextureExt };
                            this.skyboxTextures = (faces.Count > 0) ? faces.ToArray() : null;
                        }
                    }
                    if (skytex != null)
                    {
                        skytex.level = (SceneController != null) ? SceneController.skyboxOptions.skyTextureLevel : 1.0f;
                        string meshTags = (SceneController != null) ? SceneController.skyboxOptions.meshTags : String.Empty;
                        float skyboxSize = (SceneController != null) ? SceneController.skyboxOptions.skyMeshSize : 1000;
                        bool skyboxSphere = (SceneController != null) ? (SceneController.skyboxOptions.meshType == BabylonSkyboxType.Sphere) : false;
                        // Babylon Skybox Mesh
                        var skybox = new BabylonMesh();
                        skybox.id = Guid.NewGuid().ToString();
                        skybox.infiniteDistance = true;
                        skybox.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                        if (!String.IsNullOrEmpty(meshTags)) {
                            skybox.tags = meshTags;
                        }
                        if (skyboxSphere) {
                            skybox.name = "sceneSkyboxSphere";
                            Mesh sphereMesh = Tools.CreateSphereMesh(skyboxSize * 0.5f, 48, 48);
                            Tools.GenerateBabylonMeshData(sphereMesh, skybox);
                        } else {
                            skybox.name = "sceneSkyboxCube";
                            Mesh boxMesh = Tools.CreateBoxMesh(skyboxSize, skyboxSize, skyboxSize);
                            Tools.GenerateBabylonMeshData(boxMesh, skybox);
                        }
                        // Babylon Default Skybox
                        var skyboxMaterial = new BabylonDefaultMaterial();
                        skyboxMaterial.name = "sceneSkyboxMaterial";
                        skyboxMaterial.id = Guid.NewGuid().ToString();
                        skyboxMaterial.backFaceCulling = false;
                        skyboxMaterial.disableLighting = true;
                        skyboxMaterial.diffuse = Color.black.ToFloat();
                        skyboxMaterial.specular = Color.black.ToFloat();
                        skyboxMaterial.ambient = Color.clear.ToFloat();
                        skyboxMaterial.reflectionTexture = skytex;
                        // Babylon Skybox Material
                        skybox.materialId = skyboxMaterial.id;
                        babylonScene.MeshesList.Add(skybox);
                        babylonScene.MaterialsList.Add(skyboxMaterial);
                        babylonScene.AddTextureCube("sceneSkyboxMaterial");
                    }
                }
            }
        }
    }
}
