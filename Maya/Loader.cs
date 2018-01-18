using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maya2Babylon
{
    class Loader
    {
        static MGlobal global;

        public static MGlobal Global
        {
            get
            {
                return global;
            }
        }
    }
}
