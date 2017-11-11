using System;
using System.IO;
using System.Collections.Generic;
using BabylonExport.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity3D2Babylon
{
    partial class SceneBuilder
    {
        private void GenerateShadowsGenerator(BabylonLight babylonLight, Light light, float progress)
        {
            var shadows = light.gameObject.GetComponent<UnityEditor.ShadowGenerator>();
            if (shadows != null && shadows.isActiveAndEnabled)
            {
                if (babylonLight is BabylonDirectionalLight)
                {
                    ((BabylonDirectionalLight)babylonLight).shadowOrthoScale = shadows.shadowOrthoScale;
                }
                float strength = light.shadowStrength * shadows.shadowStrengthScale;
                var generator = new BabylonExport.Entities.BabylonShadowGenerator
                {
                    lightId = GetID(light.gameObject),
                    bias = shadows.shadowMapBias,
                    mapSize = shadows.shadowMapSize,
                    darkness = (1.0f - strength),
                    depthScale = shadows.shadowDepthScale,
                    blurScale = shadows.shadowBlurScale,
                    blurBoxOffset = shadows.shadowBlurOffset,
                    forceBackFacesOnly = shadows.forceBackFacesOnly
                };
                switch (shadows.shadowMapFilter)
                {
                    case BabylonLightingFilter.PoissonSampling:
                        generator.usePoissonSampling = true;
                        break;
                    case BabylonLightingFilter.ExponentialShadowMap:
                        generator.useExponentialShadowMap = true;
                        break;
                    case BabylonLightingFilter.BlurExponentialShadowMap:
                        generator.useBlurExponentialShadowMap = true;
                        break;
                }
                // Light Shadow Generator Render List
                var renderList = new List<string>();
                foreach (var gameObject in gameObjects)
                {
                    if (gameObject.layer != ExporterWindow.PrefabIndex)
                    {
                        if (!gameObject.IsLightapStatic()) {
                            var shadowmap = gameObject.GetComponent<ShadowMap>();
                            if (shadowmap != null && shadowmap.runtimeShadows == BabylonEnabled.Enabled)
                            {
                                var renderer = gameObject.GetComponent<Renderer>();
                                var meshFilter = gameObject.GetComponent<MeshFilter>();
                                if (meshFilter != null && renderer != null && renderer.shadowCastingMode != ShadowCastingMode.Off)
                                {
                                    renderList.Add(GetID(gameObject));
                                    continue;
                                }
                                var skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
                                if (skinnedMesh != null && renderer != null && renderer.shadowCastingMode != ShadowCastingMode.Off)
                                {
                                    renderList.Add(GetID(gameObject));
                                }
                            }
                        }
                    }
                }
                if (renderList.Count > 0)
                {
                    generator.renderList = renderList.ToArray();
                    babylonScene.ShadowGeneratorsList.Add(generator);
                }
            }
        }

        private void ConvertUnityLightToBabylon(Light light, GameObject gameObject, float progress, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags)
        {
            // Note: No Inactive Or Full Baking Lights Exported
			if (light.isActiveAndEnabled == false || light.type == LightType.Area || light.lightmapBakeType == LightmapBakeType.Baked) return;

            ExporterWindow.ReportProgress(progress, "Exporting light: " + light.name);
            BabylonLight babylonLight = (light.type == LightType.Directional) ? new BabylonDirectionalLight() : new BabylonLight();
            babylonLight.name = light.name;
            babylonLight.id = GetID(light.gameObject);
            babylonLight.parentId = GetParentID(light.transform);

            metaData.type = "Light";
            babylonLight.tags = componentTags;

            switch (light.type)
            {
                case LightType.Point:
                    babylonLight.type = 0;
                    babylonLight.range = light.range;
                    break;
                case LightType.Directional:
                    babylonLight.type = 1;
                    break;
                case LightType.Spot:
                    babylonLight.type = 2;
                    break;
            }

            babylonLight.position = light.transform.localPosition.ToFloat();

            var direction = new Vector3(0, 0, 1);
            var transformedDirection = light.transform.TransformDirection(direction);
            var defaultRotationOffset = (SceneController != null) ? SceneController.lightingOptions.rotationOffset : ExporterWindow.DefaultRotationOffset;
            transformedDirection[0] += defaultRotationOffset.x;
            transformedDirection[1] += defaultRotationOffset.y;
            transformedDirection[2] += defaultRotationOffset.z;
            babylonLight.direction = transformedDirection.ToFloat();
            
            babylonLight.diffuse = light.color.ToFloat();

            float defaultIntenistyFactor = (SceneController != null) ? SceneController.lightingOptions.intensityScale : ExporterWindow.DefaultIntensityScale;
            babylonLight.intensity = light.intensity *  defaultIntenistyFactor;

            babylonLight.angle = light.spotAngle * (float)Math.PI / 180;
            babylonLight.exponent = 1.0f;

            // Animations
            ExportTransformAnimationClips(light.transform, babylonLight, ref metaData);

            // Tagging
            if (!String.IsNullOrEmpty(babylonLight.tags))
            {
                babylonLight.tags = babylonLight.tags.Trim();
            }

            babylonLight.metadata = metaData;
            babylonScene.LightsList.Add(babylonLight);

            // Lens Flares
            ParseLensFlares(gameObject, babylonLight.id, ref lensFlares);

            // Realtime Shadow Maps (Scene Controller Required)
            if ((light.type == LightType.Directional || light.type == LightType.Point || light.type == LightType.Spot) && light.shadows != LightShadows.None)
            {
                GenerateShadowsGenerator(babylonLight, light, progress);
            }
            if (!exportationOptions.ExportMetadata) babylonLight.metadata = null;
        }
    }
}
