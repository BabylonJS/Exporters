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
            int shadowMapSize = 1024;
            float shadowMapBias = 0.00005f;
            BabylonLightingFilter shadowMapFilter = BabylonLightingFilter.NoFilter;
            bool shadowKernelBlur = false;
            float shadowBlurKernel = 32.0f;
            float shadowBlurScale = 2.0f;
            float shadowBlurOffset = 0.0f;
            float shadowOrthoScale = 0.5f;
            float shadowStrengthScale = 1.0f;
            float shadowDepthScale = 30.0f;
            bool forceBackFacesOnly = false;
            var shadows = light.gameObject.GetComponent<UnityEditor.ShadowGenerator>();
            if (shadows != null && shadows.isActiveAndEnabled)
            {
                shadowMapSize = shadows.shadowMapSize;
                shadowMapBias = shadows.shadowMapBias;
                shadowMapFilter = shadows.shadowMapFilter;
                shadowKernelBlur = shadows.shadowKernelBlur;
                shadowBlurKernel = shadows.shadowBlurKernel;
                shadowBlurScale = shadows.shadowBlurScale;
                shadowBlurOffset = shadows.shadowBlurOffset;
                shadowOrthoScale = shadows.shadowOrthoScale;
                shadowStrengthScale = shadows.shadowStrengthScale;
                shadowDepthScale = shadows.shadowDepthScale;
                forceBackFacesOnly = shadows.forceBackFacesOnly;
            }
            if (babylonLight is BabylonDirectionalLight)
            {
                ((BabylonDirectionalLight)babylonLight).shadowOrthoScale = shadowOrthoScale;
            }
            float strength = light.shadowStrength * shadowStrengthScale;
            var generator = new BabylonExport.Entities.BabylonShadowGenerator
            {
                lightId = GetID(light.gameObject),
                bias = shadowMapBias,
                mapSize = shadowMapSize,
                darkness = (1.0f - strength),
                depthScale = shadowDepthScale,
                blurScale = shadowBlurScale,
                blurKernel = shadowBlurKernel,
                blurBoxOffset = shadowBlurOffset,
                useKernelBlur = shadowKernelBlur,
                forceBackFacesOnly = forceBackFacesOnly
            };
            switch (shadowMapFilter)
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
                    if (!gameObject.IsLightapStatic())
                    {
                        var meshFilter = gameObject.GetComponent<MeshFilter>();
                        var meshRender = gameObject.GetComponent<MeshRenderer>();
                        if (meshFilter != null && meshRender != null && meshRender.enabled == true && meshRender.shadowCastingMode != ShadowCastingMode.Off)
                        {
                            renderList.Add(GetID(gameObject));
                            continue;
                        }
                        var skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
                        if (skinnedMesh != null && skinnedMesh.enabled == true && skinnedMesh.shadowCastingMode != ShadowCastingMode.Off)
                        {
                            renderList.Add(GetID(gameObject));
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

        private void ConvertUnityLightToBabylon(Light light, GameObject gameObject, float progress, ref UnityMetaData metaData, ref List<UnityFlareSystem> lensFlares, ref string componentTags)
        {
			if (light.isActiveAndEnabled == false) return;
            if (light.type != LightType.Area && light.lightmapBakeType == LightmapBakeType.Baked) return;
            if (light.type == LightType.Area && exportationOptions.BakedLightsMode == (int)BabylonAreaLights.ExcludeAreaBakingLights) return;

            ExporterWindow.ReportProgress(progress, "Exporting light: " + light.name);
            BabylonLight babylonLight = (light.type == LightType.Directional) ? new BabylonDirectionalLight() : new BabylonLight();
            babylonLight.name = light.name;
            babylonLight.id = GetID(light.gameObject);
            babylonLight.parentId = GetParentID(light.transform);

            metaData.type = "Light";
            babylonLight.tags = componentTags;

            switch (light.type)
            {
                case LightType.Area:
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
            babylonLight.direction = transformedDirection.ToFloat();

            //light.intensityMode = BABYLON.Light.INTENSITYMODE_AUTOMATIC;
            // Lumen (lm)
            //light.intensityMode = BABYLON.Light.INTENSITYMODE_LUMINOUSPOWER;
            // Candela (lm/sr)
            //light.intensityMode = BABYLON.Light.INTENSITYMODE_LUMINOUSINTENSITY;
            // Lux (lm/m^2)
            //light.intensityMode = BABYLON.Light.INTENSITYMODE_ILLUMINANCE;
            // Nit (cd/m^2)
            //light.intensityMode = BABYLON.Light.INTENSITYMODE_LUMINANCE;

            babylonLight.intensity = light.intensity;
            babylonLight.intensityMode = (int)BabylonLightIntensity.Automatic;
            var lightScale = gameObject.GetComponent<LightScale>();
            if (lightScale != null) {
                babylonLight.intensity *= lightScale.lightIntensity;
                babylonLight.intensityMode = (int)lightScale.intensityMode;
            }
            babylonLight.diffuse = light.color.ToFloat();
            babylonLight.specular = Color.white.ToFloat();
            babylonLight.exponent = 1.0f;
            babylonLight.angle = light.spotAngle * (float)Math.PI / 180;

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

            // Realtime Shadows
            if (light.shadows != LightShadows.None)
            {
                GenerateShadowsGenerator(babylonLight, light, progress);
            }
            if (!exportationOptions.ExportMetadata) babylonLight.metadata = null;
        }
    }
}
