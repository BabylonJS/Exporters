using BabylonExport.Entities;
using GLTFExport.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using Color = System.Drawing.Color;
using System.Linq;

namespace Maya2Babylon
{
    internal partial class BabylonExporter
    {
        public const string KHR_lights = "KHR_lights";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gltf"></param>
        /// <returns>the index of the light</returns>
        private int AddLightExtension(ref GLTF gltf, BabylonLight babylonLight)
        {
            if (gltf.extensionsUsedList.Contains(KHR_lights) == false)
            {
                gltf.extensionsUsedList.Add(KHR_lights);
                gltf.extensionsRequiredList.Add(KHR_lights);
            }

            //RaiseWarning($"gltf.extensionsUsedList: {string.Join(" ", gltf.extensionsUsedList)}", 2);
            //RaiseWarning($"gltf.extensionsRequiredList: {string.Join(" ", gltf.extensionsRequiredList)}", 2);

            // new light
            GLTFLight light = new GLTFLight
            {
                color = babylonLight.groundColor,
                type = (GLTFLight.LightType)babylonLight.type
            };

            //RaiseWarning($"light: color: {string.Join(" ", light.color)} type: {light.type}",2);
            gltf.extensions.KHR_lightsList.Add(light);
            //RaiseWarning($"gltf.extensions.KHR_lightsList: {string.Join(" ", gltf.extensions.KHR_lightsList)}",2);

            return gltf.extensions.KHR_lightsList.Count - 1; // the index of the light
        }

        private GLTFNode ExportLight(ref GLTFNode gltfNode, BabylonLight babylonLight, GLTF gltf, GLTFNode gltfParentNode, BabylonScene babylonScene)
        {
            RaiseMessage("GLTFExporter.Light | Export light named: " + babylonLight.name, 2);

            GLTFLight light = new GLTFLight
            {
                light = AddLightExtension(ref gltf, babylonLight)
            };

            //RaiseWarning($"light: light: {light.light}", 2);
            //if()
            gltfNode.extensions.KHR_lights = light;
            //RaiseWarning

            return gltfNode;
        }
    }
}
