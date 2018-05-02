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

            float arcRotateAlpha = (float)Math.PI / 2.0f;
            float arcRotateBeta = (float)Math.PI / 4.0f;
            float arcRotateRadius = 3.0f;
            float[] arcRotateTarget = new float[] { 0.0f, 1.0f, 0.0f };
            float arcRotateLowerRadiusLimit = 1;
            float arcRotateUpperRadiusLimit = 10;
            float arcRotateWheelDeltaPercentage = 0.01f;

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
            bool stereoSideBySide = false;
            int cameraRigInput = 0;
            float cameraMoveSpeed = 1.0f;
            float cameraRotateSpeed = 0.005f;
            string cameraRigType = "UniversalCamera";
            bool localMultiPlayer = false;
            bool multiPlayerElements = false;
            bool cameraCollisions = true;
            var rigger = gameObject.GetComponent<CameraRig>();
            if (rigger != null && rigger.isActiveAndEnabled) {
                localMultiPlayer = (rigger.cameraType == BabylonCameraOptions.LocalMultiPlayerViewCamera);
                cameraRigType = (localMultiPlayer == true) ? "UniversalCamera" : rigger.cameraType.ToString();
                cameraRigInput = (int)rigger.cameraInput;
                cameraMoveSpeed = rigger.inputMoveSpeed;
                cameraRotateSpeed = rigger.inputRotateSpeed;
                babylonCamera.speed = rigger.cameraSpeed;   
                babylonCamera.inertia = rigger.inertiaScaleFactor;
                babylonCamera.interaxialDistance = rigger.interaxialDistance;     
                preventDefault = rigger.preventDefaultEvents;
                stereoSideBySide = rigger.stereoSideBySide;
                virtualJoystick = (rigger.cameraType == BabylonCameraOptions.VirtualJoysticksCamera);
                cameraCollisions = rigger.checkCameraCollision;
                multiPlayerElements = rigger.multiPlayerElements;

                arcRotateAlpha = rigger.arcRotateCameraOptions.rotateAlpha;
                arcRotateBeta = rigger.arcRotateCameraOptions.rotateBeta;
                arcRotateRadius = rigger.arcRotateCameraOptions.rotateRadius;
                arcRotateTarget = rigger.arcRotateCameraOptions.rotateTarget.ToFloat();
                arcRotateLowerRadiusLimit = rigger.arcRotateCameraOptions.lowerRadiusLimit;
                arcRotateUpperRadiusLimit = rigger.arcRotateCameraOptions.upperRadiusLimit;
                arcRotateWheelDeltaPercentage = rigger.arcRotateCameraOptions.wheelDeltaPercentage;

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
            }
            SceneBuilder.Metadata.properties["virtualJoystickAttached"] = virtualJoystick;

            metaData.type = "Camera";
            metaData.properties.Add("mainCamera", (Camera.main == camera));
            metaData.properties.Add("cameraType", cameraRigType);
            metaData.properties.Add("cameraInput", cameraRigInput);
            metaData.properties.Add("clearFlags", camera.clearFlags.ToString());
            metaData.properties.Add("clearColor", babylonScene.clearColor);
            metaData.properties.Add("cullingMask", camera.cullingMask);
            metaData.properties.Add("isOrthographic", camera.orthographic);
            metaData.properties.Add("orthographicSize", camera.orthographicSize);
            metaData.properties.Add("cameraMoveSpeed", cameraMoveSpeed);
            metaData.properties.Add("cameraRotateSpeed", cameraRotateSpeed);
            metaData.properties.Add("useOcclusionCulling", camera.useOcclusionCulling);
            metaData.properties.Add("preventDefaultEvents", preventDefault);
            metaData.properties.Add("stereoscopicSideBySide", stereoSideBySide);
            metaData.properties.Add("localMultiPlayerViewCamera", localMultiPlayer);
            metaData.properties.Add("localMultiPlayerElements", multiPlayerElements);

            metaData.properties.Add("arcRotateAlpha", arcRotateAlpha);
            metaData.properties.Add("arcRotateBeta", arcRotateBeta);
            metaData.properties.Add("arcRotateRadius", arcRotateRadius);
            metaData.properties.Add("arcRotateTarget", arcRotateTarget);
            metaData.properties.Add("arcRotateLowerRadiusLimit", arcRotateLowerRadiusLimit);
            metaData.properties.Add("arcRotateUpperRadiusLimit", arcRotateUpperRadiusLimit);
            metaData.properties.Add("arcRotateWheelDeltaPercentage", arcRotateWheelDeltaPercentage);

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
            }

            // Collisions
            if (exportationOptions.ExportCollisions)
            {
                if (camera.transform.parent != null) {
                    babylonCamera.checkCollisions = false;
                } else {
                    babylonCamera.checkCollisions = cameraCollisions;
                }
            }

            // Lens Flares
            ParseLensFlares(gameObject, babylonCamera.id, ref lensFlares);

            // Greavity Ellispoid
            if (SceneController != null)
            {
                babylonCamera.applyGravity = (SceneController.sceneOptions.defaultGravity.y == 0.0f && SceneController.sceneOptions.defaultGravity.y == 0.0f && SceneController.sceneOptions.defaultGravity.z == 0.0f) ? false : true;
                babylonCamera.ellipsoid = SceneController.sceneOptions.defaultEllipsoid.ToFloat();
            }

            // Particles Systems
            if (!exportationOptions.ExportMetadata) babylonCamera.metadata = null;
        }

        private void ExportMainCameraSkyboxToBabylon()
        {
            if (RenderSettings.sun != null) {
                var direction = new Vector3(0, 0, 1);
                var transformedDirection = RenderSettings.sun.transform.TransformDirection(direction);
                SceneBuilder.SunlightDirection = transformedDirection.ToFloat();
                SceneBuilder.SunlightIndentifier = GetID(RenderSettings.sun.gameObject);
            }
            if (Camera.main != null) {
                babylonScene.clearColor = Camera.main.backgroundColor.ToFloat(1.0f);
                if ((Camera.main.clearFlags & CameraClearFlags.Skybox) == CameraClearFlags.Skybox)
                {
                    if (RenderSettings.skybox != null)
                    {
                        bool dds = false;
                        BabylonTexture skytex = null;
                        if (RenderSettings.skybox.shader.name == "Skybox/Cubemap")
                        {
                            skytex = new BabylonCubeTexture();
                            skytex.name = String.Format("{0}_Skybox", SceneName);
                            skytex.coordinatesMode = 5;
                            Cubemap cubeMap = RenderSettings.skybox.GetTexture("_Tex") as Cubemap;
                            if (cubeMap != null) {
                                var srcTexturePath = AssetDatabase.GetAssetPath(cubeMap);
                                var srcTextureExt = Path.GetExtension(srcTexturePath);
                                if (srcTextureExt.Equals(".dds", StringComparison.OrdinalIgnoreCase)) {
                                    ExporterWindow.ReportProgress(1, "Exporting skybox direct draw surface... This may take a while.");
                                    // ..
                                    // Export Draw Surface Skybox Textures
                                    // ..
                                    dds = true;
                                    skytex.name += ".dds";
                                    skytex.extensions = null;
                                    ((BabylonCubeTexture)skytex).prefiltered = true;
                                    CopyCubemapTexture(skytex.name, cubeMap, skytex);
                                } else {
                                    ExporterWindow.ReportProgress(1, "Baking skybox environment textures... This may take a while.");
                                    var imageFormat = (BabylonImageFormat)ExporterWindow.exportationOptions.ImageEncodingOptions;
                                    // ..
                                    // Export Tone Mapped Cubemap To 6-Sided Skybox Textures
                                    // ..
                                    bool jpeg = (imageFormat == BabylonImageFormat.JPEG);
                                    string faceTextureExt = (jpeg) ? ".jpg" : ".png";
                                    string frontTextureExt = "_pz" + faceTextureExt;
                                    string backTextureExt = "_nz" + faceTextureExt;
                                    string leftTextureExt = "_px" + faceTextureExt;
                                    string rightTextureExt = "_nx" + faceTextureExt;
                                    string upTextureExt = "_py" + faceTextureExt;
                                    string downTextureExt = "_ny" + faceTextureExt;
                                    skytex.extensions = new string[] { leftTextureExt, upTextureExt, frontTextureExt, rightTextureExt, downTextureExt, backTextureExt };
                                    Tools.SetTextureWrapMode(skytex, cubeMap);
                                    var outputFile = Path.Combine(babylonScene.OutputPath, skytex.name + faceTextureExt);
                                    var splitterOpts = new BabylonSplitterOptions();
                                    Tools.ExportSkybox(cubeMap, outputFile, splitterOpts, imageFormat);
                                }
                            }
                        }
                        else if (RenderSettings.skybox.shader.name == "Skybox/6 Sided" || RenderSettings.skybox.shader.name == "Mobile/Skybox")
                        {
                            skytex = new BabylonCubeTexture();
                            skytex.name = String.Format("{0}_Skybox", SceneName);
                            skytex.coordinatesMode = 5;
                            // ..
                            // 6-Sided Skybox Textures (Tone Mapped Image Formats Only)
                            // ..
                            var frontTexture = RenderSettings.skybox.GetTexture("_FrontTex") as Texture2D;
                            var backTexture = RenderSettings.skybox.GetTexture("_BackTex") as Texture2D;
                            var leftTexture = RenderSettings.skybox.GetTexture("_LeftTex") as Texture2D;
                            var rightTexture = RenderSettings.skybox.GetTexture("_RightTex") as Texture2D;
                            var upTexture = RenderSettings.skybox.GetTexture("_UpTex") as Texture2D;
                            var downTexture = RenderSettings.skybox.GetTexture("_DownTex") as Texture2D;
                            DumpSkyboxTextures( ref skytex, ref frontTexture, ref backTexture, ref leftTexture, ref rightTexture, ref upTexture, ref downTexture);
                        }
                        else if (RenderSettings.skybox.name.Equals("Default-Skybox"))
                        {
                            skytex = new BabylonCubeTexture();
                            skytex.name = String.Format("{0}_Skybox", SceneName);
                            skytex.coordinatesMode = 5;
                            // ..
                            // 6-Sided Skybox Textures (Toolkit Skybox Template Images)
                            // ..
                            string skyboxPath = "Assets/Babylon/Template/Skybox/"; 
                            var frontTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_pz.png");
                            var backTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_nz.png");
                            var leftTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_px.png");
                            var rightTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_nx.png");
                            var upTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_py.png");
                            var downTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(skyboxPath + "DefaultSkybox_ny.png");
                            DumpSkyboxTextures( ref skytex, ref frontTexture, ref backTexture, ref leftTexture, ref rightTexture, ref upTexture, ref downTexture);
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("SKYBOX: " + RenderSettings.skybox.shader.name + " shader type is unsupported. Skybox and reflections will be disabled.");
                        }
                        if (skytex != null) {
                            float size = (SceneController != null) ? SceneController.skyboxOptions.skyboxMeshSize : 1000;
                            string tags = (SceneController != null) ? SceneController.skyboxOptions.skyboxMeshTags : String.Empty;
                            // ..
                            // PBR Skybox Material Support
                            // ..
                            bool pbr = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.physicalBased : false;
                            float pbr_ms = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.microSurface : 1.0f;
                            float pbr_cc = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.cameraContrast : 1.0f;
                            float pbr_ce = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.cameraExposure : 1.0f;
                            float pbr_di = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.directIntensity : 1.0f;
                            float pbr_ei = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.emissiveIntensity : 0.5f;
                            float pbr_si = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.specularIntensity : 0.5f;
                            float pbr_ri = (SceneController != null) ? SceneController.skyboxOptions.directDrawSurface.environmentIntensity : 1.0f;
                            var skybox = new BabylonMesh();
                            skybox.id = Guid.NewGuid().ToString();
                            skybox.infiniteDistance = true;
                            skybox.numBoneInfluencers = Tools.GetMaxBoneInfluencers();
                            if (!String.IsNullOrEmpty(tags)) {
                                skybox.tags = tags;
                            }
                            skybox.name = "sceneSkyboxMesh";
                            Mesh boxMesh = Tools.CreateBoxMesh(size, size, size);
                            Tools.GenerateBabylonMeshData(boxMesh, skybox);
                            BabylonMaterial skyboxMaterial = null;
                            if (dds == true && pbr == true) {
                                var skyboxMaterialPbr = new BabylonSystemMaterial {
                                    name = "sceneSkyboxMaterial",
                                    id = Guid.NewGuid().ToString(),
                                    backFaceCulling = false,
                                    disableLighting = true,
                                    albedo = Color.white.ToFloat(),
                                    ambient = Color.black.ToFloat(),
                                    emissive = Color.black.ToFloat(),
                                    metallic = null,
                                    roughness = null,
                                    sideOrientation = 1,
                                    reflectivity = Color.white.ToFloat(),
                                    reflection = Color.white.ToFloat(),
                                    microSurface = pbr_ms,
                                    cameraContrast = pbr_cc,
                                    cameraExposure = pbr_ce,
                                    directIntensity = pbr_di,
                                    emissiveIntensity = pbr_ei,
                                    specularIntensity = pbr_si,
                                    environmentIntensity = pbr_ri,
                                    maxSimultaneousLights = 4,
                                    useSpecularOverAlpha = false,
                                    useRadianceOverAlpha = false,
                                    usePhysicalLightFalloff = false,
                                    useAlphaFromAlbedoTexture = false,
                                    useEmissiveAsIllumination = false,
                                    reflectionTexture = skytex
                                };
                                skyboxMaterial = skyboxMaterialPbr;
                            } else {
                                var skyboxMaterialStd = new BabylonDefaultMaterial {
                                    name = "sceneSkyboxMaterial",
                                    id = Guid.NewGuid().ToString(),
                                    backFaceCulling = false,
                                    disableLighting = true,
                                    diffuse = Color.black.ToFloat(),
                                    specular = Color.black.ToFloat(),
                                    ambient = Color.clear.ToFloat(),
                                    reflectionTexture = skytex
                                };
                                skyboxMaterial = skyboxMaterialStd;
                            }
                            if (skyboxMaterial != null) {
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

        private void DumpSkyboxTextures(ref BabylonTexture skytex, ref Texture2D frontTexture, ref Texture2D backTexture, ref Texture2D leftTexture, ref Texture2D rightTexture, ref Texture2D upTexture, ref Texture2D downTexture)
        {
            if (frontTexture != null && backTexture != null && leftTexture != null && rightTexture != null && upTexture != null && downTexture != null)
            {
                ExporterWindow.ReportProgress(1, "Exporting skybox environment textures... This may take a while.");
                string frontTextureExt = "_pz.jpg";
                string backTextureExt = "_nz.jpg";
                string leftTextureExt = "_px.jpg";
                string rightTextureExt = "_nx.jpg";
                string upTextureExt = "_py.jpg";
                string downTextureExt = "_ny.jpg";
                Tools.SetTextureWrapMode(skytex, frontTexture);
                var faceTextureFile = AssetDatabase.GetAssetPath(frontTexture);
                var faceTextureExt = Path.GetExtension(faceTextureFile);
                var faceImportTool = new BabylonTextureImporter(faceTextureFile);
                if (faceTextureExt.Equals(".png", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || faceTextureExt.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) {
                    frontTextureExt = "_pz" + faceTextureExt;
                    var frontTextureName = String.Format("{0}_pz{1}", skytex.name, faceTextureExt);
                    var frontTexturePath = Path.Combine(babylonScene.OutputPath, frontTextureName);
                    faceImportTool.SetReadable();
                    CopyTextureFace(frontTexturePath, frontTextureName, frontTexture);
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
                } else {
                    UnityEngine.Debug.LogWarning("SKYBOX: Unsupported cube face texture type of " + faceTextureExt + " for " + Path.GetFileName(faceTextureFile));
                }
                skytex.extensions = new string[] { leftTextureExt, upTextureExt, frontTextureExt, rightTextureExt, downTextureExt, backTextureExt };
            }
        }
    }
}
