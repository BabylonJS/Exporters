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
            RaiseError("Export world modifiers is not supported for 3dsmax 2015");
        }
    }
}