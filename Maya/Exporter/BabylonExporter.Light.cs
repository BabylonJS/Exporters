using BabylonExport.Entities;
using System;

namespace Maya2Babylon
{
    partial class BabylonExporter
    {
        void ExportDefaultLight(BabylonScene babylonScene)
        {
            var babylonLight = new BabylonLight();
            babylonLight.name = "Default light";
            babylonLight.id = Guid.NewGuid().ToString();
            babylonLight.type = 3;
            babylonLight.groundColor = new float[] { 0, 0, 0 };
            babylonLight.direction = new[] { 0, 1.0f, 0 };

            babylonLight.intensity = 1;

            babylonLight.diffuse = new[] { 1.0f, 1.0f, 1.0f };
            babylonLight.specular = new[] { 1.0f, 1.0f, 1.0f };

            babylonScene.LightsList.Add(babylonLight);
        }
    }
}
