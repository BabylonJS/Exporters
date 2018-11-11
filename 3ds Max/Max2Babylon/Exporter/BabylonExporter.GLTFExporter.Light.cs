﻿using BabylonExport.Entities;
using GLTFExport.Entities;
using System.Collections.Generic;

namespace Max2Babylon
{
    internal partial class BabylonExporter
    {
        public const string KHR_lights_punctuals = "KHR_lights_punctual";  // Name of the extension


        /// <summary>
        /// Add the light the global extensions
        /// </summary>
        /// <param name="gltf">The gltf data</param>
        /// <param name="babylonLight">The light to export</param>
        /// <returns>the index of the light</returns>
        private int AddLightExtension(ref GLTF gltf, BabylonLight babylonLight)
        {
            if (gltf.extensionsUsed.Contains(KHR_lights_punctuals) == false)
            {
                gltf.extensionsUsed.Add(KHR_lights_punctuals);
            }

            // new light in the gltf extensions
            GLTFLight light = new GLTFLight
            {
                color = babylonLight.diffuse,
                type = ((GLTFLight.LightType)babylonLight.type).ToString(),
                intensity = babylonLight.intensity,
            };

            switch (babylonLight.type)
            {
                case (0): // point
                    light.type = GLTFLight.LightType.point.ToString();
                    light.range = babylonLight.range;
                    break;
                case (1): // directional
                    light.type = GLTFLight.LightType.directional.ToString();
                    break;
                case (2): // spot
                    light.type = GLTFLight.LightType.spot.ToString();
                    light.range = babylonLight.range;
                    light.spot = new GLTFLight.Spot
                    {
                        //innerConeAngle = 0, Babylon doesn't support the innerConeAngle
                        outerConeAngle = babylonLight.angle
                    };
                    break;
                default:
                    RaiseError($"Unsupported light type {light.type} for glTF");
                    throw new System.Exception($"Unsupported light type {light.type} for glTF");
            }

            Dictionary<string, List<GLTFLight>> KHR_lightsExtension;
            if (gltf.extensions.ContainsKey(KHR_lights_punctuals))
            {
                KHR_lightsExtension = (Dictionary<string, List<GLTFLight>>)gltf.extensions[KHR_lights_punctuals];
                KHR_lightsExtension["lights"].Add(light);
            }
            else
            {
                KHR_lightsExtension = new Dictionary<string, List<GLTFLight>>();
                KHR_lightsExtension["lights"] = new List<GLTFLight>();
                KHR_lightsExtension["lights"].Add(light);
                gltf.extensions[KHR_lights_punctuals] = KHR_lightsExtension;
                if (gltf.extensionsUsed == null)
                {
                    gltf.extensionsUsed = new List<string>();
                }
                if (!gltf.extensionsUsed.Contains(KHR_lights_punctuals))
                {
                    gltf.extensionsUsed.Add(KHR_lights_punctuals);
                }
            }

            return KHR_lightsExtension["lights"].Count - 1; // the index of the light
        }

        private GLTFNode ExportLight(ref GLTFNode gltfNode, BabylonLight babylonLight, GLTF gltf, GLTFNode gltfParentNode, BabylonScene babylonScene)
        {
            if (exportParameters.enableKHRLightsPunctual)
            { 
                if (babylonLight.type == 3) // ambient light
                {
                    RaiseMessage($"GLTFExporter.Light | Ambient light {babylonLight.name} is not supported in KHR_lights_punctual.");
                }
                else
                {
                    RaiseMessage("GLTFExporter.Light | Export light named: " + babylonLight.name, 2);

                    // new light in the node extensions
                    GLTFLight light = new GLTFLight
                    {
                        light = AddLightExtension(ref gltf, babylonLight)
                    };

                    if (gltfNode.extensions == null)
                    {
                        gltfNode.extensions = new GLTFExtensions();
                    }
                    gltfNode.extensions[KHR_lights_punctuals] = light;
                }
            }

            return gltfNode;
        }
    }
}
