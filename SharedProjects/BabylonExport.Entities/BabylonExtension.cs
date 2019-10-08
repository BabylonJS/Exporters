using System;
using System.Collections.Generic;
using System.Text;

namespace BabylonExport.Entities
{
    public class BabylonExtension
    {
        public string ExtensionName;
        public object ExtensionObject;

        public BabylonExtension(string extensionName,object extensionObject)
        {
            ExtensionName = extensionName;
            ExtensionObject = extensionObject;
        }
    }
}
