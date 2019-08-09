using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using ManagedServices;

namespace Max2Babylon
{
    static class LayerUtilities
    {
        public static IILayer GetSelectedLayer()
        {
            SceneExplorerManager scenExplorer = SceneExplorerManager.Instance;
            scenExplorer.
            return Loader.Core.LayerManager.RootLayer;
        }

        public static List<IILayer> getSelectedLayers()
        {

        }
    }
}
