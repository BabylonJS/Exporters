using System;
using System.Collections.Generic;
using Autodesk.Max;
using BabylonExport.Entities;

namespace Max2Babylon
{
    partial class BabylonExporter
    {
        private void ExportWorldModifiers(IIGameNode meshNode, BabylonScene babylonScene, BabylonMesh babylonMesh)
        {
            RaiseError("export world modifiers is not yet supported for 3ds max 2018");
        }
    }
}