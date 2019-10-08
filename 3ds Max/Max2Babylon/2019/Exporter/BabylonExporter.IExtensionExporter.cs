using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BabylonExport.Entities;

namespace Max2Babylon
{
    public interface IBabylonExtensionExporter
    {
        BabylonExtension ExportBabylonExtension();
    }
}
