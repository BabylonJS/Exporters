using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;
using GLTFExport.Entities;

namespace Max2Babylon
{
    public interface IBabylonExtensionExporter
    {
        string GetGLTFExtensionName();
        Type GetGLTFExtendedType();
        object ExportBabylonExtension<T>(T babylonObject);
    }


}
